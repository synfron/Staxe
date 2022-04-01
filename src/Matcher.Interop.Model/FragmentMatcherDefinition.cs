using Synfron.Staxe.Matcher.Input;
using System;
using System.Collections.Generic;

namespace Synfron.Staxe.Matcher.Interop.Model
{
	public class FragmentMatcherDefinition : IEquatable<FragmentMatcherDefinition>
	{
		private string _name;
		private int _hashCode;

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
				_hashCode = _name?.GetHashCode() ?? 0;
			}
		}

		public string Start { get; set; }

		public string End { get; set; }

		public IList<string> Parts { get; set; } = new List<string>();

		public IList<string> Actions { get; set; }

		public string PartsDelimiter { get; set; }

		public bool PartsDelimiterRequired { get; set; } = true;

		public string PartsPadding { get; set; }

		public int? MinMatchedParts { get; set; }

		public MatchMode PartsMatchMode { get; set; }

		public bool IsNoise { get; set; }

		public FallThroughMode FallThroughMode { get; set; } = FallThroughMode.None;

		[Obsolete("Replaced by FallThroughMode", true)]
		public bool FallThrough { get; set; }

		public bool Cacheable { get; set; }

		public bool ClearCache { get; set; }

		public ExpressionMode ExpressionMode { get; set; }

		public int? ExpressionOrder { get; set; }

		public bool BoundsAsParts { get; set; }

		public bool DiscardBounds { get; set; }

		public bool Negate { get; set; }

		public bool Equals(FragmentMatcherDefinition other)
		{
			return other?.Name == Name;
		}

		public override bool Equals(object obj)
		{
			return (obj is FragmentMatcherDefinition other) && Equals(other);
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