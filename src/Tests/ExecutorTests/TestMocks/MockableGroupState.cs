using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions.Flags;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using System.Collections.Generic;

namespace ExecutorTests.TestMocks
{
	public class MockableGroupState : IGroupState<MockableGroupState>
	{
		public virtual Modifiers Modifiers { get; set; }
		public virtual List<MockableGroupState> Dependencies { get; }
		public virtual Group<MockableGroupState> Group { get; set; } = new Group<MockableGroupState>();
		public virtual List<DeclaredValuePointer<MockableGroupState>> GroupPointers { get; }
		public virtual Dictionary<int, IValuable<MockableGroupState>> ActionOverrides { get; }
		public virtual Dictionary<string, int> PointerMap { get; }

		public virtual MockableGroupState Clone(Copy copyOptions, Dictionary<object, object> entityMap = null) { return null; }
		public virtual void Merge(MockableGroupState otherGroupState, IValueProvider<MockableGroupState> valueProvider, GroupMerge mergeOptions) { }
	}
}
