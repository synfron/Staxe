using Synfron.Staxe.Matcher.Data;
using System.Collections.Generic;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
	public abstract class PatternMatcher : IMatcher
	{

		public string Name { get; set; }

		public int Id { get; set; }

		public bool IsNoise { get; set; }

		public bool IsStandard { get; set; } = true;

		public abstract (bool success, int offset) IsMatch(string text, int startOffset = 0);

		internal abstract string Generate(MatcherEngineGenerator generator);

		public abstract override string ToString();
	}
}
