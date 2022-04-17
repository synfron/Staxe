using Newtonsoft.Json;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Input.Patterns;
using Synfron.Staxe.Matcher.Interop.Model;
using System.Collections.Generic;
using System.IO;

namespace StaxeTests.Shared.Matcher
{
	public class LanguageMatcherProvider
	{
		private static readonly Dictionary<string, LanguageMatcher> _matcherMap = new Dictionary<string, LanguageMatcher>();

		public static LanguageMatcher GetStaxeTestExpressionLangMatcher()
		{
			lock (_matcherMap)
			{
				if (!_matcherMap.TryGetValue(Language.StaxeTestExpressionLang.ToString(), out LanguageMatcher languageMatcher))
				{
					string definition = File.ReadAllText("TestExpressionLang/Files/StaxeTestExpressionLangDefinition.json");
					LanguageMatcherDefinition languageMatcherModel = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definition);
					languageMatcher = DefinitionConverter.Convert(languageMatcherModel);
					_matcherMap[Language.StaxeTestExpressionLang.ToString()] = languageMatcher;
				}
				return languageMatcher;
			}
		}

		public static LanguageMatcher GetStaxeTestSimpleLangMatcher()
		{
			lock (_matcherMap)
			{
				if (!_matcherMap.TryGetValue(Language.StaxeTestSimpleLang.ToString(), out LanguageMatcher languageMatcher))
				{
					string definition = File.ReadAllText("TestSimpleLang/Files/StaxeTestSimpleLangDefinition.json");
					LanguageMatcherDefinition languageMatcherModel = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definition);
					languageMatcher = DefinitionConverter.Convert(languageMatcherModel);
					_matcherMap[Language.StaxeTestSimpleLang.ToString()] = languageMatcher;
				}
				return languageMatcher;
			}
		}

		public static LanguageMatcher GetStaxeTestComplexLangMatcher()
		{
			lock (_matcherMap)
			{
				if (!_matcherMap.TryGetValue(Language.StaxeTestComplexLang.ToString(), out LanguageMatcher languageMatcher))
				{
					string definition = File.ReadAllText("TestComplexLang/Files/StaxeTestComplexLangDefinition.json");
					LanguageMatcherDefinition languageMatcherModel = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definition);
					languageMatcher = DefinitionConverter.Convert(languageMatcherModel);
					_matcherMap[Language.StaxeTestComplexLang.ToString()] = languageMatcher;
				}
				return languageMatcher;
			}
		}

		public static LanguageMatcher GetJsonMatcher()
		{
			lock (_matcherMap)
			{
				if (!_matcherMap.TryGetValue(Language.Json.ToString(), out LanguageMatcher languageMatcher))
				{
					PatternMatcher stringLiteralPattern = PatternReader.LazyParse(id: 1, name: "String", pattern: "\"((\"!.)|\\\\.)+\"");
					PatternMatcher nullPattern = PatternReader.LazyParse(id: 2, name: "Null", pattern: "`~null");
					PatternMatcher booleanPattern = PatternReader.LazyParse(id: 3, name: "Boolean", pattern: "`~true|false");
					PatternMatcher numberPattern = PatternReader.LazyParse(id: 4, name: "Number", pattern: "~-?(\\d*\\.\\d+(e(-|\\+)?\\d+)?|\\d+)");
					PatternMatcher colonSeparatorPattern = PatternReader.LazyParse(id: 5, name: "ColonSeparator", pattern: "\\s*:\\s*");
					PatternMatcher commaSeparatorPattern = PatternReader.LazyParse(id: 6, name: "CommaSeparator", pattern: "\\s*,\\s*");
					PatternMatcher openBracePattern = PatternReader.LazyParse(id: 7, name: "OpenBrace", pattern: "\\{");
					PatternMatcher closeBracePattern = PatternReader.LazyParse(id: 8, name: "CloseBrace", pattern: "\\}");
					PatternMatcher openBracketPattern = PatternReader.LazyParse(id: 9, name: "OpenBracket", pattern: "\\[");
					PatternMatcher closeBracketPattern = PatternReader.LazyParse(id: 10, name: "CloseBracket", pattern: "\\]");
					PatternMatcher whitespacePattern = PatternReader.LazyParse(id: 11, name: "Whitespace", pattern: "\\s+");

					FragmentMatcher keyValueFragment = new FragmentMatcher(id: 5, name: "KeyValue", parts: new IMatcher[2] { stringLiteralPattern, null }, partsMatchMode: MatchMode.Ordered, partsDelimiter: colonSeparatorPattern);
					FragmentMatcher objectFragment = new FragmentMatcher(id: 4, name: "Object", parts: new IMatcher[] { keyValueFragment }, partsMatchMode: MatchMode.Multiple, partsDelimiter: commaSeparatorPattern, minMatchedParts: 0, partsPadding: whitespacePattern, start: openBracePattern, end: closeBracePattern);
					FragmentMatcher arrayFragment = new FragmentMatcher(id: 3, name: "Array", parts: new IMatcher[1], partsMatchMode: MatchMode.Multiple, partsDelimiter: commaSeparatorPattern, minMatchedParts: 0, partsPadding: whitespacePattern, start: openBracketPattern, end: closeBracketPattern);
					FragmentMatcher itemFragment = new FragmentMatcher(id: 2, name: "Item", parts: new IMatcher[] { objectFragment, arrayFragment, booleanPattern, nullPattern, stringLiteralPattern, numberPattern }, partsMatchMode: MatchMode.One, fallThroughMode: FallThroughMode.All);
					arrayFragment.Parts[0] = itemFragment;
					keyValueFragment.Parts[1] = itemFragment;
					FragmentMatcher jsonFragment = new FragmentMatcher(id: 1, name: "Json", parts: new IMatcher[] { objectFragment, arrayFragment }, partsMatchMode: MatchMode.One, partsPadding: whitespacePattern);

					languageMatcher = new LanguageMatcher
					{
						Name = Language.Json.ToString(),
						IndexingMode = IndexingMode.Lazy,
						StartingFragment = jsonFragment,
						Fragments = new List<FragmentMatcher> { keyValueFragment, objectFragment, arrayFragment, itemFragment, jsonFragment },
						Patterns = new List<PatternMatcher> { stringLiteralPattern, nullPattern, booleanPattern, numberPattern, colonSeparatorPattern, commaSeparatorPattern, openBracePattern, closeBracePattern, openBracketPattern, closeBracketPattern, whitespacePattern }
					};
					_matcherMap[Language.Json.ToString()] = languageMatcher;
				}
				return languageMatcher;
			}
		}
	}
}
