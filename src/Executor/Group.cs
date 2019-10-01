using Synfron.Staxe.Executor.Instructions;
using System;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor
{
	public sealed class Group<G> where G : IGroupState<G>, new()
	{
		private Instruction<G>[] _instructions;


		public string GroupName
		{
			get;
			set;
		}

		public ref Instruction<G>[] Instructions
		{
			get
			{
				return ref _instructions;
			}
		}

		public Dictionary<string, int> InstructionMap
		{
			get;
			private set;
		} = new Dictionary<string, int>(StringComparer.Ordinal);
	}
}
