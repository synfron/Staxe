using Moq;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Shared.Exceptions;
using System.Collections.Generic;
using Xunit;
using G = ExecutorTests.TestMocks.MockableGroupState;

namespace ExecutorTests.Values
{
	public class DefaultValueProviderTests
	{

		[Fact]
		public void DefaultValueProvider_GetAction()
		{
			int location = 20;
			G groupState = Mock.Of<G>();

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IActionValue<G> result = sut.GetAction(groupState, location);

			Assert.Equal(location, result.Location);
			Assert.Equal(groupState, result.GroupState);
		}

		[Fact]
		public void DefaultValueProvider_GetCollection_WithCapacity()
		{
			int capacity = 20;

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			ICollectionValue<G> result = sut.GetCollection(capacity, null);

			Assert.Equal(0, result.Size);
		}

		[Fact]
		public void DefaultValueProvider_GetCollection_WithEntries()
		{
			Dictionary<IValuable<G>, ValuePointer<G>> values = new Dictionary<IValuable<G>, ValuePointer<G>>()
			{
				{ new DefaultIntValue<G>(0), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(1), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(2), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(3), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(4), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } },
				{ new DefaultIntValue<G>(5), new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() } }
			};

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			ICollectionValue<G> result = sut.GetCollection(values, null);

			Assert.Equal(6, result.Size);
		}

		[Fact]
		public void DefaultValueProvider_GetCollection_WithList()
		{
			ValuePointer<G>[] values = new ValuePointer<G>[]
			{
				new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() },
				new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() },
				new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() },
				new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() },
				new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() },
				new ValuePointer<G> { Value = Mock.Of<IValuable<G>>() }
			};

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			ICollectionValue<G> result = sut.GetCollection(values, null);

			Assert.Equal(6, result.Size);
		}

		[Theory]
		[InlineData("hello")]
		[InlineData(null)]
		public void DefaultValueProvider_GetAsNullableString(string value)
		{
			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IValue<G> result = sut.GetAsNullableString(value);

			Assert.Equal(value, result.GetData());
			switch (value)
			{
				case null:
					Assert.IsAssignableFrom<INullValue<G>>(result);
					break;
				default:
					Assert.IsAssignableFrom<IValue<G, string>>(result);
					break;
			}
		}

		[Theory]
		[InlineData(10)]
		[InlineData(105810581058L)]
		[InlineData(56744806738786.575D)]
		[InlineData("hello")]
		[InlineData(true)]
		[InlineData(null)]
		public void DefaultValueProvider_GetAsValue(object value)
		{
			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IValue<G> result = sut.GetAsValue(value);

			switch (value)
			{
				case double _:
					Assert.IsAssignableFrom<IValue<G, double>>(result);
					break;
				case int _:
					Assert.IsAssignableFrom<IValue<G, int>>(result);
					break;
				case long _:
					Assert.IsAssignableFrom<IValue<G, long>>(result);
					break;
				case string _:
					Assert.IsAssignableFrom<IValue<G, string>>(result);
					break;
				case null:
					Assert.IsAssignableFrom<INullValue<G>>(result);
					break;
			}
		}

		[Fact]
		public void DefaultValueProvider_GetAsValue_WithObject_Throws()
		{
			object data = new object();

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			EngineRuntimeException ex = Assert.Throws<EngineRuntimeException>(() => sut.GetAsValue(data));

			Assert.Equal("No implementation for externals", ex.Message);
		}

		[Fact]
		public void DefaultValueProvider_GetBoolean()
		{
			bool value = true;

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IValue<G> result = sut.GetBoolean(value);

			Assert.Equal(value, result.GetData());
		}

		[Fact]
		public void DefaultValueProvider_GetDouble()
		{
			double value = 20D;

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IValue<G> result = sut.GetDouble(value);

			Assert.Equal(value, result.GetData());
		}

		[Fact]
		public void DefaultValueProvider_GetGroup()
		{
			G groupState = Mock.Of<G>();

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IGroupValue<G> result = sut.GetGroup(groupState);

			Assert.Equal(groupState, result.State);
		}

		[Fact]
		public void DefaultValueProvider_GetInt()
		{
			int value = 20;

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IValue<G> result = sut.GetInt(value);

			Assert.Equal(value, result.GetData());
		}

		[Fact]
		public void DefaultValueProvider_GetLong()
		{
			long value = 20L;

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IValue<G> result = sut.GetLong(value);

			Assert.Equal(value, result.GetData());
		}

		[Fact]
		public void DefaultValueProvider_GetNull()
		{
			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IValue<G> result = sut.GetNull();

			Assert.Null(result.GetData());
		}

		[Fact]
		public void DefaultValueProvider_GetString()
		{
			string value = "hello";

			DefaultValueProvider<G> sut = new DefaultValueProvider<G>();
			IValue<G> result = sut.GetString(value);

			Assert.Equal(value, result.GetData());
		}
	}
}
