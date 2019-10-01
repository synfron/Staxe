using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Exceptions;
using System.Linq;

namespace Synfron.Staxe.Executor.Values
{
	public readonly struct DefaultBooleanValue<G> : IValue<G, bool> where G : IGroupState<G>, new()
	{
		public DefaultBooleanValue(bool data)
		{
			Data = data;
		}

		public bool Data
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

		public ValueType ValueType => ValueType.Boolean;

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

		public IValuable<G> Add(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, string> stringValue:
					return Add(stringValue, valueProvider);
				case ICollectionValue<G> collectionValue:
					return Add(collectionValue, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for addition");
			}
		}

		public IValuable<G> IsEqualTo(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			switch (valuable)
			{
				case IValue<G, bool> booleanValue:
					return IsEqualTo(booleanValue, valueProvider);
				default:
					return valueProvider.False;
			}
		}

		public IValuable<G> IsNotEqualTo(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			switch (valuable)
			{
				case IValue<G, bool> booleanValue:
					return IsNotEqualTo(booleanValue, valueProvider);
				default:
					return valueProvider.True;
			}
		}

		public IValuable<G> Not(IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(!Data);
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

		private IValuable<G> Add(IValue<G, string> stringValue, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetString(Data.ToString().ToLower() + stringValue.Data);
		}

		private IValuable<G> And(IValue<G, bool> booleanValue, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data & booleanValue.Data);
		}

		private IValuable<G> IsEqualTo(IValue<G, bool> booleanValue, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data == booleanValue.Data);
		}

		private IValuable<G> IsNotEqualTo(IValue<G, bool> booleanValue, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data != booleanValue.Data);
		}

		private IValuable<G> Or(IValue<G, bool> booleanValue, IValueProvider<G> valueProvider)
		{
			return valueProvider.GetBoolean(Data | booleanValue.Data);
		}

		public IValuable<G> And(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, bool> booleanValue:
					return And(booleanValue, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for and");
			}
		}

		public IValuable<G> Or(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case IValue<G, bool> booleanValue:
					return Or(booleanValue, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for or");
			}
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
			throw new EngineRuntimeException("Invalid values for subtraction");
		}

		public IValuable<G> MultiplyBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for multiplication");
		}

		public IValuable<G> Remainder(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for remainder");
		}

		public IValuable<G> LeftShift(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for left shift");
		}

		public IValuable<G> RightShift(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for right shift");
		}

		public bool IsTrue()
		{
			return Data;
		}

		public override bool Equals(object obj)
		{
			return obj is IValue<G, bool> other ? Data == other.Data : false;
		}

		public void Execute(ExecutionState<G> executionState, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Value is not executable");
		}

		public ValuePointer<G> Get(IValuable<G> value, bool createNonExistent, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Value is not keyed");
		}

		public ValuePointer<G> GetAt(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Value is not indexed");
		}

		public void Deconstruct(out bool data)
		{
			data = Data;
		}
	}
}
