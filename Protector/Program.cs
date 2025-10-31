using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Protector
{
	internal static class Program
	{
		private static readonly Random _rng = new Random();
		private static readonly string[] _obfNames = GenerateObfNames(256);

		private static int Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.Error.WriteLine("Uso: Protector.exe <input> <output> [loader]");
				return 1;
			}

			var inputPath = args[0];
			var outputPath = args[1];
			var loaderOverride = args.Length >= 3 ? args[2] : null;

			try
			{
				Console.WriteLine("[*] Carregando módulo...");
				var module = ModuleDefinition.ReadModule(inputPath, new ReaderParameters 
				{ 
					ReadWrite = false, 
					InMemory = true,
					ReadingMode = ReadingMode.Deferred
				});

				Console.WriteLine("[*] Aplicando proteções...");
				AddSuppressIldasm(module);
				AddAntiTamperAttributes(module);
				var decryptMethod = EnsureDecryptMethod(module);
				EncryptAllStrings(module, decryptMethod);
				ObfuscateTypes(module);

				var tempProtected = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".tmp");
				module.Write(tempProtected);

				Console.WriteLine("[*] Criptografando payload...");
				byte[] payload = File.ReadAllBytes(tempProtected);
				File.Delete(tempProtected);

				// Dupla criptografia: AES -> XOR -> AES
				var keyIv1 = DeriveKeyIv("P1", "S1");
				var keyIv2 = DeriveKeyIv("P2", "S2");
				var xorKey = DeriveKeyIv("PX", "SX");

				byte[] enc = AesEncrypt(payload, keyIv1.key, keyIv1.iv);
				enc = XorEncrypt(enc, xorKey.key);
				enc = AesEncrypt(enc, keyIv2.key, keyIv2.iv);

				// Compressão avançada com múltiplas camadas
				Console.WriteLine("[*] Comprimindo payload...");
				byte[] compressed = CompressAdvanced(enc);

				Console.WriteLine("[*] Carregando loader stub...");
				var loaderPath = !string.IsNullOrWhiteSpace(loaderOverride) && File.Exists(loaderOverride) 
					? loaderOverride 
					: ResolveLoaderPath();
				if (loaderPath == null)
					throw new InvalidOperationException("Protector.Loader não encontrado. Compile o projeto primeiro.");

				var loader = ModuleDefinition.ReadModule(loaderPath, new ReaderParameters 
				{ 
					ReadWrite = false, 
					InMemory = true,
					ReadingMode = ReadingMode.Deferred
				});
				
				EmbedPayload(loader, compressed, GenerateRandomResourceName());

				Console.WriteLine("[*] Ofuscando loader stub...");
				ObfuscateLoader(loader);

				Console.WriteLine("[*] Salvando executável protegido...");
				loader.Write(outputPath);
				
				Console.WriteLine($"[+] Sucesso! Arquivo protegido: {outputPath}");
				return 0;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"[!] Erro: {ex.Message}");
				if (System.Diagnostics.Debugger.IsAttached)
					throw;
				return -1;
			}
		}

		private static string[] GenerateObfNames(int count)
		{
			var names = new List<string>();
			var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			var used = new HashSet<string>();

			for (int i = 0; i < count; i++)
			{
				string name;
				do
				{
					var len = _rng.Next(8, 24);
					var sb = new StringBuilder(len);
					for (int j = 0; j < len; j++)
						sb.Append(chars[_rng.Next(chars.Length)]);
					name = sb.ToString();
				} while (used.Contains(name));
				
				used.Add(name);
				names.Add(name);
			}
			return names.ToArray();
		}

		private static string GenerateRandomResourceName()
		{
			return _obfNames[_rng.Next(_obfNames.Length)] + _rng.Next(1000, 9999).ToString();
		}

		private static byte[] XorEncrypt(byte[] data, byte[] key)
		{
			var result = new byte[data.Length];
			for (int i = 0; i < data.Length; i++)
				result[i] = (byte)(data[i] ^ key[i % key.Length]);
			return result;
		}

		private static byte[] CompressAdvanced(byte[] data)
		{
			// Primeira camada: GZip
			byte[] gz;
			using (var ms = new MemoryStream())
			{
				using (var gzStream = new GZipStream(ms, CompressionLevel.Optimal, leaveOpen: true))
					gzStream.Write(data, 0, data.Length);
				gz = ms.ToArray();
			}

			// Segunda camada: Deflate (se ainda for vantajoso)
			if (gz.Length < data.Length * 0.9)
			{
				using (var ms = new MemoryStream())
				{
					using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true))
						deflate.Write(gz, 0, gz.Length);
					var deflated = ms.ToArray();
					if (deflated.Length < gz.Length)
						return deflated;
				}
			}

			return gz;
		}

		private static (byte[] key, byte[] iv) DeriveKeyIv(string part1, string part2)
		{
			// Base string determinística (compartilhada com Loader - mesma string)
			var baseStr = "SpotifyProtection2025SecureKeyBase";
			var machineId = BitConverter.ToString(Encoding.UTF8.GetBytes(Environment.MachineName)).Replace("-", "");
			var salt1 = part1 + machineId.Substring(0, Math.Min(16, machineId.Length));
			var salt2 = part2 + (machineId.Length > 16 ? machineId.Substring(16, Math.Min(16, machineId.Length - 16)) : "");

			byte[] key;
			using (var pbkdf2 = new Rfc2898DeriveBytes(salt1, Encoding.UTF8.GetBytes(baseStr + salt1), 10000))
				key = pbkdf2.GetBytes(32);

			byte[] iv;
			using (var pbkdf2 = new Rfc2898DeriveBytes(salt2, Encoding.UTF8.GetBytes(baseStr + salt2), 5000))
				iv = pbkdf2.GetBytes(16);

			return (key, iv);
		}

		private static byte[] AesEncrypt(byte[] plain, byte[] key, byte[] iv)
		{
			using var aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Key = key;
			aes.IV = iv;

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

		private static void AddAntiTamperAttributes(ModuleDefinition module)
		{
			try
			{
				var securityAttrType = module.ImportReference(typeof(System.Security.Permissions.SecurityPermissionAttribute));
				var resolvedType = securityAttrType.Resolve();
				var securityCtor = module.ImportReference(resolvedType.Methods.First(m => m.IsConstructor && m.Parameters.Count == 1 && m.Parameters[0].ParameterType.FullName == "System.Security.Permissions.SecurityAction"));
				var actionEnum = module.ImportReference(typeof(System.Security.Permissions.SecurityAction));
				var resolvedEnum = actionEnum.Resolve();
				var skipVerification = resolvedEnum.Fields.First(f => f.Name == "SkipVerification");
				
				var ca = new CustomAttribute(securityCtor);
				ca.ConstructorArguments.Add(new CustomAttributeArgument(actionEnum, skipVerification.Constant));
				module.Assembly.CustomAttributes.Add(ca);
			}
			catch
			{
				// Se não conseguir adicionar, continua sem essa proteção
			}
		}

		private static void ObfuscateTypes(ModuleDefinition module)
		{
			var nameIndex = 0;
			foreach (var type in module.Types.ToList())
			{
				if (type.Namespace == "Protector" || type.IsWindowsRuntime || type.IsInterface || type.IsSpecialName)
					continue;

				if (nameIndex < _obfNames.Length)
				{
					var newName = _obfNames[nameIndex++];
					type.Namespace = "";
					type.Name = newName;

					// Ofuscar métodos também
					var methodIndex = nameIndex;
					foreach (var method in type.Methods.Where(m => !m.IsSpecialName && !m.IsConstructor))
					{
						if (methodIndex < _obfNames.Length)
							method.Name = _obfNames[methodIndex++];
					}
				}
			}
		}

		private static void ObfuscateLoader(ModuleDefinition loader)
		{
			var nameIndex = _rng.Next(50, 100);
			foreach (var t in loader.Types.ToList())
			{
				if (t.Namespace == "PL" || t.Namespace == "Protector.Loader")
					t.Namespace = nameIndex < _obfNames.Length ? _obfNames[nameIndex++] : "";

				if (t.Name == "L" || t.Name == "Program")
					t.Name = nameIndex < _obfNames.Length ? _obfNames[nameIndex++] : "A";

				foreach (var m in t.Methods.ToList())
				{
					if (m.Name == "M" || m.Name == "Main")
						m.Name = nameIndex < _obfNames.Length ? _obfNames[nameIndex++] : "X";
					else if (!m.IsSpecialName && !m.IsConstructor && nameIndex < _obfNames.Length)
						m.Name = _obfNames[nameIndex++];

					// Variáveis locais no Mono.Cecil não têm nome diretamente mutável
					// Os nomes são removidos automaticamente durante a compilação
				}

				// Ofuscar campos
				foreach (var f in t.Fields.ToList())
				{
					if (!f.IsSpecialName && nameIndex < _obfNames.Length)
						f.Name = _obfNames[nameIndex++];
				}
			}

			// Remover metadata desnecessária
			loader.Assembly.Name.Name = _obfNames[_rng.Next(_obfNames.Length)];
			if (loader.Assembly.HasCustomAttributes)
			{
				var toRemove = loader.Assembly.CustomAttributes
					.Where(ca => ca.AttributeType.Name.Contains("Debuggable") || ca.AttributeType.Name.Contains("CompilationRelaxations"))
					.ToList();
				foreach (var ca in toRemove)
					loader.Assembly.CustomAttributes.Remove(ca);
			}
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
