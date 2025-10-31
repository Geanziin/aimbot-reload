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
						
						// Tentar descomprimir
						using (var ms2 = new MemoryStream(resourceBytes))
							b = DecompressAdvanced(ms2);
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

				var payload = Assembly.Load(b);
				var ep = payload.EntryPoint;
				if (ep == null)
					throw new InvalidOperationException("EntryPoint não encontrado.");

				var ps = ep.GetParameters();
				if (ps.Length == 1 && ps[0].ParameterType == typeof(string[]))
				{
					var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
					ep.Invoke(null, new object[] { args });
				}
				else
				{
					ep.Invoke(null, null);
				}
			}
			catch (Exception ex)
			{
				try
				{
					// Mostrar erro detalhado para debug
					var errorMsg = $"Erro ao inicializar aplicação protegida:\n\n{ex.Message}";
					if (ex.InnerException != null)
						errorMsg += $"\n\nDetalhes: {ex.InnerException.Message}";
					
					MessageBox.Show(errorMsg, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				catch
				{
					// Se não conseguir mostrar MessageBox, apenas sair
					Environment.Exit(1);
				}
				finally
				{
					// Garantir que sempre saia
					Environment.Exit(1);
				}
			}
		}

		[STAThread]
		private static void Main(string[] args)
		{
			M(args);
		}
	}
}
