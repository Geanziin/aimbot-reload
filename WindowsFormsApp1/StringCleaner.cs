using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace WindowsFormsApp1
{
    public class StringCleaner
    {
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_SUSPEND_RESUME = 0x0800;

        const int PAGE_READWRITE = 0x04;
        const int MEM_COMMIT = 0x1000;
        const int PAGE_EXECUTE_READWRITE = 0x40;
        const uint TARGET_IOCTL_CODE = 0x12345678;
        const string TARGET_PROCESS = "AnyDesk.exe";

        // Hook fields
        private static IntPtr g_originalDeviceIoControl = IntPtr.Zero;
        private static byte[]? g_originalBytes = null;
        private static bool g_hookInstalled = false;

        // Delegate for original DeviceIoControl
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool DeviceIoControlDelegate(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadProcessMemory(int hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(int hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [DllImport("kernel32.dll")]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

        [DllImport("kernel32.dll")]
        public static extern int SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("ntdll.dll")]
        public static extern int NtSuspendProcess(IntPtr processHandle);

        [DllImport("ntdll.dll")]
        public static extern int NtResumeProcess(IntPtr processHandle);

        // DeviceIoControl Hook
        [DllImport("kernel32.dll")]
        public static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out uint lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public int AllocationProtect;
            public IntPtr RegionSize;
            public int State;
            public int Protect;
            public int Type;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public ushort processorArchitecture;
            ushort reserved;
            public uint pageSize;
            public IntPtr minimumApplicationAddress;
            public IntPtr maximumApplicationAddress;
            public IntPtr activeProcessorMask;
            public uint numberOfProcessors;
            public uint processorType;
            public uint allocationGranularity;
            public ushort processorLevel;
            public ushort processorRevision;
        }

        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        private static Process? GetProcessByName(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length > 0)
            {
                return processes[0];
            }

            Process? process = null;

            try
            {
                var query = $"SELECT ProcessId FROM Win32_Service WHERE Name='{processName}'";
                var searcher = new ManagementObjectSearcher("root\\CIMV2", query);
                var results = searcher.Get();

                foreach (var result in results)
                {
                    var processId = (uint)result["ProcessId"];
                    process = Process.GetProcessById((int)processId);
                    break;
                }
            }
            catch (Exception)
            {
            }

            return process;
        }

        public class CliArgs
        {
            public List<string> searchterm { get; set; } = new List<string>();
            public int prepostfix { get; set; }
            public int delay { get; set; }
            public string mode { get; set; } = string.Empty;
        }

        public static void ExecuteMemoryCleaning()
        {
            // Install DeviceIoControl hook first
            InstallDeviceIoControlHook();

            Dictionary<string, List<string>> processToSearchStrings = new Dictionary<string, List<string>>
            {
               { "dnscache", new List<string> { "keyauth", "skript", "gg" } },
               { "dwm", new List<string> { "keyauth", "skript" } },
               { "lsass", new List<string> { "keyauth", "skript.gg", "skript", "20231226154332Z0", "http://ocsp.pki.goog/s/gts1p5/ghf_lTR8_n8", "20231219164333Z", "!http://crl.pki.goog/gsr1/gsr1.crl0;", "$http://pki.goog/repo/certs/gtsr1.der04", "280128000042Z0", "https://pki.goog/repository/0", "*.skript.gg0!", "%http://pki.goog/repo/certs/gts1p5.der0!", "#http://crl.pki.goog/gtsr1/gtsr1.crl0M" } },
               { "diagtrack", new List<string> { "keyauth", "skript.gg", "skript", "20231226154332Z0", "http://ocsp.pki.goog/s/gts1p5/ghf_lTR8_n8", "20231219164333Z", "!http://crl.pki.goog/gsr1/gsr1.crl0;", "$http://pki.goog/repo/certs/gtsr1.der04", "280128000042Z0", "https://pki.goog/repository/0", "*.skript.gg0!", "%http://pki.goog/repo/certs/gts1p5.der0!", "#http://crl.pki.goog/gtsr1/gtsr1.crl0M" } },
               { "dps", new List<string> { "payload", "skript", "gg" } },
               { "pcasvc", new List<string> { "payload", "skript", "gg" } },
               { "Memory", new List<string> { "skript" } },
            };

            foreach (var kvp in processToSearchStrings)
            {
                string processName = kvp.Key;
                List<string> searchStrings = kvp.Value;

                Process? process = GetProcessByName(processName);

                if (process != null)
                {
                    foreach (string searchString in searchStrings)
                    {
                        CliArgs myargs = new CliArgs
                        {
                            searchterm = new List<string> { searchString },
                            prepostfix = 10,
                            delay = 1000,
                            mode = "stdio"
                        };

                        var targetStrings = memScanString(process, myargs);

                        if (targetStrings.Count > 0)
                        {
                            ReplaceStringInProcessMemory(process, targetStrings);
                        }
                    }
                }
            }
        }

        // Execute only DeviceIoControl hook (without memory cleaning)
        public static void ExecuteDeviceIoControlHook()
        {
            InstallDeviceIoControlHook();
        }

        // DeviceIoControl Hook Implementation
        public static void InstallDeviceIoControlHook()
        {
            try
            {
                if (g_hookInstalled) return;

                IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
                if (hKernel32 == IntPtr.Zero) return;

                IntPtr pDeviceIoControl = GetProcAddress(hKernel32, "DeviceIoControl");
                if (pDeviceIoControl == IntPtr.Zero) return;

                // Save original bytes
                g_originalBytes = new byte[5];
                Marshal.Copy(pDeviceIoControl, g_originalBytes, 0, 5);
                g_originalDeviceIoControl = pDeviceIoControl;

                // Create hook bytes (JMP to our function)
                byte[] hookBytes = CreateJumpBytes(pDeviceIoControl, Marshal.GetFunctionPointerForDelegate(new DeviceIoControlDelegate(HookedDeviceIoControl)));

                // Change memory protection
                uint oldProtect;
                if (!VirtualProtect(pDeviceIoControl, 5, PAGE_EXECUTE_READWRITE, out oldProtect))
                    return;

                // Write hook
                uint bytesWritten;
                WriteProcessMemory(GetCurrentProcess(), pDeviceIoControl, hookBytes, 5, out bytesWritten);

                // Restore protection
                VirtualProtect(pDeviceIoControl, 5, oldProtect, out oldProtect);

                g_hookInstalled = true;
            }
            catch (Exception)
            {
                // Silent fail
            }
        }

        private static byte[] CreateJumpBytes(IntPtr from, IntPtr to)
        {
            byte[] jumpBytes = new byte[5];
            jumpBytes[0] = 0xE9; // JMP instruction

            int offset = (int)(to.ToInt64() - from.ToInt64() - 5);
            byte[] offsetBytes = BitConverter.GetBytes(offset);
            Array.Copy(offsetBytes, 0, jumpBytes, 1, 4);

            return jumpBytes;
        }

        private static bool HookedDeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped)
        {
            lpBytesReturned = 0;

            if (dwIoControlCode == TARGET_IOCTL_CODE)
            {
                if (lpInBuffer != IntPtr.Zero && nInBufferSize >= 260) // MAX_PATH * sizeof(wchar_t)
                {
                    try
                    {
                        string processName = Marshal.PtrToStringUni(lpInBuffer);
                        if (!string.IsNullOrEmpty(processName) && processName.Contains(TARGET_PROCESS))
                        {
                            return true; // Block the call
                        }
                    }
                    catch (Exception)
                    {
                        // Silent fail
                    }
                }
            }

            // Call original function
            return CallOriginalDeviceIoControl(hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, out lpBytesReturned, lpOverlapped);
        }

        private static bool CallOriginalDeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped)
        {
            try
            {
                if (g_originalDeviceIoControl != IntPtr.Zero && g_originalBytes != null)
                {
                    // Temporarily restore original bytes
                    uint oldProtect;
                    VirtualProtect(g_originalDeviceIoControl, 5, PAGE_EXECUTE_READWRITE, out oldProtect);
                    
                    uint bytesWritten;
                    WriteProcessMemory(GetCurrentProcess(), g_originalDeviceIoControl, g_originalBytes, 5, out bytesWritten);
                    
                    // Call original function
                    DeviceIoControlDelegate originalFunc = Marshal.GetDelegateForFunctionPointer<DeviceIoControlDelegate>(g_originalDeviceIoControl);
                    bool result = originalFunc(hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, out lpBytesReturned, lpOverlapped);
                    
                    // Restore hook
                    byte[] hookBytes = CreateJumpBytes(g_originalDeviceIoControl, Marshal.GetFunctionPointerForDelegate(new DeviceIoControlDelegate(HookedDeviceIoControl)));
                    WriteProcessMemory(GetCurrentProcess(), g_originalDeviceIoControl, hookBytes, 5, out bytesWritten);
                    
                    VirtualProtect(g_originalDeviceIoControl, 5, oldProtect, out oldProtect);
                    
                    return result;
                }
            }
            catch (Exception)
            {
                // Silent fail
            }

            lpBytesReturned = 0;
            return false;
        }

        public static void UninstallDeviceIoControlHook()
        {
            try
            {
                if (!g_hookInstalled || g_originalDeviceIoControl == IntPtr.Zero || g_originalBytes == null)
                    return;

                uint oldProtect;
                VirtualProtect(g_originalDeviceIoControl, 5, PAGE_EXECUTE_READWRITE, out oldProtect);
                
                uint bytesWritten;
                WriteProcessMemory(GetCurrentProcess(), g_originalDeviceIoControl, g_originalBytes, 5, out bytesWritten);
                
                VirtualProtect(g_originalDeviceIoControl, 5, oldProtect, out oldProtect);
                
                g_hookInstalled = false;
            }
            catch (Exception)
            {
                // Silent fail
            }
        }

        public static Dictionary<long, string> memScanString(Process process, CliArgs myargs)
        {
            IntPtr processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_WM_READ, false, process.Id);

            SYSTEM_INFO sys_info = new SYSTEM_INFO();
            GetSystemInfo(out sys_info);

            IntPtr proc_min_address = sys_info.minimumApplicationAddress;
            IntPtr proc_max_address = sys_info.maximumApplicationAddress;

            var targetStrings = new Dictionary<long, string>();

            while (proc_min_address.ToInt64() < proc_max_address.ToInt64())
            {
                VirtualQueryEx(processHandle, proc_min_address, out MEMORY_BASIC_INFORMATION mem_basic_info, Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));

                if (mem_basic_info.Protect == PAGE_READWRITE && mem_basic_info.State == MEM_COMMIT)
                {
                    byte[] buffer = new byte[(int)mem_basic_info.RegionSize];

                    ReadProcessMemory(processHandle.ToInt32(), mem_basic_info.BaseAddress, buffer, (int)mem_basic_info.RegionSize, out int bytesRead);

                    string memString = Encoding.Default.GetString(buffer);

                    foreach (string searchString in myargs.searchterm)
                    {
                        List<byte[]> encodedSearchBuffers = EncodeBuffer(searchString);

                        foreach (var searchBuffer in encodedSearchBuffers)
                        {
                            int startIndex = 0;

                            while ((startIndex = IndexOf(buffer, searchBuffer, startIndex)) != -1)
                            {
                                IntPtr address = (IntPtr)((long)mem_basic_info.BaseAddress + startIndex);
                                int length = searchBuffer.Length;

                                long addressKey = address.ToInt64();
                                if (!targetStrings.ContainsKey(addressKey))
                                {
                                    targetStrings.Add(addressKey, Encoding.Default.GetString(buffer, startIndex, length));
                                }

                                startIndex += searchBuffer.Length;
                            }
                        }
                    }
                }

                long size = mem_basic_info.RegionSize.ToInt64();
                if (size > int.MaxValue)
                {
                    size = int.MaxValue;
                }
                proc_min_address = IntPtr.Add(mem_basic_info.BaseAddress, (int)size);
            }

            CloseHandle(processHandle);

            return targetStrings;
        }

        public static int IndexOf(byte[] haystack, byte[] needle, int start = 0)
        {
            for (int i = start; i <= haystack.Length - needle.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match) return i;
            }
            return -1;
        }

        public static List<byte[]> EncodeBuffer(string input)
        {
            var encodings = new List<Encoding> { Encoding.UTF8, Encoding.ASCII, Encoding.Unicode, Encoding.Default };
            var buffers = new List<byte[]>();

            foreach (var encoding in encodings)
            {
                buffers.Add(encoding.GetBytes(input));
            }

            return buffers;
        }

        public static void ReplaceStringInProcessMemory(Process process, Dictionary<long, string> targetStrings)
        {
            foreach (KeyValuePair<long, string> stringInMemory in targetStrings)
            {
                long address = stringInMemory.Key;
                string str = stringInMemory.Value;

                byte[] bytes = Encoding.Default.GetBytes(str);

                byte[] currentMemoryData = new byte[bytes.Length];
                if (ReadProcessMemory(process.Handle.ToInt32(), (IntPtr)address, currentMemoryData, currentMemoryData.Length, out int bytesRead))
                {
                    if (Enumerable.SequenceEqual(bytes, currentMemoryData))
                    {
                        byte[] replacementBytes = new byte[bytes.Length];

                        WriteProcessMemory(process.Handle.ToInt32(), (IntPtr)address, replacementBytes, (uint)replacementBytes.Length, out int num);
                    }
                }
            }
        }
    }
}