using Moq;
using Synfron.Staxe.Executor;
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
	public class DefaultActionValueTests
	{
		[Fact]
		public void DefaultActionValue_Add_MapCollection()
		{
			int location = 10;
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
			};
			DefaultArrayMapValue<G> collectionValue = new DefaultArrayMapValue<G>(collectionValues, valueProvider);

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, location);
			Assert.Throws<EngineRuntimeException>(() =>
			{
				sut.Add(collectionValue, valueProvider);
			});
		}

		[Fact]
		public void DefaultActionValue_Add_NonMapCollection()
		{
			int location = 10;
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			Dictionary<IValuable<G>, ValuePointer<G>> collectionValues = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};
			ICollectionValue<G> collectionValue = valueProvider.GetCollection(collectionValues, null);

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, location);
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
		public void DefaultActionValue_Add(object otherVal, bool throwsException)
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, 10);
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
		public void DefaultActionValue_Execute_WithActionOverrides()
		{
			int location = 10;
			IValuable<G> overrideActionValue = Mock.Of<IValuable<G>>();
			Dictionary<int, IValuable<G>> actionOverrides = new Dictionary<int, IValuable<G>> { { location, overrideActionValue } };
			G groupState = Mock.Of<G>(m => m.ActionOverrides == actionOverrides);
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, location);
			sut.Execute(executionState, valueProvider);

			Mock.Get(overrideActionValue).Verify(m => m.Execute(executionState, valueProvider), Times.Once);
		}

		[Fact]
		public void DefaultActionValue_Execute_NoActionOverrides()
		{
			int location = 10;
			StackValuePointer<G>[] initStackPointers = new[] { new StackValuePointer<G>(), new StackValuePointer<G>() };
			G groupState = Mock.Of<G>();
			ExecutionState<G> executionState = new ExecutionState<G>(groupState);
			IValueProvider<G> valueProvider = Mock.Of<IValueProvider<G>>();

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, location);
			sut.InitStackPointers.AddRange(initStackPointers);
			sut.Execute(executionState, valueProvider);

			Assert.Equal(initStackPointers, executionState.StackPointers);
			Assert.Equal(location - 1, executionState.InstructionIndex);
			Assert.Equal(groupState, executionState.GroupState);
		}

		[Fact]
		public void DefaultActionValue_Clone_HasOverrides()
		{
			int location = 10;
			G groupState = Mock.Of<G>();
			int overrideLocation = 20;
			G overrideGroupState = Mock.Of<G>();

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, location);
			IActionValue<G> result = sut.Clone(overrideGroupState, overrideLocation);

			Assert.Equal(overrideLocation, result.Location);
			Assert.Equal(overrideGroupState, result.GroupState);
		}

		[Fact]
		public void DefaultActionValue_Clone_NoOverrides()
		{
			int location = 10;
			G groupState = Mock.Of<G>();

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, location);
			IActionValue<G> result = sut.Clone();

			Assert.Equal(location, result.Location);
			Assert.Equal(groupState, result.GroupState);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(105810581058L)]
		[InlineData(56744806738786.575D)]
		[InlineData("hello")]
		[InlineData(true)]
		[InlineData(null)]
		public void DefaultActionValue_IsEqualTo(object otherVal)
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, 10);
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
		public void DefaultActionValue_IsNotEqualTo(object otherVal)
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, 10);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(valueProvider.GetAsValue(otherVal), valueProvider);

			Assert.True(result.Data);
		}

		[Fact]
		public void DefaultActionValue_IsEqualTo_SameAction()
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, 10);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(sut, valueProvider);

			Assert.True(result.Data);
		}

		[Fact]
		public void DefaultActionValue_IsNotEqualTo_SameAction()
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, 10);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(sut, valueProvider);

			Assert.False(result.Data);
		}

		[Fact]
		public void DefaultActionValue_IsEqualTo_OtherAction()
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DefaultActionValue<G> otherActionValue = new DefaultActionValue<G>(groupState, 10);

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, 10);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(otherActionValue, valueProvider);

			Assert.False(result.Data);
		}

		[Fact]
		public void DefaultActionValue_IsNotEqualTo_OtherAction()
		{
			G groupState = Mock.Of<G>();
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();
			DefaultActionValue<G> otherActionValue = new DefaultActionValue<G>(groupState, 10);

			DefaultActionValue<G> sut = new DefaultActionValue<G>(groupState, 10);
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(otherActionValue, valueProvider);

			Assert.True(result.Data);
		}
	}
}
