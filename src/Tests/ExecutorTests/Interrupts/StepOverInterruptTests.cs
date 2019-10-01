using Moq;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Interrupts;
using System.Linq;
using Xunit;
using G = ExecutorTests.TestMocks.MockableGroupState;

namespace ExecutorTests.Interrupts
{
	public class StepOverInterruptTests
	{
		[Theory]
		[InlineData(10, false)]
		[InlineData(16, true)]
		[InlineData(20, true)]
		public void StepOverInterrupt_Intersects(int depth, bool triggered)
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.Frames.AddRange(Enumerable.Repeat<Frame<G>>(null, 15).ToArray(), 15);

			StepOverInterrupt<G> sut = new StepOverInterrupt<G>(depth);
			bool result = sut.Intersects(executionState);

			Assert.Equal(triggered, result);
		}
	}
}
