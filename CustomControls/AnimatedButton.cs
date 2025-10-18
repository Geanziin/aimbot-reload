// Decompiled with JetBrains decompiler
// Type: CustomControls.AnimatedButton
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#nullable disable
namespace CustomControls;

[ToolboxItem(true)]
[Description("Botão animado com efeito de preenchimento e fade")]
public class AnimatedButton : Control
{
  private AnimatedButton.ButtonAnimations animations = new AnimatedButton.ButtonAnimations();
  private Rectangle buttonRect;
  private GraphicsPath buttonPath;
  private Timer animationTimer;
  private ToolTip toolTip = new ToolTip();
  private Color insideSolidColor = Color.FromArgb((int) byte.MaxValue, 16 /*0x10*/, 16 /*0x10*/, 18);
  private Color outsideSolidColor = Color.FromArgb((int) byte.MaxValue, 24, 24, 26);
  private Color hoverColor = Color.FromArgb((int) byte.MaxValue, 107, 105, (int) byte.MaxValue);
  private Color textColor = Color.FromArgb((int) byte.MaxValue, 140, 140, 140);
  private Color textHoverColor = Color.FromArgb((int) byte.MaxValue, 20, 20, 20);
  private bool isHovered;
  private float animationSpeed = 1.5f;
  private int cornerRadius = 6;
  private int animationInterval = 10;
  private AnimatedButton.FillDirection fillDirection;
  private AnimatedButton.FillStyle fillStyle;
  private string toolTipMessage = "";
  private string toolTipIcon = "";
  private bool showToolTip;
  private bool isBalloonTooltip = true;

  [Category("Appearance")]
  [Description("Estilo de preenchimento da animação")]
  public AnimatedButton.FillStyle AnimationFillStyle
  {
    get => this.fillStyle;
    set
    {
      this.fillStyle = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Direção do preenchimento")]
  public AnimatedButton.FillDirection AnimationFillDirection
  {
    get => this.fillDirection;
    set
    {
      this.fillDirection = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Cor de fundo do botão")]
  public Color InsideColor
  {
    get => this.insideSolidColor;
    set
    {
      this.insideSolidColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Cor da borda")]
  public Color BorderColor
  {
    get => this.outsideSolidColor;
    set
    {
      this.outsideSolidColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Cor do efeito hover")]
  public Color HoverColor
  {
    get => this.hoverColor;
    set
    {
      this.hoverColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Cor do texto normal")]
  public Color TextColor
  {
    get => this.textColor;
    set
    {
      this.textColor = value;
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Cor do texto no hover")]
  public Color TextHoverColor
  {
    get => this.textHoverColor;
    set
    {
      this.textHoverColor = value;
      this.Invalidate();
    }
  }

  [Category("Behavior")]
  [Description("Velocidade da animação (1.0 = normal)")]
  public float AnimationSpeed
  {
    get => this.animationSpeed;
    set
    {
      this.animationSpeed = Math.Max(0.1f, Math.Min(5f, value));
      this.Invalidate();
    }
  }

  [Category("Appearance")]
  [Description("Raio dos cantos arredondados")]
  public int CornerRadius
  {
    get => this.cornerRadius;
    set
    {
      this.cornerRadius = Math.Max(0, Math.Min(20, value));
      this.UpdateButtonPath();
      this.Invalidate();
    }
  }

  [Category("Behavior")]
  [Description("Mostrar tooltip?")]
  public bool ShowToolTip
  {
    get => this.showToolTip;
    set
    {
      this.showToolTip = value;
      this.UpdateToolTip();
    }
  }

  [Category("Behavior")]
  [Description("Mensagem do tooltip")]
  public string ToolTipMessage
  {
    get => this.toolTipMessage;
    set
    {
      this.toolTipMessage = value;
      this.UpdateToolTip();
    }
  }

  [Category("Behavior")]
  [Description("Ícone do tooltip")]
  public string ToolTipIcon
  {
    get => this.toolTipIcon;
    set
    {
      this.toolTipIcon = value;
      this.UpdateToolTip();
    }
  }

  public AnimatedButton()
  {
    this.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    this.Size = new Size(150, 40);
    this.Text = "Button";
    this.BackColor = Color.Transparent;
    this.Font = new Font("Segoe UI", 9f);
    this.Cursor = Cursors.Hand;
    this.animationTimer = new Timer()
    {
      Interval = this.animationInterval
    };
    this.animationTimer.Tick += new EventHandler(this.AnimationTimer_Tick);
    this.animationTimer.Start();
    this.toolTip.ShowAlways = true;
    this.toolTip.IsBalloon = this.isBalloonTooltip;
    this.UpdateButtonPath();
  }

  private void UpdateButtonPath()
  {
    this.buttonRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
    this.buttonPath?.Dispose();
    this.buttonPath = this.CreateRoundedRectPath(this.buttonRect, this.cornerRadius);
  }

  private GraphicsPath CreateRoundedRectPath(Rectangle bounds, int radius)
  {
    GraphicsPath roundedRectPath = new GraphicsPath();
    if (radius <= 0)
    {
      roundedRectPath.AddRectangle(bounds);
      return roundedRectPath;
    }
    int num = radius * 2;
    Size size = new Size(num, num);
    Rectangle rect = new Rectangle(bounds.Location, size);
    roundedRectPath.AddArc(rect, 180f, 90f);
    rect.X = bounds.Right - num;
    roundedRectPath.AddArc(rect, 270f, 90f);
    rect.Y = bounds.Bottom - num;
    roundedRectPath.AddArc(rect, 0.0f, 90f);
    rect.X = bounds.Left;
    roundedRectPath.AddArc(rect, 90f, 90f);
    roundedRectPath.CloseFigure();
    return roundedRectPath;
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    if (this.buttonPath == null)
      this.UpdateButtonPath();
    base.OnPaint(e);
    Graphics graphics = e.Graphics;
    graphics.SmoothingMode = SmoothingMode.AntiAlias;
    try
    {
      using (SolidBrush solidBrush = new SolidBrush(this.insideSolidColor))
        graphics.FillPath((Brush) solidBrush, this.buttonPath);
      using (Pen pen = new Pen(this.outsideSolidColor))
        graphics.DrawPath(pen, this.buttonPath);
      if ((double) this.animations.ClosingAlpha > 0.0)
      {
        using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb((int) this.animations.ClosingAlpha, this.hoverColor)))
        {
          if (this.fillStyle == AnimatedButton.FillStyle.Solid)
          {
            int closingAnim = (int) this.animations.ClosingAnim;
            if (closingAnim > 0)
            {
              using (GraphicsPath roundedRectPath = this.CreateRoundedRectPath(this.GetFillRectangle(closingAnim), this.cornerRadius))
                graphics.FillPath((Brush) solidBrush, roundedRectPath);
            }
          }
          else
            graphics.FillPath((Brush) solidBrush, this.buttonPath);
        }
      }
      StringFormat format = new StringFormat()
      {
        Alignment = StringAlignment.Center,
        LineAlignment = StringAlignment.Center
      };
      using (SolidBrush solidBrush1 = new SolidBrush(Color.FromArgb((int) byte.MaxValue - (int) this.animations.LabelAlpha, this.textColor)))
      {
        using (SolidBrush solidBrush2 = new SolidBrush(Color.FromArgb((int) this.animations.LabelAlpha, this.textHoverColor)))
        {
          graphics.DrawString(this.Text, this.Font, (Brush) solidBrush1, (RectangleF) this.buttonRect, format);
          graphics.DrawString(this.Text, this.Font, (Brush) solidBrush2, (RectangleF) this.buttonRect, format);
        }
      }
    }
    catch
    {
      using (SolidBrush solidBrush = new SolidBrush(SystemColors.Control))
        graphics.FillRectangle((Brush) solidBrush, this.ClientRectangle);
      using (Pen pen = new Pen(SystemColors.ControlDark))
        graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
      TextRenderer.DrawText((IDeviceContext) graphics, this.Text, this.Font, this.ClientRectangle, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
  }

  private Rectangle GetFillRectangle(int animSize)
  {
    switch (this.fillDirection)
    {
      case AnimatedButton.FillDirection.TopToBottom:
        return new Rectangle(0, 0, this.Width, animSize);
      case AnimatedButton.FillDirection.LeftToRight:
        return new Rectangle(0, 0, animSize, this.Height);
      case AnimatedButton.FillDirection.RightToLeft:
        return new Rectangle(this.Width - animSize, 0, animSize, this.Height);
      case AnimatedButton.FillDirection.CenterOut:
        float num1 = (float) Math.Sqrt((double) (this.Width * this.Width + this.Height * this.Height));
        float num2 = (float) animSize / num1;
        int width = (int) ((double) this.Width * (double) num2);
        int height = (int) ((double) this.Height * (double) num2);
        return new Rectangle((this.Width - width) / 2, (this.Height - height) / 2, width, height);
      default:
        return new Rectangle(0, this.Height - animSize, this.Width, animSize);
    }
  }

  private void AnimationTimer_Tick(object sender, EventArgs e)
  {
    bool flag1 = false;
    float num = 0.01f * this.animationSpeed;
    bool flag2 = this.ClientRectangle.Contains(this.PointToClient(Cursor.Position));
    if (this.isHovered != flag2)
    {
      this.isHovered = flag2;
      flag1 = true;
    }
    if (this.isHovered)
    {
      if (this.fillStyle != AnimatedButton.FillStyle.SimpleFade)
      {
        float targetSize = this.GetTargetSize();
        this.animations.ClosingAnim = Math.Min(targetSize, this.animations.ClosingAnim + (float) (((double) targetSize - (double) this.animations.ClosingAnim) * (double) num * 12.0));
      }
      if ((double) this.animations.LabelAlpha < (double) byte.MaxValue)
      {
        this.animations.LabelAlpha += (float) (10.0 * (double) num * 160.0);
        flag1 = true;
      }
      if ((double) this.animations.ClosingAlpha < (double) byte.MaxValue)
      {
        this.animations.ClosingAlpha += (float) (20.0 * (double) num * 160.0);
        flag1 = true;
      }
    }
    else
    {
      if (this.fillStyle != AnimatedButton.FillStyle.SimpleFade)
        this.animations.ClosingAnim = Math.Max(0.0f, this.animations.ClosingAnim - (float) ((double) this.animations.ClosingAnim * (double) num * 12.0));
      if ((double) this.animations.LabelAlpha > 0.0)
      {
        this.animations.LabelAlpha -= (float) (10.0 * (double) num * 160.0);
        flag1 = true;
      }
      if ((double) this.animations.ClosingAlpha > 0.0)
      {
        this.animations.ClosingAlpha -= (float) (15.0 * (double) num * 160.0);
        flag1 = true;
      }
    }
    if (this.fillStyle != AnimatedButton.FillStyle.SimpleFade)
      this.animations.ClosingAnim = Math.Max(0.0f, Math.Min(this.GetTargetSize(), this.animations.ClosingAnim));
    this.animations.LabelAlpha = Math.Max(0.0f, Math.Min((float) byte.MaxValue, this.animations.LabelAlpha));
    this.animations.ClosingAlpha = Math.Max(0.0f, Math.Min((float) byte.MaxValue, this.animations.ClosingAlpha));
    if (!flag1 && (double) this.animations.ClosingAnim <= 0.0)
      return;
    this.Invalidate();
  }

  private float GetTargetSize()
  {
    if (this.fillStyle == AnimatedButton.FillStyle.SimpleFade)
      return 1f;
    switch (this.fillDirection)
    {
      case AnimatedButton.FillDirection.LeftToRight:
      case AnimatedButton.FillDirection.RightToLeft:
        return (float) this.Width;
      case AnimatedButton.FillDirection.CenterOut:
        return (float) Math.Sqrt((double) (this.Width * this.Width + this.Height * this.Height));
      default:
        return (float) this.Height;
    }
  }

  protected override void OnMouseEnter(EventArgs e)
  {
    base.OnMouseEnter(e);
    this.isHovered = true;
    this.Invalidate();
  }

  protected override void OnMouseLeave(EventArgs e)
  {
    base.OnMouseLeave(e);
    this.isHovered = false;
    this.Invalidate();
  }

  protected override void OnMouseClick(MouseEventArgs e)
  {
    base.OnMouseClick(e);
    if (e.Button != MouseButtons.Left)
      return;
    this.OnClick(EventArgs.Empty);
  }

  protected override void OnResize(EventArgs e)
  {
    base.OnResize(e);
    this.UpdateButtonPath();
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      this.animationTimer?.Stop();
      this.animationTimer?.Dispose();
      this.toolTip?.Dispose();
      this.buttonPath?.Dispose();
    }
    base.Dispose(disposing);
  }

  private void UpdateToolTip()
  {
    if (this.showToolTip && !string.IsNullOrEmpty(this.toolTipMessage))
      this.toolTip.SetToolTip((Control) this, !string.IsNullOrEmpty(this.toolTipIcon) ? $"{this.toolTipIcon} {this.toolTipMessage}" : this.toolTipMessage);
    else
      this.toolTip.SetToolTip((Control) this, (string) null);
  }

  public new event EventHandler Click;

  protected new virtual void OnClick(EventArgs e)
  {
    EventHandler click = this.Click;
    if (click == null)
      return;
    click((object) this, e);
  }

  public enum FillDirection
  {
    BottomToTop,
    TopToBottom,
    LeftToRight,
    RightToLeft,
    CenterOut,
  }

  public enum FillStyle
  {
    Solid,
    FadeInOut,
    SimpleFade,
  }

  private class ButtonAnimations
  {
    public float ClosingAnim { get; set; }

    public float ClosingAlpha { get; set; }

    public float LabelAlpha { get; set; }
  }
}
