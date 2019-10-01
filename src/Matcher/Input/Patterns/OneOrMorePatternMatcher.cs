using System;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class OneOrMorePatternMatcher : PatternMatcher
	{
		private readonly PatternMatcher _subPattern;
		private static int _lastId;
		private readonly int _id;

		public OneOrMorePatternMatcher(PatternMatcher subPattern)
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
			bool subSuccess = true;
			bool success = false;
			int subOffset;
			PatternMatcher subPattern = _subPattern;
			if (subPattern != null)
			{
				while (subSuccess)
				{
					(subSuccess, subOffset) = subPattern.IsMatch(text, offset);
					offset += subOffset;
					success |= subSuccess;
				}
			}
			return success ? (true, offset - startOffset) : (false, 0);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchOneOrMore{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
            int offset = startOffset;
            bool subSuccess = true;
            bool success = false;
            int subOffset;
            while (subSuccess)
            {{
                (subSuccess, subOffset) = {string.Format(_subPattern.Generate(generator), "text", "offset")};
                offset += subOffset;
                success |= subSuccess;
            }}
            return success ? (true, offset - startOffset) : (false, 0);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return $"{_subPattern}+";
		}
	}
}
