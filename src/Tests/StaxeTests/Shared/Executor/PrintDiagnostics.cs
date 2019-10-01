using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Interrupts;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace StaxeTests.Shared.Executor
{
	public static class PrintDiagnostics
	{
		public static void EnableDiagnostics<G>(string code, IInstructionExecutor<G> instructionExecutor, ExecutionState<G> executionState, bool forceInterrupts = false) where G : IGroupState<G>, new()
		{
			if (instructionExecutor.Interrupts.Count == 0)
			{
				instructionExecutor.Interrupts.Add(new StepInterrupt<G>());
				instructionExecutor.Interrupted += (sender, args) => OnInterrupt(code, sender, args);
			}
			ScanInstructions(executionState.Instructions, forceInterrupts);
		}

		private static void ScanInstructions<G>(Instruction<G>[] instructions, bool forceInterrupts) where G : IGroupState<G>, new()
		{
			Debug.WriteLine("Instructions:");
			for (int i = 0; i < instructions.Length; i++)
			{
				Instruction<G> instruction = instructions[i];
				if (forceInterrupts && !instruction.Interruptable)
				{
					instruction = new Instruction<G>(instruction.Code, instruction.Payload, instruction.SourcePosition, true, instruction.ExecutionBody);
					instructions[i] = instruction;
				}
				Debug.WriteLine($"\t{i}.\t{instruction.Code} {string.Join(", ", instruction.Payload?.Select(item => ToValueString(item)) ?? Enumerable.Empty<string>())}");
			}
			Debug.WriteLine("\n\n");
		}

		private static void OnInterrupt<G>(string code, InstructionExecutor<G> _, InterruptedEventArgs<G> args) where G : IGroupState<G>, new()
		{
			PrintStackPointers(args.ExecutionState);
			PrintStackRegister(args.ExecutionState);
			PrintListRegister(args.ExecutionState);

			Instruction<G> instruction = args.ExecutionState.GroupState.Group.Instructions[args.ExecutionState.InstructionIndex];
			Debug.WriteLine($"\nInstruction: {args.ExecutionState.InstructionIndex}. {instruction.Code} {string.Join(", ", instruction.Payload?.Select(item => ToValueString(item)) ?? Enumerable.Empty<string>())} \n {(instruction.SourcePosition != null ? EngineUtils.GetLineAtPosition(code, instruction.SourcePosition.Value, 100) : null)}");
		}

		private static void PrintStackPointers<G>(ExecutionState<G> executionState) where G : IGroupState<G>, new()
		{
			int index = executionState.StackPointers.Count;
			StringBuilder builder = new StringBuilder();
			foreach (StackValuePointer<G> stackPointer in executionState.StackPointers)
			{
				builder.Append($"({--index}) {stackPointer?.Identifier} -> {(stackPointer == null ? "undefined" : ToJsonString(stackPointer.Value, 5))}; ");
			}
			Debug.WriteLine("\tStack: " + builder.ToString());
		}

		private static void PrintStackRegister<G>(ExecutionState<G> executionState) where G : IGroupState<G>, new()
		{
			StringBuilder builder = new StringBuilder();
			foreach (ValuePointer<G> pointer in executionState.StackRegister)
			{
				switch (pointer)
				{
					case DeclaredValuePointer<G> declaredPointer:
						{
							builder.Append($"{declaredPointer?.Identifier} -> {(declaredPointer == null || !declaredPointer.IsDeclared ? "undefined" : ToJsonString(declaredPointer.Value, 5))}; ");
						}
						break;
					case EntryValuePointer<G> entrtPointer:
						{
							builder.Append($"[{ToJsonString(entrtPointer.Key)}] -> {(entrtPointer == null ? "undefined" : ToJsonString(entrtPointer.Value, 5))}; ");
						}
						break;
					default:
						{
							builder.Append($"{pointer?.Identifier ?? "_"} -> {(pointer == null ? "undefined" : ToJsonString(pointer.Value, 5))}; ");
						}
						break;
				}
			}
			Debug.WriteLine("\tRegister: " + builder.ToString());
		}

		private static void PrintListRegister<G>(ExecutionState<G> executionState) where G : IGroupState<G>, new()
		{
			StringBuilder builder = new StringBuilder();
			foreach (ValuePointer<G> pointer in executionState.ListRegister)
			{
				switch (pointer)
				{
					case DeclaredValuePointer<G> declaredPointer:
						{
							builder.Append($"{declaredPointer?.Identifier} -> {(declaredPointer == null || !declaredPointer.IsDeclared ? "undefined" : ToJsonString(declaredPointer.Value, 5))}; ");
						}
						break;
					case EntryValuePointer<G> entrtPointer:
						{
							builder.Append($"[{ToJsonString(entrtPointer.Key)}] -> {(entrtPointer == null ? "undefined" : ToJsonString(entrtPointer.Value, 5))}; ");
						}
						break;
					default:
						{
							builder.Append($"{pointer?.Identifier ?? "_"} -> {(pointer == null ? "undefined" : ToJsonString(pointer.Value, 5))}; ");
						}
						break;
				}
			}
			Debug.WriteLine("\tListRegister: " + builder.ToString());
		}

		private static string ToJsonString<G>(IValuable<G> input, int maxDepth = 30) where G : IGroupState<G>, new()
		{
			string jsonValue = null;
			if (input == null)
			{
				jsonValue = "null";
			}
			else if (input is ICollectionValue<G> collectionValue)
			{
				jsonValue = maxDepth <= 0
					? "overflow"
					: collectionValue is DefaultCollectionValue<G> defaultCollectionValue && (defaultCollectionValue.IsMap || defaultCollectionValue == null)
						? $"{{{string.Join(", ", collectionValue.GetEntries().Select(entry => $"{ToJsonString(entry.Key, 2)}: {ToJsonString(entry.Value.Value, maxDepth - 1)}"))}}}"
						: $"[{string.Join(", ", collectionValue.GetValues().Select(item => ToJsonString(item.Value, maxDepth - 1)))}]";
			}
			else if (input is IActionValue<G>)
			{
				jsonValue = "action pointer";
			}
			else if (input is IGroupValue<G> groupValue)
			{
				jsonValue = $"{groupValue.State.Group.GroupName} pointer";
			}
			else
			{
				IValue value = input as IValue;
				jsonValue = ToValueString(value?.GetData() ?? input?.ToString() ?? "undefined");
			}
			return jsonValue;
		}

		private static string ToValueString(object value)
		{
			switch (value)
			{
				case string str:
					return $"\"{str}\"";
				case null:
					return "null";
				default:
					return value.ToString().ToLower();
			}
		}
	}
}
