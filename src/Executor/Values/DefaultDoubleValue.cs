using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Exceptions;
using System.Linq;

namespace Synfron.Staxe.Executor.Values
{
	public readonly struct DefaultDoubleValue<G> : IValue<G, double> where G : IGroupState<G>, new()
	{
		public DefaultDoubleValue(double data)
		{
			Data = data;
		}

		public double Data
		{
			get;
		}

		public int Size
		{
			get
			{
				throw new EngineRuntimeException("Value does not provide a size");
			}
		}

		public ValueType ValueType => ValueType.Double;

		public bool HasValue => true;

		public override int GetHashCode()
		{
			return Data.GetHashCode();
		}

		public override string ToString()
		{
			return Data.ToString();
		}

		public object GetData()
		{
			return Data;
		}

		private IValuable<G> Add(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data + value);
		}

		private IValuable<G> Add(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data + value);
		}

		private IValuable<G> Add(string value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetString(Data + value);
		}

		private IValuable<G> Add(ICollectionValue<G> collectionValue, IValueProvider<G> valueProvider)
		{
			IValuable<G> newValuable = null;
			switch (collectionValue)
			{
				case DefaultCollectionValue<G> listValue when !listValue.IsMap:
					newValuable = valueProvider.GetCollection(new[] { new ValuePointer<G> { Value = this } }.Concat(collectionValue.GetEntries().Select(entry => entry.Value)), null);
					break;
				default:
					throw new EngineRuntimeException("Invalid values for addition");
			}
			return newValuable;
		}

		private IValuable<G> DivideBy(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data / value);
		}

		private IValuable<G> IsEqualTo(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data == value);
		}

		private IValuable<G> IsEqualTo(int value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data == value);
		}

		private IValuable<G> IsEqualTo(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data == value);
		}

		private IValuable<G> IsGreaterThan(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data > value);
		}

		private IValuable<G> IsGreaterThan(int value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data > value);
		}

		private IValuable<G> IsGreaterThan(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data > value);
		}

		private IValuable<G> IsGreaterThanOrEqualTo(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data >= value);
		}

		private IValuable<G> IsGreaterThanOrEqualTo(int value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data >= value);
		}

		private IValuable<G> IsGreaterThanOrEqualTo(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data >= value);
		}

		private IValuable<G> IsLessThan(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data < value);
		}

		private IValuable<G> IsLessThan(int value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data < value);
		}

		private IValuable<G> IsLessThan(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data < value);
		}

		private IValuable<G> IsLessThanOrEqualTo(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data <= value);
		}

		private IValuable<G> IsLessThanOrEqualTo(int value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data <= value);
		}

		private IValuable<G> IsLessThanOrEqualTo(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data <= value);
		}

		private IValuable<G> IsNotEqualTo(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data != value);
		}

		private IValuable<G> IsNotEqualTo(int value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data != value);
		}

		private IValuable<G> IsNotEqualTo(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data != value);
		}

		private IValuable<G> Minus(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data - value);
		}

		private IValuable<G> Minus(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data - value);
		}

		private IValuable<G> MultiplyBy(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data * value);
		}

		private IValuable<G> MultiplyBy(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data * value);
		}

		private IValuable<G> Remainder(double value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data % value);
		}

		private IValuable<G> Remainder(int value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data % value);
		}

		private IValuable<G> Remainder(long value, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetReducedValue(Data % value);
		}

		public override bool Equals(object obj)
		{
			return obj is IValue<G, double> other ? Data == other.Data : false;
		}

		public IValuable<G> Add(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return Add(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return Add(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return Add(longValue.Data, valueProvider);
				case IValue<G, string> stringValue:
					return Add(stringValue.Data, valueProvider);
				case ICollectionValue<G> collectionValue:
					return Add(collectionValue, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for addition");
			}
		}

		public IValuable<G> And(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for and");
		}

		public IValuable<G> DivideBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return DivideBy(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return DivideBy(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return DivideBy(longValue.Data, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for division");
			}
		}

		public IValuable<G> IsEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return IsEqualTo(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return IsEqualTo(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return IsEqualTo(longValue.Data, valueProvider);
				default:
					return valueProvider.False;
			}
		}

		public IValuable<G> IsGreaterThan(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return IsGreaterThan(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return IsGreaterThan(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return IsGreaterThan(longValue.Data, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for greater than");
			}
		}

		public IValuable<G> IsGreaterThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return IsGreaterThanOrEqualTo(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return IsGreaterThanOrEqualTo(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return IsGreaterThanOrEqualTo(longValue.Data, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for greater than or equals");
			}
		}

		public IValuable<G> IsLessThan(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return IsLessThan(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return IsLessThan(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return IsLessThan(longValue.Data, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for less than");
			}
		}

		public IValuable<G> IsLessThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return IsLessThanOrEqualTo(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return IsLessThanOrEqualTo(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return IsLessThanOrEqualTo(longValue.Data, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for less than or equals");
			}
		}

		public IValuable<G> IsNotEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return IsNotEqualTo(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return IsNotEqualTo(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return IsNotEqualTo(longValue.Data, valueProvider);
				default:
					return valueProvider.True;
			}
		}

		public IValuable<G> Minus(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return Minus(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return Minus(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return Minus(longValue.Data, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for subtraction");
			}
		}

		public IValuable<G> MultiplyBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return MultiplyBy(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return MultiplyBy(doubleValue.Data, valueProvider);
				case IValue<G, long> intValue:
					return MultiplyBy(intValue.Data, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for multiplication");
			}
		}

		public IValuable<G> Not(IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid value for negation");
		}

		public IValuable<G> Or(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for or");
		}

		public IValuable<G> Remainder(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, int> intValue:
					return Remainder(intValue.Data, valueProvider);
				case IValue<G, double> doubleValue:
					return Remainder(doubleValue.Data, valueProvider);
				case IValue<G, long> longValue:
					return Remainder(longValue.Data, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for remainder");
			}
		}

		public IValuable<G> LeftShift(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for left shift");
		}

		public IValuable<G> RightShift(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for right shift");
		}

		public void Execute(ExecutionState<G> executionState, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Value is not executable");
		}

		public bool IsTrue()
		{
			throw new EngineRuntimeException("Value is not a boolean");
		}

		public ValuePointer<G> Get(IValuable<G> value, bool createNonExistent, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Value is not keyed");
		}

		public ValuePointer<G> GetAt(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Value is not indexed");
		}

		public void Deconstruct(out double data)
		{
			data = Data;
		}
	}
}
