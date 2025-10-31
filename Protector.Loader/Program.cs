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
			try
			{
				var pc = new PerformanceCounter("Process", "Thread Count", Process.GetCurrentProcess().ProcessName);
				if (pc.NextValue() > 100) return true; // Muitos threads = possivel debugger
			}
			catch { }

			return false;
		}

		private static void AntiDebug()
		{
			if (IsDebugged())
			{
				Environment.Exit(1);
			}

			// Thread de monitoramento contínuo
			var monitor = new Thread(() =>
			{
				while (true)
				{
					Thread.Sleep(500);
					if (IsDebugged())
						Environment.Exit(1);
				}
			})
			{ IsBackground = true };
			monitor.Start();
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

			byte[] xorKey;
			using (var pbkdf2 = new Rfc2898DeriveBytes(saltX, Encoding.UTF8.GetBytes(baseStr + saltSX), 8000))
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
			byte[] decompressed;

			// Tentar Deflate primeiro, se falhar tenta GZip
			try
			{
				using (var deflate = new DeflateStream(input, CompressionMode.Decompress, leaveOpen: true))
				using (var ms = new MemoryStream())
				{
					deflate.CopyTo(ms);
					decompressed = ms.ToArray();
				}
			}
			catch
			{
				input.Position = 0;
				using (var gz = new GZipStream(input, CompressionMode.Decompress, leaveOpen: true))
				using (var ms = new MemoryStream())
				{
					gz.CopyTo(ms);
					decompressed = ms.ToArray();
				}
			}

			return decompressed;
		}

		[STAThread]
		private static void M(string[] a)
		{
			try
			{
				AntiDebug();

				// Buscar recurso (suporta nomes ofuscados)
				var asm = Assembly.GetExecutingAssembly();
				string r = asm.GetManifestResourceNames().FirstOrDefault();
				if (r == null)
					throw new InvalidOperationException("Recurso não encontrado.");

				byte[] b;
				using (var s = asm.GetManifestResourceStream(r))
					b = DecompressAdvanced(s);

				// Descriptografia reversa: AES -> XOR -> AES
				var keys = DeriveAllKeys();
				b = AesDecrypt(b, keys.key2, keys.iv2);
				b = XorDecrypt(b, keys.xorKey);
				b = AesDecrypt(b, keys.key1, keys.iv1);

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
					MessageBox.Show("Erro ao inicializar aplicação protegida:\n" + ex.Message, 
						"Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				catch
				{
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
