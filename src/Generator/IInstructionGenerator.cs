using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Matcher.Data;
using System.Collections.Generic;

namespace Synfron.Staxe.Generator
{
	public interface IInstructionGenerator<G> where G : IGroupState<G>, new()
	{
		IList<Instruction<G>> Generate(IMatchData matchData);
	}
}