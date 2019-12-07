using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;

namespace ExecutorTests.Shared
{
	public class TestInstructionProvider<G> : InstructionProvider<G> where G : IGroupState<G>, new()
	{
		public new static Instruction<G> GetInstruction(InstructionCode code, object[] payload, int? sourcePosition, bool interruptable, InstructionExecutionBody<G> executionBody)
		{
			return InstructionProvider<G>.GetInstruction(code, payload, sourcePosition, interruptable, executionBody ?? ((executor, executionState, _, stackRegister, stackPointers) => { }));
		}
	}
}
