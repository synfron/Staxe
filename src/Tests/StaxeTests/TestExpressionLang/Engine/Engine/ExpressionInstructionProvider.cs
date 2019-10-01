using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Collections;
using System;

namespace StaxeTests.TestExpressionLang.Engine.Engine
{
	public class ExpressionInstructionProvider : InstructionProvider<GroupState>
	{
		public ExpressionInstructionProvider()
		{
			SpecialInstructionMap["RFAC"] = ExecuteInstructionRFactorial;
			SpecialInstructionMap["RPOW"] = ExecuteInstructionRPower;
			SpecialInstructionMap["RSQRT"] = ExecuteInstructionRSqrt;
		}


		private static void ExecuteInstructionRPower(IInstructionExecutor<GroupState> executor, ExecutionState<GroupState> executionState, object[] payload, StackList<ValuePointer<GroupState>> stackRegister, StackList<StackValuePointer<GroupState>> stackPointers)
		{
			IValue<GroupState> secondValue = (IValue<GroupState>)stackRegister.TakeLast().Value;
			IValue<GroupState> firstValue = (IValue<GroupState>)stackRegister.Last().Value;
			IValuable<GroupState> result = executor.ValueProvider.GetReducedValue(Math.Pow(Convert.ToDouble(firstValue.GetData()), Convert.ToDouble(secondValue.GetData())));
			stackRegister.SetLast(new ValuePointer<GroupState> { Value = result });
		}

		private static void ExecuteInstructionRFactorial(IInstructionExecutor<GroupState> executor, ExecutionState<GroupState> executionState, object[] payload, StackList<ValuePointer<GroupState>> stackRegister, StackList<StackValuePointer<GroupState>> stackPointers)
		{
			IValue<GroupState> value = (IValue<GroupState>)stackRegister.Last().Value;
			int intVal = Convert.ToInt32(value.GetData());
			long resultVal = 1;
			for (int i = intVal; i > 0; i--)
			{
				resultVal *= i;
			}
			stackRegister.SetLast(new ValuePointer<GroupState> { Value = executor.ValueProvider.GetReducedValue(resultVal) });
		}

		private static void ExecuteInstructionRSqrt(IInstructionExecutor<GroupState> executor, ExecutionState<GroupState> executionState, object[] payload, StackList<ValuePointer<GroupState>> stackRegister, StackList<StackValuePointer<GroupState>> stackPointers)
		{
			IValue<GroupState> value = (IValue<GroupState>)stackRegister.Last().Value;
			double doubleVal = Math.Sqrt(Convert.ToDouble(value.GetData()));
			stackRegister.SetLast(new ValuePointer<GroupState> { Value = executor.ValueProvider.GetReducedValue(doubleVal) });
		}
	}
}
