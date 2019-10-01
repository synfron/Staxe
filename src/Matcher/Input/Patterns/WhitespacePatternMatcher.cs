namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class WhitespacePatternMatcher : PatternMatcher
	{

		public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
		{
			return startOffset < text.Length && char.IsWhiteSpace(text[startOffset]) ? (true, 1) : (false, 0);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			return "MatchWhitespace({0}, {1})";
		}

		public override string ToString()
		{
			return "\\s";
		}
	}
}
