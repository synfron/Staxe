using System;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor.Interrupts
{
	public delegate void InterruptedHandler<G>(InstructionExecutor<G> sender, InterruptedEventArgs<G> args) where G : IGroupState<G>, new();

	public class InterruptedEventArgs<G> : EventArgs where G : IGroupState<G>, new()
	{
		public List<IInterrupt<G>> Interrupts
		{
			get;
			private set;
		}

		public ExecutionState<G> ExecutionState
		{
			get;
			private set;
		}

		public InterruptedEventArgs(List<IInterrupt<G>> interrupts, ExecutionState<G> executionState)
		{
			Interrupts = interrupts;
			ExecutionState = executionState;
		}
	}
}
