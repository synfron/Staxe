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
	public class DefaultBooleanValueTests
	{

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DefaultBooleanValue_Add_MapCollection(bool value)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
			};
			DefaultArrayMapValue<G> collectionValue = new DefaultArrayMapValue<G>(collectionValues, valueProvider);

			DefaultBooleanValue<G> sut = new DefaultBooleanValue<G>(value);
			Assert.Throws<EngineRuntimeException>(() =>
			{
				sut.Add(collectionValue, valueProvider);
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DefaultBooleanValue_Add_NonMapCollection(bool value)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> collectionValue = new DefaultArrayMapValue<G>(collectionValues, valueProvider);

			DefaultBooleanValue<G> sut = new DefaultBooleanValue<G>(value);
			DefaultCollectionValue<G> result = (DefaultCollectionValue<G>)sut.Add(collectionValue, valueProvider);

			List<KeyValuePair<IValuable<G>, ValuePointer<G>>> entries = result.GetEntries();
			Assert.Equal(Enumerable.Range(0, 4), entries.Select(kv => kv.Key).Cast<IValue>().Select(v => Convert.ToInt32(v.GetData())));
			Assert.Equal(new IValuable<G>[] { sut }.Concat(collectionValues.Values.Select(v => v.Value)), entries.Select(kv => kv.Value).Select(vp => vp.Value));
			Assert.False(result.IsMap);
			Assert.Equal(Enumerable.Range(0, 4), entries.Select(e => e.Value).Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Theory]
		[InlineData(true, 10, true)]
		[InlineData(true, 105810581058L, true)]
		[InlineData(true, 56744806738786.575D, true)]
		[InlineData(true, "hello", false)]
		[InlineData(false, true, true)]
		[InlineData(true, true, true)]
		[InlineData(true, null, true)]
		public void DefaultBooleanValue_Add(bool value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultBooleanValue<G> sut = new DefaultBooleanValue<G>(value);
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
					case string stringVal:
						Assert.Equal(value.ToString().ToLower() + stringVal, newValue.GetData());
						break;
					case null:
						Assert.Equal(sut, newValue);
						break;
				}
			}
		}

		[Theory]
		[InlineData(true, 10, true)]
		[InlineData(true, 105810581058L, true)]
		[InlineData(true, 56744806738786.575D, true)]
		[InlineData(false, true, false)]
		[InlineData(true, true, false)]
		public void DefaultBooleanValue_And(bool value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultBooleanValue<G> sut = new DefaultBooleanValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.And(valueProvider.GetAsValue(otherVal), valueProvider);
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
					case bool boolVal:
						Assert.Equal(value & boolVal, newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(true, 10, false)]
		[InlineData(true, 105810581058L, false)]
		[InlineData(true, 56744806738786.575D, false)]
		[InlineData(true, "hello", false)]
		[InlineData(false, true, false)]
		[InlineData(true, true, false)]
		[InlineData(true, null, false)]
		public void DefaultBooleanValue_IsEqualTo(bool value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultBooleanValue<G> sut = new DefaultBooleanValue<G>(value);
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
					case bool boolVal:
						Assert.Equal(value == boolVal, newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(true, 10, false)]
		[InlineData(true, 105810581058L, false)]
		[InlineData(true, 56744806738786.575D, false)]
		[InlineData(true, "hello", false)]
		[InlineData(false, true, false)]
		[InlineData(true, true, false)]
		[InlineData(true, null, false)]
		public void DefaultBooleanValue_IsNotEqualTo(bool value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultBooleanValue<G> sut = new DefaultBooleanValue<G>(value);
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
					case bool boolVal:
						Assert.Equal(value != boolVal, newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(true, 10, true)]
		[InlineData(true, 105810581058L, true)]
		[InlineData(true, 56744806738786.575D, true)]
		[InlineData(false, true, false)]
		[InlineData(true, true, false)]
		public void DefaultBooleanValue_Or(bool value, object otherVal, bool throwsException)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultBooleanValue<G> sut = new DefaultBooleanValue<G>(value);
			IValue<G> newValue = null;
			Action action = () =>
			{
				newValue = (IValue<G>)sut.Or(valueProvider.GetAsValue(otherVal), valueProvider);
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
					case bool boolVal:
						Assert.Equal(value | boolVal, newValue.GetData());
						break;
				}
			}
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void DefaultBooleanValue_Not(bool value)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultBooleanValue<G> sut = new DefaultBooleanValue<G>(value);
			IValue<G, bool> result = sut.Not(valueProvider) as IValue<G, bool>;

			Assert.Equal(!value, result.Data);
		}
	}
}
