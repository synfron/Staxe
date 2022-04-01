using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatcherTests.Tests.Interop
{
    public class FragmentMatcherDefintionComparer : IEqualityComparer<FragmentMatcherDefinition>
    {
        public bool Equals(FragmentMatcherDefinition x, FragmentMatcherDefinition y)
        {
            return x.Parts.SequenceEqual(y.Parts ?? Enumerable.Empty<string>())
                && x.BoundsAsParts == y.BoundsAsParts
                && x.Cacheable == y.Cacheable
                && x.ClearCache == y.ClearCache
                && x.DiscardBounds == y.DiscardBounds
                && x.End == y.End
                && x.ExpressionMode == y.ExpressionMode
                && x.ExpressionOrder == y.ExpressionOrder
                && x.FallThroughMode == y.FallThroughMode
                && x.IsNoise == y.IsNoise
                && x.MinMatchedParts == y.MinMatchedParts
                && x.Name == y.Name
                && x.Negate == y.Negate
                && x.PartsDelimiter == y.PartsDelimiter
                && x.PartsDelimiterRequired == y.PartsDelimiterRequired
                && x.PartsMatchMode == y.PartsMatchMode
                && x.PartsPadding == y.PartsPadding
                && x.Start == y.Start
                && (x.Actions ?? Enumerable.Empty<string>()).SequenceEqual(y.Actions ?? Enumerable.Empty<string>());
        }

        public int GetHashCode(FragmentMatcherDefinition obj) => throw new NotImplementedException();
    }
}
