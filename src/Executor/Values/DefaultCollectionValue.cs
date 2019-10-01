using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Exceptions;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor.Values
{

	public abstract class DefaultCollectionValue<G> : ICollectionValue<G> where G : IGroupState<G>, new()
	{

		public bool IsMap
		{
			get;
			protected set;
		}

		public int Mode
		{
			get;
			private set;
		}

		public abstract int Size
		{
			get;
		}

		public ValueType ValueType => ValueType.Collection;

		public abstract ValuePointer<G> GetAt(IValuable<G> value, IValueProvider<G> valueProvider);

		public abstract ValuePointer<G> Get(IValuable<G> key, bool createNonExistent, IValueProvider<G> valueProvider);

		public abstract void Remove(IValuable<G> key, IValueProvider<G> valueProvider);

		public abstract void FinalizeEntry(IValuable<G> key, EntryValuePointer<G> entryValuePointer);

		public abstract List<KeyValuePair<IValuable<G>, ValuePointer<G>>> GetEntries();

		public abstract List<ValuePointer<G>> GetValues();

		public abstract List<IValuable<G>> GetKeys();

		public abstract bool ContainsKey(IValuable<G> key, IValueProvider<G> valueProvider);

		public abstract bool ContainsValue(IValuable<G> valuable, IValueProvider<G> valueProvider);

		public abstract int IndexOf(IValuable<G> valuable, IValueProvider<G> valueProvider);

		public abstract IValuable<G> KeyOf(IValuable<G> valuable, IValueProvider<G> valueProvider);

		public IValuable<G> Add(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for addition");
		}

		public IValuable<G> Minus(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Invalid values for subtraction");
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
			return valueProvider.GetBoolean(Equals(value));
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
			return valueProvider.GetBoolean(!Equals(value));
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

		public bool IsTrue()
		{
			throw new EngineRuntimeException("Value is not a boolean");
		}

		public void Execute(ExecutionState<G> executionState, IValueProvider<G> valueProvider)
		{
			throw new EngineRuntimeException("Value is not executable");
		}
	}
}
