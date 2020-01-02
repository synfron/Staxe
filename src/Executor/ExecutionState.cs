using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Interrupts;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Collections;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor
{
	public class ExecutionState<G> where G : IGroupState<G>, new()
	{
		private int _instructionIndex = 0;
		private Instruction<G>[] _instructions;
		private G _groupState;
		private bool _executable = true;
		private HashSet<IInterrupt<G>> _interrupts = new HashSet<IInterrupt<G>>();
		private StackList<ValuePointer<G>> _stackRegister;
		private StackList<StackValuePointer<G>> _stackPointers;

		public ExecutionState(G groupState)
		{
			Frame<G> frame = new Frame<G> { GroupState = groupState };
			Frames.Add(frame);

			Sync(frame);
		}

		public ExecutionState()
		{

		}


		public List<ValuePointer<G>> ListRegister
		{
			get;
		} = new List<ValuePointer<G>>();


		public StackList<Frame<G>> Frames
		{
			get;
		} = new StackList<Frame<G>>() { MaxSize = 10000 };

		public ref HashSet<IInterrupt<G>> Interrupts
		{
			get => ref _interrupts;
		}

		public ref StackList<ValuePointer<G>> StackRegister
		{
			get => ref _stackRegister;
		}

		public ref StackList<StackValuePointer<G>> StackPointers
		{
			get => ref _stackPointers;
		}

		public G GroupState
		{
			get
			{
				return _groupState;
			}
			set
			{
				_groupState = value;
				_instructions = _groupState?.Group?.Instructions;
			}
		}

		public ref int InstructionIndex
		{
			get
			{
				return ref _instructionIndex;
			}
		}

		public ref bool Executable
		{
			get
			{
				return ref _executable;
			}
		}

		public ref Instruction<G>[] Instructions
		{
			get
			{
				return ref _instructions;
			}
		}

		public Frame<G> LastFrame
		{
			get; set;
		}

		public void Sync(Frame<G> frame)
		{
			GroupState = frame.GroupState;
			_stackPointers = frame.StackPointers;
			_stackRegister = frame.StackRegister;
			_instructionIndex = frame.PreviousInstructionIndex;
			LastFrame = frame;
		}
	}
}
