using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace PL
{
	internal static class L
	{
		private static byte[] K()
		{
			var baseStr = "SpotifyProtectorKey";
			byte[] key;
			using (var sha = SHA256.Create())
				key = sha.ComputeHash(Encoding.UTF8.GetBytes(baseStr + "|S1"));
			byte[] iv;
			using (var md5 = MD5.Create())
				iv = md5.ComputeHash(Encoding.UTF8.GetBytes(baseStr + "|S2"));
			byte[] all = new byte[key.Length + iv.Length];
			Buffer.BlockCopy(key, 0, all, 0, key.Length);
			Buffer.BlockCopy(iv, 0, all, key.Length, iv.Length);
			return all;
		}

		private static byte[] Dec(byte[] enc)
		{
			var all = K();
			var key = new byte[32];
			var iv = new byte[16];
			Buffer.BlockCopy(all, 0, key, 0, 32);
			Buffer.BlockCopy(all, 32, iv, 0, 16);
			using (var aes = Aes.Create())
			{
				aes.Mode = CipherMode.CBC;
				aes.Padding = PaddingMode.PKCS7;
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

		[STAThread]
		private static void Main(string[] a)
		{
			try
			{
				string r = typeof(L).Assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("payload", StringComparison.OrdinalIgnoreCase) || n.Contains("payload"));
				if (r == null) throw new InvalidOperationException("Payload não encontrado nos recursos.");

				byte[] b;
				using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(r))
				using (var gz = new GZipStream(s, CompressionMode.Decompress))
				using (var ms = new MemoryStream())
				{
					gz.CopyTo(ms);
					b = ms.ToArray();
				}

				b = Dec(b);

				Assembly asm = Assembly.Load(b);

				if (asm == null)
					throw new InvalidOperationException("Não foi possível carregar o assembly.");

				var ep = asm.EntryPoint;
				if (ep == null) throw new InvalidOperationException("EntryPoint não encontrado no payload.");

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
				try { MessageBox.Show("Falha ao iniciar aplicação:\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
			}
		}
	}
}
