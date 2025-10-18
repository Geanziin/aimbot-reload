using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace MysteriousMem
{
	// Token: 0x0200001F RID: 31
	public class Mysterious
	{
		// Token: 0x0600019D RID: 413
		[DllImport("kernel32.dll")]
		private static extern void GetSystemInfo(out Mysterious.SYSTEM_INFO lpSystemInfo);

		// Token: 0x0600019E RID: 414
		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		// Token: 0x0600019F RID: 415
		[DllImport("kernel32")]
		public static extern bool IsWow64Process(IntPtr hProcess, out bool lpSystemInfo);

		// Token: 0x060001A0 RID: 416
		[DllImport("kernel32.dll")]
		private static extern bool VirtualProtectEx(IntPtr hProcess, UIntPtr lpAddress, IntPtr dwSize, Mysterious.MemoryProtection flNewProtect, out Mysterious.MemoryProtection lpflOldProtect);

		// Token: 0x060001A1 RID: 417
		[DllImport("kernel32.dll")]
		private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesWritten);

		// Token: 0x060001A2 RID: 418
		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);

		// Token: 0x060001A3 RID: 419
		[DllImport("kernel32.dll")]
		public static extern int CloseHandle(IntPtr hObject);

		// Token: 0x060001A4 RID: 420
		[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
		public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out Mysterious.MEMORY_BASIC_INFORMATION64 lpBuffer, UIntPtr dwLength);

		// Token: 0x060001A5 RID: 421
		[DllImport("kernel32.dll", EntryPoint = "VirtualQueryEx")]
		public static extern UIntPtr Native_VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out Mysterious.MEMORY_BASIC_INFORMATION32 lpBuffer, UIntPtr dwLength);

		// Token: 0x060001A6 RID: 422
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

		// Token: 0x060001A7 RID: 423
		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] IntPtr lpBuffer, UIntPtr nSize, out ulong lpNumberOfBytesRead);

		// Token: 0x060001A8 RID: 424 RVA: 0x0000834C File Offset: 0x0000654C
		public string LoadCode(string name, string file)
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			if (file != "")
			{
				Mysterious.GetPrivateProfileString("codes", name, "", stringBuilder, (uint)stringBuilder.Capacity, file);
			}
			else
			{
				stringBuilder.Append(name);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060001A9 RID: 425 RVA: 0x0000839C File Offset: 0x0000659C
	public byte[]? ReadMemory(string code, long length, string file = "")
	{
		byte[] array = new byte[length];
		UIntPtr code2 = this.GetCode(code, file, 8);
		if (!Mysterious.ReadProcessMemory(this.pHandle, code2, array, (UIntPtr)((ulong)length), IntPtr.Zero))
		{
			return Array.Empty<byte>();
		}
		return array;
	}

		// Token: 0x060001AA RID: 426 RVA: 0x000083D8 File Offset: 0x000065D8
		public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool readable, bool writable, bool executable, string file = "")
		{
			return Task.Run<IEnumerable<long>>(delegate()
			{
				List<MemoryRegionResult> list = new List<MemoryRegionResult>();
				string[] array = this.LoadCode(search, file).Split(new char[]
				{
					' '
				});
				byte[] aobPattern = new byte[array.Length];
				byte[] mask = new byte[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i];
					if (text == "??" || (text.Length == 1 && text == "?"))
					{
						mask[i] = 0;
						array[i] = "0x00";
					}
					else if (char.IsLetterOrDigit(text[0]) && text[1] == '?')
					{
						mask[i] = 240;
						array[i] = text[0].ToString() + "0";
					}
					else if (char.IsLetterOrDigit(text[1]) && text[0] == '?')
					{
						mask[i] = 15;
						array[i] = "0" + text[1].ToString();
					}
					else
					{
						mask[i] = byte.MaxValue;
					}
				}
			for (int j = 0; j < array.Length; j++)
			{
				aobPattern[j] = (byte)(Convert.ToByte(array[j], 16) & mask[j]);
			}
				Mysterious.SYSTEM_INFO system_INFO;
				Mysterious.GetSystemInfo(out system_INFO);
				UIntPtr minimumApplicationAddress = system_INFO.minimumApplicationAddress;
				UIntPtr maximumApplicationAddress = system_INFO.maximumApplicationAddress;
				if (start < (long)minimumApplicationAddress.ToUInt64())
				{
					start = (long)minimumApplicationAddress.ToUInt64();
				}
				if (end > (long)maximumApplicationAddress.ToUInt64())
				{
					end = (long)maximumApplicationAddress.ToUInt64();
				}
				UIntPtr uintPtr = new UIntPtr((ulong)start);
				Mysterious.MEMORY_BASIC_INFORMATION memory_BASIC_INFORMATION;
				while (this.VirtualQueryEx(this.pHandle, uintPtr, out memory_BASIC_INFORMATION).ToUInt64() != 0UL && uintPtr.ToUInt64() < (ulong)end && uintPtr.ToUInt64() + (ulong)memory_BASIC_INFORMATION.RegionSize > uintPtr.ToUInt64())
				{
					bool flag = memory_BASIC_INFORMATION.State == 4096U;
					flag &= (memory_BASIC_INFORMATION.BaseAddress.ToUInt64() < maximumApplicationAddress.ToUInt64());
					flag &= ((memory_BASIC_INFORMATION.Protect & 256U) == 0U);
					flag &= ((memory_BASIC_INFORMATION.Protect & 1U) == 0U);
					flag &= (memory_BASIC_INFORMATION.Type == this.MEM_PRIVATE || memory_BASIC_INFORMATION.Type == this.MEM_IMAGE);
					if (flag)
					{
						bool flag2 = (memory_BASIC_INFORMATION.Protect & 2U) > 0U;
						bool flag3 = (memory_BASIC_INFORMATION.Protect & 4U) > 0U || (memory_BASIC_INFORMATION.Protect & 8U) > 0U || (memory_BASIC_INFORMATION.Protect & 64U) > 0U || (memory_BASIC_INFORMATION.Protect & 128U) > 0U;
						bool flag4 = (memory_BASIC_INFORMATION.Protect & 16U) > 0U || (memory_BASIC_INFORMATION.Protect & 32U) > 0U || (memory_BASIC_INFORMATION.Protect & 64U) > 0U || (memory_BASIC_INFORMATION.Protect & 128U) > 0U;
						flag2 &= readable;
						flag3 &= writable;
						flag4 &= executable;
						flag &= (flag2 || flag3 || flag4);
					}
					if (!flag)
					{
						uintPtr = new UIntPtr(memory_BASIC_INFORMATION.BaseAddress.ToUInt64() + (ulong)memory_BASIC_INFORMATION.RegionSize);
					}
					else
					{
						MemoryRegionResult item2 = new MemoryRegionResult
						{
							CurrentBaseAddress = uintPtr,
							RegionSize = memory_BASIC_INFORMATION.RegionSize,
							RegionBase = memory_BASIC_INFORMATION.BaseAddress
						};
						uintPtr = new UIntPtr(memory_BASIC_INFORMATION.BaseAddress.ToUInt64() + (ulong)memory_BASIC_INFORMATION.RegionSize);
						if (list.Count > 0)
						{
							MemoryRegionResult memoryRegionResult = list[list.Count - 1];
							if ((ulong)memoryRegionResult.RegionBase + (ulong)memoryRegionResult.RegionSize == (ulong)memory_BASIC_INFORMATION.BaseAddress)
							{
								list[list.Count - 1] = new MemoryRegionResult
								{
									CurrentBaseAddress = memoryRegionResult.CurrentBaseAddress,
									RegionBase = memoryRegionResult.RegionBase,
									RegionSize = memoryRegionResult.RegionSize + memory_BASIC_INFORMATION.RegionSize
								};
								continue;
							}
						}
						list.Add(item2);
					}
				}
				ConcurrentBag<long> bagResult = new ConcurrentBag<long>();
				Parallel.ForEach<MemoryRegionResult>(list, delegate(MemoryRegionResult item, ParallelLoopState parallelLoopState, long index)
				{
					foreach (long item3 in this.CompareScan(item, aobPattern, mask))
					{
						bagResult.Add(item3);
					}
				});
				return (from c in bagResult.ToList<long>()
				orderby c
				select c).AsEnumerable<long>();
			});
		}

		// Token: 0x060001AB RID: 427 RVA: 0x00008436 File Offset: 0x00006636
		public string MSize()
		{
			if (!this.Is64Bit)
			{
				return "X8";
			}
			return "X16";
		}

		// Token: 0x060001AC RID: 428 RVA: 0x0000844B File Offset: 0x0000664B
	public void CloseProcess()
	{
		if (this.pHandle != IntPtr.Zero)
		{
			Mysterious.CloseHandle(this.pHandle);
			this.theProc = null!;
			this.mainModule = null!;
			this.pHandle = IntPtr.Zero;
		}
	}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x060001AD RID: 429 RVA: 0x0000847D File Offset: 0x0000667D
		// (set) Token: 0x060001AE RID: 430 RVA: 0x00008485 File Offset: 0x00006685
		public bool Is64Bit
		{
			get
			{
				return this._is64Bit;
			}
			private set
			{
				this._is64Bit = value;
			}
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00008490 File Offset: 0x00006690
		private unsafe long[] CompareScan(MemoryRegionResult item, byte[] aobPattern, byte[] mask)
		{
			if (mask.Length != aobPattern.Length)
			{
				throw new ArgumentException("aobPattern.Length != mask.Length");
			}
			IntPtr intPtr = Marshal.AllocHGlobal((int)item.RegionSize);
			ulong num;
			Mysterious.ReadProcessMemory(this.pHandle, item.CurrentBaseAddress, intPtr, (UIntPtr)((ulong)item.RegionSize), out num);
			int num2 = 0 - aobPattern.Length;
			List<long> list = new List<long>();
			do
			{
				num2 = this.FindPattern((byte*)intPtr.ToPointer(), (int)num, aobPattern, mask, num2 + aobPattern.Length);
				if (num2 >= 0)
				{
					list.Add((long)((ulong)item.CurrentBaseAddress + (ulong)((long)num2)));
				}
			}
			while (num2 != -1);
			Marshal.FreeHGlobal(intPtr);
			return list.ToArray();
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x0000852C File Offset: 0x0000672C
		private unsafe int FindPattern(byte* body, int bodyLength, byte[] pattern, byte[] masks, int start = 0)
		{
			int result = -1;
			if (bodyLength <= 0 || pattern.Length == 0 || start > bodyLength - pattern.Length || pattern.Length > bodyLength)
			{
				return result;
			}
			for (int i = start; i <= bodyLength - pattern.Length; i++)
			{
				if ((body[i] & masks[0]) == (pattern[0] & masks[0]))
				{
					bool flag = true;
					for (int j = 1; j <= pattern.Length - 1; j++)
					{
						if ((body[i + j] & masks[j]) != (pattern[j] & masks[j]))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						result = i;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x000085AC File Offset: 0x000067AC
		public UIntPtr VirtualQueryEx(IntPtr hProcess, UIntPtr lpAddress, out Mysterious.MEMORY_BASIC_INFORMATION lpBuffer)
		{
			lpBuffer = default(Mysterious.MEMORY_BASIC_INFORMATION);
			if (this.Is64Bit || IntPtr.Size == 8)
			{
				Mysterious.MEMORY_BASIC_INFORMATION64 memory_BASIC_INFORMATION = default(Mysterious.MEMORY_BASIC_INFORMATION64);
				UIntPtr result = Mysterious.Native_VirtualQueryEx(hProcess, lpAddress, out memory_BASIC_INFORMATION, new UIntPtr((uint)Marshal.SizeOf<Mysterious.MEMORY_BASIC_INFORMATION64>(memory_BASIC_INFORMATION)));
				lpBuffer.BaseAddress = memory_BASIC_INFORMATION.BaseAddress;
				lpBuffer.AllocationBase = memory_BASIC_INFORMATION.AllocationBase;
				lpBuffer.AllocationProtect = memory_BASIC_INFORMATION.AllocationProtect;
				lpBuffer.RegionSize = (long)memory_BASIC_INFORMATION.RegionSize;
				lpBuffer.State = memory_BASIC_INFORMATION.State;
				lpBuffer.Protect = memory_BASIC_INFORMATION.Protect;
				lpBuffer.Type = memory_BASIC_INFORMATION.Type;
				return result;
			}
			Mysterious.MEMORY_BASIC_INFORMATION32 memory_BASIC_INFORMATION2 = default(Mysterious.MEMORY_BASIC_INFORMATION32);
			UIntPtr result2 = Mysterious.Native_VirtualQueryEx(hProcess, lpAddress, out memory_BASIC_INFORMATION2, new UIntPtr((uint)Marshal.SizeOf<Mysterious.MEMORY_BASIC_INFORMATION32>(memory_BASIC_INFORMATION2)));
			lpBuffer.BaseAddress = memory_BASIC_INFORMATION2.BaseAddress;
			lpBuffer.AllocationBase = memory_BASIC_INFORMATION2.AllocationBase;
			lpBuffer.AllocationProtect = memory_BASIC_INFORMATION2.AllocationProtect;
			lpBuffer.RegionSize = (long)((ulong)memory_BASIC_INFORMATION2.RegionSize);
			lpBuffer.State = memory_BASIC_INFORMATION2.State;
			lpBuffer.Protect = memory_BASIC_INFORMATION2.Protect;
			lpBuffer.Type = memory_BASIC_INFORMATION2.Type;
			return result2;
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x000086B4 File Offset: 0x000068B4
		public static void notify(string message)
		{
			Process.Start(new ProcessStartInfo("cmd.exe", "/c start cmd /C \"color b && title Error && echo " + message + " && timeout /t 5\"")
			{
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false
			});
			Environment.Exit(0);
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x00008704 File Offset: 0x00006904
		public UIntPtr Get64BitCode(string name, string path = "", int size = 16)
		{
			if (path != "")
			{
				string code = this.LoadCode(name, path);
				return this.Process64BitCode(code, size);
			}
			return this.Process64BitCode(name, size);
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x00008738 File Offset: 0x00006938
		private UIntPtr Process64BitCode(string code, int size)
		{
			if (string.IsNullOrEmpty(code))
			{
				return UIntPtr.Zero;
			}
			code = code.Replace(" ", "");
			if (!code.Contains("+") && !code.Contains(","))
			{
				return new UIntPtr(Convert.ToUInt64(code, 16));
			}
			string text = code.Contains("+") ? code.Substring(code.IndexOf('+') + 1) : code;
			byte[] array = new byte[size];
			if (text.Contains(','))
			{
				string[] array2 = text.Split(new char[]
				{
					','
				});
				List<long> list = new List<long>();
				string[] array3 = array2;
				for (int i = 0; i < array3.Length; i++)
				{
					string text2 = array3[i].Replace("0x", "");
					bool flag = text2.StartsWith("-");
					if (flag)
					{
						text2 = text2.Substring(1);
					}
					long num = long.Parse(text2, NumberStyles.HexNumber);
					if (flag)
					{
						num = -num;
					}
					list.Add(num);
				}
				long num2 = 0L;
				if (code.ToLower().Contains("base") || code.ToLower().Contains("main"))
				{
					num2 = (long)this.mainModule!.BaseAddress;
				}
				else if (code.Contains("+"))
				{
					string[] array4 = code.Split(new char[]
					{
						'+'
					});
					if (array4[0].ToLower().Contains(".dll") || array4[0].ToLower().Contains(".exe"))
					{
						num2 = (long)this.modules[array4[0]];
					}
					else
					{
						num2 = long.Parse(array4[0].Replace("0x", ""), NumberStyles.HexNumber);
					}
				}
				UIntPtr uintPtr = new UIntPtr((ulong)(num2 + list[0]));
				for (int j = 1; j < list.Count; j++)
				{
					Mysterious.ReadProcessMemory(this.pHandle, uintPtr, array, (UIntPtr)((ulong)((long)size)), IntPtr.Zero);
					long value = BitConverter.ToInt64(array, 0) + list[j];
					uintPtr = new UIntPtr((ulong)value);
				}
				return uintPtr;
			}
			long num3 = Convert.ToInt64(text, 16);
			if (code.ToLower().Contains("base") || code.ToLower().Contains("main"))
			{
				return new UIntPtr((ulong)((long)this.mainModule!.BaseAddress + num3));
			}
			if (code.Contains("+"))
			{
				string[] array5 = code.Split(new char[]
				{
					'+'
				});
				long num4;
				if (array5[0].ToLower().Contains(".dll") || array5[0].ToLower().Contains(".exe"))
				{
					num4 = (long)this.modules[array5[0]];
				}
				else
				{
					num4 = long.Parse(array5[0].Replace("0x", ""), NumberStyles.HexNumber);
				}
				return new UIntPtr((ulong)(num4 + num3));
			}
			return UIntPtr.Zero;
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x00008A29 File Offset: 0x00006C29
		public UIntPtr GetCode(string code, string path = "", int size = 8)
		{
			if (this.Is64Bit)
			{
				return this.Get64BitCode(code, path, Math.Max(size, 8));
			}
			return UIntPtr.Zero;
		}

	// Token: 0x060001B6 RID: 438 RVA: 0x00008A48 File Offset: 0x00006C48
	public bool WriteMemory(string code, string type, string write, string file = "", Encoding? stringEncoding = null)
		{
			string text = type.ToLower();
			if (text != null)
			{
				byte[] array;
				int num;
				switch (text.Length)
				{
				case 3:
					if (!(text == "int"))
					{
						return false;
					}
					array = BitConverter.GetBytes(Convert.ToInt32(write));
					num = 4;
					break;
				case 4:
				{
					char c = text[0];
					if (c != 'b')
					{
						if (c != 'l')
						{
							return false;
						}
						if (!(text == "long"))
						{
							return false;
						}
						array = BitConverter.GetBytes(Convert.ToInt64(write));
						num = 8;
					}
					else
					{
						if (!(text == "byte"))
						{
							return false;
						}
						array = new byte[]
						{
							Convert.ToByte(write, 16)
						};
						num = 1;
					}
					break;
				}
				case 5:
				{
					char c = text[0];
					if (c != 'b')
					{
						if (c != 'f')
						{
							return false;
						}
						if (!(text == "float"))
						{
							return false;
						}
						array = BitConverter.GetBytes(Convert.ToSingle(write));
						num = 4;
					}
					else
					{
						if (!(text == "bytes"))
						{
							return false;
						}
						if (write.Contains(",") || write.Contains(" "))
						{
							string[] array2 = write.Split(new char[]
							{
								',',
								' '
							}, StringSplitOptions.RemoveEmptyEntries);
							array = new byte[array2.Length];
							for (int i = 0; i < array2.Length; i++)
							{
								array[i] = Convert.ToByte(array2[i], 16);
							}
							num = array2.Length;
						}
						else
						{
							array = new byte[]
							{
								Convert.ToByte(write, 16)
							};
							num = 1;
						}
					}
					break;
				}
				case 6:
				{
					char c = text[0];
					if (c != '2')
					{
						if (c != 'd')
						{
							if (c != 's')
							{
								return false;
							}
							if (!(text == "string"))
							{
								return false;
							}
							array = ((stringEncoding == null) ? Encoding.UTF8.GetBytes(write) : stringEncoding.GetBytes(write));
							num = array.Length;
						}
						else
						{
							if (!(text == "double"))
							{
								return false;
							}
							array = BitConverter.GetBytes(Convert.ToDouble(write));
							num = 8;
						}
					}
					else
					{
						if (!(text == "2bytes"))
						{
							return false;
						}
						array = new byte[]
						{
							(byte)(Convert.ToInt32(write) % 256),
							(byte)(Convert.ToInt32(write) / 256)
						};
						num = 2;
					}
					break;
				}
				default:
					return false;
				}
				UIntPtr code2 = this.GetCode(code, file, num);
				return Mysterious.WriteProcessMemory(this.pHandle, code2, array, (UIntPtr)((ulong)((long)num)), IntPtr.Zero);
			}
			return false;
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00008CD4 File Offset: 0x00006ED4
		public bool IsAdmin()
		{
			bool result;
			using (WindowsIdentity current = WindowsIdentity.GetCurrent())
			{
				result = new WindowsPrincipal(current).IsInRole(WindowsBuiltInRole.Administrator);
			}
			return result;
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00008D18 File Offset: 0x00006F18
		public bool OpenProcess(int pid)
		{
			if (!this.IsAdmin())
			{
				Mysterious.notify("WARNING: You are NOT running this program as admin!");
			}
			if (pid <= 0)
			{
				return false;
			}
			if (this.theProc != null && this.theProc.Id == pid)
			{
				return true;
			}
			bool result;
			try
			{
				this.theProc = Process.GetProcessById(pid);
				if (this.theProc == null || !this.theProc.Responding)
				{
					result = false;
				}
					else
					{
						this.pHandle = Mysterious.OpenProcess(2035711U, true, pid);
						Process.EnterDebugMode();
						if (this.pHandle == IntPtr.Zero)
						{
							Process.LeaveDebugMode();
							this.theProc = null!;
							result = false;
						}
						else
						{
							this.mainModule = this.theProc.MainModule!;
							this.GetModules();
							bool flag;
							this.Is64Bit = (Environment.Is64BitOperatingSystem && Mysterious.IsWow64Process(this.pHandle, out flag) && !flag);
							result = true;
						}
					}
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x00008E08 File Offset: 0x00007008
		public void GetModules()
		{
			this.modules.Clear();
			if (this.theProc != null)
			{
				foreach (object obj in this.theProc.Modules)
				{
					ProcessModule processModule = (ProcessModule)obj;
					if (!string.IsNullOrEmpty(processModule.ModuleName) && !this.modules.ContainsKey(processModule.ModuleName))
					{
						this.modules.Add(processModule.ModuleName, processModule.BaseAddress);
					}
				}
			}
		}

		// Token: 0x060001BA RID: 442 RVA: 0x00008EAC File Offset: 0x000070AC
		public Task<IEnumerable<long>> AoBScan(string search, bool writable = false, bool executable = false, string file = "")
		{
			return this.AoBScan(0L, long.MaxValue, search, true, writable, executable, file);
		}

		// Token: 0x060001BB RID: 443 RVA: 0x00008EC5 File Offset: 0x000070C5
		public Task<IEnumerable<long>> AoBScan(long start, long end, string search, bool writable, bool executable, string file = "")
		{
			return this.AoBScan(start, end, search, true, writable, executable, file);
		}

		// Token: 0x060001BC RID: 444 RVA: 0x00008ED8 File Offset: 0x000070D8
		public bool ChangeProtection(string code, Mysterious.MemoryProtection newProtection, out Mysterious.MemoryProtection oldProtection, string file = "")
		{
			UIntPtr code2 = this.GetCode(code, file, 8);
			if (code2 == UIntPtr.Zero || this.pHandle == IntPtr.Zero)
			{
				oldProtection = (Mysterious.MemoryProtection)0U;
				return false;
			}
			return Mysterious.VirtualProtectEx(this.pHandle, code2, (IntPtr)(this.Is64Bit ? 8 : 4), newProtection, out oldProtection);
		}

		// Token: 0x040000DE RID: 222
		private bool _is64Bit;

		// Token: 0x040000DF RID: 223
		private Dictionary<string, IntPtr> modules = new Dictionary<string, IntPtr>();

	// Token: 0x040000E0 RID: 224
	private ProcessModule? mainModule;

	// Token: 0x040000E1 RID: 225
	public Process? theProc;

		// Token: 0x040000E2 RID: 226
		private uint MEM_PRIVATE = 131072U;

		// Token: 0x040000E3 RID: 227
		private uint MEM_IMAGE = 16777216U;

		// Token: 0x040000E4 RID: 228
		public IntPtr pHandle = IntPtr.Zero;

		// Token: 0x02000020 RID: 32
		[Flags]
		public enum ThreadAccess
		{
			// Token: 0x040000E6 RID: 230
			TERMINATE = 1,
			// Token: 0x040000E7 RID: 231
			SUSPEND_RESUME = 2,
			// Token: 0x040000E8 RID: 232
			GET_CONTEXT = 8,
			// Token: 0x040000E9 RID: 233
			SET_CONTEXT = 16,
			// Token: 0x040000EA RID: 234
			SET_INFORMATION = 32,
			// Token: 0x040000EB RID: 235
			QUERY_INFORMATION = 64,
			// Token: 0x040000EC RID: 236
			SET_THREAD_TOKEN = 128,
			// Token: 0x040000ED RID: 237
			IMPERSONATE = 256,
			// Token: 0x040000EE RID: 238
			DIRECT_IMPERSONATION = 512
		}

		// Token: 0x02000021 RID: 33
		public struct MEMORY_BASIC_INFORMATION32
		{
			// Token: 0x040000EF RID: 239
			public UIntPtr BaseAddress;

			// Token: 0x040000F0 RID: 240
			public UIntPtr AllocationBase;

			// Token: 0x040000F1 RID: 241
			public uint AllocationProtect;

			// Token: 0x040000F2 RID: 242
			public uint RegionSize;

			// Token: 0x040000F3 RID: 243
			public uint State;

			// Token: 0x040000F4 RID: 244
			public uint Protect;

			// Token: 0x040000F5 RID: 245
			public uint Type;
		}

		// Token: 0x02000022 RID: 34
		public struct MEMORY_BASIC_INFORMATION64
		{
			// Token: 0x040000F6 RID: 246
			public UIntPtr BaseAddress;

			// Token: 0x040000F7 RID: 247
			public UIntPtr AllocationBase;

			// Token: 0x040000F8 RID: 248
			public uint AllocationProtect;

			// Token: 0x040000F9 RID: 249
			public uint __alignment1;

			// Token: 0x040000FA RID: 250
			public ulong RegionSize;

			// Token: 0x040000FB RID: 251
			public uint State;

			// Token: 0x040000FC RID: 252
			public uint Protect;

			// Token: 0x040000FD RID: 253
			public uint Type;

			// Token: 0x040000FE RID: 254
			public uint __alignment2;
		}

		// Token: 0x02000023 RID: 35
		[Flags]
		public enum MemoryProtection : uint
		{
			// Token: 0x04000100 RID: 256
			Execute = 16U,
			// Token: 0x04000101 RID: 257
			ExecuteRead = 32U,
			// Token: 0x04000102 RID: 258
			ExecuteReadWrite = 64U,
			// Token: 0x04000103 RID: 259
			ExecuteWriteCopy = 128U,
			// Token: 0x04000104 RID: 260
			NoAccess = 1U,
			// Token: 0x04000105 RID: 261
			ReadOnly = 2U,
			// Token: 0x04000106 RID: 262
			ReadWrite = 4U,
			// Token: 0x04000107 RID: 263
			WriteCopy = 8U,
			// Token: 0x04000108 RID: 264
			GuardModifierflag = 256U,
			// Token: 0x04000109 RID: 265
			NoCacheModifierflag = 512U,
			// Token: 0x0400010A RID: 266
			WriteCombineModifierflag = 1024U
		}

		// Token: 0x02000024 RID: 36
		public struct SYSTEM_INFO
		{
			// Token: 0x0400010B RID: 267
			public ushort processorArchitecture;

			// Token: 0x0400010C RID: 268
			private ushort reserved;

			// Token: 0x0400010D RID: 269
			public uint pageSize;

			// Token: 0x0400010E RID: 270
			public UIntPtr minimumApplicationAddress;

			// Token: 0x0400010F RID: 271
			public UIntPtr maximumApplicationAddress;

			// Token: 0x04000110 RID: 272
			public IntPtr activeProcessorMask;

			// Token: 0x04000111 RID: 273
			public uint numberOfProcessors;

			// Token: 0x04000112 RID: 274
			public uint processorType;

			// Token: 0x04000113 RID: 275
			public uint allocationGranularity;

			// Token: 0x04000114 RID: 276
			public ushort processorLevel;

			// Token: 0x04000115 RID: 277
			public ushort processorRevision;
		}

		// Token: 0x02000025 RID: 37
		public struct MEMORY_BASIC_INFORMATION
		{
			// Token: 0x04000116 RID: 278
			public UIntPtr BaseAddress;

			// Token: 0x04000117 RID: 279
			public UIntPtr AllocationBase;

			// Token: 0x04000118 RID: 280
			public uint AllocationProtect;

			// Token: 0x04000119 RID: 281
			public long RegionSize;

			// Token: 0x0400011A RID: 282
			public uint State;

			// Token: 0x0400011B RID: 283
			public uint Protect;

			// Token: 0x0400011C RID: 284
			public uint Type;
		}
	}
}
