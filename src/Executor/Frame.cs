using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Collections;

namespace Synfron.Staxe.Executor
{
	public class Frame<G> where G : IGroupState<G>, new()
	{

		public G GroupState
		{
			get; set;
		}

		public StackList<ValuePointer<G>> StackRegister
		{
			get; private set;
		} = new StackList<ValuePointer<G>>() { MaxSize = 1000 };

		public StackList<StackValuePointer<G>> StackPointers
		{
			get; private set;
		} = new StackList<StackValuePointer<G>>() { MaxSize = 1000 };

		public int PreviousInstructionIndex
		{
			get; set;
		}

		public int BlockDepth
		{
			get;
			set;
		}
	}
}
