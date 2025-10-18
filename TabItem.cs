// Decompiled with JetBrains decompiler
// Type: TabItem
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System.Drawing;

#nullable disable
public class TabItem
{
  public string Text { get; set; }

  public Image Image { get; set; }

  public int TopLeftRadius { get; set; } = 6;

  public int TopRightRadius { get; set; } = 6;

  public int BottomLeftRadius { get; set; } = 6;

  public int BottomRightRadius { get; set; } = 6;

  public TabItem(string text, Image image = null)
  {
    this.Text = text;
    this.Image = image;
  }
}
