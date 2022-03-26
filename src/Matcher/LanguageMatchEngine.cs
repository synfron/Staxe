using Synfron.Staxe.Matcher.Data;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Input.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synfron.Staxe.Matcher
{
	public abstract class LanguageMatchEngine : AbstractLanguageMatchEngine
	{
		private readonly bool _logMatches;
		protected bool _hasCheckFlags;

		private sealed class EagerIndexLanguageMatchEngine : LanguageMatchEngine
		{
			public EagerIndexLanguageMatchEngine(LanguageMatcher languageMatcher) : base(languageMatcher)
			{
				_hasCheckFlags = false;
			}

			protected override void BuildState(ref State state, string code)
			{
				state.Code = code;
				state.DistinctStringMatches = new List<StringMatchData>(2000);
				state.MatchLogBuilder = new StringBuilder();
				state.MatchCache = new Dictionary<ValueTuple<string, int>, FragmentMatchData>();

				PreMatchPatterns(ref state);
			}

			private bool PreMatchPatterns(ref State state)
			{
				int codeLength = state.Code.Length;
				List<PatternMatcher> patterns = LanguageMatcher.Patterns;
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
						StringMatchData matchData = PreMatchPattern(ref state, pattern);
						success = matchData != null;
						if (success)
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

			private StringMatchData PreMatchPattern(ref State state, PatternMatcher pattern)
			{
				int startOffset = state.CurrentIndex;
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
					state.CurrentIndex += stringMatchData.Length;
				}
				return stringMatchData;
			}

			protected override (bool success, StringMatchData matchData) MatchPattern(ref State state, PatternMatcher pattern, bool required, bool readOnly = false)
			{
				if (pattern != null)
				{
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
							state.CurrentIndex += stringMatchData.Length;
						}
						else if (!required)
						{
							success = true;
						}
						return (success, stringMatchData);
					}
					else
					{
						return (false, default);
					}

				}
				return (true, default);
			}
		}
		private sealed class NoIndexLanguageMatchEngine : LanguageMatchEngine
		{
			public NoIndexLanguageMatchEngine(LanguageMatcher languageMatcher) : base(languageMatcher)
			{
				_hasCheckFlags = false;
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
						success = stringMatchData != null;
						if (!readOnly)
						{
							state.CurrentIndex += stringMatchData.Length;
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
				_hasCheckFlags = true;
			}

			protected override void BuildState(ref State state, string code)
			{
				state.Code = code;
				state.DistinctStringMatches = new List<StringMatchData>(2000);
				state.MatchLogBuilder = new StringBuilder();
				state.MatchCache = new Dictionary<ValueTuple<string, int>, FragmentMatchData>();
			}

			protected override (bool success, StringMatchData matchData) MatchPattern(ref State state, PatternMatcher pattern, bool required, bool readOnly = false)
			{
				if (pattern != null)
				{
					bool success = false;
					int distinctIndex = state.DistinctIndex;
					if (distinctIndex >= state.MaxDistinctIndex)
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
							state.DistinctStringMatches.Add(stringMatchData);
							success = stringMatchData != null;
							state.CheckFlags[pattern.Id] = distinctIndex + 1;
							state.MaxDistinctIndex++;
							if (!readOnly)
							{
								state.DistinctIndex++;
								state.CurrentIndex += stringMatchData.Length;
							}
						}
						else if (!required)
						{
							success = true;
						}
						return (success, stringMatchData);
					}
					else
					{
						StringMatchData stringMatchData = state.DistinctStringMatches[distinctIndex];
						if (stringMatchData != null)
						{
							success = stringMatchData.Id == pattern.Id;
						}
						else
						{
							if (state.CheckFlags[pattern.Id] < distinctIndex + 1)
							{
								int length;
								int startOffset = state.CurrentIndex;
								(success, length) = pattern.IsMatch(state.Code, state.CurrentIndex);
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
									state.DistinctStringMatches[distinctIndex] = stringMatchData;
								}
								state.CheckFlags[pattern.Id] = distinctIndex + 1;
							}
						}
						if (success && !readOnly)
						{
							state.DistinctIndex++;
							state.CurrentIndex += stringMatchData.Length;
						}
						else if (!required)
						{
							success = true;
						}
						return (success, stringMatchData);
					}

				}
				return (true, default);
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

			Span<int> checkFlags = _hasCheckFlags ? stackalloc int[LanguageMatcher.Patterns.Count + 1] : default;
			State state = new State()
			{
				CheckFlags = checkFlags
			};
			BuildState(ref state, code);

			bool success = MatchPartByFragmentMatcher(ref state, matchData, startingMatcher ?? LanguageMatcher.StartingFragment);

			IMatchData resultMatchData = matchData?.Parts.FirstOrDefault();
			int? failureIndex = success ? null : state.FailureIndex;

			if (success && matchFullText && state.CurrentIndex != state.Code.Length)
			{
				success = false;
				failureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
			}

			return new MatcherResult(resultMatchData, success, state.CurrentIndex, failureIndex, _logMatches ? state.MatchLogBuilder.ToString() : string.Empty);
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

		private (bool, FragmentMatchData) MatchByFragmentMatcher(ref State state, FragmentMatcher matcher)
		{
			FragmentMatchData matchData = new FragmentMatchData
			{
				Name = matcher.Name,
				StartIndex = state.CurrentIndex,
				ExpressionOrder = matcher.ExpressionOrder
			};
			StringMatchData endMatchData = default;
			bool success = MatchFragmentBounds(ref state, matcher.Start, matcher, matcher.DiscardBounds, out StringMatchData startMatchData) && MatchFragmentParts(ref state, matcher, matchData) && MatchFragmentBounds(ref state, matcher.End, matcher, matcher.DiscardBounds, out endMatchData);

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
			return overallSuccess || thresholdSuccess;
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

		protected abstract (bool success, StringMatchData matchData) MatchPattern(ref State state, PatternMatcher pattern, bool required, bool readOnly = false);
	}
}
