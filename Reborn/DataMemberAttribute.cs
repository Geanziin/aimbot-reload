// Decompiled with JetBrains decompiler
// Type: Reborn.DataMemberAttribute
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;

#nullable disable
namespace Reborn;

internal class DataMemberAttribute : Attribute
{
  public bool IsRequired { get; set; }

  public bool EmitDefaultValue { get; set; }
}
