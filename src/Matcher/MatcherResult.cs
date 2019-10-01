using Synfron.Staxe.Matcher.Data;

namespace Synfron.Staxe.Matcher
{
	public readonly struct MatcherResult
	{
		public MatcherResult(IMatchData matchData = default, bool success = default, int endIndex = default, int? failureIndex = default, string matchLog = default)
		{
			MatchData = matchData;
			Success = success;
			EndIndex = endIndex;
			FailureIndex = failureIndex;
			MatchLog = matchLog;
		}

		public int? FailureIndex { get; }
		public IMatchData MatchData { get; }
		public string MatchLog { get; }
		public bool Success { get; }
		public int EndIndex { get; }

		public void Deconstruct(out IMatchData matchData, out bool success, out int endIndex, out int? failureIndex, out string matchLog)
		{
			matchData = MatchData;
			success = Success;
			endIndex = EndIndex;
			failureIndex = FailureIndex;
			matchLog = MatchLog;
		}
	}
}
