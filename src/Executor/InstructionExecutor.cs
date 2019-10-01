using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Interrupts;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Collections;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Executor
{
	public class InstructionExecutor<G> : IInstructionExecutor<G> where G : IGroupState<G>, new()
	{

		public ExecutionState<G> Execute(ExecutionState<G> executionState)
		{
			try
			{
				ISet<IInterrupt<G>> interrupts = Interrupts;
				ref int currentInstructionIndex = ref executionState.InstructionIndex;
				ref StackList<ValuePointer<G>> stackRegister = ref executionState.StackRegister;
				ref StackList<StackValuePointer<G>> stackPointers = ref executionState.StackPointers;
				ref Instruction<G>[] instructions = ref executionState.Instructions;
				ref HashSet<IInterrupt<G>> executionStateInterrupts = ref executionState.Interrupts;
				ref bool executable = ref executionState.Executable;
				while (executable && currentInstructionIndex < instructions.Length)
				{
					ref Instruction<G> instruction = ref instructions[currentInstructionIndex];
					if (instruction.Interruptable && (interrupts.Count > 0 || executionStateInterrupts.Count > 0))
					{
						TriggerInterrupt(executionState);
					}
					instruction.ExecutionBody(this, executionState, instruction.Payload, stackRegister, stackPointers);
					currentInstructionIndex++;
				}
				executable = false;
			}
			catch (EngineRuntimeException e)
			{
				int? sourceLocation = executionState?.GetCurrentSourcePosition();
				throw new EngineRuntimeException(e.Message, sourceLocation, e);
			}
			catch (Exception e)
			{
				int? sourceLocation = executionState?.GetCurrentSourcePosition();
				throw new EngineRuntimeException("Runtime execution error", sourceLocation, e);
			}
			return executionState;
		}

		public ISet<IInterrupt<G>> Interrupts
		{
			get;
			private set;
		} = new HashSet<IInterrupt<G>>();


		public Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> ExternalDynamicPointers
		{
			get;
			set;
		}

		public IDictionary<string, G> Groups
		{
			get;
			set;
		} = new Dictionary<string, G>(StringComparer.Ordinal);

		public IValueProvider<G> ValueProvider
		{
			get;
			set;
		} = new DefaultValueProvider<G>();

		public event InterruptedHandler<G> Interrupted;

		private void TriggerInterrupt(ExecutionState<G> executionState)
		{
			List<IInterrupt<G>> matchingInterrupts = Interrupts.Concat(executionState.Interrupts).Where(interrupt => interrupt.Intersects(executionState)).ToList();

			if (matchingInterrupts.Count > 0)
			{
				Interrupted?.Invoke(this, new InterruptedEventArgs<G>(matchingInterrupts, executionState));
			}
		}
	}
}
