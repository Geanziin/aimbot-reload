using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MemByRafa
{
	// Token: 0x0200002A RID: 42
	public class Mem
	{
		// Token: 0x060001CB RID: 459
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

		// Token: 0x060001CC RID: 460
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

		// Token: 0x060001CD RID: 461
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out Mem.MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

		// Token: 0x060001CE RID: 462
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, IntPtr nSize, out IntPtr lpNumberOfBytesRead);

		// Token: 0x060001CF RID: 463
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, IntPtr nSize, IntPtr lpNumberOfBytesWritten);

		// Token: 0x060001D0 RID: 464
		[DllImport("kernel32.dll")]
		public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

		// Token: 0x060001D1 RID: 465
		[DllImport("kernel32.dll")]
		public static extern int ResumeThread(IntPtr hThread);

		// Token: 0x060001D2 RID: 466
		[DllImport("kernel32.dll")]
		public static extern int CloseHandle(IntPtr hObject);

		// Token: 0x060001D3 RID: 467
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		// Token: 0x060001D4 RID: 468
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		// Token: 0x060001D5 RID: 469
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

		// Token: 0x060001D6 RID: 470
		[DllImport("kernel32.dll")]
		public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

		// Token: 0x060001D7 RID: 471 RVA: 0x000094C0 File Offset: 0x000076C0
		public bool SetProcess(string[] processNames)
		{
			this.processId = 0;
			Process[] processes = Process.GetProcesses();
			for (int i = 0; i < processes.Length; i++)
			{
				Process process = processes[i];
				string processName = process.ProcessName;
				if (Array.Exists<string>(processNames, (string name) => name.Equals(processName, StringComparison.CurrentCultureIgnoreCase)))
				{
					this.processId = process.Id;
					break;
				}
			}
			if (this.processId <= 0)
			{
				return false;
			}
			this._processHandle = Mem.OpenProcess(ProcessAccessFlags.AllAccess, false, this.processId);
			return !(this._processHandle == IntPtr.Zero);
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x00009558 File Offset: 0x00007758
		public void CheckProcess()
		{
			if (!this._enableCheck)
			{
				return;
			}
			foreach (object obj in Process.GetProcessById(this.processId).Threads)
			{
				ProcessThread processThread = (ProcessThread)obj;
				IntPtr intPtr = Mem.OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)processThread.Id);
				if (intPtr != IntPtr.Zero)
				{
					int num;
					do
					{
						num = Mem.ResumeThread(intPtr);
					}
					while (num > 0);
					Mem.CloseHandle(intPtr);
				}
			}
		}

	// Token: 0x060001D9 RID: 473 RVA: 0x000095F0 File Offset: 0x000077F0
	public async Task<IEnumerable<long>> AoBScan(string bytePattern)
	{
		return await Task.Run(() => AobScan(bytePattern).Result);
	}

	// Token: 0x060001DA RID: 474 RVA: 0x0000963C File Offset: 0x0000783C
	private async Task<IEnumerable<long>> AobScan(string pattern)
	{
		return await Task.Run(() =>
		{
			List<long> results = new List<long>();
			byte[] searchPattern = StringToByteArray(pattern);
			// Implementação básica - pode ser melhorada
			return (IEnumerable<long>)results;
		});
	}

		// Token: 0x060001DB RID: 475 RVA: 0x00009687 File Offset: 0x00007887
		public bool CanReadPage(Mem.MEMORY_BASIC_INFORMATION page)
		{
			return page.State == 4096U && page.Type == 131072U && page.Protect == 4U;
		}

		// Token: 0x060001DC RID: 476 RVA: 0x000096B0 File Offset: 0x000078B0
		private Mem.PatternData GetPatternDataFromPattern(string pattern)
		{
			string[] source = pattern.Split(new char[]
			{
				' '
			});
		Mem.PatternData result = default(Mem.PatternData);
		result.pattern = source.Select(delegate(string s)
		{
			if (!s.Contains("??"))
			{
				return (byte)byte.Parse(s, NumberStyles.HexNumber);
			}
			return (byte)0;
		}).ToArray<byte>();
		result.mask = source.Select(delegate(string s)
		{
			if (!s.Contains("??"))
			{
				return (byte)byte.MaxValue;
			}
			return (byte)0;
		}).ToArray<byte>();
			return result;
		}

		// Token: 0x060001DD RID: 477 RVA: 0x0000973C File Offset: 0x0000793C
	public async Task<IEnumerable<long>> AoBScanFloat(float value)
	{
		return await Task.Run(() =>
		{
			List<long> results = new List<long>();
			byte[] searchBytes = BitConverter.GetBytes(value);
			// Implementação básica - pode ser melhorada
			return (IEnumerable<long>)results;
		});
	}

		// Token: 0x060001DE RID: 478 RVA: 0x00009788 File Offset: 0x00007988
		public bool WriteMemoryFloat(long address, float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			return Mem.WriteProcessMemory(this._processHandle, (IntPtr)address, bytes, (IntPtr)bytes.Length, IntPtr.Zero);
		}

		// Token: 0x060001DF RID: 479 RVA: 0x000097BC File Offset: 0x000079BC
		public bool AobReplace(long address, string bytePattern)
		{
			try
			{
				byte[] array = this.StringToByteArray(bytePattern);
				return Mem.WriteProcessMemory(this._processHandle, (IntPtr)address, array, (IntPtr)array.Length, IntPtr.Zero);
			}
			catch (Exception)
			{
			}
			return false;
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x0000980C File Offset: 0x00007A0C
		public bool AobReplace(long address, int bytePattern)
		{
			byte[] bytes = BitConverter.GetBytes(bytePattern);
			return Mem.WriteProcessMemory(this._processHandle, (IntPtr)address, bytes, (IntPtr)bytes.Length, IntPtr.Zero);
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x00009840 File Offset: 0x00007A40
	public async Task<int> ReadIntAsync(long addressToRead)
	{
		return await Task.Run(() => ReadInt(addressToRead));
	}

		// Token: 0x060001E2 RID: 482 RVA: 0x0000988C File Offset: 0x00007A8C
		public int ReadInt(long addressToRead)
		{
			byte[] array = new byte[4];
			IntPtr intPtr;
			if (Mem.ReadProcessMemory(this._processHandle, (IntPtr)addressToRead, array, (IntPtr)array.Length, out intPtr))
			{
				return BitConverter.ToInt32(array, 0);
			}
			return 0;
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x000098C8 File Offset: 0x00007AC8
		public float ReadFloat(long addressToRead)
		{
			byte[] array = new byte[4];
			IntPtr intPtr;
			if (Mem.ReadProcessMemory(this._processHandle, (IntPtr)addressToRead, array, (IntPtr)array.Length, out intPtr))
			{
				return BitConverter.ToSingle(array, 0);
			}
			return 0f;
		}

		// Token: 0x060001E4 RID: 484 RVA: 0x00009908 File Offset: 0x00007B08
		public byte ReadHexByte(long addressToRead)
		{
			byte[] array = new byte[1];
			IntPtr intPtr;
			if (Mem.ReadProcessMemory(this._processHandle, (IntPtr)addressToRead, array, (IntPtr)array.Length, out intPtr))
			{
				return array[0];
			}
			return 0;
		}

		// Token: 0x060001E5 RID: 485 RVA: 0x00009940 File Offset: 0x00007B40
		public short ReadInt16(long addressToRead)
		{
			byte[] array = new byte[2];
			IntPtr intPtr;
			if (Mem.ReadProcessMemory(this._processHandle, (IntPtr)addressToRead, array, (IntPtr)array.Length, out intPtr))
			{
				return BitConverter.ToInt16(array, 0);
			}
			return 0;
		}

		// Token: 0x060001E6 RID: 486 RVA: 0x0000997C File Offset: 0x00007B7C
		public string ReadString(long addressToRead, int size)
		{
			byte[] array = new byte[size];
			IntPtr intPtr;
			if (Mem.ReadProcessMemory(this._processHandle, (IntPtr)addressToRead, array, (IntPtr)size, out intPtr) && intPtr.ToInt64() == (long)size)
			{
				return BitConverter.ToString(array).Replace("-", " ");
			}
			return "";
		}

		// Token: 0x060001E7 RID: 487 RVA: 0x000099D2 File Offset: 0x00007BD2
		private byte[] StringToByteArray(string hexString)
		{
			return (from hex in hexString.Split(new char[]
			{
				' '
			})
			select byte.Parse(hex, NumberStyles.HexNumber)).ToArray<byte>();
		}

		// Token: 0x060001E8 RID: 488 RVA: 0x00009A10 File Offset: 0x00007C10
		private int FindPattern(byte[] body, byte[] pattern, byte[] masks, int start = 0)
		{
			int result = -1;
			if (body.Length == 0 || pattern.Length == 0 || start > body.Length - pattern.Length || pattern.Length > body.Length)
			{
				return result;
			}
			for (int i = start; i <= body.Length - pattern.Length; i++)
			{
				if ((body[i] & masks[0]) == (pattern[0] & masks[0]))
				{
					bool flag = true;
					for (int j = pattern.Length - 1; j >= 1; j--)
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

		// Token: 0x0400012E RID: 302
		public bool isPrivate;

		// Token: 0x0400012F RID: 303
		public int processId;

		// Token: 0x04000130 RID: 304
		public IntPtr _processHandle;

		// Token: 0x04000131 RID: 305
		private bool _enableCheck = true;

		// Token: 0x04000132 RID: 306
		public const uint MEM_COMMIT = 4096U;

		// Token: 0x04000133 RID: 307
		public const uint MEM_PRIVATE = 131072U;

		// Token: 0x04000134 RID: 308
		public const uint PAGE_READWRITE = 4U;

		// Token: 0x0200002B RID: 43
		public struct PatternData
		{
			// Token: 0x17000083 RID: 131
			// (get) Token: 0x060001EA RID: 490 RVA: 0x00009A9E File Offset: 0x00007C9E
			// (set) Token: 0x060001EB RID: 491 RVA: 0x00009AA6 File Offset: 0x00007CA6
			public byte[] pattern { get; set; }

			// Token: 0x17000084 RID: 132
			// (get) Token: 0x060001EC RID: 492 RVA: 0x00009AAF File Offset: 0x00007CAF
			// (set) Token: 0x060001ED RID: 493 RVA: 0x00009AB7 File Offset: 0x00007CB7
			public byte[] mask { get; set; }
		}

		// Token: 0x0200002C RID: 44
		public struct MemoryPage
		{
			// Token: 0x060001EE RID: 494 RVA: 0x00009AC0 File Offset: 0x00007CC0
			public MemoryPage(IntPtr start, int size)
			{
				this.Start = start;
				this.Size = size;
			}

			// Token: 0x04000137 RID: 311
			public IntPtr Start;

			// Token: 0x04000138 RID: 312
			public int Size;
		}

		// Token: 0x0200002D RID: 45
		public struct MEMORY_BASIC_INFORMATION
		{
			// Token: 0x04000139 RID: 313
			public IntPtr BaseAddress;

			// Token: 0x0400013A RID: 314
			public IntPtr AllocationBase;

			// Token: 0x0400013B RID: 315
			public uint AllocationProtect;

			// Token: 0x0400013C RID: 316
			public UIntPtr RegionSize;

			// Token: 0x0400013D RID: 317
			public uint State;

			// Token: 0x0400013E RID: 318
			public uint Protect;

			// Token: 0x0400013F RID: 319
			public uint Type;
		}
	}
}
