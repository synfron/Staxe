using ExecutorTests.Shared;
using Moq;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Instructions.Flags;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using G = ExecutorTests.TestMocks.MockableGroupState;

namespace ExecutorTests.Instructions
{
	public class InstructionProviderTests
	{

		[Fact]
		public void InstructionProvider_GetInstruction()
		{
			foreach (InstructionCode instructionCode in Enum.GetValues(typeof(InstructionCode)).Cast<InstructionCode>().Skip(2))
			{
				Instruction<G> instruction = InstructionProvider<G>.GetInstruction(instructionCode);
				Assert.Contains($"ExecuteInstruction{instructionCode.ToString()}", instruction.ExecutionBody.Method.ToString());
			}
		}

		#region InstructionA
		[Fact]
		public void InstructionA_Execute_CreateFrame()
		{
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState)
			{
				InstructionIndex = 5
			};

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.A, new object[] { true });
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.ListRegister);
			Assert.Empty(executionState.StackRegister);
			Assert.Equal(2, executionState.Frames.Count);
			Assert.Equal(executionState.Frames.Last(), executionState.LastFrame);
			Assert.Equal(executionState.LastFrame.StackPointers, executionState.StackPointers);
			Assert.Equal(executionState.LastFrame.StackRegister, executionState.StackRegister);
			Assert.Equal(executionState.Frames[0].GroupState, executionState.LastFrame.GroupState);
			Assert.Equal(executionState.LastFrame.PreviousInstructionIndex, executionState.InstructionIndex);
		}

		[Fact]
		public void InstructionA_Execute_SameFrame()
		{
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.A, new object[] { false });
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.ListRegister);
			Assert.Empty(executionState.StackRegister);
			Assert.Single(executionState.Frames);
		}
		#endregion

		#region InstructionAE

		[Fact]
		public void InstructionAE_Execute_RemainingStack()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>())
			{
				Executable = true
			};
			Frame<G>[] frames = new Frame<G>[]
			{
				new Frame<G>()
			{
				GroupState = Mock.Of<G>(),
				PreviousInstructionIndex = 20
			},
				new Frame<G>()
			{
				GroupState = Mock.Of<G>(),
				PreviousInstructionIndex = 25,
			}
			};
			executionState.Frames.AddRange(frames, 2);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.AE);
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(2, executionState.Frames.Count);
			Assert.Equal(executionState.Frames.Last(), executionState.LastFrame);
			Assert.Equal(frames[0], executionState.LastFrame);
			Assert.Equal(frames[0].StackPointers, executionState.StackPointers);
			Assert.Equal(frames[0].StackRegister, executionState.StackRegister);
			Assert.Equal(frames[0].PreviousInstructionIndex, executionState.InstructionIndex);
			Assert.Equal(frames[0].GroupState, executionState.GroupState);
			Assert.True(executionState.Executable);
		}

		[Fact]
		public void InstructionAE_Execute_NoRemainingStack()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>())
			{
				Executable = true
			};

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.AE);
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
			Assert.Empty(executionState.Frames);
			Assert.False(executionState.Executable);
		}

		#endregion

		#region InstructionAVR

		[Fact]
		public void InstructionAVR_Execute_ArrayWithSize()
		{
			int mode = 3;
			int size = 20;
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G>
			{
				Value = Mock.Of<IValue<G, int>>(m => m.Data == size && m.GetData() == (object)size)
			});
			ICollectionValue<G> arrayValue = Mock.Of<ICollectionValue<G>>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m => m.GetCollection(It.IsAny<int?>(), It.IsAny<int?>()) == arrayValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CVR, new object[] { true, mode });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(valueProvider).Verify(m => m.GetCollection(size, mode), Times.Once);
			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(arrayValue, executionState.StackRegister[0].Value);
		}

		[Fact]
		public void InstructionAVR_Execute_UnsizedArray()
		{
			int mode = 3;
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			ICollectionValue<G> arrayValue = Mock.Of<ICollectionValue<G>>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m => m.GetCollection((int?)null, mode) == arrayValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CVR, new object[] { false, mode });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(valueProvider).Verify(m => m.GetCollection((int?)null, mode), Times.Once);
			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(arrayValue, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionB

		[Fact]
		public void InstructionB_Execute()
		{
			const int InstructionIndex = 20;
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>())
			{
				InstructionIndex = InstructionIndex
			};
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.B);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackRegister);
			Assert.Equal(1, executionState.LastFrame.BlockDepth);
		}

		#endregion

		#region InstructionBE

		[Fact]
		public void InstructionBE_Execute_HasBlockStatePoint()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.LastFrame.BlockDepth = 4;
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>(m => m.Depth == 3));
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>(m => m.Depth == 3));
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>(m => m.Depth == 3));
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>(m => m.Depth == 3));
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>(m => m.Depth == 4));
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>(m => m.Depth == 4));
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>(m => m.Depth == 4));
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.BE);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackRegister);
			Assert.Equal(4, executionState.StackPointers.Count);
			Assert.Equal(3, executionState.LastFrame.BlockDepth);
		}

		[Fact]
		public void InstructionBE_Execute_NoBlockStatePoints()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.BE);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackRegister);
			Assert.Empty(executionState.StackPointers);
			Assert.Equal(-1, executionState.LastFrame.BlockDepth);
		}

		#endregion

		#region InstructionC

		[Fact]
		public void InstructionC_Execute_TrueCondition()
		{
			const int ExpectedInstruction = 20;
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>()) { InstructionIndex = 10 };
			executionState.StackRegister.Add(new ValuePointer<G>() { Value = new DefaultBooleanValue<G>(true) });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.C, payload: new object[] { ExpectedInstruction + 1 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackRegister);
			Assert.Equal(ExpectedInstruction, executionState.InstructionIndex);
		}

		[Fact]
		public void InstructionC_Execute_FalseCondition()
		{
			const int ExpectedInstruction = 10;
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>()) { InstructionIndex = ExpectedInstruction };
			executionState.StackRegister.Add(new ValuePointer<G>() { Value = new DefaultBooleanValue<G>(false) });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.C, payload: new object[] { 20 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackRegister);
			Assert.Empty(executionState.StackPointers);
			Assert.Equal(ExpectedInstruction, executionState.InstructionIndex);
		}

		#endregion

		#region InstructionCDP

		[Fact]
		public void InstructionCDP_Execute()
		{
			const string PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			DeclaredValuePointer<G> valuePointer = new DeclaredValuePointer<G>("dynamic");
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = 10 };
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Add, valueProvider) == valuePointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CDP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackRegister);
			Assert.Equal(valuePointer, executionState.StackRegister[0]);
			Assert.Empty(executionState.StackPointers);
			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Add, valueProvider), Times.Once);
		}

		#endregion

		#region InstructionCRG

		[Fact]
		public void InstructionCRG_Execute_StaticGroup()
		{
			G groupState = Mock.Of<G>(m => m.Modifiers == (Modifiers.ExecuteRestricted | Modifiers.Static));
			DefaultGroupValue<G> groupValue = new DefaultGroupValue<G>(groupState);
			G clonedGroupState = Mock.Of<G>(m => m.Modifiers == (Modifiers.ExecuteRestricted | Modifiers.Static));
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CG, payload: new object[] { Copy.Modifiers });
			Assert.Throws<EngineRuntimeException>(() => sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers));
		}

		[Theory]
		[InlineData(Copy.None)]
		[InlineData(Copy.Modifiers)]
		public void InstructionCRG_Execute_NonStaticGroup(Copy payloadValue)
		{
			G groupState = Mock.Of<G>();
			DefaultGroupValue<G> groupValue = new DefaultGroupValue<G>(groupState);
			G clonedGroupState = Mock.Of<G>();
			DefaultGroupValue<G> clonedGroupValue = new DefaultGroupValue<G>(clonedGroupState);
			Mock.Get(groupState).Setup(m => m.Clone(It.IsAny<Copy>())).Returns(clonedGroupState);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });
			IValueProvider<G> valuePointer = Mock.Of<IValueProvider<G>>(m => m.GetGroup(It.IsAny<G>()) == clonedGroupValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valuePointer);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CG, payload: new object[] { payloadValue });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Mock.Get(groupState).Verify(m => m.Clone(payloadValue), Times.Once);
			Mock.Get(valuePointer).Verify(m => m.GetGroup(clonedGroupState), Times.Once);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(clonedGroupValue, Enumerable.Last(executionState.StackRegister).Value);
		}

		#endregion

		#region InstructionMG

		[Fact]
		public void InstructionMG_Execute()
		{
			GroupMerge mergeOption = GroupMerge.OverridePointers;
			G groupState = Mock.Of<G>();
			G otherGroupState = Mock.Of<G>();
			DefaultGroupValue<G> groupValue = new DefaultGroupValue<G>(groupState);
			DefaultGroupValue<G> otherGroupValue = new DefaultGroupValue<G>(otherGroupState);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = otherGroupValue });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MG, new object[] { mergeOption });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupValue, executionState.StackRegister.First().Value);
			Mock.Get(groupState).Verify(m => m.Merge(otherGroupState, valueProvider, mergeOption), Times.Once);
		}

		#endregion

		#region InstructionCGP

		[Fact]
		public void InstructionCGP_Execute()
		{
			const string GroupPointerName = "pointername";
			const int CurrentInstructionIndex = 20;
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group") };
			Dictionary<string, int> pointerMap = new Dictionary<string, int>();
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers && m.PointerMap == pointerMap);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = CurrentInstructionIndex };
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>()));

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CGP, payload: new object[] { GroupPointerName });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(2, groupPointers.Count);
			Assert.Equal(groupPointers[1], executionState.StackRegister[0]);
			Assert.Equal(GroupPointerName, groupPointers[1].Identifier); ;
		}

		#endregion

		#region InstructionCPHR

		[Fact]
		public void InstructionCPHR_Execute()
		{
			ValuePointer<G> valuePointer = new ValuePointer<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(valuePointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CPHR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(2, executionState.StackRegister.Count);
			Assert.Equal(valuePointer, executionState.StackRegister[0]);
			Assert.Equal(valuePointer, executionState.StackRegister[1]);
			Assert.Empty(executionState.StackPointers);
		}

		#endregion

		#region InstructionRM

		[Fact]
		public void InstructionRM_Execute()
		{
			List<ValuePointer<G>> originalStackRegister = new List<ValuePointer<G>>(Enumerable.Range(0, 5).Select(i => new ValuePointer<G>() { Value = new DefaultIntValue<G>(i) }));
			List<ValuePointer<G>> newStackRegister = new List<ValuePointer<G>>(originalStackRegister);
			newStackRegister.Insert(1, newStackRegister.Last());
			newStackRegister.RemoveAt(newStackRegister.Count - 1);
			ValuePointer<G> valuePointer = new ValuePointer<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.AddRange(originalStackRegister.ToArray(), originalStackRegister.Count);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RM, payload: new object[] { 3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(5, executionState.StackRegister.Count);
			Assert.Equal(newStackRegister, executionState.StackRegister);
			Assert.Empty(executionState.StackPointers);
		}

		#endregion

		#region InstructionCR

		[Fact]
		public void InstructionCR_Execute()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			List<ValuePointer<G>> pointers = new List<ValuePointer<G>>(Enumerable.Range(0, 5).Select(i => new ValuePointer<G>() { Value = new DefaultIntValue<G>(i) }));
			executionState.StackRegister.AddRange(pointers.ToArray(), pointers.Count);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackRegister);
		}

		#endregion

		#region InstructionCS

		[Fact]
		public void InstructionCS_Execute()
		{
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CS);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
		}

		#endregion

		#region InstructionCSE

		[Fact]
		public void InstructionCSE_Execute()
		{
			int expectedInstructionIndex = 20;
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CSE, payload: new object[] { expectedInstructionIndex + 1 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackRegister);
			Assert.Empty(executionState.StackPointers);
			Assert.Equal(expectedInstructionIndex, executionState.InstructionIndex);
		}

		#endregion

		#region InstructionCSP

		[Fact]
		public void InstructionCSP_Execute()
		{
			const string StackPointerName = "pointername";
			const int CurrentInstructionIndex = 20;
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>()) { InstructionIndex = CurrentInstructionIndex };
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>()));

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CSP, payload: new object[] { StackPointerName });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackRegister);
			Assert.Single(executionState.StackPointers);
			Assert.Equal(executionState.StackPointers[0], executionState.StackRegister[0]);
			Assert.Equal(StackPointerName, ((StackValuePointer<G>)executionState.StackRegister[0]).Identifier);
			Assert.Equal(CurrentInstructionIndex, ((StackValuePointer<G>)executionState.StackRegister[0]).Origin);
		}

		#endregion

		#region InstructionDPCDP

		[Fact]
		public void InstructionDPCDP_Execute()
		{
			const string PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			const string PayloadItem4 = "fourth";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem1, value) { Identifier = PayloadItem2 };
			DeclaredValuePointer<G> createdValuePointer = new DeclaredValuePointer<G>(PayloadItem3) { IsDeclared = true, Identifier = PayloadItem4 };
			G groupState = Mock.Of<G>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = 10 };
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m =>
				m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider) == dynamicPointer &&
				m(executionState, groupState, PayloadItem3, PayloadItem4, PointerOperation.Add, valueProvider) == createdValuePointer
			);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.DPCDP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3, PayloadItem4 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackRegister);
			Assert.Equal(createdValuePointer, executionState.StackRegister[0]);
			Assert.Equal(value, executionState.StackRegister[0].Value);
			Assert.Empty(executionState.StackPointers);
			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider), Times.Once);
			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem3, PayloadItem4, PointerOperation.Add, valueProvider), Times.Once);
		}

		#endregion

		#region InstructionDPCGP

		[Fact]
		public void InstructionDPCGP_Execute()
		{
			const int InstructionIndex = 10;
			const string PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			Dictionary<string, int> pointerMap = new Dictionary<string, int>();
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem1, value) { Identifier = PayloadItem2 };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>();
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers && m.PointerMap == pointerMap);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = InstructionIndex };
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>());
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider) == dynamicPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.DPCGP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider), Times.Once);
			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Single(groupPointers);
			Assert.Equal(groupPointers[0], executionState.StackRegister[0]);
			Assert.Equal(PayloadItem3, groupPointers[0].Identifier);
			Assert.Equal(value, groupPointers[0].Value);
		}

		#endregion

		#region InstructionDPCSP

		[Fact]
		public void InstructionDPCSP_Execute()
		{
			const int InstructionIndex = 10;
			const string PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem1, value) { Identifier = PayloadItem2 };
			G groupState = Mock.Of<G>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>());
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = InstructionIndex };
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider) == dynamicPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.DPCSP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider), Times.Once);
			Assert.Single(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			StackValuePointer<G> stackPointer = executionState.StackPointers[0];
			Assert.Equal(stackPointer, executionState.StackRegister[0]);
			Assert.Equal(PayloadItem3, stackPointer.Identifier);
			Assert.Equal(InstructionIndex, stackPointer.Origin);
			Assert.Equal(value, stackPointer.Value);
		}

		#endregion

		#region InstructionDPDP

		[Fact]
		public void InstructionDPDP_Execute()
		{
			const string PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			const string PayloadItem4 = "fourth";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem1, value) { Identifier = PayloadItem2 };
			DeclaredValuePointer<G> otherDynamicPointer = new DeclaredValuePointer<G>(PayloadItem3, new DefaultNullValue<G>()) { Identifier = PayloadItem4 };
			G groupState = Mock.Of<G>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = 10 };
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m =>
				m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider) == dynamicPointer &&
				m(executionState, groupState, PayloadItem3, PayloadItem4, PointerOperation.Get, valueProvider) == otherDynamicPointer
			);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.DPDP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3, PayloadItem4 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackRegister);
			Assert.Equal(otherDynamicPointer, executionState.StackRegister[0]);
			Assert.Equal(value, executionState.StackRegister[0].Value);
			Assert.Empty(executionState.StackPointers);
			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider), Times.Once);
			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem3, PayloadItem4, PointerOperation.Get, valueProvider), Times.Once);
		}

		#endregion

		#region InstructionDPGP

		[Fact]
		public void InstructionDPGP_Execute()
		{
			const string PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			const int PayloadItem3 = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem1, value) { Identifier = PayloadItem2 };
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", new DefaultNullValue<G>()) { Identifier = "grouppointername" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>());
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider) == dynamicPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.DPGP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider), Times.Once);
			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(3, groupPointers.Count);
			Assert.Equal(groupPointer, executionState.StackRegister[0]);
			Assert.Equal(value, groupPointer.Value);
		}

		#endregion

		#region InstructionDPR

		[Fact]
		public void InstructionDPR_Execute()
		{
			const string PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem1) { Identifier = PayloadItem2 };
			G groupState = Mock.Of<G>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider) == dynamicPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.DPR, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackRegister);
			Assert.Equal(dynamicPointer, executionState.StackRegister[0]);
			Assert.Empty(executionState.StackPointers);
			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider), Times.Once);
		}

		#endregion

		#region InstructionDPSP

		[Fact]
		public void InstructionDPSP_Execute()
		{
			const string PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			const int PayloadItem3 = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem1, value) { Identifier = PayloadItem2 };
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(new DefaultNullValue<G>()) { Identifier = "stackpointername" };
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>());
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider) == dynamicPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.DPSP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem1, PayloadItem2, PointerOperation.Get, valueProvider), Times.Once);
			Assert.Equal(3, executionState.StackPointers.Count);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(stackPointer, executionState.StackRegister[0]);
			Assert.Equal(value, stackPointer.Value);
		}

		#endregion

		#region InstructionG

		[Fact]
		public void InstructionG_Execute()
		{
			string groupName = "groupname";
			Instruction<G>[] originalInstructions = new Instruction<G>[]
			{
				TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, new object[] { 0 }, null, false, null),
				TestInstructionProvider<G>.GetInstruction(InstructionCode.G, null, null, false, null),
				TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, new object[] { 1 }, null, false, null),
				TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, new object[] { 2 }, null, false, null),
				TestInstructionProvider<G>.GetInstruction(InstructionCode.GE, null, null, false, null),
				TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, new object[] { 3 }, null, false, null),
				TestInstructionProvider<G>.GetInstruction(InstructionCode.NON, new object[] { 4 }, null, false, null),
			};
			Group<G> group = new Group<G> { Instructions = originalInstructions };
			G originalGroupState = Mock.Of<G>(m => m.Group == group);
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m => m.GetGroup(It.IsAny<G>()) == groupValue);
			ExecutionState<G> executionState = new ExecutionState<G>(originalGroupState)
			{
				InstructionIndex = 1
			};
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.G, payload: new object[] { groupName });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(2, executionState.Frames.Count);
			Assert.Equal(4, executionState.Frames[0].PreviousInstructionIndex);
			Assert.Equal(groupName, executionState.GroupState.Group.GroupName);
			Assert.Equal(originalInstructions.Skip(2).Take(2).Append(InstructionProvider<G>.GetInstruction(InstructionCode.IFE)), executionState.GroupState.Group.Instructions);
		}

		#endregion

		#region InstructionGDR

		[Fact]
		public void InstructionGDR_Execute()
		{
			int location = 3;
			G otherGroupState = Mock.Of<G>();
			List<G> dependencies = new List<G> { Mock.Of<G>(), Mock.Of<G>(), Mock.Of<G>(), otherGroupState, Mock.Of<G>() };
			G groupState = Mock.Of<G>(m => m.Dependencies == dependencies);
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			Mock.Get(valueProvider).Setup(m => m.GetGroup(otherGroupState)).Returns(groupValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GDR, payload: new object[] { location });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupValue, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionGE

		[Fact]
		public void InstructionGE_Execute()
		{
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GE);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
		}

		#endregion

		#region InstructionGPCDP

		[Fact]
		public void InstructionGPCDP_Execute()
		{
			const int PayloadItem1 = 1;
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", value) { Identifier = "grouppointer" };
			DeclaredValuePointer<G> createdValuePointer = new DeclaredValuePointer<G>(PayloadItem2) { IsDeclared = true, Identifier = PayloadItem3 };
			G groupState = Mock.Of<G>(m => m.GroupPointers == new List<DeclaredValuePointer<G>> { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = 10 };
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m =>
				m(executionState, groupState, It.IsAny<string>(), It.IsAny<string>(), PointerOperation.Add, valueProvider) == createdValuePointer
			);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GPCDP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackRegister);
			Assert.Equal(createdValuePointer, executionState.StackRegister[0]);
			Assert.Equal(value, createdValuePointer.Value);
			Assert.Empty(executionState.StackPointers);
			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem2, PayloadItem3, PointerOperation.Add, valueProvider), Times.Once);
		}

		#endregion

		#region InstructionGPCGP

		[Fact]
		public void InstructionGPCGP_Execute()
		{
			const int InstructionIndex = 10;
			const int PayloadItem1 = 1;
			const string PayloadItem2 = "second";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", value) { Identifier = "grouppointer" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>> { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = InstructionIndex };
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>()));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GPCGP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(4, groupPointers.Count);
			Assert.Equal(groupPointers[3], executionState.StackRegister[0]);
			Assert.Equal(PayloadItem2, groupPointers[3].Identifier);
			Assert.Equal(value, groupPointers[3].Value);
		}

		#endregion

		#region InstructionGPCSP

		[Fact]
		public void InstructionGPCSP_Execute()
		{
			const int InstructionIndex = 10;
			const int PayloadItem1 = 1;
			const string PayloadItem2 = "second";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", value) { Identifier = "grouppointer" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>> { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = InstructionIndex };
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>()));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GPCSP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			StackValuePointer<G> stackPointer = executionState.StackPointers[0];
			Assert.Equal(stackPointer, executionState.StackRegister[0]);
			Assert.Equal(PayloadItem2, stackPointer.Identifier);
			Assert.Equal(InstructionIndex, stackPointer.Origin);
			Assert.Equal(value, stackPointer.Value);
		}

		#endregion

		#region InstructionGPDP

		[Fact]
		public void InstructionGPDP_Execute()
		{
			const int PayloadItem1 = 1;
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", value) { Identifier = "grouppointername" };
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem2, new DefaultNullValue<G>()) { Identifier = PayloadItem3 };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>());
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem2, PayloadItem3, PointerOperation.Get, valueProvider) == dynamicPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GPDP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem2, PayloadItem3, PointerOperation.Get, valueProvider), Times.Once);
			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(3, groupPointers.Count);
			Assert.Equal(dynamicPointer, executionState.StackRegister[0]);
			Assert.Equal(value, dynamicPointer.Value);
		}

		#endregion

		#region InstructionGPGP

		[Fact]
		public void InstructionGPGP_Execute()
		{
			const int PayloadItem1 = 1;
			const int PayloadItem2 = 2;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", value) { Identifier = "grouppointername" };
			DeclaredValuePointer<G> targetGroupPointer = new DeclaredValuePointer<G>("group", new DefaultNullValue<G>()) { Identifier = "grouppointername2" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), groupPointer, targetGroupPointer };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>()));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GPGP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(3, groupPointers.Count);
			Assert.Equal(targetGroupPointer, executionState.StackRegister[0]);
			Assert.Equal(value, targetGroupPointer.Value);
		}

		#endregion

		#region InstructionGPR

		[Fact]
		public void InstructionGPR_Execute()
		{
			const int location = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", value) { Identifier = "grouppointername" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GPR, payload: new object[] { location });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupPointer, executionState.StackRegister[0]);
		}

		#endregion

		#region InstructionGPSP

		[Fact]
		public void InstructionGPSP_Execute()
		{
			const int PayloadItem1 = 2;
			const int PayloadItem2 = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", value) { Identifier = "grouppointername" };
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(new DefaultNullValue<G>()) { Identifier = "stackpointername" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), new DeclaredValuePointer<G>("group"), groupPointer };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>()));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GPSP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(3, executionState.StackPointers.Count);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(stackPointer, executionState.StackRegister[0]);
			Assert.Equal(value, stackPointer.Value);
		}

		#endregion

		#region InstructionGR

		[Fact]
		public void InstructionGR_Execute()
		{
			G groupState = Mock.Of<G>();
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			Mock.Get(valueProvider).Setup(m => m.GetGroup(groupState)).Returns(groupValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupValue, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionH

		[Fact]
		public void InstructionH_Execute()
		{
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>()) { Executable = true };

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.H);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
			Assert.False(executionState.Executable);
		}

		#endregion

		#region InstructionHRG

		[Theory]
		[InlineData("payloadgroupname")]
		[InlineData(null)]
		public void InstructionHRG_Execute(string payloadGroupName)
		{
			string groupName = "groupname";
			G groupState = Mock.Of<G>(m => m.Group == new Group<G> { GroupName = groupName });
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>(m => m.State == groupState);
			Dictionary<string, G> groupHost = new Dictionary<string, G>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.Groups == groupHost);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RGH, payload: new object[] { payloadGroupName });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupValue, executionState.StackRegister[0].Value);
			Assert.True(groupHost.ContainsKey(payloadGroupName ?? groupName));
			Assert.Equal(groupState, groupHost[payloadGroupName ?? groupName]);
			Assert.Equal(groupValue, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionIFE

		[Fact]
		public void InstructionIFE_Execute()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>())
			{
				Executable = true
			};
			Frame<G>[] frames = new Frame<G>[]
			{
				new Frame<G>()
			{
				GroupState = Mock.Of<G>(),
				PreviousInstructionIndex = 20
			},
				new Frame<G>()
			{
				GroupState = Mock.Of<G>(),
				PreviousInstructionIndex = 25,
			}
			};
			executionState.Frames.AddRange(frames, 2);
			executionState.Sync(frames[1]);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.IFE);
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == new DefaultValueProvider<G>()), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(2, executionState.Frames.Count);
			Assert.Equal(executionState.Frames.Last(), executionState.LastFrame);
			Assert.Equal(frames[0], executionState.LastFrame);
			Assert.Equal(frames[0].StackPointers, executionState.StackPointers);
			Assert.Equal(frames[0].StackRegister, executionState.StackRegister);
			Assert.Equal(frames[0].PreviousInstructionIndex, executionState.InstructionIndex);
			Assert.Equal(frames[0].GroupState, executionState.GroupState);
			Assert.Single(frames[0].StackRegister);
			Assert.Equal(frames[1].GroupState, ((IGroupValue<G>)frames[0].StackRegister.Last().Value).State);
			Assert.True(executionState.Executable);
		}

		#endregion

		#region InstructionJ

		[Fact]
		public void InstructionJ_Execute()
		{
			int expectedInstructionIndex = 9;
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.J, new object[] { expectedInstructionIndex + 1 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
			Assert.Equal(expectedInstructionIndex, executionState.InstructionIndex);
		}

		#endregion

		#region InstructionL

		[Fact]
		public void InstructionL_Execute()
		{
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.L);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
		}

		#endregion

		#region InstructionLE

		[Fact]
		public void InstructionLE_Execute()
		{
			int expectedInstructionIndex = 9;
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.LE, new object[] { expectedInstructionIndex + 1 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
			Assert.Equal(expectedInstructionIndex, executionState.InstructionIndex);
		}

		#endregion

		#region InstructionLRAS

		[Fact]
		public void InstructionLRAS_Execute()
		{
			G groupState = Mock.Of<G>();
			List<StackValuePointer<G>> initStackPointer = new List<StackValuePointer<G>>();
			IActionValue<G> actionValue = Mock.Of<IActionValue<G>>(m => m.InitStackPointers == initStackPointer);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = 20 };
			executionState.StackRegister.Add(new ValuePointer<G> { Value = actionValue });
			List<ValuePointer<G>> originalListRegister = new List<ValuePointer<G>>
			{
				new ValuePointer<G> { Value = new DefaultIntValue<G>(10) },
				new StackValuePointer<G>(new DefaultIntValue<G>(20)),
				new DeclaredValuePointer<G>("local", new DefaultIntValue<G>(30))
			};
			executionState.ListRegister.AddRange(originalListRegister);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.LRAS, new object[] { 3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(actionValue, executionState.StackRegister[0].Value);
			Assert.Empty(executionState.ListRegister);
			Assert.Equal(originalListRegister.Count, initStackPointer.Count);
			for (int i = 0; i < initStackPointer.Count; i++)
			{
				Assert.Same(originalListRegister[i].Value, initStackPointer[i].Value);
				Assert.Equal(executionState.InstructionIndex, initStackPointer[i].Origin);
			}
		}

		#endregion

		#region InstructionNC

		[Theory]
		[InlineData(true, 20)]
		[InlineData(false, 10)]
		public void InstructionNC_Execute(bool condition, int expectedInstructionIndex)
		{
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = 20 };
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultBooleanValue<G>(condition) });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.NC, payload: new object[] { 11 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
			Assert.Equal(expectedInstructionIndex, executionState.InstructionIndex);
		}

		#endregion

		#region InstructionOA

		[Fact]
		public void InstructionOA_Execute()
		{
			int location = 10;
			Dictionary<int, IValuable<G>> actionOverrides = new Dictionary<int, IValuable<G>>();
			G groupState = Mock.Of<G>(m => m.ActionOverrides == actionOverrides);
			IValuable<G> valuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>(m => m.State == groupState);
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = valuable });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.OA, new object[] { location });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupValue, executionState.StackRegister[0].Value);
			Assert.Single(actionOverrides);
			Assert.Equal(valuable, actionOverrides[location]);
		}

		#endregion

		#region InstructionPHR

		[Fact]
		public void InstructionPHR_Execute()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.PHR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Null(executionState.StackRegister[0]);
		}

		#endregion

		#region InstructionPLR

		[Fact]
		public void InstructionPLR_Execute()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(null);
			executionState.StackRegister.Add(null);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.PLR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Null(executionState.StackRegister[0]);
		}

		#endregion

		#region InstructionMI

		[Fact]
		public void InstructionMI_Execute()
		{
			string name = "name";
			int location = 10;
			Group<G> group = new Group<G>();
			G groupState = Mock.Of<G>(m => m.Group == group);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MI, payload: new object[] { name, location });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
			Assert.True(group.InstructionMap.ContainsKey(name));
			Assert.Equal(location, group.InstructionMap[name]);
		}
		#endregion

		#region InstructionMP

		[Fact]
		public void InstructionMP_Execute()
		{
			string name = "name";
			int location = 10;
			Dictionary<string, int> pointerMap = new Dictionary<string, int>();
			G groupState = Mock.Of<G>(m => m.PointerMap == pointerMap);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MP, payload: new object[] { name, location });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
			Assert.True(pointerMap.ContainsKey(name));
			Assert.Equal(location, pointerMap[name]);
		}
		#endregion

		#region InstructionMPR

		[Fact]
		public void InstructionMPR_Execute_CurrentGroupState()
		{
			int location = 1;
			string pointerName = "pointername";
			Dictionary<string, int> pointerMap = new Dictionary<string, int> { { pointerName, location } };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), new DeclaredValuePointer<G>("group"), new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers && m.PointerMap == pointerMap);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultStringValue<G>(pointerName) });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MPR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupPointers[location], executionState.StackRegister[0]);
		}

		[Fact]
		public void InstructionMPR_Execute_RegisterGroupState()
		{
			int location = 1;
			string pointerName = "pointername";
			Dictionary<string, int> pointerMap = new Dictionary<string, int> { { pointerName, location } };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), new DeclaredValuePointer<G>("group"), new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers && m.PointerMap == pointerMap);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultGroupValue<G>(groupState) });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultStringValue<G>(pointerName) });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MPR, new object[] { true });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupPointers[location], executionState.StackRegister[0]);
		}

		#endregion

		#region InstructionMIR

		[Fact]
		public void InstructionMIR_Execute_CurrentGroupState()
		{
			int location = 10;
			string name = "name";
			IValue<G, int> locationIntValue = Mock.Of<IValue<G, int>>(m => m.Data == location);
			Group<G> group = new Group<G>();
			group.InstructionMap[name] = location;
			G groupState = Mock.Of<G>(m => m.Group == group);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultStringValue<G>(name) });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			Mock.Get(valueProvider).Setup(m => m.GetInt(location)).Returns(locationIntValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MIR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(locationIntValue, executionState.StackRegister[0].Value);
		}

		[Fact]
		public void InstructionMIR_Execute_RegisterGroupState()
		{
			int location = 10;
			string name = "name";
			IValue<G, int> locationIntValue = Mock.Of<IValue<G, int>>(m => m.Data == location);
			Group<G> group = new Group<G>();
			group.InstructionMap[name] = location;
			G groupState = Mock.Of<G>(m => m.Group == group);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultGroupValue<G>(groupState) });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultStringValue<G>(name) });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			Mock.Get(valueProvider).Setup(m => m.GetInt(location)).Returns(locationIntValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MIR, new object[] { true });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(locationIntValue, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRAnd

		[Fact]
		public void InstructionRAnd_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.And(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RAnd);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionAR

		[Theory]
		[InlineData("name")]
		[InlineData(null)]
		public void InstructionAR_Execute(string identifier)
		{
			int location = 10;
			IActionValue<G> actionValue = Mock.Of<IActionValue<G>>();
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			Mock.Get(valueProvider).Setup(m => m.GetAction(groupState, location)).Returns(actionValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.AR, payload: new object[] { location, identifier });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(actionValue, executionState.StackRegister[0].Value);
			Mock.Get(actionValue).VerifySet(m => m.Identifier = identifier, Times.Once);
		}

		#endregion

		#region InstructionRAR

		[Theory]
		[InlineData("name")]
		[InlineData(null)]
		public void InstructionRAR_Execute(string identifier)
		{
			int location = 10;
			IValue<G, int> locationIntValue = Mock.Of<IValue<G, int>>(m => m.Data == location && m.GetData() == (object)location);
			IActionValue<G> actionValue = Mock.Of<IActionValue<G>>();
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackRegister.Add(new ValuePointer<G> { Value = locationIntValue });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			Mock.Get(valueProvider).Setup(m => m.GetAction(groupState, location)).Returns(actionValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RAR, payload: new object[] { identifier });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(actionValue, executionState.StackRegister[0].Value);
			Mock.Get(actionValue).VerifySet(m => m.Identifier = identifier, Times.Once);
		}

		#endregion

		#region InstructionRCE

		[Fact]
		public void InstructionRCE_Execute_IsExecutable()
		{
			int instructionIndex = 20;
			IValuable<G> executableValuable = Mock.Of<IValuable<G>>();
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState)
			{
				InstructionIndex = instructionIndex,
			};
			executionState.StackRegister.Add(new ValuePointer<G> { Value = executableValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RCE);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
			Assert.Equal(executionState.LastFrame.PreviousInstructionIndex, executionState.InstructionIndex);
			Mock.Get(executableValuable).Verify(m => m.Execute(executionState, valueProvider), Times.Once);
		}

		[Fact]
		public void InstructionRCE_Execute_ThrowsIfNotExecutable()
		{
			int instructionIndex = 20;
			IValuable<G> executableValuable = Mock.Of<IValuable<G>>();
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState)
			{
				InstructionIndex = instructionIndex
			};
			executionState.StackRegister.Add(new DeclaredValuePointer<G>("group", executableValuable) { Modifiers = Modifiers.ExecuteRestricted });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RCE);
			EngineRuntimeException exception = Assert.Throws<EngineRuntimeException>(() => sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers));

			Assert.Equal("Execution is not allowed", exception.Message);
		}

		#endregion

		#region InstructionRDivide

		[Fact]
		public void InstructionRDivide_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.DivideBy(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RDivide);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionREquals

		[Fact]
		public void InstructionREquals_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.IsEqualTo(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.REquals);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRGAR

		[Theory]
		[InlineData("name")]
		[InlineData(null)]
		public void InstructionRGAR_Execute(string identifier)
		{
			int location = 10;
			IValue<G, int> locationIntValue = Mock.Of<IValue<G, int>>(m => m.Data == location && m.GetData() == (object)location);
			IActionValue<G> actionValue = Mock.Of<IActionValue<G>>();
			G groupState = Mock.Of<G>();
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>(m => m.State == groupState);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = locationIntValue });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			Mock.Get(valueProvider).Setup(m => m.GetAction(groupState, location)).Returns(actionValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RGAR, payload: new object[] { identifier });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(actionValue, executionState.StackRegister[0].Value);
			Mock.Get(actionValue).VerifySet(m => m.Identifier = identifier, Times.Once);
		}

		#endregion

		#region InstructionRGD

		[Fact]
		public void InstructionRGD_Execute()
		{
			G otherGroupState = Mock.Of<G>();
			List<G> dependencies = new List<G> { Mock.Of<G>(), Mock.Of<G>(), Mock.Of<G>(), Mock.Of<G>() };
			G groupState = Mock.Of<G>(m => m.Dependencies == dependencies);
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>(m => m.State == groupState);
			IGroupValue<G> otherGroupValue = Mock.Of<IGroupValue<G>>(m => m.State == otherGroupState);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = otherGroupValue });

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RGD);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupValue, executionState.StackRegister[0].Value);
			Assert.Equal(5, dependencies.Count);
			Assert.Equal(otherGroupState, dependencies[4]);
		}

		#endregion

		#region InstructionRGDPR

		[Fact]
		public void InstructionRGDPR_Execute()
		{
			const string location = "first";
			const string name = "second";
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(location) { Identifier = name };
			G groupState = Mock.Of<G>();
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>(m => m.State == groupState);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m =>
				m(executionState, groupState, location, name, PointerOperation.Get, valueProvider) == dynamicPointer
			);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RGDPR, payload: new object[] { location, name });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackRegister);
			Assert.Equal(dynamicPointer, executionState.StackRegister[0]);
			Assert.Empty(executionState.StackPointers);
			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, location, name, PointerOperation.Get, valueProvider), Times.Once);
		}

		#endregion

		#region InstructionRGGPR

		[Fact]
		public void InstructionRGGPR_Execute()
		{
			const int location = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", value) { Identifier = "grouppointername" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>(m => m.State == groupState);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RGGPR, payload: new object[] { location });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupPointer, executionState.StackRegister[0]);
		}

		#endregion

		#region InstructionRGreaterThan

		[Fact]
		public void InstructionRGreaterThan_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.IsGreaterThan(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RGreaterThan);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRGreaterThanOrEquals

		[Fact]
		public void InstructionRGreaterThanOrEquals_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.IsGreaterThanOrEqualTo(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RGreaterThanOrEquals);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRLeftShift

		[Fact]
		public void InstructionRLeftShift_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.LeftShift(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RLeftShift);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRLessThan

		[Fact]
		public void InstructionRLessThan_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.IsLessThan(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RLessThan);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRLessThanOrEquals

		[Fact]
		public void InstructionRLessThanOrEquals_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.IsLessThanOrEqualTo(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RLessThanOrEquals);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRLR
		[Theory]
		[InlineData(4)]
		[InlineData(0)]
		[InlineData(6)]
		public void InstructionRLR_Execute(int count)
		{
			ValuePointer<G>[] stackRegisterPointers = new[] { new ValuePointer<G>(), new ValuePointer<G>(), new ValuePointer<G>(), new ValuePointer<G>(), new ValuePointer<G>(), new ValuePointer<G>() };
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.AddRange(stackRegisterPointers, stackRegisterPointers.Length);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RLR, new object[] { count });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(stackRegisterPointers.Take(stackRegisterPointers.Length - count), executionState.StackRegister);
			Assert.Empty(executionState.StackPointers);
			Assert.Equal(stackRegisterPointers.Skip(stackRegisterPointers.Length - count), executionState.ListRegister);
		}
		#endregion

		#region InstructionLRR
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void InstructionLRR_Execute_ParamsCountSatisfied(bool copyReference)
		{
			const int ParameterCount = 2;
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			ValuePointer<G>[] paramPointers = new[]
			{
				new ValuePointer<G>
				{
					Value = new DefaultIntValue<G>(1)
				},
				new ValuePointer<G>
				{
					Value = new DefaultIntValue<G>(2)
				}
			};

			executionState.ListRegister.AddRange(paramPointers);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.LRR, new object[] { ParameterCount, copyReference });
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.ListRegister);
			Assert.Equal(2, executionState.StackRegister.Count);
			Assert.Empty(executionState.StackPointers);
			if (copyReference)
			{
				Assert.Equal(paramPointers, executionState.StackRegister);
			}
			else
			{
				Assert.Equal(paramPointers.Select(p => p.Value), executionState.StackRegister.Select(p => p.Value));
			}
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void InstructionLRR_Execute_ParamsCountUnsatisfied(bool copyReference)
		{
			const int ParameterCount = 2;
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			ValuePointer<G>[] paramPointers = new[]
			{
				new ValuePointer<G>
				{
					Value = new DefaultIntValue<G>(1)
				}
			};

			executionState.ListRegister.AddRange(paramPointers);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.LRR, new object[] { ParameterCount, copyReference });
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.ListRegister);
			Assert.Equal(2, executionState.StackRegister.Count);
			Assert.Empty(executionState.StackPointers);
			if (copyReference)
			{
				Assert.Equal(paramPointers.Append(null), executionState.StackRegister);
			}
			else
			{
				Assert.Equal(paramPointers.Select(p => p.Value).Append(null), executionState.StackRegister.Select(p => p?.Value));
			}
		}
		#endregion

		#region InstructionRMultiply

		[Fact]
		public void InstructionRMultiply_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.MultiplyBy(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RMultiply);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRNot

		[Fact]
		public void InstructionRNot_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.Not(valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RNot);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRNotEquals

		[Fact]
		public void InstructionRNotEquals_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.IsNotEqualTo(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RNotEquals);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionROr

		[Fact]
		public void InstructionROr_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.Or(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.ROr);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRPlus

		[Fact]
		public void InstructionRPlus_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.Add(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RPlus);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRR

		[Fact]
		public void InstructionRR_Execute()
		{
			IValuable<G> secondValue = Mock.Of<IValuable<G>>();
			ValuePointer<G> firstPointer = new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() };
			ValuePointer<G> secondPointer = new ValuePointer<G> { Value = secondValue };
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(firstPointer);
			executionState.StackRegister.Add(secondPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(firstPointer, executionState.StackRegister[0]);
			Assert.Equal(secondValue, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRRemainder

		[Fact]
		public void InstructionRRemainder_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.Remainder(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RRemainder);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRRightShift

		[Fact]
		public void InstructionRRightShift_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.RightShift(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RRightShift);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRSubtract

		[Fact]
		public void InstructionRSubtract_Execute()
		{
			IValuable<G> firstValuable = Mock.Of<IValuable<G>>();
			IValuable<G> secondValuable = Mock.Of<IValuable<G>>();
			IValuable<G> returnedValuable = Mock.Of<IValuable<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = firstValuable });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = secondValuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(firstValuable).Setup(m => m.Minus(secondValuable, valueProvider)).Returns(returnedValuable);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RSubtract);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedValuable, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionRVI

		[Fact]
		public void InstructionRVI_Execute()
		{
			IValuable<G> indexableValue = Mock.Of<IValuable<G>>();
			IValuable<G> indexValue = Mock.Of<IValuable<G>>();
			ValuePointer<G> returnedPointer = new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() };
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = indexableValue });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = indexValue });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);
			Mock.Get(indexableValue).Setup(m => m.GetAt(indexValue, valueProvider)).Returns(returnedPointer);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RVI);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(returnedPointer, executionState.StackRegister[0]);
		}

		#endregion

		#region InstructionRVK

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void InstructionRVK_Execute(bool createKey)
		{
			DefaultIntValue<G> indexValue = new DefaultIntValue<G>(10);
			G groupState = Mock.Of<G>();
			G otherGroupState = Mock.Of<G>();
			DefaultGroupValue<G> groupValue = new DefaultGroupValue<G>(groupState);
			ICollectionValue<G> arrayValue = Mock.Of<ICollectionValue<G>>();
			IValueProvider<G> valuePointer = Mock.Of<IValueProvider<G>>();
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valuePointer);
			EntryValuePointer<G> entryPointer = new EntryValuePointer<G>(arrayValue, indexValue, 0, valuePointer, false);
			Mock.Get(arrayValue).Setup(m => m.Get(indexValue, createKey, valuePointer)).Returns(entryPointer);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = arrayValue });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = indexValue });

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RVK, payload: new object[] { createKey });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(arrayValue).Verify(m => m.Get(indexValue, createKey, valuePointer), Times.Once);
			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(entryPointer, executionState.StackRegister[0]);
		}

		#endregion

		#region InstructionRVS

		[Fact]
		public void InstructionRVS_Execute()
		{
			int size = 20;
			IValuable<G> valuable = Mock.Of<IValuable<G>>(m => m.Size == size);
			IValue<G, int> sizeValue = Mock.Of<IValue<G, int>>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = valuable });
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m => m.GetInt(size) == sizeValue);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RVS);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(sizeValue, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionSPCDP

		[Fact]
		public void InstructionSPCDP_Execute()
		{
			const int PayloadItem1 = 1;
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(value) { Identifier = "stackpointer" };
			DeclaredValuePointer<G> createdValuePointer = new DeclaredValuePointer<G>(PayloadItem2) { IsDeclared = true, Identifier = PayloadItem3 };
			G groupState = Mock.Of<G>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m =>
				m(executionState, groupState, It.IsAny<string>(), It.IsAny<string>(), PointerOperation.Add, valueProvider) == createdValuePointer
			);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.SPCDP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackRegister);
			Assert.Equal(createdValuePointer, executionState.StackRegister[0]);
			Assert.Equal(value, createdValuePointer.Value);
			Assert.Equal(3, executionState.StackPointers.Count);
			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem2, PayloadItem3, PointerOperation.Add, valueProvider), Times.Once);
		}

		#endregion

		#region InstructionSPCGP

		[Fact]
		public void InstructionSPCGP_Execute()
		{
			const int InstructionIndex = 10;
			const int PayloadItem1 = 1;
			const string PayloadItem2 = "second";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(value) { Identifier = "stackpointer" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>();
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = InstructionIndex };
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>()));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.SPCGP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(3, executionState.StackPointers.Count);
			Assert.Single(executionState.StackRegister);
			Assert.Single(groupPointers);
			Assert.Equal(groupPointers[0], executionState.StackRegister[0]);
			Assert.Equal(PayloadItem2, groupPointers[0].Identifier);
			Assert.Equal(value, groupPointers[0].Value);
		}

		#endregion

		#region InstructionSPCSP

		[Fact]
		public void InstructionSPCSP_Execute()
		{
			const int InstructionIndex = 10;
			const int PayloadItem1 = 2;
			const string PayloadItem2 = "second";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(value) { Identifier = "stackpointer" };
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = InstructionIndex };
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>()));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.SPCSP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(4, executionState.StackPointers.Count);
			Assert.Single(executionState.StackRegister);
			StackValuePointer<G> newPointer = executionState.StackPointers[3];
			Assert.Equal(newPointer, executionState.StackRegister[0]);
			Assert.Equal(PayloadItem2, newPointer.Identifier);
			Assert.Equal(InstructionIndex, newPointer.Origin);
			Assert.Equal(value, newPointer.Value);
		}

		#endregion

		#region InstructionSPDP

		[Fact]
		public void InstructionSPDP_Execute()
		{
			const int PayloadItem1 = 1;
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(value) { Identifier = "stackpointer" };
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem2, new DefaultNullValue<G>()) { Identifier = PayloadItem3 };
			G groupState = Mock.Of<G>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>());
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem2, PayloadItem3, PointerOperation.Get, valueProvider) == dynamicPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.SPDP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, PayloadItem2, PayloadItem3, PointerOperation.Get, valueProvider), Times.Once);
			Assert.Equal(3, executionState.StackPointers.Count);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(dynamicPointer, executionState.StackRegister[0]);
			Assert.Equal(value, dynamicPointer.Value);
		}

		#endregion

		#region InstructionSPGP

		[Fact]
		public void InstructionSPGP_Execute()
		{
			const int PayloadItem1 = 1;
			const int PayloadItem2 = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(value) { Identifier = "stackpointer" };
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", new DefaultNullValue<G>()) { Identifier = "grouppointername" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>()));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.SPGP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(3, executionState.StackPointers.Count);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(3, groupPointers.Count);
			Assert.Equal(groupPointer, executionState.StackRegister[0]);
			Assert.Equal(value, groupPointer.Value);
		}

		#endregion

		#region InstructionSPR

		[Fact]
		public void InstructionSPR_Execute()
		{
			const int PayloadItem1 = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(value) { Identifier = "stackpointer" };
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.SPR, payload: new object[] { PayloadItem1 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(3, executionState.StackPointers.Count);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(stackPointer, executionState.StackRegister[0]);
		}

		#endregion

		#region InstructionSPSP

		[Fact]
		public void InstructionSPSP_Execute()
		{
			const int PayloadItem1 = 2;
			const int PayloadItem2 = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(new DefaultNullValue<G>()) { Identifier = "stackpointername" };
			StackValuePointer<G> otherStackPointer = new StackValuePointer<G>(value) { Identifier = "otherstackpointername" };
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackPointers.Add(otherStackPointer);
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.SPSP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(3, executionState.StackPointers.Count);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(stackPointer, executionState.StackRegister[0]);
			Assert.Equal(value, stackPointer.Value);
		}

		#endregion

		#region InstructionURDP

		[Theory]
		[InlineData(true, 1)]
		[InlineData(false, 0)]
		public void InstructionURDP_Execute_OnDeclaredPointer(bool isDynamic, int dynamicDeleteCalls)
		{
			string location = "location";
			string identifier = "pointername";
			DeclaredValuePointer<G> pointer = new DeclaredValuePointer<G>(location, new DefaultNullValue<G>()) { Identifier = identifier, IsDynamic = isDynamic };
			G groupState = Mock.Of<G>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackRegister.Add(pointer);
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, location, identifier, PointerOperation.Delete, valueProvider) == pointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.URDP);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(externalDynamicPointers).Verify(m => m(executionState, groupState, location, identifier, PointerOperation.Delete, valueProvider), Times.Exactly(dynamicDeleteCalls));
			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
			Assert.False(pointer.IsDeclared);
		}

		[Fact]
		public void InstructionURDP_Execute_OnEntryPointer()
		{
			IValueProvider<G> valuePointer = Mock.Of<IValueProvider<G>>();
			IValuable<G> key = Mock.Of<IValuable<G>>();
			ICollectionValue<G> collection = Mock.Of<ICollectionValue<G>>();
			EntryValuePointer<G> pointer = new EntryValuePointer<G>(collection, key, 0, valuePointer);
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackRegister.Add(pointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.URDP);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Mock.Get(collection).Verify(m => m.Remove(key, valuePointer), Times.Once);
			Assert.Empty(executionState.StackPointers);
			Assert.Empty(executionState.StackRegister);
		}

		[Fact]
		public void InstructionURDP_Execute_OnNonDeclaredPointer()
		{
			ValuePointer<G> pointer = new ValuePointer<G>();
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackRegister.Add(pointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.URDP);
			EngineRuntimeException exception = Assert.Throws<EngineRuntimeException>(() => sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers));

			Assert.Equal("Cannot be undeclared.", exception.Message);
		}

		#endregion

		#region InstructionUS

		[Theory]
		[InlineData(3, 2)]
		[InlineData(0, 0)]
		[InlineData(7, 6)]
		public void InstructionUS_Execute(int origin, int remainingStack)
		{
			G groupState = Mock.Of<G>();
			StackValuePointer<G>[] stackables = new[]
			{
				Mock.Of<StackValuePointer<G>>(m => m.Origin == 1),
				Mock.Of<StackValuePointer<G>>(m => m.Origin == 2),
				Mock.Of<StackValuePointer<G>>(m => m.Origin == 3),
				Mock.Of<StackValuePointer<G>>(m => m.Origin == 4),
				Mock.Of<StackValuePointer<G>>(m => m.Origin == 5),
				Mock.Of<StackValuePointer<G>>(m => m.Origin == 6)
			};
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackPointers.AddRange(stackables, stackables.Length);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.US, payload: new object[] { origin });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(remainingStack, executionState.StackPointers.Count);
			Assert.Empty(executionState.StackRegister);
			Assert.Equal(stackables.Take(remainingStack), executionState.StackPointers);
		}

		#endregion

		#region InstructionVCDP

		[Fact]
		public void InstructionVCDP_Execute()
		{
			object PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			IValue<G> value = Mock.Of<IValue<G>>();
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem2, new DefaultNullValue<G>()) { Identifier = PayloadItem3 };
			G groupState = Mock.Of<G>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m2 => m2.GetAsValue(PayloadItem1) == value);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem2, PayloadItem3, PointerOperation.Add, valueProvider) == dynamicPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.VCDP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(dynamicPointer, executionState.StackRegister[0]);
			Assert.Equal(value, dynamicPointer.Value);
		}

		#endregion

		#region InstructionVCGP

		[Fact]
		public void InstructionVCGP_Execute()
		{
			const int InstructionIndex = 10;
			object PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>();
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = InstructionIndex };
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>() && m2.GetAsValue(PayloadItem1) == (IValue<G>)value));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.VCGP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Single(groupPointers);
			Assert.Equal(groupPointers[0], executionState.StackRegister[0]);
			Assert.Equal(PayloadItem2, groupPointers[0].Identifier);
			Assert.Equal(value, groupPointers[0].Value);
		}

		#endregion

		#region InstructionVCSP

		[Fact]
		public void InstructionVCSP_Execute()
		{
			const int InstructionIndex = 10;
			object PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState) { InstructionIndex = InstructionIndex };
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.Null == (INullValue<G>)new DefaultNullValue<G>() && m2.GetAsValue(PayloadItem1) == (IValue<G>)value));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.VCSP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Single(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			StackValuePointer<G> stackPointer = executionState.StackPointers[0];
			Assert.Equal(stackPointer, executionState.StackRegister[0]);
			Assert.Equal(PayloadItem2, stackPointer.Identifier);
			Assert.Equal(InstructionIndex, stackPointer.Origin);
			Assert.Equal(value, stackPointer.Value);
		}

		#endregion

		#region InstructionVDP

		[Fact]
		public void InstructionVDP_Execute()
		{
			object PayloadItem1 = "first";
			const string PayloadItem2 = "second";
			const string PayloadItem3 = "third";
			IValue<G> value = Mock.Of<IValue<G>>();
			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>(PayloadItem2, new DefaultNullValue<G>()) { Identifier = PayloadItem3 };
			G groupState = Mock.Of<G>();
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>(m2 => m2.GetAsValue(PayloadItem1) == value);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, PayloadItem2, PayloadItem3, PointerOperation.Get, valueProvider) == dynamicPointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.VDP, payload: new object[] { PayloadItem1, PayloadItem2, PayloadItem3 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(dynamicPointer, executionState.StackRegister[0]);
			Assert.Equal(value, dynamicPointer.Value);
		}

		#endregion

		#region InstructionVGP

		[Fact]
		public void InstructionVGP_Execute()
		{
			object PayloadItem1 = "first";
			const int PayloadItem2 = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", new DefaultNullValue<G>()) { Identifier = "grouppointername" };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.GetAsValue(PayloadItem1) == (IValue<G>)value));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.VGP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(3, groupPointers.Count);
			Assert.Equal(groupPointer, executionState.StackRegister[0]);
			Assert.Equal(value, groupPointer.Value);
		}

		#endregion

		#region InstructionVR

		[Fact]
		public void InstructionVR_Execute()
		{
			object PayloadItem1 = "first";
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.GetAsValue(PayloadItem1) == (IValue<G>)value));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.VR, payload: new object[] { PayloadItem1 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(value, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionVSP

		[Fact]
		public void InstructionVSP_Execute()
		{
			object PayloadItem1 = "first";
			const int PayloadItem2 = 1;
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(new DefaultNullValue<G>()) { Identifier = "stackpointername" };
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(Mock.Of<StackValuePointer<G>>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.GetAsValue(PayloadItem1) == (IValue<G>)value));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.VSP, payload: new object[] { PayloadItem1, PayloadItem2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(3, executionState.StackPointers.Count);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(stackPointer, executionState.StackRegister[0]);
			Assert.Equal(value, stackPointer.Value);
		}

		#endregion

		#region InstructionRPM

		[Fact]
		public void InstructionRPM_Execute()
		{
			Modifiers modifiers = Modifiers.Component | Modifiers.ExecuteRestricted;
			DefaultIntValue<G> value = new DefaultIntValue<G>((int)modifiers);
			G groupState = Mock.Of<G>();
			DeclaredValuePointer<G> pointer = new DeclaredValuePointer<G>("group");
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			executionState.StackRegister.Add(pointer);
			executionState.StackRegister.Add(new ValuePointer<G> { Value = value });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RPM);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(pointer, executionState.StackRegister[0]);
			Assert.Equal(modifiers, ((DeclaredValuePointer<G>)executionState.StackRegister[0]).Modifiers);
		}

		#endregion

		#region InstructionRGM

		[Fact]
		public void InstructionRGM_Execute()
		{
			Modifiers modifiers = Modifiers.Component | Modifiers.ExecuteRestricted;
			DefaultIntValue<G> value = new DefaultIntValue<G>((int)modifiers);
			G groupState = Mock.Of<G>();
			IGroupValue<G> groupValue = Mock.Of<IGroupValue<G>>(m => m.State == groupState);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = groupValue });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = value });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>();


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RGM);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupValue, executionState.StackRegister.First().Value);
			Assert.Equal(modifiers, groupState.Modifiers);
		}

		#endregion

		#region InstructionPMR

		[Fact]
		public void InstructionPMR_Execute()
		{
			Modifiers modifiers = Modifiers.Component | Modifiers.ExecuteRestricted;
			DefaultIntValue<G> value = new DefaultIntValue<G>((int)modifiers);
			DeclaredValuePointer<G> pointer = new DeclaredValuePointer<G>("group") { Modifiers = modifiers };
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(pointer);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.GetInt((int)modifiers) == (IValue<G>)value));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.PMR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(value, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionGMR

		[Fact]
		public void InstructionGMR_Execute()
		{
			Modifiers modifiers = Modifiers.Component | Modifiers.ExecuteRestricted;
			DefaultIntValue<G> value = new DefaultIntValue<G>((int)modifiers);
			G groupState = Mock.Of<G>(m => m.Modifiers == modifiers);
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G> { Value = Mock.Of<IGroupValue<G>>(m => m.State == groupState) });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == Mock.Of<IValueProvider<G>>(m2 => m2.GetInt((int)modifiers) == (IValue<G>)value));


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.GMR);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(value, executionState.StackRegister[0].Value);
		}

		#endregion

		#region InstructionCPR

		[Fact]
		public void InstructionCPR_Execute()
		{

			DeclaredValuePointer<G> dynamicPointer = new DeclaredValuePointer<G>("first");
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>()
			{
				null, null, new DeclaredValuePointer<G>("group", new DefaultNullValue<G>()), null
			};
			IValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(new DefaultNullValue<G>()) { Identifier = "stackpointer" };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, "first", "second", PointerOperation.Get, valueProvider) == dynamicPointer);
			executionState.StackPointers.Add(null);
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(null);
			executionState.StackRegister.Add(null);
			executionState.StackRegister.Add(null);
			executionState.StackRegister.Add(null);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.CPR, payload: new object[] { -2, 4, InstructionCode.SPR, 1, InstructionCode.DPR, "first", "second", InstructionCode.VR, 20, InstructionCode.GPR, 2 });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(3, executionState.StackPointers.Count);
			Assert.Equal(5, executionState.StackRegister.Count);
			Assert.Equal(stackPointer, executionState.StackRegister[1]);
			Assert.Equal(dynamicPointer, executionState.StackRegister[2]);
			Assert.Equal(new DefaultIntValue<G>(20), executionState.StackRegister[3].Value);
			Assert.Equal(groupPointers[2], executionState.StackRegister[4]);
		}

		#endregion

		#region InstructionRCP

		[Fact]
		public void InstructionRCP_Execute()
		{
			DeclaredValuePointer<G> dynamicPointer1 = new DeclaredValuePointer<G>("first");
			DeclaredValuePointer<G> dynamicPointer2 = new DeclaredValuePointer<G>("third");
			DefaultIntValue<G> value = new DefaultIntValue<G>(10);
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>()
			{
				null, null, new DeclaredValuePointer<G>("group", new DefaultNullValue<G>()), null
			};
			IValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			StackValuePointer<G> stackPointer = new StackValuePointer<G>(new DefaultNullValue<G>()) { Identifier = "stackpointer" };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>> externalDynamicPointers = Mock.Of<Func<ExecutionState<G>, G, string, string, PointerOperation, IValueProvider<G>, DeclaredValuePointer<G>>>(m => m(executionState, groupState, "first", "second", PointerOperation.Get, valueProvider) == dynamicPointer1 && m(executionState, groupState, "third", "fourth", PointerOperation.Add, valueProvider) == dynamicPointer2);

			executionState.StackPointers.Add(null);
			executionState.StackPointers.Add(stackPointer);
			executionState.StackPointers.Add(null);
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(10) });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(20) });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(30) });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(40) });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(50) });
			executionState.StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(60) });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ExternalDynamicPointers == externalDynamicPointers && m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.RCP, payload: new object[] { 6, InstructionCode.SPR, 1, InstructionCode.DPR, "first", "second", InstructionCode.GPR, 2, InstructionCode.CSP, "sp", InstructionCode.CGP, "gp", InstructionCode.CDP, "third", "fourth" });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(4, executionState.StackPointers.Count);
			Assert.Equal(5, groupPointers.Count);
			Assert.Empty(executionState.StackRegister);
			Assert.Equal(new DefaultIntValue<G>(10), stackPointer.Value);
			Assert.Equal(new DefaultIntValue<G>(20), dynamicPointer1.Value);
			Assert.Equal(new DefaultIntValue<G>(30), groupPointers[2].Value);
			Assert.Equal(new DefaultIntValue<G>(40), executionState.StackPointers[3].Value);
			Assert.Equal(new DefaultIntValue<G>(50), groupPointers[4].Value);
			Assert.Equal(new DefaultIntValue<G>(60), dynamicPointer2.Value);
		}

		#endregion

		#region InstructionHGR

		[Fact]
		public void InstructionHGR_Execute()
		{
			G groupState = Mock.Of<G>();
			G groupState2 = Mock.Of<G>();
			Dictionary<string, G> groups = new Dictionary<string, G> { { "name", groupState2 } };
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == new DefaultValueProvider<G>() && m.Groups == groups);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.HGR, payload: new object[] { "name" });
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(groupState2, ((IGroupValue<G>)executionState.StackRegister[0].Value).State);
		}

		#endregion

		#region InstructionID

		[Fact]
		public void InstructionID_Execute_UnsetEntryPointer()
		{
			IValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new EntryValuePointer<G>(null, null, 0, null, false));
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.ID);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(valueProvider.False, executionState.StackRegister.Last().Value);
		}

		[Fact]
		public void InstructionID_Execute_UndeclaredDeclaredValuePointer()
		{
			IValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new DeclaredValuePointer<G>("name") { IsDeclared = false });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.ID);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(valueProvider.False, executionState.StackRegister.Last().Value);
		}

		[Fact]
		public void InstructionID_Execute_SetEntryPointer()
		{
			IValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new EntryValuePointer<G>(null, null, 0, null, true));
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.ID);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(valueProvider.True, executionState.StackRegister.Last().Value);
		}

		[Fact]
		public void InstructionID_Execute_DeclaredDeclaredValuePointer()
		{
			IValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new DeclaredValuePointer<G>("name") { IsDeclared = true });
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.ID);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(valueProvider.True, executionState.StackRegister.Last().Value);
		}

		[Fact]
		public void InstructionID_Execute_ValuePointer()
		{
			IValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>());
			executionState.StackRegister.Add(new ValuePointer<G>());
			IInstructionExecutor<G> executor = Mock.Of<IInstructionExecutor<G>>(m => m.ValueProvider == valueProvider);


			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.ID);
			sut.Execute(executor, executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.StackPointers);
			Assert.Single(executionState.StackRegister);
			Assert.Equal(valueProvider.True, executionState.StackRegister.Last().Value);
		}

		#endregion

		#region InstructionMF

		[Fact]
		public void InstructionMF_Execute_MergeStackPointer()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>())
			{
				Executable = true
			};
			Frame<G>[] frames = new Frame<G>[]
			{
				new Frame<G>(),
				new Frame<G>()
			};
			frames[0].StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(10) });
			frames[0].StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(20) });
			frames[0].StackPointers.Add(new StackValuePointer<G> { Value = new DefaultIntValue<G>(30) });
			frames[0].StackPointers.Add(new StackValuePointer<G> { Value = new DefaultIntValue<G>(40) });
			executionState.Frames.AddRange(frames, 2);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MF, new object[] { 1, FrameMerge.Stack });
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(2, executionState.LastFrame.StackPointers.Count);
			Assert.Equal(frames[0].StackPointers.Select(pointer => pointer.Value), executionState.LastFrame.StackPointers.Select(pointer => pointer.Value));
			Assert.Empty(executionState.LastFrame.StackRegister);
		}

		[Fact]
		public void InstructionMF_Execute_MergeStackRegister()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>())
			{
				Executable = true
			};
			Frame<G>[] frames = new Frame<G>[]
			{
				new Frame<G>(),
				new Frame<G>()
			};
			frames[0].StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(10) });
			frames[0].StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(20) });
			frames[0].StackPointers.Add(new StackValuePointer<G> { Value = new DefaultIntValue<G>(30) });
			frames[0].StackPointers.Add(new StackValuePointer<G> { Value = new DefaultIntValue<G>(40) });
			executionState.Frames.AddRange(frames, 2);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MF, new object[] { 1, FrameMerge.Register });
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(2, executionState.LastFrame.StackRegister.Count);
			Assert.Equal(frames[0].StackRegister, executionState.LastFrame.StackRegister);
			Assert.Empty(executionState.LastFrame.StackPointers);
		}

		[Fact]
		public void InstructionMF_Execute_MergeAll()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>())
			{
				Executable = true
			};
			Frame<G>[] frames = new Frame<G>[]
			{
				new Frame<G>(),
				new Frame<G>()
			};
			frames[0].StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(10) });
			frames[0].StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(20) });
			frames[0].StackPointers.Add(new StackValuePointer<G> { Value = new DefaultIntValue<G>(30) });
			frames[0].StackPointers.Add(new StackValuePointer<G> { Value = new DefaultIntValue<G>(40) });
			executionState.Frames.AddRange(frames, 2);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MF, new object[] { 1, FrameMerge.Register | FrameMerge.Stack });
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Equal(frames[0].StackRegister, executionState.LastFrame.StackRegister);
			Assert.Equal(frames[0].StackPointers.Select(pointer => pointer.Value), executionState.LastFrame.StackPointers.Select(pointer => pointer.Value));
		}

		[Fact]
		public void InstructionMF_Execute_MergeNone()
		{
			ExecutionState<G> executionState = new ExecutionState<G>(Mock.Of<G>())
			{
				Executable = true
			};
			Frame<G>[] frames = new Frame<G>[]
			{
				new Frame<G>(),
				new Frame<G>()
			};
			frames[0].StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(10) });
			frames[0].StackRegister.Add(new ValuePointer<G> { Value = new DefaultIntValue<G>(20) });
			frames[0].StackPointers.Add(new StackValuePointer<G> { Value = new DefaultIntValue<G>(30) });
			frames[0].StackPointers.Add(new StackValuePointer<G> { Value = new DefaultIntValue<G>(40) });
			executionState.Frames.AddRange(frames, 2);

			Instruction<G> sut = InstructionProvider<G>.GetInstruction(InstructionCode.MF, new object[] { 1, FrameMerge.None });
			sut.Execute(Mock.Of<IInstructionExecutor<G>>(), executionState, executionState.StackRegister, executionState.StackPointers);

			Assert.Empty(executionState.LastFrame.StackPointers);
			Assert.Empty(executionState.LastFrame.StackRegister);
		}

		#endregion
	}
}
