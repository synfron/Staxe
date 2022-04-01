using Synfron.Staxe.Matcher.Input.Patterns;
using Xunit;

namespace MatcherTests.Patterns
{
	public class PatternReaderTests
	{
		[Theory]
		[InlineData(@"\(\\\)\|\.\?\w\d\l\s\!", @"(\(\\\)\|\.\?\w\d\l\s\!)")]
		[InlineData("a|b", "(a|b)")]
		[InlineData(@"ab\defg", @"(ab\defg)")]
		[InlineData(@"ab\(efg", @"(ab\(efg)")]
		[InlineData("(abc)", "abc")]
		[InlineData("abc", "abc")]
		[InlineData("~abc", "~abc")]
		[InlineData("abc*ba", "(abc*ba)")]
		[InlineData("abc+ba", "(abc+ba)")]
		[InlineData("abc?ba", "(abc?ba)")]
		[InlineData("ab.ba", "(ab.ba)")]
		[InlineData("abc!ba", "(abc!ba)")]
		[InlineData("abc|ba|ac", "(abc|ba|ac)")]
		[InlineData("(abc|ba)*", "(abc|ba)*")]
		[InlineData("(abc)|(cba)", "(abc|cba)")]
		[InlineData(@"(('|\\)!.|\\.)+", @"((('|\\)!.)|(\\.))+")]
		public void PatternReader_Parse_CorrectStructure(string pattern, string expected)
		{
			PatternMatcher matcher = PatternReader.Parse(pattern);
			Assert.Equal(expected, matcher.ToString());
		}

		[Theory]
		[InlineData("\"(((\\\\|\")!.)|\\\\.)*\"", "\"\\\"\"")]
		[InlineData("\"(((\\\\|\")!.)|\\\\.)*\"", "\"hello world\"")]
		[InlineData("abc!dba", "abdba")]
		[InlineData("~abc", "AbC")]
		[InlineData("ab\\(efg", "ab(efg")]
		[InlineData("ab\\defg", "ab1efg")]
		[InlineData("a|b", "b")]
		[InlineData("(abc)", "abc")]
		[InlineData("abc", "abc")]
		[InlineData("abc*ba", "abba")]
		[InlineData("abc*ba", "abccba")]
		[InlineData("abc+ba", "abccba")]
		[InlineData("a(bcb)a", "abcba")]
		[InlineData("abc?ba", "abcba")]
		[InlineData("abc?ba", "abba")]
		[InlineData("ab.ba", "abjba")]
		[InlineData("abc|ac|ba", "ba")]
		[InlineData("(abc)|(cba)", "cba")]
		public void PatternReader_Parse_Matches(string pattern, string text)
		{
			PatternMatcher matcher = PatternReader.Parse(pattern);
			(bool success, int offset) = matcher.IsMatch(text, 0);
			Assert.True(success);
			Assert.Equal(text.Length, offset);
		}
	}
}
