using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Generator
{
	public class InstructionOptimizer<G> where G : IGroupState<G>, new()
	{
		private int _groupStartIndex;
		private int _currentIndex;
		private int _innerIndex;
		private int _innerOffset;
		private IList<Instruction<G>> _oldInstructions;
		private List<Instruction<G>> _newInstructions;
		private List<InstructionOffset> _instructionOffsets;
		private bool _optmized;
		private string _currentGroupId;


		public bool EnableOptimizeOverridingInstructions { get; set; } = true;
		public bool EnableOptimizeDuplicatesInstructions { get; set; } = true;
		public bool EnableOptimizeOpposingInstructions { get; set; } = true;
		public bool EnableOptimizeAssignmentInstructions { get; set; } = true;
		public bool EnableOptimizeCopyPushInstructions { get; set; } = true;
		public bool EnableOptimizeMultipleGetPointersInstructions { get; set; } = true;


		public IList<Instruction<G>> Optimize(IList<Instruction<G>> instructions)
		{
			bool enableOptimizeOverridingInstructions = EnableOptimizeOverridingInstructions;
			bool enableOptimizeOpposingInstructions = EnableOptimizeOpposingInstructions;
			bool enableOptimizeDuplicatesInstructions = EnableOptimizeDuplicatesInstructions;
			bool enableOptimizeAssignmentInstructions = EnableOptimizeAssignmentInstructions;
			bool enableOptimizeCopyPushInstructions = EnableOptimizeCopyPushInstructions;
			bool enableOptimizeMultipleGetPointersInstructions = EnableOptimizeMultipleGetPointersInstructions;

			_oldInstructions = instructions;
			_optmized = false;
			_instructionOffsets = new List<InstructionOffset>();
			Stack<GroupPoint> groupPoints = new Stack<GroupPoint>();
			bool continueOptimization = true;
			while (continueOptimization)
			{
				int lastGroupIndex = -1;
				_groupStartIndex = 0;
				_currentIndex = 0;
				_innerIndex = 0;
				_innerOffset = 0;
				_currentGroupId = "0";
				continueOptimization = false;
				_newInstructions = new List<Instruction<G>>(_oldInstructions.Count);

				for (; _currentIndex < _oldInstructions.Count; _currentIndex++, _innerIndex++)
				{
					Instruction<G> instruction = _oldInstructions[_currentIndex];
					bool instructionOptimized = false;
					switch (instruction.Code)
					{
						case InstructionCode.G:
							groupPoints.Push(new GroupPoint(_currentGroupId, ++lastGroupIndex, _groupStartIndex, _innerIndex, _innerOffset));
							_currentGroupId = $"{_currentGroupId}.{lastGroupIndex}";
							lastGroupIndex = -1;
							_groupStartIndex = _currentIndex + 1;
							_innerOffset = 0;
							_innerIndex = 0;
							break;
						case InstructionCode.GE:
							GroupPoint groupPoint = groupPoints.Pop();
							_innerIndex += groupPoint.InnerIndex;
							_innerOffset += groupPoint.Offset;
							_currentGroupId = groupPoint.GroupId;
							lastGroupIndex = groupPoint.LastGroupIndex;
							_groupStartIndex = groupPoint.GroupStartIndex;
							break;
						default:
							instructionOptimized = (enableOptimizeOverridingInstructions && OptimizeOverridingInstructions()) ||
						(enableOptimizeDuplicatesInstructions && OptimizeDuplicateInstructions()) ||
						(enableOptimizeOpposingInstructions && OptimizeOpposingInstructions()) ||
						(enableOptimizeAssignmentInstructions && OptimizeAssignmentInstructions()) ||
						(enableOptimizeCopyPushInstructions && OptimizeCopyPushInstructions()) ||
						(enableOptimizeMultipleGetPointersInstructions && OptimizeMultipleGetPointersInstructions());
							break;
					}

					if (!instructionOptimized)
					{
						_newInstructions.Add(_oldInstructions[_currentIndex]);
					}
					continueOptimization |= instructionOptimized;
				}

				_oldInstructions = _newInstructions;
				_optmized |= continueOptimization;
			}

			if (_optmized)
			{
				PublishChanges();
			}
			return _newInstructions;
		}

		private bool OptimizeAssignmentInstructions()
		{
			bool optimized = false;
			Instruction<G> instruction1 = _oldInstructions[_currentIndex];
			Instruction<G> instruction2 = _oldInstructions.ElementAtOrDefault(_currentIndex + 1);
			Instruction<G> instruction3 = _oldInstructions.ElementAtOrDefault(_currentIndex + 2);
			Instruction<G> instruction4 = _oldInstructions.ElementAtOrDefault(_currentIndex + 3);

			if (IsPushCode(instruction2.Code) && instruction4.Code == InstructionCode.RR &&
				IsCreateOrGetPointerCode(instruction1.Code) &&
				IsGetPointerOrValueCode(instruction3.Code))
			{

				if (Enum.TryParse(instruction3.Code.ToString().Replace("R", "") + instruction1.Code.ToString().Replace("R", ""), out InstructionCode newCode))
				{
					object[] payload = instruction3.Payload.Concat(instruction1.Payload).ToArray();
					int? sourcePosition = instruction1.SourcePosition ?? instruction2.SourcePosition ?? instruction3.SourcePosition ?? instruction4.SourcePosition;
					bool interruptable = instruction1.Interruptable || instruction2.Interruptable || instruction3.Interruptable || instruction4.Interruptable;

					Instruction<G> newInstruction = InstructionProvider<G>.GetInstruction(newCode, payload, sourcePosition, interruptable);
					_newInstructions.Add(newInstruction);
					AddInstructionOffset(-3);
					optimized = true;
				}
			}

			return optimized;
		}

		private bool OptimizeOpposingInstructions()
		{
			bool optimized = false;
			Instruction<G> instruction1 = _oldInstructions[_currentIndex];
			Instruction<G> instruction2 = _oldInstructions.ElementAtOrDefault(_currentIndex + 1);
			if (((instruction1.Code == InstructionCode.PHR || instruction1.Code == InstructionCode.CPHR) && instruction2.Code == InstructionCode.PLR) ||
				(instruction1.Code == InstructionCode.B && instruction2.Code == InstructionCode.BE))
			{
				optimized = true;
				AddInstructionOffset(-2, 1);
			}
			return optimized;
		}

		private bool OptimizeCopyPushInstructions()
		{
			bool optimized = false;
			Instruction<G> instruction1 = _oldInstructions[_currentIndex];
			if (IsGetPointerOrValueCode(instruction1.Code))
			{
				Instruction<G> instruction2 = _oldInstructions.ElementAtOrDefault(_currentIndex + 1);
				Instruction<G> instruction3 = _oldInstructions.ElementAtOrDefault(_currentIndex + 2);
				if (instruction2.Code == InstructionCode.PHR
					&& instruction3.Code == instruction1.Code
					&& Enumerable.SequenceEqual(instruction1.Payload, instruction3.Payload))
				{
					_newInstructions.Add(instruction1);
					_newInstructions.Add(InstructionProvider<G>.GetInstruction(InstructionCode.CPHR, sourcePosition: instruction3.SourcePosition));
					optimized = true;
					AddInstructionOffset(-1, 2);
				}
			}
			return optimized;
		}

		private bool OptimizeMultipleGetPointersInstructions()
		{
			bool optimized = false;
			if (IsMultipleGetPointersInstructions(out bool hasPushFirst))
			{
				int offset = hasPushFirst ? 0 : -1;
				int count = 0;
				int? sourcePosition = null;
				bool interruptable = false;
				List<object> newPayload = new List<object>
				{
					_currentIndex > 0 ? offset : 0,
					null
				};

				while (true)
				{
					Instruction<G> instruction = _oldInstructions.ElementAtOrDefault(_currentIndex + count + offset + 1);
					if (!IsGetPointerOrValueCode(instruction.Code)) break;
					offset++;
					newPayload.Add(instruction.Code);
					newPayload.AddRange(instruction.Payload);
					sourcePosition = sourcePosition ?? instruction.SourcePosition;
					interruptable |= instruction.Interruptable;
					count++;

					instruction = _oldInstructions.ElementAtOrDefault(_currentIndex + count + offset);
					if (!IsPushCode(instruction.Code)) break;
				}

				newPayload[1] = count;
				offset = count + offset - 1;
				_newInstructions.Add(InstructionProvider<G>.GetInstruction(InstructionCode.CPR, payload: newPayload.ToArray(), sourcePosition: sourcePosition, interruptable: interruptable));
				AddInstructionOffset(-offset);
				optimized = true;
			}
			return optimized;
		}

		private bool IsMultipleGetPointersInstructions(out bool hasPush)
		{
			hasPush = false;
			int instructionIndex = _currentIndex;
			Instruction<G> instruction1 = _oldInstructions[instructionIndex];
			if (instruction1.Code == InstructionCode.PHR)
			{
				instruction1 = _oldInstructions.ElementAtOrDefault(++instructionIndex);
				hasPush = true;
			}
			Instruction<G> instruction2 = _oldInstructions.ElementAtOrDefault(++instructionIndex);
			Instruction<G> instruction3 = _oldInstructions.ElementAtOrDefault(++instructionIndex);
			return IsGetPointerOrValueCode(instruction1.Code) &&
				IsPushCode(instruction2.Code) &&
				IsGetPointerOrValueCode(instruction3.Code);
		}

		private bool OptimizeDuplicateInstructions()
		{
			bool optimized = false;
			Instruction<G> instruction1 = _oldInstructions[_currentIndex];
			Instruction<G> instruction2 = _oldInstructions.ElementAtOrDefault(_currentIndex + 1);
			if (instruction1.Code == instruction2.Code)
			{
				optimized = true;
				switch (instruction1.Code)
				{
					case InstructionCode.SPSP:
					case InstructionCode.SPGP:
					case InstructionCode.SPDP:
					case InstructionCode.GPSP:
					case InstructionCode.GPGP:
					case InstructionCode.GPDP:
					case InstructionCode.DPSP:
					case InstructionCode.DPGP:
					case InstructionCode.DPDP:
					case InstructionCode.VSP:
					case InstructionCode.VGP:
					case InstructionCode.VDP:
					case InstructionCode.MI:
					case InstructionCode.MP:
					case InstructionCode.CR:
					case InstructionCode.J:
					case InstructionCode.H:
					case InstructionCode.L:
					case InstructionCode.LE:
					case InstructionCode.CS:
					case InstructionCode.CSE:
					case InstructionCode.US:
						if ((instruction1.Payload is null && instruction2.Payload is null) || (instruction2.Payload != null && (instruction1.Payload?.SequenceEqual(instruction2.Payload) ?? false)))
						{
							_newInstructions.Add(instruction1);
							AddInstructionOffset(-1);
						}
						else
						{
							optimized = false;
						}
						break;
					default:
						optimized = false;
						break;
				}
			}
			return optimized;
		}

		private bool OptimizeOverridingInstructions()
		{
			bool optimized = false;
			Instruction<G> instruction1 = _oldInstructions[_currentIndex];
			Instruction<G> instruction2 = _oldInstructions.ElementAtOrDefault(_currentIndex + 1);
			if (instruction1.Code == instruction2.Code)
			{
				switch (instruction1.Code)
				{
					case InstructionCode.SPR:
					case InstructionCode.GPR:
					case InstructionCode.DPR:
					case InstructionCode.VR:
					case InstructionCode.GDR:
					case InstructionCode.HGR:
						_newInstructions.Add(instruction2);
						AddInstructionOffset(-1);
						optimized = true;
						break;
				}
			}
			return optimized;
		}

		private void AddPayloadInstructionPointers(List<InstructionPointer> instructionPointers)
		{
			string currentGroupId = "0";
			int lastGroupIndex = -1;
			int groupStartIndex = 0;
			Stack<GroupPoint> groupPoints = new Stack<GroupPoint>();
			for (int currentIndex = 0; currentIndex < _newInstructions.Count; currentIndex++)
			{
				Instruction<G> instruction = _newInstructions[currentIndex];
				switch (instruction.Code)
				{
					case InstructionCode.G:
						groupPoints.Push(new GroupPoint(currentGroupId, ++lastGroupIndex, groupStartIndex));
						currentGroupId = $"{currentGroupId}.{lastGroupIndex}";
						lastGroupIndex = -1;
						groupStartIndex = currentIndex + 1;
						break;
					case InstructionCode.GE:
						GroupPoint groupPoint = groupPoints.Pop();
						currentGroupId = groupPoint.GroupId;
						lastGroupIndex = groupPoint.LastGroupIndex;
						groupStartIndex = groupPoint.GroupStartIndex;
						break;
					case InstructionCode.CSE:
					case InstructionCode.C:
					case InstructionCode.J:
					case InstructionCode.US:
					case InstructionCode.LE:
					case InstructionCode.NC:
					case InstructionCode.AR:
						instructionPointers.Add(new InstructionPointer(instruction, currentGroupId, groupStartIndex));
						break;
				}
			}
		}

		private bool IsPushCode(InstructionCode? code)
		{
			return code == InstructionCode.PHR || code == InstructionCode.CPHR;
		}

		private bool IsGetPointerOrValueCode(InstructionCode? code)
		{
			return code >= InstructionCode.SPR && code <= InstructionCode.VR;
		}

		private bool IsCreateOrGetPointerCode(InstructionCode? code)
		{
			return code >= InstructionCode.CSP && code <= InstructionCode.DPR;
		}

		private void AddInstructionOffset(int offset)
		{
			AddInstructionOffset(offset, -offset);
		}

		private void AddInstructionOffset(int offset, int indexSkip)
		{
			_instructionOffsets.Add(new InstructionOffset(_innerIndex + _innerOffset, offset, _currentGroupId, _groupStartIndex));
			_currentIndex += indexSkip;
			_innerIndex += indexSkip;
			_innerOffset += offset;
		}

		private void PublishChanges()
		{
			List<InstructionPointer> instructionPointers = new List<InstructionPointer>();
			AddPayloadInstructionPointers(instructionPointers);

			instructionPointers.Sort();

			foreach (InstructionOffset instructionOffset in _instructionOffsets)
			{
				for (int i = instructionPointers.Count - 1; i >= 0; i--)
				{
					InstructionPointer instructionPointer = instructionPointers[i];
					if (instructionPointer.Location != null && instructionOffset.GroupId.StartsWith(instructionPointer.GroupId))
					{
						if (instructionPointer.GroupId == instructionOffset.GroupId)
						{
							if (instructionPointer.Location > instructionOffset.OffsetIndex)
							{
								instructionPointer.Location += instructionOffset.Offset;
							}
						}
						else if (instructionPointer.AbsoluteLocation >= instructionOffset.AbsoluteOffsetIndex)
						{
							instructionPointer.Location += instructionOffset.Offset;
						}
					}
				}
			}

			foreach (InstructionPointer instructionPointer in instructionPointers)
			{
				instructionPointer.UpdateSource();
			}
		}

		private readonly struct GroupPoint
		{
			public readonly string GroupId;
			public readonly int GroupStartIndex;
			public readonly int LastGroupIndex;
			public readonly int InnerIndex;
			public readonly int Offset;

			public GroupPoint(string groupId, int lastGroupIndex, int groupStartIndex, int innerIndex, int offset)
			{
				GroupId = groupId;
				GroupStartIndex = groupStartIndex;
				LastGroupIndex = lastGroupIndex;
				InnerIndex = innerIndex;
				Offset = offset;
			}

			public GroupPoint(string groupId, int lastGroupIndex, int groupStartIndex)
			{
				GroupId = groupId;
				GroupStartIndex = groupStartIndex;
				LastGroupIndex = lastGroupIndex;
				InnerIndex = 0;
				Offset = 0;
			}
		}

		private readonly struct InstructionOffset
		{
			public readonly int OffsetIndex;
			public readonly int Offset;
			public readonly string GroupId;
			public readonly int GroupStartIndex;

			public InstructionOffset(int offsetIndex, int offset, string groupId, int groupStartIndex)
			{
				OffsetIndex = offsetIndex;
				Offset = offset;
				GroupId = groupId;
				GroupStartIndex = groupStartIndex;
			}

			public int AbsoluteOffsetIndex
			{
				get
				{
					return GroupStartIndex + OffsetIndex;
				}
			}
		}

		private class InstructionPointer : IComparable<InstructionPointer>
		{
			public InstructionPointer(Instruction<G> instruction, string groupId, int groupStartIndex)
			{
				GroupId = groupId;
				Item = instruction;
				GroupStartIndex = groupStartIndex;
				int? payloadLocation = instruction.Payload[0] as int?;
				if (payloadLocation != null)
				{
					Location = payloadLocation;
				}
			}

			public Instruction<G> Item { get; private set; }

			public int? Location { get; set; }

			public int GroupStartIndex { get; private set; }

			public int? AbsoluteLocation
			{
				get
				{
					return Location != null ? GroupStartIndex + Location : null;
				}
			}

			public string GroupId { get; set; }

			public int CompareTo(InstructionPointer other)
			{
				return Location?.CompareTo(other.Location) ?? -1;
			}

			public void UpdateSource()
			{
				if (Location != null)
				{
					Item.Payload[0] = Location;
				}
			}
		}
	}
}
