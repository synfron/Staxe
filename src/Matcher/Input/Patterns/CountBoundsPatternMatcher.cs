using System;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class CountBoundsPatternMatcher : PatternMatcher
	{
		private readonly PatternMatcher _subPattern;
		private readonly int _lowerBounds;
		private readonly int _upperBounds;
		private static int _lastId;
		private readonly int _id;

		public CountBoundsPatternMatcher(PatternMatcher subPattern, int lowerBounds, int upperBounds)
		{
			if (subPattern is null)
			{
				throw new ArgumentException("Invalid pattern");
			}
			_subPattern = subPattern;
			_lowerBounds = lowerBounds;
			_upperBounds = upperBounds;
			_id = _lastId++;
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
		{
			int upperBounds = _upperBounds;
			int offset = startOffset;
			bool subSuccess = true;
			int subOffset;
			PatternMatcher subPattern = _subPattern;
			int matches = 0;
			for (; matches < upperBounds && subSuccess; matches++)
			{
				(subSuccess, subOffset) = subPattern.IsMatch(text, offset);
				offset += subOffset;
			}
			bool success = subSuccess || matches >= _lowerBounds;
			return success ? (true, offset - startOffset) : (false, 0);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchCountBounds{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
			int offset = startOffset;
			bool subSuccess = true;
			int subOffset;
			int matches = 0;
			for (; matches < {_upperBounds} && subSuccess; matches++)
			{{
				(subSuccess, subOffset) = {string.Format(_subPattern.Generate(generator), "text", "offset")};
				offset += subOffset;
            }}
			bool success = subSuccess || matches >= {_lowerBounds};
            return success ? (true, offset - startOffset) : (false, 0);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return $"{_subPattern}{{{_lowerBounds},{_upperBounds}}}";
		}
	}
}
