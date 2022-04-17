using Synfron.Staxe.Matcher.Input.Patterns;
using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.Collections.Generic;

namespace MatcherTests.Tests.Interop
{
    public class PatternMatcherDefinitionComparer : IEqualityComparer<PatternMatcherDefinition>
    {
        public bool Equals(PatternMatcherDefinition x, PatternMatcherDefinition y)
        {
            bool result = x.IsNoise == y.IsNoise
                && x.Name == y.Name
                && x.IsAuxiliary == y.IsAuxiliary
                && x.Fragment == y.Fragment
                && PatternReader.Parse(x.Pattern ?? string.Empty).ToString() == PatternReader.Parse(y.Pattern ?? string.Empty).ToString();
            return x.IsNoise == y.IsNoise
                && x.Name == y.Name
                && x.IsAuxiliary == y.IsAuxiliary
                && x.Fragment == y.Fragment
                && PatternReader.Parse(x.Pattern ?? string.Empty).ToString() == PatternReader.Parse(y.Pattern ?? string.Empty).ToString();
        }

        public int GetHashCode(PatternMatcherDefinition obj) => throw new NotImplementedException();
    }
}
