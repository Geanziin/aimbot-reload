// Decompiled with JetBrains decompiler
// Type: Costura.AssemblyLoader
// Assembly: Spotify, Version=1.2.66.447, Culture=neutral, PublicKeyToken=null
// MVID: 86D05C46-F66B-4354-A0DD-74F2377DCB52
// Assembly location: C:\Users\gean\Desktop\Spotify.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

#nullable disable
namespace Costura;

[CompilerGenerated]
internal static class AssemblyLoader
{
  private static object nullCacheLock = new object();
  private static Dictionary<string, bool> nullCache = new Dictionary<string, bool>();
  private static Dictionary<string, string> assemblyNames = new Dictionary<string, string>();
  private static Dictionary<string, string> symbolNames = new Dictionary<string, string>();
  private static int isAttached;

  private static string CultureToString(CultureInfo culture)
  {
    return culture == null ? string.Empty : culture.Name;
  }

  private static Assembly ReadExistingAssembly(AssemblyName name)
  {
    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
    {
      AssemblyName name1 = assembly.GetName();
      // ISSUE: reference to a compiler-generated method
      // ISSUE: reference to a compiler-generated method
      if (string.Equals(name1.Name, name.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals(AssemblyLoader.CultureToString(name1.CultureInfo), AssemblyLoader.CultureToString(name.CultureInfo), StringComparison.InvariantCultureIgnoreCase))
        return assembly;
    }
    return (Assembly) null;
  }

  private static string GetAssemblyResourceName(AssemblyName requestedAssemblyName)
  {
    string lowerInvariant = requestedAssemblyName.Name.ToLowerInvariant();
    if (requestedAssemblyName.CultureInfo != null && !string.IsNullOrEmpty(requestedAssemblyName.CultureInfo.Name))
    {
      // ISSUE: reference to a compiler-generated method
      lowerInvariant = $"{AssemblyLoader.CultureToString(requestedAssemblyName.CultureInfo)}.{lowerInvariant}".ToLowerInvariant();
    }
    return lowerInvariant;
  }

  private static void CopyTo(Stream source, Stream destination)
  {
    byte[] buffer = new byte[81920 /*0x014000*/];
    int count;
    while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
      destination.Write(buffer, 0, count);
  }

  private static Stream LoadStream(string fullName)
  {
    Assembly executingAssembly = Assembly.GetExecutingAssembly();
    if (!fullName.EndsWith(".compressed"))
      return executingAssembly.GetManifestResourceStream(fullName);
    using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(fullName))
    {
      using (DeflateStream source = new DeflateStream(manifestResourceStream, CompressionMode.Decompress))
      {
        MemoryStream destination = new MemoryStream();
        // ISSUE: reference to a compiler-generated method
        AssemblyLoader.CopyTo((Stream) source, (Stream) destination);
        destination.Position = 0L;
        return (Stream) destination;
      }
    }
  }

  private static Stream LoadStream(Dictionary<string, string> resourceNames, string name)
  {
    string fullName;
    // ISSUE: reference to a compiler-generated method
    return resourceNames.TryGetValue(name, out fullName) ? AssemblyLoader.LoadStream(fullName) : (Stream) null;
  }

  private static byte[] ReadStream(Stream stream)
  {
    byte[] buffer = new byte[stream.Length];
    stream.Read(buffer, 0, buffer.Length);
    return buffer;
  }

  private static Assembly ReadFromEmbeddedResources(
    Dictionary<string, string> assemblyNames,
    Dictionary<string, string> symbolNames,
    AssemblyName requestedAssemblyName)
  {
    // ISSUE: reference to a compiler-generated method
    string assemblyResourceName = AssemblyLoader.GetAssemblyResourceName(requestedAssemblyName);
    byte[] rawAssembly;
    // ISSUE: reference to a compiler-generated method
    using (Stream stream = AssemblyLoader.LoadStream(assemblyNames, assemblyResourceName))
    {
      if (stream == null)
        return (Assembly) null;
      // ISSUE: reference to a compiler-generated method
      rawAssembly = AssemblyLoader.ReadStream(stream);
    }
    // ISSUE: reference to a compiler-generated method
    using (Stream stream = AssemblyLoader.LoadStream(symbolNames, assemblyResourceName))
    {
      if (stream != null)
      {
        // ISSUE: reference to a compiler-generated method
        byte[] rawSymbolStore = AssemblyLoader.ReadStream(stream);
        return Assembly.Load(rawAssembly, rawSymbolStore);
      }
    }
    return Assembly.Load(rawAssembly);
  }

  public static Assembly ResolveAssembly(object sender, ResolveEventArgs e)
  {
    string name = e.Name;
    AssemblyName assemblyName = new AssemblyName(name);
    // ISSUE: reference to a compiler-generated field
    lock (AssemblyLoader.nullCacheLock)
    {
      // ISSUE: reference to a compiler-generated field
      if (AssemblyLoader.nullCache.ContainsKey(name))
        return (Assembly) null;
    }
    // ISSUE: reference to a compiler-generated method
    Assembly assembly1 = AssemblyLoader.ReadExistingAssembly(assemblyName);
    if ((object) assembly1 != null)
      return assembly1;
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated method
    Assembly assembly2 = AssemblyLoader.ReadFromEmbeddedResources(AssemblyLoader.assemblyNames, AssemblyLoader.symbolNames, assemblyName);
    if ((object) assembly2 == null)
    {
      // ISSUE: reference to a compiler-generated field
      lock (AssemblyLoader.nullCacheLock)
      {
        // ISSUE: reference to a compiler-generated field
        AssemblyLoader.nullCache[name] = true;
      }
      if ((assemblyName.Flags & AssemblyNameFlags.Retargetable) != AssemblyNameFlags.None)
        assembly2 = Assembly.Load(assemblyName);
    }
    return assembly2;
  }

  static AssemblyLoader()
  {
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("costura", "costura.costura.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.symbolNames.Add("costura", "costura.costura.pdb.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("cuoreui", "costura.cuoreui.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("guna.ui2", "costura.guna.ui2.dll.compressed");
    // ISSUE: reference to a compiler-generated field
    AssemblyLoader.assemblyNames.Add("newtonsoft.json", "costura.newtonsoft.json.dll.compressed");
  }

  public static void Attach(bool subscribe)
  {
    // ISSUE: reference to a compiler-generated field
    if (Interlocked.Exchange(ref AssemblyLoader.isAttached, 1) == 1 || !subscribe)
      return;
    AppDomain.CurrentDomain.AssemblyResolve += (ResolveEventHandler) ((sender, e) =>
    {
      string name = e.Name;
      AssemblyName assemblyName = new AssemblyName(name);
      // ISSUE: reference to a compiler-generated field
      lock (AssemblyLoader.nullCacheLock)
      {
        // ISSUE: reference to a compiler-generated field
        if (AssemblyLoader.nullCache.ContainsKey(name))
          return (Assembly) null;
      }
      // ISSUE: reference to a compiler-generated method
      Assembly assembly1 = AssemblyLoader.ReadExistingAssembly(assemblyName);
      if ((object) assembly1 != null)
        return assembly1;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated field
      // ISSUE: reference to a compiler-generated method
      Assembly assembly2 = AssemblyLoader.ReadFromEmbeddedResources(AssemblyLoader.assemblyNames, AssemblyLoader.symbolNames, assemblyName);
      if ((object) assembly2 == null)
      {
        // ISSUE: reference to a compiler-generated field
        lock (AssemblyLoader.nullCacheLock)
        {
          // ISSUE: reference to a compiler-generated field
          AssemblyLoader.nullCache[name] = true;
        }
        if ((assemblyName.Flags & AssemblyNameFlags.Retargetable) != AssemblyNameFlags.None)
          assembly2 = Assembly.Load(assemblyName);
      }
      return assembly2;
    });
  }
}
