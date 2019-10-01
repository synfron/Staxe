namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class CharBoundsPatternMatcher : PatternMatcher
	{
		private readonly char _lowerBounds;
		private readonly char _upperBounds;
		private static int _lastId;
		private readonly int _id;

		public CharBoundsPatternMatcher(char lowerBounds, char upperBounds)
		{
			_lowerBounds = lowerBounds;
			_upperBounds = upperBounds;
			_id = _lastId++;
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
		{
			int charVal = text[startOffset];
			return (_lowerBounds <= charVal && _upperBounds >= charVal) ? (true, 1) : (false, 0);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchCharBounds{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
			int charVal = text[startOffset];
			return ({(int)_lowerBounds} <= charVal && {(int)_upperBounds} >= charVal) ? (true, 1) : (false, 0);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return $"[{_lowerBounds}-{_upperBounds}]";
		}
	}
}
