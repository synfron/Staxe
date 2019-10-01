using Synfron.Staxe.Matcher.Input.Patterns;
using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.Collections.Generic;

namespace MatcherTests.Interop.Model.Tests
{
	public class PatternMatcherDefinitionComparer : IEqualityComparer<PatternMatcherDefinition>
	{
		public bool Equals(PatternMatcherDefinition x, PatternMatcherDefinition y)
		{
			return x.IsNoise == y.IsNoise
				&& x.Mergable == y.Mergable
				&& x.Name == y.Name
				&& PatternReader.Parse(x.Pattern).ToString() == PatternReader.Parse(y.Pattern).ToString();
		}

		public int GetHashCode(PatternMatcherDefinition obj) => throw new NotImplementedException();
	}
}
