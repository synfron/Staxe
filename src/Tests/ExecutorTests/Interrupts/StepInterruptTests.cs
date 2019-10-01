using Moq;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Interrupts;
using Xunit;
using G = ExecutorTests.TestMocks.MockableGroupState;

namespace ExecutorTests.Interrupts
{
	public class StepInterruptTests
	{
		[Fact]
		public void StepInterrupt_Intersects()
		{
			Group<G> group = new Group<G> { GroupName = "groupname" };
			G groupState = Mock.Of<G>(m => m.Group == group);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);

			StepInterrupt<G> sut = new StepInterrupt<G>();
			bool result = sut.Intersects(executionState);

			Assert.True(result);
		}
	}
}
