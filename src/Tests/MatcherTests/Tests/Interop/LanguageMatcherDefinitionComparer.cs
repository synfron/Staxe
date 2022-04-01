using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatcherTests.Tests.Interop
{
    public class LanguageMatcherDefinitionComparer : IEqualityComparer<LanguageMatcherDefinition>
    {
        public bool Equals(LanguageMatcherDefinition x, LanguageMatcherDefinition y)
        {
            return x.Fragments.SequenceEqual(y.Fragments, new FragmentMatcherDefintionComparer())
                && x.Patterns.SequenceEqual(y.Patterns, new PatternMatcherDefinitionComparer())
                && x.IndexingMode == y.IndexingMode
                && x.LogMatches == y.LogMatches
                && x.Name == y.Name
                && x.StartingFragment == y.StartingFragment
                && (x.Actions == y.Actions ||
                (x.Actions?.OrderBy(action => action.Name) ?? Enumerable.Empty<MatcherActionDefinition>())
                .SequenceEqual(y.Actions?.OrderBy(action => action.Name) ?? Enumerable.Empty<MatcherActionDefinition>(),
                new MatcherActionDefintionComparer()));
        }

        public int GetHashCode(LanguageMatcherDefinition obj) => throw new NotImplementedException();
    }
}
