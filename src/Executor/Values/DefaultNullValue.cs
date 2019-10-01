using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Exceptions;
using System.Linq;

namespace Synfron.Staxe.Executor.Values
{
	public readonly struct DefaultNullValue<G> : INullValue<G> where G : IGroupState<G>, new()
	{

		public ValueType ValueType => ValueType.Null;

		public bool HasValue => true;

		public int Size
		{
			get
			{
				throw new EngineRuntimeException("Value does not provide a size");
			}
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override string ToString()
		{
			return null;
		}

		public object GetData()
		{
			return null;
		}

		public override bool Equals(object obj)
		{
			return obj is INullValue<G>;
		}

		public IValuable<G> Add(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case ICollectionValue<G> collectionValue:
					return Add(collectionValue, valueProvider);
				default:
					throw new EngineRuntimeException("Invalid values for addition");
			}
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

		public IValuable<G> And(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for and");
		}

		public IValuable<G> DivideBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for division");
		}

		public IValuable<G> IsEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case INullValue<G> _:
					return valueProvider.True;
				default:
					return valueProvider.False;
			}
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

		public IValuable<G> IsNotEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			switch (value)
			{
				case INullValue<G> _:
					return valueProvider.False;
				default:
					return valueProvider.True;
			}
		}

		public IValuable<G> Minus(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for subtraction");
		}

		public IValuable<G> MultiplyBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for multiplication");
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
	}
}
