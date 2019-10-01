using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Shared.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Synfron.Staxe.Matcher.Interop
{
	public class MatcherClassGenerator
	{
		private readonly LanguageMatcher _languageMatcher;
		private readonly string _className;
		private readonly string _assemblyName;

		public MatcherClassGenerator(LanguageMatcher languageMatcher)
		{
			_languageMatcher = languageMatcher;
			_className = $"{languageMatcher.Name}MatchEngine";
			_assemblyName = $"Synfron.Staxe.Matcher.{languageMatcher.Name}";
		}

		private string GetGeneratedClass()
		{
			return MatcherEngineGenerator.GenerateEngine(_languageMatcher);
		}

		public string GetClass()
		{
			string classText = GetGeneratedClass();
			return CSharpSyntaxTree.ParseText(classText).GetRoot().NormalizeWhitespace().ToFullString();
		}

		public void OutputClass(string folder)
		{
			File.WriteAllText(Path.Combine(folder, _className + ".cs"), GetClass());
		}

		private CSharpCompilation GetAssemblyCompilation()
		{
			Dictionary<string, string> assemblyPathMap = AppDomain.CurrentDomain.GetAssemblies().
				Where(assembly => !assembly.IsDynamic).GroupBy(assembly => assembly.GetName().Name, (key, group) => group.First()).
				ToDictionary(assembly => assembly.GetName().Name + ".dll", assembly => assembly.Location);
			string corePath = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);
			string coreFile = Path.Combine(corePath, "mscorlib.dll");
			if (!File.Exists(coreFile))
			{
				coreFile = null;
			}
			return CSharpCompilation.Create(_assemblyName).
				WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)).
				AddReferences(
					GetMetadataReferences(
						assemblyPathMap.GetValueOrDefault("System.Runtime.dll"),
						coreFile,
						assemblyPathMap.GetValueOrDefault("netstandard.dll"),
						assemblyPathMap.GetValueOrDefault("System.Private.CoreLib.dll"),
						assemblyPathMap.GetValueOrDefault("System.Collections.dll"),
						assemblyPathMap.GetValueOrDefault("System.Collections.Specialized.dll"),
						assemblyPathMap.GetValueOrDefault("System.Linq.dll"),
						assemblyPathMap.GetValueOrDefault("Synfron.Staxe.Shared.dll"),
						assemblyPathMap.GetValueOrDefault("Synfron.Staxe.Matcher.dll"),
						assemblyPathMap.GetValueOrDefault("System.Memory.dll")
					)
				).AddSyntaxTrees(CSharpSyntaxTree.ParseText(GetGeneratedClass()));
		}

		private MetadataReference[] GetMetadataReferences(params string[] paths)
		{
			return paths.Where(path => path != null).Select(path => MetadataReference.CreateFromFile(path)).ToArray();
		}

		public Assembly GetAssembly()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				EmitResult result = GetAssemblyCompilation().Emit(stream);
				if (!result.Success)
				{
					throw new InvalidDataException($"Could not create assembly:\n {string.Join("\n", result.Diagnostics.Select(diagnostic => diagnostic.ToString()))}");
				}
				return Assembly.Load(stream.GetBuffer());
			}

		}

		public void OutputAssembly(string folder)
		{
			EmitResult result = GetAssemblyCompilation().Emit(Path.Combine(folder, _assemblyName + ".dll"));
			if (!result.Success)
			{
				throw new InvalidDataException($"Could not create assembly:\n {string.Join("\n", result.Diagnostics.Select(diagnostic => diagnostic.ToString()))}");
			}
		}
	}
}
