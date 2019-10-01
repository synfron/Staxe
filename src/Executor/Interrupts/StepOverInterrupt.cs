namespace Synfron.Staxe.Executor.Interrupts
{
	public sealed class StepOverInterrupt<G> : IInterrupt<G> where G : IGroupState<G>, new()
	{
		private readonly int Depth;

		public StepOverInterrupt(int depth)
		{
			Depth = depth;
		}

		public bool Intersects(ExecutionState<G> executionState)
		{
			return executionState.Frames.Count <= Depth;
		}

		public override int GetHashCode() => 5858678 + Depth.GetHashCode();

		public override bool Equals(object obj) => obj is StepOverInterrupt<G> other && other.Depth == Depth;
	}
}
