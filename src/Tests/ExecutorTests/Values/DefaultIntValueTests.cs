using ExecutorTests.Shared;
using Moq;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using G = ExecutorTests.TestMocks.MockableGroupState;

namespace ExecutorTests.Values
{
	public class DefaultIntValueTests
	{
		[Fact]
		public void DefaultIntValue_Add_MapCollection()
		{
			int value = 104;
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
			};
			DefaultArrayMapValue<G> collectionValue = new DefaultArrayMapValue<G>(collectionValues, valueProvider);

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			Assert.Throws<EngineRuntimeException>(() =>
			{
				sut.Add(collectionValue, valueProvider);
			});
		}

		[Fact]
		public void DefaultIntValue_Add_NonMapCollection()
		{
			int value = 104;
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> collectionValue = new DefaultArrayMapValue<G>(collectionValues, valueProvider);

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			DefaultCollectionValue<G> result = (DefaultCollectionValue<G>)sut.Add(collectionValue, valueProvider);

			List<KeyValuePair<IValuable<G>, ValuePointer<G>>> entries = result.GetEntries();
			Assert.Equal(Enumerable.Range(0, 4), entries.Select(kv => kv.Key).Cast<IValue>().Select(v => Convert.ToInt32(v.GetData())));
			Assert.Equal(new IValuable<G>[] { sut }.Concat(collectionValues.Values.Select(v => v.Value)), entries.Select(kv => kv.Value).Select(vp => vp.Value));
			Assert.False(result.IsMap);
			Assert.Equal(Enumerable.Range(0, 4), entries.Select(e => e.Value).Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Theory]
		[InlineData(104, 104, typeof(int), false)]
		[InlineData(1046775, 105887568, typeof(int), false)]
		[InlineData(104, 1058L, typeof(int), false)]
		[InlineData(104, 56.5D, typeof(double), false)]
		[InlineData(104, 56744806738786.575D, typeof(double), false)]
		[InlineData(104, "hello", typeof(string), false)]
		[InlineData(104, 10, typeof(int), false)]
		[InlineData(104, 56D, typeof(int), false)]
		[InlineData(104, 105810581058L, typeof(long), false)]
		[InlineData(104, 56744806738786D, typeof(long), false)]
		[InlineData(454, true, typeof(long), true)]
		[InlineData(454, null, typeof(long), true)]
		public void DefaultIntValue_Add(int value, object otherVal, Type resultType, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.Add(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}

			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(Convert.ChangeType(value + doubleVal, resultType), newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(Convert.ChangeType(value + intVal, resultType), newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(Convert.ChangeType(value + longVal, resultType), newValue.GetData());
						break;
					case string strVal:
						Assert.Equal(Convert.ChangeType(value + strVal, resultType), newValue.GetData());
						break;
					case null:
						Assert.Equal(sut, newValue);
						break;

				}
			}
		}

		[Theory]
		[InlineData(104, 104, typeof(int), false)]
		[InlineData(1046775, 105887568, typeof(int), false)]
		[InlineData(104, 1058L, typeof(int), false)]
		[InlineData(104, 56.5D, typeof(double), true)]
		[InlineData(104, 56744806738786.575D, typeof(double), true)]
		[InlineData(104, "hello", typeof(string), true)]
		[InlineData(104, 10, typeof(int), false)]
		[InlineData(104, 56D, typeof(int), true)]
		[InlineData(104, 105810581058L, typeof(int), false)]
		[InlineData(104, 56744806738786D, typeof(int), true)]
		[InlineData(454, true, typeof(long), true)]
		[InlineData(454, null, typeof(long), true)]
		public void DefaultIntValue_And(int value, object otherVal, Type resultType, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.And(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) & Convert.ToInt64(doubleVal), resultType), newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) & Convert.ToInt64(intVal), resultType), newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) & Convert.ToInt64(longVal), resultType), newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, typeof(int), false)]
		[InlineData(1046775, 105887568, typeof(double), false)]
		[InlineData(104, 1058L, typeof(double), false)]
		[InlineData(104, 56.5D, typeof(double), false)]
		[InlineData(104, 56744806738786.575D, typeof(double), false)]
		[InlineData(104, "hello", typeof(string), true)]
		[InlineData(104, 10, typeof(double), false)]
		[InlineData(104, 56D, typeof(double), false)]
		[InlineData(104, 105810581058L, typeof(double), false)]
		[InlineData(104, 56744806738786D, typeof(double), false)]
		[InlineData(454, true, typeof(long), true)]
		[InlineData(454, null, typeof(long), true)]
		public void DefaultIntValue_DivideBy(int value, object otherVal, Type resultType, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.DivideBy(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(Convert.ChangeType(Convert.ToDouble(value) / doubleVal, resultType), newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(Convert.ChangeType(Convert.ToDouble(value) / intVal, resultType), newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(Convert.ChangeType(Convert.ToDouble(value) / longVal, resultType), newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, false)]
		[InlineData(1046775, 105887568, false)]
		[InlineData(104, 1058L, false)]
		[InlineData(104, 56.5D, false)]
		[InlineData(104, 56744806738786.575D, false)]
		[InlineData(104, "hello", false)]
		[InlineData(104, 10, false)]
		[InlineData(104, 56D, false)]
		[InlineData(104, 105810581058L, false)]
		[InlineData(104, 56744806738786D, false)]
		[InlineData(454, true, false)]
		[InlineData(454, null, false)]
		public void DefaultIntValue_IsEqualTo(int value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.IsEqualTo(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(value == doubleVal, newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(value == intVal, newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(value == longVal, newValue.GetData());
						break;
					default:
						Assert.False((bool)newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, false)]
		[InlineData(1046775, 105887568, false)]
		[InlineData(104, 1058L, false)]
		[InlineData(104, 56.5D, false)]
		[InlineData(104, 56744806738786.575D, false)]
		[InlineData(104, "hello", true)]
		[InlineData(104, 10, false)]
		[InlineData(104, 56D, false)]
		[InlineData(104, 105810581058L, false)]
		[InlineData(104, 56744806738786D, false)]
		[InlineData(454, true, true)]
		[InlineData(454, null, true)]
		public void DefaultIntValue_IsGreaterThan(int value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.IsGreaterThan(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(value > doubleVal, newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(value > intVal, newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(value > longVal, newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, false)]
		[InlineData(1046775, 105887568, false)]
		[InlineData(104, 1058L, false)]
		[InlineData(104, 56.5D, false)]
		[InlineData(104, 56744806738786.575D, false)]
		[InlineData(104, "hello", true)]
		[InlineData(104, 10, false)]
		[InlineData(104, 56D, false)]
		[InlineData(104, 105810581058L, false)]
		[InlineData(104, 56744806738786D, false)]
		[InlineData(454, true, true)]
		[InlineData(454, null, true)]
		public void DefaultIntValue_IsGreaterThanOrEqualTo(int value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.IsGreaterThanOrEqualTo(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(value >= doubleVal, newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(value >= intVal, newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(value >= longVal, newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, false)]
		[InlineData(1046775, 105887568, false)]
		[InlineData(104, 1058L, false)]
		[InlineData(104, 56.5D, false)]
		[InlineData(104, 56744806738786.575D, false)]
		[InlineData(104, "hello", true)]
		[InlineData(104, 10, false)]
		[InlineData(104, 56D, false)]
		[InlineData(104, 105810581058L, false)]
		[InlineData(104, 56744806738786D, false)]
		[InlineData(454, true, true)]
		[InlineData(454, null, true)]
		public void DefaultIntValue_IsLessThan(int value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.IsLessThan(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(value < doubleVal, newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(value < intVal, newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(value < longVal, newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, false)]
		[InlineData(1046775, 105887568, false)]
		[InlineData(104, 1058L, false)]
		[InlineData(104, 56.5D, false)]
		[InlineData(104, 56744806738786.575D, false)]
		[InlineData(104, "hello", true)]
		[InlineData(104, 10, false)]
		[InlineData(104, 56D, false)]
		[InlineData(104, 105810581058L, false)]
		[InlineData(104, 56744806738786D, false)]
		[InlineData(454, true, true)]
		[InlineData(454, null, true)]
		public void DefaultIntValue_IsLessThanOrEqualTo(int value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.IsLessThanOrEqualTo(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(value <= doubleVal, newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(value <= intVal, newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(value <= longVal, newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, false)]
		[InlineData(1046775, 105887568, false)]
		[InlineData(104, 1058L, false)]
		[InlineData(104, 56.5D, false)]
		[InlineData(104, 56744806738786.575D, false)]
		[InlineData(104, "hello", false)]
		[InlineData(104, 10, false)]
		[InlineData(104, 56D, false)]
		[InlineData(104, 105810581058L, false)]
		[InlineData(104, 56744806738786D, false)]
		[InlineData(454, true, false)]
		[InlineData(454, null, false)]
		public void DefaultIntValue_IsNotEqualTo(int value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.IsNotEqualTo(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(value != doubleVal, newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(value != intVal, newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(value != longVal, newValue.GetData());
						break;
					default:
						Assert.True((bool)newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, typeof(long), false)]
		[InlineData(1046775, 105887568, typeof(long), false)]
		[InlineData(104, 1058L, typeof(long), true)]
		[InlineData(104, 56.5D, typeof(double), true)]
		[InlineData(104, 56744806738786.575D, typeof(double), true)]
		[InlineData(104, "hello", typeof(string), true)]
		[InlineData(104, 10, typeof(int), false)]
		[InlineData(104, 56D, typeof(long), true)]
		[InlineData(104, 105810581058L, typeof(long), true)]
		[InlineData(104, 56744806738786D, typeof(long), true)]
		[InlineData(454, true, typeof(long), true)]
		[InlineData(454, null, typeof(long), true)]
		public void DefaultIntValue_LeftShift(int value, object otherVal, Type resultType, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.LeftShift(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) << Convert.ToInt32(doubleVal), resultType), newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) << Convert.ToInt32(intVal), resultType), newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) << Convert.ToInt32(longVal), resultType), newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, typeof(int), false)]
		[InlineData(1046775, 105887568, typeof(int), false)]
		[InlineData(104, 1058L, typeof(int), false)]
		[InlineData(104, 56.5D, typeof(double), false)]
		[InlineData(104, 56744806738786.575D, typeof(double), false)]
		[InlineData(104, "hello", typeof(string), true)]
		[InlineData(104, 10, typeof(int), false)]
		[InlineData(104, 56D, typeof(int), false)]
		[InlineData(104, 105810581058L, typeof(long), false)]
		[InlineData(104, 56744806738786D, typeof(long), false)]
		[InlineData(454, true, typeof(long), true)]
		[InlineData(454, null, typeof(long), true)]
		public void DefaultIntValue_Minus(int value, object otherVal, Type resultType, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.Minus(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(Convert.ChangeType(value - doubleVal, resultType), newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(Convert.ChangeType(value - intVal, resultType), newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(Convert.ChangeType(value - longVal, resultType), newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, typeof(int), false)]
		[InlineData(1046775, 105887568, typeof(long), false)]
		[InlineData(104, 1058L, typeof(int), false)]
		[InlineData(104, 56.5D, typeof(int), false)]
		[InlineData(104, 56744806738786.575D, typeof(long), false)]
		[InlineData(104, "hello", typeof(string), true)]
		[InlineData(104, 10, typeof(int), false)]
		[InlineData(104, 56D, typeof(int), false)]
		[InlineData(104, 105810581058L, typeof(long), false)]
		[InlineData(104, 56744806738786D, typeof(long), false)]
		[InlineData(454, true, typeof(long), true)]
		[InlineData(454, null, typeof(long), true)]
		public void DefaultIntValue_MultiplyBy(int value, object otherVal, Type resultType, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.MultiplyBy(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(Convert.ChangeType(value * doubleVal, resultType), newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(Convert.ChangeType((long)value * intVal, resultType), newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(Convert.ChangeType(value * longVal, resultType), newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, typeof(int), false)]
		[InlineData(1046775, 105887568, typeof(int), false)]
		[InlineData(104, 1058L, typeof(int), false)]
		[InlineData(104, 56.5D, typeof(double), true)]
		[InlineData(104, 56744806738786.575D, typeof(double), true)]
		[InlineData(104, "hello", typeof(string), true)]
		[InlineData(104, 10, typeof(int), false)]
		[InlineData(104, 56D, typeof(int), true)]
		[InlineData(104, 105810581058L, typeof(long), false)]
		[InlineData(104, 56744806738786D, typeof(long), true)]
		[InlineData(454, true, typeof(long), true)]
		[InlineData(454, null, typeof(long), true)]
		public void DefaultIntValue_Or(int value, object otherVal, Type resultType, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.Or(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) | Convert.ToInt64(doubleVal), resultType), newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) | Convert.ToInt64(intVal), resultType), newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) | Convert.ToInt64(longVal), resultType), newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, typeof(int), false)]
		[InlineData(1046775, 105887568, typeof(int), false)]
		[InlineData(104, 1058L, typeof(int), false)]
		[InlineData(104, 56.5D, typeof(double), false)]
		[InlineData(104, 56744806738786.575D, typeof(int), false)]
		[InlineData(104, "hello", typeof(string), true)]
		[InlineData(104, 10, typeof(int), false)]
		[InlineData(104, 56D, typeof(int), false)]
		[InlineData(104, 105810581058L, typeof(int), false)]
		[InlineData(104, 56744806738786D, typeof(int), false)]
		[InlineData(454, true, typeof(long), true)]
		[InlineData(454, null, typeof(long), true)]
		public void DefaultIntValue_Remainder(int value, object otherVal, Type resultType, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.Remainder(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(Convert.ChangeType(value % doubleVal, resultType), newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(Convert.ChangeType(value % intVal, resultType), newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(Convert.ChangeType(value % longVal, resultType), newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(104, 104, typeof(int), false)]
		[InlineData(1046775, 105887568, typeof(int), false)]
		[InlineData(104, 1058L, typeof(int), true)]
		[InlineData(104, 56.5D, typeof(double), true)]
		[InlineData(104, 56744806738786.575D, typeof(double), true)]
		[InlineData(104, "hello", typeof(string), true)]
		[InlineData(104, 10, typeof(int), false)]
		[InlineData(104, 56D, typeof(int), true)]
		[InlineData(104, 105810581058L, typeof(long), true)]
		[InlineData(104, 56744806738786D, typeof(long), true)]
		[InlineData(454, true, typeof(long), true)]
		[InlineData(454, null, typeof(long), true)]
		public void DefaultIntValue_RightShift(int value, object otherVal, Type resultType, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultIntValue<G> sut = new DefaultIntValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.RightShift(otherVal.GetAsValue(), valueProvider);
			};
			if (throwsException)
			{
				Assert.Throws<EngineRuntimeException>(action);
			}
			else
			{
				action();
			}


			if (!throwsException)
			{
				switch (otherVal)
				{
					case double doubleVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) >> Convert.ToInt32(doubleVal), resultType), newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) >> Convert.ToInt32(intVal), resultType), newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(Convert.ChangeType(Convert.ToInt64(value) >> Convert.ToInt32(longVal), resultType), newValue.GetData());
						break;
				}
			}
		}
	}
}
