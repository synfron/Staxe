namespace Synfron.Staxe.Matcher
{
	public interface ILanguageMatchEngine
	{
		MatcherResult Match(string code, bool matchFullText = true);
		MatcherResult Match(string code, string fragmentMatcher, bool matchFullText = true);
	}
}