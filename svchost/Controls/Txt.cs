// Decompiled with JetBrains decompiler
// Type: svchost.Controls.Txt
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

#nullable disable
namespace svchost.Controls;

public class Txt : Label
{
  public Txt.HorizontalAlignment HorizontalTextAlignment { get; set; } = Txt.HorizontalAlignment.Center;

  public Txt.VerticalAlignment VerticalTextAlignment { get; set; } = Txt.VerticalAlignment.Middle;

  public Txt()
  {
    this.SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
    this.BackColor = Color.Transparent;
    this.UseCompatibleTextRendering = true;
    this.DoubleBuffered = true;
    Utils.FPS((Control) this);
  }

  protected override void OnPaint(PaintEventArgs e)
  {
    SizeF sizeF = e.Graphics.MeasureString(this.Text, this.Font);
    float d1 = 0.0f;
    float d2 = 0.0f;
    switch (this.HorizontalTextAlignment)
    {
      case Txt.HorizontalAlignment.Left:
        d1 = 0.0f;
        break;
      case Txt.HorizontalAlignment.Center:
        d1 = (float) (((double) this.Width - (double) sizeF.Width) / 2.0);
        break;
      case Txt.HorizontalAlignment.Right:
        d1 = (float) this.Width - sizeF.Width;
        break;
    }
    switch (this.VerticalTextAlignment)
    {
      case Txt.VerticalAlignment.Top:
        d2 = 0.0f;
        break;
      case Txt.VerticalAlignment.Middle:
        d2 = (float) (((double) this.Height - (double) sizeF.Height) / 2.0);
        break;
      case Txt.VerticalAlignment.Bottom:
        d2 = (float) this.Height - sizeF.Height;
        break;
    }
    float x = (float) Math.Floor((double) d1);
    float y = (float) Math.Floor((double) d2);
    Bitmap bitmap = Utils.TextToBitmap(this.Text, this.Font, this.ForeColor);
    Utils.Quality(e.Graphics);
    e.Graphics.DrawImage((Image) bitmap, x, y);
  }

  public void ChangeColor(Color tCol, int t)
  {
    new Thread((ThreadStart) (() =>
    {
      for (int currentStep = 0; currentStep < t; ++currentStep)
      {
        Color lerpedColor = Utils.ColorLerp(this.ForeColor, tCol, currentStep, t);
        this.Invoke((Delegate) (() => this.ForeColor = lerpedColor));
        Thread.Sleep(1);
      }
    })).Start();
  }

  public enum HorizontalAlignment
  {
    Left,
    Center,
    Right,
  }

  public enum VerticalAlignment
  {
    Top,
    Middle,
    Bottom,
  }
}
