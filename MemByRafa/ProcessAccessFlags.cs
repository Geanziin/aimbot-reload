// Decompiled with JetBrains decompiler
// Type: MemByRafa.ProcessAccessFlags
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;

#nullable disable
namespace MemByRafa;

[Flags]
public enum ProcessAccessFlags
{
  AllAccess = 2035711, // 0x001F0FFF
  CreateProcess = 128, // 0x00000080
  CreateThread = 2,
  DupHandle = 64, // 0x00000040
  QueryInformation = 1024, // 0x00000400
  QueryLimitedInformation = 4096, // 0x00001000
  SetInformation = 512, // 0x00000200
  SetQuota = 256, // 0x00000100
  SuspendResume = 2048, // 0x00000800
  Terminate = 1,
  VmOperation = 8,
  VmRead = 16, // 0x00000010
  VmWrite = 32, // 0x00000020
  Synchronize = 1048576, // 0x00100000
}
