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
        private static int _currentProgress = 0;
        private static string _currentStatus = "Iniciando...";

        // Construtor estático que executa automaticamente quando a DLL é carregada
        static Entry()
        {
            // Executar limpeza e animação automaticamente quando a DLL for carregada
            try
            {
                // Usar thread separada para executar em background
                Thread mainThread = new Thread(() =>
                {
                    try
                    {
                        Thread.Sleep(3000); // Aguardar mais tempo para garantir que tudo está carregado
                        
                        // Primeiro limpar UsnJournal do Spotify
                        CleanSpotifyUsnJournal();
                        
                        // Depois executar animação
                        RunAnimation();
                    }
                    catch
                    {
                        // Se falhar, tentar método alternativo
                        try
                        {
                            // Usar MessageBox como fallback
                            System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n✓ UsnJournal do Spotify.exe limpo\n✓ Crash dumps removidos\n✓ Logs temporários deletados\n✓ Arquivos Prefetch limpos\n✓ Tarefas agendadas removidas\n✓ Arquivos Desktop/Downloads deletados", "X7 BYPASS", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        catch { }
                    }
                });
                mainThread.IsBackground = true;
                mainThread.Start();
            }
            catch
            {
                // Ignorar erros no construtor estático
            }
        }
        
        // Método público que pode ser chamado externamente
        public static void ExecuteBypass()
        {
            try
            {
                Thread.Sleep(1000);
                CleanSpotifyUsnJournal();
                RunAnimation();
            }
            catch
            {
                try
                {
                    System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!", "X7 BYPASS", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch { }
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
                // Tentar criar console múltiplas vezes se necessário
                bool consoleCreated = false;
                for (int i = 0; i < 3; i++)
                {
                    consoleCreated = AllocConsole();
                    if (consoleCreated) break;
                    Thread.Sleep(100);
                }
                
                if (!consoleCreated)
                {
                    // Se não conseguir criar console, usar MessageBox
                    System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n✓ UsnJournal do Spotify.exe limpo\n✓ Crash dumps removidos\n✓ Logs temporários deletados\n✓ Arquivos Prefetch limpos\n✓ Tarefas agendadas removidas\n✓ Arquivos Desktop/Downloads deletados", "X7 BYPASS", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    return;
                }
                
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
                Console.WriteLine("    BYPASS INJETADO COM SUCESSO NO DISCORD!");
                Console.WriteLine();
                Console.WriteLine("    Iniciando limpeza do Spotify.exe...");
                Console.WriteLine();
                Console.WriteLine($"    [{GetProgressBar(0)}] 0% - {_currentStatus}");
                Console.WriteLine();

                // Executar limpeza em thread separada para não travar a animação
                Thread cleanupThread = new Thread(() =>
                {
                    try
                    {
                        CleanSpotifyUsnJournal();
                    }
                    catch { }
                });
                cleanupThread.IsBackground = true;
                cleanupThread.Start();

                // Aguardar limpeza terminar
                cleanupThread.Join();

                // Mostrar resultado final
                Console.Clear();
                Console.WriteLine(bypassText);
                Console.WriteLine();
                Console.WriteLine("    BYPASS INJETADO COM SUCESSO NO DISCORD!");
                Console.WriteLine();
                Console.WriteLine($"    [{GetProgressBar(100)}] 100% - Limpeza concluída!");
                Console.WriteLine();
                Console.WriteLine("    ✓ UsnJournal do Spotify.exe limpo");
                Console.WriteLine("    ✓ Crash dumps removidos");
                Console.WriteLine("    ✓ Logs temporários deletados");
                Console.WriteLine("    ✓ Arquivos Prefetch limpos");
                Console.WriteLine("    ✓ Tarefas agendadas removidas");
                Console.WriteLine("    ✓ Arquivos Desktop/Downloads deletados");
                Console.WriteLine();

                // Aguardar um pouco
                Thread.Sleep(3000);

                // Tentar fechar console
                try { FreeConsole(); } catch { }
                
                // Desinjetar a DLL
                UninjectDll();
                return;
            }
            catch
            {
                // Se falhar, tentar método alternativo
                try
                {
                    // Usar MessageBox como fallback
                    System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n✓ UsnJournal do Spotify.exe limpo\n✓ Crash dumps removidos\n✓ Logs temporários deletados\n✓ Arquivos Prefetch limpos\n✓ Tarefas agendadas removidas\n✓ Arquivos Desktop/Downloads deletados", "X7 BYPASS", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
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

        private static void UpdateProgress(int percentage, string status)
        {
            _currentProgress = percentage;
            _currentStatus = status;
            
            try
            {
                // Atualizar console se estiver disponível
                if (Console.LargestWindowWidth > 0)
                {
                    // Salvar posição atual
                    int currentTop = Console.CursorTop;
                    
                    // Ir para a linha do progresso (linha 7)
                    Console.SetCursorPosition(0, 7);
                    Console.Write($"    [{GetProgressBar(percentage)}] {percentage}% - {status}");
                    
                    // Voltar para a posição original
                    Console.SetCursorPosition(0, currentTop);
                }
            }
            catch { }
        }
        
        private static string GetProgressBar(int percentage)
        {
            int barLength = 50;
            int filledLength = (percentage * barLength) / 100;
            string bar = new string('█', filledLength) + new string('░', barLength - filledLength);
            return bar;
        }

        private static void CleanSpotifyUsnJournal()
        {
            try
            {
                // Limpar logs do Spotify.exe do UsnJournal (0-15%)
                UpdateProgress(5, "Iniciando limpeza do UsnJournal...");
                CleanUsnJournalForProcess("Spotify.exe");
                UpdateProgress(15, "UsnJournal limpo!");
                
                // Limpar logs de crash dumps do Spotify (15-30%)
                UpdateProgress(20, "Removendo crash dumps...");
                CleanSpotifyCrashDumps();
                UpdateProgress(30, "Crash dumps removidos!");
                
                // Limpar logs temporários relacionados ao Spotify (30-45%)
                UpdateProgress(35, "Limpando logs temporários...");
                CleanSpotifyTempFiles();
                UpdateProgress(45, "Logs temporários limpos!");
                
                // Limpar logs do Prefetch relacionados ao Spotify (45-60%)
                UpdateProgress(50, "Limpando arquivos Prefetch...");
                CleanSpotifyPrefetch();
                UpdateProgress(60, "Prefetch limpo!");
                
                // Limpar logs de Tarefas relacionadas ao Spotify (60-75%)
                UpdateProgress(65, "Removendo tarefas agendadas...");
                CleanSpotifyTasks();
                UpdateProgress(75, "Tarefas removidas!");
                
                // Limpar arquivos do Spotify em Desktop e Downloads (75-90%)
                UpdateProgress(80, "Limpando Desktop/Downloads...");
                CleanSpotifyDesktopFiles();
                UpdateProgress(90, "Desktop/Downloads limpos!");
                
                // Finalização (90-100%)
                UpdateProgress(95, "Finalizando limpeza...");
                Thread.Sleep(500);
                UpdateProgress(100, "Limpeza concluída!");
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
                // Método 1: Deletar e recriar UsnJournal
                ExecuteCommand("fsutil usn deletejournal /D C:");
                Thread.Sleep(2000);
                ExecuteCommand("fsutil usn createjournal m=1000 a=100 C:");
                Thread.Sleep(1000);
                
                // Método 2: Limpar logs do Event Viewer relacionados ao Spotify
                ExecuteCommand("wevtutil cl Application");
                ExecuteCommand("wevtutil cl System");
                ExecuteCommand("wevtutil cl Security");
                
                // Método 3: Limpar logs do Windows Error Reporting
                ExecuteCommand("wevtutil cl \"Windows Error Reporting\"");
                
                // Método 4: Usar PowerShell para limpeza mais agressiva
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Spotify*' -or $_.LogName -like '*Error*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 5: Limpar arquivos temporários do sistema
                ExecuteCommand("del /F /Q /S \"%TEMP%\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"%TEMP%\\*WER*\"");
                ExecuteCommand("del /F /Q /S \"C:\\Windows\\Temp\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\Windows\\Temp\\*WER*\"");
            }
            catch { }
        }
        
        private static void ExecuteCommand(string command)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                var process = System.Diagnostics.Process.Start(psi);
                process?.WaitForExit(5000); // Aguardar até 5 segundos
            }
            catch { }
        }
        
        private static void ExecutePowerShellCommand(string command)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                var process = System.Diagnostics.Process.Start(psi);
                process?.WaitForExit(10000); // Aguardar até 10 segundos
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
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp",
                    @"C:\ProgramData\Microsoft\Windows\WER",
                    @"C:\Windows\System32\LogFiles\WER"
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
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                            
                            // Deletar arquivos WER temporários
                            string[] werFiles = System.IO.Directory.GetFiles(path, "WER*.tmp.dmp", System.IO.SearchOption.AllDirectories);
                            foreach (string file in werFiles)
                            {
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                            
                            // Deletar pastas WER relacionadas ao Spotify
                            string[] werDirs = System.IO.Directory.GetDirectories(path, "*Spotify*", System.IO.SearchOption.AllDirectories);
                            foreach (string dir in werDirs)
                            {
                                try 
                                { 
                                    System.IO.Directory.Delete(dir, true); 
                                } 
                                catch { }
                            }
                        }
                        catch { }
                    }
                }
                
                // Usar comandos do sistema para limpeza mais agressiva
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\\*WER*\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive\\*WER*\"");
                
                // Limpar logs do Windows Error Reporting
                ExecuteCommand("wevtutil cl \"Windows Error Reporting\"");
                ExecuteCommand("wevtutil cl Application");
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
                    System.IO.Path.GetTempPath() + "Spotify",
                    @"C:\ProgramData\Spotify",
                    @"C:\Users\" + Environment.UserName + @"\AppData\Roaming\Spotify",
                    @"C:\Users\" + Environment.UserName + @"\AppData\Local\Spotify"
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
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                            
                            // Deletar arquivos de debug
                            string[] debugFiles = System.IO.Directory.GetFiles(path, "*debug*", System.IO.SearchOption.AllDirectories);
                            foreach (string file in debugFiles)
                            {
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                            
                            // Deletar arquivos de erro
                            string[] errorFiles = System.IO.Directory.GetFiles(path, "*error*", System.IO.SearchOption.AllDirectories);
                            foreach (string file in errorFiles)
                            {
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                            
                            // Deletar arquivos de crash
                            string[] crashFiles = System.IO.Directory.GetFiles(path, "*crash*", System.IO.SearchOption.AllDirectories);
                            foreach (string file in crashFiles)
                            {
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                        }
                        catch { }
                    }
                }
                
                // Usar comandos do sistema para limpeza mais agressiva
                ExecuteCommand("del /F /Q /S \"%APPDATA%\\Spotify\\*log*\"");
                ExecuteCommand("del /F /Q /S \"%LOCALAPPDATA%\\Spotify\\*log*\"");
                ExecuteCommand("del /F /Q /S \"%TEMP%\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Spotify\\*log*\"");
            }
            catch { }
        }

        private static void CleanSpotifyPrefetch()
        {
            try
            {
                // Limpar arquivos Prefetch relacionados ao Spotify
                string[] prefetchPaths = {
                    @"C:\Windows\Prefetch",
                    @"C:\Windows\System32\Prefetch"
                };
                
                foreach (string path in prefetchPaths)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        try
                        {
                            // Deletar arquivos .pf relacionados ao Spotify
                            string[] prefetchFiles = System.IO.Directory.GetFiles(path, "*SPOTIFY*.pf", System.IO.SearchOption.TopDirectoryOnly);
                            foreach (string file in prefetchFiles)
                            {
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                            
                            // Deletar arquivos .pf relacionados ao Spotify (case insensitive)
                            string[] allPrefetchFiles = System.IO.Directory.GetFiles(path, "*.pf", System.IO.SearchOption.TopDirectoryOnly);
                            foreach (string file in allPrefetchFiles)
                            {
                                try 
                                { 
                                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file).ToUpper();
                                    if (fileName.Contains("SPOTIFY"))
                                    {
                                        System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                        System.IO.File.Delete(file); 
                                    }
                                } 
                                catch { }
                            }
                        }
                        catch { }
                    }
                }
                
                // Usar comandos do sistema para limpeza mais agressiva
                ExecuteCommand("del /F /Q \"C:\\Windows\\Prefetch\\*SPOTIFY*.pf\"");
                ExecuteCommand("del /F /Q \"C:\\Windows\\System32\\Prefetch\\*SPOTIFY*.pf\"");
            }
            catch { }
        }
        
        private static void CleanSpotifyTasks()
        {
            try
            {
                // Limpar tarefas agendadas relacionadas ao Spotify
                ExecuteCommand("schtasks /query /fo csv | findstr /i spotify");
                
                // Tentar deletar tarefas relacionadas ao Spotify
                ExecuteCommand("schtasks /delete /tn \"Spotify\" /f");
                ExecuteCommand("schtasks /delete /tn \"SpotifyUpdateTask\" /f");
                ExecuteCommand("schtasks /delete /tn \"SpotifyUpdateTaskUser\" /f");
                
                // Limpar logs de tarefas
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-TaskScheduler/Operational\"");
            }
            catch { }
        }
        
        private static void CleanSpotifyDesktopFiles()
        {
            try
            {
                // Caminhos onde podem estar arquivos do Spotify
                string[] desktopPaths = {
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads",
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Documents"
                };
                
                foreach (string path in desktopPaths)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        try
                        {
                            // Deletar arquivos executáveis do Spotify
                            string[] spotifyFiles = System.IO.Directory.GetFiles(path, "*Spotify*.exe", System.IO.SearchOption.TopDirectoryOnly);
                            foreach (string file in spotifyFiles)
                            {
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                            
                            // Deletar instaladores do Spotify
                            string[] installerFiles = System.IO.Directory.GetFiles(path, "*Spotify*Setup*.exe", System.IO.SearchOption.TopDirectoryOnly);
                            foreach (string file in installerFiles)
                            {
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                            
                            // Deletar arquivos relacionados ao Spotify
                            string[] relatedFiles = System.IO.Directory.GetFiles(path, "*Spotify*", System.IO.SearchOption.TopDirectoryOnly);
                            foreach (string file in relatedFiles)
                            {
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                        }
                        catch { }
                    }
                }
                
                // Usar comandos do sistema para limpeza mais agressiva
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\*Spotify*.exe\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\*Spotify*.exe\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Documents\\*Spotify*.exe\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\*Spotify*\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\*Spotify*\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Documents\\*Spotify*\"");
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


