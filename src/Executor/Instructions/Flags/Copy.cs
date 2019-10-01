using System;

namespace Synfron.Staxe.Executor.Instructions.Flags
{
	[Flags]
	public enum Copy
	{
		None = 0,
		Modifiers = 1

	}

	public static class CopyExtensions
	{
		public static bool Contains(this Copy container, Copy otherEnum)
		{
			return (container & otherEnum) != 0;
		}
	}
}
