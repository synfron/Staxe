using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatcherTests.Interop.Model.Tests
{
	public class LanguageMatcherDefinitionComparer : IEqualityComparer<LanguageMatcherDefinition>
	{
		public bool Equals(LanguageMatcherDefinition x, LanguageMatcherDefinition y)
		{
			return Enumerable.SequenceEqual(x.Fragments, y.Fragments, new FragmentMatcherDefintionComparer())
				&& Enumerable.SequenceEqual(x.Patterns, y.Patterns, new PatternMatcherDefinitionComparer())
				&& x.IndexingMode == y.IndexingMode
				&& x.LogMatches == y.LogMatches
				&& x.Name == y.Name
				&& x.StartingFragment == y.StartingFragment;
		}

		public int GetHashCode(LanguageMatcherDefinition obj) => throw new NotImplementedException();
	}
}
