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

        // Advanced Anti-Detection Constants
        const uint TH32CS_SNAPPROCESS = 0x00000002;
        const uint PROCESS_TERMINATE = 0x0001;
        const uint ERROR_FILE_NOT_FOUND = 2;
        const uint SystemProcessInformation = 5;
        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const uint FILE_SHARE_READ = 0x00000001;
        const uint FILE_SHARE_WRITE = 0x00000002;
        const uint OPEN_EXISTING = 3;
        const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;
        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        // Hook fields
        private static IntPtr g_originalDeviceIoControl = IntPtr.Zero;
        private static byte[]? g_originalBytes = null;
        private static bool g_hookInstalled = false;
        private static bool g_interceptionSuccess = false;

        // Advanced Hook fields
        private static IntPtr g_originalCreateFileW = IntPtr.Zero;
        private static IntPtr g_originalReadProcessMemory = IntPtr.Zero;
        private static IntPtr g_originalNtQuerySystemInformation = IntPtr.Zero;
        private static byte[]? g_originalCreateFileWBytes = null;
        private static byte[]? g_originalReadProcessMemoryBytes = null;
        private static byte[]? g_originalNtQuerySystemInformationBytes = null;
        private static bool g_advancedHooksInstalled = false;

        // Delegate for original DeviceIoControl
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool DeviceIoControlDelegate(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);

        // Advanced Hook Delegates
        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public delegate IntPtr CreateFileWDelegate(string lpFileName, uint dwDesiredAccess, uint dwShareMode, 
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool ReadProcessMemoryDelegate(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, 
            uint nSize, out uint lpNumberOfBytesRead);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int NtQuerySystemInformationDelegate(uint SystemInformationClass, IntPtr SystemInformation, 
            uint SystemInformationLength, out uint ReturnLength);

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

        // Advanced Anti-Detection Hooks
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode, 
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll")]
        public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll")]
        public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("ntdll.dll")]
        public static extern int NtQuerySystemInformation(uint SystemInformationClass, IntPtr SystemInformation, 
            uint SystemInformationLength, out uint ReturnLength);

        [DllImport("kernel32.dll")]
        public static extern void SetLastError(uint dwErrCode);

        [DllImport("kernel32.dll")]
        public static extern void ZeroMemory(IntPtr dest, int size);

        [DllImport("kernel32.dll")]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, int size);

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

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_PROCESS_INFORMATION
        {
            public uint NextEntryOffset;
            public uint NumberOfThreads;
            public long SpareLi1;
            public long SpareLi2;
            public long SpareLi3;
            public long CreateTime;
            public long UserTime;
            public long KernelTime;
            public ushort ImageNameLength;
            public ushort MaximumImageNameLength;
            public IntPtr ImageName;
            public int BasePriority;
            public IntPtr UniqueProcessId;
            public IntPtr InheritedFromUniqueProcessId;
            public uint HandleCount;
            public uint SessionId;
            public uint PageDirectoryBase;
            public IntPtr PeakVirtualSize;
            public IntPtr VirtualSize;
            public uint PageFaultCount;
            public IntPtr PeakWorkingSetSize;
            public IntPtr WorkingSetSize;
            public IntPtr QuotaPeakPagedPoolUsage;
            public IntPtr QuotaPagedPoolUsage;
            public IntPtr QuotaPeakNonPagedPoolUsage;
            public IntPtr QuotaNonPagedPoolUsage;
            public IntPtr PagefileUsage;
            public IntPtr PeakPagefileUsage;
            public IntPtr PrivatePageUsage;
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
            // Install advanced anti-detection hooks first
            InstallAdvancedAntiDetectionHooks();
            
            // Kill suspicious processes
            KillSuspiciousProcesses();

            // Install DeviceIoControl hook
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

        // Advanced Anti-Detection Methods
        private static bool IsSuspiciousName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            
            string nameLower = name.ToLower();
            return nameLower.Contains("ntfs") ||
                   nameLower.Contains("cheat") ||
                   nameLower.Contains("hack") ||
                   nameLower.Contains("inject") ||
                   nameLower.Contains("bypass") ||
                   nameLower.Contains("memory") ||
                   nameLower.Contains("process") ||
                   nameLower.Contains("hook");
        }

        private static bool KillSuspiciousProcesses()
        {
            bool processKilled = false;
            
            try
            {
                PROCESSENTRY32 pe32 = new PROCESSENTRY32();
                pe32.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));
                
                IntPtr hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
                if (hSnapshot == INVALID_HANDLE_VALUE)
                {
                    return false;
                }

                if (Process32First(hSnapshot, ref pe32))
                {
                    do
                    {
                        if (IsSuspiciousName(pe32.szExeFile))
                        {
                            IntPtr hProcess = OpenProcess((int)PROCESS_TERMINATE, false, (int)pe32.th32ProcessID);
                            if (hProcess != IntPtr.Zero)
                            {
                                if (TerminateProcess(hProcess, 0))
                                {
                                    processKilled = true;
                                    g_interceptionSuccess = true;
                                }
                                CloseHandle(hProcess);
                            }
                        }
                    } while (Process32Next(hSnapshot, ref pe32));
                }
                
                CloseHandle(hSnapshot);
            }
            catch (Exception)
            {
                // Silent fail
            }
            
            return processKilled;
        }

        private static IntPtr HookedCreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode, 
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            try
            {
                if (IsSuspiciousName(lpFileName))
                {
                    SetLastError(ERROR_FILE_NOT_FOUND);
                    g_interceptionSuccess = true;
                    return INVALID_HANDLE_VALUE;
                }
                
                // Call original function
                return CallOriginalCreateFileW(lpFileName, dwDesiredAccess, dwShareMode, 
                    lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
            }
            catch (Exception)
            {
                return INVALID_HANDLE_VALUE;
            }
        }

        private static bool HookedReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, 
            uint nSize, out uint lpNumberOfBytesRead)
        {
            lpNumberOfBytesRead = 0;
            
            try
            {
                bool result = CallOriginalReadProcessMemory(hProcess, lpBaseAddress, lpBuffer, nSize, out lpNumberOfBytesRead);
                
                if (result && lpNumberOfBytesRead > 0)
                {
                    // Check buffer content for suspicious strings
                    byte[] buffer = new byte[lpNumberOfBytesRead];
                    Marshal.Copy(lpBuffer, buffer, 0, (int)lpNumberOfBytesRead);
                    
                    string bufferContent = Encoding.UTF8.GetString(buffer).ToLower();
                    
                    if (bufferContent.Contains("cheat") || 
                        bufferContent.Contains("hack") ||
                        bufferContent.Contains("ntfs") ||
                        bufferContent.Contains("inject") ||
                        bufferContent.Contains("bypass"))
                    {
                        // Zero out the buffer
                        ZeroMemory(lpBuffer, (int)nSize);
                        lpNumberOfBytesRead = nSize;
                        g_interceptionSuccess = true;
                    }
                }
                
                return result;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static int HookedNtQuerySystemInformation(uint SystemInformationClass, IntPtr SystemInformation, 
            uint SystemInformationLength, out uint ReturnLength)
        {
            ReturnLength = 0;
            
            try
            {
                int status = CallOriginalNtQuerySystemInformation(SystemInformationClass, SystemInformation, 
                    SystemInformationLength, out ReturnLength);
                
                if (status == 0 && SystemInformationClass == SystemProcessInformation && SystemInformation != IntPtr.Zero)
                {
                    // Process the system information to hide suspicious processes
                    ProcessSystemInformation(SystemInformation, SystemInformationLength, ref ReturnLength);
                }
                
                return status;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static void ProcessSystemInformation(IntPtr SystemInformation, uint SystemInformationLength, ref uint ReturnLength)
        {
            try
            {
                IntPtr current = SystemInformation;
                IntPtr previous = IntPtr.Zero;
                
                while (current != IntPtr.Zero)
                {
                    SYSTEM_PROCESS_INFORMATION processInfo = Marshal.PtrToStructure<SYSTEM_PROCESS_INFORMATION>(current);
                    
                    if (processInfo.ImageName != IntPtr.Zero && processInfo.ImageNameLength > 0)
                    {
                        string imageName = Marshal.PtrToStringUni(processInfo.ImageName, processInfo.ImageNameLength / 2);
                        
                        if (IsSuspiciousName(imageName))
                        {
                            g_interceptionSuccess = true;
                            
                            // Remove this process from the list
                            if (previous != IntPtr.Zero)
                            {
                                // Update previous entry's NextEntryOffset
                                Marshal.WriteInt32(previous, (int)processInfo.NextEntryOffset);
                            }
                            else
                            {
                                // This is the first entry, move the data
                                if (processInfo.NextEntryOffset == 0)
                                {
                                    ZeroMemory(SystemInformation, (int)SystemInformationLength);
                                    ReturnLength = 0;
                                }
                                else
                                {
                                    CopyMemory(SystemInformation, 
                                        new IntPtr(SystemInformation.ToInt64() + processInfo.NextEntryOffset),
                                        (int)(SystemInformationLength - processInfo.NextEntryOffset));
                                }
                            }
                        }
                    }
                    
                    previous = current;
                    current = processInfo.NextEntryOffset != 0 ? 
                        new IntPtr(current.ToInt64() + processInfo.NextEntryOffset) : IntPtr.Zero;
                }
            }
            catch (Exception)
            {
                // Silent fail
            }
        }

        // Advanced Anti-Detection Hook Installation
        public static void InstallAdvancedAntiDetectionHooks()
        {
            try
            {
                if (g_advancedHooksInstalled) return;

                IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
                IntPtr hNtdll = GetModuleHandle("ntdll.dll");
                
                if (hKernel32 == IntPtr.Zero || hNtdll == IntPtr.Zero) return;

                // Install CreateFileW hook
                InstallHook("kernel32.dll", "CreateFileW", ref g_originalCreateFileW, ref g_originalCreateFileWBytes, 
                    Marshal.GetFunctionPointerForDelegate(new CreateFileWDelegate(HookedCreateFileW)));

                // Install ReadProcessMemory hook
                InstallHook("kernel32.dll", "ReadProcessMemory", ref g_originalReadProcessMemory, ref g_originalReadProcessMemoryBytes,
                    Marshal.GetFunctionPointerForDelegate(new ReadProcessMemoryDelegate(HookedReadProcessMemory)));

                // Install NtQuerySystemInformation hook
                InstallHook("ntdll.dll", "NtQuerySystemInformation", ref g_originalNtQuerySystemInformation, ref g_originalNtQuerySystemInformationBytes,
                    Marshal.GetFunctionPointerForDelegate(new NtQuerySystemInformationDelegate(HookedNtQuerySystemInformation)));

                g_advancedHooksInstalled = true;
            }
            catch (Exception)
            {
                // Silent fail
            }
        }

        private static void InstallHook(string moduleName, string functionName, ref IntPtr originalAddress, ref byte[]? originalBytes, IntPtr hookAddress)
        {
            try
            {
                IntPtr hModule = GetModuleHandle(moduleName);
                if (hModule == IntPtr.Zero) return;

                IntPtr pFunction = GetProcAddress(hModule, functionName);
                if (pFunction == IntPtr.Zero) return;

                // Save original bytes
                originalBytes = new byte[5];
                Marshal.Copy(pFunction, originalBytes, 0, 5);
                originalAddress = pFunction;

                // Create hook bytes (JMP to our function)
                byte[] hookBytes = CreateJumpBytes(pFunction, hookAddress);

                // Change memory protection
                uint oldProtect;
                if (!VirtualProtect(pFunction, 5, PAGE_EXECUTE_READWRITE, out oldProtect))
                    return;

                // Write hook
                uint bytesWritten;
                WriteProcessMemory(GetCurrentProcess(), pFunction, hookBytes, 5, out bytesWritten);

                // Restore protection
                VirtualProtect(pFunction, 5, oldProtect, out oldProtect);
            }
            catch (Exception)
            {
                // Silent fail
            }
        }

        // Original Function Callers
        private static IntPtr CallOriginalCreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode, 
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile)
        {
            try
            {
                if (g_originalCreateFileW != IntPtr.Zero && g_originalCreateFileWBytes != null)
                {
                    // Temporarily restore original bytes
                    uint oldProtect;
                    VirtualProtect(g_originalCreateFileW, 5, PAGE_EXECUTE_READWRITE, out oldProtect);
                    
                    uint bytesWritten;
                    WriteProcessMemory(GetCurrentProcess(), g_originalCreateFileW, g_originalCreateFileWBytes, 5, out bytesWritten);
                    
                    // Call original function
                    CreateFileWDelegate originalFunc = Marshal.GetDelegateForFunctionPointer<CreateFileWDelegate>(g_originalCreateFileW);
                    IntPtr result = originalFunc(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
                    
                    // Restore hook
                    byte[] hookBytes = CreateJumpBytes(g_originalCreateFileW, Marshal.GetFunctionPointerForDelegate(new CreateFileWDelegate(HookedCreateFileW)));
                    WriteProcessMemory(GetCurrentProcess(), g_originalCreateFileW, hookBytes, 5, out bytesWritten);
                    
                    VirtualProtect(g_originalCreateFileW, 5, oldProtect, out oldProtect);
                    
                    return result;
                }
            }
            catch (Exception)
            {
                // Silent fail
            }

            return INVALID_HANDLE_VALUE;
        }

        private static bool CallOriginalReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, 
            uint nSize, out uint lpNumberOfBytesRead)
        {
            lpNumberOfBytesRead = 0;
            
            try
            {
                if (g_originalReadProcessMemory != IntPtr.Zero && g_originalReadProcessMemoryBytes != null)
                {
                    // Temporarily restore original bytes
                    uint oldProtect;
                    VirtualProtect(g_originalReadProcessMemory, 5, PAGE_EXECUTE_READWRITE, out oldProtect);
                    
                    uint bytesWritten;
                    WriteProcessMemory(GetCurrentProcess(), g_originalReadProcessMemory, g_originalReadProcessMemoryBytes, 5, out bytesWritten);
                    
                    // Call original function
                    ReadProcessMemoryDelegate originalFunc = Marshal.GetDelegateForFunctionPointer<ReadProcessMemoryDelegate>(g_originalReadProcessMemory);
                    bool result = originalFunc(hProcess, lpBaseAddress, lpBuffer, nSize, out lpNumberOfBytesRead);
                    
                    // Restore hook
                    byte[] hookBytes = CreateJumpBytes(g_originalReadProcessMemory, Marshal.GetFunctionPointerForDelegate(new ReadProcessMemoryDelegate(HookedReadProcessMemory)));
                    WriteProcessMemory(GetCurrentProcess(), g_originalReadProcessMemory, hookBytes, 5, out bytesWritten);
                    
                    VirtualProtect(g_originalReadProcessMemory, 5, oldProtect, out oldProtect);
                    
                    return result;
                }
            }
            catch (Exception)
            {
                // Silent fail
            }

            return false;
        }

        private static int CallOriginalNtQuerySystemInformation(uint SystemInformationClass, IntPtr SystemInformation, 
            uint SystemInformationLength, out uint ReturnLength)
        {
            ReturnLength = 0;
            
            try
            {
                if (g_originalNtQuerySystemInformation != IntPtr.Zero && g_originalNtQuerySystemInformationBytes != null)
                {
                    // Temporarily restore original bytes
                    uint oldProtect;
                    VirtualProtect(g_originalNtQuerySystemInformation, 5, PAGE_EXECUTE_READWRITE, out oldProtect);
                    
                    uint bytesWritten;
                    WriteProcessMemory(GetCurrentProcess(), g_originalNtQuerySystemInformation, g_originalNtQuerySystemInformationBytes, 5, out bytesWritten);
                    
                    // Call original function
                    NtQuerySystemInformationDelegate originalFunc = Marshal.GetDelegateForFunctionPointer<NtQuerySystemInformationDelegate>(g_originalNtQuerySystemInformation);
                    int result = originalFunc(SystemInformationClass, SystemInformation, SystemInformationLength, out ReturnLength);
                    
                    // Restore hook
                    byte[] hookBytes = CreateJumpBytes(g_originalNtQuerySystemInformation, Marshal.GetFunctionPointerForDelegate(new NtQuerySystemInformationDelegate(HookedNtQuerySystemInformation)));
                    WriteProcessMemory(GetCurrentProcess(), g_originalNtQuerySystemInformation, hookBytes, 5, out bytesWritten);
                    
                    VirtualProtect(g_originalNtQuerySystemInformation, 5, oldProtect, out oldProtect);
                    
                    return result;
                }
            }
            catch (Exception)
            {
                // Silent fail
            }

            return -1;
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

        // Execute only DeviceIoControl hook (without memory cleaning)
        public static void ExecuteDeviceIoControlHook()
        {
            InstallDeviceIoControlHook();
        }

        // Execute only advanced anti-detection hooks (without memory cleaning)
        public static void ExecuteAdvancedAntiDetectionHooks()
        {
            InstallAdvancedAntiDetectionHooks();
            KillSuspiciousProcesses();
        }

        // Check if any interception was successful
        public static bool HasInterceptionSuccess()
        {
            return g_interceptionSuccess;
        }

        // Reset interception success flag
        public static void ResetInterceptionSuccess()
        {
            g_interceptionSuccess = false;
        }

        // Uninstall all hooks
        public static void UninstallAllHooks()
        {
            try
            {
                UninstallDeviceIoControlHook();
                UninstallAdvancedAntiDetectionHooks();
            }
            catch (Exception)
            {
                // Silent fail
            }
        }

        // Uninstall advanced anti-detection hooks
        public static void UninstallAdvancedAntiDetectionHooks()
        {
            try
            {
                if (!g_advancedHooksInstalled) return;

                // Uninstall CreateFileW hook
                UninstallHook(ref g_originalCreateFileW, ref g_originalCreateFileWBytes);

                // Uninstall ReadProcessMemory hook
                UninstallHook(ref g_originalReadProcessMemory, ref g_originalReadProcessMemoryBytes);

                // Uninstall NtQuerySystemInformation hook
                UninstallHook(ref g_originalNtQuerySystemInformation, ref g_originalNtQuerySystemInformationBytes);

                g_advancedHooksInstalled = false;
            }
            catch (Exception)
            {
                // Silent fail
            }
        }

        private static void UninstallHook(ref IntPtr originalAddress, ref byte[]? originalBytes)
        {
            try
            {
                if (originalAddress != IntPtr.Zero && originalBytes != null)
                {
                    uint oldProtect;
                    VirtualProtect(originalAddress, 5, PAGE_EXECUTE_READWRITE, out oldProtect);
                    
                    uint bytesWritten;
                    WriteProcessMemory(GetCurrentProcess(), originalAddress, originalBytes, 5, out bytesWritten);
                    
                    VirtualProtect(originalAddress, 5, oldProtect, out oldProtect);
                }
            }
            catch (Exception)
            {
                // Silent fail
            }
        }
    }
}