using Synfron.Staxe.Executor;
using Synfron.Staxe.Matcher.Data;
using System.Collections.Generic;

namespace Synfron.Staxe.Generator
{
	public interface IGroupStateGenerator<G> where G : IGroupState<G>, new()
	{
		IList<G> Generate(IMatchData matchData);
	}
}
