using System;

namespace Synfron.Staxe.Executor.Values
{
	[Flags]
	public enum ValueType
	{
		Null = 0, Integer = 1, Long = 2, Double = 4, String = 8, Boolean = 16, Action = 32, Collection = 64, Group = 128, External = 256
	}
}
