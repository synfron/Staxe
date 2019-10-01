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
	public class DefaultGroupValueTests
	{
		[Fact]
		public void DefaultGroupValue_Add_MapCollection()
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
			};
			DefaultArrayMapValue<G> collectionValue = new DefaultArrayMapValue<G>(collectionValues, valueProvider);

			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			Assert.Throws<EngineRuntimeException>(() =>
			{
				sut.Add(collectionValue, valueProvider);
			});
		}

		[Fact]
		public void DefaultGroupValue_Add_NonMapCollection()
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			DefaultArrayMapValue<G> collectionValue = new DefaultArrayMapValue<G>(collectionValues, valueProvider);

			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			DefaultCollectionValue<G> result = (DefaultCollectionValue<G>)sut.Add(collectionValue, valueProvider);

			List<KeyValuePair<IValuable<G>, ValuePointer<G>>> entries = result.GetEntries();
			Assert.Equal(Enumerable.Range(0, 4), entries.Select(kv => kv.Key).Cast<IValue>().Select(v => Convert.ToInt32(v.GetData())));
			Assert.Equal(new IValuable<G>[] { sut }.Concat(collectionValues.Values.Select(v => v.Value)), entries.Select(kv => kv.Value).Select(vp => vp.Value));
			Assert.False(result.IsMap);
			Assert.Equal(Enumerable.Range(0, 4), entries.Select(e => e.Value).Cast<EntryValuePointer<G>>().Select(v => v.Index));
		}

		[Theory]
		[InlineData(10, true)]
		[InlineData(105810581058L, true)]
		[InlineData(56744806738786.575D, true)]
		[InlineData("hello", true)]
		[InlineData(true, true)]
		[InlineData(null, true)]
		public void DefaultGroupValue_Add(object otherVal, bool throwsException)
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			IValuable<G> newValue = null;
			Action action = () =>
			{
				newValue = sut.Add(valueProvider.GetAsValue(otherVal), valueProvider);
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
				}
			}
		}

		[Fact]
		public void DefaultGroupValue_Get_Existing()
		{
			string pointerName = "grouppointername";
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", valueProvider.Null) { Identifier = pointerName };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			Dictionary<string, int> pointerMap = new Dictionary<string, int> { { pointerName, 1 } };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers && m.PointerMap == pointerMap);
			DefaultStringValue<G> key = new DefaultStringValue<G>(pointerName);


			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			ValuePointer<G> result = sut.Get(key, false, valueProvider) as ValuePointer<G>;

			Assert.Equal(groupPointer, result);
		}

		[Fact]
		public void DefaultGroupValue_Get_NotExisting()
		{
			string pointerName = "grouppointername";
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DeclaredValuePointer<G> groupPointer = new DeclaredValuePointer<G>("group", valueProvider.Null) { Identifier = pointerName };
			List<DeclaredValuePointer<G>> groupPointers = new List<DeclaredValuePointer<G>>() { new DeclaredValuePointer<G>("group"), groupPointer, new DeclaredValuePointer<G>("group") };
			Dictionary<string, int> pointerMap = new Dictionary<string, int> { { pointerName, 1 } };
			G groupState = Mock.Of<G>(m => m.GroupPointers == groupPointers && m.PointerMap == pointerMap);
			DefaultStringValue<G> key = new DefaultStringValue<G>("otherpointer");


			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			EngineRuntimeException ex = Assert.Throws<EngineRuntimeException>(() =>
			{
				sut.Get(key, true, valueProvider);
			});
			Assert.Equal("Cannot create group pointer using accessor", ex.Message);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(105810581058L)]
		[InlineData(56744806738786.575D)]
		[InlineData("hello")]
		[InlineData(true)]
		[InlineData(null)]
		public void DefaultGroupValue_IsEqualTo(object otherVal)
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(valueProvider.GetAsValue(otherVal), valueProvider);

			Assert.False(result.Data);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(105810581058L)]
		[InlineData(56744806738786.575D)]
		[InlineData("hello")]
		[InlineData(true)]
		[InlineData(null)]
		public void DefaultGroupValue_IsNotEqualTo(object otherVal)
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(valueProvider.GetAsValue(otherVal), valueProvider);

			Assert.True(result.Data);
		}

		[Fact]
		public void DefaultGroupValue_IsEqualTo_SameGroup()
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DefaultGroupValue<G> otherGroupValue = new DefaultGroupValue<G>(groupState);

			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(otherGroupValue, valueProvider);

			Assert.True(result.Data);
		}

		[Fact]
		public void DefaultGroupValue_IsNotEqualTo_SameGroup()
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DefaultGroupValue<G> otherGroupValue = new DefaultGroupValue<G>(groupState);

			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(otherGroupValue, valueProvider);

			Assert.False(result.Data);
		}

		[Fact]
		public void DefaultGroupValue_IsEqualTo_OtherGroup()
		{
			G groupState = Mock.Of<G>();
			G otherGroupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DefaultGroupValue<G> otherGroupValue = new DefaultGroupValue<G>(otherGroupState);

			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(otherGroupValue, valueProvider);

			Assert.False(result.Data);
		}

		[Fact]
		public void DefaultGroupValue_IsNotEqualTo_OtherGroup()
		{
			G groupState = Mock.Of<G>();
			G otherGroupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DefaultGroupValue<G> otherGroupValue = new DefaultGroupValue<G>(otherGroupState);

			DefaultGroupValue<G> sut = new DefaultGroupValue<G>(groupState);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(otherGroupValue, valueProvider);

			Assert.True(result.Data);
		}
	}
}
