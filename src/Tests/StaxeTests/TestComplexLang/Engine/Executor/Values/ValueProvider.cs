using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Values;

namespace StaxeTests.TestComplexLang.Engine.Executor.Values
{
	public class ValueProvider : DefaultValueProvider<GroupState>
	{
		public override IValue<GroupState> GetExternal(object data)
		{
			return new ExternalValue<GroupState>(data);
		}
	}
}
