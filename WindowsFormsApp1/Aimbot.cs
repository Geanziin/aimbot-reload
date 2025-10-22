﻿// Decompiled with JetBrains decompiler
// Type: WindowsFormsApp1.Aimbot
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using Guna.UI2.WinForms;
using MemByRafa;
using MysteriousMem;
using svchost.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable disable
namespace WindowsFormsApp1;

public class Aimbot : UserControl
{
  private static Mysterious Hello = new Mysterious();
  private ToolTip toolTip = new ToolTip();
  private string AimbotScan = "FF FF ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 ?? ?? ?? ?? ?? ?? ?? ?? 00 00 A5 43";
  private string headoffset = "0x80";
  private string chestoffset = "0x7C";
  private string AimbotScan1 = "FF FF ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 00 00 ?? ?? ?? ?? ?? ?? ?? ?? 00 00 A5 43";
  private string headoffset1 = "0xC0";
  private string chestoffset1 = "0x7C";
  
  // No Recoil patterns
  private string NoRecoilOld = "7A 44 F0 48 2D E9 10 B0 8D E2 02 8B 2D ED 08 D0";
  private string NoRecoilNew = "7A FF F0 48 2D E9 10 B0 8D E2 02 8B 2D ED 08 D0";
  
  
  private Dictionary<long, int> OrginalValues1 = new Dictionary<long, int>();
  private Dictionary<long, int> OrginalValues2 = new Dictionary<long, int>();
  private Dictionary<long, int> OrginalValues3 = new Dictionary<long, int>();
  private Dictionary<long, int> OrginalValues4 = new Dictionary<long, int>();
  private Dictionary<long, int> OrginalValues5 = new Dictionary<long, int>();
  private Mem memory = new Mem();
  private IContainer components;
  private Guna2Panel guna2Panel2;
  private Txt txt1;
  private Guna2Panel guna2Panel1;
  private Txt txt2;
  private CustomCheckbox customCheckbox6;
  private CustomCheckbox customCheckbox5;
  private Txt status;
  private Guna2DragControl guna2DragControl1;
  private CustomCheckbox customCheckbox1;
  private CustomCheckbox customCheckboxPrecision;
  private CustomCheckbox customCheckboxNoRecoil;

  public Aimbot()
  {
    this.InitializeComponent();
    this.ConfigureToolTips();
  }

  private void ConfigureToolTips()
  {
    this.toolTip.BackColor = Color.White;
    this.toolTip.ForeColor = Color.Black;
    this.toolTip.SetToolTip((Control) this.customCheckbox6, "Ativa/Desativa Aimbot New:\n• Ativar Todo Inicio de Partida\n");
    this.toolTip.SetToolTip((Control) this.customCheckbox1, "Ativa/Desativa Aimbot Legit:\n• Ativar Todo Inicio de Partida\n");
    this.toolTip.SetToolTip((Control) this.customCheckbox5, "Ativa/Desativa Aimbot 2x:\n• Ativar no treinamento/partida\n• Sistema de ativação/desativação automática\n• Melhora a precisão em 2x");
    this.toolTip.SetToolTip((Control) this.customCheckboxPrecision, "Ativa/Desativa Precision:\n• Modifica hexadecimal do HD-Player\n• Reduz trem do aimbot\n• Melhora precisão de mira");
    this.toolTip.SetToolTip((Control) this.customCheckboxNoRecoil, "Ativa/Desativa No Recoil:\n• Remove recuo da arma\n• Melhora precisão de tiro\n• Aplica patch de memória");
    this.toolTip.AutoPopDelay = 5000;
    this.toolTip.InitialDelay = 500;
    this.toolTip.ReshowDelay = 500;
    this.toolTip.ShowAlways = true;
    this.toolTip.ToolTipTitle = "Função do Aimbot";
    this.toolTip.IsBalloon = false;
    this.toolTip.Draw += new DrawToolTipEventHandler(this.ToolTip_Draw);
  }

  private void ToolTip_Draw(object sender, DrawToolTipEventArgs e)
  {
    e.Graphics.FillRectangle(Brushes.Black, e.Bounds);
    Graphics graphics = e.Graphics;
    Pen white = Pens.White;
    Rectangle bounds = e.Bounds;
    int x = bounds.X;
    bounds = e.Bounds;
    int y = bounds.Y;
    bounds = e.Bounds;
    int width = bounds.Width - 1;
    bounds = e.Bounds;
    int height = bounds.Height - 1;
    Rectangle rect = new Rectangle(x, y, width, height);
    graphics.DrawRectangle(white, rect);
    e.DrawText();
  }

  [DllImport("user32.dll", SetLastError = true)]
  private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool VirtualProtectEx(
    IntPtr hProcess,
    UIntPtr lpAddress,
    IntPtr dwSize,
    MysteriousMem.Mysterious.MemoryProtection flNewProtect,
    out MysteriousMem.Mysterious.MemoryProtection lpflOldProtect);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool ReadProcessMemory(
    IntPtr hProcess,
    UIntPtr lpBaseAddress,
    [Out] byte[] lpBuffer,
    UIntPtr nSize,
    out IntPtr lpNumberOfBytesRead);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool WriteProcessMemory(
    IntPtr hProcess,
    UIntPtr lpBaseAddress,
    byte[] lpBuffer,
    UIntPtr nSize,
    IntPtr lpNumberOfBytesWritten);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern IntPtr OpenProcess(
    uint dwDesiredAccess,
    bool bInheritHandle,
    int dwProcessId);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool CloseHandle(IntPtr hObject);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool VirtualProtectEx(
    IntPtr hProcess,
    IntPtr lpAddress,
    UIntPtr dwSize,
    uint flNewProtect,
    out uint lpflOldProtect);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern IntPtr VirtualAllocEx(
    IntPtr hProcess,
    IntPtr lpAddress,
    UIntPtr dwSize,
    uint flAllocationType,
    uint flProtect);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool VirtualFreeEx(
    IntPtr hProcess,
    IntPtr lpAddress,
    UIntPtr dwSize,
    uint dwFreeType);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern IntPtr CreateRemoteThread(
    IntPtr hProcess,
    IntPtr lpThreadAttributes,
    UIntPtr dwStackSize,
    IntPtr lpStartAddress,
    IntPtr lpParameter,
    uint dwCreationFlags,
    out IntPtr lpThreadId);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern uint WaitForSingleObject(
    IntPtr hHandle,
    uint dwMilliseconds);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool GetExitCodeThread(
    IntPtr hThread,
    out uint lpExitCode);

  // Constantes para acesso ao processo
  private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
  private const uint PROCESS_VM_READ = 0x0010;
  private const uint PROCESS_VM_WRITE = 0x0020;
  private const uint PROCESS_VM_OPERATION = 0x0008;
  private const uint PROCESS_QUERY_INFORMATION = 0x0400;
  
  // Constantes para proteção de memória
  private const uint PAGE_EXECUTE_READWRITE = 0x40;
  private const uint PAGE_READWRITE = 0x04;
  private const uint PAGE_EXECUTE_READ = 0x20;
  
  // Constantes para alocação de memória
  private const uint MEM_COMMIT = 0x1000;
  private const uint MEM_RESERVE = 0x2000;
  private const uint MEM_RELEASE = 0x8000;

  // Método para injeção direta de hex sem CMD/shell
  private bool InjectHexDirectly(Process targetProcess, string hexPattern, string replacementHex)
  {
    try
    {
      int processId = targetProcess.Id;
      Aimbot.Hello.OpenProcess(processId);
      
      string str1 = "A0 42 00 00 C0 3F 33 33 13 40 00 00 F0 3F 00 00 80 3F";
      string str2 = "A0 42 00 00 C0 3F 33 33 13 40 00 00 F0 3F 00 00 29 5C";
      string bytePattern = hexPattern;
      string valorNovo = replacementHex;
      
      IEnumerable<long> source = Aimbot.Hello.AoBScan(bytePattern, true, true).Result;
      if (source == null || !source.Any<long>())
      {
        string wildcardPattern = "A0 42 ?? ?? C0 3F 33 33 13 40 ?? ?? F0 3F ?? ?? ?? ??";
        source = Aimbot.Hello.AoBScan(wildcardPattern, true, true).Result;
        
        if (source == null || !source.Any<long>())
        {
          return false;
        }
      }
      
      int successCount = 0;
      foreach (long address in source)
      {
        try
        {
          int patternLength = valorNovo.Split(' ').Length;
          byte[] readTest = new byte[patternLength];
          IntPtr bytesRead;
          bool readSuccess = ReadProcessMemory(
            Aimbot.Hello.pHandle,
            new UIntPtr((ulong)address),
            readTest,
            new UIntPtr((ulong)patternLength),
            out bytesRead
          );
          
          if (readSuccess && bytesRead.ToInt32() == patternLength)
          {
            byte[] writeBytes = HexStringToByteArray(valorNovo);
            
            IntPtr bytesWritten = IntPtr.Zero;
            bool writeOk = WriteProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)address),
              writeBytes,
              new UIntPtr((ulong)writeBytes.Length),
              bytesWritten
            );
            
            if (writeOk)
            {
              successCount++;
            }
          }
        }
        catch
        {
        }
      }
      
      return successCount > 0;
    }
    catch
    {
      return false;
    }
  }

  // Método para buscar padrão na memória do processo
  private List<long> FindPatternInMemory(IntPtr processHandle, byte[] pattern)
  {
    var addresses = new List<long>();
    
    try
    {
      // Obter informações do sistema
      SYSTEM_INFO sysInfo;
      GetSystemInfo(out sysInfo);
      
      IntPtr currentAddress = sysInfo.minimumApplicationAddress;
      IntPtr maxAddress = sysInfo.maximumApplicationAddress;
      
      byte[] buffer = new byte[0x1000]; // Buffer de 4KB
      
      while (currentAddress.ToInt64() < maxAddress.ToInt64())
      {
        // Ler página de memória
        IntPtr bytesRead;
        if (ReadProcessMemory(processHandle, new UIntPtr((ulong)currentAddress.ToInt64()), buffer, new UIntPtr(0x1000), out bytesRead))
        {
          // Buscar padrão no buffer
          for (int i = 0; i <= buffer.Length - pattern.Length; i++)
          {
            bool found = true;
            for (int j = 0; j < pattern.Length; j++)
            {
              if (buffer[i + j] != pattern[j])
              {
                found = false;
                break;
              }
            }
            
            if (found)
            {
              addresses.Add(currentAddress.ToInt64() + i);
            }
          }
        }
        
        currentAddress = new IntPtr(currentAddress.ToInt64() + 0x1000);
      }
    }
    catch
    {
      // Erro silencioso
    }
    
    return addresses;
  }

  // Método para injeção de DLL sem CMD/shell
  private bool InjectDLLDirectly(Process targetProcess, string dllPath)
  {
    try
    {
      IntPtr processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcess.Id);
      if (processHandle == IntPtr.Zero)
        return false;

      try
      {
        // Alocar memória no processo alvo
        IntPtr allocatedMemory = VirtualAllocEx(processHandle, IntPtr.Zero, new UIntPtr((uint)dllPath.Length + 1), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        if (allocatedMemory == IntPtr.Zero)
          return false;

        // Escrever caminho da DLL na memória alocada
        byte[] dllPathBytes = System.Text.Encoding.ASCII.GetBytes(dllPath);
        IntPtr bytesWritten = IntPtr.Zero;
        if (!WriteProcessMemory(processHandle, new UIntPtr((ulong)allocatedMemory.ToInt64()), dllPathBytes, new UIntPtr((uint)dllPathBytes.Length), bytesWritten))
          return false;

        // Obter endereço de LoadLibraryA
        IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
        if (loadLibraryAddr == IntPtr.Zero)
          return false;

        // Criar thread remota para executar LoadLibraryA
        IntPtr threadId;
        IntPtr threadHandle = CreateRemoteThread(processHandle, IntPtr.Zero, UIntPtr.Zero, loadLibraryAddr, allocatedMemory, 0, out threadId);
        if (threadHandle == IntPtr.Zero)
          return false;

        // Aguardar execução da thread
        WaitForSingleObject(threadHandle, 5000); // 5 segundos timeout

        // Obter código de saída
        uint exitCode;
        GetExitCodeThread(threadHandle, out exitCode);

        // Limpar recursos
        CloseHandle(threadHandle);
        VirtualFreeEx(processHandle, allocatedMemory, UIntPtr.Zero, MEM_RELEASE);

        return exitCode != 0;
      }
      finally
      {
        CloseHandle(processHandle);
      }
    }
    catch
    {
      return false;
    }
  }

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern IntPtr GetModuleHandle(string lpModuleName);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

  [StructLayout(LayoutKind.Sequential)]
  private struct SYSTEM_INFO
  {
    public ushort wProcessorArchitecture;
    public ushort wReserved;
    public uint dwPageSize;
    public IntPtr lpMinimumApplicationAddress;
    public IntPtr lpMaximumApplicationAddress;
    public IntPtr dwActiveProcessorMask;
    public uint dwNumberOfProcessors;
    public uint dwProcessorType;
    public uint dwAllocationGranularity;
    public ushort wProcessorLevel;
    public ushort wProcessorRevision;
    
    // Propriedades para compatibilidade
    public IntPtr minimumApplicationAddress => lpMinimumApplicationAddress;
    public IntPtr maximumApplicationAddress => lpMaximumApplicationAddress;
  }

  // Método híbrido para injeção com offsets específicos
  private bool InjectHexWithOffsets(Process targetProcess, string hexPattern, string readOffsetHex, string writeOffsetHex)
  {
    try
    {
      int processId = targetProcess.Id;
      Aimbot.Hello.OpenProcess(processId);
      
      long readOffset = Convert.ToInt64(readOffsetHex, 16);
      long writeOffset = Convert.ToInt64(writeOffsetHex, 16);
      
      IEnumerable<long> source = Aimbot.Hello.AoBScan(hexPattern, true, true).Result;
      if (source.Count<long>() != 0)
      {
        foreach (long num1 in source)
        {
          try
          {
            long key1 = num1 + writeOffset;
            byte[] bytes1 = new byte[4];
            IntPtr bytesRead1;
            bool read1Success = ReadProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)key1),
              bytes1,
              new UIntPtr(4),
              out bytesRead1
            );
            
            if (!read1Success || bytesRead1.ToInt32() != 4)
              continue;
            int int32_1 = BitConverter.ToInt32(bytes1, 0);
            this.OrginalValues1[key1] = int32_1;
            
            long key2 = num1 + readOffset;
            byte[] bytes2 = new byte[4];
            IntPtr bytesRead2;
            bool read2Success = ReadProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)key2),
              bytes2,
              new UIntPtr(4),
              out bytesRead2
            );
            
            if (!read2Success || bytesRead2.ToInt32() != 4)
              continue;
            int int32_2 = BitConverter.ToInt32(bytes2, 0);
            this.OrginalValues2[key2] = int32_2;
            
            long num2 = num1 + readOffset;
            long num3 = num1 + writeOffset;
            
            byte[] bytes3 = new byte[4];
            byte[] bytes4 = new byte[4];
            IntPtr bytesRead3, bytesRead4;
            
            bool read3Success = ReadProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)num2),
              bytes3,
              new UIntPtr(4),
              out bytesRead3
            );
            
            bool read4Success = ReadProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)num3),
              bytes4,
              new UIntPtr(4),
              out bytesRead4
            );
            
            if (!read3Success || !read4Success || bytesRead3.ToInt32() != 4 || bytesRead4.ToInt32() != 4)
              continue;
            
            int int32_3 = BitConverter.ToInt32(bytes3, 0);
            int int32_4 = BitConverter.ToInt32(bytes4, 0);
            
            byte[] writeBytes3 = BitConverter.GetBytes(int32_3);
            byte[] writeBytes4 = BitConverter.GetBytes(int32_4);
            
            IntPtr bytesWritten1 = IntPtr.Zero;
            IntPtr bytesWritten2 = IntPtr.Zero;
            bool write1 = WriteProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)num3),
              writeBytes3,
              new UIntPtr(4),
              bytesWritten1
            );
            
            bool write2 = WriteProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)num2),
              writeBytes4,
              new UIntPtr(4),
              bytesWritten2
            );
            
            if (!write1 || !write2)
              continue;
            
            byte[] bytes5 = new byte[4];
            byte[] bytes6 = new byte[4];
            IntPtr bytesRead5, bytesRead6;
            
            bool read5Success = ReadProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)key1),
              bytes5,
              new UIntPtr(4),
              out bytesRead5
            );
            
            bool read6Success = ReadProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)key2),
              bytes6,
              new UIntPtr(4),
              out bytesRead6
            );
            
            if (read5Success && bytesRead5.ToInt32() == 4)
            {
              int int32_5 = BitConverter.ToInt32(bytes5, 0);
              this.OrginalValues3[key1] = int32_5;
            }
            if (read6Success && bytesRead6.ToInt32() == 4)
            {
              int int32_6 = BitConverter.ToInt32(bytes6, 0);
              this.OrginalValues4[key2] = int32_6;
            }
          }
          catch
          {
          }
        }
        return true;
      }
      else
      {
        foreach (KeyValuePair<long, int> keyValuePair in this.OrginalValues1)
        {
          try
          {
            Aimbot.Hello.WriteMemory(keyValuePair.Key.ToString("X"), "int", keyValuePair.Value.ToString(), "", System.Text.Encoding.UTF8);
          }
          catch
          {
          }
        }
        foreach (KeyValuePair<long, int> keyValuePair in this.OrginalValues2)
        {
          try
          {
            Aimbot.Hello.WriteMemory(keyValuePair.Key.ToString("X"), "int", keyValuePair.Value.ToString(), "", System.Text.Encoding.UTF8);
          }
          catch
          {
          }
        }
        return true;
      }
    }
    catch
    {
      return false;
    }
  }

  private void Aimbot_Load(object sender, EventArgs e)
  {
  }

  private void customCheckbox6_CheckedChanged(object sender, EventArgs e)
  {
    this.OrginalValues1.Clear();
    this.OrginalValues2.Clear();
    this.OrginalValues3.Clear();
    this.OrginalValues4.Clear();
    this.status.Text = "Aimbot New inject...";
    
    Process[] processesByName = Process.GetProcessesByName("HD-Player");
    if (processesByName.Length == 0)
    {
      this.status.Text = "HD-Player não encontrado!";
      return;
    }
    
    // Usar injeção híbrida com offsets específicos
    bool success = InjectHexWithOffsets(processesByName[0], this.AimbotScan, this.headoffset, this.chestoffset);
    this.status.Text = success ? "Aimbot New ativado" : "Erro ao injetar Aimbot New";
  }

  public async Task FUNÇÃO1BTN(bool ativado)
  {
    if (ativado)
    {
      this.status.Text = "Aimbot 2x inject...";
      if (await Aimbot.FUNÇÃO1(true))
      {
        this.status.Text = "Aimbot 2x inject sucesso.";
      }
      else
      {
        this.status.Text = "Erro ao injetar.";
      }
    }
    else
    {
      this.status.Text = "Aimbot 2x desativando...";
      if (await Aimbot.FUNÇÃO1(false))
      {
        this.status.Text = "Aimbot 2x desativado.";
      }
      else
      {
        this.status.Text = "Erro ao desativar.";
      }
    }
  }

  private static Task<bool> FUNÇÃO1(bool ativar)
  {
    try
    {
      Process[] processesByName = Process.GetProcessesByName("HD-Player");
      if (processesByName.Length == 0)
        return Task.FromResult(false);

      string str1 = "A0 42 00 00 C0 3F 33 33 13 40 00 00 F0 3F 00 00 80 3F";
      string str2 = "A0 42 00 00 C0 3F 33 33 13 40 00 00 F0 3F 00 00 29 5C";
      string bytePattern = ativar ? str1 : str2;
      string valorNovo = ativar ? str2 : str1;
      
      // Usar injeção direta sem CMD/shell
      Aimbot aimbot = new Aimbot();
      bool success = aimbot.InjectHexDirectly(processesByName[0], bytePattern, valorNovo);
      
      return Task.FromResult(success);
        }
        catch (Exception)
        {
      return Task.FromResult(false);
    }
  }

  private void customCheckbox5_CheckedChanged(object sender, EventArgs e)
  {
    this.FUNÇÃO1BTN(this.customCheckbox5.Checked).Wait();
  }

  private void customCheckbox1_CheckedChanged(object sender, EventArgs e)
  {
    this.OrginalValues1.Clear();
    this.OrginalValues2.Clear();
    this.OrginalValues3.Clear();
    this.OrginalValues4.Clear();
    this.status.Text = "Aimbot legit inject...";
    
    Process[] processesByName = Process.GetProcessesByName("HD-Player");
    if (processesByName.Length == 0)
    {
      this.status.Text = "HD-Player não encontrado!";
      return;
    }
    
    // Usar injeção híbrida com offsets específicos
    bool success = InjectHexWithOffsets(processesByName[0], this.AimbotScan1, this.headoffset1, this.chestoffset1);
    this.status.Text = success ? "Aimbot legit ativado" : "Erro ao injetar Aimbot legit";
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
    this.txt1 = new Txt();
    this.guna2Panel1 = new Guna2Panel();
    this.txt2 = new Txt();
    this.customCheckbox6 = new CustomCheckbox();
    this.customCheckbox5 = new CustomCheckbox();
    this.status = new Txt();
    this.guna2DragControl1 = new Guna2DragControl(this.components);
    this.customCheckbox1 = new CustomCheckbox();
    this.customCheckboxPrecision = new CustomCheckbox();
    this.customCheckboxNoRecoil = new CustomCheckbox();
    ((Control) this.guna2Panel2).SuspendLayout();
    ((Control) this.guna2Panel1).SuspendLayout();
    this.SuspendLayout();
    ((Control) this.guna2Panel2).BackColor = Color.Transparent;
    this.guna2Panel2.BorderColor = Color.FromArgb(20, 20, 21);
    this.guna2Panel2.BorderRadius = 10;
    this.guna2Panel2.BorderThickness = 1;
    ((Control) this.guna2Panel2).Controls.Add((Control) this.customCheckboxNoRecoil);
    ((Control) this.guna2Panel2).Controls.Add((Control) this.customCheckboxPrecision);
    ((Control) this.guna2Panel2).Controls.Add((Control) this.customCheckbox1);
    ((Control) this.guna2Panel2).Controls.Add((Control) this.txt1);
    this.guna2Panel2.CustomBorderColor = Color.FromArgb(20, 20, 21);
    this.guna2Panel2.CustomBorderThickness = new Padding(0, 48 /*0x30*/, 0, 0);
    ((Control) this.guna2Panel2).Location = new Point(250, 0);
    ((Control) this.guna2Panel2).Name = "guna2Panel2";
    ((Control) this.guna2Panel2).Size = new Size(238, 173);
    ((Control) this.guna2Panel2).TabIndex = 5;
    this.txt1.AutoSize = true;
    this.txt1.BackColor = Color.Transparent;
    this.txt1.Font = new Font("Microsoft Sans Serif", 14.5f);
    this.txt1.ForeColor = Color.White;
    this.txt1.HorizontalTextAlignment = Txt.HorizontalAlignment.Center;
    this.txt1.Location = new Point(7, 10);
    this.txt1.Name = "txt1";
    this.txt1.Size = new Size(50, 28);
    this.txt1.TabIndex = 0;
    this.txt1.Text = "Legit";
    this.txt1.UseCompatibleTextRendering = true;
    this.txt1.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    ((Control) this.guna2Panel1).BackColor = Color.Transparent;
    this.guna2Panel1.BorderColor = Color.FromArgb(20, 20, 21);
    this.guna2Panel1.BorderRadius = 10;
    this.guna2Panel1.BorderThickness = 1;
    ((Control) this.guna2Panel1).Controls.Add((Control) this.txt2);
    ((Control) this.guna2Panel1).Controls.Add((Control) this.customCheckbox6);
    ((Control) this.guna2Panel1).Controls.Add((Control) this.customCheckbox5);
    ((Control) this.guna2Panel1).Controls.Add((Control) this.status);
    this.guna2Panel1.CustomBorderColor = Color.FromArgb(20, 20, 21);
    this.guna2Panel1.CustomBorderThickness = new Padding(0, 48 /*0x30*/, 0, 0);
    ((Control) this.guna2Panel1).Location = new Point(3, 0);
    ((Control) this.guna2Panel1).Name = "guna2Panel1";
    ((Control) this.guna2Panel1).Size = new Size(238, 185);
    ((Control) this.guna2Panel1).TabIndex = 4;
    this.txt2.AutoSize = true;
    this.txt2.BackColor = Color.Transparent;
    this.txt2.Font = new Font("Microsoft Sans Serif", 14.5f);
    this.txt2.ForeColor = Color.White;
    this.txt2.HorizontalTextAlignment = Txt.HorizontalAlignment.Center;
    this.txt2.Location = new Point(6, 10);
    this.txt2.Name = "txt2";
    this.txt2.Size = new Size(55, 28);
    this.txt2.TabIndex = 0;
    this.txt2.Text = "Rage";
    this.txt2.UseCompatibleTextRendering = true;
    this.txt2.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    this.customCheckbox6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
    this.customCheckbox6.BorderColor = Color.Transparent;
    this.customCheckbox6.BorderRadius = 5;
    this.customCheckbox6.BorderThickness = 1.5f;
    this.customCheckbox6.Checked = false;
    this.customCheckbox6.CheckmarkColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.customCheckbox6.CheckmarkSize = 9f;
    this.customCheckbox6.FillColor = Color.Transparent;
    this.customCheckbox6.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.customCheckbox6.LabelColor = Color.White;
    this.customCheckbox6.LabelFont = new Font("Microsoft Sans Serif", 11.8f);
    this.customCheckbox6.LabelSpacing = 83;
    this.customCheckbox6.LabelText = "Aim New - Risk";
    this.customCheckbox6.Location = new Point(6, 55);
    this.customCheckbox6.Name = "customCheckbox6";
    this.customCheckbox6.Size = new Size(228, 25);
    this.customCheckbox6.TabIndex = 2;
    this.customCheckbox6.CheckedChanged += new EventHandler(this.customCheckbox6_CheckedChanged);
    this.customCheckbox5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
    this.customCheckbox5.BorderColor = Color.Transparent;
    this.customCheckbox5.BorderRadius = 5;
    this.customCheckbox5.BorderThickness = 1.5f;
    this.customCheckbox5.Checked = false;
    this.customCheckbox5.CheckmarkColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.customCheckbox5.CheckmarkSize = 9f;
    this.customCheckbox5.FillColor = Color.Transparent;
    this.customCheckbox5.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.customCheckbox5.LabelColor = Color.White;
    this.customCheckbox5.LabelFont = new Font("Microsoft Sans Serif", 11.8f);
    this.customCheckbox5.LabelSpacing = 70;
    this.customCheckbox5.LabelText = "Aim Scope - Risk";
    this.customCheckbox5.Location = new Point(6, 86);
    this.customCheckbox5.Name = "customCheckbox5";
    this.customCheckbox5.Size = new Size(228, 25);
    this.customCheckbox5.TabIndex = 3;
    this.customCheckbox5.CheckedChanged += new EventHandler(this.customCheckbox5_CheckedChanged);
    this.status.AutoSize = true;
    this.status.BackColor = Color.Transparent;
    this.status.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.status.ForeColor = Color.White;
    this.status.HorizontalTextAlignment = Txt.HorizontalAlignment.Center;
    this.status.Location = new Point(6, 161);
    this.status.Name = "status";
    this.status.Size = new Size(0, 19);
    this.status.TabIndex = 5;
    this.status.UseCompatibleTextRendering = true;
    this.status.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6;
    this.guna2DragControl1.DragStartTransparencyValue = 1.0;
    this.guna2DragControl1.TargetControl = (Control) this;
    this.guna2DragControl1.UseTransparentDrag = true;
    this.customCheckbox1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
    this.customCheckbox1.BorderColor = Color.Transparent;
    this.customCheckbox1.BorderRadius = 5;
    this.customCheckbox1.BorderThickness = 1.5f;
    this.customCheckbox1.Checked = false;
    this.customCheckbox1.CheckmarkColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.customCheckbox1.CheckmarkSize = 9f;
    this.customCheckbox1.FillColor = Color.Transparent;
    this.customCheckbox1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.customCheckbox1.LabelColor = Color.White;
    this.customCheckbox1.LabelFont = new Font("Microsoft Sans Serif", 11.8f);
    this.customCheckbox1.LabelSpacing = 124;
    this.customCheckbox1.LabelText = "Aim Legit";
    this.customCheckbox1.Location = new Point(7, 55);
    this.customCheckbox1.Name = "customCheckbox1";
    this.customCheckbox1.Size = new Size(228, 25);
    this.customCheckbox1.TabIndex = 6;
    this.customCheckbox1.CheckedChanged += new EventHandler(this.customCheckbox1_CheckedChanged);
    this.customCheckboxPrecision.Anchor = AnchorStyles.Top | AnchorStyles.Right;
    this.customCheckboxPrecision.BorderColor = Color.Transparent;
    this.customCheckboxPrecision.BorderRadius = 5;
    this.customCheckboxPrecision.BorderThickness = 1.5f;
    this.customCheckboxPrecision.Checked = false;
    this.customCheckboxPrecision.CheckmarkColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.customCheckboxPrecision.CheckmarkSize = 9f;
    this.customCheckboxPrecision.FillColor = Color.Transparent;
    this.customCheckboxPrecision.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.customCheckboxPrecision.LabelColor = Color.White;
    this.customCheckboxPrecision.LabelFont = new Font("Microsoft Sans Serif", 11.8f);
    this.customCheckboxPrecision.LabelSpacing = 124;
    this.customCheckboxPrecision.LabelText = "Precision";
    this.customCheckboxPrecision.Location = new Point(7, 85);
    this.customCheckboxPrecision.Name = "customCheckboxPrecision";
    this.customCheckboxPrecision.Size = new Size(228, 25);
    this.customCheckboxPrecision.TabIndex = 7;
    this.customCheckboxPrecision.CheckedChanged += new EventHandler(this.customCheckboxPrecision_CheckedChanged);
    this.customCheckboxNoRecoil.Anchor = AnchorStyles.Top | AnchorStyles.Right;
    this.customCheckboxNoRecoil.BorderColor = Color.Transparent;
    this.customCheckboxNoRecoil.BorderRadius = 5;
    this.customCheckboxNoRecoil.BorderThickness = 1.5f;
    this.customCheckboxNoRecoil.Checked = false;
    this.customCheckboxNoRecoil.CheckmarkColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.customCheckboxNoRecoil.CheckmarkSize = 9f;
    this.customCheckboxNoRecoil.FillColor = Color.Transparent;
    this.customCheckboxNoRecoil.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.customCheckboxNoRecoil.LabelColor = Color.White;
    this.customCheckboxNoRecoil.LabelFont = new Font("Microsoft Sans Serif", 11.8f);
    this.customCheckboxNoRecoil.LabelSpacing = 121;
    this.customCheckboxNoRecoil.LabelText = "No Recoil";
    this.customCheckboxNoRecoil.Location = new Point(7, 115);
    this.customCheckboxNoRecoil.Name = "customCheckboxNoRecoil";
    this.customCheckboxNoRecoil.Size = new Size(228, 25);
    this.customCheckboxNoRecoil.TabIndex = 8;
    this.customCheckboxNoRecoil.CheckedChanged += new EventHandler(this.customCheckboxNoRecoil_CheckedChanged);
    this.AutoScaleDimensions = new SizeF(6f, 13f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.BackColor = Color.FromArgb(15, 15, 16);
    this.Controls.Add((Control) this.guna2Panel2);
    this.Controls.Add((Control) this.guna2Panel1);
    this.Name = nameof (Aimbot);
    this.Size = new Size(496, 236);
    this.Load += new EventHandler(this.Aimbot_Load);
    ((Control) this.guna2Panel2).ResumeLayout(false);
    ((Control) this.guna2Panel2).PerformLayout();
    ((Control) this.guna2Panel1).ResumeLayout(false);
    ((Control) this.guna2Panel1).PerformLayout();
    this.ResumeLayout(false);
  }

  private void customCheckboxPrecision_CheckedChanged(object sender, EventArgs e)
  {
    try
    {
      this.status.Text = "Precision inject...";
      Process[] processesByName = Process.GetProcessesByName("HD-Player");
      if (processesByName.Length == 0 || processesByName[0].HasExited)
      {
        this.status.Text = "HD-Player não encontrado!";
        return;
      }
      int processId = processesByName[0].Id;
      if (!Aimbot.Hello.OpenProcess(processId))
      {
        this.status.Text = "Falha ao abrir processo!";
        return;
      }
      bool activate = this.customCheckboxPrecision.Checked;
      Task.Run(() => this.ApplyPrecisionValuesSync(activate)).Wait();
      this.status.Text = activate ? "Precision ativado" : "Precision desativado";
    }
    catch (Exception ex)
    {
      this.status.Text = $"Erro: {ex.Message}";
    }
  }

  private void ApplyPrecisionValuesSync(bool activate)
  {
    try
    {
      Process[] processesByName = Process.GetProcessesByName("HD-Player");
      if (processesByName.Length == 0)
      {
        this.status.Text = "HD-Player não encontrado!";
        return;
      }

      int processId = processesByName[0].Id;
      if (!Aimbot.Hello.OpenProcess(processId))
      {
        this.status.Text = "Falha ao abrir processo!";
        return;
      }

      string originalPattern = "00 00 70 41 00 00 0c 42 00 00 20 41 00 00 a0 41";
      string precisionPattern = "00 00 71 41 00 00 0f 38 00 00 72 41 00 00 47 45";
      string searchPattern = activate ? originalPattern : precisionPattern;
      string replacePattern = activate ? precisionPattern : originalPattern;
      
      IEnumerable<long> result = Aimbot.Hello.AoBScan(searchPattern, true, true).Result;
      
      if (result != null && result.Any())
      {
        int successCount = 0;
        foreach (var currentAddress in result)
        {
          try
          {
            int patternLength = replacePattern.Split(' ').Length;
            byte[] currentBytes = new byte[patternLength];
            IntPtr bytesRead;
            
            bool readSuccess = ReadProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)currentAddress),
              currentBytes,
              new UIntPtr((ulong)patternLength),
              out bytesRead
            );
            
            if (!readSuccess || bytesRead.ToInt32() != patternLength)
              continue;
            
            byte[] searchBytes = HexStringToByteArray(searchPattern);
            bool patternMatches = true;
            for (int i = 0; i < searchBytes.Length && i < currentBytes.Length; i++)
            {
              if (searchBytes[i] != currentBytes[i])
              {
                patternMatches = false;
                break;
              }
            }
            
            if (!patternMatches)
              continue;
            
            MysteriousMem.Mysterious.MemoryProtection oldProt;
            bool protChanged = VirtualProtectEx(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)currentAddress),
              new IntPtr(patternLength),
              MysteriousMem.Mysterious.MemoryProtection.ExecuteReadWrite,
              out oldProt
            );
            
            byte[] newBytes = HexStringToByteArray(replacePattern);
            
            IntPtr bytesWritten = IntPtr.Zero;
            bool writeSuccess = WriteProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)currentAddress),
              newBytes,
              new UIntPtr((ulong)newBytes.Length),
              bytesWritten
            );
            
            if (!writeSuccess)
            {
              if (protChanged)
              {
                VirtualProtectEx(
                  Aimbot.Hello.pHandle,
                  new UIntPtr((ulong)currentAddress),
                  new IntPtr(patternLength),
                  oldProt,
                  out _
                );
              }
              continue;
            }
            
            byte[] verifyBytes = new byte[patternLength];
            IntPtr verifyBytesRead;
            bool verifySuccess = ReadProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)currentAddress),
              verifyBytes,
              new UIntPtr((ulong)patternLength),
              out verifyBytesRead
            );
            
            if (verifySuccess && verifyBytesRead.ToInt32() == patternLength)
            {
              bool allBytesMatch = true;
              for (int i = 0; i < newBytes.Length && i < verifyBytes.Length; i++)
              {
                if (verifyBytes[i] != newBytes[i])
                {
                  allBytesMatch = false;
                  break;
                }
              }
              
              if (allBytesMatch)
              {
                successCount++;
              }
            }
            
            if (protChanged && oldProt != MysteriousMem.Mysterious.MemoryProtection.ExecuteReadWrite)
            {
              VirtualProtectEx(
                Aimbot.Hello.pHandle,
                new UIntPtr((ulong)currentAddress),
                new IntPtr(patternLength),
                oldProt,
                out _
              );
            }
          }
          catch
          {
          }
        }
        
        this.status.Text = successCount > 0 ? (activate ? "Precision ativado" : "Precision desativado") : "Erro ao aplicar Precision";
      }
      else
      {
        this.status.Text = "Padrão não encontrado na memória";
      }
    }
    catch (Exception ex)
    {
      this.status.Text = $"Erro: {ex.Message}";
    }
  }

  private static byte[] HexStringToByteArray(string hexString)
  {
    // Remove espaços e converte para bytes
    hexString = hexString.Replace(" ", "");
    int length = hexString.Length;
    byte[] bytes = new byte[length / 2];
    
    for (int i = 0; i < length; i += 2)
    {
      bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
    }
    
    return bytes;
  }

  private void customCheckboxNoRecoil_CheckedChanged(object sender, EventArgs e)
  {
    try
    {
      this.status.Text = "No Recoil inject...";
      
      Process[] processesByName = Process.GetProcessesByName("HD-Player");
      if (processesByName.Length == 0)
      {
        this.status.Text = "HD-Player não encontrado!";
        return;
      }
      
      int processId = processesByName[0].Id;
      
      // Verificar se o processo ainda está rodando
      if (processesByName[0].HasExited)
      {
        this.status.Text = "HD-Player foi encerrado!";
        return;
      }
      
      Aimbot.Hello.OpenProcess(processId);
      
      // Aplicar No Recoil
      this.ApplyNoRecoilValues(this.customCheckboxNoRecoil.Checked).Wait();
      
      if (this.customCheckboxNoRecoil.Checked)
      {
        this.status.Text = "No Recoil ativado";
      }
      else
      {
        this.status.Text = "No Recoil desativado";
      }
    }
    catch (Exception ex)
    {
      this.status.Text = $"Erro: {ex.Message}";
    }
  }

  private Task ApplyNoRecoilValues(bool activate)
  {
    try
    {
      Process[] processesByName = Process.GetProcessesByName("HD-Player");
      if (processesByName.Length == 0)
      {
        this.status.Text = "HD-Player não encontrado!";
        return Task.FromResult(false);
      }

      int processId = processesByName[0].Id;
      Aimbot.Hello.OpenProcess(processId);

      string searchPattern = activate ? this.NoRecoilOld : this.NoRecoilNew;
      string replacePattern = activate ? this.NoRecoilNew : this.NoRecoilOld;
      
      IEnumerable<long> result = Aimbot.Hello.AoBScan(searchPattern, true, true).Result;

      if (result != null && result.Any())
      {
        int successCount = 0;
        foreach (var currentAddress in result)
        {
          try
          {
            byte[] writeBytes = HexStringToByteArray(replacePattern);
            
            IntPtr bytesWritten = IntPtr.Zero;
            bool writeOk = WriteProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)currentAddress),
              writeBytes,
              new UIntPtr((ulong)writeBytes.Length),
              bytesWritten
            );
            
            if (writeOk)
            {
              successCount++;
            }
          }
          catch
          {
          }
        }
        
        this.status.Text = successCount > 0 ? (activate ? "No Recoil ativado" : "No Recoil desativado") : "Erro ao aplicar No Recoil";
        return Task.FromResult(successCount > 0);
      }
      else
      {
        this.status.Text = "Padrão não encontrado na memória";
        return Task.FromResult(false);
      }
    }
    catch (Exception ex)
    {
      this.status.Text = $"Erro: {ex.Message}";
      return Task.FromResult(false);
    }
  }


  // Removido: manter apenas AoBScan simples conforme solicitado

  // Removido: manter apenas AoBScan simples conforme solicitado

  // Removidos imports e escrita nativa; uso apenas AoBScan e WriteMemory

  private void customCheckbox2_CheckedChanged(object sender, EventArgs e)
  {
    // Checkbox de configuração do aimbot alterado
    if (sender is CustomCheckbox checkbox)
    {
      // Atualiza configuração do aimbot
      // Implementar lógica específica se necessário
    }
  }
}