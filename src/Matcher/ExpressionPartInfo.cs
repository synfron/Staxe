using Synfron.Staxe.Matcher.Data;

namespace Synfron.Staxe.Matcher
{
	public class ExpressionPartInfo
	{
		public IMatchData Part { get; set; }

		public ExpressionPartInfo Previous { get; set; }

		public ExpressionPartInfo Next { get; set; }

		public int? ExpressionOrder { get; set; }
	}
}
