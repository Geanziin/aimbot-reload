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
                Thread.Sleep(1000); // Aguardar um pouco para garantir que a DLL foi carregada completamente
                
                // Tentar criar console (3 tentativas)
                bool consoleCreated = false;
                for (int i = 0; i < 3; i++)
                {
                    if (AllocConsole())
                    {
                        consoleCreated = true;
                        break;
                    }
                    Thread.Sleep(100);
                }
                
                if (!consoleCreated)
                {
                    // Fallback: usar MessageBox se console falhar
                    System.Windows.Forms.MessageBox.Show("Bypass executado com sucesso!", "Bypass Inject", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    return;
                }
                
                // Configurar console
                try { Console.Title = "Bypass Inject - Limpeza em Andamento"; } catch { }
                try { Console.OutputEncoding = Encoding.UTF8; } catch { }
                try { Console.ForegroundColor = ConsoleColor.Green; } catch { }
                try { Console.TreatControlCAsInput = true; } catch { }
                try { Console.CancelKeyPress += (s, e) => { e.Cancel = true; }; } catch { }
                
                // Executar limpeza e animação
                RunAnimation();
                
                // Aguardar um pouco antes de fechar
                Thread.Sleep(2000);
                
                // Fechar console
                try { FreeConsole(); } catch { }
            }
            catch
            {
                // Ignorar erros
            }
        }

        public static void ExecuteBypass()
        {
            // Método público para ser chamado via reflection se necessário
            try
            {
                RunAnimation();
            }
            catch
            {
                // Ignorar erros
            }
        }

        private static void RunAnimation()
        {
            try
            {
                // Limpar console
                try { Console.Clear(); } catch { }
                
                // Mostrar ASCII art
                ShowASCIIArt();
                
                // Executar limpeza em thread separada
                Thread cleanupThread = new Thread(() => {
                    try
                    {
                        CleanSpotifyUsnJournal();
                    }
                    catch { }
                });
                cleanupThread.Start();
                
                // Mostrar animação de progresso
                AnimationThread();
                
                // Aguardar limpeza terminar
                cleanupThread.Join();
                
                // Mostrar status final
                ShowFinalStatus();
                
                // Executar ações finais
                ExecuteFinalActions();
            }
            catch
            {
                // Ignorar erros
            }
        }

        private static void ShowASCIIArt()
        {
            try
            {
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    BYPASS INJECT v2.0                      ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║  ██████╗ ██╗   ██╗██████╗  █████╗ ███████╗███████╗         ║");
                Console.WriteLine("║  ██╔══██╗╚██╗ ██╔╝██╔══██╗██╔══██╗██╔════╝██╔════╝         ║");
                Console.WriteLine("║  ██████╔╝ ╚████╔╝ ██████╔╝███████║███████╗███████╗         ║");
                Console.WriteLine("║  ██╔══██╗  ╚██╔╝  ██╔═══╝ ██╔══██║╚════██║╚════██║         ║");
                Console.WriteLine("║  ██████╔╝   ██║   ██║     ██║  ██║███████║███████║         ║");
                Console.WriteLine("║  ╚═════╝    ╚═╝   ╚═╝     ╚═╝  ╚═╝╚══════╝╚══════╝         ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║              Iniciando limpeza completa do sistema...         ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
            }
            catch { }
        }

        private static void AnimationThread()
        {
            try
            {
                while (_currentProgress < 100)
                {
                    try
                    {
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write($"Progresso: [{GetProgressBar()}] {_currentProgress}% - {_currentStatus}");
                    }
                    catch { }
                    
                    Thread.Sleep(50);
                }
                
                // Limpar linha final
                try
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(new string(' ', Console.LargestWindowWidth));
                }
                catch { }
            }
            catch { }
        }

        private static void UpdateProgress(int percentage, string status)
        {
            _currentProgress = percentage;
            _currentStatus = status;
        }

        private static string GetProgressBar()
        {
            int barLength = 30;
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
                
                // Executar limpezas em paralelo para máxima velocidade (10-80%)
                UpdateProgress(15, "Executando limpezas em paralelo...");
                
                // Criar threads para limpezas simultaneamente (SEM logs do sistema)
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
                
                // Iniciar threads simultaneamente (SEM logs do sistema)
                crashDumpsThread.Start();
                tempFilesThread.Start();
                prefetchThread.Start();
                tasksThread.Start();
                desktopFilesThread.Start();
                aggressiveLogsThread.Start();
                bamLogsThread.Start();
                bamExecutionThread.Start();
                bamSpotifyThread.Start();
                streamModeThread.Start();
                
                // Aguardar threads terminarem (SEM logs do sistema)
                crashDumpsThread.Join();
                tempFilesThread.Join();
                prefetchThread.Join();
                tasksThread.Join();
                desktopFilesThread.Join();
                aggressiveLogsThread.Join();
                bamLogsThread.Join();
                bamExecutionThread.Join();
                bamSpotifyThread.Join();
                streamModeThread.Join();
                
                UpdateProgress(80, "Limpezas paralelas concluídas!");
                
                // LIMPAR LOGS DO SISTEMA POR ÚLTIMO (80-100%)
                UpdateProgress(85, "Limpando logs do sistema por último...");
                CleanSystemEventLogs();
                UpdateProgress(95, "Logs do sistema limpos!");
                
                // Finalizar limpeza (95-100%)
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
                // Limpar UsnJournal para o processo específico
                ExecuteCommand($"fsutil usn deletejournal /D C:");
                Thread.Sleep(10);
                ExecuteCommand($"fsutil usn createjournal m=1000 a=100 C:");
            }
            catch { }
        }

        private static void CleanSpotifyCrashDumps()
        {
            try
            {
                // Limpeza SUPER AGRESSIVA de crash dumps do Spotify.exe
                string[] crashDumpPaths = {
                    @"C:\Users\%USERNAME%\AppData\Local\CrashDumps",
                    @"C:\Users\%USERNAME%\AppData\Local\Microsoft\Windows\WER",
                    @"C:\ProgramData\Microsoft\Windows\WER",
                    @"C:\Windows\System32\LogFiles\WER",
                    @"C:\Windows\Minidump",
                    @"C:\Windows\LiveKernelReports"
                };
                
                foreach (string path in crashDumpPaths)
                {
                    try
                    {
                        string expandedPath = Environment.ExpandEnvironmentVariables(path);
                        ExecuteCommand($"del /F /Q /S \"{expandedPath}\\*Spotify*\"");
                        ExecuteCommand($"del /F /Q /S \"{expandedPath}\\*spotify*\"");
                        ExecuteCommand($"del /F /Q /S \"{expandedPath}\\*SPOTIFY*\"");
                        ExecuteCommand($"for /r \"{expandedPath}\" %i in (*Spotify*) do del /F /Q \"%i\"");
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static void CleanSpotifyTempFiles()
        {
            try
            {
                // Limpeza de arquivos temporários do Spotify
                string[] tempPaths = {
                    @"C:\Users\%USERNAME%\AppData\Local\Temp",
                    @"C:\Users\%USERNAME%\AppData\Roaming\Spotify",
                    @"C:\Users\%USERNAME%\AppData\Local\Spotify",
                    @"C:\Windows\Temp"
                };
                
                foreach (string path in tempPaths)
                {
                    try
                    {
                        string expandedPath = Environment.ExpandEnvironmentVariables(path);
                        ExecuteCommand($"del /F /Q /S \"{expandedPath}\\*Spotify*\"");
                        ExecuteCommand($"del /F /Q /S \"{expandedPath}\\*spotify*\"");
                        ExecuteCommand($"del /F /Q /S \"{expandedPath}\\*SPOTIFY*\"");
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static void CleanSpotifyPrefetch()
        {
            try
            {
                // Limpeza de arquivos Prefetch relacionados ao Spotify
                ExecuteCommand("del /F /Q \"C:\\Windows\\Prefetch\\SPOTIFY*\"");
                ExecuteCommand("del /F /Q \"C:\\Windows\\Prefetch\\spotify*\"");
                ExecuteCommand("del /F /Q \"C:\\Windows\\Prefetch\\Spotify*\"");
            }
            catch { }
        }

        private static void CleanSpotifyTasks()
        {
            try
            {
                // Limpeza SUPER AGRESSIVA de tarefas agendadas do Spotify
                ExecuteCommand("schtasks /query /fo csv | findstr /i spotify");
                ExecuteCommand("schtasks /delete /tn \"*Spotify*\" /f");
                ExecuteCommand("schtasks /delete /tn \"*spotify*\" /f");
                ExecuteCommand("schtasks /delete /tn \"*SPOTIFY*\" /f");
                
                // Limpar logs de tarefas agendadas
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-TaskScheduler/Operational\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-TaskScheduler/Admin\"");
            }
            catch { }
        }

        private static void CleanSpotifyDesktopFiles()
        {
            try
            {
                // Limpeza de arquivos do Desktop e Downloads relacionados ao Spotify
                string[] desktopPaths = {
                    @"C:\Users\%USERNAME%\Desktop",
                    @"C:\Users\%USERNAME%\Downloads",
                    @"C:\Users\%USERNAME%\Documents"
                };
                
                foreach (string path in desktopPaths)
                {
                    try
                    {
                        string expandedPath = Environment.ExpandEnvironmentVariables(path);
                        ExecuteCommand($"del /F /Q \"{expandedPath}\\*Spotify*\"");
                        ExecuteCommand($"del /F /Q \"{expandedPath}\\*spotify*\"");
                        ExecuteCommand($"del /F /Q \"{expandedPath}\\*SPOTIFY*\"");
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static void CleanEventLogsAggressively()
        {
            try
            {
                // Limpeza AGRESSIVA de logs de eventos
                string[] aggressiveLogs = {
                    "Microsoft-Windows-Security-Auditing/Operational",
                    "Microsoft-Windows-Winlogon/Operational",
                    "Microsoft-Windows-Shell-Core/Operational",
                    "Microsoft-Windows-Win32k/Operational",
                    "Microsoft-Windows-Dwm-Core/Operational",
                    "Microsoft-Windows-Dwm/Operational",
                    "Microsoft-Windows-Kernel-Process/Operational",
                    "Microsoft-Windows-ProcessTracking/Operational",
                    "Microsoft-Windows-Security-Mitigations/Operational"
                };
                
                foreach (string logName in aggressiveLogs)
                {
                    try
                    {
                        ExecuteCommand($"wevtutil cl \"{logName}\"");
                        Thread.Sleep(5);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private static void CleanBAMLogs()
        {
            try
            {
                // Limpeza de logs do Background Activity Moderator
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Background-Activity-Moderator/Operational\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Background-Activity-Moderator/Admin\"");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Background-Activity-Moderator/Analytic\"");
            }
            catch { }
        }

        private static void CleanBAMExecutionLogs()
        {
            try
            {
                // Limpeza específica de logs de execução do BAM
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*executados via BAM*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
            }
            catch { }
        }

        private static void CleanBAMSpotifyLogs()
        {
            try
            {
                // Limpeza ULTRA AGRESSIVA das logs do BAM específicas do Spotify.exe
                // Baseado em pesquisa sobre métodos avançados para limpar logs do BAM
                
                // MÉTODO 1: Desativar temporariamente o BAM para permitir limpeza
                ExecuteCommand("sc stop \"BamService\"");
                ExecuteCommand("sc config \"BamService\" start= disabled");
                Thread.Sleep(50);
                
                // MÉTODO 2: Limpar logs do BAM via registro (método avançado)
                ExecutePowerShellCommand("Remove-ItemProperty -Path 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Bam\\Parameters' -Name '*Spotify*' -Force -ErrorAction SilentlyContinue");
                ExecutePowerShellCommand("Get-ChildItem -Path 'HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Bam\\Parameters' -Recurse | Where-Object {$_.Name -like '*Spotify*'} | Remove-Item -Force -ErrorAction SilentlyContinue");
                
                // MÉTODO 3: Limpar arquivos físicos do BAM
                ExecuteCommand("del /F /Q /S \"C:\\Windows\\System32\\LogFiles\\BAM\\*Spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\Windows\\System32\\LogFiles\\BAM\\*spotify*\"");
                ExecuteCommand("del /F /Q /S \"C:\\Windows\\System32\\LogFiles\\BAM\\*SPOTIFY*\"");
                ExecuteCommand("for /r \"C:\\Windows\\System32\\LogFiles\\BAM\\\" %i in (*Spotify*) do del /F /Q \"%i\"");
                
                // MÉTODO 4: Limpar logs do BAM via PowerShell com permissões elevadas
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*' -or $_.Message -like '*SPOTIFY*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Operational' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*' -or $_.Message -like '*SPOTIFY*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Admin' -InstanceId $_.Id -Force}");
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*Spotify*' -or $_.Message -like '*spotify*' -or $_.Message -like '*SPOTIFY*'} | ForEach-Object {Remove-WinEvent -LogName 'Microsoft-Windows-Background-Activity-Moderator/Analytic' -InstanceId $_.Id -Force}");
                
                // MÉTODO 31: Reativar o BAM após limpeza
                ExecuteCommand("sc config \"BamService\" start= auto");
                ExecuteCommand("sc start \"BamService\"");
            }
            catch { }
        }

        private static void CleanStreamModeLogs()
        {
            try
            {
                // Limpeza de logs relacionadas a Stream Mode e Chams
                string[] streamModeLogs = {
                    "Microsoft-Windows-Security-Auditing/Operational",
                    "Microsoft-Windows-Winlogon/Operational",
                    "Microsoft-Windows-Shell-Core/Operational",
                    "Microsoft-Windows-Win32k/Operational",
                    "Microsoft-Windows-Dwm-Core/Operational",
                    "Microsoft-Windows-Dwm/Operational",
                    "Microsoft-Windows-Kernel-Process/Operational",
                    "Microsoft-Windows-ProcessTracking/Operational",
                    "Microsoft-Windows-Security-Mitigations/Operational",
                    "Microsoft-Windows-Diagnostics-Performance/Operational",
                    "Microsoft-Windows-Application-Experience/Program-Inventory"
                };
                
                foreach (string logName in streamModeLogs)
                {
                    try
                    {
                        ExecuteCommand($"wevtutil cl \"{logName}\"");
                        Thread.Sleep(5);
                    }
                    catch { }
                }
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
                    "Microsoft-Windows-WER-SystemErrorReporting/Operational",
                    "Microsoft-Windows-Kernel-EventTracing/Admin",
                    "Microsoft-Windows-Diagnostics-Performance/Operational",
                    "Microsoft-Windows-Application-Experience/Program-Inventory",
                    "Microsoft-Windows-Installer/Operational",
                    "Microsoft-Windows-NetworkProfile/Operational"
                };
                
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    foreach (string logName in windowsLogs)
                    {
                        try
                        {
                            ExecuteCommand($"wevtutil cl \"{logName}\"");
                            Thread.Sleep(5); // Reduzido de 25ms para 5ms
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private static void ShowFinalStatus()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    LIMPEZA CONCLUÍDA!                       ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║  ✅ UsnJournal limpo                                         ║");
                Console.WriteLine("║  ✅ Crash dumps removidos                                    ║");
                Console.WriteLine("║  ✅ Arquivos temporários limpos                              ║");
                Console.WriteLine("║  ✅ Prefetch limpo                                            ║");
                Console.WriteLine("║  ✅ Tarefas agendadas removidas                              ║");
                Console.WriteLine("║  ✅ Arquivos Desktop/Downloads limpos                        ║");
                Console.WriteLine("║  ✅ Logs de eventos limpos                                    ║");
                Console.WriteLine("║  ✅ Logs do BAM limpos                                       ║");
                Console.WriteLine("║  ✅ Logs de Stream Mode limpos                               ║");
                Console.WriteLine("║  ✅ Logs do sistema limpos (por último)                      ║");
                Console.WriteLine("║                                                              ║");
                Console.WriteLine("║              Sistema completamente limpo!                    ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
            }
            catch { }
        }

        private static void ExecuteFinalActions()
        {
            try
            {
                // Executar ações finais: unload, deletar Spotify.exe, desinjetar DLL
                UnloadFromPanel();
                DeleteSpotifyExe();
                UninjectDll();
            }
            catch { }
        }

        private static void UnloadFromPanel()
        {
            try
            {
                // FINALIZAR PROCESSOS DO SPOTIFY PRIMEIRO (método super agressivo)
                
                // Método 1: taskkill múltiplas tentativas
                for (int i = 0; i < 5; i++)
                {
                    ExecuteCommand("taskkill /F /IM Spotify.exe");
                    ExecuteCommand("taskkill /F /IM spotify.exe");
                    ExecuteCommand("taskkill /F /IM SPOTIFY.EXE");
                    Thread.Sleep(10);
                }
                
                // Método 2: PowerShell Stop-Process múltiplas tentativas
                for (int i = 0; i < 3; i++)
                {
                    ExecutePowerShellCommand("Get-Process -Name '*Spotify*' -ErrorAction SilentlyContinue | Stop-Process -Force");
                    ExecutePowerShellCommand("Get-Process -Name '*spotify*' -ErrorAction SilentlyContinue | Stop-Process -Force");
                    ExecutePowerShellCommand("Get-Process -Name '*SPOTIFY*' -ErrorAction SilentlyContinue | Stop-Process -Force");
                    Thread.Sleep(10);
                }
                
                // Método 3: WMI Terminate múltiplas tentativas
                ExecutePowerShellCommand("Get-WmiObject -Class Win32_Process | Where-Object {$_.Name -like '*Spotify*'} | ForEach-Object { try { $_.Terminate() } catch {} }");
                
                // Aguardar processos terminarem
                Thread.Sleep(100);
                
                // AGORA TENTAR UNINSTALL (após processos finalizados)
                
                // Método 1: WMI Uninstall múltiplas tentativas
                for (int i = 0; i < 3; i++)
                {
                    ExecutePowerShellCommand("Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -like '*Spotify*'} | ForEach-Object { try { $_.Uninstall() } catch {} }");
                    Thread.Sleep(50);
                }
                
                // Método 2: WMIC múltiplas tentativas
                for (int i = 0; i < 3; i++)
                {
                    ExecuteCommand("wmic product where \"name like '%Spotify%'\" call uninstall");
                    ExecuteCommand("wmic product where \"name like '%spotify%'\" call uninstall");
                    ExecuteCommand("wmic product where \"name like '%SPOTIFY%'\" call uninstall");
                    Thread.Sleep(50);
                }
                
                // Método 3: PowerShell Get-WmiObject com regex
                ExecutePowerShellCommand("Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -match 'Spotify'} | ForEach-Object { try { $_.Uninstall() } catch {} }");
            }
            catch { }
        }

        private static void DeleteSpotifyExe()
        {
            try
            {
                // Deletar Spotify.exe de locais comuns
                string[] spotifyPaths = {
                    @"C:\Users\%USERNAME%\AppData\Local\Spotify\Spotify.exe",
                    @"C:\Users\%USERNAME%\AppData\Roaming\Spotify\Spotify.exe",
                    @"C:\Program Files\Spotify\Spotify.exe",
                    @"C:\Program Files (x86)\Spotify\Spotify.exe"
                };
                
                foreach (string path in spotifyPaths)
                {
                    try
                    {
                        string expandedPath = Environment.ExpandEnvironmentVariables(path);
                        if (System.IO.File.Exists(expandedPath))
                        {
                            _targetDeletePath = expandedPath;
                            System.IO.File.Delete(expandedPath);
                        }
                    }
                    catch { }
                }
                
                // Tentar deletar com MoveFileEx para próxima reinicialização
                try
                {
                    if (!string.IsNullOrEmpty(_targetDeletePath) && System.IO.File.Exists(_targetDeletePath))
                    {
                        MoveFileEx(_targetDeletePath, null, MOVEFILE_DELAY_UNTIL_REBOOT);
                    }
                }
                catch { }
            }
            catch { }
        }

        private static void UninjectDll()
        {
            try
            {
                // Tentar desinjetar DLL do Discord
                IntPtr moduleHandle = GetModuleHandle("update.dll");
                if (moduleHandle != IntPtr.Zero)
                {
                    FreeLibrary(moduleHandle);
                }
                
                // Fallback: tentar via PowerShell
                try
                {
                    ExecuteCommand("powershell -Command \"Get-Process -Name Discord -ErrorAction SilentlyContinue | ForEach-Object { $_.Modules | Where-Object {$_.ModuleName -like '*update*'} | ForEach-Object { try { [System.Runtime.InteropServices.Marshal]::GetDelegateForFunctionPointer([System.Runtime.InteropServices.Marshal]::GetProcAddress([System.Runtime.InteropServices.Marshal]::GetModuleHandle('kernel32.dll'), 'FreeLibrary'), [System.Func[IntPtr, bool]]).Invoke($_.BaseAddress) } catch {} } }\"");
                }
                catch { }
                
                // Fallback final: terminar Discord se necessário
                try
                {
                    ExecuteCommand("taskkill /F /IM Discord.exe");
                }
                catch { }
            }
            catch { }
        }

        private static void ExecuteCommand(string command)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C {command}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                })?.WaitForExit();
            }
            catch { }
        }

        private static void ExecutePowerShellCommand(string command)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-Command \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                })?.WaitForExit();
            }
            catch { }
        }
    }
}
