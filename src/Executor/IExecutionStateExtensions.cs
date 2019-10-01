using System.Linq;

namespace Synfron.Staxe.Executor
{

	public static class IExecutionStateExtensions
	{
		public static int? GetCurrentSourcePosition<G>(this ExecutionState<G> executionState) where G : IGroupState<G>, new()
		{
			int? sourceLocation = executionState.GroupState.Group.Instructions.ElementAtOrDefault(executionState.InstructionIndex).SourcePosition;
			return sourceLocation;
		}
	}
}