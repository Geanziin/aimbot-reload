// Decompiled with JetBrains decompiler
// Type: WindowsFormsApp1.Program
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;
using System.Windows.Forms;

#nullable disable
namespace WindowsFormsApp1;

internal static class Program
{
  [STAThread]
  private static void Main()
  {
    try
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run((Form) new Spotify());
    }
    catch (Exception)
    {
      // Em caso de erro, termina a aplicação
      Environment.Exit(1);
    }
  }
}
