using System;

namespace Synfron.Staxe.Executor.Instructions.Flags
{
	[Flags]
	public enum GroupMerge
	{
		None = 0,
		MapPointers = 1,
		ReverseMapInstructions = 2,
		OverridePointers = 4,
		AsDependency = 8

	}

	public static class GroupMergeExtensions
	{
		public static bool Contains(this GroupMerge container, GroupMerge otherEnum)
		{
			return (container & otherEnum) != 0;
		}
	}
}