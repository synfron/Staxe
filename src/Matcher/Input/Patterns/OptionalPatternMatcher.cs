using System;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class OptionalPatternMatcher : PatternMatcher
	{
		private readonly PatternMatcher _subPattern;
		private static int _lastId;
		private readonly int _id;

		public OptionalPatternMatcher(PatternMatcher subPattern)
		{
			if (subPattern is null)
			{
				throw new ArgumentException("Invalid pattern");
			}
			_subPattern = subPattern;
			_id = _lastId++;
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
		{
			return (true, _subPattern.IsMatch(text, startOffset).offset);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchOptional{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
            return (true, {string.Format(_subPattern.Generate(generator), "text", "startOffset")}.offset);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return $"{_subPattern}?";
		}
	}
}
