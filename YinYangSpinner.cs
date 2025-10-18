// Decompiled with JetBrains decompiler
// Type: YinYangSpinner
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#nullable disable
public class YinYangSpinner : Control
{
  private Timer timer;
  private float time;

  public float Radius { get; set; } = 30f;

  public float Thickness { get; set; } = 4f;

  public float Speed { get; set; } = 2.8f;

  public float Angle { get; set; } = 2.1991148f;

  public bool Reverse { get; set; }

  public float YangDeltaRadius { get; set; } = 5f;

  public Color ColorI { get; set; } = Color.White;

  public Color ColorY { get; set; } = Color.White;

  public int Mode { get; set; }

  public YinYangSpinner()
  {
    this.DoubleBuffered = true;
    this.Size = new Size(100, 100);
    this.timer = new Timer();
    this.timer.Interval = 16 /*0x10*/;
    this.timer.Tick += (EventHandler) ((s, e) =>
    {
      this.time += 0.016f;
      this.Invalidate();
    });
    this.timer.Start();
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);
    Graphics graphics = e.Graphics;
    graphics.SmoothingMode = SmoothingMode.AntiAlias;
    float num1 = 60f;
    float num2 = this.Angle / num1;
    float num3 = this.Thickness / num1;
    PointF center = new PointF((float) this.Width / 2f, (float) this.Height / 2f);
    float num4 = this.time * this.Speed;
    float num5 = this.time * (this.Speed + ((double) this.YangDeltaRadius > 0.0 ? YinYangSpinner.Clamp(this.YangDeltaRadius * 0.5f, 0.5f, 2f) : 0.0f));
    for (int index = 0; (double) index < (double) num1; ++index)
    {
      float num6 = YinYangSpinner.Ease(this.Mode, num4 + (float) ((double) index * 3.1415927410125732 / 2.0) / num1, 3.14159274f, 1f, 0.0f);
      float angle1 = (float) ((double) num4 + (double) num6 + (double) index * (double) num2);
      float angle2 = (float) ((double) num4 + (double) num6 + (double) (index + 1) * (double) num2);
      this.DrawLine(graphics, center, this.Radius, angle1, angle2, this.ColorI, num3 * (float) index);
    }
    float num7 = YinYangSpinner.Ease(this.Mode, num4 + 1.57079637f, 3.14159274f, 1f, 0.0f);
    float angle = (float) ((double) num4 + (double) num7 + (double) num1 * (double) num2);
    PointF circlePoint1 = YinYangSpinner.GetCirclePoint(center, this.Radius, angle);
    graphics.FillEllipse((Brush) new SolidBrush(this.ColorI), circlePoint1.X - this.Thickness / 2f, circlePoint1.Y - this.Thickness / 2f, this.Thickness, this.Thickness);
    float num8 = this.Reverse ? -1f : 1f;
    float radius = this.Radius - this.YangDeltaRadius;
    for (int index = 0; (double) index < (double) num1; ++index)
    {
      float num9 = YinYangSpinner.Ease(this.Mode, num4 + (float) ((double) index * 3.1415927410125732 / 2.0) / num1, 3.14159274f, 1f, 0.0f);
      float num10 = (float) ((double) num5 - (double) num9 + 3.1415927410125732 + (double) index * (double) num2);
      float num11 = (float) ((double) num5 - (double) num9 + 3.1415927410125732 + (double) (index + 1) * (double) num2);
      this.DrawLine(graphics, center, radius, num10 * num8, num11 * num8, this.ColorY, num3 * (float) index);
    }
    float num12 = YinYangSpinner.Ease(this.Mode, num4 + 1.57079637f, 3.14159274f, 1f, 0.0f);
    float num13 = (float) ((double) num5 - (double) num12 + 3.1415927410125732 + (double) num1 * (double) num2);
    PointF circlePoint2 = YinYangSpinner.GetCirclePoint(center, radius, num13 * num8);
    graphics.FillEllipse((Brush) new SolidBrush(this.ColorY), circlePoint2.X - this.Thickness / 2f, circlePoint2.Y - this.Thickness / 2f, this.Thickness, this.Thickness);
  }

  private void DrawLine(
    Graphics g,
    PointF center,
    float radius,
    float angle1,
    float angle2,
    Color color,
    float width)
  {
    PointF circlePoint1 = YinYangSpinner.GetCirclePoint(center, radius, angle1);
    PointF circlePoint2 = YinYangSpinner.GetCirclePoint(center, radius, angle2);
    using (Pen pen = new Pen(color, width))
      g.DrawLine(pen, circlePoint1, circlePoint2);
  }

  private static PointF GetCirclePoint(PointF center, float radius, float angle)
  {
    return new PointF(center.X + (float) Math.Cos((double) angle) * radius, center.Y + (float) Math.Sin((double) angle) * radius);
  }

  private static float Clamp(float val, float min, float max) => Math.Max(min, Math.Min(max, val));

  private static float Ease(int mode, float t, float d, float b, float c) => c * (t / d) + b;
}
