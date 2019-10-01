using System;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class WholeWordPatternMatcher : PatternMatcher
	{
		private readonly PatternMatcher _subPattern;
		private static int _lastId;
		private readonly int _id;

		public WholeWordPatternMatcher(PatternMatcher subPattern)
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
			if (startOffset - 1 >= 0 && char.IsLetterOrDigit(text[startOffset - 1]))
			{
				return (false, 0);
			}

			(bool success, int offset) = _subPattern.IsMatch(text, startOffset);

			int next = startOffset + offset;
			return success && next < text.Length && char.IsLetterOrDigit(text[next]) ? (false, 0) : (success, offset);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchWholeWord{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
            if (startOffset - 1 >= 0 && char.IsLetterOrDigit(text[startOffset - 1]))
            {{
                return (false, 0);
            }}

            (bool success, int offset) = {string.Format(_subPattern.Generate(generator), "text", "startOffset")};

            int next = startOffset + offset;
            if (success && next < text.Length && char.IsLetterOrDigit(text[next]))
            {{
                return (false, 0);
            }}
            return (success, offset);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return $"`{(_subPattern is GroupPatternMatcher groupPattern ? groupPattern.ToString(true) : _subPattern.ToString())}";
		}
	}
}
