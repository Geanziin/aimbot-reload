using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public static class DllInjector
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        private const uint PROCESS_CREATE_THREAD = 2;
        private const uint PROCESS_QUERY_INFORMATION = 1024;
        private const uint PROCESS_VM_OPERATION = 8;
        private const uint PROCESS_VM_WRITE = 32;
        private const uint PROCESS_VM_READ = 16;
        private const uint MEM_COMMIT = 4096;
        private const uint MEM_RESERVE = 8192;
        private const uint PAGE_READWRITE = 4;

        public static bool InjectDll(string processName, string dllPath)
        {
            IntPtr processHandle = IntPtr.Zero;
            IntPtr allocatedMemory = IntPtr.Zero;
            IntPtr remoteThread = IntPtr.Zero;

            try
            {
                Process process = Process.GetProcessesByName(processName)[0];
                if (process == null || process.HasExited)
                {
                    return false;
                }

                uint processAccess = PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ;
                processHandle = OpenProcess(processAccess, false, process.Id);

                if (processHandle == IntPtr.Zero)
                {
                    return false;
                }

                IntPtr moduleHandle = GetModuleHandle("kernel32.dll");
                if (moduleHandle == IntPtr.Zero)
                {
                    return false;
                }

                IntPtr procAddress = GetProcAddress(moduleHandle, "LoadLibraryA");
                if (procAddress == IntPtr.Zero)
                {
                    return false;
                }

                if (!File.Exists(dllPath))
                {
                    return false;
                }

                byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath);
                allocatedMemory = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)(dllPathBytes.Length + 1), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                if (allocatedMemory == IntPtr.Zero)
                {
                    return false;
                }

                if (!WriteProcessMemory(processHandle, allocatedMemory, dllPathBytes, (uint)dllPathBytes.Length, out int bytesWritten))
                {
                    return false;
                }

                remoteThread = CreateRemoteThread(processHandle, IntPtr.Zero, 0U, procAddress, allocatedMemory, 0U, IntPtr.Zero);

                if (remoteThread == IntPtr.Zero)
                {
                    return false;
                }

                // Aumentado para 30 segundos (30000ms) ou usar 0xFFFFFFFF para INFINITE
                uint waitResult = WaitForSingleObject(remoteThread, 30000U);

                return waitResult == 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (remoteThread != IntPtr.Zero)
                    CloseHandle(remoteThread);
                if (allocatedMemory != IntPtr.Zero)
                    CloseHandle(allocatedMemory);
                if (processHandle != IntPtr.Zero)
                    CloseHandle(processHandle);
            }
        }

        public static void DownloadDll(string url, string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                client.DownloadFile(url, path);
            }
        }

        public static Process GetProcessByName(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0 ? processes[0] : null!;
        }
    }
}
