namespace Synfron.Staxe.Executor.Interrupts
{
	public sealed class StepOutInterrupt<G> : IInterrupt<G> where G : IGroupState<G>, new()
	{
		public readonly int Depth;

		public StepOutInterrupt(int depth)
		{
			Depth = depth;
		}

		public bool Intersects(ExecutionState<G> executionState)
		{
			return executionState.Frames.Count < Depth;
		}

		public override int GetHashCode() => 7756449 + Depth.GetHashCode();

		public override bool Equals(object obj) => obj is StepOutInterrupt<G> other && other.Depth == Depth;
	}
}
