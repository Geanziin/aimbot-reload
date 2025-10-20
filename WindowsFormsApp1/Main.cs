// Decompiled with JetBrains decompiler
// Type: WindowsFormsApp1.Main
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using Guna.UI2.WinForms;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WindowsFormsApp1.Properties;

#nullable disable
namespace WindowsFormsApp1;

public class Main : UserControl
{
  private int currentTab;
  private IContainer components;
  private Guna2Elipse guna2Elipse1;
  private Guna2DragControl guna2DragControl1;
  private Guna2DragControl guna2DragControl2;
  private Guna2DragControl guna2DragControl3;
  private Guna2DragControl guna2DragControl4;
  private Guna2DragControl guna2DragControl5;
  private Guna2DragControl guna2DragControl6;
  public Bypass bypass1;
  private SlidingTabControl slidingTabControl1;
  private Aimbot aimbot1;
  private PictureBox pictureBox1;

  public Bypass Bypass1 => this.bypass1;

  public Main()
  {
    this.InitializeComponent();
    this.ShowTabMessageBox(this.currentTab);
    this.slidingTabControl1.SelectedIndex = this.currentTab;
    this.slidingTabControl1.SelectedIndexChanged += new EventHandler(this.SlidingTabControl1_SelectedIndexChanged);
    
    // Garantir que os controles estejam habilitados
    if (this.bypass1 != null)
    {
      this.bypass1.StreamingChanged += new EventHandler<bool>(this.Bypass1_StreamingChanged);
    }
    if (this.aimbot1 != null)
    {
      // Inicializar controles do aimbot se necessário
    }
  }

  private void Bypass1_StreamingChanged(object sender, bool isStreaming)
  {
    EventHandler<bool> streamingChanged = this.StreamingChanged;
    if (streamingChanged == null)
      return;
    streamingChanged((object) this, isStreaming);
  }

  public event EventHandler<bool> StreamingChanged;

  // Seleciona a aba principal (Combat) e posiciona os controles
  public void ShowCombatTab()
  {
    try
    {
      this.slidingTabControl1.SelectedIndex = 0;
      this.ShowTabMessageBox(0);
    }
    catch (Exception ex)
    {

    }
  }

  public void position_fora()
  {
    this.bypass1.Location = new Point(552, 22);
    this.aimbot1.Location = new Point(12, 22);
    this.bypass1.BringToFront();
    this.aimbot1.BringToFront();
    this.bypass1.Show();
    this.aimbot1.Show();
  }

  public void position_dentro()
  {
    this.aimbot1.Location = new Point(-522, 22);
    this.bypass1.Location = new Point(12, 22);
    this.bypass1.BringToFront();
    this.aimbot1.BringToFront();
    this.bypass1.Show();
    this.aimbot1.Show();
  }

  public void position_fora_instant()
  {
    this.bypass1.Location = new Point(552, 22);
    this.aimbot1.Location = new Point(12, 22);
    this.bypass1.BringToFront();
    this.aimbot1.BringToFront();
    this.bypass1.Show();
    this.aimbot1.Show();
  }

  public void position_dentro_instant()
  {
    this.aimbot1.Location = new Point(-522, 22);
    this.bypass1.Location = new Point(12, 22);
    this.bypass1.BringToFront();
    this.aimbot1.BringToFront();
    this.bypass1.Show();
    this.aimbot1.Show();
  }

  private void SlidingTabControl1_SelectedIndexChanged(object sender, EventArgs e)
  {
    this.currentTab = this.slidingTabControl1.SelectedIndex;
    this.ShowTabMessageBox(this.currentTab);
  }

  private void SlidingTabControl2_SelectedIndexChanged(object sender, EventArgs e)
  {
  }

  private void ShowTabMessageBox(int tabIndex)
  {
    try
    {
      if (tabIndex == 0)
        this.position_fora_instant();
      else if (tabIndex == 1)
        this.position_dentro_instant();
    }
    catch (Exception ex)
    {
      // Log do erro se necessário

    }
  }

  private void Main_Load(object sender, EventArgs e)
  {
    // Inicialização simples sem animações
    this.bypass1.Location = new Point(600, 87);
    this.bypass1.BringToFront();
    this.bypass1.Show();
  }

  private void slidingTabControl1_Click(object sender, EventArgs e)
  {
  }

  private void aimbot1_Load(object sender, EventArgs e)
  {
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
    this.guna2Elipse1 = new Guna2Elipse(this.components);
    this.guna2DragControl1 = new Guna2DragControl(this.components);
    this.guna2DragControl2 = new Guna2DragControl(this.components);
    this.guna2DragControl3 = new Guna2DragControl(this.components);
    this.guna2DragControl4 = new Guna2DragControl(this.components);
    this.guna2DragControl5 = new Guna2DragControl(this.components);
    this.guna2DragControl6 = new Guna2DragControl(this.components);
    this.slidingTabControl1 = new SlidingTabControl();
    this.aimbot1 = new Aimbot();
    this.bypass1 = new Bypass();
    this.pictureBox1 = new PictureBox();
    ((ISupportInitialize) this.pictureBox1).BeginInit();
    this.SuspendLayout();
    this.guna2Elipse1.BorderRadius = 75;
    this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6;
    this.guna2DragControl1.DragStartTransparencyValue = 1.0;
    this.guna2DragControl1.UseTransparentDrag = true;
    this.guna2DragControl2.DockIndicatorTransparencyValue = 0.6;
    this.guna2DragControl2.DragStartTransparencyValue = 1.0;
    this.guna2DragControl2.UseTransparentDrag = true;
    this.guna2DragControl3.DockIndicatorTransparencyValue = 0.6;
    this.guna2DragControl3.DragStartTransparencyValue = 1.0;
    this.guna2DragControl3.UseTransparentDrag = true;
    this.guna2DragControl4.DockIndicatorTransparencyValue = 0.6;
    this.guna2DragControl4.DragStartTransparencyValue = 1.0;
    this.guna2DragControl4.TargetControl = (Control) this;
    this.guna2DragControl4.UseTransparentDrag = true;
    this.guna2DragControl5.DockIndicatorTransparencyValue = 0.6;
    this.guna2DragControl5.DragStartTransparencyValue = 1.0;
    this.guna2DragControl5.UseTransparentDrag = true;
    this.guna2DragControl6.DockIndicatorTransparencyValue = 0.6;
    this.guna2DragControl6.DragStartTransparencyValue = 1.0;
    this.guna2DragControl6.UseTransparentDrag = true;
    this.slidingTabControl1.AnimationSpeed = 0.0f; // Desabilitar animação de transição
    this.slidingTabControl1.BackColor = Color.FromArgb(15, 15, 16);
    this.slidingTabControl1.BorderColor = Color.Transparent;
    this.slidingTabControl1.ColorAnimationSpeed = 0.0f; // Desabilitar animação de cor
    this.slidingTabControl1.Cursor = Cursors.Default;
    this.slidingTabControl1.Font = new Font("Microsoft Sans Serif", 12f);
    this.slidingTabControl1.ForeColor = SystemColors.ControlText;
    this.slidingTabControl1.ImagePosition = ImagePosition.Center;
    this.slidingTabControl1.ImageSize = new Size(25, 25);
    this.slidingTabControl1.Location = new Point(155, 312);
    this.slidingTabControl1.Name = "slidingTabControl1";
    this.slidingTabControl1.SelectedIndex = 0;
    this.slidingTabControl1.Size = new Size(218, 47);
    this.slidingTabControl1.SlideColor = Color.FromArgb(147, 51, 234); // Roxo moderno
    this.slidingTabControl1.Tab10BottomLeftRadius = 6;
    this.slidingTabControl1.Tab10BottomRightRadius = 6;
    this.slidingTabControl1.Tab10Image = (Image) null;
    this.slidingTabControl1.Tab10Text = "";
    this.slidingTabControl1.Tab10TopLeftRadius = 6;
    this.slidingTabControl1.Tab10TopRightRadius = 6;
    this.slidingTabControl1.Tab1BottomLeftRadius = 23;
    this.slidingTabControl1.Tab1BottomRightRadius = 0;
    this.slidingTabControl1.Tab1Image = (Image) null;
    this.slidingTabControl1.Tab1Text = "Combat";
    this.slidingTabControl1.Tab1TopLeftRadius = 23;
    this.slidingTabControl1.Tab1TopRightRadius = 0;
    this.slidingTabControl1.Tab2BottomLeftRadius = 0;
    this.slidingTabControl1.Tab2BottomRightRadius = 23;
    this.slidingTabControl1.Tab2Image = (Image) null;
    this.slidingTabControl1.Tab2Text = "Settings";
    this.slidingTabControl1.Tab2TopLeftRadius = 0;
    this.slidingTabControl1.Tab2TopRightRadius = 23;
    this.slidingTabControl1.Tab3BottomLeftRadius = 6;
    this.slidingTabControl1.Tab3BottomRightRadius = 6;
    this.slidingTabControl1.Tab3Image = (Image) null;
    this.slidingTabControl1.Tab3Text = "";
    this.slidingTabControl1.Tab3TopLeftRadius = 6;
    this.slidingTabControl1.Tab3TopRightRadius = 6;
    this.slidingTabControl1.Tab4BottomLeftRadius = 6;
    this.slidingTabControl1.Tab4BottomRightRadius = 6;
    this.slidingTabControl1.Tab4Image = (Image) null;
    this.slidingTabControl1.Tab4Text = "";
    this.slidingTabControl1.Tab4TopLeftRadius = 6;
    this.slidingTabControl1.Tab4TopRightRadius = 6;
    this.slidingTabControl1.Tab5BottomLeftRadius = 6;
    this.slidingTabControl1.Tab5BottomRightRadius = 6;
    this.slidingTabControl1.Tab5Image = (Image) null;
    this.slidingTabControl1.Tab5Text = "";
    this.slidingTabControl1.Tab5TopLeftRadius = 6;
    this.slidingTabControl1.Tab5TopRightRadius = 6;
    this.slidingTabControl1.Tab6BottomLeftRadius = 6;
    this.slidingTabControl1.Tab6BottomRightRadius = 6;
    this.slidingTabControl1.Tab6Image = (Image) null;
    this.slidingTabControl1.Tab6Text = "";
    this.slidingTabControl1.Tab6TopLeftRadius = 6;
    this.slidingTabControl1.Tab6TopRightRadius = 6;
    this.slidingTabControl1.Tab7BottomLeftRadius = 6;
    this.slidingTabControl1.Tab7BottomRightRadius = 6;
    this.slidingTabControl1.Tab7Image = (Image) null;
    this.slidingTabControl1.Tab7Text = "";
    this.slidingTabControl1.Tab7TopLeftRadius = 6;
    this.slidingTabControl1.Tab7TopRightRadius = 6;
    this.slidingTabControl1.Tab8BottomLeftRadius = 6;
    this.slidingTabControl1.Tab8BottomRightRadius = 6;
    this.slidingTabControl1.Tab8Image = (Image) null;
    this.slidingTabControl1.Tab8Text = "";
    this.slidingTabControl1.Tab8TopLeftRadius = 6;
    this.slidingTabControl1.Tab8TopRightRadius = 6;
    this.slidingTabControl1.Tab9BottomLeftRadius = 6;
    this.slidingTabControl1.Tab9BottomRightRadius = 6;
    this.slidingTabControl1.Tab9Image = (Image) null;
    this.slidingTabControl1.Tab9Text = "";
    this.slidingTabControl1.Tab9TopLeftRadius = 6;
    this.slidingTabControl1.Tab9TopRightRadius = 6;
    this.slidingTabControl1.TabCount = 2;
    this.slidingTabControl1.TabIndex = 34;
    this.slidingTabControl1.TabsFont = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
    this.slidingTabControl1.Text = "slidingTabControl1";
    this.slidingTabControl1.TextColorInactive = Color.White;
    this.aimbot1.BackColor = Color.Transparent;
    this.aimbot1.Location = new Point(12, 22);
    this.aimbot1.Name = "aimbot1";
    this.aimbot1.Size = new Size(505, 265);
    this.aimbot1.TabIndex = 33;
    this.aimbot1.Load += new EventHandler(this.aimbot1_Load);
    this.bypass1.BackColor = Color.FromArgb(15, 15, 16);
    this.bypass1.Location = new Point(12, 22);
    this.bypass1.Name = "bypass1";
    this.bypass1.Size = new Size(505, 265);
    this.bypass1.TabIndex = 32 /*0x20*/;
    // Carregar imagem do arquivo com fallback para recurso embutido
    string imagePath = Path.Combine(Application.StartupPath, "Images", "x7_image.png");
    if (File.Exists(imagePath))
    {
      this.pictureBox1.Image = Image.FromFile(imagePath);
    }
    else
    {
      using (Stream s = typeof(Main).Assembly.GetManifestResourceStream("WindowsFormsApp1.Images.x7_image.png"))
      {
        if (s != null)
        {
          this.pictureBox1.Image = Image.FromStream(s);
        }
      }
    }
    this.pictureBox1.Location = new Point(1, 302);
    this.pictureBox1.Name = "pictureBox1";
    this.pictureBox1.Size = new Size(74, 59);
    this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
    this.pictureBox1.TabIndex = 35;
    this.pictureBox1.TabStop = false;
    this.AutoScaleDimensions = new SizeF(6f, 13f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.BackColor = Color.FromArgb(15, 15, 16);
    this.Controls.Add((Control) this.pictureBox1);
    this.Controls.Add((Control) this.slidingTabControl1);
    this.Controls.Add((Control) this.aimbot1);
    this.Controls.Add((Control) this.bypass1);
    this.Name = nameof (Main);
    this.Size = new Size(528, 375);
    this.Load += new EventHandler(this.Main_Load);
    ((ISupportInitialize) this.pictureBox1).EndInit();
    this.ResumeLayout(false);
  }
}
