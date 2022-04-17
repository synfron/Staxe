using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatcherTests.Tests.Interop
{
    public class MatcherActionDefintionComparer : IEqualityComparer<MatcherActionDefinition>
    {
        public bool Equals(MatcherActionDefinition x, MatcherActionDefinition y)
        {
            return x.Change == y.Change
                && x.Source == y.Source
                && x.FirstVariableName == y.FirstVariableName
                && x.SecondVariableName == y.SecondVariableName
                && x.Name == y.Name
                && x.Assert == y.Assert
                && x.Action == y.Action
                && x.Value == y.Value;
        }

        public int GetHashCode(MatcherActionDefinition obj) => throw new NotImplementedException();
    }
}
