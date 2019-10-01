using Synfron.Staxe.Executor.Values;
using G = ExecutorTests.TestMocks.MockableGroupState;

namespace ExecutorTests.Shared
{
	public static class Extensions
	{
		public static IValue<G> GetAsValue(this object data)
		{
			switch (data)
			{
				case null:
					return new DefaultNullValue<G>();
				case int dataInt:
					return new DefaultIntValue<G>(dataInt);
				case bool dataBool:
					return new DefaultBooleanValue<G>(dataBool);
				case double dataDouble:
					return new DefaultDoubleValue<G>(dataDouble);
				case long dataLong:
					return new DefaultLongValue<G>(dataLong);
				default:
					return new DefaultStringValue<G>(data.ToString());
			}
		}
	}
}
