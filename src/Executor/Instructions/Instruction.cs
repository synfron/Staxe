using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Collections;

namespace Synfron.Staxe.Executor.Instructions
{

	public readonly struct Instruction<G>
			where G : IGroupState<G>, new()
	{
		internal Instruction(InstructionCode code, object[] payload, int? sourcePosition, bool interruptable, InstructionExecutionBody<G> execute)
		{
			Code = code;
			Payload = payload;
			SourcePosition = sourcePosition;
			Interruptable = interruptable;
			ExecutionBody = execute;
		}

		public InstructionCode Code { get; }

		public object[] Payload { get; }

		public int? SourcePosition { get; }


		public bool Interruptable { get; }


		public InstructionExecutionBody<G> ExecutionBody { get; }

	}

	public static class InstructionExtensions
	{
		public static void Execute<G>(this Instruction<G> instruction, IInstructionExecutor<G> executor, ExecutionState<G> executionState, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
			where G : IGroupState<G>, new()
		{
			instruction.ExecutionBody(executor, executionState, instruction.Payload, stackRegister, stackPointers);
		}
	}
}
