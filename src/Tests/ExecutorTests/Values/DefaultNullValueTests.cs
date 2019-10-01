using ExecutorTests.Shared;
using Synfron.Staxe.Executor.Values;
using Xunit;
using G = ExecutorTests.TestMocks.MockableGroupState;

namespace ExecutorTests.Values
{
	public class DefaultNullValueTests
	{
		[Theory]
		[InlineData(10)]
		[InlineData(105810581058L)]
		[InlineData(56744806738786.575D)]
		[InlineData("hello")]
		[InlineData(true)]
		[InlineData(null)]
		public void DefaultNullValue_IsEqualTo(object otherVal)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultNullValue<G> sut = new DefaultNullValue<G>();
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(otherVal.GetAsValue(), valueProvider);

			Assert.Equal(otherVal == null, result.Data);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(105810581058L)]
		[InlineData(56744806738786.575D)]
		[InlineData("hello")]
		[InlineData(true)]
		[InlineData(null)]
		public void DefaultNullValue_IsNotEqualTo(object otherVal)
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultNullValue<G> sut = new DefaultNullValue<G>();
			IValue<G, bool> result = (IValue<G, bool>)sut.IsNotEqualTo(otherVal.GetAsValue(), valueProvider);

			Assert.Equal(otherVal != null, result.Data);
		}

		[Fact]
		public void DefaultNullValue_IsEqualTo_SameAction()
		{
			DefaultValueProvider<G> valueProvider = new DefaultValueProvider<G>();

			DefaultNullValue<G> sut = new DefaultNullValue<G>();
			IValue<G, bool> result = (IValue<G, bool>)sut.IsEqualTo(sut, valueProvider);

			Assert.True(result.Data);
		}
	}
}
