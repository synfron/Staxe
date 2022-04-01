namespace Synfron.Staxe.Matcher.Data
{
	public interface IMatchData
	{
		int StartIndex { get; set; }

		int Length { get; set; }

		string Name { get; }

		string ToXml();

	}
}
