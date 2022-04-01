using Synfron.Staxe.Matcher.Data;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Input.Actions;
using Synfron.Staxe.Matcher.Input.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Synfron.Staxe.Matcher
{
    public abstract class LanguageMatchEngine : AbstractLanguageMatchEngine
    {
        private readonly bool _logMatches;

        private sealed class EagerIndexLanguageMatchEngine : LanguageMatchEngine
        {
            public EagerIndexLanguageMatchEngine(LanguageMatcher languageMatcher) : base(languageMatcher)
            {
            }

            protected override void BuildState(ref State state, string code)
            {
                state.Code = code;
                state.DistinctStringMatches = new List<StringMatchData>(2000);
                state.MatchLogBuilder = new StringBuilder();
                state.MatchCache = new Dictionary<ValueTuple<string, int>, FragmentMatchData>();

                state.PreMatchSuccess = PreMatchPatterns(ref state);
            }

            private bool PreMatchPatterns(ref State state)
            {
                int codeLength = state.Code.Length;
                IList<PatternMatcher> patterns = LanguageMatcher.Patterns;
                int patternsCount = LanguageMatcher.Patterns.Count;
                bool success = true;
                bool previousNoise = false;

                int currentIndex;
                while ((currentIndex = state.CurrentIndex) < codeLength)
                {
                    success = false;
                    for (int patternIndex = 0; patternIndex < patternsCount; patternIndex++)
                    {
                        PatternMatcher pattern = patterns[patternIndex];
                        StringMatchData matchData; 
                        (success, matchData) = PreMatchPattern(ref state, pattern);
                        if (success)
                        {
                            if (pattern.IsStandard)
                            {
                                if (_logMatches)
                                {
                                    state.MatchLogBuilder.AppendLine($"{currentIndex}. Prematched {matchData.Name}: {matchData.Text}");
                                }
                                if (matchData.IsNoise)
                                {
                                    previousNoise = true;
                                }
                                else if (previousNoise)
                                {
                                    if (state.DistinctIndex > 1)
                                    {
                                        StringMatchData previousMatchData = state.DistinctStringMatches[state.DistinctIndex - 2];
                                        if (previousMatchData.Name == matchData.Name && previousMatchData.Mergable)
                                        {
                                            previousMatchData.Text += matchData.Text;
                                            previousMatchData.Length = state.CurrentIndex - previousMatchData.StartIndex;
                                            state.DistinctIndex--;
                                            state.MaxDistinctIndex--;
                                            state.DistinctStringMatches.RemoveAt(state.DistinctIndex);
                                        }
                                    }
                                    previousNoise = false;
                                }
                            }
                            break;
                        }
                    }
                    if (!success)
                    {
                        break;
                    }
                }
                state.CurrentIndex = 0;
                state.DistinctIndex = 0;
                return success;
            }

            private (bool, StringMatchData) PreMatchPattern(ref State state, PatternMatcher pattern)
            {
                int startOffset = state.CurrentIndex;
                if (pattern.IsStandard)
                {
                    StringMatchData stringMatchData = null;
                    (bool success, int length) = pattern.IsMatch(state.Code, state.CurrentIndex);
                    if (success)
                    {
                        stringMatchData = new StringMatchData
                        {
                            Name = pattern.Name,
                            Text = state.Code.Substring(startOffset, length),
                            StartIndex = startOffset,
                            Length = length,
                            Id = pattern.Id,
                            IsNoise = pattern.IsNoise,
                            Mergable = pattern.Mergable
                        };
                        if (!stringMatchData.IsNoise)
                        {
                            state.DistinctStringMatches.Add(stringMatchData);
                            state.DistinctIndex++;
                            state.MaxDistinctIndex++;
                        }
                        state.CurrentIndex = startOffset + length;
                        state.MaxIndex = state.CurrentIndex;
                    }
                    return (success, stringMatchData);
                }
                else if (pattern is FragmentPatternMatcher fragmentPatternMatcher && fragmentPatternMatcher.Fragment is FragmentMatcher fragmentMatcher)
                {
                    int startIndex = state.CurrentIndex;
                    int distinctStringMatchesCount = state.DistinctStringMatches.Count;
                    int distinctIndex = state.DistinctIndex;
                    int maxDistinctIndex = state.MaxDistinctIndex;

                    (bool _, FragmentMatchData partMatcherData) = MatchByFragmentMatcher(ref state, fragmentPatternMatcher.Fragment);
                    if (fragmentMatcher.Cacheable)
                    {
                        state.MatchCache[new ValueTuple<string, int>(fragmentMatcher.Name, startIndex)] = partMatcherData;
                    }
                    if (pattern.IsNoise)
                    {
                        state.DistinctStringMatches.RemoveRange(distinctStringMatchesCount, state.DistinctStringMatches.Count - distinctStringMatchesCount);
                        state.DistinctIndex = distinctIndex;
                        state.MaxDistinctIndex = maxDistinctIndex;
                    }
                    state.CurrentIndex = state.MaxIndex;
                    bool success = state.CurrentIndex > startIndex;
                    return (success, null);
                }
                return (false, null);
            }
        }

        private sealed class NoIndexLanguageMatchEngine : LanguageMatchEngine
        {
            public NoIndexLanguageMatchEngine(LanguageMatcher languageMatcher) : base(languageMatcher)
            {
            }

            protected override void BuildState(ref State state, string code)
            {
                state.Code = code;
                state.MatchLogBuilder = new StringBuilder();
                state.MatchCache = new Dictionary<ValueTuple<string, int>, FragmentMatchData>();
            }

            protected override (bool success, StringMatchData matchData) MatchPattern(ref State state, PatternMatcher pattern, bool required, bool readOnly = false)
            {
                if (pattern != null)
                {
                    int startOffset = state.CurrentIndex;
                    (bool success, int length) = pattern.IsMatch(state.Code, state.CurrentIndex);
                    StringMatchData stringMatchData = default;
                    if (success)
                    {
                        stringMatchData = new StringMatchData
                        {
                            Name = pattern.Name,
                            Text = state.Code.Substring(startOffset, length),
                            StartIndex = startOffset,
                            Length = length,
                            Id = pattern.Id,
                            IsNoise = pattern.IsNoise,
                            Mergable = pattern.Mergable
                        };
                        if (!readOnly)
                        {
                            state.CurrentIndex = startOffset + length;
                            state.MaxIndex = Math.Max(state.MaxIndex, state.CurrentIndex);
                        }
                    }
                    else if (!required)
                    {
                        success = true;
                    }
                    return (success, stringMatchData);

                }
                return (true, default);
            }
        }

        private sealed class LazyIndexLanguageMatchEngine : LanguageMatchEngine
        {
            public LazyIndexLanguageMatchEngine(LanguageMatcher languageMatcher) : base(languageMatcher)
            {
            }

            protected override void BuildState(ref State state, string code)
            {
                state.Code = code;
                state.DistinctStringMatches = new List<StringMatchData>(2000);
                state.MatchLogBuilder = new StringBuilder();
                state.MatchCache = new Dictionary<ValueTuple<string, int>, FragmentMatchData>();
            }
        }

        public static LanguageMatchEngine Build(LanguageMatcher languageMatcher)
        {
            switch (languageMatcher.IndexingMode)
            {
                case IndexingMode.Lazy:
                    return new LazyIndexLanguageMatchEngine(languageMatcher);
                case IndexingMode.Eager:
                    return new EagerIndexLanguageMatchEngine(languageMatcher);
                default:
                    return new NoIndexLanguageMatchEngine(languageMatcher);
            }
        }

        public LanguageMatchEngine(LanguageMatcher languageMatcher)
        {
            LanguageMatcher = languageMatcher;
            _logMatches = languageMatcher.LogMatches;
        }

        public LanguageMatcher LanguageMatcher { get; private set; }

        #region Post-Gen

        public override MatcherResult Match(string code, bool matchFullText = true)
        {
            return Match(code, LanguageMatcher.StartingFragment, matchFullText);
        }

        public override MatcherResult Match(string code, string fragmentMatcher, bool matchFullText = true)
        {
            return Match(code, LanguageMatcher.Fragments.First(matcher => matcher.Name == fragmentMatcher), matchFullText);
        }

        public MatcherResult Match(string code, FragmentMatcher startingMatcher = null, bool matchFullText = true)
        {

            FragmentMatchData matchData = new FragmentMatchData
            {
                StartIndex = 0
            };

            State state = new State();
            if (LanguageMatcher.Blobs.Count > 0) {
                state.BlobDatas = new Span<BlobData>(new BlobData[LanguageMatcher.Blobs.Count]);
            }
            BuildState(ref state, code);

            bool success = MatchPartByFragmentMatcher(ref state, matchData, startingMatcher ?? LanguageMatcher.StartingFragment);

            IMatchData resultMatchData = matchData?.Parts.FirstOrDefault();
            int? failureIndex = success ? null : state.FailureIndex;

            if (
                success && 
                matchFullText && 
                state.CurrentIndex != (state.PreMatchSuccess ? 
                    state.DistinctStringMatches.LastOrDefault().GetEndIndex() : state.Code.Length
                ))
            {
                success = false;
                failureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
            }

            return new MatcherResult(resultMatchData, success, state.CurrentIndex, failureIndex, _logMatches ? state.MatchLogBuilder.ToString() : string.Empty);
        }

        protected virtual (bool success, StringMatchData matchData) MatchPattern(ref State state, PatternMatcher pattern, bool required, bool readOnly = false)
        {
            if (pattern == null)
            {
                return (true, default);
            }
            bool success = false;
            int distinctIndex = state.DistinctIndex;
            if (distinctIndex < state.MaxDistinctIndex)
            {
                StringMatchData stringMatchData = state.DistinctStringMatches[distinctIndex];
                if (stringMatchData != null)
                {
                    success = stringMatchData.Id == pattern.Id;
                }
                if (success && !readOnly)
                {
                    state.DistinctIndex++;
                    state.CurrentIndex = stringMatchData.StartIndex + stringMatchData.Length;
                }
                else if (!required)
                {
                    success = true;
                }
                return (success, stringMatchData);
            }
            else if (state.CurrentIndex < state.Code.Length)
            {
                int length;
                int startOffset = state.CurrentIndex;
                (success, length) = pattern.IsMatch(state.Code, state.CurrentIndex);
                StringMatchData stringMatchData = default;
                if (success)
                {
                    stringMatchData = new StringMatchData
                    {
                        Name = pattern.Name,
                        Text = state.Code.Substring(startOffset, length),
                        StartIndex = startOffset,
                        Length = length,
                        Id = pattern.Id,
                        IsNoise = pattern.IsNoise,
                        Mergable = pattern.Mergable
                    };
                    if (!pattern.IsNoise)
                    {
                        state.DistinctStringMatches.Add(stringMatchData);
                        state.MaxDistinctIndex++;
                        state.DistinctIndex++;
                    }
                    if (!readOnly)
                    {
                        state.CurrentIndex = startOffset + length;
                        state.MaxIndex = Math.Max(state.MaxIndex, state.CurrentIndex);
                    }
                }
                else if (!required)
                {
                    success = true;
                }
                return (success, stringMatchData);
            }
            return (false, default);
        }

        private bool MatchPartByFragmentMatcher(ref State state, FragmentMatchData matchData, FragmentMatcher part)
        {
            bool success;
            bool negate = part.Negate;
            if (!part.Cacheable || !state.MatchCache.TryGetValue(new ValueTuple<string, int>(part.Name, state.CurrentIndex), out FragmentMatchData partMatcherData))
            {
                int startIndex = state.CurrentIndex;
                int distinctIndex = state.DistinctIndex;
                (success, partMatcherData) = MatchByFragmentMatcher(ref state, part);
                if (part.Cacheable)
                {
                    state.MatchCache[new ValueTuple<string, int>(part.Name, startIndex)] = partMatcherData;
                }
                if (!success || negate)
                {
                    state.CurrentIndex = startIndex;
                    state.DistinctIndex = distinctIndex;
                }
            }
            else if ((success = partMatcherData != null) && !negate)
            {
                state.CurrentIndex = partMatcherData.StartIndex + partMatcherData.Length;
                state.MaxIndex = Math.Max(state.MaxIndex, state.CurrentIndex);
                state.DistinctIndex = partMatcherData.EndDistinctIndex;
            }
            if (success && !negate)
            {
                if (!part.IsNoise)
                {
                    if (part.FallThroughMode == FallThroughMode.All || partMatcherData.Parts.Count <= (int)part.FallThroughMode)
                    {
                        matchData.Parts.AddRange(partMatcherData.Parts);
                    }
                    else
                    {
                        matchData.Parts.Add(partMatcherData);
                    }
                }
                if (part.ClearCache)
                {
                    state.MatchCache.Clear();
                }
            }
            return success ^ negate;
        }

        protected (bool, FragmentMatchData) MatchByFragmentMatcher(ref State state, FragmentMatcher matcher)
        {
            FragmentMatchData matchData = new FragmentMatchData
            {
                Name = matcher.Name,
                StartIndex = state.CurrentIndex,
                ExpressionOrder = matcher.ExpressionOrder
            };
            StringMatchData endMatchData = default;
            bool success = MatchFragmentBounds(ref state, matcher.Start, matcher, matcher.DiscardBounds, out StringMatchData startMatchData) && MatchFragmentParts(ref state, matcher, matchData) && MatchFragmentBounds(ref state, matcher.End, matcher, matcher.DiscardBounds, out endMatchData);

            if (success && matcher.Actions != null)
            {
                IEnumerator<MatcherAction> actionEnumerator = matcher.Actions.GetEnumerator();
                actionEnumerator.Reset();
                while (success && actionEnumerator.MoveNext())
                {
                    MatcherAction action = actionEnumerator.Current;
                    success = action.Perform(state.BlobDatas, matchData.Parts);
                }
            }

            if (success)
            {
                if (!matcher.Negate)
                {
                    matchData.Length = state.CurrentIndex - matchData.StartIndex;
                    matchData.EndDistinctIndex = state.DistinctIndex;
                    if (matcher.ExpressionMode != ExpressionMode.None)
                    {
                        ConvertToExpressionTree(matchData, matcher.ExpressionMode);
                    }
                    if (matcher.BoundsAsParts)
                    {
                        if (startMatchData != null)
                        {
                            matchData.Parts.Insert(0, startMatchData);
                        }
                        if (endMatchData != null)
                        {
                            matchData.Parts.Add(endMatchData);
                        }
                    }
                }
                return (true, matchData);
            }
            return (false, null);
        }

        private bool MatchFragmentParts(ref State state, FragmentMatcher matcher, FragmentMatchData matchData)
        {
            bool success = true;
            if (matcher.Parts.Count > 0)
            {
                switch (matcher.PartsMatchMode)
                {
                    case MatchMode.Multiple:
                        success = MatchFragmentPartsMultipleMode(ref state, matcher, matchData);
                        break;
                    case MatchMode.One:
                        success = MatchFragmentPartsOneMode(ref state, matcher, matchData);
                        break;
                    case MatchMode.Ordered:
                        success = MatchFragmentPartsOrderedMode(ref state, matcher, matchData);
                        break;
                }
            }
            if (!success ^ matcher.Negate)
            {
                state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
            }
            return success;
        }

        private bool MatchFragmentPartsMultipleMode(ref State state, FragmentMatcher matcher, FragmentMatchData matchData)
        {
            bool overallSuccess = false;
            bool subSuccess;
            bool delimiterSuccess = false;
            StringMatchData range = default;
            int matchCount = 0;
            int distinctIndex = state.DistinctIndex;
            MatchPattern(ref state, matcher.PartsPadding, false);
            do
            {
                subSuccess = false;
                foreach (IMatcher part in matcher.Parts)
                {
                    bool individualSuccess = MatchFragmentPart(ref state, matchData, part);
                    subSuccess |= individualSuccess;
                    if (individualSuccess)
                    {
                        matchCount++;
                        distinctIndex = state.DistinctIndex;
                        (delimiterSuccess, range) = MatchPattern(ref state, matcher.PartsDelimiter, matcher.PartsDelimiterRequired);
                        break;
                    }
                }
                overallSuccess |= subSuccess;
            }
            while (subSuccess && delimiterSuccess);
            if (delimiterSuccess && range != null)
            {
                state.CurrentIndex = range.StartIndex;
                state.DistinctIndex = distinctIndex;
            }
            if (overallSuccess)
            {
                MatchPattern(ref state, matcher.PartsPadding, false);
            }
            bool thresholdSuccess = (matcher.MinMatchedParts ?? 1) <= matchCount;
            return overallSuccess && thresholdSuccess;
        }

        private bool MatchFragmentPartsOneMode(ref State state, FragmentMatcher matcher, FragmentMatchData matchData)
        {
            bool success = false;
            int matchCount = 0;
            MatchPattern(ref state, matcher.PartsPadding, false);
            foreach (IMatcher part in matcher.Parts)
            {
                success = MatchFragmentPart(ref state, matchData, part);
                if (success)
                {
                    matchCount++;
                    break;
                }
            }
            if (success)
            {
                MatchPattern(ref state, matcher.PartsPadding, false);
            }
            bool thresholdSuccess = (matcher.MinMatchedParts ?? 1) <= 0 || matchCount > 0;
            return thresholdSuccess;
        }

        private bool MatchFragmentPartsOrderedMode(ref State state, FragmentMatcher matcher, FragmentMatchData matchData)
        {
            bool success = true;
            bool partSuccess;
            int matchCount = 0;
            StringMatchData stringMatchData = default;
            int distinctIndex = state.DistinctIndex;
            MatchPattern(ref state, matcher.PartsPadding, false);
            for (int partIndex = 0; partIndex < matcher.Parts.Count; partIndex++)
            {
                if (partIndex > 0)
                {
                    distinctIndex = state.DistinctIndex;
                    (partSuccess, stringMatchData) = MatchPattern(ref state, matcher.PartsDelimiter, matcher.PartsDelimiterRequired);
                    success = partSuccess;
                    if (!success)
                    {
                        break;
                    }
                }

                IMatcher part = matcher.Parts[partIndex];
                success = MatchFragmentPart(ref state, matchData, part);
                if (!success)
                {
                    if (stringMatchData != null)
                    {
                        state.CurrentIndex = stringMatchData.StartIndex;
                        state.DistinctIndex = distinctIndex;
                    }
                    break;
                }
                else
                {
                    matchCount++;
                }
            }
            if (success)
            {
                MatchPattern(ref state, matcher.PartsPadding, false);
            }
            bool thresholdSuccess = (matcher.MinMatchedParts ?? matcher.Parts.Count) <= matchCount;
            return success || thresholdSuccess;
        }

        private bool MatchFragmentPart(ref State state, FragmentMatchData matchData, IMatcher part)
        {
            int currentId = ++state.Id;
            if (_logMatches)
            {
                state.MatchLogBuilder.AppendLine($"{new string('\t', currentId)} {state.CurrentIndex}. Try: {part}");
            }
            bool success = false;
            switch (part)
            {
                case FragmentMatcher partFragmentMatcher:
                    success = MatchPartByFragmentMatcher(ref state, matchData, partFragmentMatcher);
                    break;
                case PatternMatcher partPatternMatcher:
                    success = MatchPartByTextMatcher(ref state, matchData, partPatternMatcher);
                    break;
            }

            if (_logMatches)
            {
                state.MatchLogBuilder.AppendLine($"{new string('\t', currentId)} {state.CurrentIndex}. {(success ? "Passed" : "Failed")}: {part}");
            }
            state.Id = currentId - 1;
            return success;
        }

        private bool MatchPartByTextMatcher(ref State state, FragmentMatchData matchData, PatternMatcher part)
        {
            (bool success, StringMatchData stringMatchData) = MatchPattern(ref state, part, true);
            if (success)
            {
                matchData.Parts.Add(stringMatchData);
                if (_logMatches)
                {
                    state.MatchLogBuilder.AppendLine($"{new string('\t', state.Id + 1)} {state.CurrentIndex}. Matched: {stringMatchData.Text}");
                }
            }
            return success;
        }

        private bool MatchFragmentBounds(ref State state, PatternMatcher patternMatcher, FragmentMatcher matcher, bool readOnly, out StringMatchData matchData)
        {
            bool success;
            (success, matchData) = MatchPattern(ref state, patternMatcher, true, readOnly);
            if (!success ^ matcher.Negate)
            {
                state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
            }
            if (patternMatcher != null && _logMatches)
            {
                state.MatchLogBuilder.AppendLine($"{new string('\t', state.Id + 1)} {state.CurrentIndex}. {(success ? "Passed" : "Failed")} Bounds: {patternMatcher}");
            }
            return success;
        }

        protected abstract void BuildState(ref State state, string code);

        #endregion

        #region Pre-Gen


        internal static string Generate(MatcherEngineGenerator generator)
        {
            string version = AssemblyName.GetAssemblyName(generator.GetType().Assembly.Location).Version.ToString();
            return $@"
/* Generated by Synfron.Staxe.Matcher v{version}*/

using Synfron.Staxe.Matcher.Data;
using System.Collections.Generic;
using System;
using System.Linq;
using Synfron.Staxe.Matcher.Input;
{(generator.LanguageMatcher.LogMatches ? "using System.Text;" : null)}

namespace Synfron.Staxe.Matcher
{{
    public class {GetSafeMethodName(generator.LanguageMatcher.Name)}MatchEngine : AbstractLanguageMatchEngine
    {{

        {GenerateMatch(generator)}

        <<Generated Methods>>
    }}
}}";
        }

        private static string GenerateMatch(MatcherEngineGenerator generator)
        {
            LanguageMatcher languageMatcher = generator.LanguageMatcher;
            return $@"
        public override MatcherResult Match(string code, string fragmentMatcher, bool matchFullText = true)
        {{
            FragmentMatchData matchData = new FragmentMatchData
            {{
                StartIndex = 0
            }};

            State state = new State()
            {{
                Code = code{(languageMatcher.IndexingMode != IndexingMode.None ? @",
				DistinctStringMatches = new List<StringMatchData>(2000)" : null)}{(languageMatcher.Fragments.Any(fragment => fragment.Cacheable) ? @",
				MatchCache = new Dictionary<ValueTuple<string, int>, FragmentMatchData>()" : null)}{(languageMatcher.LogMatches ? $@",
				MatchLogBuilder = new StringBuilder()" : null)}{(languageMatcher.Blobs.Count > 0 ? $@",
				BlobDatas = new Span<BlobData>(new BlobData[{languageMatcher.Blobs.Count}]);" : null)}
            }};
            
			{(languageMatcher.IndexingMode == IndexingMode.Eager ? "state.PreMatchSuccess = PreMatchPatterns(ref state);" : null)}

			bool success = false;
            switch (fragmentMatcher) 
            {{
                {string.Join("\n", languageMatcher.Fragments.Where(matcher => !matcher.IsNoise).Select(matcher => $@"
                case ""{HttpUtility.JavaScriptStringEncode(matcher.Name)}"":
                        success = {Generate(generator, matcher)};
                    break;
                "))}
            }}

			IMatchData resultMatchData = matchData.Parts.FirstOrDefault();
			int? failureIndex = success ? null : state.FailureIndex;

			if (success && matchFullText && (state.PreMatchSuccess ? state.MaxIndex : state.CurrentIndex) != state.Code.Length)
			{{
				success = false;
				failureIndex = state.CurrentIndex;
			}}

			return new MatcherResult(resultMatchData, success, state.CurrentIndex, failureIndex, state.MatchLogBuilder?.ToString());
        }}

        public override MatcherResult Match(string code, bool matchFullText = true)
        {{
            FragmentMatchData matchData = new FragmentMatchData
            {{
                StartIndex = 0
            }};
            
            State state = new State()
            {{
                Code = code{(languageMatcher.IndexingMode != IndexingMode.None ? @",
				DistinctStringMatches = new List<StringMatchData>(2000)" : null)}{(languageMatcher.Fragments.Any(fragment => fragment.Cacheable) ? @",
				MatchCache = new Dictionary<ValueTuple<string, int>, FragmentMatchData>()" : null)}{(languageMatcher.LogMatches ? $@",
				MatchLogBuilder = new StringBuilder()" : null)}
            }};
            
			{(languageMatcher.IndexingMode == IndexingMode.Eager ? "PreMatchPatterns(ref state);" : null)}

            bool success = {Generate(generator, languageMatcher.StartingFragment)};

			IMatchData resultMatchData = matchData?.Parts.FirstOrDefault();
			int? failureIndex = success ? null : state.FailureIndex;

			if (success && matchFullText && state.CurrentIndex != state.Code.Length)
			{{
				success = false;
				failureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
			}}

			return new MatcherResult(resultMatchData, success, state.CurrentIndex, failureIndex, state.MatchLogBuilder?.ToString());
        }}

		{(languageMatcher.IndexingMode == IndexingMode.Eager ? $@"private bool PreMatchPatterns(ref State state)
		{{
			int codeLength = state.Code.Length;
			bool success = true;
			bool previousNoise = false;
			StringMatchData matchData = null;
			int currentIndex = 0;
			while ((currentIndex = state.CurrentIndex) < codeLength)
			{{
				success = {string.Join(" ||\n", languageMatcher.Patterns.Select(pattern => $@"{string.Format(GenerateMatchPattern(generator, pattern), "matchData", "true", "false")}"))};
				if (!success)
				{{
					break;
				}}
				else if (matchData != null) {{
					{(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{currentIndex}}. Prematched {{matchData.Name}}: {{matchData.Text}}"");" : null)}
					if (matchData.IsNoise)
					{{
						previousNoise = true;
					}}
					else if (previousNoise)
					{{
						if (state.DistinctIndex > 1)
						{{
							StringMatchData previousMatchData = state.DistinctStringMatches[state.DistinctIndex - 2];
							if (previousMatchData.Name == matchData.Name && previousMatchData.Mergable)
							{{
								previousMatchData.Text += matchData.Text;
								previousMatchData.Length = state.CurrentIndex - previousMatchData.StartIndex;
								state.DistinctIndex--;
								state.MaxDistinctIndex--;
								state.DistinctStringMatches.RemoveAt(state.DistinctIndex);
							}}
						}}
						previousNoise = false;
					}}
				}}
			}}
			state.CurrentIndex = 0;
			{(languageMatcher.IndexingMode != IndexingMode.None ? "state.DistinctIndex = 0;" : null)}
			return success;
		}}" : null)}";
        }


        private static string GenerateMatchPattern(MatcherEngineGenerator generator, PatternMatcher pattern)
        {
            string methodName = $"MatchPattern{GetSafeMethodName(pattern.Name)}";
            string method = $"{methodName}(ref state, out {{0}}, {{1}}, {{2}})";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, out StringMatchData matchData, bool required, bool readOnly = false)
            {{
                bool success = false;
                {(generator.IndexingMode != IndexingMode.None && pattern.IsStandard ? $@"int distinctIndex = state.DistinctIndex;
                if (distinctIndex >= state.MaxDistinctIndex)
                {{" : null)}
                    int length;
                    int startOffset = state.CurrentIndex;
                    (success, length) = {GenerateRawMatchPattern(generator, pattern)};
					matchData = default;
					if (success)
					{{
						matchData = new StringMatchData
						{{
							Name = ""{HttpUtility.JavaScriptStringEncode(pattern.Name)}"",
							Text = state.Code.Substring(startOffset, length),
							StartIndex = startOffset,
							Length = length,
							IsNoise = {pattern.IsNoise.ToString().ToLower()},
							Mergable = {pattern.Mergable.ToString().ToLower()},
							Id = {pattern.Id}
						}};
						{(!pattern.IsNoise && generator.IndexingMode != IndexingMode.None ? $@"state.DistinctStringMatches.Add(matchData);" : null)}
						{(!pattern.IsNoise && generator.IndexingMode != IndexingMode.None ? $@"state.MaxDistinctIndex++;" : null)}
						{(!pattern.IsNoise && generator.IndexingMode != IndexingMode.None ? $@"state.DistinctIndex++;" : null)}
						if (!readOnly)
						{{
							state.CurrentIndex = startOffset + length;
                            state.MaxIndex = Math.Max(state.MaxIndex, state.CurrentIndex);
						}}
					}}
					else if (!required)
					{{
						success = true;
					}}
					return success;
                {(generator.IndexingMode != IndexingMode.None && pattern.IsStandard ? $@"}}
                else
                {{
					matchData = state.DistinctStringMatches[distinctIndex];
                    if (matchData != null)
                    {{
						success = matchData.Id == {pattern.Id};
                    }}
                    else
                    {{
                        int length;
                        int startOffset = state.CurrentIndex;
                        (success, length) = {GenerateRawMatchPattern(generator, pattern)};
                        if (success)
                        {{
							matchData = new StringMatchData
							{{
								Name = ""{GetSafeMethodName(pattern.Name)}"",
								Text = state.Code.Substring(startOffset, length),
								StartIndex = startOffset,
								Length = length,
								IsNoise = {pattern.IsNoise.ToString().ToLower()},
								Mergable = {pattern.Mergable.ToString().ToLower()},
								Id = {pattern.Id}
							}};
							{(!pattern.IsNoise ? $@"state.DistinctStringMatches[distinctIndex] = matchData;" : null)}
                        }}
                    }}
					if (success && !readOnly)
					{{
						{(!pattern.IsNoise ? $@"state.DistinctIndex++;" : null)}
						state.CurrentIndex = startOffset + length;
                        state.MaxIndex = Math.Max(state.MaxIndex, state.CurrentIndex);
					}}
					if (!required)
					{{
						success = true;
					}}
					return success;
                }}" : null)}
                {(!pattern.IsStandard && pattern is FragmentPatternMatcher fragmentPatternMatcher && fragmentPatternMatcher.Fragment is FragmentMatcher fragmentMatcher ? $@"
                int startOffset = state.CurrentIndex;{(pattern.IsNoise ? $@"
                int distinctStringMatchesCount = state.DistinctStringMatches.Count;
                int distinctIndex = state.DistinctIndex;
                int maxDistinctIndex = state.MaxDistinctIndex;
                " : null)}
                FragmentMatchData partMatcherData;
				{GenerateMatchByFragmentMatcherSection(generator, fragmentMatcher)}
                {(fragmentMatcher.Cacheable ? $@"
                state.MatchCache[new ValueTuple<string, int>({fragmentMatcher.Name}, startOffset)] = partMatcherData;
                " : null)}{(pattern.IsNoise ? $@"
                state.DistinctStringMatches.RemoveRange(distinctStringMatchesCount, state.DistinctStringMatches.Count - distinctStringMatchesCount);
                state.DistinctIndex = distinctIndex;
                state.MaxDistinctIndex = maxDistinctIndex;
                " : null)}
                success = success && state.CurrentIndex > startIndex;" : null)}
            }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GenerateRawMatchPattern(MatcherEngineGenerator generator, PatternMatcher pattern)
        {
            return string.Format(pattern.Generate(generator), "state.Code", "state.CurrentIndex");
        }

        private static string GenerateMatcherAction(MatcherEngineGenerator generator, MatcherAction action)
        {
            return string.Format(action.Generate(generator), "state.BlobDatas", "partMatcherData.Parts");
        }

        private static string GenerateMatchFragmentPartsOrderedMode(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher)
        {
            string methodName = $"MatchFragmentPartsOrderedMode{GetSafeMethodName(fragmentMatcher.Name)}";
            string method = $"{methodName}(ref state, partMatcherData)";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                StringBuilder functionText = new StringBuilder();
                for (int partIndex = 0; partIndex < fragmentMatcher.Parts.Count; partIndex++)
                {
                    functionText.AppendLine($@"{(partIndex > 0 ? $@"distinctIndex = state.DistinctIndex;
                    partSuccess = {(fragmentMatcher.PartsDelimiter != null ? string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsDelimiter), "stringMatchData", fragmentMatcher.PartsDelimiterRequired.ToString().ToLower(), "false") : "true")};
                    success = partSuccess;
                    if (!success)
                    {{
                        goto Break;
                    }}" : null)}

                    success = {GenerateMatchFragmentPart(generator, fragmentMatcher.Parts[partIndex])};
                if (!success)
                {{
                    if (stringMatchData != null)
                    {{
                        state.CurrentIndex = stringMatchData.StartIndex;
                        state.DistinctIndex = distinctIndex;
                    }}
                    goto Break;
                }}
                else
                {{
                    matchCount++;
                }}");
                }

                string code = $@"private bool {methodName}(ref State state, FragmentMatchData matchData)
                {{
                    bool success = true;
                    bool partSuccess;
                    int matchCount = 0;
                    StringMatchData stringMatchData = null;
                    int distinctIndex = state.DistinctIndex;
                    {(fragmentMatcher.PartsPadding != null ? string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "_", "false", "false") : null)};

                    {functionText}

                    Break:
                    {(fragmentMatcher.PartsPadding != null ?
                        $@"if (success)
                    {{
                        {string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "_", "false", "false")};
                    }}" : null)}
            
                    success = success && {(fragmentMatcher.MinMatchedParts ?? fragmentMatcher.Parts.Count)} <= matchCount;
                    if ({(!fragmentMatcher.Negate ? "!" : null)}success)
                    {{
						state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
                    }}
                    return success;
                }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GenerateMatchFragmentPartsOneMode(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher)
        {
            string methodName = $"MatchFragmentPartsOneMode{GetSafeMethodName(fragmentMatcher.Name)}";
            string method = $"{methodName}(ref state, partMatcherData)";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, FragmentMatchData matchData)
                {{
                    bool success = false;
                    int matchCount = 0;
                    {(fragmentMatcher.PartsPadding != null ? $"{string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "_", "false", "false")};" : null)}

                    {(fragmentMatcher.Parts.Count > 0 ? $@"
                    success = {string.Join(" || ", fragmentMatcher.Parts.Select(part => GenerateMatchFragmentPart(generator, part)))};
                    if (success)
                    {{
                        matchCount++;
                    }}" : null)}

                    {(fragmentMatcher.PartsPadding != null ?
                        $@"if (success)
                    {{
                        {string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "_", "false", "false")};
                    }}" : null)}
            
                    success = {((fragmentMatcher.MinMatchedParts ?? 1) <= 0).ToString().ToLower()} && matchCount > 0;
                    if ({(!fragmentMatcher.Negate ? "!" : null)}success)
                    {{
						state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
                    }}
                    return success;
                }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GenerateMatchFragmentPartsMultipleMode(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher)
        {
            string methodName = $"MatchFragmentPartsMultipleMode{GetSafeMethodName(fragmentMatcher.Name)}";
            string method = $"{methodName}(ref state, partMatcherData)";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                StringBuilder functionText = new StringBuilder();
                for (int partIndex = 0; partIndex < fragmentMatcher.Parts.Count; partIndex++)
                {
                    functionText.AppendLine($@"individualSuccess = {GenerateMatchFragmentPart(generator, fragmentMatcher.Parts[partIndex])};
                    subSuccess |= individualSuccess;
                    if (individualSuccess)
                    {{
                        matchCount++;
                        distinctIndex = state.DistinctIndex;
                        delimiterSuccess = {(fragmentMatcher.PartsDelimiter != null ? string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsDelimiter), "range", fragmentMatcher.PartsDelimiterRequired.ToString().ToLower(), "false") : "true")};
                        goto Break;
                    }}
                    ");
                }

                string code = $@"private bool {methodName}(ref State state, FragmentMatchData matchData)
                {{
                    bool overallSuccess = false;
                    bool subSuccess = false;
                    bool delimiterSuccess = false;
                    StringMatchData range = default;
                    int matchCount = 0;
                    int distinctIndex = state.DistinctIndex;
                    {(fragmentMatcher.PartsPadding != null ? string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "_", "false", "false") : null)};

                    do
                    {{
                        subSuccess = false;
                        bool individualSuccess;
                        {functionText.ToString()}

                        Break:
                        overallSuccess |= subSuccess;
                    }}
                    while (subSuccess && delimiterSuccess);
                    if (delimiterSuccess && range != null)
                    {{
                        state.CurrentIndex = range.StartIndex;
                        state.DistinctIndex = distinctIndex;
                    }}
                    {(fragmentMatcher.PartsPadding != null ?
                        $@"if (overallSuccess)
                    {{
                        {string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "_", "false", "false")};
                    }}" : null)}
            
                    bool thresholdSuccess = {fragmentMatcher.MinMatchedParts ?? 1} <= matchCount;
                    bool success = overallSuccess && thresholdSuccess;
                    if ({(!fragmentMatcher.Negate ? "!" : null)}success)
                    {{
						state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
                    }}
                    return success;
                }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GenerateMatchFragmentPart(MatcherEngineGenerator generator, IMatcher part)
        {
            return part is FragmentMatcher fragmentMatcher
                ? Generate(generator, fragmentMatcher)
                : GenerateMatchPartByTextMatcher(generator, (PatternMatcher)part);
        }

        private static string GenerateMatchFragmentBounds(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher, PatternMatcher matcher)
        {
            string methodName = $"MatchFragmentBounds{GetSafeMethodName(matcher.Name)}";
            string method = $"{methodName}(ref state, {{0}}, out {{1}})";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, bool readOnly, out StringMatchData matchData)
        {{
            bool success = {string.Format(GenerateMatchPattern(generator, matcher), "matchData", "true", "readOnly")};
            if ({(!fragmentMatcher.Negate ? "!" : null)}success)
            {{
				state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
            }}
			{(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{new String('\t', state.Id + 1)}} {{state.CurrentIndex}}. {{(success ? ""Passed"" : ""Failed"")}} Bounds: {{""{HttpUtility.JavaScriptStringEncode(matcher.ToString())}""}}"");" : null)}
            return success;
        }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GenerateMatchPartByTextMatcher(MatcherEngineGenerator generator, PatternMatcher matcher)
        {
            string methodName = $"MatchPartByTextMatcher{GetSafeMethodName(matcher.Name)}";
            string method = $"{methodName}(ref state, matchData)";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, FragmentMatchData matchData)
        {{
            
            {(generator.LanguageMatcher.LogMatches ? $@"int currentId = ++state.Id;
            state.MatchLogBuilder.AppendLine($""{{new String('\t', currentId)}} {{state.CurrentIndex}}. Try: {HttpUtility.JavaScriptStringEncode(matcher.Name)}"");" : null)}
			StringMatchData partMatchData;
            bool success = {string.Format(GenerateMatchPattern(generator, matcher), "partMatchData", "true", "false")};
            if (success)
            {{
                matchData.Parts.Add(partMatchData);
                {(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{new String('\t', state.Id + 1)}} {{state.CurrentIndex}}. Matched: {{partMatchData.Text}}"");" : null)}
            }}

            {(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{new String('\t', currentId)}} {{state.CurrentIndex}}. {{(success ? ""Passed"" : ""Failed"")}}: {HttpUtility.JavaScriptStringEncode(matcher.Name)}"");
            state.Id = currentId - 1;" : null)}
            return success;
        }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GenerateMatchByFragmentMatcherSection(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher)
        {
            return $@"partMatcherData = new FragmentMatchData
			{{
				Name = ""{GetEscapedName(fragmentMatcher.Name)}"",
				StartIndex = state.CurrentIndex{(fragmentMatcher.ExpressionOrder != null ? $@",
				ExpressionOrder = {fragmentMatcher.ExpressionOrder}" : null)}
			}};

			{(fragmentMatcher.Start != null ? $"StringMatchData startMatchData;" : null)}
			{(fragmentMatcher.End != null ? $"StringMatchData endMatchData;" : null)}
			success = ({(fragmentMatcher.Start != null ? $"{string.Format(GenerateMatchFragmentBounds(generator, fragmentMatcher, fragmentMatcher.Start), fragmentMatcher.DiscardBounds.ToString().ToLower(), "startMatchData")} && " : null)}{GenerateMatchFragmentParts(generator, fragmentMatcher)}{(fragmentMatcher.End != null ? $" && {string.Format(GenerateMatchFragmentBounds(generator, fragmentMatcher, fragmentMatcher.End), fragmentMatcher.DiscardBounds.ToString().ToLower(), "endMatchData")}" : null)});
				
			{(fragmentMatcher.Actions != null && fragmentMatcher.Actions.Count > 0 ? $@"
			if (success)
			{{
				success = {string.Join(" && ", fragmentMatcher.Actions.Select(action => GenerateMatcherAction(generator, action)))};
			}}
			" : null)}
				
			if (success)
			{{
				{(!fragmentMatcher.Negate ? $@"partMatcherData.Length = state.CurrentIndex - partMatcherData.StartIndex;
				partMatcherData.EndDistinctIndex = state.DistinctIndex;" : null)}
				{(fragmentMatcher.Cacheable ? $@"state.MatchCache[new ValueTuple<string, int>(""{GetEscapedName(fragmentMatcher.Name)}"", startIndex)] = partMatcherData;" : null)}
				{(!fragmentMatcher.IsNoise && fragmentMatcher.ExpressionMode != ExpressionMode.None && !fragmentMatcher.Negate ? $"ConvertToExpressionTree(partMatcherData, ExpressionMode.{fragmentMatcher.ExpressionMode});" : null)}
				{(fragmentMatcher.BoundsAsParts && fragmentMatcher.Start != null && !fragmentMatcher.Negate ? $@"if (startMatchData != null)
				{{
					partMatcherData.Parts.Insert(0, startMatchData);
				}}" : null)}
				{(fragmentMatcher.BoundsAsParts && fragmentMatcher.End != null && !fragmentMatcher.Negate ? $@"if (endMatchData != null)
				{{
					partMatcherData.Parts.Add(endMatchData);
				}}" : null)}
			}}";
        }
        
        private static string Generate(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher)
        {
            string methodName = $"MatchFragment{GetSafeMethodName(fragmentMatcher.Name)}";
            string method = $"{methodName}(ref state, matchData)";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, FragmentMatchData matchData)
        {{
            {(generator.LanguageMatcher.LogMatches ? $@"int currentId = ++state.Id;
            state.MatchLogBuilder.AppendLine($""{{new String('\t', currentId)}} {{state.CurrentIndex}}. Try: {GetEscapedName(fragmentMatcher.Name)}"");" : null)}
			bool success = false;
			FragmentMatchData partMatcherData = null;
			{(fragmentMatcher.Cacheable ? $@"if (!state.MatchCache.TryGetValue(new ValueTuple<string, int>(""{GetEscapedName(fragmentMatcher.Name)}"", state.CurrentIndex), out partMatcherData))
			{{" : null)}
			int startIndex = state.CurrentIndex;
			int distinctIndex = state.DistinctIndex;
			{GenerateMatchByFragmentMatcherSection(generator, fragmentMatcher)}
			else
			{{
				{(fragmentMatcher.Cacheable ? $@"state.MatchCache[new ValueTuple<string, int>(""{GetEscapedName(fragmentMatcher.Name)}"", startIndex)] = null;" : null)}
				{(!fragmentMatcher.Negate ? $@"state.CurrentIndex = startIndex;
				state.DistinctIndex = distinctIndex;" : null)}
			}}
			{(fragmentMatcher.Negate ? $@"state.CurrentIndex = startIndex;
			state.DistinctIndex = distinctIndex;" : null)}
			{(fragmentMatcher.Cacheable ? $@"}}" : null)}
			{(fragmentMatcher.Cacheable && !fragmentMatcher.Negate ? $@"else if (success = partMatcherData != null)
			{{
				state.CurrentIndex = startIndex + partMatcherData.Length;
                state.MaxIndex = Math.Max(state.MaxIndex, state.CurrentIndex);
				state.DistinctIndex = partMatcherData.EndDistinctIndex;
			}}" : null)}
            {(!fragmentMatcher.Negate ? $@"if (success)
            {{
				{(!fragmentMatcher.IsNoise && fragmentMatcher.FallThroughMode == FallThroughMode.All ? "matchData.Parts.AddRange(partMatcherData.Parts);" : null)}
				{(!fragmentMatcher.IsNoise && fragmentMatcher.FallThroughMode == FallThroughMode.None ? "matchData.Parts.Add(partMatcherData);" : null)}
				{(!fragmentMatcher.IsNoise && fragmentMatcher.FallThroughMode != FallThroughMode.None && fragmentMatcher.FallThroughMode != FallThroughMode.All ? $@"if (partMatcherData.Parts.Count <= {(int)fragmentMatcher.FallThroughMode})
				{{ 
					matchData.Parts.AddRange(partMatcherData.Parts);
				}}
				else
				{{
					matchData.Parts.Add(partMatcherData);
				}}" : null)}
				{(fragmentMatcher.ClearCache ? "state.MatchCache.Clear();" : null)}
			}}" : null)}
            {(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{new String('\t', currentId)}} {{state.CurrentIndex}}. {{({(fragmentMatcher.Negate ? "!" : null)}success ? ""Passed"" : ""Failed"")}}: {GetEscapedName(fragmentMatcher.Name)}"");
            state.Id = currentId - 1;" : null)}
            return {(fragmentMatcher.Negate ? "!" : null)}success;
        }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GenerateMatchFragmentParts(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher)
        {
            switch (fragmentMatcher.PartsMatchMode)
            {
                case MatchMode.Ordered:
                    return GenerateMatchFragmentPartsOrderedMode(generator, fragmentMatcher);
                case MatchMode.One:
                    return GenerateMatchFragmentPartsOneMode(generator, fragmentMatcher);
                case MatchMode.Multiple:
                default:
                    return GenerateMatchFragmentPartsMultipleMode(generator, fragmentMatcher);
            }
        }

        private static string GetEscapedName(string name)
        {
            return HttpUtility.JavaScriptStringEncode(name);
        }

        private static string GetSafeMethodName(string name)
        {
            return Regex.Replace(name, @"\W", (match) => $"x{(int)match.Value[0]}");
        }

        #endregion
    }
}
