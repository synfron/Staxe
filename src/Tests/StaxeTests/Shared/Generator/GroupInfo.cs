using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using System.Collections.Generic;

namespace StaxeTests.Shared.Generator
{
	public class GroupInfo
	{
		public string ShortName
		{
			get;
			set;
		}
		public string Name
		{
			get;
			set;
		}

		public bool IsStatic
		{
			get;
			set;
		}

		public bool IsSubClass
		{
			get;
			set;
		}

		public NamespaceInfo Namespace
		{
			get;
			set;
		}

		public Dictionary<InstructionCode, List<int>> ReprocessRequiredIndexes
		{
			get;
			set;
		} = new Dictionary<InstructionCode, List<int>>();

		public List<Instruction<GroupState>> Instructions
		{
			get;
			set;
		} = new List<Instruction<GroupState>>();

		public Dictionary<string, int> Pointers
		{
			get;
			set;
		} = new Dictionary<string, int>();

		public Dictionary<string, int> Actions
		{
			get;
			set;
		} = new Dictionary<string, int>();

		public Dictionary<string, int> Dependencies
		{
			get;
			set;
		} = new Dictionary<string, int>();

		public int AddOrGetDependency(string name)
		{
			if (!Dependencies.TryGetValue(name, out int location))
			{
				location = Dependencies.Count;
				Dependencies.Add(name, location);
			}
			return location;
		}
	}
}