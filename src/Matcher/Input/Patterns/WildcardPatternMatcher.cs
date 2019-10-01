using System;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class WildcardPatternMatcher : PatternMatcher
	{
		private readonly PatternMatcher _subPattern;
		private static ushort _lastId;
		private readonly ushort _id;

		public WildcardPatternMatcher(PatternMatcher subPattern)
		{
			if (subPattern is null)
			{
				throw new ArgumentException("Invalid pattern");
			}
			if (subPattern is NotPatternMatcher)
			{
				throw new ArgumentException("Infinite pattern");
			}
			_subPattern = subPattern;
			_id = _lastId++;
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
		{
			int offset = startOffset;
			bool success = true;
			int subOffset;
			PatternMatcher subPattern = _subPattern;
			if (subPattern != null)
			{
				while (success)
				{
					(success, subOffset) = subPattern.IsMatch(text, offset);
					offset += subOffset;
				}
			}
			return (true, offset - startOffset);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchWildcard{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
            int offset = startOffset;
            bool success = true;
            int subOffset;
            while (success)
            {{
                (success, subOffset) = {string.Format(_subPattern.Generate(generator), "text", "offset")};
                offset += subOffset;
            }}
            return (true, offset - startOffset);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return $"{_subPattern}*";
		}
	}
}
