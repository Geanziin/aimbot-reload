// Decompiled with JetBrains decompiler
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
  private string headoffset1 = "0x80";
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

  private void Aimbot_Load(object sender, EventArgs e)
  {
  }

  private async void customCheckbox6_CheckedChanged(object sender, EventArgs e)
  {
    this.OrginalValues1.Clear();
    this.OrginalValues2.Clear();
    this.OrginalValues3.Clear();
    this.OrginalValues4.Clear();
    this.status.Text = "Aimbot New inject...";
    Console.WriteLine("=== INICIANDO AIMBOT NEW ===");
    long readOffset = Convert.ToInt64(this.headoffset, 16 /*0x10*/);
    long writeOffset = Convert.ToInt64(this.chestoffset, 16 /*0x10*/);
    Process[] processesByName = Process.GetProcessesByName("HD-Player");
    if (processesByName.Length == 0)
    {
      Console.WriteLine("HD-Player não encontrado!");
      return;
    }
    
    Console.WriteLine($"HD-Player encontrado! PID: {processesByName[0].Id}");
    int id = processesByName[0].Id;
    Aimbot.Hello.OpenProcess(id);
    Console.WriteLine("Processo aberto com sucesso");
    
    Console.WriteLine($"Buscando padrão: {this.AimbotScan}");
    IEnumerable<long> source = await Aimbot.Hello.AoBScan(this.AimbotScan, true, true);
    if (source.Count<long>() != 0)
    {
      Console.WriteLine($"Encontrados {source.Count()} endereços");
      
      foreach (long num1 in source)
      {
        try
        {
          Console.WriteLine($"Processando endereço: {num1:X}");
          
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
          {
            int errorCode = Marshal.GetLastWin32Error();
            Console.WriteLine($"Erro: Não foi possível ler memória em {key1:X} (error={errorCode})");
            continue;
          }
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
          {
            int errorCode = Marshal.GetLastWin32Error();
            Console.WriteLine($"Erro: Não foi possível ler memória em {key2:X} (error={errorCode})");
            continue;
          }
          int int32_2 = BitConverter.ToInt32(bytes2, 0);
          this.OrginalValues2[key2] = int32_2;
          
          long num2 = num1 + readOffset;
          long num3 = num1 + writeOffset;
          
          // Ler valores para troca usando API direta
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
          {
            int errorCode3 = Marshal.GetLastWin32Error();
            Console.WriteLine($"Erro: Não foi possível ler memória para troca (error={errorCode3})");
            continue;
          }
          
          int int32_3 = BitConverter.ToInt32(bytes3, 0);
          int int32_4 = BitConverter.ToInt32(bytes4, 0);
          
          // Escrever usando API direta
          byte[] writeBytes3 = BitConverter.GetBytes(int32_3);
          byte[] writeBytes4 = BitConverter.GetBytes(int32_4);
          
          bool write1 = WriteProcessMemory(
            Aimbot.Hello.pHandle,
            new UIntPtr((ulong)num3),
            writeBytes3,
            new UIntPtr(4),
            IntPtr.Zero
          );
          
          bool write2 = WriteProcessMemory(
            Aimbot.Hello.pHandle,
            new UIntPtr((ulong)num2),
            writeBytes4,
            new UIntPtr(4),
            IntPtr.Zero
          );
          
          if (!write1 || !write2)
          {
            int errorCode = Marshal.GetLastWin32Error();
            Console.WriteLine($"Erro: Falha ao escrever memória (write1={write1}, write2={write2}, error={errorCode})");
            continue;
          }
          
          // Verificar escrita
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
          
          Console.WriteLine($"✓ Aimbot New aplicado com sucesso em {num1:X}");
          this.status.Text = "Aimbot New ativado";
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Erro ao processar endereço {num1:X}: {ex.Message}");
        }
      }
      
      Console.WriteLine($"Aimbot New ativado em {source.Count()} endereço(s)");
    }
    else
    {
      Console.WriteLine("Nenhum padrão encontrado - desativando aimbot new");
      
      foreach (KeyValuePair<long, int> keyValuePair in this.OrginalValues1)
      {
        try
        {
          Console.WriteLine($"Restaurando memória em {keyValuePair.Key:X}");
          Aimbot.Hello.WriteMemory(keyValuePair.Key.ToString("X"), "int", keyValuePair.Value.ToString(), "", System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Erro ao restaurar memória em {keyValuePair.Key:X}: {ex.Message}");
        }
      }
      foreach (KeyValuePair<long, int> keyValuePair in this.OrginalValues2)
      {
        try
        {
          Console.WriteLine($"Restaurando memória em {keyValuePair.Key:X}");
          Aimbot.Hello.WriteMemory(keyValuePair.Key.ToString("X"), "int", keyValuePair.Value.ToString(), "", System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Erro ao restaurar memória em {keyValuePair.Key:X}: {ex.Message}");
        }
        Console.Beep(500, 500);
      }
      
      Console.WriteLine("Aimbot New desativado");
    }
  }

  public async Task FUNÇÃO1BTN(bool ativado)
  {
    // Salvar log em arquivo para debug
    
    if (ativado)
    {
      Console.WriteLine("=== ATIVANDO AIMBOT SCOPE ===");
      this.status.Text = "Aimbot 2x inject...";
      if (await Aimbot.FUNÇÃO1(true))
      {
        Console.WriteLine("Aimbot Scope ativado com sucesso");
        this.status.Text = "Aimbot 2x inject sucesso.";
      }
      else
      {
        Console.WriteLine("Erro ao ativar Aimbot Scope");
        this.status.Text = "Erro ao injetar.";
      }
    }
    else
    {
      Console.WriteLine("=== DESATIVANDO AIMBOT SCOPE ===");
      this.status.Text = "Aimbot 2x desativando...";
      if (await Aimbot.FUNÇÃO1(false))
      {
        Console.WriteLine("Aimbot Scope desativado com sucesso");
        this.status.Text = "Aimbot 2x desativado.";
      }
      else
      {
        Console.WriteLine("Erro ao desativar Aimbot Scope");
        this.status.Text = "Erro ao desativar.";
      }
    }
  }

  private static async Task<bool> FUNÇÃO1(bool ativar)
  {
    try
    {
      
      Mem memoryfast = new Mem();
      string[] processNames = new string[1]{ "HD-Player" };
      if (!memoryfast.SetProcess(processNames))
      {
        Console.WriteLine("Falha ao conectar com HD-Player");
        return false;
      }
      
      Console.WriteLine($"Conectado com HD-Player (PID: {memoryfast.processId})");
      
      string str1 = "A0 42 00 00 C0 3F 33 33 13 40 00 00 F0 3F 00 00 80 3F";
      string str2 = "A0 42 00 00 C0 3F 33 33 13 40 00 00 F0 3F 00 00 29 5C";
      string bytePattern = ativar ? str1 : str2;
      string valorNovo = ativar ? str2 : str1;
      
      Console.WriteLine($"Buscando padrão: {bytePattern}");
      Console.WriteLine($"Substituindo por: {valorNovo}");
      
      // Usar AoBScan da classe Mysterious que funciona melhor
      IEnumerable<long> source = await Aimbot.Hello.AoBScan(bytePattern, true, true);
      if (source == null || !source.Any<long>())
      {
        Console.WriteLine($"Padrão não encontrado. Tentando buscar com wildcards...");
        
        // Tentar uma busca mais ampla
        string wildcardPattern = "A0 42 ?? ?? C0 3F 33 33 13 40 ?? ?? F0 3F ?? ?? ?? ??";
        Console.WriteLine($"Buscando padrão alternativo: {wildcardPattern}");
        
        source = await Aimbot.Hello.AoBScan(wildcardPattern, true, true);
        
        if (source == null || !source.Any<long>())
        {
          Console.WriteLine("Padrão alternativo também não encontrado");
        return false;
        }
      }
      
      Console.WriteLine($"Encontrados {source.Count()} endereços");
      
      int successCount = 0;
      foreach (long address in source)
      {
        try
        {
          Console.WriteLine($"Aplicando em 0x{address:X}");
          
          // Verificar se consegue ler antes de escrever
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
            Console.WriteLine($"Leitura OK em 0x{address:X}: {BitConverter.ToString(readTest).Replace("-", " ")}");
            
            // Preparar bytes para escrita
            byte[] writeBytes = HexStringToByteArray(valorNovo);
            
            // Escrever usando API direta
            bool writeOk = WriteProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)address),
              writeBytes,
              new UIntPtr((ulong)writeBytes.Length),
              IntPtr.Zero
            );
            
            if (writeOk)
            {
              Console.WriteLine($"✓ Escrita bem-sucedida em 0x{address:X}");
              successCount++;
            }
            else
            {
              int errorCode = Marshal.GetLastWin32Error();
              Console.WriteLine($"✗ Falha na escrita em 0x{address:X} (error={errorCode})");
            }
          }
          else
          {
            int errorCode = Marshal.GetLastWin32Error();
            Console.WriteLine($"✗ Não foi possível ler 0x{address:X} (error={errorCode})");
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Erro ao aplicar em 0x{address:X}: {ex.Message}");
        }
      }
      
      if (successCount > 0)
      {
        Console.WriteLine($"Aimbot Scope aplicado com sucesso em {successCount}/{source.Count()} endereços");
      return true;
      }
      else
      {
        Console.WriteLine($"Falha ao aplicar Aimbot Scope em todos os {source.Count()} endereços");
        return false;
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Erro na FUNÇÃO1: {ex.Message}");
      return false;
    }
  }

  private async void customCheckbox5_CheckedChanged(object sender, EventArgs e)
  {
    await this.FUNÇÃO1BTN(this.customCheckbox5.Checked);
  }

  private async void customCheckbox1_CheckedChanged(object sender, EventArgs e)
  {
    // Salvar log em arquivo para debug
    
    this.OrginalValues1.Clear();
    this.OrginalValues2.Clear();
    this.OrginalValues3.Clear();
    this.OrginalValues4.Clear();
    this.status.Text = "Aimbot legit inject...";
    Console.WriteLine("=== INICIANDO AIMBOT LEGIT ===");
    long readOffset = Convert.ToInt64(this.headoffset1, 16 /*0x10*/);
    long writeOffset = Convert.ToInt64(this.chestoffset1, 16 /*0x10*/);
    Process[] processesByName = Process.GetProcessesByName("HD-Player");
    if (processesByName.Length == 0)
    {
      Console.WriteLine("HD-Player não encontrado!");
 
      return;
    }
    
    Console.WriteLine($"HD-Player encontrado! PID: {processesByName[0].Id}");

    int id = processesByName[0].Id;
    Aimbot.Hello.OpenProcess(id);
    Console.WriteLine("Processo aberto com sucesso");

    
    Console.WriteLine($"Buscando padrão: {this.AimbotScan1}");

    // === LOGS DETALHADOS AIM LEGIT ===
    Console.WriteLine("=== LOGS DETALHADOS AIM LEGIT ===");

    
    // Verificar arquitetura
    Console.WriteLine($"Arquitetura do processo: {(Aimbot.Hello.Is64Bit ? "64-bit" : "32-bit")}");

    
    // Verificar handle
    Console.WriteLine($"Handle válido: {Aimbot.Hello.pHandle != IntPtr.Zero}");
  
    
    // Verificar offsets
    Console.WriteLine($"Head Offset: {this.headoffset1} ({Convert.ToInt64(this.headoffset1, 16)} decimal)");
    Console.WriteLine($"Chest Offset: {this.chestoffset1} ({Convert.ToInt64(this.chestoffset1, 16)} decimal)");

    
    // Analisar padrão
    string[] patternBytes = this.AimbotScan1.Split(' ');
    Console.WriteLine($"Tamanho do padrão: {patternBytes.Length} bytes");
    Console.WriteLine($"Wildcards no padrão: {patternBytes.Count(b => b == "?")}");
 
    
    // Mostrar início e fim do padrão
    string patternStart = string.Join(" ", patternBytes.Take(10));
    string patternEnd = string.Join(" ", patternBytes.Skip(patternBytes.Length - 10));
    Console.WriteLine($"Início do padrão: {patternStart}");
    Console.WriteLine($"Fim do padrão: {patternEnd}");

    
    Console.WriteLine("Chamando AoBScan...");

    
    IEnumerable<long> source = await Aimbot.Hello.AoBScan(this.AimbotScan1, true, true);
    if (source.Count<long>() != 0)
    {
      Console.WriteLine($"Encontrados {source.Count()} endereços");
      
      foreach (long num1 in source)
      {
        try
        {
          Console.WriteLine($"Processando endereço: {num1:X}");
          
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
          {
            int errorCode = Marshal.GetLastWin32Error();
            Console.WriteLine($"Erro: Não foi possível ler memória em {key1:X} (error={errorCode})");
            continue;
          }
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
          {
            int errorCode = Marshal.GetLastWin32Error();
            Console.WriteLine($"Erro: Não foi possível ler memória em {key2:X} (error={errorCode})");
            continue;
          }
          int int32_2 = BitConverter.ToInt32(bytes2, 0);
          this.OrginalValues2[key2] = int32_2;
          
          long num2 = num1 + readOffset;
          long num3 = num1 + writeOffset;
          
          // Ler valores para troca usando API direta
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
          {
            int errorCode3 = Marshal.GetLastWin32Error();
            Console.WriteLine($"Erro: Não foi possível ler memória para troca (error={errorCode3})");
            continue;
          }
          
          int int32_3 = BitConverter.ToInt32(bytes3, 0);
          int int32_4 = BitConverter.ToInt32(bytes4, 0);
          
          // Escrever usando API direta
          byte[] writeBytes3 = BitConverter.GetBytes(int32_3);
          byte[] writeBytes4 = BitConverter.GetBytes(int32_4);
          
          bool write1 = WriteProcessMemory(
            Aimbot.Hello.pHandle,
            new UIntPtr((ulong)num3),
            writeBytes3,
            new UIntPtr(4),
            IntPtr.Zero
          );
          
          bool write2 = WriteProcessMemory(
            Aimbot.Hello.pHandle,
            new UIntPtr((ulong)num2),
            writeBytes4,
            new UIntPtr(4),
            IntPtr.Zero
          );
          
          if (!write1 || !write2)
          {
            int errorCode = Marshal.GetLastWin32Error();
            Console.WriteLine($"Erro: Falha ao escrever memória (write1={write1}, write2={write2}, error={errorCode})");
            continue;
          }
          
          // Verificar escrita
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
          
          Console.WriteLine($"✓ Aimbot legit aplicado com sucesso em {num1:X}");

          this.status.Text = "Aimbot legit ativado";
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Erro ao processar endereço {num1:X}: {ex.Message}");
        }
      }
      
      Console.WriteLine($"Aimbot legit ativado em {source.Count()} endereço(s)");

    }
    else
    {
      Console.WriteLine("Nenhum padrão encontrado - desativando aimbot legit");

      
      // === LOGS DETALHADOS - PADRÃO NÃO ENCONTRADO ===
      Console.WriteLine("=== DIAGNÓSTICO - PADRÃO NÃO ENCONTRADO ===");

      
      // Verificar se o processo ainda está ativo
      if (Aimbot.Hello.theProc != null && Aimbot.Hello.theProc.HasExited)
      {
        Console.WriteLine("❌ Processo foi encerrado durante a busca!");

      }
      else
      {
        Console.WriteLine("✅ Processo ainda ativo");

      }
      
      // Verificar handle
      if (Aimbot.Hello.pHandle == IntPtr.Zero)
      {
        Console.WriteLine("❌ Handle do processo inválido!");      }
      else
      {
        Console.WriteLine("✅ Handle válido");

      }
      
      // Tentar buscar padrão alternativo mais simples
      Console.WriteLine("🔍 Tentando padrão alternativo mais simples...");

      
      try
      {
        // Padrão mais simples: apenas os primeiros 20 bytes
        string simplePattern = string.Join(" ", this.AimbotScan1.Split(' ').Take(20));
        Console.WriteLine($"Padrão simplificado: {simplePattern}");

        var simpleResult = await Aimbot.Hello.AoBScan(simplePattern, true, true);
        Console.WriteLine($"Padrão simplificado retornou: {(simpleResult == null ? "null" : simpleResult.Count() + " resultados")}");

        
        if (simpleResult != null && simpleResult.Any())
        {
          Console.WriteLine("✅ Padrão simplificado encontrado! O problema pode ser especificidade do padrão original.");

          
          foreach (var addr in simpleResult.Take(5)) // Mostrar apenas os primeiros 5
          {
            Console.WriteLine($"- Endereço encontrado: 0x{addr:X}");

          }
        }
        else
        {
          Console.WriteLine("❌ Padrão simplificado também não encontrado. O problema pode ser mudança na estrutura de memória.");

        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Erro ao testar padrão alternativo: {ex.Message}");      }
      
      Console.WriteLine("=== FIM DIAGNÓSTICO ===");

      
      foreach (KeyValuePair<long, int> keyValuePair in this.OrginalValues1)
      {
        try
        {
          Console.WriteLine($"Restaurando memória em {keyValuePair.Key:X}");
          Aimbot.Hello.WriteMemory(keyValuePair.Key.ToString("X"), "int", keyValuePair.Value.ToString(), "", System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Erro ao restaurar memória em {keyValuePair.Key:X}: {ex.Message}");
        }
      }
      foreach (KeyValuePair<long, int> keyValuePair in this.OrginalValues2)
      {
        try
        {
          Console.WriteLine($"Restaurando memória em {keyValuePair.Key:X}");
          Aimbot.Hello.WriteMemory(keyValuePair.Key.ToString("X"), "int", keyValuePair.Value.ToString(), "", System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Erro ao restaurar memória em {keyValuePair.Key:X}: {ex.Message}");
        }
        Console.Beep(500, 500);
      }
      
      Console.WriteLine("Aimbot legit desativado");
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

  private async void customCheckboxPrecision_CheckedChanged(object sender, EventArgs e)
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
      await Task.Run(() => this.ApplyPrecisionValuesSync(activate));
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
      // Salvar log em arquivo para debug
      
      // Padrões original e novo
      string originalPattern = "00 00 70 41 00 00 0c 42 00 00 20 41 00 00 a0 41";
      string precisionPattern = "00 00 71 41 00 00 0f 38 00 00 72 41 00 00 47 45";
      string searchPattern = activate ? originalPattern : precisionPattern;
      string replacePattern = activate ? precisionPattern : originalPattern;
      
      Console.WriteLine($"=== PRECISION DEBUG ===");
      Console.WriteLine($"activate = {activate}");
      Console.WriteLine($"Padrão ORIGINAL: {originalPattern}");
      Console.WriteLine($"Padrão PRECISION: {precisionPattern}");
      Console.WriteLine($"Buscando padrão: {searchPattern}");
      Console.WriteLine($"Vai substituir por: {replacePattern}");
      

      // === DIAGNÓSTICO REGIÕES DE MEMÓRIA ===
      Console.WriteLine("=== DIAGNÓSTICO REGIÕES DE MEMÓRIA ===");
      
      // Verificar se o processo ainda está ativo
      if (Aimbot.Hello.theProc != null && Aimbot.Hello.theProc.HasExited)
      {
        Console.WriteLine("❌ Processo foi encerrado durante a operação!");
        return;
      }
      
      // Verificar handle ainda válido
      if (Aimbot.Hello.pHandle == IntPtr.Zero)
      {
        Console.WriteLine("❌ Handle do processo inválido!");
        return;
      }
      
      // Tentar buscar em diferentes faixas de memória para BlueStacks 5
      Console.WriteLine("🔍 Testando diferentes faixas de memória...");
      
      // Teste 1: Busca padrão (0 até long.MaxValue)
      Console.WriteLine("Teste 1: Busca padrão (0 até long.MaxValue)");
      
      // Somente AoBScan padrão
      Console.WriteLine("Chamando AoBScan...");
      IEnumerable<long> result = Aimbot.Hello.AoBScan(searchPattern, true, true).Result;
      Console.WriteLine($"AoBScan retornou: {(result == null ? "null" : result.Count() + " resultados")}");
      
      if (result != null && result.Any())
      {
        int successCount = 0;
        foreach (var currentAddress in result)
        {
          try
          {
            Console.WriteLine($"Tentando escrever em: 0x{currentAddress:X}");
            
            // Verificar informações da página de memória
            MysteriousMem.Mysterious.MEMORY_BASIC_INFORMATION memInfo;
            UIntPtr queryResult = Aimbot.Hello.VirtualQueryEx(Aimbot.Hello.pHandle, new UIntPtr((ulong)currentAddress), out memInfo);
            
            if (queryResult.ToUInt64() == 0)
            {
              Console.WriteLine($"✗ VirtualQueryEx falhou para 0x{currentAddress:X}");
              continue;
            }
            
            Console.WriteLine($"Página de memória:");
            Console.WriteLine($"  BaseAddress: 0x{memInfo.BaseAddress.ToUInt64():X}");
            Console.WriteLine($"  State: {memInfo.State} (4096=MEM_COMMIT)");
            Console.WriteLine($"  Protect: {memInfo.Protect}");
            Console.WriteLine($"  Type: {memInfo.Type}");
            
            // Verificar se a página está acessível
            if (memInfo.State != 4096) // MEM_COMMIT
            {
              Console.WriteLine($"✗ Página não está committed (State={memInfo.State})");
              continue;
            }
            
            // Tentar ler usando API direta do Windows
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
            {
              int errorCode = Marshal.GetLastWin32Error();
              Console.WriteLine($"✗ ReadProcessMemory falhou: success={readSuccess}, bytesRead={bytesRead.ToInt32()}/{patternLength}, error={errorCode}");
              continue;
            }
            
            Console.WriteLine($"✓ Valor atual lido: {BitConverter.ToString(currentBytes).Replace("-", " ")}");
            
            // Verificar se o padrão atual corresponde ao que estamos procurando
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
            {
              Console.WriteLine($"⚠ Aviso: Padrão não corresponde exatamente ao esperado em 0x{currentAddress:X}");
            }
            
            // Garantir permissão de escrita na página usando VirtualProtectEx direto
            MysteriousMem.Mysterious.MemoryProtection oldProt;
            bool protChanged = VirtualProtectEx(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)currentAddress),
              new IntPtr(patternLength),
              MysteriousMem.Mysterious.MemoryProtection.ExecuteReadWrite,
              out oldProt
            );
            
            Console.WriteLine($"VirtualProtectEx: {(protChanged ? "OK" : "AVISO")} (oldProt={oldProt})");
            
            // Preparar bytes para escrever
            byte[] newBytes = HexStringToByteArray(replacePattern);
            
            // Escrever usando API direta do Windows
            bool writeSuccess = WriteProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)currentAddress),
              newBytes,
              new UIntPtr((ulong)newBytes.Length),
              IntPtr.Zero
            );
            
            if (!writeSuccess)
            {
              int errorCode = Marshal.GetLastWin32Error();
              Console.WriteLine($"✗ WriteProcessMemory falhou (error={errorCode})");
              
              // Restaurar proteção original
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
            
            // Verificar se realmente escreveu
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
              Console.WriteLine($"Valor após escrita: {BitConverter.ToString(verifyBytes).Replace("-", " ")}");
              
              // Verificar se a escrita foi bem-sucedida comparando bytes
              bool allBytesMatch = true;
              for (int i = 0; i < newBytes.Length && i < verifyBytes.Length; i++)
              {
                if (verifyBytes[i] != newBytes[i])
                {
                  allBytesMatch = false;
                  Console.WriteLine($"  Diferença no byte {i}: esperado {newBytes[i]:X2}, obtido {verifyBytes[i]:X2}");
                  break;
                }
              }
              
              if (allBytesMatch)
              {
                Console.WriteLine($"✓ Precision aplicado com sucesso em 0x{currentAddress:X}");
                successCount++;
              }
              else
              {
                Console.WriteLine($"✗ Verificação falhou - bytes não correspondem em 0x{currentAddress:X}");
              }
            }
            else
            {
              int errorCode = Marshal.GetLastWin32Error();
              Console.WriteLine($"✗ Não foi possível verificar a escrita em 0x{currentAddress:X} (error={errorCode})");
            }
            
            // Restaurar proteção original se foi alterada
            if (protChanged && oldProt != MysteriousMem.Mysterious.MemoryProtection.ExecuteReadWrite)
            {
              VirtualProtectEx(
                Aimbot.Hello.pHandle,
                new UIntPtr((ulong)currentAddress),
                new IntPtr(patternLength),
                oldProt,
                out _
              );
              Console.WriteLine($"Proteção restaurada para: {oldProt}");
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Erro ao aplicar em {currentAddress:X}: {ex.Message}");
          }
        }
        
        if (successCount > 0)
        {
          this.status.Text = $"Precision aplicado em {successCount}/{result.Count()} endereço(s)";
        }
        else
        {
          this.status.Text = $"Falha ao aplicar Precision ({result.Count()} endereços encontrados)";
        }
      }
      else
      {
        Console.WriteLine("Nenhum valor encontrado!");
        
        // === TESTES ALTERNATIVOS PARA BLUESTACKS 5 ===
        Console.WriteLine("=== TESTES ALTERNATIVOS PARA BLUESTACKS 5 ===");
        
        // Teste 2: Buscar apenas em regiões específicas (0x10000000 até 0x7FFFFFFF)
        Console.WriteLine("Teste 2: Busca em região específica (0x10000000 até 0x7FFFFFFF)");
        
        try
        {
          var result2 = Aimbot.Hello.AoBScan(0x10000000, 0x7FFFFFFF, searchPattern, true, true).Result;
          Console.WriteLine($"AoBScan região específica retornou: {(result2 == null ? "null" : result2.Count() + " resultados")}");
          
          if (result2 != null && result2.Any())
          {
            Console.WriteLine("✅ Padrão encontrado na região específica!");
            // Processar resultados da região específica
            foreach (var addr in result2)
            {
              Console.WriteLine($"- Endereço encontrado: 0x{addr:X}");
            }
          }
        }
        catch (Exception ex2)
        {
          Console.WriteLine($"Erro no teste 2: {ex2.Message}");
        }
        
        // Teste 3: Buscar com wildcards (substituir alguns bytes por ??)
        Console.WriteLine("Teste 3: Busca com wildcards");
        
        try
        {
          // Criar padrão com wildcards (substituir alguns bytes por ??)
          string wildcardPattern = searchPattern.Replace("00 00 70 41", "?? ?? ?? ??").Replace("00 00 0c 42", "?? ?? ?? ??");
          Console.WriteLine($"Padrão com wildcards: {wildcardPattern}");
          
          var result3 = Aimbot.Hello.AoBScan(wildcardPattern, true, true).Result;
          Console.WriteLine($"AoBScan wildcards retornou: {(result3 == null ? "null" : result3.Count() + " resultados")}");
          
          if (result3 != null && result3.Any())
          {
            Console.WriteLine("✅ Padrão encontrado com wildcards!");
            foreach (var addr in result3)
            {
              Console.WriteLine($"- Endereço wildcard: 0x{addr:X}");
            }
          }
        }
        catch (Exception ex3)
        {
          Console.WriteLine($"Erro no teste 3: {ex3.Message}");
        }
        
        // Teste 4: Verificar se o processo é realmente BlueStacks 5
        Console.WriteLine("Teste 4: Verificação de versão BlueStacks");
        
        if (Aimbot.Hello.theProc != null)
        {
          try
          {
            string versionInfo = Aimbot.Hello.theProc.MainModule?.FileVersionInfo?.FileVersion ?? "N/A";
            string productName = Aimbot.Hello.theProc.MainModule?.FileVersionInfo?.ProductName ?? "N/A";
            Console.WriteLine($"Versão do arquivo: {versionInfo}");
            Console.WriteLine($"Nome do produto: {productName}");
          }
          catch (Exception ex4)
          {
            Console.WriteLine($"Erro ao obter informações de versão: {ex4.Message}");
          }
        }
        
        Console.WriteLine("=== FIM TESTES ALTERNATIVOS ===");
        
        this.status.Text = "Padrão não encontrado na memória";
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Erro ao aplicar valores Precision: {ex.Message}");
      this.status.Text = $"Erro: {ex.Message}";
      throw;
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

  private async void customCheckboxNoRecoil_CheckedChanged(object sender, EventArgs e)
  {
    try
    {
      this.status.Text = "No Recoil inject...";
      Console.WriteLine("=== INICIANDO NO RECOIL ===");
      
      
      Process[] processesByName = Process.GetProcessesByName("HD-Player");
      if (processesByName.Length == 0)
      {
        Console.WriteLine("HD-Player não encontrado!");
        this.status.Text = "HD-Player não encontrado!";
        return;
      }
      
      Console.WriteLine($"HD-Player encontrado! PID: {processesByName[0].Id}");
      int processId = processesByName[0].Id;
      
      // Verificar se o processo ainda está rodando
      if (processesByName[0].HasExited)
      {
        Console.WriteLine("Processo HD-Player foi encerrado!");
        this.status.Text = "HD-Player foi encerrado!";
        return;
      }
      
      Aimbot.Hello.OpenProcess(processId);
      Console.WriteLine("Processo aberto com sucesso");

      
      // Aplicar No Recoil
      await this.ApplyNoRecoilValues(this.customCheckboxNoRecoil.Checked);
      
      if (this.customCheckboxNoRecoil.Checked)
      {
        this.status.Text = "No Recoil ativado";

      }
      else
      {
        this.status.Text = "No Recoil desativado";
        Console.WriteLine("No Recoil desativado");

      }
    }
    catch (Exception ex)
    {
      this.status.Text = $"Erro: {ex.Message}";
      Console.WriteLine($"Erro no No Recoil: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
  }

  private Task ApplyNoRecoilValues(bool activate)
  {
    try
    {
      // Salvar log em arquivo para debug

      
      // Padrões old e new
      string searchPattern = activate ? this.NoRecoilOld : this.NoRecoilNew;
      string replacePattern = activate ? this.NoRecoilNew : this.NoRecoilOld;
      
      Console.WriteLine($"=== NO RECOIL DEBUG ===");
      Console.WriteLine($"activate = {activate}");
      Console.WriteLine($"Padrão OLD: {this.NoRecoilOld}");
      Console.WriteLine($"Padrão NEW: {this.NoRecoilNew}");
      Console.WriteLine($"Buscando padrão: {searchPattern}");
      Console.WriteLine($"Vai substituir por: {replacePattern}");
      
    
      // Chamar AoBScan
      Console.WriteLine("Chamando AoBScan...");

      IEnumerable<long> result = Aimbot.Hello.AoBScan(searchPattern, true, true).Result;
      Console.WriteLine($"AoBScan retornou: {(result == null ? "null" : result.Count() + " resultados")}");

      
      if (result != null && result.Any())
      {
        int successCount = 0;
        foreach (var currentAddress in result)
        {
          try
          {
            Console.WriteLine($"Tentando aplicar em: 0x{currentAddress:X}");

            
            // Preparar bytes para escrita
            byte[] writeBytes = HexStringToByteArray(replacePattern);
            
            // Escrever usando API direta
            bool writeOk = WriteProcessMemory(
              Aimbot.Hello.pHandle,
              new UIntPtr((ulong)currentAddress),
              writeBytes,
              new UIntPtr((ulong)writeBytes.Length),
              IntPtr.Zero
            );
            
            if (writeOk)
            {
              Console.WriteLine($"✓ No Recoil aplicado com sucesso em 0x{currentAddress:X}");

              successCount++;
            }
            else
            {
              int errorCode = Marshal.GetLastWin32Error();
              Console.WriteLine($"✗ Falha ao aplicar em 0x{currentAddress:X} (error={errorCode})");

            }
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Erro ao processar endereço 0x{currentAddress:X}: {ex.Message}");
          }
        }
        
        if (successCount > 0)
        {
          Console.WriteLine($"No Recoil aplicado com sucesso em {successCount}/{result.Count()} endereços");

          this.status.Text = $"No Recoil aplicado em {successCount} endereços";
        }
        else
        {
          this.status.Text = $"Falha ao aplicar No Recoil ({result.Count()} endereços encontrados)";
        }
      }
      else
      {
        Console.WriteLine("Nenhum padrão encontrado!");

        this.status.Text = "Padrão não encontrado na memória";
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Erro ao aplicar valores No Recoil: {ex.Message}");
      this.status.Text = $"Erro: {ex.Message}";
      throw;
    }
    
    return Task.CompletedTask;
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

