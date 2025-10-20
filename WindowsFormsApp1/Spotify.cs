// Decompiled with JetBrains decompiler
// Type: WindowsFormsApp1.Spotify
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using CuoreUI.Components;
using CustomControls;
using Guna.UI2.WinForms;
using svchost.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using WindowsFormsApp1.Properties;

#nullable disable
namespace WindowsFormsApp1;

public class Spotify : Form
{
  private const uint PROCESS_CREATE_THREAD = 2;
  private const uint PROCESS_QUERY_INFORMATION = 1024 /*0x0400*/;
  private const uint PROCESS_VM_OPERATION = 8;
  private const uint PROCESS_VM_WRITE = 32 /*0x20*/;
  private const uint PROCESS_VM_READ = 16 /*0x10*/;
  private const uint MEM_COMMIT = 4096 /*0x1000*/;
  private const uint MEM_RESERVE = 8192 /*0x2000*/;
  private const uint PAGE_READWRITE = 4;
  private bool isDragging;
  private Point lastCursor;
  private Point lastForm;
  private const uint WDA_NONE = 0;
  private const uint WDA_MONITOR = 1;
  private IContainer components;
  private cuiFormRounder cuiFormRounder1;
  private PictureBox pictureBox2;
  private Guna2Panel guna2Panel2;
  private Txt txt3;
  private Txt txt2;
  private Txt txt1;
  private Guna2Panel guna2Panel1;
  private PictureBox pictureBox1;
  private YinYangSpinner yinYangSpinner1;
  private AnimatedButton animatedButton2;
  private Guna2DragControl guna2DragControl1;
  private Main main1;
  private Guna2TextBox txtUserId;
  private Txt txtEnterUserKey;

  [DllImport("kernel32.dll")]
  private static extern IntPtr OpenProcess(
    uint dwDesiredAccess,
    bool bInheritHandle,
    int dwProcessId);

  [DllImport("kernel32.dll")]
  private static extern IntPtr GetModuleHandle(string lpModuleName);

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

  [DllImport("user32.dll")]
  private static extern bool SetWindowDisplayAffinity(IntPtr hWnd, uint dwAffinity);

  public Spotify()
  {
    this.InitializeComponent();
    this.main1.StreamingChanged += new EventHandler<bool>(this.Main1_StreamingChanged);
    this.AddDragEventsToControls((Control) this);
    this.position_fora_instant();
    this.hide_controls();
    
    // Inicializar KeyAuth da mesma forma que o projeto de bypass
    api.KeyAuthApp.init();
    
    // Verificar privilégios de administrador
    this.CheckAdminPrivileges();
  }

  private void CheckAdminPrivileges()
  {
    try
    {
      System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
      System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
      bool isAdmin = principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
      
      if (!isAdmin)
      {
        DialogResult result = MessageBox.Show(
          "Esta aplicação requer privilégios de administrador para funcionar corretamente.\n\n" +
          "Deseja reiniciar como administrador?",
          "Privilégios de Administrador",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Warning
        );
        
        if (result == DialogResult.Yes)
        {
          this.RestartAsAdmin();
        }
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Erro ao verificar privilégios: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }

  private void RestartAsAdmin()
  {
    try
    {
      ProcessStartInfo startInfo = new ProcessStartInfo
      {
        FileName = Application.ExecutablePath,
        UseShellExecute = true,
        Verb = "runas"
      };
      
      Process.Start(startInfo);
      Application.Exit();
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Erro ao reiniciar como administrador: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }

  private void AddDragEventsToControls(Control container)
  {
    foreach (Control control in (ArrangedElementCollection) container.Controls)
      ;
  }

  private void Control_MouseDown(object sender, MouseEventArgs e)
  {
    if (e.Button != MouseButtons.Left)
      return;
    this.isDragging = true;
    this.lastCursor = Cursor.Position;
    this.lastForm = this.Location;
  }

  private void Control_MouseMove(object sender, MouseEventArgs e)
  {
    if (!this.isDragging)
      return;
    Point position = Cursor.Position;
    int num1 = position.X - this.lastCursor.X;
    position = Cursor.Position;
    int num2 = position.Y - this.lastCursor.Y;
    this.Location = new Point(this.lastForm.X + num1, this.lastForm.Y + num2);
  }

  private void Control_MouseUp(object sender, MouseEventArgs e)
  {
    if (e.Button != MouseButtons.Left)
      return;
    this.isDragging = false;
  }

  public void hide_controls()
  {
    ((Control) this.guna2Panel1).Hide();
    ((Control) this.guna2Panel2).Hide();
    this.animatedButton2.Hide();
    this.yinYangSpinner1.Hide();
    this.main1.Hide();
  }

  public void position_fora()
  {
    this.guna2Panel2.Location = new Point(138, -154);
    this.animatedButton2.Location = new Point(186, 483);
    this.guna2Panel1.Location = new Point(-110, 0);
    this.guna2Panel2.BringToFront();
    this.animatedButton2.BringToFront();
    this.guna2Panel1.BringToFront();
    this.guna2Panel2.Show();
    this.animatedButton2.Show();
    this.guna2Panel1.Show();
  }

  public void position_dentro()
  {
    this.guna2Panel1.Location = new Point(0, 0);
    this.guna2Panel2.Location = new Point(138, 44);
    this.animatedButton2.Location = new Point(186, 218);
    this.yinYangSpinner1.Location = new Point(184, -200);
    this.guna2Panel1.BringToFront();
    this.guna2Panel2.BringToFront();
    this.animatedButton2.BringToFront();
    this.yinYangSpinner1.BringToFront();
    this.guna2Panel1.Show();
    this.guna2Panel2.Show();
    this.animatedButton2.Show();
    this.yinYangSpinner1.Show();
  }

  public void position_fora_instant()
  {
    this.guna2Panel2.Location = new Point(138, -154);
    this.animatedButton2.Location = new Point(186, 483);
    this.guna2Panel1.Location = new Point(-110, 0);
    this.guna2Panel2.BringToFront();
    this.animatedButton2.BringToFront();
    this.guna2Panel1.BringToFront();
    this.guna2Panel2.Show();
    this.animatedButton2.Show();
    this.guna2Panel1.Show();
  }

  public void position_dentro_instant()
  {
    this.guna2Panel1.Location = new Point(0, 0);
    this.guna2Panel2.Location = new Point(138, 44);
    this.animatedButton2.Location = new Point(186, 218);
    this.yinYangSpinner1.Location = new Point(184, -200);
    this.guna2Panel1.BringToFront();
    this.guna2Panel2.BringToFront();
    this.animatedButton2.BringToFront();
    this.yinYangSpinner1.BringToFront();
    this.guna2Panel1.Show();
    this.guna2Panel2.Show();
    this.animatedButton2.Show();
    this.yinYangSpinner1.Show();
  }


  private void Form1_Load(object sender, EventArgs e)
  {
    Spotify spotify = this;
    spotify.MouseDown += new MouseEventHandler(spotify.Control_MouseDown);
    spotify.MouseMove += new MouseEventHandler(spotify.Control_MouseMove);
    spotify.MouseUp += new MouseEventHandler(spotify.Control_MouseUp);
    
    // Garantir que os controles estejam habilitados
    spotify.animatedButton2.Enabled = true;
    spotify.yinYangSpinner1.Enabled = true;
    spotify.pictureBox2.Enabled = true;
    
    // Inicialização instantânea sem delays
    spotify.position_dentro_instant();
    spotify.animatedButton2.Focus();
  }

  private void pictureBox2_Click(object sender, EventArgs e)
  {
    try
    {
      this.Close();
      Application.Exit();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Erro no pictureBox2_Click: {ex.Message}");
      Environment.Exit(0);
    }
  }

  public void AnimacaoReverseDoBypass()
  {
    try
    {
      Spotify spotify = this;
      spotify.pictureBox2.Visible = false;
      spotify.main1.Location = new Point(0, 400);
      spotify.main1.BringToFront();
      spotify.main1.Show();
      
      // Fechamento seguro sem causar erro CLR
      spotify.Close();
      Application.Exit();
    }
    catch (Exception ex)
    {
      // Log do erro e fechamento seguro
      System.Diagnostics.Debug.WriteLine($"Erro no AnimacaoReverseDoBypass: {ex.Message}");
      try
      {
        Application.Exit();
      }
      catch
      {
        Environment.Exit(0);
      }
    }
  }

  private void animatedButton1_Click_1(object sender, EventArgs e)
  {
    // Botão Enter - Inicia a aplicação instantaneamente
    this.position_fora_instant();
    this.main1.Location = new Point(0, 0);
    this.main1.BringToFront();
    this.main1.Show();
  }

  private void Main1_StreamingChanged(object sender, bool isStreaming)
  {
    if (isStreaming)
      this.AtivarStreaming();
    else
      this.DesativarStreaming();
  }

  private void AtivarStreaming()
  {
    this.ShowInTaskbar = false; // Não mostrar na barra de tarefas
    Spotify.SetWindowDisplayAffinity(this.Handle, 17U); // Tornar a janela invisível para programas de captura
  }

  private void DesativarStreaming()
  {
    this.ShowInTaskbar = true; // Mostrar na barra de tarefas novamente
    Spotify.SetWindowDisplayAffinity(this.Handle, 0U); // Tornar a janela visível novamente
  }

  private void main1_Load(object sender, EventArgs e)
  {
  }

  protected override CreateParams CreateParams
  {
    get
    {
      CreateParams createParams = base.CreateParams;
      createParams.ExStyle |= 128 /*0x80*/;
      return createParams;
    }
  }

  private void smoothTextBox1_TextChanged(object sender, EventArgs e)
  {
  }

  private void smoothTextBox1_TextChanged_1(object sender, EventArgs e)
  {
  }

  private void pictureBox2_Click_1(object sender, EventArgs e)
  {
    try
    {
      this.Close();
      Application.Exit();
    }
    catch (Exception ex)
    {
      System.Diagnostics.Debug.WriteLine($"Erro no pictureBox2_Click_1: {ex.Message}");
      Environment.Exit(0);
    }
  }

  private void yinYangSpinner1_Click(object sender, EventArgs e)
  {
    // Spinner clicado - Inicia carregamento
    this.position_fora_instant();
    
    // Mostrar tela de carregamento (spinner com duas linhas em loop)
    this.yinYangSpinner1.Location = new Point(184, 104);
    this.yinYangSpinner1.BringToFront();
    this.yinYangSpinner1.Show();
  }

  private void animatedButton2_Click(object sender, EventArgs e)
  {
    try
    {
      // Validar se o campo de ID foi preenchido
      if (string.IsNullOrEmpty(this.txtUserId?.Text?.Trim()))
      {
        MessageBox.Show("Por favor, insira seu ID de usuário.", "Campo Obrigatório", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        this.txtUserId?.Focus();
        return;
      }

      // Desabilitar botão para evitar múltiplos cliques
      this.animatedButton2.Enabled = false;
      
      // Mostrar spinner de carregamento
      this.yinYangSpinner1.Location = new Point(184, 104);
      this.yinYangSpinner1.BringToFront();
      this.yinYangSpinner1.Show();
      
      // Processar eventos da UI
      Application.DoEvents();

      // Usar o método login() com ID como username e senha padrão "1"
      api.KeyAuthApp.login(this.txtUserId.Text.Trim(), "1"); // ID como username, senha padrão "1"
      
      if (api.KeyAuthApp.response.success)
      {
        // Autenticação bem-sucedida - prosseguir para a aplicação
        this.position_fora_instant();
        this.main1.Location = new Point(0, 0);
        this.main1.BringToFront();
        this.main1.Show();
      }
      else
      {
        // Falha na autenticação
        MessageBox.Show("Status: " + api.KeyAuthApp.response.message, "Erro de Autenticação", MessageBoxButtons.OK, MessageBoxIcon.Error);
        this.txtUserId.Focus();
      }
    }
    catch (Exception ex)
    {
      MessageBox.Show($"Erro durante a autenticação: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    finally
    {
      try
      {
        // Esconder spinner e reabilitar botão
        this.yinYangSpinner1.Hide();
        this.animatedButton2.Enabled = true;
        Application.DoEvents();
      }
      catch (Exception finallyEx)
      {
        // Ignorar erros no finally
      }
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
    ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Spotify));
    this.cuiFormRounder1 = new cuiFormRounder();
    this.guna2Panel2 = new Guna2Panel();
    this.txt2 = new Txt();
    this.txt3 = new Txt();
    this.txt1 = new Txt();
    this.txtUserId = new Guna2TextBox();
    this.txtEnterUserKey = new Txt();
    this.guna2Panel1 = new Guna2Panel();
    this.pictureBox1 = new PictureBox();
    this.pictureBox2 = new PictureBox();
    this.guna2DragControl1 = new Guna2DragControl(this.components);
    this.animatedButton2 = new AnimatedButton();
    this.yinYangSpinner1 = new YinYangSpinner();
    this.main1 = new Main();
    ((Control) this.guna2Panel2).SuspendLayout();
    ((Control) this.guna2Panel1).SuspendLayout();
    ((ISupportInitialize) this.pictureBox1).BeginInit();
    ((ISupportInitialize) this.pictureBox2).BeginInit();
    this.SuspendLayout();
    // this.cuiFormRounder1.EnhanceCorners = false; // Método não disponível na versão atual
    this.cuiFormRounder1.OutlineColor = Color.Transparent;
    this.cuiFormRounder1.Rounding = 9;
    this.cuiFormRounder1.TargetForm = (Form) this;
    ((Control) this.guna2Panel2).BackColor = Color.Transparent;
    this.guna2Panel2.BorderColor = Color.Transparent;
    this.guna2Panel2.BorderRadius = 10;
    this.guna2Panel2.BorderThickness = 1;
    ((Control) this.guna2Panel2).Controls.Add((Control) this.txt3);
    ((Control) this.guna2Panel2).Controls.Add((Control) this.txt1);
    ((Control) this.guna2Panel2).Controls.Add((Control) this.txtUserId);
    this.guna2Panel2.CustomBorderColor = Color.Transparent;
    this.guna2Panel2.CustomBorderThickness = new Padding(0, 48 /*0x30*/, 0, 0);
    ((Control) this.guna2Panel2).ForeColor = Color.Transparent;
    ((Control) this.guna2Panel2).Location = new Point(138, 97);
    ((Control) this.guna2Panel2).Name = "guna2Panel2";
    ((Control) this.guna2Panel2).Size = new Size(266, 160);
    ((Control) this.guna2Panel2).TabIndex = 13;
    this.txt3.AutoSize = true;
    this.txt3.BackColor = Color.Transparent;
    this.txt3.Font = new Font("Microsoft Sans Serif", 14.5f);
    this.txt3.ForeColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.txt3.HorizontalTextAlignment = Txt.HorizontalAlignment.Center;
    this.txt3.Location = new Point(138, 96 /*0x60*/);
    this.txt3.Name = "txt3";
    this.txt3.Size = new Size(48 /*0x30*/, 28);
    this.txt3.TabIndex = 3;
    this.txt3.Text = "Private";
    this.txt3.UseCompatibleTextRendering = true;
    this.txt3.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    this.txt1.AutoSize = true;
    this.txt1.BackColor = Color.Transparent;
    this.txt1.Font = new Font("Microsoft Sans Serif", 14.5f);
    this.txt1.ForeColor = Color.White;
    this.txt1.HorizontalTextAlignment = Txt.HorizontalAlignment.Left;
    this.txt1.Location = new Point(98, 96 /*0x60*/);
    this.txt1.Name = "txt1";
    this.txt1.Size = new Size(47, 28);
    this.txt1.TabIndex = 1;
    this.txt1.Text = "x7";
    this.txt1.UseCompatibleTextRendering = true;
    this.txt1.VerticalTextAlignment = Txt.VerticalAlignment.Middle;
    this.txtUserId.BackColor = Color.FromArgb(25, 25, 26);
    this.txtUserId.BorderColor = Color.FromArgb(147, 51, 234);
    this.txtUserId.BorderRadius = 6;
    this.txtUserId.BorderThickness = 1;
    this.txtUserId.Cursor = Cursors.IBeam;
    this.txtUserId.DefaultText = "";
    this.txtUserId.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
    this.txtUserId.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
    this.txtUserId.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
    this.txtUserId.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
    this.txtUserId.FillColor = Color.FromArgb(25, 25, 26);
    this.txtUserId.FocusedState.BorderColor = Color.FromArgb(147, 51, 234);
    this.txtUserId.Font = new Font("Microsoft Sans Serif", 10f);
    this.txtUserId.ForeColor = Color.White;
    this.txtUserId.HoverState.BorderColor = Color.FromArgb(147, 51, 234);
    this.txtUserId.Location = new Point(48, 130);
    this.txtUserId.Margin = new Padding(4, 4, 4, 4);
    this.txtUserId.Name = "txtUserId";
    this.txtUserId.PasswordChar = '\0';
    this.txtUserId.PlaceholderForeColor = Color.FromArgb(125, 125, 125);
    this.txtUserId.PlaceholderText = "";
    this.txtUserId.SelectedText = "";
    this.txtUserId.Size = new Size(170, 30);
    this.txtUserId.TabIndex = 4;
    ((Control) this.guna2Panel1).BackColor = Color.FromArgb(15, 15, 16);
    this.guna2Panel1.BorderColor = Color.Transparent;
    this.guna2Panel1.BorderThickness = 1;
    ((Control) this.guna2Panel1).Controls.Add((Control) this.pictureBox1);
    this.guna2Panel1.CustomBorderColor = Color.Transparent;
    this.guna2Panel1.CustomBorderThickness = new Padding(0, 48 /*0x30*/, 0, 0);
    ((Control) this.guna2Panel1).Location = new Point(0, 0);
    ((Control) this.guna2Panel1).Name = "guna2Panel1";
    ((Control) this.guna2Panel1).Size = new Size(80 /*0x50*/, 379);
    ((Control) this.guna2Panel1).TabIndex = 12;
    // Carregar imagem do arquivo com fallback para recurso embutido
    string imagePath = Path.Combine(Application.StartupPath, "Images", "x7_image.png");
    if (File.Exists(imagePath))
    {
      this.pictureBox1.Image = Image.FromFile(imagePath);
    }
    else
    {
      using (Stream s = typeof(Spotify).Assembly.GetManifestResourceStream("WindowsFormsApp1.Images.x7_image.png"))
      {
        if (s != null)
        {
          this.pictureBox1.Image = Image.FromStream(s);
        }
      }
    }
    this.pictureBox1.Location = new Point(3, 152);
    this.pictureBox1.Name = "pictureBox1";
    this.pictureBox1.Size = new Size(74, 75);
    this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
    this.pictureBox1.TabIndex = 0;
    this.pictureBox1.TabStop = false;
    this.pictureBox2.BackColor = Color.Transparent;
    this.pictureBox2.Image = (Image) componentResourceManager.GetObject("pictureBox2.Image");
    this.pictureBox2.Location = new Point(519, 6);
    this.pictureBox2.Name = "pictureBox2";
    this.pictureBox2.Size = new Size(16 /*0x10*/, 16 /*0x10*/);
    this.pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
    this.pictureBox2.TabIndex = 8;
    this.pictureBox2.TabStop = false;
    this.pictureBox2.Click += new EventHandler(this.pictureBox2_Click_1);
    this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6;
    this.guna2DragControl1.DragStartTransparencyValue = 1.0;
    this.guna2DragControl1.TargetControl = (Control) this.guna2Panel2;
    this.guna2DragControl1.UseTransparentDrag = true;
    this.animatedButton2.AnimationFillDirection = AnimatedButton.FillDirection.CenterOut;
    this.animatedButton2.AnimationFillStyle = AnimatedButton.FillStyle.Solid;
    this.animatedButton2.AnimationSpeed = 0.6f; // Velocidade ainda mais suave
    this.animatedButton2.BackColor = Color.Transparent;
    this.animatedButton2.BorderColor = Color.Transparent;
    this.animatedButton2.CornerRadius = 6;
    this.animatedButton2.Cursor = Cursors.Hand;
    this.animatedButton2.Font = new Font("Microsoft Sans Serif", 11.5f);
    this.animatedButton2.HoverColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.animatedButton2.InsideColor = Color.FromArgb(15, 15, 16);
    this.animatedButton2.Location = new Point(186, 277);
    this.animatedButton2.Name = "animatedButton2";
    this.animatedButton2.ShowToolTip = false;
    this.animatedButton2.Size = new Size(170, 37);
    this.animatedButton2.TabIndex = 14;
    this.animatedButton2.Text = "Enter";
    this.animatedButton2.TextColor = Color.White;
    this.animatedButton2.TextHoverColor = Color.White;
    this.animatedButton2.ToolTipIcon = "";
    this.animatedButton2.ToolTipMessage = "";
    this.animatedButton2.Click += new EventHandler(this.animatedButton2_Click);
    this.yinYangSpinner1.Angle = 2f;
    this.yinYangSpinner1.ColorI = Color.White;
    this.yinYangSpinner1.ColorY = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.yinYangSpinner1.Location = new Point(184, 104);
    this.yinYangSpinner1.Mode = 0;
    this.yinYangSpinner1.Name = "yinYangSpinner1";
    this.yinYangSpinner1.Radius = 45f;
    this.yinYangSpinner1.Reverse = false;
    this.yinYangSpinner1.Size = new Size(174, 170);
    this.yinYangSpinner1.Speed = 10f;
    this.yinYangSpinner1.TabIndex = 11;
    this.yinYangSpinner1.Text = "yinYangSpinner1";
    this.yinYangSpinner1.Thickness = 3f;
    this.yinYangSpinner1.YangDeltaRadius = 6f;
    this.yinYangSpinner1.Click += new EventHandler(this.yinYangSpinner1_Click);
    this.main1.BackColor = Color.FromArgb(15, 15, 16);
    this.main1.Location = new Point(0, 0);
    this.main1.Name = "main1";
    this.main1.Size = new Size(543, 379);
    this.main1.TabIndex = 5;
    this.main1.Load += new EventHandler(this.main1_Load);
    this.AutoScaleDimensions = new SizeF(6f, 13f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.BackColor = Color.FromArgb(15, 15, 16);
    this.ClientSize = new Size(543, 379);
    this.Controls.Add((Control) this.animatedButton2);
    this.Controls.Add((Control) this.pictureBox2);
    this.Controls.Add((Control) this.guna2Panel2);
    this.Controls.Add((Control) this.guna2Panel1);
    this.Controls.Add((Control) this.yinYangSpinner1);
    this.Controls.Add((Control) this.main1);
    this.FormBorderStyle = FormBorderStyle.None;
    this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
    this.Name = nameof (Spotify);
    this.StartPosition = FormStartPosition.CenterScreen;
    this.Load += new EventHandler(this.Form1_Load);
    ((Control) this.guna2Panel2).ResumeLayout(false);
    ((Control) this.guna2Panel2).PerformLayout();
    ((Control) this.guna2Panel1).ResumeLayout(false);
    ((ISupportInitialize) this.pictureBox1).EndInit();
    ((ISupportInitialize) this.pictureBox2).EndInit();
    this.ResumeLayout(false);
  }
}
