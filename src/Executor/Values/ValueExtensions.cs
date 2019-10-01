using System;

namespace Synfron.Staxe.Executor.Values
{
	public static class ValueExtensions
	{

		public static int ToInt(this IValue value)
		{
			return Convert.ToInt32(value.GetData());
		}
	}
}