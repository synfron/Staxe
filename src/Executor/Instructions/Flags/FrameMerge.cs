using System;

namespace Synfron.Staxe.Executor.Instructions.Flags
{
	[Flags]
	public enum FrameMerge
	{
		None = 0,
		Stack = 1,
		Register = 2
	}

	public static class FrameMergeExtensions
	{
		public static bool Contains(this FrameMerge container, FrameMerge otherEnum)
		{
			return (container & otherEnum) != 0;
		}
	}
}