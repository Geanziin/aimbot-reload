using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace PL
{
	internal static class L
	{
		[DllImport("kernel32.dll")]
		private static extern bool IsDebuggerPresent();

		[DllImport("kernel32.dll")]
		private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool pbDebuggerPresent);

		[DllImport("kernel32.dll")]
		private static extern void OutputDebugString(string lpOutputString);

		private static bool IsDebugged()
		{
			if (IsDebuggerPresent()) return true;
			bool debuggerPresent = false;
			if (CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref debuggerPresent))
				if (debuggerPresent) return true;

			// Verificação adicional via PerformanceCounter (mais lento mas eficaz)
			// Desabilitado temporariamente para evitar falsos positivos
			/*
			try
			{
				var pc = new PerformanceCounter("Process", "Thread Count", Process.GetCurrentProcess().ProcessName);
				if (pc.NextValue() > 100) return true; // Muitos threads = possivel debugger
			}
			catch { }
			*/

			return false;
		}

		private static void AntiDebug()
		{
			// Verificação inicial - menos agressiva para permitir execução normal
			try
			{
				if (IsDebugged())
				{
					// Dar uma segunda chance (evita falsos positivos)
					Thread.Sleep(100);
					if (IsDebugged())
					{
						Environment.Exit(1);
					}
				}

				// Thread de monitoramento contínuo
				var monitor = new Thread(() =>
				{
					// Aguardar antes de começar monitoramento agressivo
					Thread.Sleep(2000);
					while (true)
					{
						Thread.Sleep(1000);
						if (IsDebugged())
							Environment.Exit(1);
					}
				})
				{ IsBackground = true };
				monitor.Start();
			}
			catch
			{
				// Se anti-debug falhar, continua execução (não quebra o app)
			}
		}

		private static (byte[] key1, byte[] iv1, byte[] key2, byte[] iv2, byte[] xorKey) DeriveAllKeys()
		{
			// Base string determinística (mesma do Protector - mesma string)
			var baseStr = "SpotifyProtection2025SecureKeyBase";
			var machineId = BitConverter.ToString(Encoding.UTF8.GetBytes(Environment.MachineName)).Replace("-", "");
			
			var salt1 = "P1" + machineId.Substring(0, Math.Min(16, machineId.Length));
			var salt2 = "S1" + (machineId.Length > 16 ? machineId.Substring(16, Math.Min(16, machineId.Length - 16)) : "");
			var salt3 = "P2" + machineId.Substring(0, Math.Min(16, machineId.Length));
			var salt4 = "S2" + (machineId.Length > 16 ? machineId.Substring(16, Math.Min(16, machineId.Length - 16)) : "");
			var saltX = "PX" + machineId.Substring(0, Math.Min(16, machineId.Length));
			var saltSX = "SX" + (machineId.Length > 16 ? machineId.Substring(16, Math.Min(16, machineId.Length - 16)) : "");

			byte[] key1;
			using (var pbkdf2 = new Rfc2898DeriveBytes(salt1, Encoding.UTF8.GetBytes(baseStr + salt1), 10000))
				key1 = pbkdf2.GetBytes(32);

			byte[] iv1;
			using (var pbkdf2 = new Rfc2898DeriveBytes(salt2, Encoding.UTF8.GetBytes(baseStr + salt2), 5000))
				iv1 = pbkdf2.GetBytes(16);

			byte[] key2;
			using (var pbkdf2 = new Rfc2898DeriveBytes(salt3, Encoding.UTF8.GetBytes(baseStr + salt3), 10000))
				key2 = pbkdf2.GetBytes(32);

			byte[] iv2;
			using (var pbkdf2 = new Rfc2898DeriveBytes(salt4, Encoding.UTF8.GetBytes(baseStr + salt4), 5000))
				iv2 = pbkdf2.GetBytes(16);

			// XOR key deve ser derivado da mesma forma que no Protector: DeriveKeyIv("PX", "SX")
			// No Protector: salt1="PX" + machineId[...], usa salt1 com baseStr+salt1, 10000 iterações
			byte[] xorKey;
			using (var pbkdf2 = new Rfc2898DeriveBytes(saltX, Encoding.UTF8.GetBytes(baseStr + saltX), 10000))
				xorKey = pbkdf2.GetBytes(32);

			return (key1, iv1, key2, iv2, xorKey);
		}

		private static byte[] XorDecrypt(byte[] data, byte[] key)
		{
			var result = new byte[data.Length];
			for (int i = 0; i < data.Length; i++)
				result[i] = (byte)(data[i] ^ key[i % key.Length]);
			return result;
		}

		private static byte[] AesDecrypt(byte[] enc, byte[] key, byte[] iv)
		{
			using (var aes = Aes.Create())
			{
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;
				aes.KeySize = 256;
				aes.BlockSize = 128;
				aes.Key = key;
				aes.IV = iv;

				using (var ms = new MemoryStream())
				{
					using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
					{
						cs.Write(enc, 0, enc.Length);
						cs.FlushFinalBlock();
					}
					return ms.ToArray();
				}
			}
		}

		private static byte[] DecompressAdvanced(Stream input)
		{
			input.Position = 0;

			// Ler todos os bytes primeiro
			byte[] allBytes;
			using (var ms = new MemoryStream())
			{
				input.CopyTo(ms);
				allBytes = ms.ToArray();
			}

			// Tentar descomprimir Deflate primeiro (se foi comprimido duas vezes)
			// Se CompressAdvanced retornou Deflate, precisa descomprimir Deflate -> GZip
			try
			{
				using (var msIn = new MemoryStream(allBytes))
				using (var deflate = new DeflateStream(msIn, CompressionMode.Decompress))
				using (var msOut = new MemoryStream())
				{
					deflate.CopyTo(msOut);
					var deflateResult = msOut.ToArray();
					
					// Se descompressão Deflate funcionou, tentar descomprimir GZip
					if (deflateResult.Length > 0)
					{
						try
						{
							using (var msIn2 = new MemoryStream(deflateResult))
							using (var gz = new GZipStream(msIn2, CompressionMode.Decompress))
							using (var msOut2 = new MemoryStream())
							{
								gz.CopyTo(msOut2);
								var final = msOut2.ToArray();
								if (final.Length > 0)
									return final;
							}
						}
						catch { }
						
						// Se GZip não funcionar, retornar resultado do Deflate
						return deflateResult;
					}
				}
			}
			catch { }

			// Tentar apenas GZip (se foi comprimido apenas uma vez)
			try
			{
				using (var msIn = new MemoryStream(allBytes))
				using (var gz = new GZipStream(msIn, CompressionMode.Decompress))
				using (var msOut = new MemoryStream())
				{
					gz.CopyTo(msOut);
					var result = msOut.ToArray();
					if (result.Length > 0)
						return result;
				}
			}
			catch { }

			throw new InvalidOperationException("Falha ao descomprimir payload. Formato de compressão inválido ou corrompido.");
		}

		[STAThread]
		private static void M(string[] a)
		{
			try
			{
				// Iniciar anti-debug em background (não bloqueia a inicialização)
				try
				{
					AntiDebug();
				}
				catch { }

				// Buscar recurso (suporta nomes ofuscados)
				var asm = Assembly.GetExecutingAssembly();
				var resourceNames = asm.GetManifestResourceNames();
				if (resourceNames == null || resourceNames.Length == 0)
					throw new InvalidOperationException("Nenhum recurso encontrado no assembly.");
				
				// Tentar encontrar recurso com nome comum ou qualquer recurso
				string r = resourceNames.FirstOrDefault(n => n.Contains("payload") || n.Contains("Payload")) 
					?? resourceNames.FirstOrDefault();
					
				if (r == null)
					throw new InvalidOperationException("Recurso não encontrado.");

				byte[] b;
				using (var s = asm.GetManifestResourceStream(r))
				{
					if (s == null)
						throw new InvalidOperationException("Não foi possível ler o recurso.");
					
					// Ler todos os bytes do recurso primeiro
					using (var ms = new MemoryStream())
					{
						s.CopyTo(ms);
						var resourceBytes = ms.ToArray();
						
						if (resourceBytes == null || resourceBytes.Length == 0)
							throw new InvalidOperationException("Recurso vazio ou corrompido.");
						
						// Tentar descomprimir
						using (var ms2 = new MemoryStream(resourceBytes))
							b = DecompressAdvanced(ms2);
							
						if (b == null || b.Length == 0)
							throw new InvalidOperationException("Descompressão resultou em dados vazios.");
					}
				}

				// Descriptografia reversa: AES -> XOR -> AES (ordem inversa da criptografia)
				var keys = DeriveAllKeys();
				try
				{
					// Descriptografar última camada (AES com key2/iv2) - última camada aplicada
					if (b.Length < 16)
						throw new InvalidOperationException("Payload muito pequeno para descriptografia.");
					
					b = AesDecrypt(b, keys.key2, keys.iv2);
					if (b == null || b.Length == 0)
						throw new InvalidOperationException("Falha na primeira etapa de descriptografia.");
					
					// Remover XOR
					b = XorDecrypt(b, keys.xorKey);
					if (b == null || b.Length == 0)
						throw new InvalidOperationException("Falha na etapa XOR.");
					
					// Descriptografar primeira camada (AES com key1/iv1) - primeira camada aplicada
					b = AesDecrypt(b, keys.key1, keys.iv1);
					if (b == null || b.Length == 0)
						throw new InvalidOperationException("Falha na última etapa de descriptografia.");
				}
				catch (CryptographicException cex)
				{
					throw new InvalidOperationException($"Erro de descriptografia (padding inválido): {cex.Message}. As chaves podem estar incorretas ou o payload corrompido.", cex);
				}
				catch (Exception ex)
				{
					throw new InvalidOperationException($"Erro durante descriptografia: {ex.Message}", ex);
				}

				// Carregar assembly do payload
				Assembly payload;
				try
				{
					payload = Assembly.Load(b);
				}
				catch (Exception loadEx)
				{
					throw new InvalidOperationException($"Falha ao carregar assembly do payload: {loadEx.Message}", loadEx);
				}

				var ep = payload.EntryPoint;
				if (ep == null)
					throw new InvalidOperationException("EntryPoint não encontrado no assembly do payload.");

				// Invocar EntryPoint do payload
				// Para aplicações WinForms, precisamos garantir que Application.Run() funcione
				try
				{
					var ps = ep.GetParameters();
					
					// Garantir que estamos em um thread STA (já garantido pelo [STAThread])
					if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
					{
						throw new InvalidOperationException("Thread não está em modo STA. Aplicações WinForms requerem STA.");
					}
					
					// Invocar EntryPoint
					object result = null;
					if (ps.Length == 1 && ps[0].ParameterType == typeof(string[]))
					{
						var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
						result = ep.Invoke(null, new object[] { args });
					}
					else
					{
						// Main() sem parâmetros - chamar com array vazio de argumentos
						result = ep.Invoke(null, null);
					}
					
					// Se o EntryPoint retornou (não deveria para Application.Run), aguardar
					// Application.Run() normalmente bloqueia até a aplicação fechar
					// Se retornou imediatamente, pode indicar um problema
					if (result != null)
					{
						// Aguardar um pouco para garantir que a aplicação iniciou
						System.Threading.Thread.Sleep(1000);
					}
				}
				catch (TargetInvocationException tie)
				{
					// Exceção interna do EntryPoint - pode ser uma exceção da aplicação original
					var innerEx = tie.InnerException;
					if (innerEx != null)
					{
						// Se a exceção interna é esperada (ex: Application.Run iniciou corretamente),
						// não relançar. Caso contrário, lançar.
						throw new InvalidOperationException($"Erro ao invocar EntryPoint (exceção interna): {innerEx.Message}", innerEx);
					}
					throw new InvalidOperationException($"Erro ao invocar EntryPoint: {tie.Message}", tie);
				}
				catch (Exception invokeEx)
				{
					throw new InvalidOperationException($"Erro ao invocar EntryPoint: {invokeEx.Message}", invokeEx);
				}
			}
			catch (Exception ex)
			{
				// Log detalhado do erro (útil para debug)
				var errorDetails = new System.Text.StringBuilder();
				errorDetails.AppendLine($"Erro ao inicializar aplicação protegida:");
				errorDetails.AppendLine($"Tipo: {ex.GetType().Name}");
				errorDetails.AppendLine($"Mensagem: {ex.Message}");
				
				if (ex.InnerException != null)
				{
					errorDetails.AppendLine($"\nErro interno:");
					errorDetails.AppendLine($"Tipo: {ex.InnerException.GetType().Name}");
					errorDetails.AppendLine($"Mensagem: {ex.InnerException.Message}");
				}
				
				errorDetails.AppendLine($"\nStack Trace:");
				errorDetails.AppendLine(ex.StackTrace);
				
				// Tentar mostrar erro em MessageBox (visível mesmo em modo Hidden)
				try
				{
					// Forçar criação de janela visível
					var form = new System.Windows.Forms.Form
					{
						WindowState = System.Windows.Forms.FormWindowState.Normal,
						ShowInTaskbar = true,
						TopMost = true
					};
					form.Show();
					
					MessageBox.Show(errorDetails.ToString(), "Erro - Aplicação Protegida", 
						MessageBoxButtons.OK, MessageBoxIcon.Error);
					
					form.Close();
				}
				catch
				{
					// Se MessageBox falhar, tentar escrever no console de erro
					try
					{
						Console.Error.WriteLine(errorDetails.ToString());
					}
					catch { }
				}
				
				// Aguardar mais tempo antes de sair (permite que testes detectem o processo)
				// E também tenta novamente em caso de erro não crítico
				for (int i = 0; i < 5; i++)
				{
					System.Threading.Thread.Sleep(500);
				}
				
				// Sair com código de erro
				Environment.Exit(1);
			}
		}

		[STAThread]
		private static void Main(string[] args)
		{
			// Garantir que o processo não saia imediatamente
			try
			{
				M(args);
				
				// Se chegou aqui, o EntryPoint foi invocado com sucesso
				// A aplicação deve continuar rodando normalmente
				// Aguardar um pouco para garantir que iniciou
				System.Threading.Thread.Sleep(500);
			}
			catch (Exception mainEx)
			{
				// Se houver erro no Main, também tratar
				try
				{
					MessageBox.Show($"Erro fatal no Main:\n{mainEx.Message}", "Erro Fatal", 
						MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				catch { }
				
				// Aguardar antes de sair
				System.Threading.Thread.Sleep(2000);
				Environment.Exit(1);
			}
		}
	}
}
