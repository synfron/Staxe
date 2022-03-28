namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class LazyPatternMatcher : PatternMatcher
	{
		private readonly string _pattern;
		private PatternMatcher _patternMatcher;

		public LazyPatternMatcher(string pattern)
		{
			_pattern = pattern;
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			ResolveMatcher();
			return _patternMatcher.Generate(generator);
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset)
		{
			ResolveMatcher();
			return _patternMatcher.IsMatch(text, startOffset);
		}

		public override string ToString()
        {
            ResolveMatcher();
            return _patternMatcher is GroupPatternMatcher groupPatternMatcher ? groupPatternMatcher.ToString(true) : _patternMatcher?.ToString();
        }

        private void ResolveMatcher()
        {
            if (_patternMatcher == null)
            {
                _patternMatcher = PatternReader.Parse(_pattern);
            }
        }
    }
}
