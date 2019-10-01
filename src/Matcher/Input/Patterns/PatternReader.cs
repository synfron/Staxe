using Synfron.Staxe.Shared.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class PatternReader
	{
		private PatternReader()
		{
		}

		private bool Insensitive { get; set; }

		private int Offset { get; set; }

		public static PatternMatcher Parse(string pattern)
		{
			PatternReader reader = new PatternReader();
			return reader.AsMatcher(pattern);
		}

		public static PatternMatcher LazyParse(string pattern)
		{
			return new LazyPatternMatcher(pattern);
		}

		public static PatternMatcher RegexParse(string pattern)
		{
			return new RegexPatternMatcher($@"\G{pattern}");
		}

		public static PatternMatcher Parse(int id, string name, string pattern)
		{
			PatternReader reader = new PatternReader();
			PatternMatcher matcher = reader.AsMatcher(pattern);
			matcher.Id = id;
			matcher.Name = name;
			return matcher;
		}

		public static PatternMatcher LazyParse(int id, string name, string pattern)
		{
			return new LazyPatternMatcher(pattern) { Id = id, Name = name };
		}

		public static PatternMatcher RegexParse(int id, string name, string pattern)
		{
			return new RegexPatternMatcher($@"\G{pattern}") { Id = id, Name = name };
		}

		private PatternMatcher AsMatcher(string pattern)
		{
			if (string.IsNullOrEmpty(pattern))
			{
				return new LiteralPatternMatcher(string.Empty);
			}
			else if (pattern[Offset] == '~')
			{
				Offset++;
				Insensitive = true;
				return new InsensitivePatternMatcher(AsMatcher(pattern));
			}
			else if (pattern[Offset] == '`')
			{
				Offset++;
				return new WholeWordPatternMatcher(AsMatcher(pattern));
			}
			else
			{
				return AsGroupMatcher(pattern);
			}
		}

		private PatternMatcher AsLiteralMatcher(string pattern)
		{
			int offset = Offset;
			int i = offset;
			bool terminate = false;
			if (i < pattern.Length)
			{
				i++;
				while (i < pattern.Length && !terminate)
				{
					switch (pattern[i])
					{
						case '\\':
						case ')':
						case '.':
						case '|':
						case '(':
						case '{':
						case '}':
						case '[':
						case ']':
							terminate = true;
							break;
						case '*':
						case '+':
						case '?':
						case '!':
							if (i - offset > 1)
							{
								i--;
							}
							terminate = true;
							break;
						default:
							i++;
							break;
					}
				}
			}
			Offset = i;
			return Insensitive ? new InsensitiveLiteralPatternMatcher(pattern.Substring(offset, i - offset)) : (PatternMatcher)new LiteralPatternMatcher(pattern.Substring(offset, i - offset));
		}

		private PatternMatcher AsGroupMatcher(string pattern)
		{
			List<PatternMatcher> matchers = new List<PatternMatcher>();
			PatternMatcher lastMatcher = null;
			bool terminate = false;
			while (Offset < pattern.Length && !terminate)
			{
				switch (pattern[Offset++])
				{
					case '\\':
						lastMatcher = AsSpecialMatcher(pattern);
						matchers.Add(lastMatcher);
						break;
					case '.':
						lastMatcher = new AnyPatternMatcher();
						matchers.Add(lastMatcher);
						break;
					case '!':
						lastMatcher = new NotPatternMatcher(lastMatcher);
						matchers.SetLast(lastMatcher);
						break;
					case '(':
						lastMatcher = AsGroupMatcher(pattern);
						matchers.Add(lastMatcher);
						break;
					case '[':
						lastMatcher = AsCharBoundsMatcher(pattern);
						matchers.Add(lastMatcher);
						break;
					case '*':
						lastMatcher = new WildcardPatternMatcher(lastMatcher);
						matchers.SetLast(lastMatcher);
						break;
					case '+':
						lastMatcher = new OneOrMorePatternMatcher(lastMatcher);
						matchers.SetLast(lastMatcher);
						break;
					case '?':
						lastMatcher = new OptionalPatternMatcher(lastMatcher);
						matchers.SetLast(lastMatcher);
						break;
					case '|':
						lastMatcher = AsOrPatternMatcher(pattern, matchers);
						matchers = new List<PatternMatcher> { lastMatcher };
						break;
					case ')':
						terminate = true;
						break;
					case '{':
						lastMatcher = AsCountBoundsMatcher(pattern, lastMatcher);
						matchers.SetLast(lastMatcher);
						break;
					default:
						Offset--;
						lastMatcher = AsLiteralMatcher(pattern);
						matchers.Add(lastMatcher);
						break;
				}
			}
			return matchers.Count == 1 ? matchers[0] : new GroupPatternMatcher(matchers);
		}

		private PatternMatcher AsSpecialMatcher(string pattern)
		{
			switch (pattern[Offset++])
			{
				case 'w':
					return new WordCharPatternMatcher();
				case 's':
					return new WhitespacePatternMatcher();
				case 'd':
					return new DigitPatternMatcher();
				case 'l':
					return new LetterPatternMatcher();
				case 'r':
					return new LiteralPatternMatcher("\r");
				case 'n':
					return new LiteralPatternMatcher("\n");
				case 't':
					return new LiteralPatternMatcher("\t");
				default:
					Offset--;
					return AsLiteralMatcher(pattern);
			}
		}

		private PatternMatcher AsCountBoundsMatcher(string pattern, PatternMatcher lastMatcher)
		{
			int lowerBounds = 0;
			int upperBounds;
			string lowerBoundsStr = string.Empty;
			string upperBoundsStr;
			int startOffset = Offset;
			int delimiterIndex = Offset;
			while (Offset < pattern.Length)
			{
				switch (pattern[Offset++])
				{
					case ',':
						{
							lowerBoundsStr = pattern.Substring(startOffset, Offset - startOffset - 1).Trim();
							lowerBounds = string.IsNullOrEmpty(lowerBoundsStr) ? 0 : int.Parse(lowerBoundsStr);
							delimiterIndex = Offset;
							break;
						}
					case '}':
						upperBoundsStr = pattern.Substring(delimiterIndex, Offset - delimiterIndex - 1).Trim();
						if (string.IsNullOrEmpty(lowerBoundsStr) && string.IsNullOrEmpty(upperBoundsStr))
						{
							return lastMatcher;
						}
						upperBounds = string.IsNullOrEmpty(upperBoundsStr) ? int.MaxValue : int.Parse(upperBoundsStr);
						return new CountBoundsPatternMatcher(lastMatcher, lowerBounds, upperBounds);
				}
			}
			throw new ArgumentException("Invalid count bounds pattern");
		}

		private PatternMatcher AsCharBoundsMatcher(string pattern)
		{
			string lowerBoundsStr = string.Empty;
			string upperBoundsStr;
			int startOffset = Offset;
			int delimiterIndex = Offset;
			while (Offset < pattern.Length)
			{
				switch (pattern[Offset++])
				{

					case '-':
						{
							lowerBoundsStr = pattern.Substring(startOffset, Offset - startOffset - 1).Trim();
							delimiterIndex = Offset;
							break;
						}
					case ']':
						upperBoundsStr = pattern.Substring(delimiterIndex, Offset - delimiterIndex - 1).Trim();
						return new CharBoundsPatternMatcher(char.Parse(Unescape(lowerBoundsStr)), char.Parse(Unescape(upperBoundsStr)));
				}
			}
			throw new ArgumentException("Invalid char bounds pattern");
		}

		private OrPatternMatcher AsOrPatternMatcher(string pattern, List<PatternMatcher> firstParts)
		{
			List<PatternMatcher> matchers = new List<PatternMatcher> { firstParts.Count == 1 ? firstParts[0] : new GroupPatternMatcher(firstParts) };
			bool terminate = false;
			while (Offset < pattern.Length && !terminate)
			{
				switch (pattern[Offset])
				{
					case ')':
						terminate = true;
						break;
					default:
						matchers.Add(AsOrGroupPatternMatcher(pattern));
						break;
				}
			}
			return new OrPatternMatcher(matchers);
		}

		private PatternMatcher AsOrGroupPatternMatcher(string pattern)
		{
			List<PatternMatcher> matchers = new List<PatternMatcher>();
			PatternMatcher lastMatcher = null;
			bool terminate = false;
			while (Offset < pattern.Length && !terminate)
			{
				switch (pattern[Offset++])
				{
					case '\\':
						lastMatcher = AsSpecialMatcher(pattern);
						matchers.Add(lastMatcher);
						break;
					case '.':
						lastMatcher = new AnyPatternMatcher();
						matchers.Add(lastMatcher);
						break;
					case '(':
						lastMatcher = AsGroupMatcher(pattern);
						matchers.Add(lastMatcher);
						break;
					case '[':
						lastMatcher = AsCharBoundsMatcher(pattern);
						matchers.Add(lastMatcher);
						break;
					case '*':
						lastMatcher = new WildcardPatternMatcher(lastMatcher);
						matchers.SetLast(lastMatcher);
						break;
					case '+':
						lastMatcher = new OneOrMorePatternMatcher(lastMatcher);
						matchers.SetLast(lastMatcher);
						break;
					case '?':
						lastMatcher = new OptionalPatternMatcher(lastMatcher);
						matchers.SetLast(lastMatcher);
						break;
					case '|':
						terminate = true;
						break;
					case ')':
						Offset--;
						terminate = true;
						break;
					case '{':
						lastMatcher = AsCountBoundsMatcher(pattern, lastMatcher);
						matchers.SetLast(lastMatcher);
						break;
					default:
						Offset--;
						lastMatcher = AsLiteralMatcher(pattern);
						matchers.Add(lastMatcher);
						break;
				}
			}
			return matchers.Count == 1 ? matchers[0] : new GroupPatternMatcher(matchers);
		}

		public static string Escape(string text)
		{
			StringBuilder builder = new StringBuilder();
			int lastEscape = 0;
			ReadOnlySpan<char> textSpan = text.AsSpan();
			for (int i = 0; i < textSpan.Length; i++)
			{
				switch (text[i])
				{
					case '\\':
					case ')':
					case '.':
					case '|':
					case '(':
					case '*':
					case '+':
					case '?':
					case '!':
					case '{':
					case '}':
					case '[':
					case ']':
						builder.Append(textSpan.Slice(lastEscape, i - lastEscape).ToArray());
						builder.Append('\\');
						lastEscape = i;
						break;
					case '\n':
						builder.Append(textSpan.Slice(lastEscape, i - lastEscape).ToArray());
						lastEscape = ++i;
						builder.Append("\\n");
						break;
					case '\r':
						builder.Append(textSpan.Slice(lastEscape, i - lastEscape).ToArray());
						lastEscape = ++i;
						builder.Append("\\r");
						break;
					case '\t':
						builder.Append(textSpan.Slice(lastEscape, i - lastEscape).ToArray());
						lastEscape = ++i;
						builder.Append("\\t");
						break;
				}
			}
			return builder.Append(textSpan.Slice(lastEscape, textSpan.Length - lastEscape).ToArray()).ToString();
		}

		public static string EscapeNonWhitespace(string text)
		{
			StringBuilder builder = new StringBuilder();
			int lastEscape = 0;
			ReadOnlySpan<char> textSpan = text.AsSpan();
			for (int i = 0; i < textSpan.Length; i++)
			{
				switch (text[i])
				{
					case '\\':
					case ')':
					case '.':
					case '|':
					case '(':
					case '*':
					case '+':
					case '?':
					case '!':
					case '{':
					case '}':
					case '[':
					case ']':
						builder.Append(textSpan.Slice(lastEscape, i - lastEscape).ToArray());
						builder.Append('\\');
						lastEscape = i;
						break;
				}
			}
			return builder.Append(textSpan.Slice(lastEscape, textSpan.Length - lastEscape).ToArray()).ToString();
		}

		public static string Unescape(string text)
		{
			StringBuilder builder = new StringBuilder();
			int lastEscape = 0;
			bool isEscaped = false;
			ReadOnlySpan<char> textSpan = text.AsSpan();
			for (int i = 0; i < textSpan.Length; i++)
			{

				switch (text[i])
				{
					case '\\':
						if (isEscaped)
						{
							builder.Append(textSpan.Slice(lastEscape, i - lastEscape).ToArray());
						}
						isEscaped = !isEscaped;
						lastEscape = i;
						break;
					case 'n':
						if (isEscaped)
						{
							builder.Append("\\n");
							lastEscape = ++i;
							isEscaped = false;
						}
						break;
					case '\r':
						if (isEscaped)
						{
							builder.Append("\\r");
							isEscaped = false;
						}
						break;
					case '\t':
						if (isEscaped)
						{
							builder.Append("\\t");
							isEscaped = false;
						}
						break;
					default:
						isEscaped = false;
						break;
				}
			}
			return builder.Append(textSpan.Slice(lastEscape, textSpan.Length - lastEscape).ToArray()).ToString();
		}
	}
}
