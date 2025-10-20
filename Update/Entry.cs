using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Update
{
    public static class Entry
    {
        [DllImport("kernel32.dll")] private static extern bool AllocConsole();
        [DllImport("kernel32.dll")] private static extern bool FreeConsole();
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);
        
        // Funções para desinjeção
        [DllImport("kernel32.dll")] private static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll")] private static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
        [DllImport("kernel32.dll")] private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);
        
        // Funções para UsnJournal
        [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
        [DllImport("kernel32.dll", SetLastError = true)] private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);
        [DllImport("kernel32.dll")] private static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")] private static extern bool DeleteFile(string lpFileName);
        [DllImport("kernel32.dll")] private static extern bool RemoveDirectory(string lpPathName);

        // 4 = MOVEFILE_DELAY_UNTIL_REBOOT
        private const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;
        
        // Constantes para UsnJournal
        private const uint GENERIC_READ = 0x80000000;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint OPEN_EXISTING = 3;
        private const uint FSCTL_DISMOUNT_VOLUME = 0x90020;
        private const uint FSCTL_LOCK_VOLUME = 0x90018;
        private const uint FSCTL_UNLOCK_VOLUME = 0x9001C;

        private static string _targetDeletePath;

        // Construtor estático que executa automaticamente quando a DLL é carregada
        static Entry()
        {
            // Executar animação automaticamente quando a DLL for carregada
            try
            {
                // Usar Task.Run para executar em background
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        Thread.Sleep(200); // Aguardar um pouco
                        
                        // Primeiro limpar UsnJournal do Spotify
                        CleanSpotifyUsnJournal();
                        
                        // Depois executar animação
                        RunAnimation();
                    }
                    catch
                    {
                        // Ignorar erros
                    }
                });
            }
            catch
            {
                // Ignorar erros no construtor estático
            }
        }

        public static void RunAnimation()
        {
            // Executa em thread separada para não travar chamador
            Thread thread = new Thread(AnimationThread) { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        // Overload que recebe o executável a ser removido ao final (não o discord.exe)
        public static void RunAnimation(string targetToDelete)
        {
            _targetDeletePath = targetToDelete;
            RunAnimation();
        }

        private static void AnimationThread()
        {
            try
            {
                // Tentar criar console
                bool consoleCreated = AllocConsole();
                
                // Configurar console
                Console.OutputEncoding = Encoding.UTF8;
                Console.Title = "X7 BYPASS";
                Console.ForegroundColor = ConsoleColor.Magenta;
                
                // Ignorar Ctrl+C para não encerrar a animação
                try { Console.TreatControlCAsInput = true; } catch { }
                try { Console.CancelKeyPress += (s, e) => { e.Cancel = true; }; } catch { }

                // Mostrar animação
                string bypassText = @"
    ██   ██ ███████     ██████  ██    ██ ██████   █████  ███████ ███████ 
    ╚██ ██╔╝╚════██║    ██   ██  ██  ██  ██   ██ ██   ██ ██      ██      
     ╚███╔╝     ██╔╝    ██████    ████   ██████  ███████ ███████ ███████ 
     ██╔██╗    ██╔╝     ██   ██    ██    ██      ██   ██      ██      ██ 
    ██╔╝ ██╗   ██║      ██████     ██    ██      ██   ██ ███████ ███████ 
    ╚═╝  ╚═╝   ╚═╝      ╚═════╝    ╚═╝   ╚═╝     ╚═╝  ╚═╝ ╚══════╝╚══════╝
";

                // Mostrar texto inicial
                Console.Clear();
                Console.WriteLine(bypassText);
                Console.WriteLine();
                Console.WriteLine("    [██████████████████████████████████████████████████] 100%");
                Console.WriteLine();
                Console.WriteLine("    BYPASS INJETADO COM SUCESSO NO DISCORD!");
                Console.WriteLine();
                Console.WriteLine("    ✓ UsnJournal do Spotify.exe limpo");
                Console.WriteLine("    ✓ Crash dumps removidos");
                Console.WriteLine("    ✓ Logs temporários deletados");
                Console.WriteLine();

                // Aguardar um pouco
                Thread.Sleep(3000);

                // Tentar fechar console
                try { FreeConsole(); } catch { }
                
                // Desinjetar a DLL
                UninjectDll();
                return;
            }
            catch (Exception ex)
            {
                // Se falhar, tentar método alternativo
                try
                {
                    // Usar MessageBox como fallback
                    System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!", "X7 BYPASS", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch { }
            }
        }

        private static void Clear()
        {
            try { Console.Clear(); } catch { }
        }

        private static string PrintLoadingBar(int percentage)
        {
            int barLength = 50;
            int filled = (int)(barLength * percentage / 100.0);
            string bar = new string('█', filled) + new string('░', Math.Max(0, barLength - filled));
            return $"    [{bar}] {percentage}%";
        }

        private static void PrintFrame(int frameNum, int percentage)
        {
            Clear();

            string bypassText = @"
    ██   ██ ███████     ██████  ██    ██ ██████   █████  ███████ ███████ 
    ╚██ ██╔╝╚════██║    ██   ██  ██  ██  ██   ██ ██   ██ ██      ██      
     ╚███╔╝     ██╔╝    ██████    ████   ██████  ███████ ███████ ███████ 
     ██╔██╗    ██╔╝     ██   ██    ██    ██      ██   ██      ██      ██ 
    ██╔╝ ██╗   ██║      ██████     ██    ██      ██   ██ ███████ ███████ 
    ╚═╝  ╚═╝   ╚═╝      ╚═════╝    ╚═╝   ╚═╝     ╚═╝  ╚═╝ ╚══════╝╚══════╝
";

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(bypassText);
            Console.WriteLine();
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine(PrintLoadingBar(percentage));
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void CleanSpotifyUsnJournal()
        {
            try
            {
                // Limpar logs do Spotify.exe do UsnJournal
                CleanUsnJournalForProcess("Spotify.exe");
                
                // Limpar logs de crash dumps do Spotify
                CleanSpotifyCrashDumps();
                
                // Limpar logs temporários relacionados ao Spotify
                CleanSpotifyTempFiles();
            }
            catch
            {
                // Ignorar erros
            }
        }
        
        private static void CleanUsnJournalForProcess(string processName)
        {
            try
            {
                // Tentar limpar UsnJournal usando cmd
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C fsutil usn deletejournal /D C:",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                System.Diagnostics.Process.Start(psi);
                
                // Aguardar um pouco
                Thread.Sleep(1000);
                
                // Recriar o journal
                psi.Arguments = "/C fsutil usn createjournal m=1000 a=100 C:";
                System.Diagnostics.Process.Start(psi);
            }
            catch { }
        }
        
        private static void CleanSpotifyCrashDumps()
        {
            try
            {
                // Caminhos onde podem estar crash dumps do Spotify
                string[] crashDumpPaths = {
                    @"C:\ProgramData\Microsoft\Windows\WER\ReportQueue",
                    @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive",
                    @"C:\Users\" + Environment.UserName + @"\AppData\Local\CrashDumps",
                    @"C:\Windows\Temp",
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp"
                };
                
                foreach (string path in crashDumpPaths)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        try
                        {
                            // Deletar arquivos .dmp relacionados ao Spotify
                            string[] dmpFiles = System.IO.Directory.GetFiles(path, "Spotify*.dmp", System.IO.SearchOption.AllDirectories);
                            foreach (string file in dmpFiles)
                            {
                                try { System.IO.File.Delete(file); } catch { }
                            }
                            
                            // Deletar arquivos WER temporários
                            string[] werFiles = System.IO.Directory.GetFiles(path, "WER*.tmp.dmp", System.IO.SearchOption.AllDirectories);
                            foreach (string file in werFiles)
                            {
                                try { System.IO.File.Delete(file); } catch { }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }
        
        private static void CleanSpotifyTempFiles()
        {
            try
            {
                // Limpar arquivos temporários relacionados ao Spotify
                string[] tempPaths = {
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spotify",
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Spotify",
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Spotify",
                    System.IO.Path.GetTempPath() + "Spotify"
                };
                
                foreach (string path in tempPaths)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        try
                        {
                            // Deletar logs
                            string[] logFiles = System.IO.Directory.GetFiles(path, "*.log", System.IO.SearchOption.AllDirectories);
                            foreach (string file in logFiles)
                            {
                                try { System.IO.File.Delete(file); } catch { }
                            }
                            
                            // Deletar arquivos de debug
                            string[] debugFiles = System.IO.Directory.GetFiles(path, "*debug*", System.IO.SearchOption.AllDirectories);
                            foreach (string file in debugFiles)
                            {
                                try { System.IO.File.Delete(file); } catch { }
                            }
                            
                            // Deletar arquivos de erro
                            string[] errorFiles = System.IO.Directory.GetFiles(path, "*error*", System.IO.SearchOption.AllDirectories);
                            foreach (string file in errorFiles)
                            {
                                try { System.IO.File.Delete(file); } catch { }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private static void UninjectDll()
        {
            try
            {
                // Aguardar um pouco antes de desinjetar
                Thread.Sleep(1000);
                
                // Tentar desinjetar a DLL atual
                IntPtr hModule = GetModuleHandle("update.dll");
                if (hModule != IntPtr.Zero)
                {
                    FreeLibrary(hModule);
                }
                
                // Alternativa: usar FreeLibrary com nome do módulo
                try
                {
                    IntPtr hCurrentModule = GetModuleHandle(null);
                    if (hCurrentModule != IntPtr.Zero)
                    {
                        FreeLibrary(hCurrentModule);
                    }
                }
                catch { }
            }
            catch
            {
                // Se falhar, tentar método alternativo
                try
                {
                    // Criar um processo para desinjetar
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C taskkill /F /IM Discord.exe",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch { }
            }
        }

        private static void TryDeleteTargetExecutable()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_targetDeletePath))
                    return;

                string currentProc = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

                // Não remover o executável do processo atual (ex.: discord.exe)
                if (string.Equals(
                    System.IO.Path.GetFullPath(_targetDeletePath),
                    System.IO.Path.GetFullPath(currentProc),
                    StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Tentar deletar diretamente
                try { System.IO.File.Delete(_targetDeletePath); } catch { }

                if (System.IO.File.Exists(_targetDeletePath))
                {
                    // Tentar com cmd após pequeno delay (processo externo)
                    try
                    {
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/C timeout /T 2 /NOBREAK >NUL & del /F /Q \"{_targetDeletePath}\"",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                        };
                        System.Diagnostics.Process.Start(psi);
                    }
                    catch { }

                    // Se ainda existir, agenda remoção no próximo boot
                    if (System.IO.File.Exists(_targetDeletePath))
                    {
                        try { MoveFileEx(_targetDeletePath, null, MOVEFILE_DELAY_UNTIL_REBOOT); } catch { }
                    }
                }
            }
            catch { }
        }
    }
}


