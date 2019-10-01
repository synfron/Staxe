using System.Collections.Generic;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public class GroupPatternMatcher : PatternMatcher
	{
		private readonly IList<PatternMatcher> _subPatterns;
		private static int _lastId;
		private readonly int _id;

		public GroupPatternMatcher(IList<PatternMatcher> subPatterns)
		{
			_subPatterns = subPatterns;
			_id = _lastId++;
		}

		public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
		{
			int offset = startOffset;
			foreach (PatternMatcher matcher in _subPatterns)
			{
				(bool success, int subOffset) = matcher.IsMatch(text, offset);
				if (!success)
				{
					return (false, 0);
				}
				offset += subOffset;
			}
			return (true, offset - startOffset);
		}

		internal override string Generate(MatcherEngineGenerator generator)
		{
			string methodName = $"MatchGroup{_id}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				int numPatterns = _subPatterns.Count;
				string[] parts = new string[numPatterns];
				for (int i = 0; i < numPatterns; i++)
				{
					parts[i] = $@"
                (success, subOffset) = {string.Format(_subPatterns[i].Generate(generator), "text", "offset")};
                if (!success)
                {{
                    return (false, 0);
                }}
                offset += subOffset;";
				}

				string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{

            int offset = startOffset;
            bool success;
            int subOffset;
            {string.Join("\n", parts)}
            return (true, offset - startOffset);
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}

		public override string ToString()
		{
			return $"({string.Join("", _subPatterns)})";
		}

		public string ToString(bool getInner)
		{
			return getInner ? string.Join("", _subPatterns) : ToString();
		}
	}
}
