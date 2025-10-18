// Decompiled with JetBrains decompiler
// Type: GradientSeparator
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#nullable disable
public class GradientSeparator : Control
{
  private int thickness = 2;

  [Category("Appearance")]
  [Description("Espessura do separador.")]
  public int Thickness
  {
    get => this.thickness;
    set
    {
      this.thickness = Math.Max(1, value);
      this.Width = this.thickness;
      this.Invalidate();
    }
  }

  public GradientSeparator()
  {
    this.Width = this.thickness;
    this.Height = 200;
    this.DoubleBuffered = true;
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    base.OnPaint(e);
    Rectangle rect = new Rectangle(0, 0, this.thickness, this.Height);
    using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, Color.Black, Color.Black, LinearGradientMode.Vertical))
    {
      ColorBlend colorBlend = new ColorBlend()
      {
        Colors = new Color[4]
        {
          Color.FromArgb(45, 163, 70),
          Color.FromArgb(178, 97, 43),
          Color.FromArgb(201, 53, 39),
          Color.FromArgb(153, 50, 50)
        },
        Positions = new float[4]{ 0.0f, 0.4f, 0.7f, 1f }
      };
      linearGradientBrush.InterpolationColors = colorBlend;
      e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
      e.Graphics.FillRectangle((Brush) linearGradientBrush, rect);
    }
  }
}
