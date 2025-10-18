// Decompiled with JetBrains decompiler
// Type: MemByRafa.ThreadAccess
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;

#nullable disable
namespace MemByRafa;

[Flags]
public enum ThreadAccess
{
  TERMINATE = 1,
  SUSPEND_RESUME = 2,
  GET_CONTEXT = 8,
  SET_CONTEXT = 16, // 0x00000010
  SET_INFORMATION = 32, // 0x00000020
  QUERY_INFORMATION = 64, // 0x00000040
  SET_THREAD_TOKEN = 128, // 0x00000080
  IMPERSONATE = 256, // 0x00000100
  DIRECT_IMPERSONATION = 512, // 0x00000200
}
