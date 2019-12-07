using ExecutorTests.Shared;
using Moq;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Interrupts;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Collections;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Linq;
using Xunit;
using E = Synfron.Staxe.Executor.ExecutionState<ExecutorTests.TestMocks.MockableGroupState>;
using G = ExecutorTests.TestMocks.MockableGroupState;

namespace ExecutorTests
{
	public class InstructionExecutorTests
	{

		[Fact]
		public void InstructionExecutor_Execute_NotExecutable_StopProcessing()
		{
			InstructionExecutionBody<G> executionBody = Mock.Of<InstructionExecutionBody<G>>();
			InstructionExecutionBody<G> specialExecutionBody = Mock.Of<InstructionExecutionBody<G>>();
			Instruction<G> specialInstruction = TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, specialExecutionBody);
			Group<G> group = new Group<G>
			{
				Instructions = new[]
				{
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					specialInstruction,
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
				}
			};
			G groupState = Mock.Of<G>(m => m.Group == group);
			Mock.Get(specialExecutionBody).Setup(m => m(It.IsAny<IInstructionExecutor<G>>(), It.IsAny<E>(), It.IsAny<object[]>(), It.IsAny<StackList<ValuePointer<G>>>(), It.IsAny<StackList<StackValuePointer<G>>>())).Callback((IInstructionExecutor<G> ie, ExecutionState<G> e, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers) =>
			{
				e.Executable = false;
			});
			E executionState = new ExecutionState<G>(groupState);


			InstructionExecutor<G> sut = new InstructionExecutor<G>();
			sut.Execute(executionState);

			Assert.Equal(4, executionState.InstructionIndex);
		}

		[Fact]
		public void InstructionExecutor_Execute_AllExecutable()
		{
			InstructionExecutionBody<G> executionBody = Mock.Of<InstructionExecutionBody<G>>();
			Instruction<G> instruction = TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody);
			Group<G> group = new Group<G>
			{
				Instructions = new[]
				{
					instruction,
					instruction,
					instruction,
					instruction,
					instruction,
					instruction,
					instruction
				}
			};
			G groupState = Mock.Of<G>(m => m.Group == group);
			E executionState = new ExecutionState<G>(groupState);


			InstructionExecutor<G> sut = new InstructionExecutor<G>();
			sut.Execute(executionState);

			Assert.Equal(7, executionState.InstructionIndex);
			Mock.Get(executionBody).Verify(m => m(sut, executionState, It.IsAny<object[]>(), It.IsAny<StackList<ValuePointer<G>>>(), It.IsAny<StackList<StackValuePointer<G>>>()), Times.Exactly(7));
		}

		[Fact]
		public void InstructionExecutor_Execute_WithInterrupts()
		{
			InstructionExecutionBody<G> interruptableExecutionBody = Mock.Of<InstructionExecutionBody<G>>();
			InstructionExecutionBody<G> uninterruptableExecutionBody = Mock.Of<InstructionExecutionBody<G>>();
			Instruction<G> interruptableInstruction = TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, true, interruptableExecutionBody);
			Instruction<G> uninterruptableInstruction = TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, uninterruptableExecutionBody);
			Group<G> group = new Group<G>
			{
				Instructions = new[]
				{
					uninterruptableInstruction,
					uninterruptableInstruction,
					uninterruptableInstruction,
					interruptableInstruction,
					uninterruptableInstruction,
					uninterruptableInstruction,
					uninterruptableInstruction
				}
			};
			G groupState = Mock.Of<G>(m => m.Group == group);
			E executionState = new ExecutionState<G>(groupState);
			IInterrupt<G> inactiveInterrupt = Mock.Of<IInterrupt<G>>(m => m.Intersects(executionState) == false);
			IInterrupt<G>[] activeInterrupts = new IInterrupt<G>[]
			{
				Mock.Of<IInterrupt<G>>(m => m.Intersects(executionState) == true),
				Mock.Of<IInterrupt<G>>(m => m.Intersects(executionState) == true),
				Mock.Of<IInterrupt<G>>(m => m.Intersects(executionState) == true),
				Mock.Of<IInterrupt<G>>(m => m.Intersects(executionState) == true)
			};
			executionState.Interrupts.Add(inactiveInterrupt);
			executionState.Interrupts.Add(activeInterrupts[0]);
			executionState.Interrupts.Add(inactiveInterrupt);
			executionState.Interrupts.Add(activeInterrupts[1]);
			InterruptedHandler<G> interruptedHandler = Mock.Of<InterruptedHandler<G>>();

			InstructionExecutor<G> sut = new InstructionExecutor<G>();
			sut.Interrupts.Add(inactiveInterrupt);
			sut.Interrupts.Add(activeInterrupts[2]);
			sut.Interrupts.Add(inactiveInterrupt);
			sut.Interrupts.Add(activeInterrupts[3]);
			sut.Interrupted += interruptedHandler;
			Mock.Get(interruptedHandler).Setup(m => m(sut, It.IsAny<InterruptedEventArgs<G>>())).Callback((InstructionExecutor<G> sender, InterruptedEventArgs<G> args) =>
			{
				Assert.Equal(executionState, args.ExecutionState);
				Assert.Equal(activeInterrupts.OrderBy(i => i.GetHashCode()), args.Interrupts.OrderBy(i => i.GetHashCode()));
			});

			sut.Execute(executionState);

			Assert.Equal(7, executionState.InstructionIndex);
			Mock.Get(uninterruptableExecutionBody).Verify(m => m(sut, executionState, It.IsAny<object[]>(), It.IsAny<StackList<ValuePointer<G>>>(), It.IsAny<StackList<StackValuePointer<G>>>()), Times.Exactly(6));
			Mock.Get(interruptableExecutionBody).Verify(m => m(sut, executionState, It.IsAny<object[]>(), It.IsAny<StackList<ValuePointer<G>>>(), It.IsAny<StackList<StackValuePointer<G>>>()), Times.Once);
			Mock.Get(interruptedHandler).Verify(m => m(sut, It.IsAny<InterruptedEventArgs<G>>()), Times.Once);
		}

		[Fact]
		public void InstructionExecutor_Execute_EngineRuntimeException_RethrownAsNewEngineRuntimeException()
		{
			InstructionExecutionBody<G> executionBody = Mock.Of<InstructionExecutionBody<G>>();
			InstructionExecutionBody<G> specialExecutionBody = Mock.Of<InstructionExecutionBody<G>>();
			int sourcePosition = 20;
			string exceptionMessage = "Engine exception";
			EngineRuntimeException thrownException = new EngineRuntimeException(exceptionMessage);
			Instruction<G> specialInstruction = TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, sourcePosition, false, specialExecutionBody);
			Group<G> group = new Group<G>
			{
				Instructions = new[]
				{
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					specialInstruction,
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody)
				}
			};
			G groupState = Mock.Of<G>(m => m.Group == group);
			Mock.Get(specialExecutionBody).Setup(m => m(It.IsAny<IInstructionExecutor<G>>(), It.IsAny<E>(), It.IsAny<object[]>(), It.IsAny<StackList<ValuePointer<G>>>(), It.IsAny<StackList<StackValuePointer<G>>>())).Callback((IInstructionExecutor<G> ie, ExecutionState<G> e, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers) =>
			{
				throw thrownException;
			});
			E executionState = new ExecutionState<G>(groupState);


			InstructionExecutor<G> sut = new InstructionExecutor<G>();
			EngineRuntimeException ex = Assert.Throws<EngineRuntimeException>(() => sut.Execute(executionState));
			Assert.Equal(exceptionMessage, ex.Message);
			Assert.Equal(thrownException, ex.InnerException);
			Assert.Equal(sourcePosition, ex.Position);
		}

		[Fact]
		public void InstructionExecutor_Execute_Exception_RethrownAsEngineRuntimeException()
		{
			InstructionExecutionBody<G> specialExecutionBody = Mock.Of<InstructionExecutionBody<G>>();
			InstructionExecutionBody<G> executionBody = Mock.Of<InstructionExecutionBody<G>>();
			int sourcePosition = 20;
			Exception thrownException = new Exception("Engine exception");
			Instruction<G> specialInstruction = TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, sourcePosition, false, specialExecutionBody);
			Group<G> group = new Group<G>
			{
				Instructions = new[]
				{
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					specialInstruction,
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody),
					TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, null, null, false, executionBody)
				}
			};
			G groupState = Mock.Of<G>(m => m.Group == group);
			Mock.Get(specialExecutionBody).Setup(m => m(It.IsAny<IInstructionExecutor<G>>(), It.IsAny<E>(), It.IsAny<object[]>(), It.IsAny<StackList<ValuePointer<G>>>(), It.IsAny<StackList<StackValuePointer<G>>>())).Callback((IInstructionExecutor<G> ie, ExecutionState<G> e, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers) =>
			{
				throw thrownException;
			});
			E executionState = new ExecutionState<G>(groupState);


			InstructionExecutor<G> sut = new InstructionExecutor<G>();
			EngineRuntimeException ex = Assert.Throws<EngineRuntimeException>(() => sut.Execute(executionState));
			Assert.Equal("Runtime execution error", ex.Message);
			Assert.Equal(thrownException, ex.InnerException);
			Assert.Equal(sourcePosition, ex.Position);
		}

	}
}
