namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class DigitPatternMatcher : PatternMatcher
	{
		public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
		{
			return startOffset < text.Length && char.IsDigit(text[startOffset]) ? (true, 1) : (false, 0);
		}
		internal override string Generate(MatcherEngineGenerator generator)
		{
			return "MatchDigit({0}, {1})";
		}

		public override string ToString()
		{
			return "\\d";
		}
	}
}
