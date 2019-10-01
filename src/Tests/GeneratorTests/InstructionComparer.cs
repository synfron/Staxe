using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using System.Collections.Generic;
using System.Linq;

namespace GeneratorTests
{
	public class InstructionComparer<G> : IEqualityComparer<Instruction<G>> where G : IGroupState<G>, new()
	{

		public bool WithDebugInfo
		{
			get;
			set;
		}

		public bool Equals(Instruction<G> x, Instruction<G> y)
		{
			return x.Code == y.Code
				&& PayloadEquals(x.Payload, y.Payload)
				&& x.ExecutionBody == y.ExecutionBody
				&& (!WithDebugInfo || (x.Interruptable == y.Interruptable && x.SourcePosition == y.SourcePosition));
		}

		public int GetHashCode(Instruction<G> instruction)
		{
			return instruction.Code.GetHashCode()
				+ (instruction.Payload?.GetHashCode() ?? 0)
				+ (instruction.ExecutionBody?.GetHashCode() ?? 0)
				+ (WithDebugInfo ? (instruction.Interruptable.GetHashCode()) + (instruction.SourcePosition?.GetHashCode() ?? 0) : 0);
		}

		private bool PayloadEquals(object[] x, object[] y)
		{
			return x is null && y is null || (x != null && y != null && Enumerable.SequenceEqual(x, y));
		}
	}
}
