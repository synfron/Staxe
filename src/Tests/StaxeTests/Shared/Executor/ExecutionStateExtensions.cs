using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using System.Linq;

namespace StaxeTests.Shared
{

	public static class ExecutionStateExtensions
	{

		public static IValuable GetReturn<G>(this ExecutionState<G> executionState) where G : IGroupState<G>, new()
		{
			return executionState.ListRegister.FirstOrDefault()?.Value;
		}

		public static T GetReturn<G, T>(this ExecutionState<G> executionState) where T : IValuable where G : IGroupState<G>, new()
		{
			return executionState.ListRegister.FirstOrDefault()?.Value is T value ? value : default;
		}

		public static IValuable<G> GetStackValue<E, G>(this E executionState, string pointerId) where E : ExecutionState<G> where G : IGroupState<G>, new()
		{
			return executionState.LastFrame.StackPointers.Reverse().OfType<StackValuePointer<G>>().Where(pointer => pointer.Identifier == pointerId).First().Value;
		}
	}
}
