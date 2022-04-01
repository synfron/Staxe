using Synfron.Staxe.Matcher;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Interop.Bnf;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace MatcherTests.Interop.Ebnf.Tests
{
	public class ConversionTests
	{

		[Theory]
		[InlineData("abc.bnf", 12)]
		[InlineData("abc-iso.ebnf", 10)]
		[InlineData("abc-w3c.ebnf", 10)]
		public void ConversionTest(string grammarFileName, int numRules)
		{
			const string fileDirectory = "Files";
			string ebnf = File.ReadAllText(Path.Combine(fileDirectory, "Grammars", grammarFileName));

			EbnfConverter ebnfConverter = new EbnfConverter();
			LanguageMatcher languageMatcher = ebnfConverter.Convert("ebnf", ebnf);

			Assert.Equal(numRules, languageMatcher.Fragments.Count(f => f.FallThroughMode != FallThroughMode.All));

			string parsable = File.ReadAllText(Path.Combine(fileDirectory, "Parsables", "abc.txt"));

			Stopwatch watch = new Stopwatch();
			watch.Start();
			LanguageMatchEngine languageMatchEngine = LanguageMatchEngine.Build(languageMatcher);

			MatcherResult result = languageMatchEngine.Match(parsable);
			watch.Stop();
			Assert.True(result.Success);
		}
	}
}
