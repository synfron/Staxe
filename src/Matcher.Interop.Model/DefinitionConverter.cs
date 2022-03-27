using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Input.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Matcher.Interop.Model
{
	public static class DefinitionConverter
	{

		public static LanguageMatcher Convert(LanguageMatcherDefinition languageMatcherModel)
		{
			Dictionary<string, PatternMatcher> patternMatcherMap = new Dictionary<string, PatternMatcher>(StringComparer.Ordinal);
			List<PatternMatcher> patternMatchers = new List<PatternMatcher>(languageMatcherModel.Patterns.Count);
			Dictionary<string, FragmentMatcher> fragmentMatcherMap = new Dictionary<string, FragmentMatcher>(StringComparer.Ordinal);
			List<FragmentMatcher> fragmentMatchers = new List<FragmentMatcher>(languageMatcherModel.Fragments.Count);

			bool isEagar = languageMatcherModel.IndexingMode == IndexingMode.Eager;

			for (int patternIndex = 0; patternIndex < languageMatcherModel.Patterns.Count; patternIndex++)
			{
				PatternMatcherDefinition model = languageMatcherModel.Patterns[patternIndex];
				PatternMatcher patternMatcher = isEagar ? PatternReader.Parse(patternIndex + 1, model.Name, model.Pattern) : PatternReader.LazyParse(patternIndex + 1, model.Name, model.Pattern);
				patternMatcher.IsNoise = model.IsNoise;
				patternMatcher.Mergable = model.Mergable;
				patternMatcherMap.Add(patternMatcher.Name, patternMatcher);
				patternMatchers.Add(patternMatcher);
			}

			for (int matcherIndex = 0; matcherIndex < languageMatcherModel.Fragments.Count; matcherIndex++)
			{
				FragmentMatcherDefinition model = languageMatcherModel.Fragments[matcherIndex];
				FragmentMatcher fragmentMatcher = new FragmentMatcher
				(
					id: matcherIndex + 1,
					name: model.Name,
					parts: new List<IMatcher>(),
					fallThroughMode: model.FallThroughMode,
					isNoise: model.IsNoise,
					partsDelimiterRequired: model.PartsDelimiterRequired,
					partsMatchMode: model.PartsMatchMode,
					minMatchedParts: model.MinMatchedParts,
					cacheable: model.Cacheable,
					clearCache: model.ClearCache,
					expressionMode: model.ExpressionMode,
					expressionOrder: model.ExpressionOrder,
					boundsAsParts: model.BoundsAsParts,
					discardBounds: model.DiscardBounds,
					end: model.End != null ? patternMatcherMap[model.End] : null,
					partsDelimiter: model.PartsDelimiter != null ? patternMatcherMap[model.PartsDelimiter] : null,
					partsPadding: model.PartsPadding != null ? patternMatcherMap[model.PartsPadding] : null,
					start: model.Start != null ? patternMatcherMap[model.Start] : null,
					negate: model.Negate
				);
				fragmentMatcherMap.Add(fragmentMatcher.Name, fragmentMatcher);
				fragmentMatchers.Add(fragmentMatcher);
			};

			foreach (FragmentMatcherDefinition model in languageMatcherModel.Fragments)
			{
				FragmentMatcher fragmentMatcher = fragmentMatcherMap[model.Name];
				foreach (IMatcher part in model.Parts.Select(GetPartMatcher))
				{
					fragmentMatcher.Parts.Add(part);
				}
			}

			FragmentMatcher startingMatcher = fragmentMatcherMap[languageMatcherModel.StartingFragment];

			return new LanguageMatcher
			{
				Name = languageMatcherModel.Name,
				Fragments = fragmentMatchers,
				Patterns = patternMatchers,
				StartingFragment = startingMatcher,
				LogMatches = languageMatcherModel.LogMatches,
				IndexingMode = languageMatcherModel.IndexingMode
			};

			IMatcher GetPartMatcher(string name)
			{
				return name.StartsWith("[") ? fragmentMatcherMap[name.Substring(1, name.Length - 2)] : (IMatcher)patternMatcherMap[name];
			}
		}
		public static LanguageMatcherDefinition Convert(LanguageMatcher languageMatcher)
		{

			HashSet<string> convertedPatterns = new HashSet<string>();
			List<PatternMatcherDefinition> patterns = new List<PatternMatcherDefinition>(languageMatcher.Patterns.Select(ConvertPattern));
			List<FragmentMatcherDefinition> fragments = new List<FragmentMatcherDefinition>(languageMatcher.Fragments.Select(ConvertFragment));

			LanguageMatcherDefinition matcherDefinition = new LanguageMatcherDefinition
			{
				Name = languageMatcher.Name,
				LogMatches = languageMatcher.LogMatches,
				IndexingMode = languageMatcher.IndexingMode,
				StartingFragment = languageMatcher.StartingFragment.Name,
				Patterns = patterns,
				Fragments = fragments
			};

			return matcherDefinition;

			PatternMatcherDefinition ConvertPattern(PatternMatcher patternMatcher)
			{
				convertedPatterns.Add(patternMatcher.Name);
				return new PatternMatcherDefinition
				{
					IsNoise = patternMatcher.IsNoise,
					Mergable = patternMatcher.Mergable,
					Name = patternMatcher.Name,
					Pattern = patternMatcher is GroupPatternMatcher groupPatternMatcher ? groupPatternMatcher.ToString(true) : patternMatcher.ToString()
				};
			};

			string AddPatternAndGetName(PatternMatcher patternMatcher)
			{
				if (patternMatcher != null && !convertedPatterns.Contains(patternMatcher.Name))
				{
					patterns.Add(ConvertPattern(patternMatcher));
				}
				return patternMatcher?.Name;
			};

			FragmentMatcherDefinition ConvertFragment(FragmentMatcher fragmentMatcher)
			{
				return new FragmentMatcherDefinition
				{
					BoundsAsParts = fragmentMatcher.BoundsAsParts,
					Cacheable = fragmentMatcher.Cacheable,
					ClearCache = fragmentMatcher.ClearCache,
					DiscardBounds = fragmentMatcher.DiscardBounds,
					ExpressionMode = fragmentMatcher.ExpressionMode,
					ExpressionOrder = fragmentMatcher.ExpressionOrder,
					FallThroughMode = fragmentMatcher.FallThroughMode,
					IsNoise = fragmentMatcher.IsNoise,
					MinMatchedParts = fragmentMatcher.MinMatchedParts,
					Negate = fragmentMatcher.Negate,
					Start = AddPatternAndGetName(fragmentMatcher.Start),
					End = AddPatternAndGetName(fragmentMatcher.End),
					Name = fragmentMatcher.Name,
					PartsDelimiter = AddPatternAndGetName(fragmentMatcher.PartsDelimiter),
					PartsDelimiterRequired = fragmentMatcher.PartsDelimiterRequired,
					PartsMatchMode = fragmentMatcher.PartsMatchMode,
					PartsPadding = AddPatternAndGetName(fragmentMatcher.PartsPadding),
					Parts = fragmentMatcher.Parts.Select(part =>
					{
						return part is FragmentMatcher fragmentPart ? $"[{fragmentPart.Name}]" : AddPatternAndGetName((PatternMatcher)part);
					}).ToList()
				};
			};
		}
	}
}
