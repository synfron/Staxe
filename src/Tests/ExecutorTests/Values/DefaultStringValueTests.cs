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
	public class DefaultStringValueTests
	{

		[Fact]
		public void DefaultStringValue_Add_MapCollection()
		{
			string value = "hello";
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
			};
			DefaultArrayMapValue<G> collectionValue = new DefaultArrayMapValue<G>(collectionValues, valueProvider);

			DefaultStringValue<G> sut = new DefaultStringValue<G>(value);
			Assert.Throws<EngineRuntimeException>(() =>
			{
				sut.Add(collectionValue, valueProvider);
			});
		}

		[Fact]
		public void DefaultStringValue_Add_NonMapCollection()
		{
			string value = "hello";
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> collectionValue = new DefaultArrayMapValue<G>(collectionValues, valueProvider);

			DefaultStringValue<G> sut = new DefaultStringValue<G>(value);
			DefaultCollectionValue<G> result = (DefaultCollectionValue<G>)sut.Add(collectionValue, valueProvider);

			List<KeyValuePair<IValuable<G>, ValuePointer<G>>> entries = result.GetEntries();
			Assert.Equal(Enumerable.Range(0, 4), entries.Select(kv => kv.Key).Cast<IValue>().Select(v => Convert.ToInt32(v.GetData())));
			Assert.Equal(new IValuable<G>[] { sut }.Concat(collectionValues.Values.Select(v => v.Value)), entries.Select(kv => kv.Value).Select(vp => vp.Value));
			Assert.False(result.IsMap);
			Assert.Equal(Enumerable.Range(0, 4), entries.Select(e => e.Value).Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Theory]
		[InlineData(10, false)]
		[InlineData(105810581058L, false)]
		[InlineData(56744806738786.575D, false)]
		[InlineData("hello", false)]
		[InlineData(true, false)]
		public void DefaultStringValue_Add(object otherVal, bool throwsException)
		{
			string value = "hello";
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultStringValue<G> sut = new DefaultStringValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.Add(valueProvider.GetAsValue(otherVal), valueProvider);
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
						Assert.Equal(value + doubleVal, newValue.GetData());
						break;
					case int intVal:
						Assert.Equal(value + intVal, newValue.GetData());
						break;
					case long longVal:
						Assert.Equal(value + longVal, newValue.GetData());
						break;
					case string strVal:
						Assert.Equal(value + strVal, newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(10, false)]
		[InlineData(105810581058L, false)]
		[InlineData(56744806738786.575D, false)]
		[InlineData("other", false)]
		[InlineData("hello", false)]
		[InlineData(true, false)]
		[InlineData(20, false)]
		[InlineData(20L, false)]
		[InlineData(20D, false)]
		[InlineData(null, false)]
		public void DefaultStringValue_Minus(object otherVal, bool throwsException)
		{
			string value = "hello20true";
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultStringValue<G> sut = new DefaultStringValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.Minus(valueProvider.GetAsValue(otherVal), valueProvider);
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
					case null:
						Assert.Equal(sut, newValue);
						break;
					default:
						Assert.Equal(value.Replace(otherVal.ToString(), ""), newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(10, false)]
		[InlineData(105810581058L, false)]
		[InlineData(56744806738786.575D, false)]
		[InlineData("hello", false)]
		[InlineData(true, false)]
		[InlineData(null, false)]
		public void DefaultStringValue_IsEqualTo(object otherVal, bool throwsException)
		{
			string value = "hello";
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultStringValue<G> sut = new DefaultStringValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.IsEqualTo(valueProvider.GetAsValue(otherVal), valueProvider);
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
					case string strVal:
						Assert.Equal(value == strVal, newValue.GetData());
						break;
					default:
						Assert.False((bool)newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(10, false)]
		[InlineData(105810581058L, false)]
		[InlineData(56744806738786.575D, false)]
		[InlineData("hello", false)]
		[InlineData(true, false)]
		[InlineData(null, false)]
		public void DefaultStringValue_IsNotEqualTo(object otherVal, bool throwsException)
		{
			string value = "hello";
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultStringValue<G> sut = new DefaultStringValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.IsNotEqualTo(valueProvider.GetAsValue(otherVal), valueProvider);
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
					case string strVal:
						Assert.Equal(value != strVal, newValue.GetData());
						break;
					default:
						Assert.True((bool)newValue.GetData());
						break;
				}
			}
		}
	}
}
