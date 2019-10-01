namespace Synfron.Staxe.Executor.Interrupts
{
	public sealed class StepInterrupt<G> : IInterrupt<G> where G : IGroupState<G>, new()
	{
		public bool Intersects(ExecutionState<G> executionState)
		{
			return true;
		}

		public override int GetHashCode() => 857449;

		public override bool Equals(object obj) => obj is StepInterrupt<G>;
	}
}
