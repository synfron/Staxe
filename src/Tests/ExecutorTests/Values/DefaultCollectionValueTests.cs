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
	public class DefaultCollectionValueTests
	{
		[Fact]
		public void DefaultCollectionValue_GetEntries_FromInitialCapacityConstructor()
		{
			int capacity = 20;

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(capacity);
			List<KeyValuePair<IValuable<G>, ValuePointer<G>>> result = sut.GetEntries();

			Assert.Empty(result);
		}

		[Fact]
		public void DefaultCollectionValue_GetEntries_FromIValuablesListConstructor()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			IValuable<G>[] values = new IValuable<G>[] { Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>() };

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			List<KeyValuePair<IValuable<G>, ValuePointer<G>>> result = sut.GetEntries();

			Assert.Equal(Enumerable.Range(0, values.Length), result.Select(kv => kv.Key).Cast<IValue>().Select(v => Convert.ToInt32(v.GetData())));
			Assert.Equal(values, result.Select(kv => kv.Value).Select(vp => vp.Value));
			Assert.Equal(Enumerable.Range(0, 6), result.Select(e => e.Value).Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Fact]
		public void DefaultCollectionValue_GetEntries_FromValuePointersListConstructor()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			IValuable<G>[] values = new IValuable<G>[] { Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>() };

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values.Select(v => new ValuePointer<G> { Value = v }), valueProvider);
			List<KeyValuePair<IValuable<G>, ValuePointer<G>>> result = sut.GetEntries();

			Assert.Equal(Enumerable.Range(0, values.Length), result.Select(kv => kv.Key).Cast<IValue>().Select(v => Convert.ToInt32(v.GetData())));
			Assert.Equal(values, result.Select(kv => kv.Value).Select(vp => vp.Value));
			Assert.Equal(Enumerable.Range(0, 6), result.Select(e => e.Value).Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Fact]
		public void DefaultCollectionValue_GetEntries_FromEntriesListConstructor()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			List<KeyValuePair<IValuable<G>, ValuePointer<G>>> result = sut.GetEntries();

			Assert.Equal(Enumerable.Range(0, values.Count), result.Select(kv => kv.Key).Cast<IValue>().Select(v => Convert.ToInt32(v.GetData())));
			Assert.Equal(values.Values.Select(v => v.Value), result.Select(kv => kv.Value).Select(vp => vp.Value));
			Assert.False(sut.IsMap);
			Assert.Equal(Enumerable.Range(0, 6), result.Select(e => e.Value).Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Fact]
		public void DefaultCollectionValue_GetEntries_FromUnorderedEntriesListConstructor()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			List<KeyValuePair<IValuable<G>, ValuePointer<G>>> result = sut.GetEntries();

			Assert.Equal(new object[] { 0.0, 1, 2.0, 4L, 5, 3L }, result.Select(kv => kv.Key).Cast<IValue>().Select(v => v.GetData()));
			Assert.Equal(values.Values.Select(v => v.Value), result.Select(kv => kv.Value).Select(vp => vp.Value));
			Assert.True(sut.IsMap);
			Assert.Equal(Enumerable.Range(0, 6), result.Select(e => e.Value).Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Fact]
		public void DefaultCollectionValue_GetAt()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			IValuable<G> value = Mock.Of<IValuable<G>>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = value } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			ValuePointer<G> result = sut.GetAt(new DefaultIntValue<G>(5), valueProvider);

			Assert.Equal(value, result.Value);
		}

		[Fact]
		public void DefaultCollectionValue_GetValues()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			IValuable<G>[] values = new IValuable<G>[] { Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>(), Mock.Of<IValuable<G>>() };

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			List<ValuePointer<G>> result = sut.GetValues();

			Assert.Equal(values, result.Select(p => p.Value));
		}

		[Fact]
		public void DefaultCollectionValue_GetKeys_Unordered()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			List<IValuable<G>> result = sut.GetKeys();

			Assert.Equal(values.Keys, result);
		}

		[Fact]
		public void DefaultCollectionValue_GetKeys_Ordered()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			List<IValuable<G>> result = sut.GetKeys();

			Assert.Equal(values.Keys, result);
		}

		[Theory]
		[InlineData(3, true)]
		[InlineData(3.0, false)]
		[InlineData(-1, false)]
		[InlineData(6, false)]
		[InlineData("hi", false)]
		public void DefaultCollectionValue_ContainsKey_Ordered(object key, bool contains)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			bool result = sut.ContainsKey(key.GetAsValue(), valueProvider);

			Assert.Equal(contains, result);
		}

		[Theory]
		[InlineData(3, true)]
		[InlineData(3.0, false)]
		[InlineData(-1, false)]
		[InlineData(6, false)]
		[InlineData("hi", false)]
		public void DefaultCollectionValue_ContainsKey_Unordered(object key, bool contains)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			bool result = sut.ContainsKey(key.GetAsValue(), valueProvider);

			Assert.Equal(contains, result);
		}

		[Fact]
		public void DefaultCollectionValue_ContainsValue_Present()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			IValuable<G> value = Mock.Of<IValuable<G>>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = value } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			bool result = sut.ContainsValue(value, valueProvider);

			Assert.True(result);
		}

		[Fact]
		public void DefaultCollectionValue_ContainsValue_NotPresent()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			IValuable<G> value = Mock.Of<IValuable<G>>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			bool result = sut.ContainsValue(value, valueProvider);

			Assert.False(result);
		}

		[Fact]
		public void DefaultCollectionValue_IndexOf_Present()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			IValuable<G> value = Mock.Of<IValuable<G>>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = value } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			int result = sut.IndexOf(value, valueProvider);

			Assert.Equal(3, result);
		}

		[Fact]
		public void DefaultCollectionValue_IndexOf_NotPresent()
		{
			IValuable<G> value = Mock.Of<IValuable<G>>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			int result = sut.IndexOf(value, valueProvider);

			Assert.Equal(-1, result);
		}

		[Fact]
		public void DefaultCollectionValue_KeyOf_Present()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DefaultLongValue<G> key = new DefaultLongValue<G>(4);
			IValuable<G> value = Mock.Of<IValuable<G>>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ key, new ValuePointer<G> { Value = value } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			IValuable<G> result = sut.KeyOf(value, valueProvider);

			Assert.Equal(key, result);
		}

		[Fact]
		public void DefaultCollectionValue_KeyOf_NotPresent()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			IValuable<G> value = Mock.Of<IValuable<G>>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			IValuable<G> result = sut.KeyOf(value, valueProvider);

			Assert.Null(result);
		}

		[Fact]
		public void DefaultCollectionValue_Add_NonMapCollection()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> firstValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
			};
			Dictionary<IValuable<G>, ValuePointer<G>> secondValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultCollectionValue<G> otherCollection = new DefaultArrayMapValue<G>(secondValues, valueProvider) as DefaultCollectionValue<G>;

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(firstValues, valueProvider);
			EngineRuntimeException exception = Assert.Throws<EngineRuntimeException>(() => sut.Add(otherCollection, valueProvider) as DefaultCollectionValue<G>);

			Assert.Equal("Invalid values for addition", exception.Message);
		}

		[Fact]
		public void DefaultCollectionValue_Minus_MapCollection()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			IValuable<G> sharedValue1 = Mock.Of<IValuable<G>>();
			IValuable<G> sharedValue2 = Mock.Of<IValuable<G>>();
			Dictionary<IValuable<G>, ValuePointer<G>> firstValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = sharedValue2 } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = sharedValue1 } }
			};
			Dictionary<IValuable<G>, ValuePointer<G>> secondValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = sharedValue2 } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = sharedValue1 } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> otherCollection = new DefaultArrayMapValue<G>(secondValues, valueProvider);

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(firstValues, valueProvider);

			EngineRuntimeException exception = Assert.Throws<EngineRuntimeException>(() => sut.Minus(otherCollection, valueProvider));

			Assert.Equal("Invalid values for subtraction", exception.Message);
		}

		[Fact]
		public void DefaultCollectionValue_Remove_ValueOnNonMap()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			sut.Remove(new DefaultIntValue<G>(3), valueProvider);

			Assert.Equal(values.Values.Take(3).Concat(values.Values.Skip(4).Take(2)).Select(vp => vp.Value), sut.GetValues().Select(vp => vp.Value));
			Assert.Equal(Enumerable.Range(0, 5), sut.GetKeys().Cast<IValue<G, int>>().Select(v => v.Data));
			Assert.Equal(Enumerable.Range(0, 5), sut.GetValues().Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Fact]
		public void DefaultCollectionValue_Remove_ValueOnMap()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			sut.Remove(new DefaultLongValue<G>(4), valueProvider);

			Assert.Equal(values.Values.Take(3).Concat(values.Values.Skip(4).Take(2)).Select(vp => vp.Value), sut.GetValues().Select(vp => vp.Value));
			Assert.Equal(values.Keys.Take(3).Concat(values.Keys.Skip(4).Take(2)), sut.GetKeys());
			Assert.Equal(Enumerable.Range(0, 5), sut.GetValues().Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Fact]
		public void DefaultCollectionValue_FinalizeEntry_EntryInsert()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			EntryValuePointer<G> entryPointer = new EntryValuePointer<G>(sut, new DefaultLongValue<G>(4), 3, valueProvider);

			sut.FinalizeEntry(new DefaultLongValue<G>(4), entryPointer);

			Assert.Equal(6, sut.Size);
			KeyValuePair<IValuable<G>, ValuePointer<G>> item = sut.GetEntries()[3];
			Assert.Equal(entryPointer.Key, item.Key);
			Assert.Equal(entryPointer, item.Value);
		}

		[Fact]
		public void DefaultCollectionValue_FinalizeEntry_EntryAdd()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			EntryValuePointer<G> entryPointer = new EntryValuePointer<G>(sut, new DefaultLongValue<G>(34), 6, valueProvider);

			sut.FinalizeEntry(new DefaultLongValue<G>(34), entryPointer);

			Assert.Equal(7, sut.Size);
			KeyValuePair<IValuable<G>, ValuePointer<G>> item = sut.GetEntries()[6];
			Assert.Equal(entryPointer.Key, item.Key);
			Assert.Equal(entryPointer, item.Value);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void DefaultCollectionValue_Get_OnMapItemExists(bool create)
		{
			DefaultLongValue<G> key = new DefaultLongValue<G>(4);
			IValuable<G> value = Mock.Of<IValuable<G>>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ key, new ValuePointer<G> { Value = value } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			EntryValuePointer<G> result = sut.Get(key, create, valueProvider) as EntryValuePointer<G>;

			Assert.Equal(key, result.Key);
			Assert.Equal(value, result.Value);
			Assert.Equal(3, result.Index);
			Assert.True(result.IsSet);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void DefaultCollectionValue_Get_OnNonMapItemExists(bool create)
		{
			DefaultIntValue<G> key = new DefaultIntValue<G>(3);
			IValuable<G> value = Mock.Of<IValuable<G>>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ key, new ValuePointer<G> { Value = value } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			EntryValuePointer<G> result = sut.Get(key, create, valueProvider) as EntryValuePointer<G>;

			Assert.False(sut.IsMap);
			Assert.Equal(key, result.Key);
			Assert.Equal(value, result.Value);
			Assert.Equal(3, result.Index);
			Assert.True(result.IsSet);
		}

		[Fact]
		public void DefaultCollectionValue_Get_OnMapItemNotExistsCreateEntry()
		{
			DefaultLongValue<G> key = new DefaultLongValue<G>(20);
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			EntryValuePointer<G> result = sut.Get(key, true, valueProvider) as EntryValuePointer<G>;

			Assert.True(sut.IsMap);
			Assert.Equal(key, result.Key);
			Assert.Null(result.Value);
			Assert.Equal(6, result.Index);
			Assert.False(result.IsSet);
		}

		[Theory]
		[InlineData(6, false)]
		[InlineData(10, true)]
		public void DefaultCollectionValue_Get_OnNonMapItemNotExistsCreateEntry(int keyVal, bool isMap)
		{
			DefaultIntValue<G> key = new DefaultIntValue<G>(keyVal);
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			EntryValuePointer<G> result = sut.Get(key, true, valueProvider) as EntryValuePointer<G>;

			Assert.Equal(isMap, sut.IsMap);
			Assert.Equal(key, result.Key);
			Assert.Null(result.Value);
			Assert.Equal(6, result.Index);
			Assert.False(result.IsSet);
		}

		[Fact]
		public void DefaultCollectionValue_Get_OnMapItemNotExistsNoCreate()
		{
			DefaultLongValue<G> key = new DefaultLongValue<G>(20);
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultDoubleValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultDoubleValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultLongValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			EntryValuePointer<G> result = sut.Get(key, false, valueProvider) as EntryValuePointer<G>;

			Assert.True(sut.IsMap);
			Assert.Null(result);
		}

		[Fact]
		public void DefaultCollectionValue_Get_OnNonMapItemNotExistsNoCreate()
		{
			DefaultLongValue<G> key = new DefaultLongValue<G>(20);
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>(values, valueProvider);
			EntryValuePointer<G> result = sut.Get(key, false, valueProvider) as EntryValuePointer<G>;

			Assert.False(sut.IsMap);
			Assert.Null(result);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(105810581058L)]
		[InlineData(56744806738786.575D)]
		[InlineData("hello")]
		[InlineData(true)]
		[InlineData(null)]
		public void DefaultCollectionValue_IsEqualTo(object otherVal)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>();
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(otherVal.GetAsValue(), valueProvider);

			Assert.False(result.Data);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(105810581058L)]
		[InlineData(56744806738786.575D)]
		[InlineData("hello")]
		[InlineData(true)]
		[InlineData(null)]
		public void DefaultCollectionValue_IsNotEqualTo(object otherVal)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>();
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(otherVal.GetAsValue(), valueProvider);

			Assert.True(result.Data);
		}

		[Fact]
		public void DefaultCollectionValue_IsEqualTo_SameCollection()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>();
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(sut, valueProvider);

			Assert.True(result.Data);
		}

		[Fact]
		public void DefaultCollectionValue_IsNotEqualTo_SameCollection()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>();
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(sut, valueProvider);

			Assert.False(result.Data);
		}

		[Fact]
		public void DefaultCollectionValue_IsEqualTo_OtherCollection()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DefaultArrayMapValue<G> otherCollectionValue = new DefaultArrayMapValue<G>();

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>();
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(otherCollectionValue, valueProvider);

			Assert.False(result.Data);
		}

		[Fact]
		public void DefaultCollectionValue_IsNotEqualTo_OtherCollection()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DefaultArrayMapValue<G> otherCollectionValue = new DefaultArrayMapValue<G>();

			DefaultArrayMapValue<G> sut = new DefaultArrayMapValue<G>();
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(otherCollectionValue, valueProvider);

			Assert.True(result.Data);
		}
	}
}
