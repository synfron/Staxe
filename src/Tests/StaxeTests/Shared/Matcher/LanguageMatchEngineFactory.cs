using Synfron.Staxe.Matcher;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Interop;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StaxeTests.Shared.Matcher
{
	public class GeneratedLanguageMatchEngineRegistry
	{
		private static readonly Dictionary<string, Assembly> _assemblyMap = new Dictionary<string, Assembly>();

		public static ILanguageMatchEngine GetStaxeTestExpressionLangMatchEngine()
		{
			return (ILanguageMatchEngine)Activator.CreateInstance(GetAssembly(Language.StaxeTestExpressionLang, LanguageMatcherProvider.GetStaxeTestExpressionLangMatcher()).GetType($"Synfron.Staxe.Matcher.{Language.StaxeTestExpressionLang.ToString()}MatchEngine"));
		}

		public static ILanguageMatchEngine GetStaxeTestSimpleLangMatchEngine()
		{
			return (ILanguageMatchEngine)Activator.CreateInstance(GetAssembly(Language.StaxeTestSimpleLang, LanguageMatcherProvider.GetStaxeTestSimpleLangMatcher()).GetType($"Synfron.Staxe.Matcher.{Language.StaxeTestSimpleLang.ToString()}MatchEngine"));
		}

		public static ILanguageMatchEngine GetStaxeTestComplexLangMatchEngine()
		{
			return (ILanguageMatchEngine)Activator.CreateInstance(GetAssembly(Language.StaxeTestComplexLang, LanguageMatcherProvider.GetStaxeTestComplexLangMatcher()).GetType($"Synfron.Staxe.Matcher.{Language.StaxeTestComplexLang.ToString()}MatchEngine"));
		}

		public static ILanguageMatchEngine GetJsonMatchEngine()
		{
			return (ILanguageMatchEngine)Activator.CreateInstance(GetAssembly(Language.Json, LanguageMatcherProvider.GetJsonMatcher()).GetType($"Synfron.Staxe.Matcher.{Language.Json.ToString()}MatchEngine"));
		}

		private static Assembly GetAssembly(Language language, LanguageMatcher matcher)
		{
			lock (_assemblyMap)
			{
				if (!_assemblyMap.TryGetValue(language.ToString(), out Assembly assembly))
				{
					MatcherClassGenerator generator = new MatcherClassGenerator(matcher);
					assembly = generator.GetAssembly();
					_assemblyMap[language.ToString()] = assembly;
				}
				return assembly;
			}
		}
	}
}
