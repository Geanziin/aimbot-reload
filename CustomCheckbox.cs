// Decompiled with JetBrains decompiler
// Type: CustomCheckbox
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using svchost.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#nullable disable
[DefaultEvent("CheckedChanged")]
public class CustomCheckbox : UserControl
{
  private bool isChecked;
  private bool isInitialized;
  private Timer animationTimer;
  private float lerpAmount;
  private float checkOffset;
  private Color backgroundColor = Color.FromArgb(52, 52, 53);

  [Category("Custom")]
  public bool Checked
  {
    get => this.isChecked;
    set
    {
      if (this.isChecked == value)
        return;
      this.isChecked = value;
      this.lerpAmount = this.isChecked ? 0.0f : 1f;
      this.checkOffset = this.isChecked ? 4f : 0.0f;
      if (this.isInitialized)
      {
        EventHandler checkedChanged = this.CheckedChanged;
        if (checkedChanged != null)
          checkedChanged((object) this, EventArgs.Empty);
      }
      if (this.animationTimer.Enabled)
        return;
      this.animationTimer.Start();
    }
  }

  [Category("Custom")]
  public int LabelSpacing { get; set; } = 8;

  [Category("Custom")]
  public Color BorderColor { get; set; } = Color.FromArgb(24, 24, 26);

  [Category("Custom")]
  public int BorderRadius { get; set; } = 6;

  [Category("Custom")]
  public Color FillColor { get; set; } = Color.FromArgb(52, 52, 53);

  [Category("Custom")]
  public Color CheckmarkColor { get; set; } = Color.FromArgb(21, 21, 23);

  [Category("Custom")]
  public float CheckmarkSize { get; set; } = 9f;

  [Category("Custom")]
  public float BorderThickness { get; set; } = 1.5f;

  [Category("Custom")]
  public string LabelText { get; set; } = "Check me!";

  [Category("Custom")]
  public Color LabelColor { get; set; } = Color.White;

  [Category("Custom")]
  public Font LabelFont { get; set; } = new Font("Segoe UI", 9f);

  public event EventHandler CheckedChanged;

  public CustomCheckbox()
  {
    this.DoubleBuffered = true;
    this.Size = new Size(120, 30);
    this.animationTimer = new Timer()
    {
      Interval = 16 /*0x10*/
    };
    this.animationTimer.Tick += (EventHandler) ((s, e) =>
    {
      float end = this.isChecked ? 1f : 0.0f;
      this.lerpAmount = this.Lerp(this.lerpAmount, end, 0.35f);
      this.checkOffset = this.Lerp(this.checkOffset, this.isChecked ? 0.0f : 4f, 0.35f);
      this.Invalidate();
      if ((double) Math.Abs(this.lerpAmount - end) >= 0.0099999997764825821)
        return;
      this.lerpAmount = end;
      this.animationTimer.Stop();
    });
    this.MouseClick += (MouseEventHandler) ((s, e) => this.Checked = !this.Checked);
  }

  protected override void OnHandleCreated(EventArgs e)
  {
    base.OnHandleCreated(e);
    this.isInitialized = true;
  }

  private float Lerp(float start, float end, float amt) => start + (end - start) * amt;

  private Color InterpolateColor(Color from, Color to, float t)
  {
    int red = (int) ((double) from.R + (double) ((int) to.R - (int) from.R) * (double) t);
    int green = (int) ((double) from.G + (double) ((int) to.G - (int) from.G) * (double) t);
    int blue = (int) ((double) from.B + (double) ((int) to.B - (int) from.B) * (double) t);
    return Color.FromArgb((int) ((double) from.A + (double) ((int) to.A - (int) from.A) * (double) t), red, green, blue);
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);
    Graphics graphics = e.Graphics;
    Utils.Quality(graphics);
    float y1 = this.BorderThickness / 2f;
    float num1 = (float) this.Height - 2f * y1;
    RectangleF bounds = new RectangleF((string.IsNullOrEmpty(this.LabelText) ? 0.0f : graphics.MeasureString(this.LabelText, this.LabelFont).Width + (float) this.LabelSpacing) + y1, y1, num1, num1);
    using (GraphicsPath path = this.RoundedRect(bounds, (float) this.BorderRadius))
    {
      using (SolidBrush solidBrush = new SolidBrush(this.backgroundColor))
        graphics.FillPath((Brush) solidBrush, path);
      using (SolidBrush solidBrush = new SolidBrush(this.backgroundColor))
        graphics.FillPath((Brush) solidBrush, path);
      if ((double) this.BorderThickness > 0.0)
      {
        using (Pen pen = new Pen(this.BorderColor, this.BorderThickness))
          graphics.DrawPath(pen, path);
      }
    }
    if ((double) this.lerpAmount > 0.10000000149011612)
    {
      float checkmarkSize = this.CheckmarkSize;
      float num2 = bounds.X + bounds.Width / 2f;
      float num3 = bounds.Y + bounds.Height / 2f;
      PointF pointF1 = new PointF(num2 - checkmarkSize * 0.6f, num3 + checkmarkSize * 0.1f + this.checkOffset);
      PointF pointF2 = new PointF(num2 - checkmarkSize * 0.2f, num3 + checkmarkSize * 0.5f + this.checkOffset);
      PointF pointF3 = new PointF(num2 + checkmarkSize * 0.6f, num3 - checkmarkSize * 0.4f + this.checkOffset);
      using (Pen pen = new Pen(Color.FromArgb((int) ((double) byte.MaxValue * (double) this.lerpAmount), this.CheckmarkColor), 2.5f))
        graphics.DrawLines(pen, new PointF[3]
        {
          pointF1,
          pointF2,
          pointF3
        });
    }
    if (string.IsNullOrEmpty(this.LabelText))
      return;
    float x = 0.0f;
    float y2 = (float) (((double) this.Height - (double) graphics.MeasureString(this.LabelText, this.LabelFont).Height) / 2.0);
    using (SolidBrush solidBrush = new SolidBrush(this.LabelColor))
      graphics.DrawString(this.LabelText, this.LabelFont, (Brush) solidBrush, x, y2);
  }

  private GraphicsPath RoundedRect(RectangleF bounds, float radius)
  {
    GraphicsPath graphicsPath = new GraphicsPath();
    float num = radius * 2f;
    if ((double) radius > 0.0)
    {
      graphicsPath.AddArc(bounds.X, bounds.Y, num, num, 180f, 90f);
      graphicsPath.AddArc(bounds.Right - num, bounds.Y, num, num, 270f, 90f);
      graphicsPath.AddArc(bounds.Right - num, bounds.Bottom - num, num, num, 0.0f, 90f);
      graphicsPath.AddArc(bounds.X, bounds.Bottom - num, num, num, 90f, 90f);
      graphicsPath.CloseFigure();
    }
    else
      graphicsPath.AddRectangle(bounds);
    return graphicsPath;
  }

  private void InitializeComponent()
  {
    this.SuspendLayout();
    this.Name = nameof (CustomCheckbox);
    this.Load += new EventHandler(this.CustomCheckbox_Load);
    this.ResumeLayout(false);
  }

  private void CustomCheckbox_Load(object sender, EventArgs e)
  {
  }
}
