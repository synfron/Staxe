using System.Net;

namespace Synfron.Staxe.Matcher.Data
{
	public class StringMatchData : IMatchData
	{
		public int StartIndex { get; set; }

		public int Length { get; set; }

		public string Text { get; set; }

		public string Name { get; set; }

		public bool IsNoise { get; set; }

		public bool Mergable { get; set; }

		public int Id { get; set; }

		public string ToXml()
		{
			return $"<{Name ?? string.Empty}>{WebUtility.HtmlEncode(Text)}</{Name ?? string.Empty}>";
		}

		public override string ToString()
		{
			return Text;
		}
	}
}
