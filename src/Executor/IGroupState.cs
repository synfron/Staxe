using Synfron.Staxe.Executor.Instructions.Flags;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor
{
	public interface IGroupState<G> where G : IGroupState<G>, new()
	{
		Modifiers Modifiers { get; set; }
		List<G> Dependencies { get; }
		Group<G> Group { get; set; }
		List<DeclaredValuePointer<G>> GroupPointers { get; }
		Dictionary<int, IValuable<G>> ActionOverrides { get; }
		Dictionary<string, int> PointerMap { get; }
		G Clone(Copy copyOptions);
		void Merge(G otherGroupState, IValueProvider<G> valueProvider, GroupMerge mergeOptions);
	}
}