using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Linq;
using ValueType = Synfron.Staxe.Executor.Values.ValueType;

namespace StaxeTests.TestComplexLang.Engine.Executor.Values
{
	public class ExternalValue<G> : IValue<G, object> where G : IGroupState<G>, new()
	{
		public ExternalValue(object data)
		{
			Data = data;
		}

		public object Data { get; }

		public bool HasValue => true;

		public ValueType ValueType => ValueType.External;

		public int Size => GetSize();

		private int GetSize()
		{
			dynamic array = Data;
			return array.Count;
		}

		public IValuable<G> Add(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for addition";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue + dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> And(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for and";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue & dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public void Deconstruct(out object data)
		{
			data = Data;
		}

		public IValuable<G> DivideBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for division";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue / dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public void Execute(ExecutionState<G> executionState, IValueProvider<G> valueProvider)
		{
			object[] parameters = executionState.ListRegister.Select(pointer => ((IValue<G>)pointer.Value).GetData()).ToArray();
			executionState.ListRegister.Clear();
			dynamic thisValue = Data;
			executionState.ListRegister.Add(new ValuePointer<G> { Value = valueProvider.GetAsValue(thisValue.DynamicInvoke(parameters)) });


		}

		public ValuePointer<G> Get(IValuable<G> value, bool createNonExistent, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Value is not keyed";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => new ValuePointer<G> { Value = valueProvider.GetAsValue(thisValue[dynamicValue]) }, exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public ValuePointer<G> GetAt(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Value is not indexed";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => new ValuePointer<G> { Value = valueProvider.GetAsValue(Enumerable.ElementAt(thisValue, dynamicValue)) }, exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public object GetData()
		{
			return Data;
		}

		public IValuable<G> IsEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for equals";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue == dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> IsGreaterThan(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for greater than";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue > dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> IsGreaterThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for greater than or equals";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue >= dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> IsLessThan(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for less than";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue < dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> IsLessThanOrEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for less than or equals";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue <= dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> IsNotEqualTo(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for not equals";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue != dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public bool IsTrue()
		{
			throw new EngineRuntimeException("Value is not a boolean");
		}

		public IValuable<G> LeftShift(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for left shift";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue << dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> Minus(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for subtraction";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue - dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> MultiplyBy(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for multiplication";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue * dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> Not(IValueProvider<G> valueProvider)
		{
			dynamic thisValue = Data;
			return EvaluateOrThrow(() => valueProvider.GetAsValue(!thisValue), "Invalid value for negation");
		}

		public IValuable<G> Or(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for or";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue | dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		public IValuable<G> Remainder(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for remainder";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue % dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException("Invalid values for remainder");
			}
		}

		public IValuable<G> RightShift(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			string exceptionMessage = "Invalid values for right shift";
			switch (value)
			{
				case IValue<G> val:
					dynamic thisValue = Data;
					dynamic dynamicValue = val.GetData();
					return EvaluateOrThrow(() => valueProvider.GetAsValue(thisValue >> dynamicValue), exceptionMessage);
				default:
					throw new EngineRuntimeException(exceptionMessage);
			}
		}

		private T EvaluateOrThrow<T>(Func<T> func, string exceptionMessage)
		{
			try
			{
				return func();
			}
			catch
			{
				throw new EngineRuntimeException(exceptionMessage);
			}
		}
	}
}
