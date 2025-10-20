using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Protector
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
            if (args.Length < 2)
            {
                return 1;
            }

			var inputPath = args[0];
			var outputPath = args[1];
			var loaderOverride = args.Length >= 3 ? args[2] : null;

			try
			{
				// 1) Proteções leves
				var module = ModuleDefinition.ReadModule(inputPath, new ReaderParameters { ReadWrite = false, InMemory = true });
				AddSuppressIldasm(module);
				var decryptMethod = EnsureDecryptMethod(module);
				EncryptAllStrings(module, decryptMethod);

				var tempProtected = Path.GetTempFileName();
				module.Write(tempProtected);

				// 2) Ler payload e cifrar AES, depois GZip (Loader faz GZip->AES decrypt em ordem inversa)
				byte[] payload = File.ReadAllBytes(tempProtected);
				File.Delete(tempProtected);

				// AES encrypt
				var keyIv = DeriveKeyIv();
				var enc = AesEncrypt(payload, keyIv.key, keyIv.iv);

				// GZip do payload cifrado
				byte[] gz;
				using (var ms = new MemoryStream())
				{
					using (var gzStream = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
					{
						gzStream.Write(enc, 0, enc.Length);
					}
					gz = ms.ToArray();
				}

				// 3) Carregar stub Loader
				var loaderPath = !string.IsNullOrWhiteSpace(loaderOverride) && File.Exists(loaderOverride) ? loaderOverride : ResolveLoaderPath();
				if (loaderPath == null)
					throw new InvalidOperationException("Protector.Loader não encontrado (compile o projeto).");

				var loader = ModuleDefinition.ReadModule(loaderPath, new ReaderParameters { ReadWrite = false, InMemory = true });
				EmbedPayload(loader, gz, "payload");

				// 4) Ofuscação mínima do stub
				foreach (var t in loader.Types.ToList())
				{
					if (t.Namespace == "PL" || t.Namespace == "Protector.Loader") t.Namespace = "";
					if (t.Name == "L" || t.Name == "Program") t.Name = "A";
					foreach (var m in t.Methods)
					{
						if (m.Name == "M" || m.Name == "Main") m.Name = "X";
					}
				}

                loader.Write(outputPath);
                return 0;
			}
			catch (Exception ex)
			{
                return -1;
			}
		}

		private static (byte[] key, byte[] iv) DeriveKeyIv()
		{
			// Constante compartilhada com o Loader para derivação
			var baseStr = "SpotifyProtectorKey";
			byte[] key;
			using (var sha = SHA256.Create()) key = sha.ComputeHash(Encoding.UTF8.GetBytes(baseStr + "|S1"));
			byte[] iv;
			using (var md5 = MD5.Create()) iv = md5.ComputeHash(Encoding.UTF8.GetBytes(baseStr + "|S2"));
			return (key, iv);
		}

		private static byte[] AesEncrypt(byte[] plain, byte[] key, byte[] iv)
		{
			using var aes = Aes.Create();
			aes.Mode = CipherMode.CBC; aes.Padding = PaddingMode.PKCS7; aes.Key = key; aes.IV = iv;
			using var ms = new MemoryStream();
			using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
			{
				cs.Write(plain, 0, plain.Length);
				cs.FlushFinalBlock();
			}
			return ms.ToArray();
		}

		private static string ResolveLoaderPath()
		{
			var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			for (int i = 0; i < 8 && current != null; i++)
			{
				try
				{
					var found = Directory.GetFiles(current.FullName, "Protector.Loader.exe", SearchOption.AllDirectories).FirstOrDefault();
					if (found != null) return found;
				}
				catch { }
				current = current.Parent;
			}
			return null;
		}

		private static void EmbedPayload(ModuleDefinition loader, byte[] data, string name)
		{
			var res = new EmbeddedResource(name, ManifestResourceAttributes.Private, data);
			loader.Resources.Add(res);
		}

		private static void AddSuppressIldasm(ModuleDefinition module)
		{
			var attrType = module.ImportReference(typeof(System.Runtime.CompilerServices.SuppressIldasmAttribute));
			var ctor = module.ImportReference(attrType.Resolve().Methods.First(m => m.IsConstructor && !m.HasParameters));
			var ca = new CustomAttribute(ctor);
			module.Assembly.CustomAttributes.Add(ca);
		}

		private static MethodDefinition EnsureDecryptMethod(ModuleDefinition module)
		{
			var type = new TypeDefinition("Protector", "RuntimeStrings", TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed | TypeAttributes.Class, module.TypeSystem.Object);
			module.Types.Add(type);

			var method = new MethodDefinition("D", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.String);
			var param = new ParameterDefinition("b64", ParameterAttributes.None, module.TypeSystem.String);
			method.Parameters.Add(param);
			type.Methods.Add(method);

			var il = method.Body.GetILProcessor();
			var convertType = module.ImportReference(typeof(Convert));
			var fromBase64 = module.ImportReference(convertType.Resolve().Methods.First(m => m.Name == "FromBase64String" && m.Parameters.Count == 1));
			var encodingType = module.ImportReference(typeof(Encoding));
			var utf8Prop = module.ImportReference(encodingType.Resolve().Properties.First(p => p.Name == "UTF8").GetMethod);
			var getString = module.ImportReference(encodingType.Resolve().Methods.First(m => m.Name == "GetString" && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.Byte[]"));

			// byte[] raw = Convert.FromBase64String(b64);
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, fromBase64);
			var rawLocal = new VariableDefinition(module.ImportReference(typeof(byte[])));
			method.Body.InitLocals = true;
			method.Body.Variables.Add(rawLocal);
			il.Emit(OpCodes.Stloc, rawLocal);

			// return Encoding.UTF8.GetString(raw);
			il.Emit(OpCodes.Call, utf8Prop);
			il.Emit(OpCodes.Ldloc, rawLocal);
			il.Emit(OpCodes.Callvirt, getString);
			il.Emit(OpCodes.Ret);

			return method;
		}

		private static void EncryptAllStrings(ModuleDefinition module, MethodDefinition decryptMethod)
		{
			foreach (var type in module.Types)
			{
				EncryptStringsInType(type, module, decryptMethod);
			}
		}

		private static void EncryptStringsInType(TypeDefinition type, ModuleDefinition module, MethodDefinition decryptMethod)
		{
			foreach (var nested in type.NestedTypes)
			{
				EncryptStringsInType(nested, module, decryptMethod);
			}

			foreach (var method in type.Methods)
			{
				if (!method.HasBody) continue;
				var il = method.Body.GetILProcessor();
				var instructions = method.Body.Instructions.ToList();
				for (int i = 0; i < instructions.Count; i++)
				{
					var ins = instructions[i];
					if (ins.OpCode == OpCodes.Ldstr && ins.Operand is string s && !string.IsNullOrEmpty(s))
					{
						var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
						ins.Operand = b64; // replace operand
						il.InsertAfter(ins, Instruction.Create(OpCodes.Call, module.ImportReference(decryptMethod)));
					}
				}
			}
		}
	}
}
