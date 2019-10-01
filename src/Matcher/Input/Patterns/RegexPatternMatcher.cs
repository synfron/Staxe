using System.Text.RegularExpressions;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class RegexPatternMatcher : PatternMatcher
	{
		private readonly Regex regex;

		public RegexPatternMatcher(string pattern)
		{
			regex = new Regex(pattern, RegexOptions.Compiled);
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset)
		{
			Match match = regex.Match(text, startOffset);
			return (match.Success, match.Length);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			return $@"(() => {{
                Regex regex = new Regex({regex.ToString()});
                Match match = regex.Match({{0}}, {{1}});
                return (match?.Value, match?.Length ?? 0);
            }})()";
		}

		public override string ToString()
		{
			return regex.ToString();
		}
	}
}
