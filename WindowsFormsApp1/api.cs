// Decompiled with JetBrains decompiler
// Type: WindowsFormsApp1.api
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

#nullable disable
namespace WindowsFormsApp1;

public class api
{
  private string name;
  private string ownerid;
  private string secret;
  private string version;

  public api(string name, string ownerid, string secret, string version)
  {
    this.name = name;
    this.ownerid = ownerid;
    this.secret = secret;
    this.version = version;
  }
}
