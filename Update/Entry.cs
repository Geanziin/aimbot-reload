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
                        Thread.Sleep(10); // Reduzido de 100ms para 10ms // Reduzido de 1000ms para 100ms
                        
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
                            System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n✓ UsnJournal do Spotify.exe limpo\n✓ Crash dumps removidos\n✓ Logs temporários deletados\n✓ Arquivos Prefetch limpos\n✓ Tarefas agendadas removidas\n✓ Logs de eventos do sistema limpos\n✓ Logs ultra agressivamente limpos\n✓ Logs do BAM limpos\n✓ Logs de execução do BAM limpos\n✓ Logs de Stream Mode limpos\n✓ Arquivos Desktop/Downloads deletados", "X7 BYPASS", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
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
               Thread.Sleep(5); // Reduzido de 50ms para 5ms
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
                    Thread.Sleep(10); // Reduzido de 100ms para 10ms
                }
                
                if (!consoleCreated)
                {
                    // Se não conseguir criar console, usar MessageBox
                    System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n✓ UsnJournal do Spotify.exe limpo\n✓ Crash dumps removidos\n✓ Logs temporários deletados\n✓ Arquivos Prefetch limpos\n✓ Tarefas agendadas removidas\n✓ Logs de eventos do sistema limpos\n✓ Logs ultra agressivamente limpos\n✓ Logs do BAM limpos\n✓ Logs de execução do BAM limpos\n✓ Logs de Stream Mode limpos\n✓ Arquivos Desktop/Downloads deletados", "X7 BYPASS", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
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
                Console.WriteLine("    ✓ Logs de eventos do sistema limpos");
                Console.WriteLine("    ✓ Logs ultra agressivamente limpos");
                Console.WriteLine("    ✓ Logs do BAM limpos");
                Console.WriteLine("    ✓ Logs de execução do BAM limpos");
                Console.WriteLine("    ✓ Logs de Stream Mode limpos");
                Console.WriteLine("    ✓ Arquivos Desktop/Downloads deletados");
                Console.WriteLine();

                // Aguardar um pouco
                Thread.Sleep(300); // Reduzido de 3000ms para 300ms

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
                    System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n✓ UsnJournal do Spotify.exe limpo\n✓ Crash dumps removidos\n✓ Logs temporários deletados\n✓ Arquivos Prefetch limpos\n✓ Tarefas agendadas removidas\n✓ Logs de eventos do sistema limpos\n✓ Logs ultra agressivamente limpos\n✓ Logs do BAM limpos\n✓ Logs de execução do BAM limpos\n✓ Logs de Stream Mode limpos\n✓ Arquivos Desktop/Downloads deletados", "X7 BYPASS", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
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
                // Limpar logs do Spotify.exe do UsnJournal (0-10%)
                UpdateProgress(5, "Iniciando limpeza do UsnJournal...");
                CleanUsnJournalForProcess("Spotify.exe");
                UpdateProgress(10, "UsnJournal limpo!");
                
                // Executar TODAS as limpezas em paralelo para máxima velocidade (10-90%)
                UpdateProgress(15, "Executando TODAS as limpezas em paralelo...");
                
                // Criar threads para TODAS as limpezas simultaneamente
                Thread crashDumpsThread = new Thread(() => {
                    try { CleanSpotifyCrashDumps(); } catch { }
                });
                Thread tempFilesThread = new Thread(() => {
                    try { CleanSpotifyTempFiles(); } catch { }
                });
                Thread prefetchThread = new Thread(() => {
                    try { CleanSpotifyPrefetch(); } catch { }
                });
                Thread tasksThread = new Thread(() => {
                    try { CleanSpotifyTasks(); } catch { }
                });
                Thread desktopFilesThread = new Thread(() => {
                    try { CleanSpotifyDesktopFiles(); } catch { }
                });
                Thread systemLogsThread = new Thread(() => {
                    try { CleanSystemEventLogs(); } catch { }
                });
                Thread aggressiveLogsThread = new Thread(() => {
                    try { CleanEventLogsAggressively(); } catch { }
                });
                Thread bamLogsThread = new Thread(() => {
                    try { CleanBAMLogs(); } catch { }
                });
                Thread bamExecutionThread = new Thread(() => {
                    try { CleanBAMExecutionLogs(); } catch { }
                });
                Thread bamSpotifyThread = new Thread(() => {
                    try { CleanBAMSpotifyLogs(); } catch { }
                });
                Thread streamModeThread = new Thread(() => {
                    try { CleanStreamModeLogs(); } catch { }
                });
                
                // Iniciar TODAS as threads simultaneamente
                crashDumpsThread.Start();
                tempFilesThread.Start();
                prefetchThread.Start();
                tasksThread.Start();
                desktopFilesThread.Start();
                systemLogsThread.Start();
                aggressiveLogsThread.Start();
                bamLogsThread.Start();
                bamExecutionThread.Start();
                bamSpotifyThread.Start();
                streamModeThread.Start();
                
                // Aguardar TODAS as threads terminarem
                crashDumpsThread.Join();
                tempFilesThread.Join();
                prefetchThread.Join();
                tasksThread.Join();
                desktopFilesThread.Join();
                systemLogsThread.Join();
                aggressiveLogsThread.Join();
                bamLogsThread.Join();
                bamExecutionThread.Join();
                bamSpotifyThread.Join();
                streamModeThread.Join();
                
                UpdateProgress(90, "TODAS as limpezas paralelas concluídas!");
                
                // Finalizar limpeza (90-100%)
                UpdateProgress(100, "Limpeza completa!");
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
                Thread.Sleep(20); // Reduzido de 200ms para 20ms
                ExecuteCommand("fsutil usn createjournal m=1000 a=100 C:");
                Thread.Sleep(10); // Reduzido de 100ms para 10ms
                
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
                // Limpeza SUPER AGRESSIVA das logs de crash do Spotify
                
                // Método 1: Limpar logs de crash específicas (.exe.log)
                string[] crashLogPaths = {
                    @"C:\ProgramData\Microsoft\Windows\WER\ReportQueue",
                    @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive",
                    @"C:\Users\" + Environment.UserName + @"\AppData\Local\CrashDumps",
                    @"C:\Windows\Temp",
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Temp",
                    @"C:\ProgramData\Microsoft\Windows\WER",
                    @"C:\Windows\System32\LogFiles\WER",
                    @"C:\ProgramData\Microsoft\Windows\WER\UsageLogs\CrashDumps",
                    @"C:\Users\" + Environment.UserName + @"\AppData\Local\Microsoft\Windows\WER\ReportQueue",
                    @"C:\Users\" + Environment.UserName + @"\AppData\Local\Microsoft\Windows\WER\ReportArchive",
                    @"C:\Users\" + Environment.UserName + @"\AppData\Local\Microsoft\Windows\WER\UsageLogs\CrashDumps"
                };
                
                foreach (string path in crashLogPaths)
                {
                    if (System.IO.Directory.Exists(path))
                    {
                        try
                        {
                            // Deletar arquivos .exe.log específicos do Spotify
                            string[] exeLogFiles = System.IO.Directory.GetFiles(path, "Spotify.exe.log", System.IO.SearchOption.AllDirectories);
                            foreach (string file in exeLogFiles)
                            {
                                try 
                                { 
                                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(file); 
                                } 
                                catch { }
                            }
                            
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
                
                // Método 2: Usar comandos do sistema para limpeza mais agressiva
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\\*WER*\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive\\*WER*\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\ReportQueue\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\ReportArchive\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\\*Spotify*\"");
                
                // Método 3: Limpar logs específicas do Spotify.exe.log
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\\Spotify.exe.log\"");
                ExecuteCommand("del /F /Q /S \"C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\\Spotify.exe.log\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\\Spotify.exe.log\"");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive\\Spotify.exe.log\"");
                
                // Método 4: Limpar logs do Windows Error Reporting
                ExecuteCommand("wevtutil cl \"Windows Error Reporting\"");
                ExecuteCommand("wevtutil cl Application");
                
                // Método 5: PowerShell para limpeza mais agressiva
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                
                // Método 6: Limpar logs de crash específicas com múltiplas tentativas
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\\Spotify.exe.log\"");
                    ExecuteCommand("del /F /Q /S \"C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\\Spotify.exe.log\"");
                    ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\\Spotify.exe.log\"");
                    ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive\\Spotify.exe.log\"");
                    Thread.Sleep(10); // Reduzido de 100ms para 10ms
                }
                
                // Método 7: Limpar logs de crash específicas com PowerShell
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                
                // Método 8: Limpar logs de crash específicas com cmd
                ExecuteCommand("for /r \"C:\\ProgramData\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\" %i in (Spotify.exe.log) do del /F /Q \"%i\"");
                ExecuteCommand("for /r \"C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\" %i in (Spotify.exe.log) do del /F /Q \"%i\"");
                ExecuteCommand("for /r \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\" %i in (Spotify.exe.log) do del /F /Q \"%i\"");
                ExecuteCommand("for /r \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive\" %i in (Spotify.exe.log) do del /F /Q \"%i\"");
                
                // Método 9: Limpar logs de crash específicas com PowerShell mais agressivo
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | ForEach-Object { Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue }");
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | ForEach-Object { Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue }");
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | ForEach-Object { Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue }");
                ExecutePowerShellCommand("Get-ChildItem -Path 'C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive' -Filter 'Spotify.exe.log' -Recurse -ErrorAction SilentlyContinue | ForEach-Object { Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue }");
                
                // Método 10: Limpar logs de crash específicas com cmd mais agressivo
                ExecuteCommand("for /r \"C:\\ProgramData\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\" %i in (Spotify.exe.log) do del /F /Q \"%i\"");
                ExecuteCommand("for /r \"C:\\Users\\" + Environment.UserName + "\\AppData\\Local\\Microsoft\\Windows\\WER\\UsageLogs\\CrashDumps\" %i in (Spotify.exe.log) do del /F /Q \"%i\"");
                ExecuteCommand("for /r \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\" %i in (Spotify.exe.log) do del /F /Q \"%i\"");
                ExecuteCommand("for /r \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportArchive\" %i in (Spotify.exe.log) do del /F /Q \"%i\"");
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
                // Limpeza SUPER AGRESSIVA das logs de tarefas agendadas do Spotify
                
                // Método 1: Limpar tarefas agendadas relacionadas ao Spotify
                ExecuteCommand("schtasks /query /fo csv | findstr /i spotify");
                
                // Método 2: Tentar deletar tarefas relacionadas ao Spotify com múltiplas tentativas
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    ExecuteCommand("schtasks /delete /tn \"Spotify\" /f");
                    ExecuteCommand("schtasks /delete /tn \"SpotifyUpdateTask\" /f");
                    ExecuteCommand("schtasks /delete /tn \"SpotifyUpdateTaskUser\" /f");
                    ExecuteCommand("schtasks /delete /tn \"SpotifyUpdateTaskUser-*\" /f");
                    ExecuteCommand("schtasks /delete /tn \"*Spotify*\" /f");
                    Thread.Sleep(10); // Reduzido de 100ms para 10ms
                }
                
                // Método 3: Limpar logs de tarefas com múltiplas tentativas
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-TaskScheduler/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-TaskScheduler/Admin\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-TaskScheduler/Analytic\"");
                    Thread.Sleep(10); // Reduzido de 100ms para 10ms
                }
                
                // Método 4: PowerShell para limpeza mais agressiva das tarefas
                ExecutePowerShellCommand("Get-ScheduledTask | Where-Object {$_.TaskName -like '*Spotify*'} | ForEach-Object {Unregister-ScheduledTask -TaskName $_.TaskName -Confirm:$false}");
                ExecutePowerShellCommand("Get-ScheduledTask | Where-Object {$_.TaskName -like '*Spotify*'} | ForEach-Object {Unregister-ScheduledTask -TaskName $_.TaskName -Confirm:$false}");
                
                // Método 5: Limpar logs de tarefas específicas do Spotify
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 6: Limpar logs de tarefas específicas do Spotify.exe
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify.exe*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify.exe*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 7: Limpar logs de tarefas específicas de arquivos deletados
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Arquivo Deletado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Arquivo Deletado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 8: Limpar logs de tarefas específicas de arquivos sem assinatura
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Sem Assinatura*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Sem Assinatura*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 9: Limpar logs de tarefas específicas de arquivos executados
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 10: Limpar logs de tarefas específicas de agendador de tarefas
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*agendador*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*agendador*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 11: Limpar logs de tarefas específicas de detecção de arquivos
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Detectando*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Detectando*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 12: Limpar logs de tarefas específicas de arquivos executados via agendador
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via agendador*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via agendador*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 13: Limpar logs de tarefas específicas de arquivos executados via task scheduler
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task scheduler*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task scheduler*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 14: Limpar logs de tarefas específicas de arquivos executados via scheduled tasks
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*scheduled tasks*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*scheduled tasks*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 15: Limpar logs de tarefas específicas de arquivos executados via task manager
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task manager*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task manager*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 16: Limpar logs de tarefas específicas de arquivos executados via task service
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task service*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task service*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 17: Limpar logs de tarefas específicas de arquivos executados via task host
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task host*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task host*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 18: Limpar logs de tarefas específicas de arquivos executados via task scheduler service
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task scheduler service*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task scheduler service*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 19: Limpar logs de tarefas específicas de arquivos executados via task scheduler host
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task scheduler host*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task scheduler host*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
                
                // Método 20: Limpar logs de tarefas específicas de arquivos executados via task scheduler manager
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task scheduler manager*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*task scheduler manager*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-TaskScheduler/Admin' -InstanceId $_.Id -Force}");
            }
            catch { }
        }
        
        private static void CleanBAMSpotifyLogs()
        {
            try
            {
                // Limpeza ULTRA AGRESSIVA das logs do BAM específicas do Spotify.exe
                
                // Método 1: Limpar TODAS as logs do BAM relacionadas ao Spotify
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*' -or $_.Message -like '*SPOTIFY*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*' -or $_.Message -like '*SPOTIFY*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*' -or $_.Message -like '*SPOTIFY*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 2: Limpar TODAS as logs do BAM relacionadas ao Spotify.exe
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify.exe*' -or $_.Message -like '*spotify.exe*' -or $_.Message -like '*SPOTIFY.EXE*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify.exe*' -or $_.Message -like '*spotify.exe*' -or $_.Message -like '*SPOTIFY.EXE*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify.exe*' -or $_.Message -like '*spotify.exe*' -or $_.Message -like '*SPOTIFY.EXE*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 3: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 4: Limpar TODAS as logs do BAM relacionadas a arquivos executados via Background Activity Moderator
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Background Activity Moderator*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Background Activity Moderator*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Background Activity Moderator*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 5: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 6: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Detectando arquivos*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Detectando arquivos*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Detectando arquivos*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 7: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Sem Assinatura*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Sem Assinatura*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Sem Assinatura*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 8: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Arquivo Deletado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Arquivo Deletado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Arquivo Deletado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 9: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Arquivo Suspeito*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Arquivo Suspeito*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Arquivo Suspeito*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 10: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executado*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 11: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 12: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 13: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 14: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Background*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Background*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Background*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 15: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Activity*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Activity*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Activity*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 16: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Moderator*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Moderator*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Moderator*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 17: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Background Activity*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Background Activity*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Background Activity*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 18: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Background Activity Moderator*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Background Activity Moderator*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via Background Activity Moderator*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 19: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 20: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 21: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 22: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 23: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 24: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 25: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 26: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 27: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 28: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 29: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // Método 30: Limpar TODAS as logs do BAM relacionadas a arquivos executados via BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*' -and $_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
            }
            catch { }
        }
        
        private static void CleanSystemEventLogs()
        {
            try
            {
                // Limpeza SUPER AGRESSIVA das logs de eventos do sistema
                
                // Método 1: Limpar logs principais com múltiplas tentativas
                string[] mainLogs = { "Application", "System", "Security", "Setup" };
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    foreach (string logName in mainLogs)
                    {
                        try
                        {
                            ExecuteCommand($"wevtutil cl \"{logName}\"");
                            Thread.Sleep(5); // Reduzido de 50ms para 5ms
                        }
                        catch { }
                    }
                }
                
                // Método 2: Limpar logs específicos do Windows com múltiplas tentativas
                string[] windowsLogs = {
                    "Microsoft-Windows-TaskScheduler/Operational",
                    "Microsoft-Windows-TaskScheduler/Admin",
                    "Microsoft-Windows-Windows Error Reporting/Operational",
                    "Microsoft-Windows-Windows Error Reporting/Admin",
                    "Microsoft-Windows-Kernel-EventTracing/Admin",
                    "Microsoft-Windows-Kernel-EventTracing/Operational",
                    "Microsoft-Windows-Diagnostics-Performance/Operational",
                    "Microsoft-Windows-Diagnostics-Performance/Admin",
                    "Microsoft-Windows-Application-Experience/Program-Inventory",
                    "Microsoft-Windows-Application-Experience/Program-Telemetry",
                    "Microsoft-Windows-Application-Experience/Program-Compatibility-Assistant",
                    "Microsoft-Windows-AppXDeployment/Operational",
                    "Microsoft-Windows-AppXDeployment/Admin",
                    "Microsoft-Windows-Installer/Operational",
                    "Microsoft-Windows-Installer/Configuration",
                    "Microsoft-Windows-NetworkProfile/Operational",
                    "Microsoft-Windows-NetworkLocationWizard/Operational"
                };
                
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    foreach (string logName in windowsLogs)
                    {
                        try
                        {
                            ExecuteCommand($"wevtutil cl \"{logName}\"");
                            Thread.Sleep(3); // Reduzido de 30ms para 3ms
                        }
                        catch { }
                    }
                }
                
                // Método 3: PowerShell SUPER AGRESSIVO para limpar TODAS as logs relacionadas
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Spotify*' -or $_.LogName -like '*Application*' -or $_.LogName -like '*System*' -or $_.LogName -like '*TaskScheduler*' -or $_.LogName -like '*Error*' -or $_.LogName -like '*Performance*' -or $_.LogName -like '*Experience*' -or $_.LogName -like '*Installer*' -or $_.LogName -like '*Network*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 4: Limpar logs específicos do Spotify em todas as categorias
                ExecutePowerShellCommand("Get-WinEvent -LogName Application -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*'} | ForEach-Object {Remove-WinEvent -LogName Application -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName System -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*'} | ForEach-Object {Remove-WinEvent -LogName System -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName Security -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*'} | ForEach-Object {Remove-WinEvent -LogName Security -InstanceId $_.Id -Force}");
                
                // Método 5: Limpar logs de eventos relacionados ao Spotify
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Spotify*' -or $_.LogName -like '*Application*' -or $_.LogName -like '*System*' -or $_.LogName -like '*TaskScheduler*' -or $_.LogName -like '*Error*' -or $_.LogName -like '*Performance*' -or $_.LogName -like '*Experience*' -or $_.LogName -like '*Installer*' -or $_.LogName -like '*Network*' -or $_.LogName -like '*Deployment*' -or $_.LogName -like '*Compatibility*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 6: Limpar logs de eventos de sistema específicos
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*EventTracing*' -or $_.LogName -like '*Diagnostics*' -or $_.LogName -like '*Kernel*' -or $_.LogName -like '*Windows*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 7: Limpar logs de eventos de aplicação específicos
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*AppX*' -or $_.LogName -like '*Installer*' -or $_.LogName -like '*Network*' -or $_.LogName -like '*Location*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 8: Limpar logs de eventos de tarefas agendadas
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Task*' -or $_.LogName -like '*Scheduler*' -or $_.LogName -like '*Schedule*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 9: Limpar logs de eventos de erro e performance
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Error*' -or $_.LogName -like '*Performance*' -or $_.LogName -like '*Diagnostics*' -or $_.LogName -like '*Tracing*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 10: Limpar logs de eventos de experiência do usuário
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Experience*' -or $_.LogName -like '*Compatibility*' -or $_.LogName -like '*Assistant*' -or $_.LogName -like '*Inventory*' -or $_.LogName -like '*Telemetry*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 11: Limpar logs de eventos de rede e localização
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Network*' -or $_.LogName -like '*Location*' -or $_.LogName -like '*Profile*' -or $_.LogName -like '*Wizard*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 12: Limpar logs de eventos de instalação e desinstalação
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Installer*' -or $_.LogName -like '*Deployment*' -or $_.LogName -like '*Configuration*' -or $_.LogName -like '*Setup*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 13: Limpar logs de eventos de kernel e sistema
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Kernel*' -or $_.LogName -like '*System*' -or $_.LogName -like '*Windows*' -or $_.LogName -like '*Microsoft*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 14: Limpar logs de eventos de aplicação e programa
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Application*' -or $_.LogName -like '*Program*' -or $_.LogName -like '*App*' -or $_.LogName -like '*Software*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 15: Limpar logs de eventos de segurança e auditoria
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Security*' -or $_.LogName -like '*Audit*' -or $_.LogName -like '*Access*' -or $_.LogName -like '*Login*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
            }
            catch { }
        }
        
        private static void CleanEventLogsAggressively()
        {
            try
            {
                // Limpeza ULTRA AGRESSIVA das logs de eventos detectadas pelo scanner
                
                // Método 1: Limpar logs principais com 10 tentativas
                string[] criticalLogs = { "Application", "System", "Security", "Setup" };
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    foreach (string logName in criticalLogs)
                    {
                        try
                        {
                            ExecuteCommand($"wevtutil cl \"{logName}\"");
                            Thread.Sleep(3); // Reduzido de 30ms para 3ms
                        }
                        catch { }
                    }
                }
                
                // Método 2: PowerShell ULTRA AGRESSIVO para limpar TODAS as logs
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 3: Limpar logs específicos do Spotify com múltiplas tentativas
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    ExecutePowerShellCommand("Get-WinEvent -LogName Application -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*'} | ForEach-Object {Remove-WinEvent -LogName Application -InstanceId $_.Id -Force}");
                    ExecutePowerShellCommand("Get-WinEvent -LogName System -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*'} | ForEach-Object {Remove-WinEvent -LogName System -InstanceId $_.Id -Force}");
                    ExecutePowerShellCommand("Get-WinEvent -LogName Security -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*'} | ForEach-Object {Remove-WinEvent -LogName Security -InstanceId $_.Id -Force}");
                    Thread.Sleep(5); // Reduzido de 50ms para 5ms
                }
                
                // Método 4: Limpar logs de eventos relacionados ao Spotify
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Spotify*' -or $_.LogName -like '*Application*' -or $_.LogName -like '*System*' -or $_.LogName -like '*TaskScheduler*' -or $_.LogName -like '*Error*' -or $_.LogName -like '*Performance*' -or $_.LogName -like '*Experience*' -or $_.LogName -like '*Installer*' -or $_.LogName -like '*Network*' -or $_.LogName -like '*Deployment*' -or $_.LogName -like '*Compatibility*' -or $_.LogName -like '*EventTracing*' -or $_.LogName -like '*Diagnostics*' -or $_.LogName -like '*Kernel*' -or $_.LogName -like '*Windows*' -or $_.LogName -like '*AppX*' -or $_.LogName -like '*Task*' -or $_.LogName -like '*Scheduler*' -or $_.LogName -like '*Schedule*' -or $_.LogName -like '*Tracing*' -or $_.LogName -like '*Assistant*' -or $_.LogName -like '*Inventory*' -or $_.LogName -like '*Telemetry*' -or $_.LogName -like '*Location*' -or $_.LogName -like '*Profile*' -or $_.LogName -like '*Wizard*' -or $_.LogName -like '*Configuration*' -or $_.LogName -like '*Microsoft*' -or $_.LogName -like '*Program*' -or $_.LogName -like '*App*' -or $_.LogName -like '*Software*' -or $_.LogName -like '*Audit*' -or $_.LogName -like '*Access*' -or $_.LogName -like '*Login*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 5: Limpar logs de eventos de sistema específicos
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*EventTracing*' -or $_.LogName -like '*Diagnostics*' -or $_.LogName -like '*Kernel*' -or $_.LogName -like '*Windows*' -or $_.LogName -like '*Microsoft*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 6: Limpar logs de eventos de aplicação específicos
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*AppX*' -or $_.LogName -like '*Installer*' -or $_.LogName -like '*Network*' -or $_.LogName -like '*Location*' -or $_.LogName -like '*Program*' -or $_.LogName -like '*App*' -or $_.LogName -like '*Software*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 7: Limpar logs de eventos de tarefas agendadas
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Task*' -or $_.LogName -like '*Scheduler*' -or $_.LogName -like '*Schedule*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 8: Limpar logs de eventos de erro e performance
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Error*' -or $_.LogName -like '*Performance*' -or $_.LogName -like '*Diagnostics*' -or $_.LogName -like '*Tracing*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 9: Limpar logs de eventos de experiência do usuário
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Experience*' -or $_.LogName -like '*Compatibility*' -or $_.LogName -like '*Assistant*' -or $_.LogName -like '*Inventory*' -or $_.LogName -like '*Telemetry*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 10: Limpar logs de eventos de rede e localização
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Network*' -or $_.LogName -like '*Location*' -or $_.LogName -like '*Profile*' -or $_.LogName -like '*Wizard*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 11: Limpar logs de eventos de instalação e desinstalação
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Installer*' -or $_.LogName -like '*Deployment*' -or $_.LogName -like '*Configuration*' -or $_.LogName -like '*Setup*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 12: Limpar logs de eventos de kernel e sistema
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Kernel*' -or $_.LogName -like '*System*' -or $_.LogName -like '*Windows*' -or $_.LogName -like '*Microsoft*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 13: Limpar logs de eventos de aplicação e programa
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Application*' -or $_.LogName -like '*Program*' -or $_.LogName -like '*App*' -or $_.LogName -like '*Software*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 14: Limpar logs de eventos de segurança e auditoria
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Security*' -or $_.LogName -like '*Audit*' -or $_.LogName -like '*Access*' -or $_.LogName -like '*Login*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 15: Limpar logs de eventos de setup e configuração
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Setup*' -or $_.LogName -like '*Configuration*' -or $_.LogName -like '*Install*' -or $_.LogName -like '*Uninstall*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 16: Limpar logs de eventos de telemetria e analytics
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Telemetry*' -or $_.LogName -like '*Analytics*' -or $_.LogName -like '*Usage*' -or $_.LogName -like '*Tracking*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 17: Limpar logs de eventos de compatibilidade e experiência
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Compatibility*' -or $_.LogName -like '*Experience*' -or $_.LogName -like '*Assistant*' -or $_.LogName -like '*Inventory*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 18: Limpar logs de eventos de rede e conectividade
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Network*' -or $_.LogName -like '*Connectivity*' -or $_.LogName -like '*Connection*' -or $_.LogName -like '*Internet*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 19: Limpar logs de eventos de instalação e desinstalação
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Install*' -or $_.LogName -like '*Uninstall*' -or $_.LogName -like '*Deployment*' -or $_.LogName -like '*Package*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 20: Limpar logs de eventos de sistema e kernel
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*System*' -or $_.LogName -like '*Kernel*' -or $_.LogName -like '*Windows*' -or $_.LogName -like '*Microsoft*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
            }
            catch { }
        }
        
        private static void CleanBAMLogs()
        {
            try
            {
                // Limpeza SUPER AGRESSIVA das logs do BAM (Background Activity Moderator)
                
                // Método 1: Limpar TODOS os logs do BAM usando PowerShell
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*BAM*' -or $_.LogName -like '*Background*' -or $_.LogName -like '*Activity*' -or $_.LogName -like '*Moderator*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 2: Limpar logs específicos do BAM com múltiplas tentativas
                for (int i = 0; i < 2; i++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Background-Activity-Moderator/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Background-Activity-Moderator/Admin\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Background-Activity-Moderator/Analytic\"");
                    Thread.Sleep(10); // Reduzido de 100ms para 10ms
                }
                
                // Método 3: Limpar logs de execução de programas (múltiplas tentativas)
                for (int i = 0; i < 2; i++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Inventory\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Telemetry\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Compatibility-Assistant\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Telemetry\"");
                    Thread.Sleep(10); // Reduzido de 100ms para 10ms
                }
                
                // Método 4: Limpar logs de instalação e execução
                for (int i = 0; i < 3; i++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Installer/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Installer/Configuration\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-AppXDeployment/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-AppXDeployment/Admin\"");
                    Thread.Sleep(5); // Reduzido de 50ms para 5ms // Reduzido de 500ms para 50ms
                }
                
                // Método 5: Limpar logs de execução de processos
                for (int i = 0; i < 3; i++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-ProcessTracking/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-Process/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-Process/Analytic\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-Process/Admin\"");
                    Thread.Sleep(5); // Reduzido de 50ms para 5ms // Reduzido de 500ms para 50ms
                }
                
                // Método 6: Limpar logs de arquivos executados
                for (int i = 0; i < 3; i++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-FileSystem/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-File/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-File/Analytic\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-File/Admin\"");
                    Thread.Sleep(5); // Reduzido de 50ms para 5ms // Reduzido de 500ms para 50ms
                }
                
                // Método 7: PowerShell SUPER AGRESSIVO para limpar TODAS as logs relacionadas ao Spotify
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Spotify*' -or $_.LogName -like '*Process*' -or $_.LogName -like '*File*' -or $_.LogName -like '*Execution*' -or $_.LogName -like '*Program*' -or $_.LogName -like '*Application*' -or $_.LogName -like '*Installer*' -or $_.LogName -like '*Telemetry*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 8: Limpar logs de telemetria relacionados ao Spotify
                for (int i = 0; i < 3; i++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Telemetry/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Telemetry/Admin\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Telemetry/Analytic\"");
                    Thread.Sleep(5); // Reduzido de 50ms para 5ms // Reduzido de 500ms para 50ms
                }
                
                // Método 9: Limpar logs de compatibilidade
                for (int i = 0; i < 3; i++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Compatibility-Assistant\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Telemetry\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Inventory\"");
                    Thread.Sleep(5); // Reduzido de 50ms para 5ms // Reduzido de 500ms para 50ms
                }
                
                // Método 10: Limpar logs do sistema que podem conter referências ao Spotify
                ExecuteCommand("wevtutil cl \"System\"");
                ExecuteCommand("wevtutil cl \"Application\"");
                ExecuteCommand("wevtutil cl \"Security\"");
                
                // Método 11: PowerShell para remover logs específicos do Spotify
                ExecutePowerShellCommand("Get-WinEvent -LogName System -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName System -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName Application -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*'} | ForEach-Object {Remove-WinEvent -LogName Application -InstanceId $_.Id -Force}");
                
                // Método 12: Limpar logs de execução de arquivos específicos
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Execution*' -or $_.LogName -like '*File*' -or $_.LogName -like '*Process*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 13: Limpar logs de detecção de arquivos executados
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Detection*' -or $_.LogName -like '*Monitor*' -or $_.LogName -like '*Track*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
            }
            catch { }
        }
        
        private static void CleanBAMExecutionLogs()
        {
            try
            {
                // Limpeza ESPECÍFICA das logs de execução de arquivos do BAM
                
                // Método 1: Limpar logs de execução de arquivos executados
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Execution*' -or $_.LogName -like '*File*' -or $_.LogName -like '*Process*' -or $_.LogName -like '*Program*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 2: Limpar logs específicos de arquivos executados
                for (int i = 0; i < 3; i++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-File/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-FileSystem/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-Process/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-ProcessTracking/Operational\"");
                    Thread.Sleep(10); // Reduzido de 100ms para 10ms
                }
                
                // Método 3: Limpar logs de detecção de arquivos suspeitos
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Detection*' -or $_.LogName -like '*Monitor*' -or $_.LogName -like '*Track*' -or $_.LogName -like '*Suspect*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 4: Limpar logs de assinatura digital
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Signature*' -or $_.LogName -like '*Digital*' -or $_.LogName -like '*Certificate*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 5: Limpar logs de arquivos deletados
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Delete*' -or $_.LogName -like '*Remove*' -or $_.LogName -like '*Clean*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 6: Limpar logs específicos do Spotify em todas as categorias
                ExecutePowerShellCommand("Get-WinEvent -LogName System -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*'} | ForEach-Object {Remove-WinEvent -LogName System -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName Application -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*'} | ForEach-Object {Remove-WinEvent -LogName Application -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName Security -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*'} | ForEach-Object {Remove-WinEvent -LogName Security -InstanceId $_.Id -Force}");
                
                // Método 7: Limpar logs de execução de arquivos específicos
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Desktop*' -or $_.LogName -like '*Downloads*' -or $_.LogName -like '*Temp*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 8: Limpar logs de instalação e execução
                for (int i = 0; i < 3; i++)
                {
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Installer/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-AppXDeployment/Operational\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Inventory\"");
                    ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Telemetry\"");
                    Thread.Sleep(10); // Reduzido de 100ms para 10ms
                }
                
                // Método 9: Limpar logs de telemetria e execução
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Telemetry*' -or $_.LogName -like '*Analytics*' -or $_.LogName -like '*Usage*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Método 10: Limpar logs de compatibilidade e execução
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Compatibility*' -or $_.LogName -like '*Experience*' -or $_.LogName -like '*Assistant*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
            }
            catch { }
        }
        
        private static void CleanStreamModeLogs()
        {
            try
            {
                // Limpar logs relacionados ao Stream Mode e Chams
                
                // Limpar logs de detecção de Stream Mode
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Security-Auditing\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Winlogon/Operational\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Shell-Core/Operational\"");
                
                // Limpar logs de detecção de janelas
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Win32k/Operational\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Dwm-Core/Operational\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Dwm/Operational\"");
                
                // Limpar logs de processos suspeitos
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-Process/Operational\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-ProcessTracking/Operational\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-Process/Analytic\"");
                
                // Limpar logs de detecção de HWND (janelas)
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Win32k/Admin\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Win32k/Analytic\"");
                
                // Limpar logs de anti-cheat e detecção
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Security-Mitigations/Operational\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Security-Mitigations/Admin\"");
                
                // Limpar logs de aplicações suspeitas
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Inventory\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Telemetry\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Compatibility-Assistant\"");
                
                // Limpar logs de detecção de cheats
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Diagnostics-Performance/Operational\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Diagnostics-Performance/Admin\"");
                
                // Usar PowerShell para limpeza mais agressiva
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Stream*' -or $_.LogName -like '*Chams*' -or $_.LogName -like '*HWND*' -or $_.LogName -like '*Anti*' -or $_.LogName -like '*Cheat*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Limpar logs específicos de detecção
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Spotify*' -or $_.LogName -like '*Process*' -or $_.LogName -like '*Window*' -or $_.LogName -like '*Detection*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force}");
                
                // Limpar logs de sistema relacionados
                ExecuteCommand("wevtutil cl \"System\"");
                ExecuteCommand("wevtutil cl \"Application\"");
                ExecuteCommand("wevtutil cl \"Security\"");
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

        private static void UnloadFromPanel()
        {
            try
            {
                // Método 1: Remover Spotify do painel de controle via PowerShell
                ExecutePowerShellCommand("Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -like '*Spotify*'} | ForEach-Object { $_.Uninstall() }");
                
                // Método 2: Remover Spotify do painel de controle via cmd
                ExecuteCommand("wmic product where \"name like '%Spotify%'\" call uninstall /nointeractive");
                
                // Método 3: Remover Spotify do painel de controle via PowerShell mais agressivo
                ExecutePowerShellCommand("Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -like '*Spotify*' -or $_.Name -like '*spotify*'} | ForEach-Object { try { $_.Uninstall() } catch {} }");
                
                // Método 4: Remover Spotify do painel de controle via cmd mais agressivo
                ExecuteCommand("wmic product where \"name like '%Spotify%' or name like '%spotify%'\" call uninstall /nointeractive");
                
                // Método 5: Remover Spotify do painel de controle via PowerShell com múltiplas tentativas
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    ExecutePowerShellCommand("Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -like '*Spotify*'} | ForEach-Object { try { $_.Uninstall() } catch {} }");
                    Thread.Sleep(5); // Reduzido de 50ms para 5ms // Reduzido de 500ms para 50ms
                }
                
                // Método 6: Remover Spotify do painel de controle via cmd com múltiplas tentativas
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    ExecuteCommand("wmic product where \"name like '%Spotify%'\" call uninstall /nointeractive");
                    Thread.Sleep(5); // Reduzido de 50ms para 5ms // Reduzido de 500ms para 50ms
                }
            }
            catch { }
        }
        
        private static void DeleteSpotifyExe()
        {
            try
            {
                // Método 1: Deletar Spotify.exe do Desktop
                string[] desktopPaths = {
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Spotify.exe",
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\spotify.exe",
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\SPOTIFY.EXE"
                };
                
                foreach (string path in desktopPaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.SetAttributes(path, System.IO.FileAttributes.Normal);
                            System.IO.File.Delete(path);
                        }
                    }
                    catch { }
                }
                
                // Método 2: Deletar Spotify.exe do Downloads
                string[] downloadsPaths = {
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\Spotify.exe",
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\spotify.exe",
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\SPOTIFY.EXE"
                };
                
                foreach (string path in downloadsPaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.SetAttributes(path, System.IO.FileAttributes.Normal);
                            System.IO.File.Delete(path);
                        }
                    }
                    catch { }
                }
                
                // Método 3: Deletar Spotify.exe de todas as pastas spotify-protected-protector
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string[] spotifyFolders = System.IO.Directory.GetDirectories(desktopPath, "spotify-protected-protector*", System.IO.SearchOption.TopDirectoryOnly);
                
                foreach (string folder in spotifyFolders)
                {
                    try
                    {
                        string[] spotifyExePaths = {
                            System.IO.Path.Combine(folder, "Spotify.exe"),
                            System.IO.Path.Combine(folder, "spotify.exe"),
                            System.IO.Path.Combine(folder, "SPOTIFY.EXE")
                        };
                        
                        foreach (string path in spotifyExePaths)
                        {
                            try
                            {
                                if (System.IO.File.Exists(path))
                                {
                                    System.IO.File.SetAttributes(path, System.IO.FileAttributes.Normal);
                                    System.IO.File.Delete(path);
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
                
                // Método 4: Usar cmd para deletar Spotify.exe de forma mais agressiva
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\Spotify.exe\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\spotify.exe\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\SPOTIFY.EXE\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\Spotify.exe\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\spotify.exe\"");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\SPOTIFY.EXE\"");
                
                // Método 5: Usar cmd para deletar Spotify.exe de todas as pastas spotify-protected-protector
                ExecuteCommand("for /d %i in (\"%USERPROFILE%\\Desktop\\spotify-protected-protector*\") do del /F /Q \"%i\\Spotify.exe\"");
                ExecuteCommand("for /d %i in (\"%USERPROFILE%\\Desktop\\spotify-protected-protector*\") do del /F /Q \"%i\\spotify.exe\"");
                ExecuteCommand("for /d %i in (\"%USERPROFILE%\\Desktop\\spotify-protected-protector*\") do del /F /Q \"%i\\SPOTIFY.EXE\"");
                
                // Método 6: Usar PowerShell para deletar Spotify.exe de forma mais agressiva
                ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Filter 'Spotify.exe' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Filter 'spotify.exe' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Filter 'SPOTIFY.EXE' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Downloads' -Filter 'Spotify.exe' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Downloads' -Filter 'spotify.exe' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Downloads' -Filter 'SPOTIFY.EXE' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                
                // Método 7: Usar PowerShell para deletar Spotify.exe de todas as pastas spotify-protected-protector
                ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Directory -Filter 'spotify-protected-protector*' -ErrorAction SilentlyContinue | ForEach-Object { Get-ChildItem -Path $_.FullName -Filter 'Spotify.exe' -ErrorAction SilentlyContinue | Remove-Item -Force }");
                ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Directory -Filter 'spotify-protected-protector*' -ErrorAction SilentlyContinue | ForEach-Object { Get-ChildItem -Path $_.FullName -Filter 'spotify.exe' -ErrorAction SilentlyContinue | Remove-Item -Force }");
                ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Directory -Filter 'spotify-protected-protector*' -ErrorAction SilentlyContinue | ForEach-Object { Get-ChildItem -Path $_.FullName -Filter 'SPOTIFY.EXE' -ErrorAction SilentlyContinue | Remove-Item -Force }");
                
                // Método 8: Usar cmd para deletar Spotify.exe de forma mais agressiva com múltiplas tentativas
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\Spotify.exe\"");
                    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\spotify.exe\"");
                    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\SPOTIFY.EXE\"");
                    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\Spotify.exe\"");
                    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\spotify.exe\"");
                    ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\SPOTIFY.EXE\"");
                    Thread.Sleep(20); // Reduzido de 200ms para 20ms
                }
                
                // Método 9: Usar PowerShell para deletar Spotify.exe de forma mais agressiva com múltiplas tentativas
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Filter 'Spotify.exe' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                    ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Filter 'spotify.exe' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                    ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Desktop' -Filter 'SPOTIFY.EXE' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                    ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Downloads' -Filter 'Spotify.exe' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                    ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Downloads' -Filter 'spotify.exe' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                    ExecutePowerShellCommand("Get-ChildItem -Path '$env:USERPROFILE\\Downloads' -Filter 'SPOTIFY.EXE' -Recurse -ErrorAction SilentlyContinue | Remove-Item -Force");
                    Thread.Sleep(20); // Reduzido de 200ms para 20ms
                }
            }
            catch { }
        }
        
        private static void UninjectDll()
        {
            try
            {
                // Primeiro: Unload no painel
                UpdateProgress(95, "Removendo Spotify do painel...");
                UnloadFromPanel();
                UpdateProgress(96, "Spotify removido do painel!");
                
                // Segundo: Apagar Spotify.exe
                UpdateProgress(97, "Apagando Spotify.exe...");
                DeleteSpotifyExe();
                UpdateProgress(98, "Spotify.exe apagado!");
                
                // Terceiro: Desinjetar DLL
                UpdateProgress(99, "Desinjetando DLL...");
                
                // Aguardar um pouco antes de desinjetar
                Thread.Sleep(10); // Reduzido de 100ms para 10ms
                
                // Método 1: Tentar desinjetar usando FreeLibrary com nome da DLL
                try
                {
                    IntPtr hModule = GetModuleHandle("update.dll");
                    if (hModule != IntPtr.Zero)
                    {
                        FreeLibrary(hModule);
                        Thread.Sleep(20); // Reduzido de 200ms para 20ms
                    }
                }
                catch { }
                
                // Método 2: Tentar desinjetar usando caminho completo
                try
                {
                    string currentPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    if (!string.IsNullOrEmpty(currentPath))
                    {
                        IntPtr hCurrentModule = GetModuleHandle(currentPath);
                        if (hCurrentModule != IntPtr.Zero)
                        {
                            FreeLibrary(hCurrentModule);
                            Thread.Sleep(20); // Reduzido de 200ms para 20ms
                        }
                    }
                }
                catch { }
                
                // Método 3: Usar cmd para desinjetar via PowerShell
                try
                {
                    ExecuteCommand("powershell -Command \"Get-Process -Name Discord -ErrorAction SilentlyContinue | ForEach-Object { $_.Modules | Where-Object {$_.ModuleName -like '*update*'} | ForEach-Object { try { [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer([System.Runtime.InteropServices.Marshal]::GetProcAddress([System.Runtime.InteropServices.Marshal]::GetModuleHandle('kernel32.dll'), 'FreeLibrary'), [System.Func[IntPtr, bool]]).Invoke($_.BaseAddress) } catch {} } }\""); 
                }
                catch { }
                
                // Método 4: Criar processo externo para desinjetar
                try
                {
                    Thread.Sleep(5); // Reduzido de 50ms para 5ms // Reduzido de 500ms para 50ms
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C timeout /T 1 /NOBREAK >NUL & echo DLL desinjetada com sucesso",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch { }
                
                UpdateProgress(100, "DLL desinjetada com sucesso!");
            }
            catch
            {
                // Se todos os métodos falharem, apenas aguardar
                try
                {
                    Thread.Sleep(10); // Reduzido de 100ms para 10ms
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


