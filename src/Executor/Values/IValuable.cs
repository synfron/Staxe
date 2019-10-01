using Synfron.Staxe.Executor.Pointers;

namespace Synfron.Staxe.Executor.Values
{
	public interface IValuable<G> : IValuable where G : IGroupState<G>, new()
	{
		IValuable<G> Add(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> Minus(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> And(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> Or(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> Not(IValueProvider<G> valueProvider);
		IValuable<G> MultiplyBy(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> DivideBy(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> Remainder(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> IsEqualTo(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> IsNotEqualTo(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> IsGreaterThan(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> IsLessThan(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> IsGreaterThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> IsLessThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> LeftShift(IValuable<G> value, IValueProvider<G> valueProvider);
		IValuable<G> RightShift(IValuable<G> value, IValueProvider<G> valueProvider);
		void Execute(ExecutionState<G> executionState, IValueProvider<G> valueProvider);
		ValuePointer<G> Get(IValuable<G> value, bool createNonExistent, IValueProvider<G> valueProvider);
		ValuePointer<G> GetAt(IValuable<G> value, IValueProvider<G> valueProvider);
		bool IsTrue();

	}

	public interface IValuable
	{
		ValueType ValueType
		{
			get;
		}

		int Size
		{
			get;
		}
	}
}