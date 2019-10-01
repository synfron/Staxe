using System;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class NotPatternMatcher : PatternMatcher
	{
		private static int _lastId;
		private readonly PatternMatcher _subPattern;
		private readonly int _id;

		public NotPatternMatcher(PatternMatcher subPattern)
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
			return text.Length > startOffset && !_subPattern.IsMatch(text, startOffset).success ? (true, 0) : (false, 0);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchNot{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
            return text.Length > startOffset && !{string.Format(_subPattern.Generate(generator), "text", "startOffset")}.success ? (true, 0) : (false, 0);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return $"{_subPattern}!";
		}
	}
}
