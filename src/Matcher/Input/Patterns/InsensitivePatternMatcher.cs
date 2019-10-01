using System;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class InsensitivePatternMatcher : PatternMatcher
	{
		private readonly PatternMatcher _subPattern;

		public InsensitivePatternMatcher(PatternMatcher subPattern)
		{
			if (subPattern is null)
			{
				throw new ArgumentException("Invalid pattern");
			}
			_subPattern = subPattern;
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset = 0) => _subPattern.IsMatch(text, startOffset);

		internal override string Generate(MatcherEngineGenerator generator) => _subPattern.Generate(generator);

		public override string ToString()
		{
			return $"~{(_subPattern is GroupPatternMatcher groupPattern ? groupPattern.ToString(true) : _subPattern.ToString())}";
		}
	}
}
