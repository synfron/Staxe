using Synfron.Staxe.Matcher.Input.Actions;
using Synfron.Staxe.Matcher.Input.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Synfron.Staxe.Matcher.Input
{
	public class FragmentMatcher : IMatcher, IEquatable<FragmentMatcher>
	{
		private readonly int _hashCode;

		public FragmentMatcher(
			int id,
			string name,
			PatternMatcher start = default,
			PatternMatcher end = default,
			IList<IMatcher> parts = default,
			PatternMatcher partsDelimiter = default,
			bool partsDelimiterRequired = true,
			PatternMatcher partsPadding = default,
			int? minMatchedParts = default,
			MatchMode partsMatchMode = default,
			bool isNoise = default,
			FallThroughMode fallThroughMode = FallThroughMode.None,
			bool cacheable = default,
			bool clearCache = default,
			ExpressionMode expressionMode = default,
			int? expressionOrder = default,
			bool boundsAsParts = default,
			bool discardBounds = default,
			bool negate = default,
			IList<MatcherAction> actions = default
		)
		{
			Id = id;
			Name = name;
			Start = start;
			End = end;
			Parts = parts ?? new List<IMatcher>();
			PartsDelimiter = partsDelimiter;
			PartsDelimiterRequired = partsDelimiterRequired;
			PartsPadding = partsPadding;
			MinMatchedParts = minMatchedParts;
			PartsMatchMode = partsMatchMode;
			IsNoise = isNoise;
			FallThroughMode = fallThroughMode;
			Cacheable = cacheable;
			ClearCache = clearCache;
			ExpressionMode = expressionMode;
			ExpressionOrder = expressionOrder;
			BoundsAsParts = boundsAsParts;
			DiscardBounds = discardBounds;
			Negate = negate;
			Actions = actions;

			_hashCode = name.GetHashCode();
		}

		public int Id { get; set; }

		public string Name { get; set; }

		public PatternMatcher Start { get; set; }

		public PatternMatcher End { get; set; }

		public IList<IMatcher> Parts { get; set; }

		public PatternMatcher PartsDelimiter { get; set; }

		public bool PartsDelimiterRequired { get; set; }

		public PatternMatcher PartsPadding { get; set; }

		public int? MinMatchedParts { get; set; }

		public MatchMode PartsMatchMode { get; set; }

		public bool IsNoise { get; set; }

		public FallThroughMode FallThroughMode { get; set; } = FallThroughMode.None;

		public bool Cacheable { get; set; }

		public bool ClearCache { get; set; }

		public ExpressionMode ExpressionMode { get; set; }

		public int? ExpressionOrder { get; set; }

		public bool BoundsAsParts { get; set; }

		public bool DiscardBounds { get; set; }

		public bool Negate { get; set; }

		public IList<MatcherAction> Actions { get; set; }

		public bool Equals(FragmentMatcher other)
		{
			return other.Name == Name;
		}

		public override bool Equals(object obj)
		{
			return (obj is FragmentMatcher other) && Equals(other);
		}

		public override int GetHashCode()
		{
			return _hashCode;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
