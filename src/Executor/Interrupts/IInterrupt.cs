namespace Synfron.Staxe.Executor.Interrupts
{
	public interface IInterrupt<G> where G : IGroupState<G>, new()
	{
		bool Intersects(ExecutionState<G> executionState);
	}
}