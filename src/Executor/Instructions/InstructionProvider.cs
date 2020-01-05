using Synfron.Staxe.Executor.Instructions.Flags;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Collections;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Executor.Instructions
{
	public delegate void InstructionExecutionBody<G>(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
			where G : IGroupState<G>, new();

	public class InstructionProvider<G>
			where G : IGroupState<G>, new()
	{

		public Dictionary<string, InstructionExecutionBody<G>> SpecialInstructionMap
		{
			get;
			private set;
		} = new Dictionary<string, InstructionExecutionBody<G>>();

		public Instruction<G> GetSpecialInstruction(object[] payload, int? sourcePosition = null, bool interruptable = false)
		{
			return new Instruction<G>(InstructionCode.SPL, payload, sourcePosition, interruptable, SpecialInstructionMap[(string)payload[0]]);
		}

		protected static Instruction<G> GetInstruction(InstructionCode code, object[] payload, int? sourcePosition, bool interruptable, InstructionExecutionBody<G> executionBody)
		{
			if (executionBody == null)
			{
				throw new ArgumentNullException(nameof(executionBody));
			}
			return new Instruction<G>(code, payload, sourcePosition, interruptable, executionBody);
		}

		private static readonly InstructionExecutionBody<G>[] _instructionExecutionBodies = new InstructionExecutionBody<G>[]
		{
				 ExecuteInstructionNON,
				 null,
				 ExecuteInstructionG,
				 ExecuteInstructionGE,
				 ExecuteInstructionIFE,
				 ExecuteInstructionA,
				 ExecuteInstructionAE,
				 ExecuteInstructionB,
				 ExecuteInstructionBE,
				 ExecuteInstructionL,
				 ExecuteInstructionLE,
				 ExecuteInstructionCS,
				 ExecuteInstructionCSE,
				 ExecuteInstructionCSP,
				 ExecuteInstructionCGP,
				 ExecuteInstructionCDP,
				 ExecuteInstructionSPR,
				 ExecuteInstructionGPR,
				 ExecuteInstructionDPR,
				 ExecuteInstructionVR,
				 ExecuteInstructionCPR,
				 ExecuteInstructionRCP,
				 ExecuteInstructionURDP,
				 ExecuteInstructionCG,
				 ExecuteInstructionMG,
				 ExecuteInstructionMF,
				 ExecuteInstructionPLR,
				 ExecuteInstructionPHR,
				 ExecuteInstructionCPHR,
				 ExecuteInstructionRM,
				 ExecuteInstructionGR,
				 ExecuteInstructionGDR,
				 ExecuteInstructionRGD,
				 ExecuteInstructionRGH,
				 ExecuteInstructionHGR,
				 ExecuteInstructionMI,
				 ExecuteInstructionMP,
				 ExecuteInstructionMIR,
				 ExecuteInstructionMPR,
				 ExecuteInstructionOA,
				 ExecuteInstructionRVS,
				 ExecuteInstructionAR,
				 ExecuteInstructionRAR,
				 ExecuteInstructionRGAR,
				 ExecuteInstructionRGGPR,
				 ExecuteInstructionRGDPR,
				 ExecuteInstructionSPSP,
				 ExecuteInstructionSPGP,
				 ExecuteInstructionSPDP,
				 ExecuteInstructionGPSP,
				 ExecuteInstructionGPGP,
				 ExecuteInstructionGPDP,
				 ExecuteInstructionDPSP,
				 ExecuteInstructionDPGP,
				 ExecuteInstructionDPDP,
				 ExecuteInstructionVSP,
				 ExecuteInstructionVGP,
				 ExecuteInstructionVDP,
				 ExecuteInstructionRR,
				 ExecuteInstructionSPCSP,
				 ExecuteInstructionSPCGP,
				 ExecuteInstructionSPCDP,
				 ExecuteInstructionGPCSP,
				 ExecuteInstructionGPCGP,
				 ExecuteInstructionGPCDP,
				 ExecuteInstructionDPCSP,
				 ExecuteInstructionDPCGP,
				 ExecuteInstructionDPCDP,
				 ExecuteInstructionVCSP,
				 ExecuteInstructionVCGP,
				 ExecuteInstructionVCDP,
				 ExecuteInstructionRLR,
				 ExecuteInstructionLRR,
				 ExecuteInstructionLRAS,
				 ExecuteInstructionCVR,
				 ExecuteInstructionRVK,
				 ExecuteInstructionRVI,
				 ExecuteInstructionPMR,
				 ExecuteInstructionGMR,
				 ExecuteInstructionRPM,
				 ExecuteInstructionRGM,
				 ExecuteInstructionCR,
				 ExecuteInstructionUS,
				 ExecuteInstructionRCE,
				 ExecuteInstructionC,
				 ExecuteInstructionNC,
				 ExecuteInstructionID,
				 ExecuteInstructionJ,
				 ExecuteInstructionH,
				 ExecuteInstructionRPlus,
				 ExecuteInstructionRSubtract,
				 ExecuteInstructionRMultiply,
				 ExecuteInstructionRDivide,
				 ExecuteInstructionRRemainder,
				 ExecuteInstructionRLeftShift,
				 ExecuteInstructionRRightShift,
				 ExecuteInstructionRLessThan,
				 ExecuteInstructionRGreaterThan,
				 ExecuteInstructionRLessThanOrEquals,
				 ExecuteInstructionRGreaterThanOrEquals,
				 ExecuteInstructionREquals,
				 ExecuteInstructionRNotEquals,
				 ExecuteInstructionRAnd,
				 ExecuteInstructionROr,
				 ExecuteInstructionRNot
		};

		public static Instruction<G> GetInstruction(InstructionCode code, object[] payload = null, int? sourcePosition = null, bool interruptable = false)
		{
			return GetInstruction(code, payload, sourcePosition, interruptable, _instructionExecutionBodies[(int)code]);
		}

		private static void ExecuteInstructionNON(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{

		}

		private static void ExecuteInstructionA(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			if ((bool)payload[0])
			{
				Frame<G> newFrame = new Frame<G>
				{
					GroupState = executionState.GroupState,
					PreviousInstructionIndex = executionState.InstructionIndex
				};
				executionState.Frames.Add(newFrame);
				executionState.Sync(newFrame);
			}
		}

		private static void ExecuteInstructionAE(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			executionState.Frames.RemoveLast();
			if (executionState.Frames.LastOrDefault() is Frame<G> frame)
			{
				frame.PreviousInstructionIndex = payload?.ElementAtOrDefault(0) as int? ?? frame.PreviousInstructionIndex;
				executionState.Sync(frame);
			}
			else
			{
				executionState.Executable = false;
			}
		}


		private static void ExecuteInstructionCVR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int? initialCapacity = null;
			if ((bool)payload[0])
			{
				IValue<G> value = (IValue<G>)stackRegister.Last().Value;
				initialCapacity = value.ToInt();
			}
			stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetCollection(initialCapacity, (int?)payload[1]) });
		}


		private static void ExecuteInstructionB(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			executionState.LastFrame.BlockDepth++;
		}


		private static void ExecuteInstructionBE(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int blockDepth = executionState.LastFrame.BlockDepth;
			int removeCount = 0;
			for (int i = stackPointers.Count - 1; i >= 0 && stackPointers[i].Depth >= blockDepth; i--, removeCount++) ;

			stackPointers.RemoveRange(stackPointers.Count - removeCount, removeCount);
			executionState.LastFrame.BlockDepth--;
		}


		private static void ExecuteInstructionID(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = stackRegister.Last();
			switch (pointer)
			{
				case null:
				case EntryValuePointer<G> entryPointer when !entryPointer.IsSet:
				case DeclaredValuePointer<G> declaredPointer when !declaredPointer.IsDeclared:
				case VoidableStackValuePointer<G> voidablePointer when voidablePointer.IsVoid:
					stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.False });
					break;
				default:
					stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.True });
					break;
			}
		}

		private static void ExecuteInstructionC(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> valuable = stackRegister.TakeLast().Value;
			if (valuable.IsTrue())
			{
				executionState.InstructionIndex = (int)payload[0] - 1;
			}
		}


		private static void ExecuteInstructionCDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[0], (string)payload[1], PointerOperation.Add, executor.ValueProvider);
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionCG(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			Copy copyOptions = (Copy)payload[0];
			IGroupValue<G> groupValue = (IGroupValue<G>)stackRegister.Last().Value;
			if (groupValue.State.Modifiers.Contains(Modifiers.Static)) throw new EngineRuntimeException("Value is static");
			stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetGroup(groupValue.State.Clone(copyOptions)) });
		}


		private static void ExecuteInstructionMG(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			GroupMerge mergeOptions = (GroupMerge)payload[0];
			IGroupValue<G> otherGroupValue = (IGroupValue<G>)stackRegister.TakeLast().Value;
			IGroupValue<G> groupValue = (IGroupValue<G>)stackRegister.Last().Value;
			groupValue.State.Merge(otherGroupValue.State, executor.ValueProvider, mergeOptions);
		}


		private static void ExecuteInstructionMF(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			Frame<G> sourceFrame = executionState.Frames[executionState.Frames.Count - (int)payload[0] - 1];
			FrameMerge mergeOptions = (FrameMerge)payload[1];
			if (mergeOptions.Contains(FrameMerge.Register))
			{
				StackList<ValuePointer<G>> sourceStackRegister = sourceFrame.StackRegister;
				int size = sourceStackRegister.Count;
				int index = stackRegister.Reserve(size);
				for (int i = 0; i < size; i++)
				{
					stackRegister.UnsafeSet(index + i, sourceStackRegister[i]);
				}
			}
			if (mergeOptions.Contains(FrameMerge.Stack))
			{
				StackList<StackValuePointer<G>> sourceStackPointers = sourceFrame.StackPointers;
				int size = sourceStackPointers.Count;
				int currentLocation = executionState.InstructionIndex;
				int depth = executionState.LastFrame.BlockDepth;
				int index = stackPointers.Reserve(size);
				for (int i = 0; i < size; i++)
				{
					stackPointers.UnsafeSet(index + i, new ReferenceStackValuePointer<G>(sourceStackPointers[i])
					{
						Origin = currentLocation,
						Depth = depth
					});
				}
			}
		}


		private static void ExecuteInstructionCGP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			string identifier = (string)payload[0];
			DeclaredValuePointer<G> pointer = new DeclaredValuePointer<G>("group", executor.ValueProvider.Null)
			{
				Identifier = identifier
			};
			executionState.GroupState.GroupPointers.Add(pointer);
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionCPHR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.Add(stackRegister.LastOrDefault());
		}


		private static void ExecuteInstructionRM(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int offset = (int)payload[0];
			ValuePointer<G> pointer = stackRegister.TakeLast();
			stackRegister.Insert(stackRegister.Count - offset, pointer);
		}


		private static void ExecuteInstructionCR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.Clear();
		}


		private static void ExecuteInstructionCS(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
		}


		private static void ExecuteInstructionCSE(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			executionState.InstructionIndex = (int)payload[0] - 1;
		}


		private static void ExecuteInstructionCSP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			StackValuePointer<G> pointer = new StackValuePointer<G>(executor.ValueProvider.Null)
			{
				Identifier = (string)payload[0],
				Origin = executionState.InstructionIndex,
				Depth = executionState.LastFrame.BlockDepth
			};
			stackPointers.Add(pointer);
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionDPCDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[2], (string)payload[3], PointerOperation.Add, executor.ValueProvider);
			stackRegister.SetLast(pointer);
			pointer.Value = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[0], (string)payload[1], PointerOperation.Get, executor.ValueProvider).Value;
		}


		private static void ExecuteInstructionDPCGP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			string identifier = (string)payload[2];

			DeclaredValuePointer<G> pointer = new DeclaredValuePointer<G>("group", executor.ValueProvider.Null)
			{
				Identifier = identifier
			};
			executionState.GroupState.GroupPointers.Add(pointer);
			stackRegister.SetLast(pointer);
			pointer.Value = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[0], (string)payload[1], PointerOperation.Get, executor.ValueProvider).Value;
		}


		private static void ExecuteInstructionDPCSP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			StackValuePointer<G> pointer = new StackValuePointer<G>(executor.ValueProvider.Null)
			{
				Identifier = (string)payload[2],
				Origin = executionState.InstructionIndex,
				Depth = executionState.LastFrame.BlockDepth
			};
			stackPointers.Add(pointer);
			stackRegister.SetLast(pointer);
			pointer.Value = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[0], (string)payload[1], PointerOperation.Get, executor.ValueProvider).Value;
		}


		private static void ExecuteInstructionDPDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[2], (string)payload[3], PointerOperation.Get, executor.ValueProvider);
			pointer.Value = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[0], (string)payload[1], PointerOperation.Get, executor.ValueProvider).Value;
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionDPGP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executionState.GroupState.GroupPointers[(int)payload[2]];
			pointer.Value = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[0], (string)payload[1], PointerOperation.Get, executor.ValueProvider).Value;
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionDPR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.SetLast(executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[0], (string)payload[1], PointerOperation.Get, executor.ValueProvider));
		}


		private static void ExecuteInstructionDPSP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			StackValuePointer<G> pointer = stackPointers[stackPointers.Count - 1 - (int)payload[2]];
			pointer.Value = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[0], (string)payload[1], PointerOperation.Get, executor.ValueProvider).Value;
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionG(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			G groupState = Activator.CreateInstance<G>();
			groupState.Group = new Group<G> { GroupName = (string)payload[0] };
			stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetGroup(groupState) });

			List<Instruction<G>> newInstructions = new List<Instruction<G>>();
			while (++executionState.InstructionIndex < executionState.Instructions.Length)
			{
				ref Instruction<G> instruction = ref executionState.Instructions[executionState.InstructionIndex];
				if (instruction.Code != InstructionCode.GE)
				{
					newInstructions.Add(instruction);
				}
				else
				{
					break;
				}
			}
			newInstructions.Add(GetInstruction(InstructionCode.IFE));

			groupState.Group.Instructions = newInstructions.ToArray();

			executionState.LastFrame.PreviousInstructionIndex = executionState.InstructionIndex;

			Frame<G> newFrame = new Frame<G>
			{
				GroupState = groupState,
				PreviousInstructionIndex = -1
			};
			executionState.Frames.Add(newFrame);
			executionState.Sync(newFrame);
		}


		private static void ExecuteInstructionGDR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int location = payload?.ElementAtOrDefault(0) is int payloadLocation ? payloadLocation : ((IValue<G, int>)stackRegister.TakeLast().Value).Data;
			stackRegister.SetLast(executionState.GroupState.Dependencies.ElementAtOrDefault(location) is G groupState ? new ValuePointer<G> { Value = executor.ValueProvider.GetGroup(groupState) } : null);
		}


		private static void ExecuteInstructionGE(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
		}


		private static void ExecuteInstructionGPCDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[1], (string)payload[2], PointerOperation.Add, executor.ValueProvider);
			stackRegister.SetLast(pointer);
			pointer.Value = executionState.GroupState.GroupPointers[(int)payload[0]].Value;
		}


		private static void ExecuteInstructionGPCGP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			string identifier = (string)payload[1];
			DeclaredValuePointer<G> pointer = new DeclaredValuePointer<G>("group", executor.ValueProvider.Null)
			{
				Identifier = identifier
			};
			executionState.GroupState.GroupPointers.Add(pointer);
			stackRegister.SetLast(pointer);
			pointer.Value = executionState.GroupState.GroupPointers[(int)payload[0]].Value;
		}


		private static void ExecuteInstructionGPCSP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			StackValuePointer<G> pointer = new StackValuePointer<G>(executor.ValueProvider.Null)
			{
				Identifier = (string)payload[1],
				Origin = executionState.InstructionIndex,
				Depth = executionState.LastFrame.BlockDepth
			};
			stackPointers.Add(pointer);
			stackRegister.SetLast(pointer);
			pointer.Value = executionState.GroupState.GroupPointers[(int)payload[0]].Value;
		}


		private static void ExecuteInstructionGPDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[1], (string)payload[2], PointerOperation.Get, executor.ValueProvider);
			pointer.Value = executionState.GroupState.GroupPointers[(int)payload[0]].Value;
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionGPGP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executionState.GroupState.GroupPointers[(int)payload[1]];
			pointer.Value = executionState.GroupState.GroupPointers[(int)payload[0]].Value;
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionGPR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.SetLast(executionState.GroupState.GroupPointers.ElementAtOrDefault((int)payload[0]));
		}


		private static void ExecuteInstructionGPSP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			StackValuePointer<G> pointer = stackPointers[stackPointers.Count - 1 - (int)payload[1]];
			pointer.Value = executionState.GroupState.GroupPointers[(int)payload[0]].Value;
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionGR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetGroup(executionState.GroupState) });
		}


		private static void ExecuteInstructionH(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			executionState.Executable = false;
		}


		private static void ExecuteInstructionRGH(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IGroupValue<G> groupValue = (IGroupValue<G>)stackRegister.Last().Value;
			executor.Groups[payload.ElementAtOrDefault(0) as string ?? groupValue.State.Group.GroupName] = groupValue.State;
		}


		private static void ExecuteInstructionHGR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetGroup(executor.Groups[(string)payload[0]]) });
		}


		private static void ExecuteInstructionIFE(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			G groupState = executionState.GroupState;
			ExecuteInstructionAE(executor, executionState, payload, stackRegister, stackPointers);
			executionState.StackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetGroup(groupState) });
		}


		private static void ExecuteInstructionJ(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			executionState.InstructionIndex = (int)payload[0] - 1;
		}


		private static void ExecuteInstructionL(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
		}


		private static void ExecuteInstructionLE(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			executionState.InstructionIndex = (int)payload[0] - 1;
		}


		private static void ExecuteInstructionLRAS(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int itemCount = (int)payload[0];
			int currentLocation = executionState.InstructionIndex;
			int depth = executionState.LastFrame.BlockDepth;
			int listItemCount = executionState.ListRegister.Count;
			StackValuePointer<G>[] pointers = new StackValuePointer<G>[itemCount];
			for (int index = 0; index < itemCount && index < listItemCount; index++)
			{
				pointers[index] = new ReferenceStackValuePointer<G>(executionState.ListRegister[index])
				{
					Origin = currentLocation,
					Depth = depth
				};
			}
			IActionValue<G> actionValue = (IActionValue<G>)stackRegister.Last().Value;
			actionValue.InitStackPointers.AddRange(pointers);
			executionState.ListRegister.Clear();
		}


		private static void ExecuteInstructionNC(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> valuable = stackRegister.TakeLast().Value;
			if (!valuable.IsTrue())
			{
				executionState.InstructionIndex = (int)payload[0] - 1;
			}
		}


		private static void ExecuteInstructionOA(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int location = (int)payload[0];
			IValuable<G> valuable = stackRegister.TakeLast().Value;
			IGroupValue<G> groupValue = (IGroupValue<G>)stackRegister.Last().Value;
			groupValue.State.ActionOverrides[location] = valuable;
		}


		private static void ExecuteInstructionPHR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.Add(null);
		}


		private static void ExecuteInstructionPLR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.TakeLast();
		}


		private static void ExecuteInstructionMI(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			string name = (string)payload[0];
			int location = (int)payload[1];
			executionState.GroupState.Group.InstructionMap[name] = location;
		}


		private static void ExecuteInstructionMP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			string name = (string)payload[0];
			int location = (int)payload[1];
			executionState.GroupState.PointerMap[name] = location;
		}


		private static void ExecuteInstructionMPR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			G groupState;
			IValue<G, string> identity;
			if (payload?.ElementAtOrDefault(0) is bool useRegisterGroup && useRegisterGroup)
			{
				identity = (IValue<G, string>)stackRegister.TakeLast().Value;
				groupState = ((IGroupValue<G>)stackRegister.Last().Value).State;
			}
			else
			{
				identity = (IValue<G, string>)stackRegister.Last().Value;
				groupState = executionState.GroupState;
			}
			if (groupState.PointerMap.TryGetValue(identity.Data, out int index))
			{
				stackRegister.SetLast(groupState.GroupPointers.ElementAtOrDefault(index));
			}
			else
			{
				stackRegister.SetLast(null);
			}
		}


		private static void ExecuteInstructionMIR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			G groupState;
			IValue<G, string> identity;
			if (payload?.ElementAtOrDefault(0) is bool useRegisterGroup && useRegisterGroup)
			{
				identity = (IValue<G, string>)stackRegister.TakeLast().Value;
				groupState = ((IGroupValue<G>)stackRegister.Last().Value).State;
			}
			else
			{
				identity = (IValue<G, string>)stackRegister.Last().Value;
				groupState = executionState.GroupState;
			}
			stackRegister.SetLast(groupState.Group.InstructionMap.TryGetValue(identity.Data, out int location) ? new ValuePointer<G>
			{
				Value = executor.ValueProvider.GetInt(location)
			} : null);
		}


		private static void ExecuteInstructionRAR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int location;
			IValue<G> value = (IValue<G>)stackRegister.Last().Value;
			location = value.ToInt();
			IActionValue<G> actionValue = executor.ValueProvider.GetAction(executionState.GroupState, location);
			actionValue.Identifier = payload.ElementAtOrDefault(0) as string;
			stackRegister.SetLast(new ValuePointer<G>
			{
				Value = actionValue
			});
		}


		private static void ExecuteInstructionAR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int location = (int)payload[0];
			IActionValue<G> actionValue = executor.ValueProvider.GetAction(executionState.GroupState, location);
			actionValue.Identifier = payload.ElementAtOrDefault(1) as string;
			stackRegister.SetLast(new ValuePointer<G>
			{
				Value = actionValue
			});
		}


		private static void ExecuteInstructionRCE(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> executablePointer = stackRegister.TakeLast();
			if ((executablePointer as DeclaredValuePointer<G>)?.Modifiers.Contains(Modifiers.ExecuteRestricted) ?? false) throw new EngineRuntimeException("Execution is not allowed");

			executionState.LastFrame.PreviousInstructionIndex = executionState.InstructionIndex;
			executablePointer.Value.Execute(executionState, executor.ValueProvider);
		}


		private static void ExecuteInstructionRGAR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValue<G> value = (IValue<G>)stackRegister.TakeLast().Value;
			IGroupValue<G> group = (IGroupValue<G>)stackRegister.Last().Value;
			IActionValue<G> actionValue = executor.ValueProvider.GetAction(group.State, value.ToInt());
			actionValue.Identifier = payload?.ElementAtOrDefault(0) as string;
			stackRegister.SetLast(new ValuePointer<G>
			{
				Value = actionValue
			});
		}


		private static void ExecuteInstructionRGD(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IGroupValue<G> dependencyGroupValue = (IGroupValue<G>)stackRegister.TakeLast().Value;
			IGroupValue<G> groupValue = (IGroupValue<G>)stackRegister.Last().Value;
			groupValue.State.Dependencies.Add(dependencyGroupValue.State);
		}


		private static void ExecuteInstructionRGDPR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IGroupValue<G> value = (IGroupValue<G>)stackRegister.Last().Value;
			string location = (string)payload[0];
			string name = payload.ElementAtOrDefault(1) as string;
			stackRegister.SetLast(executor.ExternalDynamicPointers(executionState, value.State, location, name, PointerOperation.Get, executor.ValueProvider));
		}


		private static void ExecuteInstructionRGGPR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IGroupValue<G> value = (IGroupValue<G>)stackRegister.Last().Value;
			stackRegister.SetLast(value.State.GroupPointers[(int)payload[0]]);
		}


		private static void ExecuteInstructionRAnd(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.And(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRDivide(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.DivideBy(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionREquals(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.IsEqualTo(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRGreaterThan(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.IsGreaterThan(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRGreaterThanOrEquals(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.IsGreaterThanOrEqualTo(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRLeftShift(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.LeftShift(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRLessThan(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.IsLessThan(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRLessThanOrEquals(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.IsLessThanOrEqualTo(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRLR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int count = (int)payload[0];
			ValuePointer<G>[] pointers = new ValuePointer<G>[count];
			for (int index = count - 1; index >= 0; index--)
			{
				pointers[index] = stackRegister.TakeLast();
			}
			executionState.ListRegister.AddRange(pointers);
		}

		private static void ExecuteInstructionLRR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int paramCount = (int)payload[0];
			bool copyReference = (bool)payload[1];
			int listCount = executionState.ListRegister.Count;
			for (int paramIndex = 0; paramIndex < paramCount; paramIndex++)
			{
				if (listCount > paramIndex)
				{
					ValuePointer<G> pointer = executionState.ListRegister[paramIndex];
					stackRegister.Add(copyReference ? pointer : new ValuePointer<G> { Value = pointer.Value });
				}
				else
				{
					stackRegister.Add(null);
				}
			}
			executionState.ListRegister.Clear();
		}


		private static void ExecuteInstructionRMultiply(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.MultiplyBy(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRNot(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> valuable = stackRegister.Last().Value.Not(executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRNotEquals(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.IsNotEqualTo(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionROr(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.Or(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRPlus(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.Add(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = stackRegister.TakeLast();
			stackRegister.Last().Value = pointer.Value;
		}


		private static void ExecuteInstructionRRemainder(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.Remainder(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRRightShift(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.RightShift(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRSubtract(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> secondValuable = stackRegister.TakeLast().Value;
			IValuable<G> valuable = stackRegister.Last().Value.Minus(secondValuable, executor.ValueProvider);
			stackRegister.SetLast(new ValuePointer<G> { Value = valuable });
		}


		private static void ExecuteInstructionRVI(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> valuable = stackRegister.TakeLast().Value;
			stackRegister.SetLast((stackRegister.Last().Value).GetAt(valuable, executor.ValueProvider));
		}


		private static void ExecuteInstructionRVK(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValue<G> value = (IValue<G>)stackRegister.TakeLast().Value;
			stackRegister.SetLast(stackRegister.Last().Value.Get(value, (bool)payload[0], executor.ValueProvider));
		}


		private static void ExecuteInstructionRVS(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValuable<G> valueable = stackRegister.Last().Value;
			stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetInt(valueable.Size) });
		}


		private static void ExecuteInstructionSPCDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[1], (string)payload[2], PointerOperation.Add, executor.ValueProvider);
			stackRegister.SetLast(pointer);
			pointer.Value = (stackPointers[stackPointers.Count - 1 - (int)payload[0]]).Value;
		}


		private static void ExecuteInstructionSPCGP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			string identifier = (string)payload[1];
			DeclaredValuePointer<G> pointer = new DeclaredValuePointer<G>("group", executor.ValueProvider.Null)
			{
				Identifier = identifier
			};
			executionState.GroupState.GroupPointers.Add(pointer);
			stackRegister.SetLast(pointer);
			pointer.Value = (stackPointers[stackPointers.Count - 1 - (int)payload[0]]).Value;
		}


		private static void ExecuteInstructionSPCSP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			StackValuePointer<G> pointer = new StackValuePointer<G>(executor.ValueProvider.Null)
			{
				Identifier = (string)payload[1],
				Origin = executionState.InstructionIndex,
				Depth = executionState.LastFrame.BlockDepth
			};
			stackPointers.Add(pointer);
			stackRegister.SetLast(pointer);
			pointer.Value = (stackPointers[stackPointers.Count - 1 - (int)payload[0]]).Value;
		}


		private static void ExecuteInstructionSPDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[1], (string)payload[2], PointerOperation.Get, executor.ValueProvider);
			pointer.Value = (stackPointers[stackPointers.Count - 1 - (int)payload[0]]).Value;
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionSPGP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executionState.GroupState.GroupPointers[(int)payload[1]];
			pointer.Value = (stackPointers[stackPointers.Count - 1 - (int)payload[0]]).Value;
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionSPR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.SetLast(stackPointers[stackPointers.Count - 1 - (int)payload[0]]);
		}


		private static void ExecuteInstructionSPSP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			StackValuePointer<G> pointer = stackPointers[stackPointers.Count - 1 - (int)payload[1]];
			pointer.Value = (stackPointers[stackPointers.Count - 1 - (int)payload[0]]).Value;
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionURDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = stackRegister.TakeLast();
			switch (pointer)
			{
				case DeclaredValuePointer<G> declaredPointer:
					if (declaredPointer.IsDynamic)
					{
						declaredPointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, declaredPointer.Location, declaredPointer.Identifier, PointerOperation.Delete, executor.ValueProvider);
					}
					declaredPointer.IsDeclared = false;
					break;
				case EntryValuePointer<G> entryPointer:
					entryPointer.Undeclare();
					break;
				default:
					throw new EngineRuntimeException("Cannot be undeclared.");
			}
		}


		private static void ExecuteInstructionUS(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int origin = (int)payload[0];
			while (stackPointers.Count > 0 && stackPointers.Last().Origin >= origin)
			{
				stackPointers.TakeLast();
			}
		}


		private static void ExecuteInstructionVCDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[1], (string)payload[2], PointerOperation.Add, executor.ValueProvider);
			pointer.Value = executor.ValueProvider.GetAsValue(payload[0]);
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionVCGP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			DeclaredValuePointer<G> pointer = new DeclaredValuePointer<G>("group", executor.ValueProvider.Null)
			{
				Identifier = (string)payload[1]
			};
			pointer.Value = executor.ValueProvider.GetAsValue(payload[0]);
			executionState.GroupState.GroupPointers.Add(pointer);
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionVCSP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			StackValuePointer<G> pointer = new StackValuePointer<G>(executor.ValueProvider.Null)
			{
				Identifier = (string)payload[1],
				Origin = executionState.InstructionIndex,
				Depth = executionState.LastFrame.BlockDepth
			};
			stackPointers.Add(pointer);
			pointer.Value = executor.ValueProvider.GetAsValue(payload[0]);
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionVDP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[1], (string)payload[2], PointerOperation.Get, executor.ValueProvider);
			pointer.Value = executor.ValueProvider.GetAsValue(payload[0]);
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionVGP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			ValuePointer<G> pointer = executionState.GroupState.GroupPointers[(int)payload[1]];
			pointer.Value = executor.ValueProvider.GetAsValue(payload[0]);
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionVR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetAsValue(payload[0]) });
		}


		private static void ExecuteInstructionVSP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			StackValuePointer<G> pointer = stackPointers[stackPointers.Count - 1 - (int)payload[1]];
			pointer.Value = executor.ValueProvider.GetAsValue(payload[0]);
			stackRegister.SetLast(pointer);
		}


		private static void ExecuteInstructionRPM(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValue<G> modifierValue = (IValue<G>)stackRegister.TakeLast().Value;
			if (stackRegister.Last() is DeclaredValuePointer<G> pointer)
			{
				pointer.Modifiers = (Modifiers)modifierValue.ToInt();
			}
		}


		private static void ExecuteInstructionRGM(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IValue<G> modifierValue = (IValue<G>)stackRegister.TakeLast().Value;
			IGroupValue<G> groupValue = (IGroupValue<G>)stackRegister.Last().Value;
			groupValue.State.Modifiers = (Modifiers)modifierValue.ToInt();
		}


		private static void ExecuteInstructionPMR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetInt((int)(((DeclaredValuePointer<G>)stackRegister.Last())?.Modifiers ?? Modifiers.None)) });
		}


		private static void ExecuteInstructionGMR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			IGroupValue<G> groupValue = (IGroupValue<G>)stackRegister.Last().Value;
			stackRegister.SetLast(new ValuePointer<G> { Value = executor.ValueProvider.GetInt((int)groupValue.State.Modifiers) });
		}

		private static void ExecuteInstructionCPR(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int endOffset = (int)payload[0];
			int count = (int)payload[1];
			int payloadIndex = 2;
			int index = stackRegister.Count > 0 ? stackRegister.Reserve(Math.Max(count + endOffset, 0)) + endOffset : stackRegister.Reserve(count);
			for (int i = 0; i < count; i++)
			{
				switch ((InstructionCode)payload[payloadIndex++])
				{
					case InstructionCode.SPR:
						stackRegister.UnsafeSet(index++, stackPointers[stackPointers.Count - 1 - (int)payload[payloadIndex++]]);
						break;
					case InstructionCode.VR:
						stackRegister.UnsafeSet(index++, new ValuePointer<G> { Value = executor.ValueProvider.GetAsValue(payload[payloadIndex++]) });
						break;
					case InstructionCode.GPR:
						stackRegister.UnsafeSet(index++, executionState.GroupState.GroupPointers[(int)payload[payloadIndex++]]);
						break;
					case InstructionCode.DPR:
						stackRegister.UnsafeSet(index++, executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[payloadIndex++], (string)payload[payloadIndex++], PointerOperation.Get, executor.ValueProvider));
						break;
				}
			}
		}

		private static void ExecuteInstructionRCP(IInstructionExecutor<G> executor, ExecutionState<G> executionState, object[] payload, StackList<ValuePointer<G>> stackRegister, StackList<StackValuePointer<G>> stackPointers)
		{
			int count = (int)payload[0];
			int payloadIndex = 1;
			int index = stackRegister.Count - count;
			for (int i = 0; i < count; i++)
			{
				ValuePointer<G> pointer = stackRegister[index++];
				switch ((InstructionCode)payload[payloadIndex++])
				{
					case InstructionCode.SPR:
						stackPointers[stackPointers.Count - 1 - (int)payload[payloadIndex++]].Value = pointer.Value;
						break;
					case InstructionCode.GPR:
						executionState.GroupState.GroupPointers[(int)payload[payloadIndex++]].Value = pointer.Value;
						break;
					case InstructionCode.DPR:
						executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[payloadIndex++], (string)payload[payloadIndex++], PointerOperation.Get, executor.ValueProvider).Value = pointer.Value;
						break;
					case InstructionCode.CSP:
						switch (pointer)
						{
							case null:
								stackPointers.Add(new VoidableStackValuePointer<G>
								{
									Identifier = (string)payload[payloadIndex++],
									Origin = executionState.InstructionIndex,
									Depth = executionState.LastFrame.BlockDepth,
									IsVoid = true
								});
								break;
							default:
								stackPointers.Add(new StackValuePointer<G>(pointer.Value)
								{
									Identifier = (string)payload[payloadIndex++],
									Origin = executionState.InstructionIndex,
									Depth = executionState.LastFrame.BlockDepth
								});
								break;
						}
						break;
					case InstructionCode.CGP:
						executionState.GroupState.GroupPointers.Add(new DeclaredValuePointer<G>("group", pointer.Value)
						{
							Identifier = (string)payload[payloadIndex++]
						});
						break;
					case InstructionCode.CDP:
						executor.ExternalDynamicPointers(executionState, executionState.GroupState, (string)payload[payloadIndex++], (string)payload[payloadIndex++], PointerOperation.Add, executor.ValueProvider).Value = pointer.Value;
						break;
				}
			}
			stackRegister.RemoveRange(stackRegister.Count - count, count);
		}
	}
}