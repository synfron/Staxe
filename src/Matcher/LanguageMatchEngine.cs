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

                int currentIndex;
                while ((currentIndex = state.CurrentIndex) < codeLength)
                {
                    success = false;
                    for (int patternIndex = 0; patternIndex < patternsCount; patternIndex++)
                    {
                        PatternMatcher pattern = patterns[patternIndex];
                        StringMatchData matchData; 
                        (success, matchData) = PreMatchPattern(ref state, pattern);
                        if (matchData != null && _logMatches)
                        {
                            state.MatchLogBuilder.AppendLine($"{currentIndex}. Prematched {matchData.Name}: {matchData.Text}");
                        }
                        if (success)
                        {
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
                            Id = pattern.Id
                        };
                        if (!pattern.IsNoise)
                        {
                            state.DistinctStringMatches.Add(stringMatchData);
                            state.DistinctIndex++;
                            state.MaxDistinctIndex++;
                        }
                        state.CurrentIndex = startOffset + length;
                    }
                    return (success, stringMatchData);
                }
                else if (pattern is FragmentPatternMatcher fragmentPatternMatcher && fragmentPatternMatcher.Fragment is FragmentMatcher fragmentMatcher)
                {
                    int startIndex = state.CurrentIndex;
                    StringMatchData lastDistinctMatchData = state.DistinctStringMatches.LastOrDefault();
                    int cacheIndex = lastDistinctMatchData != null ?
                        lastDistinctMatchData.StartIndex + lastDistinctMatchData.Length :
                        0;
                    int distinctIndex = state.DistinctIndex;
                    int maxDistinctIndex = state.MaxDistinctIndex;

                    (bool fragmentSuccess, FragmentMatchData partMatchData) = MatchByFragmentMatcher(ref state, fragmentPatternMatcher.Fragment);
                    if (fragmentSuccess && fragmentMatcher.Cacheable)
                    {
                        state.MatchCache[new ValueTuple<string, int>(fragmentMatcher.Name, cacheIndex)] = partMatchData;
                    }
                    if (!fragmentSuccess || fragmentPatternMatcher.Fragment.Negate || pattern.IsNoise)
                    {
                        state.DistinctStringMatches.RemoveRange(distinctIndex, state.DistinctIndex - distinctIndex);
                        state.DistinctIndex = distinctIndex;
                        state.MaxDistinctIndex = maxDistinctIndex;
                        if (!fragmentSuccess || fragmentPatternMatcher.Fragment.Negate)
                        {
                            state.CurrentIndex = startIndex;
                        }
                        if (pattern.IsNoise && partMatchData != null)
                        {
                            partMatchData.EndDistinctIndex = distinctIndex;
                        }
                    }
                    return (fragmentSuccess, null);
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

            protected override (bool success, StringMatchData matchData) MatchPattern(ref State state, FragmentMatchData fragmentMatchData, PatternMatcher pattern, bool required, bool readOnly = false)
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
                            Id = pattern.Id
                        };
                        if (!readOnly)
                        {
                            state.CurrentIndex = startOffset + length;
                            if (fragmentMatchData.StartIndex < 0)
                            {
                                fragmentMatchData.StartIndex = startOffset;
                            }
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
                state.CurrentIndex < (state.PreMatchSuccess ? 
                    state.DistinctStringMatches.LastOrDefault().GetEndIndex() : state.Code.Length
                ))
            {
                success = false;
                failureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
            }

            return new MatcherResult(resultMatchData, success, state.CurrentIndex, failureIndex, _logMatches ? state.MatchLogBuilder.ToString() : string.Empty);
        }

        protected virtual (bool success, StringMatchData matchData) MatchPattern(ref State state, FragmentMatchData fragmentMatchData, PatternMatcher pattern, bool required, bool readOnly = false)
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
                    if (fragmentMatchData.StartIndex < 0)
                    {
                        fragmentMatchData.StartIndex = stringMatchData.StartIndex;
                    }
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
                        Id = pattern.Id
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
                        if (fragmentMatchData.StartIndex < 0)
                        {
                            fragmentMatchData.StartIndex = startOffset;
                        }
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

        private bool MatchPartByFragmentMatcher(ref State state, FragmentMatchData parentMatchData, FragmentMatcher part)
        {
            bool success;
            bool negate = part.Negate;
            if (_logMatches)
            {
                state.MatchLogBuilder.AppendLine($"{new string('\t', state.Id)} {state.CurrentIndex}. Try: {part}");
            }
            if (!part.Cacheable || !state.MatchCache.TryGetValue(new ValueTuple<string, int>(part.Name, state.CurrentIndex), out FragmentMatchData partMatchData))
            {
                int startIndex = state.CurrentIndex;
                int distinctIndex = state.DistinctIndex;
                (success, partMatchData) = MatchByFragmentMatcher(ref state, part);
                if (!success || negate)
                {
                    state.CurrentIndex = startIndex;
                    state.DistinctIndex = distinctIndex;
                }
                if (part.Cacheable)
                {
                    state.MatchCache[new ValueTuple<string, int>(part.Name, startIndex)] = partMatchData;
                }
            }
            else if ((success = partMatchData != null) && !negate)
            {
                state.CurrentIndex = partMatchData.StartIndex + partMatchData.Length;
                state.DistinctIndex = partMatchData.EndDistinctIndex;
            }
            if (success && !negate)
            {
                if (parentMatchData.StartIndex < 0)
                {
                    parentMatchData.StartIndex = partMatchData.StartIndex;
                }
                if (!part.IsNoise)
                {
                    if (part.FallThroughMode == FallThroughMode.All || partMatchData.Parts.Count <= (int)part.FallThroughMode)
                    {
                        parentMatchData.Parts.AddRange(partMatchData.Parts);
                    }
                    else
                    {
                        parentMatchData.Parts.Add(partMatchData);
                    }
                }
                if (part.ClearCache)
                {
                    state.MatchCache.Clear();
                }
            }
            if (_logMatches)
            {
                state.MatchLogBuilder.AppendLine($"{new string('\t', state.Id)} {state.CurrentIndex}. {(success ? "Passed" : "Failed")}: {part}");
            }
            return success ^ negate;
        }

        protected (bool, FragmentMatchData) MatchByFragmentMatcher(ref State state, FragmentMatcher matcher)
        {
            FragmentMatchData matchData = new FragmentMatchData
            {
                Name = matcher.Name,
                StartIndex = -1,
                ExpressionOrder = matcher.ExpressionOrder
            };
            StringMatchData endMatchData = default;
            bool success = MatchFragmentBounds(ref state, matcher, matchData, matcher.Start, matcher.DiscardBounds, out StringMatchData startMatchData) && MatchFragmentParts(ref state, matcher, matchData) && MatchFragmentBounds(ref state, matcher, matchData, matcher.End, matcher.DiscardBounds, out endMatchData);

            if (success && matcher.Actions != null)
            {
                foreach (MatcherAction action in matcher.Actions)
                {
                    success = action.Perform(state.BlobDatas, matchData.Parts);
                    if (!success)
                    {
                        break;
                    }
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

        private bool MatchFragmentParts(ref State state, FragmentMatcher parentMatcher, FragmentMatchData parentMatchData)
        {
            bool success = true;
            if (parentMatcher.Parts.Count > 0)
            {
                switch (parentMatcher.PartsMatchMode)
                {
                    case MatchMode.Multiple:
                        success = MatchFragmentPartsMultipleMode(ref state, parentMatcher, parentMatchData);
                        break;
                    case MatchMode.One:
                        success = MatchFragmentPartsOneMode(ref state, parentMatcher, parentMatchData);
                        break;
                    case MatchMode.Ordered:
                        success = MatchFragmentPartsOrderedMode(ref state, parentMatcher, parentMatchData);
                        break;
                }
            }
            if (!success ^ parentMatcher.Negate)
            {
                state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
            }
            return success;
        }

        private bool MatchFragmentPartsMultipleMode(ref State state, FragmentMatcher parentMatcher, FragmentMatchData parentMatchData)
        {
            bool overallSuccess = false;
            bool subSuccess;
            bool delimiterSuccess = false;
            StringMatchData range = default;
            int matchCount = 0;
            int distinctIndex = state.DistinctIndex;
            MatchPattern(ref state, parentMatchData, parentMatcher.PartsPadding, false);
            do
            {
                subSuccess = false;
                foreach (IMatcher part in parentMatcher.Parts)
                {
                    bool individualSuccess = MatchFragmentPart(ref state, parentMatchData, part);
                    subSuccess |= individualSuccess;
                    if (individualSuccess)
                    {
                        matchCount++;
                        distinctIndex = state.DistinctIndex;
                        (delimiterSuccess, range) = MatchPattern(ref state, parentMatchData, parentMatcher.PartsDelimiter, parentMatcher.PartsDelimiterRequired);
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
                MatchPattern(ref state, parentMatchData, parentMatcher.PartsPadding, false);
            }
            return (parentMatcher.MinMatchedParts ?? 1) <= matchCount;
        }

        private bool MatchFragmentPartsOneMode(ref State state, FragmentMatcher parentMatcher, FragmentMatchData parentMatchData)
        {
            bool success = false;
            int matchCount = 0;
            MatchPattern(ref state, parentMatchData, parentMatcher.PartsPadding, false);
            foreach (IMatcher part in parentMatcher.Parts)
            {
                success = MatchFragmentPart(ref state, parentMatchData, part);
                if (success)
                {
                    matchCount++;
                    break;
                }
            }
            if (success)
            {
                MatchPattern(ref state, parentMatchData, parentMatcher.PartsPadding, false);
            }
            return (parentMatcher.MinMatchedParts ?? 1) <= 0 || matchCount > 0;
        }

        private bool MatchFragmentPartsOrderedMode(ref State state, FragmentMatcher parentMatcher, FragmentMatchData parentMatchData)
        {
            bool success = true;
            bool partSuccess;
            int matchCount = 0;
            StringMatchData stringMatchData = default;
            int distinctIndex = state.DistinctIndex;
            MatchPattern(ref state, parentMatchData, parentMatcher.PartsPadding, false);
            for (int partIndex = 0; partIndex < parentMatcher.Parts.Count; partIndex++)
            {
                if (partIndex > 0)
                {
                    distinctIndex = state.DistinctIndex;
                    (partSuccess, stringMatchData) = MatchPattern(ref state, parentMatchData, parentMatcher.PartsDelimiter, parentMatcher.PartsDelimiterRequired);
                    success = partSuccess;
                    if (!success)
                    {
                        break;
                    }
                }

                IMatcher part = parentMatcher.Parts[partIndex];
                success = MatchFragmentPart(ref state, parentMatchData, part);
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
                MatchPattern(ref state, parentMatchData, parentMatcher.PartsPadding, false);
            }
            return (parentMatcher.MinMatchedParts ?? parentMatcher.Parts.Count) <= matchCount;
        }

        private bool MatchFragmentPart(ref State state, FragmentMatchData parentMatchData, IMatcher part)
        {
            state.Id++;
            bool success = false;
            switch (part)
            {
                case FragmentMatcher partFragmentMatcher:
                    success = MatchPartByFragmentMatcher(ref state, parentMatchData, partFragmentMatcher);
                    break;
                case PatternMatcher partPatternMatcher:
                    success = MatchPartByTextMatcher(ref state, parentMatchData, partPatternMatcher);
                    break;
            }
            state.Id--;
            return success;
        }

        private bool MatchPartByTextMatcher(ref State state, FragmentMatchData parentMatchData, PatternMatcher part)
        {
            int startIndex = state.CurrentIndex;
            if (_logMatches)
            {
                state.MatchLogBuilder.AppendLine($"{new string('\t', state.Id)} {startIndex}. Try: {part}");
            }

            (bool success, StringMatchData stringMatchData) = MatchPattern(ref state, parentMatchData, part, true);
            if (success)
            {
                parentMatchData.Parts.Add(stringMatchData);
                if (_logMatches)
                {
                    state.MatchLogBuilder.AppendLine($"{new string('\t', state.Id + 1)} {(stringMatchData?.StartIndex ?? startIndex)}. Matched: {stringMatchData.Text}");
                }
            }

            if (_logMatches)
            {
                state.MatchLogBuilder.AppendLine($"{new string('\t', state.Id)} {(success && stringMatchData != null ? stringMatchData.StartIndex + stringMatchData.Length : startIndex)}. {(success ? "Passed" : "Failed")}: {part}");
            }
            return success;
        }

        private bool MatchFragmentBounds(ref State state, FragmentMatcher parentMatcher, FragmentMatchData parentMatchData, PatternMatcher patternMatcher, bool readOnly, out StringMatchData matchData)
        {
            if (patternMatcher == null)
            {
                matchData = default;
                return true;
            }
            bool success;
            int startIndex = state.CurrentIndex;
            (success, matchData) = MatchPattern(ref state, parentMatchData, patternMatcher, true, readOnly);
            if (!success ^ parentMatcher.Negate)
            {
                state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
            }
            if (patternMatcher != null && _logMatches)
            {
                state.MatchLogBuilder.AppendLine($"{new string('\t', state.Id + 1)} {(success && matchData != null ? matchData.StartIndex + matchData.Length : startIndex)}. {(success ? "Passed" : "Failed")} Bounds: {patternMatcher}");
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
using Synfron.Staxe.Matcher.Input;
using System;
using System.Collections.Generic;
using System.Linq;
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
            FragmentMatchData parentMatchData = new FragmentMatchData
            {{
                StartIndex = 0
            }};

            State state = new State()
            {{
                Code = code{(languageMatcher.IndexingMode != IndexingMode.None ? @",
				DistinctStringMatches = new List<StringMatchData>(2000)" : null)}{(languageMatcher.Fragments.Any(fragment => fragment.Cacheable) ? @",
				MatchCache = new Dictionary<ValueTuple<string, int>, FragmentMatchData>()" : null)}{(languageMatcher.LogMatches ? $@",
				MatchLogBuilder = new StringBuilder()" : null)}{(languageMatcher.Blobs.Count > 0 ? $@",
				BlobDatas = new Span<BlobData>(new BlobData[{languageMatcher.Blobs.Count}])" : null)}
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

			IMatchData resultMatchData = parentMatchData.Parts.FirstOrDefault();
			int? failureIndex = success ? null : state.FailureIndex;

			if (success && matchFullText && state.CurrentIndex < {(languageMatcher.IndexingMode == IndexingMode.Eager ? $@"(state.PreMatchSuccess ? 
                    state.DistinctStringMatches.LastOrDefault().GetEndIndex() : " : null)}state.Code.Length
                {(languageMatcher.IndexingMode == IndexingMode.Eager ? ")" : null)})
			{{
				success = false;
				failureIndex = state.CurrentIndex;
			}}

			return new MatcherResult(resultMatchData, success, state.CurrentIndex, failureIndex, state.MatchLogBuilder?.ToString());
        }}

        public override MatcherResult Match(string code, bool matchFullText = true)
        {{
            FragmentMatchData parentMatchData = new FragmentMatchData
            {{
                StartIndex = 0
            }};
            
            State state = new State()
            {{
                Code = code{(languageMatcher.IndexingMode != IndexingMode.None ? @",
				DistinctStringMatches = new List<StringMatchData>(2000)" : null)}{(languageMatcher.Fragments.Any(fragment => fragment.Cacheable) ? @",
				MatchCache = new Dictionary<ValueTuple<string, int>, FragmentMatchData>()" : null)}{(languageMatcher.LogMatches ? $@",
				MatchLogBuilder = new StringBuilder()" : null)}{(languageMatcher.Blobs.Count > 0 ? $@",
				BlobDatas = new Span<BlobData>(new BlobData[{languageMatcher.Blobs.Count}])" : null)}
            }};
            
			{(languageMatcher.IndexingMode == IndexingMode.Eager ? "state.PreMatchSuccess = PreMatchPatterns(ref state);" : null)}

            bool success = {Generate(generator, languageMatcher.StartingFragment)};

			IMatchData resultMatchData = parentMatchData?.Parts.FirstOrDefault();
			int? failureIndex = success ? null : state.FailureIndex;

			if (success && matchFullText && state.CurrentIndex < {(languageMatcher.IndexingMode == IndexingMode.Eager ? $@"(state.PreMatchSuccess ? 
                    state.DistinctStringMatches.LastOrDefault().GetEndIndex() : " : null )}state.Code.Length
                {(languageMatcher.IndexingMode == IndexingMode.Eager ? ")" : null)})
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
			StringMatchData matchData = null;
			int currentIndex = 0;
			while (success && (currentIndex = state.CurrentIndex) < codeLength)
			{{
				success = {string.Join(" ||\n", languageMatcher.Patterns.Select(pattern => $@"{string.Format(GeneratePreMatchPattern(generator, pattern), "null", "matchData", "true", "false")}"))};
				{(generator.LanguageMatcher.LogMatches ? $@"if (matchData != null) {{
				    state.MatchLogBuilder.AppendLine($""{{currentIndex}}. Prematched {{matchData.Name}}: {{matchData.Text}}"");
				}}" : null)}
			}}
			state.CurrentIndex = 0;
			{(languageMatcher.IndexingMode != IndexingMode.None ? "state.DistinctIndex = 0;" : null)}
			return success;
		}}" : null)}";
        }


        private static string GenerateMatchPattern(MatcherEngineGenerator generator, PatternMatcher pattern)
        {
            string methodName = $"MatchPattern{GetSafeMethodName(pattern.Name)}";
            string method = $"{methodName}(ref state, {{0}}, out {{1}}, {{2}}, {{3}})";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, FragmentMatchData fragmentMatchData, out StringMatchData matchData, bool required, bool readOnly = false)
            {{
                bool success = false;
                int startOffset = state.CurrentIndex;
                {(generator.IndexingMode != IndexingMode.None ? $@"int distinctIndex = state.DistinctIndex;
                if (distinctIndex >= state.MaxDistinctIndex)
                {{" : null)}
                    int length;
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
							Id = {pattern.Id}
						}};
						{(!pattern.IsNoise && generator.IndexingMode != IndexingMode.None ? $@"state.DistinctStringMatches.Add(matchData);" : null)}
						{(!pattern.IsNoise && generator.IndexingMode != IndexingMode.None ? $@"state.MaxDistinctIndex++;" : null)}
						{(!pattern.IsNoise && generator.IndexingMode != IndexingMode.None ? $@"state.DistinctIndex++;" : null)}
						if (!readOnly)
						{{
							state.CurrentIndex = startOffset + length;
                            if ((fragmentMatchData?.StartIndex ?? 0) < 0)
                            {{
                                fragmentMatchData.StartIndex = startOffset;
                            }}
						}}
					}}
					else if (!required)
					{{
						success = true;
					}}
					return success;
                {(generator.IndexingMode != IndexingMode.None ? $@"}}
                else
                {{
                    int length;
					matchData = state.DistinctStringMatches[distinctIndex];
                    if (matchData != null)
                    {{
						success = matchData.Id == {pattern.Id};
                    }}
                    else
                    {{
                        (success, length) = {GenerateRawMatchPattern(generator, pattern)};
                        if (success)
                        {{
							matchData = new StringMatchData
							{{
								Name = ""{GetSafeMethodName(pattern.Name)}"",
								Text = state.Code.Substring(startOffset, length),
								StartIndex = startOffset,
								Length = length,
								Id = {pattern.Id}
							}};
							{(!pattern.IsNoise ? $@"state.DistinctStringMatches[distinctIndex] = matchData;" : null)}
                        }}
                    }}
					if (success && !readOnly)
					{{
						{(!pattern.IsNoise ? $@"state.DistinctIndex++;" : null)}
						state.CurrentIndex = matchData.StartIndex + matchData.Length;
                        if ((fragmentMatchData?.StartIndex ?? 0) < 0)
                        {{
                            fragmentMatchData.StartIndex = matchData.StartIndex;
                        }}
					}}
					if (!required)
					{{
						success = true;
					}}
					return success;
                }}" : null)}
            }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GeneratePreMatchPattern(MatcherEngineGenerator generator, PatternMatcher pattern)
        {
            if (pattern is FragmentPatternMatcher)
            {
                string methodName = $"MatchPattern{GetSafeMethodName(pattern.Name)}";
                string method = $"{methodName}(ref state, {{0}}, out {{1}}, {{2}}, {{3}})";
                if (!generator.TryGetMethod(methodName, ref method))
                {
                    generator.Add(methodName, method);
                    string code = $@"private bool {methodName}(ref State state, FragmentMatchData fragmentMatchData, out StringMatchData matchData, bool required, bool readOnly = false)
            {{
                {(!pattern.IsStandard && pattern is FragmentPatternMatcher fragmentPatternMatcher && fragmentPatternMatcher.Fragment is FragmentMatcher fragmentMatcher ? $@"
                bool success = false;
                int startOffset = state.CurrentIndex;
                int distinctIndex = state.DistinctIndex;
                int maxDistinctIndex = state.MaxDistinctIndex;
                {(fragmentMatcher.Cacheable ? $@"
                StringMatchData lastDistinctMatchData = state.DistinctStringMatches.LastOrDefault();
                int cacheIndex = lastDistinctMatchData != null ?
                    lastDistinctMatchData.StartIndex + lastDistinctMatchData.Length :
                    0;
                " : null)}
                FragmentMatchData partMatchData;
				{GenerateMatchByFragmentMatcherSection(generator, fragmentMatcher, $@"{(fragmentMatcher.Cacheable ? $@"
                state.MatchCache[new ValueTuple<string, int>(""{GetEscapedName(fragmentMatcher.Name)}"", cacheIndex)] = partMatchData;" : null)}
                {(pattern.IsNoise ? "partMatchData.EndDistinctIndex = distinctIndex;" : null)}", null)}
                {(!fragmentMatcher.Negate && !pattern.IsNoise ? $@"if (!success)
                {{" : null)}
                    state.DistinctStringMatches.RemoveRange(distinctIndex, state.DistinctIndex - distinctIndex);
                    state.DistinctIndex = distinctIndex;
                    state.MaxDistinctIndex = maxDistinctIndex;
                    {(!fragmentMatcher.Negate && pattern.IsNoise ? $@"if (!success)
                    {{
                        state.CurrentIndex = startOffset;
                    }}" : "state.CurrentIndex = startOffset;")}
                {(!fragmentMatcher.Negate && !pattern.IsNoise ? $"}}" : null)}
                matchData = null;
                return success;" : null)}
            }}";
                    method = generator.Add(method, methodName, code);
                }
                return method;
            } 
            else
            {
                return GenerateMatchPattern(generator, pattern);
            }
        }

        private static string GenerateRawMatchPattern(MatcherEngineGenerator generator, PatternMatcher pattern)
        {
            return string.Format(pattern.Generate(generator), "state.Code", "state.CurrentIndex");
        }

        private static string GenerateMatcherAction(MatcherEngineGenerator generator, MatcherAction action)
        {
            return string.Format(action.Generate(generator), "state.BlobDatas", "partMatchData.Parts");
        }

        private static string GenerateMatchFragmentPartsOrderedMode(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher)
        {
            string methodName = $"MatchFragmentPartsOrderedMode{GetSafeMethodName(fragmentMatcher.Name)}";
            string method = $"{methodName}(ref state, partMatchData)";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                StringBuilder functionText = new StringBuilder();
                for (int partIndex = 0; partIndex < fragmentMatcher.Parts.Count; partIndex++)
                {
                    functionText.AppendLine($@"{(partIndex > 0 ? $@"distinctIndex = state.DistinctIndex;
                    partSuccess = {(fragmentMatcher.PartsDelimiter != null ? string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsDelimiter), "parentMatchData", "stringMatchData", fragmentMatcher.PartsDelimiterRequired.ToString().ToLower(), "false") : "true")};
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

                string code = $@"private bool {methodName}(ref State state, FragmentMatchData parentMatchData)
                {{
                    bool success = true;
                    bool partSuccess;
                    int matchCount = 0;
                    StringMatchData stringMatchData = null;
                    int distinctIndex = state.DistinctIndex;
                    {(fragmentMatcher.PartsPadding != null ? string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "parentMatchData", "_", "false", "false") : null)};

                    {functionText}

                    Break:
                    {(fragmentMatcher.PartsPadding != null ?
                        $@"if (success)
                    {{
                        {string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "parentMatchData", "_", "false", "false")};
                    }}" : null)}
            
                    success = {(fragmentMatcher.MinMatchedParts ?? fragmentMatcher.Parts.Count)} <= matchCount;
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
            string method = $"{methodName}(ref state, partMatchData)";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, FragmentMatchData parentMatchData)
                {{
                    bool success = false;
                    int matchCount = 0;
                    {(fragmentMatcher.PartsPadding != null ? $"{string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "parentMatchData", "_", "false", "false")};" : null)}

                    {(fragmentMatcher.Parts.Count > 0 ? $@"
                    success = {string.Join(" || ", fragmentMatcher.Parts.Select(part => GenerateMatchFragmentPart(generator, part)))};
                    if (success)
                    {{
                        matchCount++;
                    }}" : null)}

                    {(fragmentMatcher.PartsPadding != null ?
                        $@"if (success)
                    {{
                        {string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "parentMatchData", "_", "false", "false")};
                    }}" : null)}
            
                    success = {((fragmentMatcher.MinMatchedParts ?? 1) <= 0 ? "true" : "matchCount > 0")};
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
            string method = $"{methodName}(ref state, partMatchData)";
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
                        delimiterSuccess = {(fragmentMatcher.PartsDelimiter != null ? string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsDelimiter), "parentMatchData", "range", fragmentMatcher.PartsDelimiterRequired.ToString().ToLower(), "false") : "true")};
                        goto Break;
                    }}
                    ");
                }

                string code = $@"private bool {methodName}(ref State state, FragmentMatchData parentMatchData)
                {{
                    bool overallSuccess = false;
                    bool subSuccess = false;
                    bool delimiterSuccess = false;
                    StringMatchData range = default;
                    int matchCount = 0;
                    int distinctIndex = state.DistinctIndex;
                    {(fragmentMatcher.PartsPadding != null ? string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "parentMatchData", "_", "false", "false") : null)};

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
                        {string.Format(GenerateMatchPattern(generator, fragmentMatcher.PartsPadding), "parentMatchData", "_", "false", "false")};
                    }}" : null)}
            
                    bool success = {fragmentMatcher.MinMatchedParts ?? 1} <= matchCount;
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
            string method = $"{methodName}(ref state, {{0}}, {{1}}, out {{2}})";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, FragmentMatchData parentMatchData, bool readOnly, out StringMatchData matchData)
        {{{(generator.LanguageMatcher.LogMatches ? @"
            int startIndex = state.CurrentIndex;" : "")}
            bool success = {string.Format(GenerateMatchPattern(generator, matcher), "parentMatchData", "matchData", "true", "readOnly")};
            if ({(!fragmentMatcher.Negate ? "!" : null)}success)
            {{
				state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
            }}
			{(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{new String('\t', state.Id + 1)}} {{(success && matchData != null ? matchData.StartIndex + matchData.Length : startIndex)}}. {{(success ? ""Passed"" : ""Failed"")}} Bounds: {{""{HttpUtility.JavaScriptStringEncode(matcher.ToString())}""}}"");" : null)}
            return success;
        }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GenerateMatchPartByTextMatcher(MatcherEngineGenerator generator, PatternMatcher matcher)
        {
            string methodName = $"MatchPartByTextMatcher{GetSafeMethodName(matcher.Name)}";
            string method = $"{methodName}(ref state, parentMatchData)";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, FragmentMatchData parentMatchData)
        {{
            
            {(generator.LanguageMatcher.LogMatches ? $@"state.Id++;
            int startIndex = state.CurrentIndex;
            state.MatchLogBuilder.AppendLine($""{{new String('\t', state.Id)}} {{startIndex}}. Try: {HttpUtility.JavaScriptStringEncode(matcher.ToString())}"");" : null)}
			StringMatchData partMatchData;
            bool success = {string.Format(GenerateMatchPattern(generator, matcher), "parentMatchData", "partMatchData", "true", "false")};
            if (success)
            {{
                parentMatchData.Parts.Add(partMatchData);
                {(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{new String('\t', state.Id + 1)}} {{partMatchData?.StartIndex ?? startIndex}}. Matched: {{partMatchData.Text}}"");" : null)}
            }}

            {(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{new String('\t', state.Id)}} {{(success && partMatchData != null ? partMatchData.StartIndex + partMatchData.Length : startIndex)}}. {{(success ? ""Passed"" : ""Failed"")}}: {HttpUtility.JavaScriptStringEncode(matcher.ToString())}"");
            state.Id--;" : null)}
            return success;
        }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }

        private static string GenerateMatchByFragmentMatcherSection(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher, string onSuccess, string onFail)
        {
            return $@"partMatchData = new FragmentMatchData
			{{
				Name = ""{GetEscapedName(fragmentMatcher.Name)}"",
				StartIndex = -1{(fragmentMatcher.ExpressionOrder != null ? $@",
				ExpressionOrder = {fragmentMatcher.ExpressionOrder}" : null)}
			}};

			{(fragmentMatcher.Start != null ? $"StringMatchData startMatchData;" : null)}
			{(fragmentMatcher.End != null ? $"StringMatchData endMatchData = null;" : null)}
			success = ({(fragmentMatcher.Start != null ? $"{string.Format(GenerateMatchFragmentBounds(generator, fragmentMatcher, fragmentMatcher.Start), "partMatchData", fragmentMatcher.DiscardBounds.ToString().ToLower(), "startMatchData")} && " : null)}{GenerateMatchFragmentParts(generator, fragmentMatcher)}{(fragmentMatcher.End != null ? $" && {string.Format(GenerateMatchFragmentBounds(generator, fragmentMatcher, fragmentMatcher.End), "partMatchData", fragmentMatcher.DiscardBounds.ToString().ToLower(), "endMatchData")}" : null)});
				
			{(fragmentMatcher.Actions != null && fragmentMatcher.Actions.Count > 0 ? $@"
			if (success)
			{{
				success = {string.Join(" && ", fragmentMatcher.Actions.Select(action => GenerateMatcherAction(generator, action)))};
			}}
			" : null)}
			if (success)
			{{
				{(!fragmentMatcher.Negate ? $@"partMatchData.Length = state.CurrentIndex - partMatchData.StartIndex;
				partMatchData.EndDistinctIndex = state.DistinctIndex;" : null)}
				{(!fragmentMatcher.IsNoise && fragmentMatcher.ExpressionMode != ExpressionMode.None && !fragmentMatcher.Negate ? $"ConvertToExpressionTree(partMatchData, ExpressionMode.{fragmentMatcher.ExpressionMode});" : null)}
				{(fragmentMatcher.BoundsAsParts && fragmentMatcher.Start != null && !fragmentMatcher.Negate ? $@"if (startMatchData != null)
				{{
					partMatchData.Parts.Insert(0, startMatchData);
				}}" : null)}
				{(fragmentMatcher.BoundsAsParts && fragmentMatcher.End != null && !fragmentMatcher.Negate ? $@"if (endMatchData != null)
				{{
					partMatchData.Parts.Add(endMatchData);
				}}" : null)}
                {(!string.IsNullOrWhiteSpace(onSuccess) ? onSuccess : null)}
			}}{(!string.IsNullOrWhiteSpace(onFail) ? $@"
            else {{
                {onFail}
            }}
            " : null)}";
        }
        
        private static string Generate(MatcherEngineGenerator generator, FragmentMatcher fragmentMatcher)
        {
            string methodName = $"MatchFragment{GetSafeMethodName(fragmentMatcher.Name)}";
            string method = $"{methodName}(ref state, parentMatchData)";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(ref State state, FragmentMatchData parentMatchData)
        {{
			bool success = false;
			int startIndex = state.CurrentIndex;
			FragmentMatchData partMatchData = null;
            {(generator.LanguageMatcher.LogMatches ? $@"state.Id++;
            state.MatchLogBuilder.AppendLine($""{{new String('\t', state.Id)}} {{startIndex}}. Try: {GetEscapedName(fragmentMatcher.ToString())}"");" : null)}
			{(fragmentMatcher.Cacheable ? $@"if (!state.MatchCache.TryGetValue(new ValueTuple<string, int>(""{GetEscapedName(fragmentMatcher.Name)}"", state.CurrentIndex), out partMatchData))
			{{" : null)}
			int distinctIndex = state.DistinctIndex;
			{GenerateMatchByFragmentMatcherSection(generator, fragmentMatcher, $@"{(fragmentMatcher.Cacheable ? $@"state.MatchCache[new ValueTuple<string, int>(""{GetEscapedName(fragmentMatcher.Name)}"", startIndex)] = partMatchData;" : null)}", $@"{(fragmentMatcher.Cacheable ? $@"state.MatchCache[new ValueTuple<string, int>(""{GetEscapedName(fragmentMatcher.Name)}"", startIndex)] = null;" : null)}
				{(!fragmentMatcher.Negate ? $@"state.CurrentIndex = startIndex;
				state.DistinctIndex = distinctIndex;" : null)}")}

			{(fragmentMatcher.Negate ? $@"state.CurrentIndex = startIndex;
			state.DistinctIndex = distinctIndex;" : null)}
			{(fragmentMatcher.Cacheable ? $@"}}" : null)}
			{(fragmentMatcher.Cacheable && !fragmentMatcher.Negate ? $@"else if (success = partMatchData != null)
			{{
				state.CurrentIndex = partMatchData.StartIndex + partMatchData.Length;
				state.DistinctIndex = partMatchData.EndDistinctIndex;
			}}" : null)}
            {(!fragmentMatcher.Negate ? $@"if (success)
            {{
                if (parentMatchData.StartIndex < 0)
                {{
                    parentMatchData.StartIndex = partMatchData.StartIndex;
                }}
				{(!fragmentMatcher.IsNoise && fragmentMatcher.FallThroughMode == FallThroughMode.All ? "parentMatchData.Parts.AddRange(partMatchData.Parts);" : null)}
				{(!fragmentMatcher.IsNoise && fragmentMatcher.FallThroughMode == FallThroughMode.None ? "parentMatchData.Parts.Add(partMatchData);" : null)}
				{(!fragmentMatcher.IsNoise && fragmentMatcher.FallThroughMode.IsCountBased() ? $@"if (partMatchData.Parts.Count <= {(int)fragmentMatcher.FallThroughMode})
				{{ 
					parentMatchData.Parts.AddRange(partMatchData.Parts);
				}}
				else
				{{
					parentMatchData.Parts.Add(partMatchData);
				}}" : null)}
				{(fragmentMatcher.ClearCache ? "state.MatchCache.Clear();" : null)}
			}}" : null)}
            {(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{new String('\t', state.Id)}} {{state.CurrentIndex}}. {{({(fragmentMatcher.Negate ? "!" : null)}success ? ""Passed"" : ""Failed"")}}: {GetEscapedName(fragmentMatcher.ToString())}"");
            state.Id--;" : null)}
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
