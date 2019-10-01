using System;

namespace Synfron.Staxe.Executor.Instructions.Flags
{
	[Flags]
	public enum Modifiers
	{
		None = 0,
		/// <summary>Restrict derived groups</summary>
		Static = 1,
		/// <summary>Restrict getting value</summary>
		ReadRestricted = 2,
		/// <summary>Restrict setting value</summary>
		WriteRestricted = 4,
		/// <summary>Restrict executing</summary>
		ExecuteRestricted = 8,
		/// <summary>Is clonable value</summary>
		Component = 16
	}

	public static class ModifiersExtensions
	{
		public static bool Contains(this Modifiers container, Modifiers otherEnum)
		{
			return (container & otherEnum) != 0;
		}
	}
}
