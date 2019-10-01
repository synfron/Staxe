using Synfron.Staxe.Executor.Pointers;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor.Values
{

	public interface IActionValue<G> : IValuable<G>
		where G : IGroupState<G>, new()
	{

		G GroupState { get; }

		int Location { get; }

		string Identifier { get; set; }

		List<StackValuePointer<G>> InitStackPointers { get; }

		IActionValue<G> Clone(G newGroupState = default, int? location = null);
	}
}