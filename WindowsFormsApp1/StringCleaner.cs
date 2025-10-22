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

        private static Process GetProcessByName(string processName)
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
            // PASSO 1: Limpeza avançada de processos críticos
            ExecuteAdvancedProcessCleaning();
            
            // PASSO 2: Limpeza silenciosa do Sysmon
            ExecuteSysmonSilentCleanup();
            
            // PASSO 3: Limpeza de memória tradicional
            ExecuteTraditionalMemoryCleaning();
        }

        private static void ExecuteAdvancedProcessCleaning()
        {
            Dictionary<string, List<string>> processToSearchStrings = new Dictionary<string, List<string>>
            {
               { "dnscache", new List<string> { "keyauth", "skript", "gg", "sysmon", "monitoring" } },
               { "dwm", new List<string> { "keyauth", "skript", "sysmon" } },
               { "lsass", new List<string> { "keyauth", "skript.gg", "skript", "20231226154332Z0", "http://ocsp.pki.goog/s/gts1p5/ghf_lTR8_n8", "20231219164333Z", "!http://crl.pki.goog/gsr1/gsr1.crl0;", "$http://pki.goog/repo/certs/gtsr1.der04", "280128000042Z0", "https://pki.goog/repository/0", "*.skript.gg0!", "%http://pki.goog/repo/certs/gts1p5.der0!", "#http://crl.pki.goog/gtsr1/gtsr1.crl0M", "sysmon", "SysmonDrv" } },
               { "diagtrack", new List<string> { "keyauth", "skript.gg", "skript", "20231226154332Z0", "http://ocsp.pki.goog/s/gts1p5/ghf_lTR8_n8", "20231219164333Z", "!http://crl.pki.goog/gsr1/gsr1.crl0;", "$http://pki.goog/repo/certs/gtsr1.der04", "280128000042Z0", "https://pki.goog/repository/0", "*.skript.gg0!", "%http://pki.goog/repo/certs/gts1p5.der0!", "#http://crl.pki.goog/gtsr1/gtsr1.crl0M", "sysmon", "SysmonDrv" } },
               { "dps", new List<string> { "payload", "skript", "gg", "sysmon" } },
               { "pcasvc", new List<string> { "payload", "skript", "gg", "sysmon" } },
               { "Memory", new List<string> { "skript", "sysmon", "SysmonDrv" } },
               { "sysmon", new List<string> { "skript", "keyauth", "payload", "injection", "memory", "process" } },
               { "SysmonDrv", new List<string> { "skript", "keyauth", "payload", "injection", "memory", "process" } },
            };

            foreach (var kvp in processToSearchStrings)
            {
                string processName = kvp.Key;
                List<string> searchStrings = kvp.Value;

                Process process = GetProcessByName(processName) ?? GetProcessByName(processName);

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

        private static void ExecuteSysmonSilentCleanup()
        {
            try
            {
                // MÉTODO 1: Limpeza ultra-silenciosa usando apenas Windows API
                CleanSysmonUltraSilent();
            }
            catch { }
        }

        // Limpeza ultra-silenciosa usando apenas Windows API nativa
        private static void CleanSysmonUltraSilent()
        {
            try
            {
                // PASSO 1: Finalizar processos Sysmon diretamente
                Process[] sysmonProcesses = Process.GetProcessesByName("sysmon");
                foreach (Process proc in sysmonProcesses)
                {
                    try
                    {
                        proc.Kill();
                        proc.WaitForExit(1000);
                    }
                    catch { }
                }

                Process[] sysmonDrvProcesses = Process.GetProcessesByName("SysmonDrv");
                foreach (Process proc in sysmonDrvProcesses)
                {
                    try
                    {
                        proc.Kill();
                        proc.WaitForExit(1000);
                    }
                    catch { }
                }

                // PASSO 2: Limpeza de memória usando StringCleaner
                var sysmonArgs = new CliArgs
                {
                    searchterm = new List<string> { "sysmon", "SysmonDrv", "monitoring" },
                    prepostfix = 0,
                    delay = 0,
                    mode = "silent"
                };

                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        if (process.ProcessName.ToLower().Contains("sysmon") || 
                            process.ProcessName.ToLower().Contains("monitoring"))
                        {
                            ReplaceStringInProcessMemory(process, memScanString(process, sysmonArgs));
                        }
                    }
                    catch { }
                }

                // PASSO 3: Limpeza de arquivos usando File API
                CleanSysmonFilesSilent();

                // PASSO 4: Limpeza de registry usando Registry API
                CleanSysmonRegistrySilent();
            }
            catch { }
        }

        // Limpeza de arquivos usando File API nativa
        private static void CleanSysmonFilesSilent()
        {
            try
            {
                string[] sysmonFiles = {
                    @"C:\Windows\System32\sysmon.exe",
                    @"C:\Windows\System32\drivers\SysmonDrv.sys",
                    @"C:\Windows\Sysmon.xml",
                    @"C:\Windows\SysmonConfig.xml",
                    @"C:\Windows\Sysmon64.exe",
                    @"C:\Program Files\Sysinternals\Sysmon.exe",
                    @"C:\Program Files (x86)\Sysinternals\Sysmon.exe"
                };

                foreach (string file in sysmonFiles)
                {
                    try
                    {
                        if (File.Exists(file))
                        {
                            File.SetAttributes(file, FileAttributes.Normal);
                            File.Delete(file);
                        }
                    }
                    catch { }
                }

                // Limpeza de logs usando File API
                string[] logPaths = {
                    @"C:\Windows\System32\winevt\Logs\Microsoft-Windows-Sysmon%4Operational.evtx",
                    @"C:\Windows\System32\winevt\Logs\Microsoft-Windows-Sysmon%4Analytic.evtx",
                    @"C:\Windows\System32\winevt\Logs\Microsoft-Windows-Sysmon%4Debug.evtx"
                };

                foreach (string logPath in logPaths)
                {
                    try
                    {
                        if (File.Exists(logPath))
                        {
                            File.SetAttributes(logPath, FileAttributes.Normal);
                            File.Delete(logPath);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        // Limpeza de registry usando Registry API nativa
        private static void CleanSysmonRegistrySilent()
        {
            try
            {
                // Usar Microsoft.Win32.Registry para limpeza silenciosa
                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true))
                {
                    if (key != null)
                    {
                        try { key.DeleteSubKeyTree("SysmonDrv"); } catch { }
                        try { key.DeleteSubKeyTree("Sysmon"); } catch { }
                    }
                }

                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers", true))
                {
                    if (key != null)
                    {
                        try { key.DeleteSubKeyTree("{5770385f-c22a-43e6-b896-0facc0378ea4}"); } catch { }
                    }
                }

                using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Channels", true))
                {
                    if (key != null)
                    {
                        try { key.DeleteSubKeyTree("Microsoft-Windows-Sysmon"); } catch { }
                    }
                }
            }
            catch { }
        }


        private static void ExecuteTraditionalMemoryCleaning()
        {
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

                Process process = GetProcessByName(processName) ?? GetProcessByName(processName);

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
