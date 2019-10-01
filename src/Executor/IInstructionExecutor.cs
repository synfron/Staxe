using Synfron.Staxe.Executor.Interrupts;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using System;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor
{
	public interface IInstructionExecutor<G>
		where G : IGroupState<G>, new()
	{
		ISet<IInterrupt<G>> Interrupts { get; }

		Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> ExternalDynamicPointers { get; set; }

		IValueProvider<G> ValueProvider { get; set; }

		IDictionary<string, G> Groups { get; set; }

		event InterruptedHandler<G> Interrupted;

		ExecutionState<G> Execute(ExecutionState<G> executionState);
	}
}