using Synfron.Staxe.Executor.Values;
using G = Synfron.Staxe.Executor.GroupState;

namespace StaxeTests.TestSimpleLang.Engine.Executor.Values
{
	public sealed class ValueProvider : DefaultValueProvider<G>
	{

		public override IValue<G> GetAsValue(object data)
		{
			switch (data)
			{
				case null:
					return Null;
				case int dataInt:
					return GetDouble(dataInt);
				case bool dataBool:
					return GetBoolean(dataBool);
				case double dataDouble:
					return GetDouble(dataDouble);
				case long dataLong:
					return GetDouble(dataLong);
				case string dataString:
					return GetString(dataString);
				default:
					return GetExternal(data);
			}
		}

		public override IValue<G> GetInt(int data)
		{
			return GetDouble(data);
		}

		public override IValue<G> GetLong(long data)
		{
			return GetDouble(data);
		}

		public override IValue<G> GetReducedValue(long value)
		{
			return GetDouble(value);
		}

		public override IValue<G> GetReducedValue(double value)
		{
			return GetDouble(value);
		}
	}
}
