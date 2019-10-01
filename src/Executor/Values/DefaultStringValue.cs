using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Exceptions;
using System.Linq;

namespace Synfron.Staxe.Executor.Values
{
	public readonly struct DefaultStringValue<G> : IValue<G, string> where G : IGroupState<G>, new()
	{
		public DefaultStringValue(string data)
		{
			Data = data;
		}

		public string Data
		{
			get;
		}

		public int Size
		{
			get
			{
				return Data?.Length ?? 0;
			}
		}

		public ValueType ValueType => ValueType.String;

		public bool HasValue => true;

		public object GetData()
		{
			return Data;
		}

		public override int GetHashCode()
		{
			return Data.GetHashCode();
		}

		public override string ToString()
		{
			return Data.ToString();
		}

		private IValuable<G> Add(string stringVal, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetString(Data + stringVal);
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

		private IValuable<G> IsEqualTo(IValue<G, string> stringValue, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data == stringValue.Data);
		}

		private IValuable<G> IsNotEqualTo(IValue<G, string> stringValue, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data != stringValue.Data);
		}

		private IValuable<G> Minus(string stringVal, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetString(Data.Replace(stringVal, ""));
		}

		public override bool Equals(object obj)
		{
			return obj is IValue<G, string> other ? Data == other.Data : false;
		}

		public IValuable<G> And(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for and");
		}

		public IValuable<G> Or(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for or");
		}

		public IValuable<G> DivideBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for division");
		}

		public IValuable<G> IsGreaterThan(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for greater than");
		}

		public IValuable<G> IsGreaterThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for greater than or equals");
		}

		public IValuable<G> IsLessThan(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for less than");
		}

		public IValuable<G> IsLessThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for less than or equals");
		}

		public IValuable<G> Minus(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, string> stringValue:
					return Minus(stringValue.Data, valueProvider);
				case IValue<G, int> intValue:
					return Minus(intValue.Data.ToString(), valueProvider);
				case IValue<G, double> doubleValue:
					return Minus(doubleValue.Data.ToString(), valueProvider);
				case IValue<G, long> longValue:
					return Minus(longValue.Data.ToString(), valueProvider);
				case IValue<G, bool> booleanValue:
					return Minus(booleanValue.Data.ToString(), valueProvider);
				case INullValue<G> _:
					return this;
				default:
					throw new EngineRuntimeException("Invalid values for subtraction");
			}
		}

		public IValuable<G> MultiplyBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for multiplication");
		}

		public IValuable<G> Remainder(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for remainder");
		}

		public IValuable<G> Add(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, string> stringValue:
					return Add(stringValue.Data, valueProvider);
				case IValue<G, int> intValue:
					return Add(intValue.Data.ToString(), valueProvider);
				case IValue<G, double> doubleValue:
					return Add(doubleValue.Data.ToString(), valueProvider);
				case IValue<G, long> longValue:
					return Add(longValue.Data.ToString(), valueProvider);
				case IValue<G, bool> booleanValue:
					return Add(booleanValue.Data.ToString().ToLower(), valueProvider);
				case ICollectionValue<G> collectionValue:
					return Add(collectionValue, valueProvider);
				case INullValue<G> _:
					return this;
				default:
					throw new EngineRuntimeException("Invalid values for addition");
			}
		}

		public IValuable<G> IsEqualTo(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			switch (valuable)
			{
				case IValue<G, string> stringValue:
					return IsEqualTo(stringValue, valueProvider);
				default:
					return valueProvider.False;
			}
		}

		public IValuable<G> IsNotEqualTo(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			switch (valuable)
			{
				case IValue<G, string> stringValue:
					return IsNotEqualTo(stringValue, valueProvider);
				default:
					return valueProvider.True;
			}
		}

		public IValuable<G> Not(IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid value for negation");
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
			IValue<G, int> index = (IValue<G, int>)value;
			return new ValuePointer<G> { Value = valueProvider.GetString(Data[index.Data].ToString()) };
		}

		public ValuePointer<G> GetAt(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			return new ValuePointer<G> { Value = valueProvider.GetString(Data[((IValue)value).ToInt()].ToString()) };
		}

		public void Deconstruct(out string data)
		{
			data = Data;
		}
	}
}
