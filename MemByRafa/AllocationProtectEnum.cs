// Decompiled with JetBrains decompiler
// Type: MemByRafa.AllocationProtectEnum
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

#nullable disable
namespace MemByRafa;

public enum AllocationProtectEnum : uint
{
  PAGE_NOACCESS = 1,
  PAGE_READONLY = 2,
  PAGE_READWRITE = 4,
  PAGE_WRITECOPY = 8,
  PAGE_EXECUTE = 16, // 0x00000010
  PAGE_EXECUTE_READ = 32, // 0x00000020
  PAGE_EXECUTE_READWRITE = 64, // 0x00000040
  PAGE_EXECUTE_WRITECOPY = 128, // 0x00000080
  PAGE_GUARD = 256, // 0x00000100
  PAGE_NOCACHE = 512, // 0x00000200
  PAGE_WRITECOMBINE = 1024, // 0x00000400
}
