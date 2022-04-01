using System;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Matcher.Data
{
	public class FragmentMatchData : IMatchData, IComparable<FragmentMatchData>
	{
		public int StartIndex { get; set; }

		public int Length { get; set; }

		public List<IMatchData> Parts { get; set; } = new List<IMatchData>();

		public int? ExpressionOrder { get; set; }

		public int EndDistinctIndex { get; set; }

		public string Name
		{
			get;
			set;
		}


        public string ToXml()
		{
			return $"<{Name ?? string.Empty}>{string.Join("", Parts.Select(part => part.ToXml()))}</{Name ?? string.Empty}>";
		}

		public int CompareTo(FragmentMatchData other)
		{
			return StartIndex - other.StartIndex;
		}

		public override bool Equals(object obj)
		{
			return obj is FragmentMatchData other ? other.StartIndex == StartIndex && other.Length == Length : false;
		}

		public override int GetHashCode()
		{
			return StartIndex.GetHashCode() + Length.GetHashCode();
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
