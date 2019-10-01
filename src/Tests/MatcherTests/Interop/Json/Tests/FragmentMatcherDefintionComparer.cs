using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatcherTests.Interop.Model.Tests
{
	public class FragmentMatcherDefintionComparer : IEqualityComparer<FragmentMatcherDefinition>
	{
		public bool Equals(FragmentMatcherDefinition x, FragmentMatcherDefinition y)
		{
			return Enumerable.SequenceEqual(x.Parts, y.Parts)
				&& x.BoundsAsParts == y.BoundsAsParts
				&& x.Cacheable == y.Cacheable
				&& x.ClearCache == y.ClearCache
				&& x.DiscardBounds == y.DiscardBounds
				&& x.End == y.End
				&& x.ExpressionMode == y.ExpressionMode
				&& x.ExpressionOrder == y.ExpressionOrder
				&& x.FallThrough == y.FallThrough
				&& x.IsNoise == y.IsNoise
				&& x.MinMatchedParts == y.MinMatchedParts
				&& x.Name == y.Name
				&& x.Negate == y.Negate
				&& x.PartsDelimiter == y.PartsDelimiter
				&& x.PartsDelimiterRequired == y.PartsDelimiterRequired
				&& x.PartsMatchMode == y.PartsMatchMode
				&& x.PartsPadding == y.PartsPadding
				&& x.Start == y.Start;
		}

		public int GetHashCode(FragmentMatcherDefinition obj) => throw new NotImplementedException();
	}
}
