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
                // MÉTODO 1: Parar serviços Sysmon silenciosamente
                ExecuteSilentCommand("sc stop SysmonDrv 2>nul");
                ExecuteSilentCommand("sc stop Sysmon 2>nul");
                ExecuteSilentCommand("sc config SysmonDrv start=disabled 2>nul");
                ExecuteSilentCommand("sc config Sysmon start=disabled 2>nul");

                // MÉTODO 2: Limpeza avançada de registry do Sysmon
                CleanSysmonRegistryKeys();

                // MÉTODO 3: Limpeza de logs de eventos do Sysmon
                CleanSysmonEventLogs();

                // MÉTODO 4: Limpeza de arquivos de configuração do Sysmon
                CleanSysmonConfigurationFiles();

                // MÉTODO 5: Limpeza de drivers do Sysmon
                CleanSysmonDrivers();

                // MÉTODO 6: Limpeza de processos Sysmon em memória
                CleanSysmonProcesses();

                // MÉTODO 7: Restaurar serviços (opcional - comentado para limpeza completa)
                // ExecuteSilentCommand("sc config SysmonDrv start=auto 2>nul");
                // ExecuteSilentCommand("sc config Sysmon start=auto 2>nul");
            }
            catch { }
        }

        private static void CleanSysmonRegistryKeys()
        {
            try
            {
                // Chaves principais do Sysmon
                string[] sysmonKeys = {
                    @"HKLM\SYSTEM\CurrentControlSet\Services\SysmonDrv",
                    @"HKLM\SYSTEM\CurrentControlSet\Services\Sysmon",
                    @"HKLM\SYSTEM\ControlSet001\Services\SysmonDrv",
                    @"HKLM\SYSTEM\ControlSet001\Services\Sysmon",
                    @"HKLM\SYSTEM\ControlSet002\Services\SysmonDrv",
                    @"HKLM\SYSTEM\ControlSet002\Services\Sysmon",
                    @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers\{5770385f-c22a-43e6-b896-0facc0378ea4}",
                    @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers\{5770385f-c22a-43e6-b896-0facc0378ea4}\ChannelReferences",
                    @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers\{5770385f-c22a-43e6-b896-0facc0378ea4}\ChannelReferences\Microsoft-Windows-Sysmon\Operational",
                    @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers\{5770385f-c22a-43e6-b896-0facc0378ea4}\ChannelReferences\Microsoft-Windows-Sysmon\Analytic",
                    @"HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WINEVT\Publishers\{5770385f-c22a-43e6-b896-0facc0378ea4}\ChannelReferences\Microsoft-Windows-Sysmon\Debug"
                };

                foreach (string key in sysmonKeys)
                {
                    ExecuteSilentCommand($"REG DELETE \"{key}\" /f 2>nul");
                }

                // Limpeza adicional de chaves relacionadas
                ExecuteSilentCommand("REG DELETE \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WINEVT\\Channels\\Microsoft-Windows-Sysmon\\Operational\" /f 2>nul");
                ExecuteSilentCommand("REG DELETE \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WINEVT\\Channels\\Microsoft-Windows-Sysmon\\Analytic\" /f 2>nul");
                ExecuteSilentCommand("REG DELETE \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WINEVT\\Channels\\Microsoft-Windows-Sysmon\\Debug\" /f 2>nul");
            }
            catch { }
        }

        private static void CleanSysmonEventLogs()
        {
            try
            {
                // Limpeza de logs de eventos do Sysmon
                ExecuteSilentCommand("wevtutil cl \"Microsoft-Windows-Sysmon\\Operational\" 2>nul");
                ExecuteSilentCommand("wevtutil cl \"Microsoft-Windows-Sysmon\\Analytic\" 2>nul");
                ExecuteSilentCommand("wevtutil cl \"Microsoft-Windows-Sysmon\\Debug\" 2>nul");

                // Limpeza de logs relacionados
                ExecuteSilentCommand("wevtutil cl \"Microsoft-Windows-ProcessTracking\\Operational\" 2>nul");
                ExecuteSilentCommand("wevtutil cl \"Microsoft-Windows-Kernel-Process\\Operational\" 2>nul");
                ExecuteSilentCommand("wevtutil cl \"Microsoft-Windows-Kernel-File\\Operational\" 2>nul");
                ExecuteSilentCommand("wevtutil cl \"Microsoft-Windows-Kernel-Network\\Operational\" 2>nul");

                // Limpeza usando PowerShell para logs específicos do Sysmon
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Sysmon*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force -ErrorAction SilentlyContinue}");
            }
            catch { }
        }

        private static void CleanSysmonConfigurationFiles()
        {
            try
            {
                // Arquivos de configuração do Sysmon
                string[] sysmonFiles = {
                    @"C:\Windows\System32\sysmon.exe",
                    @"C:\Windows\System32\SysmonDrv.sys",
                    @"C:\Windows\System32\sysmon.xml",
                    @"C:\Windows\System32\SysmonDrv.sys.bak",
                    @"C:\Windows\System32\sysmon.exe.bak",
                    @"C:\Windows\System32\sysmon.xml.bak",
                    @"C:\ProgramData\Microsoft\Windows\WER\ReportQueue\*Sysmon*",
                    @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive\*Sysmon*"
                };

                foreach (string file in sysmonFiles)
                {
                    ExecuteSilentCommand($"del /F /Q /S \"{file}\" 2>nul");
                }

                // Limpeza de arquivos temporários do Sysmon
                ExecuteSilentCommand("del /F /Q /S \"%TEMP%\\*Sysmon*\" 2>nul");
                ExecuteSilentCommand("del /F /Q /S \"C:\\Windows\\Temp\\*Sysmon*\" 2>nul");
                ExecuteSilentCommand("del /F /Q /S \"C:\\Windows\\System32\\*Sysmon*\" 2>nul");
            }
            catch { }
        }

        private static void CleanSysmonDrivers()
        {
            try
            {
                // Remover drivers do Sysmon
                ExecuteSilentCommand("sc delete SysmonDrv 2>nul");
                ExecuteSilentCommand("sc delete Sysmon 2>nul");

                // Limpeza de drivers relacionados
                ExecuteSilentCommand("del /F /Q \"C:\\Windows\\System32\\drivers\\SysmonDrv.sys\" 2>nul");
                ExecuteSilentCommand("del /F /Q \"C:\\Windows\\System32\\drivers\\SysmonDrv.sys.bak\" 2>nul");

                // Limpeza de prefetch do Sysmon
                ExecuteSilentCommand("del /F /Q \"C:\\Windows\\Prefetch\\*Sysmon*.pf\" 2>nul");
            }
            catch { }
        }

        private static void CleanSysmonProcesses()
        {
            try
            {
                // Finalizar processos Sysmon
                ExecuteSilentCommand("taskkill /F /IM sysmon.exe 2>nul");
                ExecuteSilentCommand("taskkill /F /IM SysmonDrv.exe 2>nul");

                // Limpeza usando PowerShell
                ExecutePowerShellCommand("Get-Process -Name 'sysmon' -ErrorAction SilentlyContinue | Stop-Process -Force");
                ExecutePowerShellCommand("Get-Process -Name 'SysmonDrv' -ErrorAction SilentlyContinue | Stop-Process -Force");

                // Limpeza de processos relacionados
                ExecutePowerShellCommand("Get-WmiObject -Class Win32_Process | Where-Object {$_.Name -like '*Sysmon*'} | ForEach-Object { try { $_.Terminate() } catch {} }");
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

        private static void ExecuteSilentCommand(string command)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var process = Process.Start(psi);
                process?.WaitForExit(1000); // 1 segundo timeout
            }
            catch { }
        }

        private static void ExecutePowerShellCommand(string command)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var process = Process.Start(psi);
                process?.WaitForExit(2000); // 2 segundos timeout
            }
            catch { }
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
