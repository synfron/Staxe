using Synfron.Staxe.Matcher.Input.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Synfron.Staxe.Matcher.Input
{
	public class FragmentMatcher : IMatcher, IEquatable<FragmentMatcher>
	{
		private readonly int _hashCode;

		public FragmentMatcher(
			int id,
			string name,
			PatternMatcher start = default,
			PatternMatcher end = default,
			IList<IMatcher> parts = default,
			PatternMatcher partsDelimiter = default,
			bool partsDelimiterRequired = true,
			PatternMatcher partsPadding = default,
			int? minMatchedParts = default,
			MatchMode partsMatchMode = default,
			bool isNoise = default,
			FallThroughMode fallThroughMode = FallThroughMode.None,
			bool cacheable = default,
			bool clearCache = default,
			ExpressionMode expressionMode = default,
			int? expressionOrder = default,
			bool boundsAsParts = default,
			bool discardBounds = default,
			bool negate = default
		)
		{
			Id = id;
			Name = name;
			Start = start;
			End = end;
			Parts = parts ?? new List<IMatcher>();
			PartsDelimiter = partsDelimiter;
			PartsDelimiterRequired = partsDelimiterRequired;
			PartsPadding = partsPadding;
			MinMatchedParts = minMatchedParts;
			PartsMatchMode = partsMatchMode;
			IsNoise = isNoise;
			FallThroughMode = fallThroughMode;
			Cacheable = cacheable;
			ClearCache = clearCache;
			ExpressionMode = expressionMode;
			ExpressionOrder = expressionOrder;
			BoundsAsParts = boundsAsParts;
			DiscardBounds = discardBounds;
			Negate = negate;

			_hashCode = name.GetHashCode();
		}

		public int Id { get; set; }

		public string Name { get; set; }

		public PatternMatcher Start { get; set; }

		public PatternMatcher End { get; set; }

		public IList<IMatcher> Parts { get; set; }

		public PatternMatcher PartsDelimiter { get; set; }

		public bool PartsDelimiterRequired { get; set; }

		public PatternMatcher PartsPadding { get; set; }

		public int? MinMatchedParts { get; set; }

		public MatchMode PartsMatchMode { get; set; }

		public bool IsNoise { get; set; }

		public FallThroughMode FallThroughMode { get; set; } = FallThroughMode.None;

		public bool Cacheable { get; set; }

		public bool ClearCache { get; set; }

		public ExpressionMode ExpressionMode { get; set; }

		public int? ExpressionOrder { get; set; }

		public bool BoundsAsParts { get; set; }

		public bool DiscardBounds { get; set; }

		public bool Negate { get; }

		public bool Equals(FragmentMatcher other)
		{
			return other.Name == Name;
		}

		public override bool Equals(object obj)
		{
			return (obj is FragmentMatcher other) && Equals(other);
		}

		private string GenerateMatchFragmentPartsOrderedMode(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchFragmentPartsOrderedMode{GetSafeMethodName(Name)}";
			string method = $"{methodName}(ref state, partMatcherData)";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				StringBuilder functionText = new StringBuilder();
				for (int partIndex = 0; partIndex < Parts.Count; partIndex++)
				{
					functionText.AppendLine($@"{(partIndex > 0 ? $@"distinctIndex = state.DistinctIndex;
                    partSuccess = {(PartsDelimiter != null ? string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, PartsDelimiter), "stringMatchData", PartsDelimiterRequired.ToString().ToLower(), "false") : "true")};
                    success = partSuccess;
                    if (!success)
                    {{
                        goto Break;
                    }}" : null)}

                    success = {GenerateMatchFragmentPart(generator, Parts[partIndex])};
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
                    {(PartsPadding != null ? string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, PartsPadding), "_", "false", "false") : null)};

                    {functionText.ToString()}

                    Break:
                    {(PartsPadding != null ?
						$@"if (success)
                    {{
                        {string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, PartsPadding), "_", "false", "false")};
                    }}" : null)}
            
                    success = success || {(MinMatchedParts ?? Parts.Count)} <= matchCount;
                    if ({(!Negate ? "!" : null)}success)
                    {{
						state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
                    }}
                    return success;
                }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		private string GenerateMatchFragmentPartsOneMode(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchFragmentPartsOneMode{GetSafeMethodName(Name)}";
			string method = $"{methodName}(ref state, partMatcherData)";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private bool {methodName}(ref State state, FragmentMatchData matchData)
                {{
                    bool success = false;
                    int matchCount = 0;
                    {(PartsPadding != null ? $"{string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, PartsPadding), "_", "false", "false")};" : null)}

                    {(Parts.Count > 0 ? $@"
                    success = {string.Join(" || ", Parts.Select(part => GenerateMatchFragmentPart(generator, part)))};
                    if (success)
                    {{
                        matchCount++;
                    }}" : null)}

                    {(PartsPadding != null ?
						$@"if (success)
                    {{
                        {string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, PartsPadding), "_", "false", "false")};
                    }}" : null)}
            
                    success = {((MinMatchedParts ?? 1) <= 0).ToString().ToLower()} || matchCount > 0;
                    if ({(!Negate ? "!" : null)}success)
                    {{
						state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
                    }}
                    return success;
                }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		private string GenerateMatchFragmentPartsMultipleMode(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchFragmentPartsMultipleMode{GetSafeMethodName(Name)}";
			string method = $"{methodName}(ref state, partMatcherData)";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				StringBuilder functionText = new StringBuilder();
				for (int partIndex = 0; partIndex < Parts.Count; partIndex++)
				{
					functionText.AppendLine($@"individualSuccess = {GenerateMatchFragmentPart(generator, Parts[partIndex])};
                    subSuccess |= individualSuccess;
                    if (individualSuccess)
                    {{
                        matchCount++;
                        distinctIndex = state.DistinctIndex;
                        delimiterSuccess = {(PartsDelimiter != null ? string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, PartsDelimiter), "range", PartsDelimiterRequired.ToString().ToLower(), "false") : "true")};
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
                    {(PartsPadding != null ? string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, PartsPadding), "_", "false", "false") : null)};

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
                    {(PartsPadding != null ?
						$@"if (overallSuccess)
                    {{
                        {string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, PartsPadding), "_", "false", "false")};
                    }}" : null)}
            
                    bool thresholdSuccess = {MinMatchedParts ?? 1} <= matchCount;
                    bool success = overallSuccess || thresholdSuccess;
                    if ({(!Negate ? "!" : null)}success)
                    {{
						state.FailureIndex = Math.Max(state.FailureIndex ?? 0, state.CurrentIndex);
                    }}
                    return success;
                }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		private string GenerateMatchFragmentPart(MatcherEngineGenerator generator, IMatcher part)
		{
			return part is FragmentMatcher fragmentMatcher
				? fragmentMatcher.Generate(generator)
				: GenerateMatchPartByTextMatcher(generator, (PatternMatcher)part);
		}

		private string GenerateMatchFragmentBounds(MatcherEngineGenerator generator, PatternMatcher matcher)
		{
			string methodName = $"MatchFragmentBounds{GetSafeMethodName(matcher.Name)}";
			string method = $"{methodName}(ref state, {{0}}, out {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private bool {methodName}(ref State state, bool readOnly, out StringMatchData matchData)
        {{
            bool success = {string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, matcher), "matchData", "true", "readOnly")};
            if ({(!Negate ? "!" : null)}success)
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



		private string GenerateMatchPartByTextMatcher(MatcherEngineGenerator generator, PatternMatcher matcher)
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
            bool success = {string.Format(generator.LanguageMatcher.GenerateMatchPattern(generator, matcher), "partMatchData", "true", "false")};
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



		public string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchFragment{GetSafeMethodName(Name)}";
			string method = $"{methodName}(ref state, matchData)";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private bool {methodName}(ref State state, FragmentMatchData matchData)
        {{
            {(generator.LanguageMatcher.LogMatches ? $@"int currentId = ++state.Id;
            state.MatchLogBuilder.AppendLine($""{{new String('\t', currentId)}} {{state.CurrentIndex}}. Try: {GetEscapedName()}"");" : null)}
			bool success = false;
			FragmentMatchData partMatcherData = null;
			{(Cacheable ? $@"if (!state.MatchCache.TryGetValue(new ValueTuple<string, int>(""{GetEscapedName()}"", state.CurrentIndex), out partMatcherData))
			{{" : null)}
				int startIndex = state.CurrentIndex;
				int distinctIndex = state.DistinctIndex;
				partMatcherData = new FragmentMatchData
				{{
					Name = ""{GetEscapedName()}"",
					StartIndex = state.CurrentIndex{(ExpressionOrder != null ? $@",
					ExpressionOrder = {ExpressionOrder}" : null)}
				}};

				{(Start != null ? $"StringMatchData startMatchData;" : null)}
				{(End != null ? $"StringMatchData endMatchData;" : null)}
				success = ({(Start != null ? $"{string.Format(GenerateMatchFragmentBounds(generator, Start), DiscardBounds.ToString().ToLower(), "startMatchData")} && " : null)}{GenerateMatchFragmentParts(generator)}{(End != null ? $" && {string.Format(GenerateMatchFragmentBounds(generator, End), DiscardBounds.ToString().ToLower(), "endMatchData")}" : null)});
				
				
				if (success)
				{{
					{(!Negate ? $@"partMatcherData.Length = state.CurrentIndex - partMatcherData.StartIndex;
					partMatcherData.EndDistinctIndex = state.DistinctIndex;" : null)}
					{(Cacheable ? $@"state.MatchCache[new ValueTuple<string, int>(""{GetEscapedName()}"", startIndex)] = partMatcherData;" : null)}
					{(!IsNoise && ExpressionMode != ExpressionMode.None && !Negate ? $"ConvertToExpressionTree(partMatcherData, ExpressionMode.{ExpressionMode.ToString()});" : null)}
					{(BoundsAsParts && Start != null && !Negate ? $@"if (startMatchData != null)
					{{
						partMatcherData.Parts.Insert(0, startMatchData);
					}}" : null)}
					{(BoundsAsParts && End != null && !Negate ? $@"if (endMatchData != null)
					{{
						partMatcherData.Parts.Add(endMatchData);
					}}" : null)}
				}}
				else
				{{
					{(Cacheable ? $@"state.MatchCache[new ValueTuple<string, int>(""{GetEscapedName()}"", startIndex)] = null;" : null)}
					{(!Negate ? $@"state.CurrentIndex = startIndex;
					state.DistinctIndex = distinctIndex;" : null)}
				}}
				{(Negate ? $@"state.CurrentIndex = startIndex;
				state.DistinctIndex = distinctIndex;" : null)}
			{(Cacheable ? $@"}}" : null)}
			{(Cacheable && !Negate ? $@"else if (success = partMatcherData != null)
			{{
				state.CurrentIndex = partMatcherData.StartIndex + partMatcherData.Length;
				state.DistinctIndex = partMatcherData.EndDistinctIndex;
			}}" : null)}
            {(!Negate ? $@"if (success)
            {{
				{(!IsNoise && FallThroughMode == FallThroughMode.All ? "matchData.Parts.AddRange(partMatcherData.Parts);" : null)}
				{(!IsNoise && FallThroughMode == FallThroughMode.None ? "matchData.Parts.Add(partMatcherData);" : null)}
				{(!IsNoise && FallThroughMode != FallThroughMode.None && FallThroughMode != FallThroughMode.All ? $@"if (partMatcherData.Parts.Count <= {(int)FallThroughMode})
				{{ 
					matchData.Parts.AddRange(partMatcherData.Parts);
				}}
				else
				{{
					matchData.Parts.Add(partMatcherData);
				}}" : null)}
				{(ClearCache ? "state.MatchCache.Clear();" : null)}
			}}" : null)}
            {(generator.LanguageMatcher.LogMatches ? $@"state.MatchLogBuilder.AppendLine($""{{new String('\t', currentId)}} {{state.CurrentIndex}}. {{({(Negate ? "!" : null)}success ? ""Passed"" : ""Failed"")}}: {GetEscapedName()}"");
            state.Id = currentId - 1;" : null)}
            return {(Negate ? "!" : null)}success;
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		private string GenerateMatchFragmentParts(MatcherEngineGenerator generator)
		{
			switch (PartsMatchMode)
			{
				case MatchMode.Ordered:
					return GenerateMatchFragmentPartsOrderedMode(generator);
				case MatchMode.One:
					return GenerateMatchFragmentPartsOneMode(generator);
				case MatchMode.Multiple:
				default:
					return GenerateMatchFragmentPartsMultipleMode(generator);
			}
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}

		public override string ToString()
		{
			return Name;
		}

		private string GetEscapedName()
		{
			return HttpUtility.JavaScriptStringEncode(Name);
		}

		private static string GetSafeMethodName(string name)
		{
			return Regex.Replace(name, @"\W", (match) => $"x{(int)match.Value[0]:X}");
		}
	}
}
