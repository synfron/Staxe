using System.Collections.Generic;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class OrPatternMatcher : PatternMatcher
	{
		private readonly IList<PatternMatcher> _subPatterns;
		private static int _lastId;
		private readonly int _id;

		public OrPatternMatcher(IList<PatternMatcher> subPatterns)
		{
			_subPatterns = subPatterns;
			_id = _lastId++;
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
		{
			int numPatterns = _subPatterns.Count;
			for (int i = 0; i < numPatterns; i++)
			{
				PatternMatcher matcher = _subPatterns[i];
				(bool success, int subOffset) = matcher.IsMatch(text, startOffset);
				if (success)
				{
					return (true, subOffset);
				}
			}
			return (false, 0);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchOr{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				int numPatterns = _subPatterns.Count;
				string[] parts = new string[numPatterns];
				for (int i = 0; i < numPatterns; i++)
				{
					parts[i] = $@"
                (success, subOffset) = {string.Format(_subPatterns[i].Generate(generator), "text", "startOffset")};
                if (success)
                {{
                    return (true, subOffset);
                }}";
				}

				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
            bool success;
            int subOffset;
            {string.Join("\n", parts)}
            return (false, 0);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return $"({string.Join("|", _subPatterns)})";
		}
	}
}
