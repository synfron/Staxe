namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class LiteralPatternMatcher : PatternMatcher
	{
		private readonly string _literal;
		private static int _lastId;
		private readonly int _id;

		public LiteralPatternMatcher(string literal)
		{
			_literal = literal;
			_id = _lastId++;
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
		{
			string literal = _literal;
			int length = literal.Length;
			if (startOffset + length > text.Length)
			{
				return (false, 0);
			}
			for (int i = 0; i < length; i++)
			{
				if (text[i + startOffset] != literal[i])
				{
					return (false, 0);
				}
			}
			return (true, length);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchLiteral{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
            string literal = {$"\"{_literal.Replace(@"\", @"\\").Replace(@"""", @"\""").Replace("\n", @"\n").Replace("\t", @"\t")}\""};
            int length = literal.Length;
            if (startOffset + length > text.Length)
            {{
                return (false, 0);
            }}
            for (int i = 0; i < length; i++)
            {{
                if (text[i + startOffset] != literal[i])
                {{
                    return (false, 0);
                }}
            }}
            return (true, length);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return _literal
				.Replace(@"\", @"\\")
				.Replace(".", @"\.")
				.Replace("+", @"\+")
				.Replace("*", @"\*")
				.Replace("?", @"\?")
				.Replace("!", @"\!")
				.Replace("(", @"\(")
				.Replace(@")", @"\)")
				.Replace(@"}", @"\}")
				.Replace(@"{", @"\{")
				.Replace(@"[", @"\[")
				.Replace(@"]", @"\]")
				.Replace("\r", @"\r")
				.Replace("\n", @"\n")
				.Replace("\t", @"\t")
				.Replace(@"|", @"\|");
		}
	}
}
