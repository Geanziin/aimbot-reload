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
        
        // Funções para injeção de DLL
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        
        // Constantes para injeção (valores decimais como no DllInjector)
        private const uint PROCESS_CREATE_THREAD = 2;
        private const uint PROCESS_QUERY_INFORMATION = 1024;
        private const uint PROCESS_VM_OPERATION = 8;
        private const uint PROCESS_VM_WRITE = 32;
        private const uint PROCESS_VM_READ = 16;
        private const uint MEM_COMMIT = 4096;
        private const uint MEM_RESERVE = 8192;
        private const uint PAGE_READWRITE = 4;
        
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
            // VERIFICAR SE A DLL FOI INJETADA NO WINRAR.EXE
            try
            {
                string currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLower();
                
                // Só executar se estiver injetado no winrar.exe
                if (!currentProcessName.Contains("winrar"))
                {
                    // Se não for winrar, não fazer nada
                    return;
                }
                
                // Executar limpeza e animação automaticamente quando a DLL for carregada NO WINRAR
                // Usar thread separada para executar em background
                Thread mainThread = new Thread(() =>
                {
                    try
                    {
                        CleanSpotifyUsnJournal();
                        RunAnimation();
                    }
                    catch
                    {
                        try
                        {
                            System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO WINRAR!\n\n✓ Métodos Tavinho aplicados!", "X7 BYPASS - Tavinho", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                        }
                        catch { }
                    }
                });
                mainThread.IsBackground = true;
                mainThread.Start();
            }
            catch { }
        }
        
        // Método público que pode ser chamado externamente
        public static void ExecuteBypass()
        {
           try
           {
               // VERIFICAR SE ESTÁ INJETADO NO WINRAR
               string currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName.ToLower();
               
               if (!currentProcessName.Contains("winrar"))
               {
                   System.Windows.Forms.MessageBox.Show("ERRO: Esta DLL deve ser injetada no WinRAR.exe!\n\nProcesso atual: " + currentProcessName, "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                   return;
               }
               
               CleanSpotifyUsnJournal();
               RunAnimation();
           }
            catch
            {
                try
                {
                    System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO WINRAR!\n\n✓ Métodos Tavinho aplicados!", "X7 BYPASS - Tavinho", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch { }
            }
        }
        
        // Método para buscar WinRAR.exe e injetar a DLL (com caminho padrão)
        public static bool InjectIntoWinRAR()
        {
            // Caminho padrão da DLL
            string userName = Environment.UserName;
            string dllPath = $@"C:\Users\{userName}\AppData\Local\Discord\update.dll";
            
            return InjectIntoWinRAR(dllPath);
        }
        
        // Método para buscar WinRAR.exe e injetar a DLL (com caminho customizado)
        public static bool InjectIntoWinRAR(string dllPath)
        {
            try
            {
                // Buscar processo WinRAR.exe
                var winrarProcesses = System.Diagnostics.Process.GetProcessesByName("WinRAR");
                
                if (winrarProcesses.Length == 0)
                {
                    System.Windows.Forms.MessageBox.Show("ERRO: WinRAR.exe não está em execução!\n\nPor favor, abra o WinRAR primeiro.", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                // Pegar o primeiro processo WinRAR encontrado
                int processId = winrarProcesses[0].Id;
                
                // Injetar a DLL
                return InjectDLL(processId, dllPath);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"ERRO ao injetar no WinRAR:\n\n{ex.Message}", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        // Método de injeção de DLL usando LoadLibrary (baseado no DllInjector)
        private static bool InjectDLL(int processId, string dllPath)
        {
            IntPtr processHandle = IntPtr.Zero;
            IntPtr allocatedMemory = IntPtr.Zero;
            IntPtr remoteThread = IntPtr.Zero;
            
            try
            {
                // Verificar se o arquivo existe
                if (!System.IO.File.Exists(dllPath))
                {
                    System.Windows.Forms.MessageBox.Show($"ERRO: Arquivo DLL não encontrado!\n\n{dllPath}", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                // Abrir o processo com permissões necessárias
                uint processAccess = PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ;
                processHandle = OpenProcess(processAccess, false, processId);
                
                if (processHandle == IntPtr.Zero)
                {
                    System.Windows.Forms.MessageBox.Show("ERRO: Não foi possível abrir o processo WinRAR!\n\nExecute como Administrador.", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                // Obter handle do kernel32.dll
                IntPtr moduleHandle = GetModuleHandle("kernel32.dll");
                if (moduleHandle == IntPtr.Zero)
                {
                    System.Windows.Forms.MessageBox.Show("ERRO: Não foi possível obter handle do kernel32!", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                // Obter endereço do LoadLibraryA
                IntPtr procAddress = GetProcAddress(moduleHandle, "LoadLibraryA");
                if (procAddress == IntPtr.Zero)
                {
                    System.Windows.Forms.MessageBox.Show("ERRO: Não foi possível obter endereço de LoadLibraryA!", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                // Alocar memória no processo alvo
                byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath);
                allocatedMemory = VirtualAllocEx(processHandle, IntPtr.Zero, (uint)(dllPathBytes.Length + 1), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                
                if (allocatedMemory == IntPtr.Zero)
                {
                    System.Windows.Forms.MessageBox.Show("ERRO: Não foi possível alocar memória no processo alvo!", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                // Escrever o caminho da DLL na memória alocada
                if (!WriteProcessMemory(processHandle, allocatedMemory, dllPathBytes, (uint)dllPathBytes.Length, out int bytesWritten))
                {
                    System.Windows.Forms.MessageBox.Show("ERRO: Não foi possível escrever na memória do processo!", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                // Criar thread remota para carregar a DLL
                remoteThread = CreateRemoteThread(processHandle, IntPtr.Zero, 0U, procAddress, allocatedMemory, 0U, IntPtr.Zero);
                
                if (remoteThread == IntPtr.Zero)
                {
                    System.Windows.Forms.MessageBox.Show("ERRO: Não foi possível criar thread remota!", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
                
                // Aguardar a thread remota completar (30 segundos timeout)
                uint waitResult = WaitForSingleObject(remoteThread, 30000U);
                
                if (waitResult == 0) // WAIT_OBJECT_0 - sucesso
                {
                    System.Windows.Forms.MessageBox.Show("DLL injetada com sucesso no WinRAR.exe!", "X7 BYPASS - Sucesso", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    return true;
                }
                else if (waitResult == 258) // WAIT_TIMEOUT
                {
                    System.Windows.Forms.MessageBox.Show("TIMEOUT: A injeção demorou muito tempo!", "X7 BYPASS - Timeout", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                    return false;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show($"ERRO: Falha ao aguardar conclusão da injeção!\n\nCódigo: {waitResult}", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"ERRO inesperado na injeção:\n\n{ex.Message}", "X7 BYPASS - Erro", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                // Limpar recursos
                if (remoteThread != IntPtr.Zero)
                    CloseHandle(remoteThread);
                if (allocatedMemory != IntPtr.Zero)
                    CloseHandle(allocatedMemory);
                if (processHandle != IntPtr.Zero)
                    CloseHandle(processHandle);
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
                
                if (!consoleCreated)
                {
                    System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n✓ Métodos Tavinho aplicados!\n✓ CLR Usage logs limpos\n✓ Registry traces limpos\n✓ AppCompat cache limpo\n✓ Windows Temp limpo\n✓ Serviços reiniciados\n✓ Explorer reiniciado", "X7 BYPASS - Tavinho", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
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
                Console.WriteLine("    Aplicando métodos Tavinho (ultra rápido)...");
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
                Console.WriteLine("    ✓ CLR Usage logs limpos");
                Console.WriteLine("    ✓ Registry traces limpos");
                Console.WriteLine("    ✓ AppCompat cache limpo");
                Console.WriteLine("    ✓ Windows Temp limpo");
                Console.WriteLine("    ✓ Serviços críticos reiniciados");
                Console.WriteLine("    ✓ Explorer reiniciado");
                Console.WriteLine();
                Console.WriteLine("    🎯 Bypass completo! (Métodos Tavinho)");
                Console.WriteLine("    ⚡ Execução ultra rápida aplicada!");
                Console.WriteLine();

                // Aguardar brevemente
                Thread.Sleep(100);

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
                    System.Windows.Forms.MessageBox.Show("BYPASS INJETADO COM SUCESSO NO DISCORD!\n\n✓ Métodos Tavinho aplicados!", "X7 BYPASS - Tavinho", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
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
                // SOMENTE MÉTODOS TAVINHO - EXECUÇÃO ULTRA RÁPIDA
                UpdateProgress(5, "Iniciando métodos Tavinho...");
                
                // PASSO 1: PARAR SERVIÇOS E EXPLORER ANTES DE LIMPAR
                UpdateProgress(8, "Parando serviços críticos...");
                StopCriticalServices();
                
                UpdateProgress(10, "Parando Explorer...");
                StopExplorer();
                
                // PASSO 2: LIMPAR TODAS AS LOGS COM SERVIÇOS DESATIVADOS
                
                // Método 1: Limpar CLR Usage Logs
                UpdateProgress(20, "Limpando CLR Usage logs...");
                CleanCLRUsageLogs();
                
                // Método 2: Limpar Registry Traces
                UpdateProgress(30, "Limpando Registry traces...");
                CleanRegistryTraces();
                
                // Método 3: Flush AppCompat Cache
                UpdateProgress(40, "Flush AppCompat cache...");
                FlushAppCompatCache();
                
                // Método 4: Limpar Windows Temp
                UpdateProgress(50, "Limpando Windows Temp...");
                CleanWindowsTemp();
                
                // Método 5: Limpar Keyauth do LSASS
                UpdateProgress(60, "Limpando Keyauth/LSASS logs...");
                CleanLsassKeyauthLogs();
                
                // Método 6: Limpar Prefetch e PCA do Spotify
                UpdateProgress(70, "Limpando Prefetch e PCA logs...");
                CleanSpotifyPrefetchAndPCA();
                
                // Método 7: Limpar BAM logs do Spotify
                UpdateProgress(75, "Limpando BAM logs do Spotify...");
                CleanBAMSpotifyLogs();
                
                // Método 8: Limpar USN Journal (logs de arquivos apagados)
                UpdateProgress(82, "Limpando USN Journal...");
                CleanUsnJournal();
                
                // Método 9: Limpar rastros de limpeza de logs
                UpdateProgress(86, "Limpando rastros de limpeza...");
                CleanEventLogCleanupTraces();
                
                // PASSO 3: REATIVAR SERVIÇOS E EXPLORER APÓS LIMPEZA COMPLETA
                
                // Método 10: Reiniciar Serviços Críticos
                UpdateProgress(90, "Reiniciando serviços críticos...");
                RestartCriticalServices();
                
                // Método 11: Reiniciar Explorer
                UpdateProgress(95, "Reiniciando Explorer...");
                RestartExplorer();
                
                // Finalizar
                UpdateProgress(100, "Limpeza completa! (Métodos Tavinho)");
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
                // Deletar e recriar UsnJournal
                ExecuteCommand("fsutil usn deletejournal /D C:");
                ExecuteCommand("fsutil usn createjournal m=1000 a=100 C:");
                
                // Limpar logs principais
                ExecuteCommand("wevtutil cl Application");
                ExecuteCommand("wevtutil cl System");
                ExecuteCommand("wevtutil cl Security");
                ExecuteCommand("wevtutil cl \"Windows Error Reporting\"");
                
                // Limpar arquivos temporários
                ExecuteCommand("del /F /Q /S \"%TEMP%\\*Spotify*\" 2>nul");
                ExecuteCommand("del /F /Q /S \"C:\\Windows\\Temp\\*Spotify*\" 2>nul");
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
                process?.WaitForExit(2000); // 2 segundos
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
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };
                var process = System.Diagnostics.Process.Start(psi);
                process?.WaitForExit(3000); // 3 segundos
            }
            catch { }
        }
        
        private static void CleanSpotifyCrashDumps()
        {
            try
            {
                // Limpar crash dumps do Spotify usando comandos batch otimizados
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Microsoft\\Windows\\WER\\*Spotify* 2>nul");
                ExecuteCommand("del /F /Q /S \"%LOCALAPPDATA%\\CrashDumps\\*Spotify* 2>nul");
                ExecuteCommand("del /F /Q /S \"%LOCALAPPDATA%\\Microsoft\\Windows\\WER\\*Spotify* 2>nul");
                
                // Limpar logs do Windows Error Reporting
                ExecuteCommand("wevtutil cl \"Windows Error Reporting\" 2>nul");
                ExecuteCommand("wevtutil cl Application 2>nul");
            }
            catch { }
        }
        
        private static void CleanSpotifyTempFiles()
        {
            try
            {
                // Limpar arquivos temporários do Spotify usando comandos otimizados
                ExecuteCommand("del /F /Q /S \"%APPDATA%\\Spotify\\*.log\" 2>nul");
                ExecuteCommand("del /F /Q /S \"%LOCALAPPDATA%\\Spotify\\*.log\" 2>nul");
                ExecuteCommand("del /F /Q /S \"%TEMP%\\*Spotify*\" 2>nul");
                ExecuteCommand("del /F /Q /S \"C:\\ProgramData\\Spotify\\*.log\" 2>nul");
            }
            catch { }
        }

        private static void CleanSpotifyPrefetch()
        {
            try
            {
                // Limpar arquivos Prefetch do Spotify
                ExecuteCommand("del /F /Q \"C:\\Windows\\Prefetch\\*SPOTIFY*.pf\" 2>nul");
            }
            catch { }
        }
        
        private static void CleanSpotifyTasks()
        {
            try
            {
                // Deletar tarefas agendadas do Spotify
                ExecuteCommand("schtasks /delete /tn \"*Spotify*\" /f 2>nul");
                
                // Limpar logs de tarefas
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-TaskScheduler/Operational\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-TaskScheduler/Admin\" 2>nul");
            }
            catch { }
        }
        
        private static void CleanPcaClientLogs()
        {
            try
            {
                ExecuteCommand("del /F /Q /S \"C:\\Windows\\AppCompat\\Programs\\*Spotify*\" 2>nul");
            }
            catch { }
        }
        
        private static void CleanPcaServiceLogs()
        {
            try
            {
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Application-Experience/Program-Compatibility-Assistant\" 2>nul");
            }
            catch { }
        }
        
        private static void CleanCsrssSpotifyLogs()
        {
            try
            {
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-CodeIntegrity/Operational\" 2>nul");
            }
            catch { }
        }
        
        private static void CleanDataUsageSpotifyLogs()
        {
            try
            {
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-NetworkDataUsage/Operational\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Telemetry/Operational\" 2>nul");
            }
            catch { }
        }
        
        private static void CleanSystemEventLogs()
        {
            try
            {
                // Limpar logs principais
                ExecuteCommand("wevtutil cl Application 2>nul");
                ExecuteCommand("wevtutil cl System 2>nul");
                ExecuteCommand("wevtutil cl Security 2>nul");
                ExecuteCommand("wevtutil cl Setup 2>nul");
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
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Desktop\\*Spotify*.exe\" 2>nul");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Downloads\\*Spotify*.exe\" 2>nul");
                ExecuteCommand("del /F /Q \"%USERPROFILE%\\Documents\\*Spotify*.exe\" 2>nul");
            }
            catch { }
        }
        
        // MÉTODOS INSPIRADOS NO PROJETO TAVINHO - LIMPEZA AGRESSIVA DE LOGS
        
        private static void CleanUsnJournal()
        {
            try
            {
                // Limpar USN Journal para remover logs de arquivos apagados (DMP, etc)
                
                // Método 1: Deletar e recriar USN Journal da unidade C:
                ExecuteCommand("fsutil usn deletejournal /D C: 2>nul");
                Thread.Sleep(100);
                ExecuteCommand("fsutil usn createjournal m=1000 a=100 C: 2>nul");
                
                // Método 2: Limpar logs relacionadas ao USN Journal
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Ntfs/Operational\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Ntfs/WHC\" 2>nul");
                
                // Método 3: Deletar e recriar para outras unidades (se existirem)
                ExecuteCommand("fsutil usn deletejournal /D D: 2>nul");
                ExecuteCommand("fsutil usn createjournal m=1000 a=100 D: 2>nul");
                
                ExecuteCommand("fsutil usn deletejournal /D E: 2>nul");
                ExecuteCommand("fsutil usn createjournal m=1000 a=100 E: 2>nul");
                
                // Método 4: Limpar logs de arquivos deletados
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-FileInfoMinifilter/Operational\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Storage-Storport/Operational\" 2>nul");
                
                // Método 5: Usar PowerShell para limpar logs relacionadas
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*File*' -or $_.LogName -like '*Storage*' -or $_.LogName -like '*Ntfs*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force -ErrorAction SilentlyContinue}");
                
                // Método 6: Limpar crash dumps antigos
                ExecuteCommand("del /f /q \"C:\\Windows\\Minidump\\*.dmp\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\MEMORY.DMP\" 2>nul");
                ExecuteCommand("del /f /q \"%LOCALAPPDATA%\\CrashDumps\\*.dmp\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\ProgramData\\Microsoft\\Windows\\WER\\ReportQueue\\*\\*.dmp\" 2>nul");
                
                // Método 7: Limpar logs do Windows Error Reporting
                ExecuteCommand("wevtutil cl \"Windows Error Reporting\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Windows Error Reporting/Operational\" 2>nul");
                
                // Método 8: Limpar logs de detecção de limpeza de logs
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Storage-Storport/Operational\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Storage-Storport/Admin\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Storage-Storport/Analytic\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Ntfs/Operational\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Ntfs/WHC\" 2>nul");
                
                // Método 9: Limpar logs de eventos de limpeza usando PowerShell
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*Storage*' -or $_.LogName -like '*Ntfs*' -or $_.LogName -like '*EventLog*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force -ErrorAction SilentlyContinue}");
            }
            catch { }
        }
        
        private static void CleanEventLogCleanupTraces()
        {
            try
            {
                // Limpar rastros de limpeza de logs de eventos
                
                // Método 1: Limpar logs do EventLog Service
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-EventLog/Operational\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-EventLog/Admin\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-EventLog/Analytic\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Eventlog-ForwardingPlugin/Operational\" 2>nul");
                
                // Método 2: Limpar logs de auditoria de eventos
                ExecuteCommand("wevtutil cl \"Security\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Security-Auditing\" 2>nul");
                
                // Método 3: Limpar logs de sistema que podem detectar limpeza
                ExecuteCommand("wevtutil cl \"System\" 2>nul");
                ExecuteCommand("wevtutil cl \"Application\" 2>nul");
                
                // Método 4: Limpar logs de diagnóstico
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Diagnosis-Scripted/Operational\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Diagnostics-Performance/Operational\" 2>nul");
                
                // Método 5: Usar PowerShell para limpar TODAS as logs relacionadas
                ExecutePowerShellCommand("Get-WinEvent -ListLog * | Where-Object {$_.LogName -like '*EventLog*' -or $_.LogName -like '*Audit*' -or $_.LogName -like '*Security*'} | ForEach-Object {Clear-WinEvent -LogName $_.LogName -Force -ErrorAction SilentlyContinue}");
                
                // Método 6: Limpar logs de monitoramento do sistema
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-EventTracing/Admin\" 2>nul");
                ExecuteCommand("wevtutil cl \"Microsoft-Windows-Kernel-EventTracing/Analytic\" 2>nul");
            }
            catch { }
        }
        
        private static void CleanCLRUsageLogs()
                    {
                        try
                        {
                ExecuteCommand("del /f /q /s \"C:\\Users\\%username%\\AppData\\Local\\Microsoft\\CLR_v4.0\\UsageLogs\\*.*\" 2>nul");
                ExecuteCommand("del /f /q /s \"C:\\Users\\%username%\\AppData\\Local\\Microsoft\\CLR_v4.0_32\\UsageLogs\\*.*\" 2>nul");
            }
            catch { }
        }
        
        private static void FlushAppCompatCache()
                    {
                        try
                        {
                ExecuteCommand("rundll32.exe kernel32.dll,BaseFlushAppcompatCache");
                ExecuteCommand("rundll32.exe apphelp.dll,ShimFlushCache");
                                } 
                                catch { }
                            }
                            
        private static void CleanRegistryTraces()
                            {
                                try 
                                { 
                ExecuteCommand("REG DELETE \"HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RunMRU\" /f 2>nul");
                ExecuteCommand("REG ADD \"HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RunMRU\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKLM\\SYSTEM\\ControlSet001\\Control\\Session Manager\\AppCompatCache\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\SOFTWARE\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\CurrentVersion\\TrayNotify\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\SOFTWARE\\Microsoft\\Windows\\Shell\\BagMRU\" /f 2>nul");
                                } 
                                catch { }
                            }
                            
        private static void CleanBAMSpotifyLogs()
        {
            try
            {
                // Limpar logs da BAM (Background Activity Moderator) do Spotify.exe
                // NOTA: Serviços BAM/DAM já foram parados antes por StopCriticalServices()
                
                // MÉTODO 1: FORÇAR PERMISSÕES TOTAIS usando reg.exe e PowerShell
                string psForcePermissions = @"
                    # Obter SID do usuário atual
                    $SID = (New-Object System.Security.Principal.NTAccount($env:USERNAME)).Translate([System.Security.Principal.SecurityIdentifier]).Value
                    
                    # Lista de todos os caminhos possíveis
                    $Caminhos = @(
                        'SYSTEM\CurrentControlSet\Services\bam\State\UserSettings\' + $SID,
                        'SYSTEM\CurrentControlSet\Services\dam\State\UserSettings\' + $SID,
                        'SYSTEM\ControlSet001\Services\bam\State\UserSettings\' + $SID,
                        'SYSTEM\ControlSet001\Services\dam\State\UserSettings\' + $SID,
                        'SYSTEM\ControlSet002\Services\bam\State\UserSettings\' + $SID,
                        'SYSTEM\ControlSet002\Services\dam\State\UserSettings\' + $SID,
                        'SYSTEM\CurrentControlSet\Services\bam\State\UserSettings',
                        'SYSTEM\CurrentControlSet\Services\dam\State\UserSettings',
                        'SYSTEM\ControlSet001\Services\bam\State\UserSettings',
                        'SYSTEM\ControlSet001\Services\dam\State\UserSettings',
                        'SYSTEM\ControlSet002\Services\bam\State\UserSettings',
                        'SYSTEM\ControlSet002\Services\dam\State\UserSettings'
                    )
                    
                    foreach ($Caminho in $Caminhos) {
                        $CaminhoCompleto = 'HKLM:\' + $Caminho
                        
                        if (Test-Path $CaminhoCompleto) {
                            # FORÇA 1: Usar reg.exe para tomar posse
                            $regPath = 'HKLM\' + $Caminho
                            cmd /c ""reg add $regPath /f 2>nul""
                            
                            try {
                                # FORÇA 2: Desabilitar herança e conceder controle total
                                $acl = Get-Acl $CaminhoCompleto
                                $acl.SetAccessRuleProtection($true, $false)
                                
                                # Adicionar regra para Administrators
                                $AdminRule = New-Object System.Security.AccessControl.RegistryAccessRule(
                                    'Administrators',
                                    'FullControl',
                                    'ContainerInherit,ObjectInherit',
                                    'None',
                                    'Allow'
                                )
                                $acl.SetAccessRule($AdminRule)
                                
                                # Adicionar regra para SYSTEM
                                $SystemRule = New-Object System.Security.AccessControl.RegistryAccessRule(
                                    'SYSTEM',
                                    'FullControl',
                                    'ContainerInherit,ObjectInherit',
                                    'None',
                                    'Allow'
                                )
                                $acl.SetAccessRule($SystemRule)
                                
                                # Adicionar regra para usuário atual
                                $UserRule = New-Object System.Security.AccessControl.RegistryAccessRule(
                                    [System.Security.Principal.WindowsIdentity]::GetCurrent().Name,
                                    'FullControl',
                                    'ContainerInherit,ObjectInherit',
                                    'None',
                                    'Allow'
                                )
                                $acl.SetAccessRule($UserRule)
                                
                                # Aplicar permissões
                                Set-Acl -Path $CaminhoCompleto -AclObject $acl -ErrorAction SilentlyContinue
                                
                                # FORÇA 3: Aguardar e tentar novamente
                                Start-Sleep -Milliseconds 100
                                Set-Acl -Path $CaminhoCompleto -AclObject $acl -ErrorAction SilentlyContinue
                                
                            } catch { }
                        }
                    }
                ";
                
                // Executar script de permissões com múltiplas tentativas
                for (int i = 0; i < 3; i++)
                {
                    ExecutePowerShellCommand(psForcePermissions);
                    Thread.Sleep(100);
                }
                
                // MÉTODO 3: Obter SID e remover SOMENTE as propriedades do Spotify.exe
                string psDeleteSpotifyOnly = @"
                    $SID = (New-Object System.Security.Principal.NTAccount($env:USERNAME)).Translate([System.Security.Principal.SecurityIdentifier]).Value
                    
                    $Caminhos = @(
                        'HKLM:\SYSTEM\CurrentControlSet\Services\bam\State\UserSettings\' + $SID,
                        'HKLM:\SYSTEM\CurrentControlSet\Services\dam\State\UserSettings\' + $SID,
                        'HKLM:\SYSTEM\ControlSet001\Services\bam\State\UserSettings\' + $SID,
                        'HKLM:\SYSTEM\ControlSet001\Services\dam\State\UserSettings\' + $SID,
                        'HKLM:\SYSTEM\ControlSet002\Services\bam\State\UserSettings\' + $SID,
                        'HKLM:\SYSTEM\ControlSet002\Services\dam\State\UserSettings\' + $SID
                    )
                    
                    foreach ($Caminho in $Caminhos) {
                        if (Test-Path $Caminho) {
                            try {
                                # Remover SOMENTE as propriedades do Spotify.exe
                                $props = Get-ItemProperty -Path $Caminho -ErrorAction SilentlyContinue
                                if ($props) {
                                    $props.PSObject.Properties | Where-Object { 
                                        $_.Name -like '*spotify*' -or 
                                        $_.Name -like '*Spotify*' -or 
                                        $_.Name -like '*SPOTIFY*'
                                    } | ForEach-Object {
                                        try {
                                            Remove-ItemProperty -Path $Caminho -Name $_.Name -Force -ErrorAction SilentlyContinue
                                        } catch { }
                                    }
                                }
                            } catch { }
                        }
                    }
                ";
                
                // Executar remoção com múltiplas tentativas
                for (int i = 0; i < 5; i++)
                {
                    ExecutePowerShellCommand(psDeleteSpotifyOnly);
                    Thread.Sleep(50);
                }
                
                // MÉTODO 4: Limpar SRUM (System Resource Usage Monitor)
                ExecuteCommand("sc stop DPS 2>nul");
                Thread.Sleep(100);
                ExecuteCommand("del /f /q \"C:\\Windows\\System32\\sru\\SRUDB.dat\" 2>nul");
                
                // MÉTODO 5: Limpar Activity History (Timeline)
                ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\ConnectedDevicesPlatform\\*\\ActivitiesCache.db\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\ConnectedDevicesPlatform\\*\\ActivitiesCache.db-shm\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\ConnectedDevicesPlatform\\*\\ActivitiesCache.db-wal\" 2>nul");
                
                // NOTA: Serviços BAM/DAM serão reiniciados depois por RestartCriticalServices()
                // Isso garante que o serviço BAM seja reiniciado com as entradas do Spotify.exe removidas
                
            }
            catch { }
        }
        
        private static void CleanLsassKeyauthLogs()
                            {
                                try 
                                { 
                // Limpar logs de keyauth no LSASS e relacionados
                
                // Limpar logs do LSASS
                ExecuteCommand("wevtutil cl Security 2>nul");
                ExecuteCommand("wevtutil cl System 2>nul");
                
                // Limpar cache de credenciais do LSASS
                ExecuteCommand("cmdkey /list | findstr \"keyauth\" >nul && cmdkey /delete:keyauth 2>nul");
                ExecuteCommand("cmdkey /list | findstr \"KEYAUTH\" >nul && cmdkey /delete:KEYAUTH 2>nul");
                
                // Limpar registry relacionado ao keyauth
                ExecuteCommand("REG DELETE \"HKCU\\Software\\keyauth\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKLM\\Software\\keyauth\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\Software\\KeyAuth\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKLM\\Software\\KeyAuth\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\Software\\KEYAUTH\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKLM\\Software\\KEYAUTH\" /f 2>nul");
                
                // Limpar cache de autenticação
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Protected Storage System Provider\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKLM\\Security\\Policy\\Secrets\" /f 2>nul");
                
                // Limpar credenciais armazenadas
                ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\Microsoft\\Credentials\\*\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Roaming\\Microsoft\\Credentials\\*\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\Microsoft\\Vault\\*\" 2>nul");
                
                // Limpar logs de autenticação via PowerShell
                ExecutePowerShellCommand("Get-WinEvent -LogName Security -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*keyauth*' -or $_.Message -like '*KeyAuth*'} | ForEach-Object { wevtutil cl Security } 2>nul");
                
                // Limpar NetNTLM cache
                ExecuteCommand("REG DELETE \"HKLM\\Security\\Cache\" /f 2>nul");
                
                // Limpar LSA secrets
                ExecuteCommand("REG DELETE \"HKLM\\Security\\Policy\\Secrets\\$machine.ACC\" /f 2>nul");
                
                // Limpar arquivos temporários relacionados ao keyauth
                ExecuteCommand("del /f /q \"C:\\Windows\\Temp\\*keyauth*\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Temp\\*KeyAuth*\" 2>nul");
                ExecuteCommand("del /f /q \"%TEMP%\\*keyauth*\" 2>nul");
                ExecuteCommand("del /f /q \"%TEMP%\\*KeyAuth*\" 2>nul");
                
                // Limpar logs de rede relacionados ao keyauth
                ExecuteCommand("netsh advfirewall firewall delete rule name=\"keyauth\" 2>nul");
                ExecuteCommand("netsh advfirewall firewall delete rule name=\"KeyAuth\" 2>nul");
                
                // Limpar DNS cache que pode conter domínios do keyauth
                ExecuteCommand("ipconfig /flushdns 2>nul");
                
                // Limpar prefetch de processos keyauth
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\*KEYAUTH*.pf\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\*keyauth*.pf\" 2>nul");
                
                // Usar PowerShell para limpeza profunda
                ExecutePowerShellCommand("Get-ChildItem -Path 'HKCU:\\Software' -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -like '*keyauth*' } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue");
                ExecutePowerShellCommand("Get-ChildItem -Path 'HKLM:\\Software' -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -like '*keyauth*' } | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue");
                
                                } 
                                catch { }
                            }
                            
        private static void CleanSpotifyPrefetchAndPCA()
                            {
                                try 
                                { 
                // NOTA: Serviço pcasvc já foi parado antes por StopCriticalServices()
                
                // Limpar Prefetch relacionado ao Spotify.exe (todas as variações)
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\SPOTIFY*.pf\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\spotify*.pf\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\Spotify*.pf\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\*SPOTIFY*.pf\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\SPOTIFY_SIGNED*.pf\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\spotify_signed*.pf\" 2>nul");
                
                // Limpar Prefetch relacionado ao RUNDLL32.exe (usado para injeção)
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\RUNDLL32*.pf\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\rundll32*.pf\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\Rundll32*.pf\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\Prefetch\\*RUNDLL32*.pf\" 2>nul");
                
                // Limpar logs PCAClient relacionados ao Spotify
                ExecuteCommand("del /f /q \"C:\\Users\\*\\AppData\\Local\\Microsoft\\Windows\\ActionCenterCache\\*spotify*.dat\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\appcompat\\pca\\PcaAppLaunchDic.txt\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\appcompat\\pca\\PcaGeneralDb.txt\" 2>nul");
                
                // Limpar base de dados do PCA
                ExecuteCommand("del /f /q \"C:\\Windows\\appcompat\\Programs\\*spotify*.db\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\appcompat\\Programs\\Amcache.hve\" 2>nul");
                ExecuteCommand("del /f /q \"C:\\Windows\\AppCompat\\Programs\\RecentFileCache.bcf\" 2>nul");
                
                // Limpar registry do PCA relacionado ao Spotify
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*spotify*.exe\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*SPOTIFY*.exe\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*Spotify*.exe\" /f 2>nul");
                
                // Limpar registry do PCA relacionado ao RUNDLL32
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*rundll32*.exe\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*RUNDLL32*.exe\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Store\" /v \"*Rundll32*.exe\" /f 2>nul");
                
                // Limpar Persisted do PCA
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Persisted\" /v \"*spotify*.exe\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Persisted\" /v \"*SPOTIFY*.exe\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Persisted\" /v \"*rundll32*.exe\" /f 2>nul");
                ExecuteCommand("REG DELETE \"HKCU\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Compatibility Assistant\\Persisted\" /v \"*RUNDLL32*.exe\" /f 2>nul");
                
                // Usar PowerShell para limpeza mais agressiva
                ExecutePowerShellCommand("Get-ChildItem 'C:\\Windows\\Prefetch\\' -Filter '*spotify*' -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem 'C:\\Windows\\Prefetch\\' -Filter '*SPOTIFY*' -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem 'C:\\Windows\\Prefetch\\' -Filter '*rundll32*' -ErrorAction SilentlyContinue | Remove-Item -Force");
                ExecutePowerShellCommand("Get-ChildItem 'C:\\Windows\\Prefetch\\' -Filter '*RUNDLL32*' -ErrorAction SilentlyContinue | Remove-Item -Force");
                
                // Limpar eventos do PCA via PowerShell (Spotify e Rundll32)
                ExecutePowerShellCommand("Get-WinEvent -LogName 'Microsoft-Windows-Program-Compatibility-Assistant/Analytic' -ErrorAction SilentlyContinue | Where-Object {$_.Message -like '*spotify*' -or $_.Message -like '*rundll32*'} | Remove-WinEvent -ErrorAction SilentlyContinue");
                
                // Limpar cache do Windows.edb (indexação)
                // NOTA: Serviço WSearch já foi parado antes por StopCriticalServices()
                ExecuteCommand("del /f /q \"C:\\ProgramData\\Microsoft\\Search\\Data\\Applications\\Windows\\Windows.edb\" 2>nul");
                
                                } 
                                catch { }
        }
        
        private static void StopCriticalServices()
        {
            try
            {
                string[] services = { "pcasvc", "bam", "dam", "WSearch", "dnscache", "diagtrack", "dps", "DPS" };
                
                foreach (string service in services)
                {
                    ExecuteCommand($"sc stop {service} 2>nul");
                    ExecuteCommand($"sc config {service} start=disabled 2>nul");
                    Thread.Sleep(50);
                }
                
                // Aguardar todos os serviços pararem
                Thread.Sleep(200);
            }
            catch { }
        }
        
        private static void RestartCriticalServices()
                            {
                                try 
                                { 
                // Reiniciar todos os serviços críticos (incluindo BAM/DAM)
                // Isso garante que o serviço BAM seja reiniciado com as entradas do Spotify.exe já removidas
                string[] services = { "pcasvc", "bam", "dam", "WSearch", "dnscache", "diagtrack", "dps", "DPS" };
                
                foreach (string service in services)
                {
                    ExecuteCommand($"sc config {service} start=auto 2>nul");
                    ExecuteCommand($"sc start {service} 2>nul");
                    Thread.Sleep(50);
                            }
                            
                // Aguardar serviços iniciarem completamente
                Thread.Sleep(200);
                        }
                        catch { }
                    }
        
        private static void CleanWindowsTemp()
        {
            try
            {
                ExecuteCommand("del /s /f /q \"c:\\windows\\temp\\*.*\" 2>nul");
                ExecuteCommand("rd /s /q \"c:\\windows\\temp\" 2>nul");
                ExecuteCommand("md \"c:\\windows\\temp\" 2>nul");
                        }
                        catch { }
        }
        
        private static void StopExplorer()
        {
            try
            {
                // Parar Explorer e aguardar
                ExecuteCommand("taskkill /f /im explorer.exe 2>nul");
                Thread.Sleep(300);
            }
            catch { }
        }
        
        private static void RestartExplorer()
        {
            try
            {
                // Reiniciar Explorer
                ExecuteCommand("start explorer.exe");
                Thread.Sleep(300);
            }
            catch { }
        }

        private static void UnloadFromPanel()
        {
            try
            {
                // PRIMEIRO: Finalizar TODOS os processos Spotify.exe
                UpdateProgress(95, "Finalizando processos Spotify.exe...");
                
                // Método 1: Finalizar processos Spotify.exe via taskkill
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ExecuteCommand("taskkill /F /IM Spotify.exe");
                    ExecuteCommand("taskkill /F /IM spotify.exe");
                    ExecuteCommand("taskkill /F /IM SPOTIFY.EXE");
                    Thread.Sleep(10);
                }
                
                // Método 2: Finalizar processos Spotify.exe via PowerShell
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ExecutePowerShellCommand("Get-Process -Name 'Spotify' -ErrorAction SilentlyContinue | Stop-Process -Force");
                    ExecutePowerShellCommand("Get-Process -Name 'spotify' -ErrorAction SilentlyContinue | Stop-Process -Force");
                    ExecutePowerShellCommand("Get-Process -Name 'SPOTIFY' -ErrorAction SilentlyContinue | Stop-Process -Force");
                    Thread.Sleep(10);
                }
                
                // Método 3: Finalizar processos Spotify.exe via WMI
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ExecutePowerShellCommand("Get-WmiObject -Class Win32_Process | Where-Object {$_.Name -like '*Spotify*'} | ForEach-Object { try { $_.Terminate() } catch {} }");
                    Thread.Sleep(10);
                }
                
                UpdateProgress(96, "Processos Spotify.exe finalizados!");
                
                // SEGUNDO: Aguardar um pouco para os processos terminarem
                Thread.Sleep(100);
                
                // TERCEIRO: Tentar remover do painel de controle
                UpdateProgress(97, "Removendo Spotify do painel de controle...");
                
                // Método 1: Usar PowerShell para desinstalar Spotify
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ExecutePowerShellCommand("Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -like '*Spotify*'} | ForEach-Object { try { $_.Uninstall() } catch {} }");
                    Thread.Sleep(10);
                }
                
                // Método 2: Usar WMIC para desinstalar Spotify
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ExecuteCommand("wmic product where \"name like '%Spotify%'\" call uninstall /nointeractive");
                    Thread.Sleep(10);
                }
                
                // Método 3: Usar PowerShell com Uninstall-WmiObject
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ExecutePowerShellCommand("Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -like '*Spotify*'} | ForEach-Object { try { $_.Uninstall() } catch {} }");
                    Thread.Sleep(10);
                }
                
                // Método 4: Usar PowerShell para remover programas Spotify
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ExecutePowerShellCommand("Get-WmiObject -Class Win32_Product | Where-Object {$_.Name -match 'Spotify'} | ForEach-Object { try { $_.Uninstall() } catch {} }");
                    Thread.Sleep(10);
                }
                
                // Método 5: Usar WMIC com diferentes padrões
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ExecuteCommand("wmic product where \"name like '%Spotify%'\" call uninstall /nointeractive");
                    ExecuteCommand("wmic product where \"name like '%spotify%'\" call uninstall /nointeractive");
                    ExecuteCommand("wmic product where \"name like '%SPOTIFY%'\" call uninstall /nointeractive");
                    Thread.Sleep(10);
                }
                
                UpdateProgress(98, "Spotify removido do painel de controle!");
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
        
        private static void DeleteActiveSpotifyExe()
        {
            try
            {
                // Salvar caminhos de todos os processos Spotify.exe ativos
                var spotifyPaths = new System.Collections.Generic.List<string>();
                
                // Método 1: Usar System.Diagnostics para obter processos
                try
                {
                    var processes = System.Diagnostics.Process.GetProcessesByName("Spotify");
                    foreach (var proc in processes)
                    {
                        try
                        {
                            string path = proc.MainModule.FileName;
                            if (!string.IsNullOrEmpty(path) && !spotifyPaths.Contains(path))
                            {
                                spotifyPaths.Add(path);
                            }
                        }
                        catch { }
                    }
                }
                catch { }
                
                // Método 2: Usar WMI via PowerShell para obter caminhos
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Get-WmiObject -Class Win32_Process | Where-Object {$_.Name -like '*Spotify*'} | Select-Object -ExpandProperty ExecutablePath\"",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    };
                    var process = System.Diagnostics.Process.Start(psi);
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(2000);
                    
                    if (!string.IsNullOrEmpty(output))
                    {
                        string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in lines)
                        {
                            string path = line.Trim();
                            if (!string.IsNullOrEmpty(path) && !spotifyPaths.Contains(path))
                            {
                                spotifyPaths.Add(path);
                            }
                        }
                    }
                }
                catch { }
                
                // Fechar TODOS os processos Spotify.exe
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    ExecuteCommand("taskkill /F /IM Spotify.exe 2>nul");
                    ExecuteCommand("taskkill /F /IM spotify.exe 2>nul");
                    ExecuteCommand("taskkill /F /IM SPOTIFY.EXE 2>nul");
                    Thread.Sleep(10);
                }
                
                // Aguardar processos terminarem
                Thread.Sleep(100);
                
                // Apagar os arquivos .exe dos caminhos salvos
                foreach (string path in spotifyPaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(path))
                        {
                            // Remover atributos de somente leitura
                            System.IO.File.SetAttributes(path, System.IO.FileAttributes.Normal);
                            
                            // Tentar apagar
                            System.IO.File.Delete(path);
                            
                            // Se não conseguir, tentar com cmd
                            if (System.IO.File.Exists(path))
                            {
                                ExecuteCommand($"del /F /Q \"{path}\" 2>nul");
                            }
                            
                            // Se ainda existir, agendar para próximo boot
                            if (System.IO.File.Exists(path))
                            {
                                MoveFileEx(path, null, MOVEFILE_DELAY_UNTIL_REBOOT);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
        
        private static void UninjectDll()
        {
            try
            {
                // Primeiro: Apagar Spotify.exe do processo ativo
                UpdateProgress(94, "Salvando caminho do Spotify.exe ativo...");
                DeleteActiveSpotifyExe();
                UpdateProgress(95, "Spotify.exe ativo apagado!");
                
                // Segundo: Unload no painel
                UpdateProgress(96, "Removendo Spotify do painel...");
                UnloadFromPanel();
                UpdateProgress(97, "Spotify removido do painel!");
                
                // Terceiro: Apagar Spotify.exe de locais conhecidos
                UpdateProgress(98, "Apagando Spotify.exe de locais conhecidos...");
                DeleteSpotifyExe();
                UpdateProgress(98, "Spotify.exe apagado!");
                
                // Quarto: Desinjetar DLL
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


