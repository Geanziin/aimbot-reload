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

        // 4 = MOVEFILE_DELAY_UNTIL_REBOOT
        private const int MOVEFILE_DELAY_UNTIL_REBOOT = 0x00000004;

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


