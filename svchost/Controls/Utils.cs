// Decompiled with JetBrains decompiler
// Type: svchost.Controls.Utils
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Management;
using System.Threading;
using System.Windows.Forms;

#nullable disable
namespace svchost.Controls;

internal class Utils
{
  public static Stopwatch stopwatch = new Stopwatch();
  public static double targetFrameTime = 125.0 / 18.0;
  public static Control f_;

  public static Bitmap TextToBitmap(string text, Font font, Color color)
  {
    SizeF sizeF = Graphics.FromHwndInternal(IntPtr.Zero).MeasureString(text, font);
    Bitmap bitmap = new Bitmap((int) Math.Ceiling((double) sizeF.Width), (int) Math.Ceiling((double) sizeF.Height));
    SolidBrush solidBrush = new SolidBrush(color);
    using (Graphics g = Graphics.FromImage((Image) bitmap))
    {
      Utils.Quality(g);
      g.DrawString(text, font, (Brush) solidBrush, 0.0f, 0.0f);
      g.Flush();
    }
    return bitmap;
  }

  public static void Quality(Graphics g)
  {
    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    g.TextRenderingHint = TextRenderingHint.AntiAlias;
    g.SmoothingMode = SmoothingMode.AntiAlias;
    g.CompositingQuality = CompositingQuality.HighQuality;
    g.CompositingMode = CompositingMode.SourceOver;
    g.PixelOffsetMode = PixelOffsetMode.None;
  }

  public static GraphicsPath GetFigurePath(Rectangle rect, int radius)
  {
    GraphicsPath figurePath = new GraphicsPath();
    float num = (float) radius * 2f;
    figurePath.StartFigure();
    figurePath.AddArc((float) rect.X, (float) rect.Y, num, num, 180f, 90f);
    figurePath.AddArc((float) rect.Right - num, (float) rect.Y, num, num, 270f, 90f);
    figurePath.AddArc((float) rect.Right - num, (float) rect.Bottom - num, num, num, 0.0f, 90f);
    figurePath.AddArc((float) rect.X, (float) rect.Bottom - num, num, num, 90f, 90f);
    figurePath.CloseFigure();
    return figurePath;
  }

  public static Color ColorLerp(Color color1, Color color2, int currentStep, int totalSteps)
  {
    float num1 = Math.Max(0.0f, Math.Min(1f, (float) currentStep / (float) totalSteps));
    int red = (int) ((double) color1.R * (1.0 - (double) num1) + (double) color2.R * (double) num1);
    int num2 = (int) ((double) color1.G * (1.0 - (double) num1) + (double) color2.G * (double) num1);
    int num3 = (int) ((double) color1.B * (1.0 - (double) num1) + (double) color2.B * (double) num1);
    int green = num2;
    int blue = num3;
    return Color.FromArgb(red, green, blue);
  }

  public static int GetMonitorRefreshRate()
  {
    int monitorRefreshRate = 60;
    try
    {
      using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = new ManagementObjectSearcher("SELECT * FROM CIM_VideoControllerResolution").Get().GetEnumerator())
      {
        if (enumerator.MoveNext())
          monitorRefreshRate = Convert.ToInt32(enumerator.Current["CurrentRefreshRate"]);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("Erro ao obter a taxa de atualização do monitor: " + ex.Message);
    }
    return monitorRefreshRate;
  }

  public static void FPS(Control f)
  {
    Utils.f_ = f;
    new Thread((ThreadStart) (() =>
    {
      while (true)
        Utils.FrameTimerCallback();
    }))
    {
      IsBackground = true
    }.Start();
    Utils.stopwatch.Start();
  }

  public static void FrameTimerCallback()
  {
    double totalMilliseconds = Utils.stopwatch.Elapsed.TotalMilliseconds;
    Utils.stopwatch.Restart();
    Utils.UpdateFrame();
    if (totalMilliseconds >= Utils.targetFrameTime)
      return;
    Thread.Sleep((int) (Utils.targetFrameTime - totalMilliseconds));
  }

  public static void UpdateFrame() => Utils.f_.Invalidate();
}
