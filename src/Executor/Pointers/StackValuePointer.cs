using Synfron.Staxe.Executor.Values;

namespace Synfron.Staxe.Executor.Pointers
{
	public class StackValuePointer<G> : ValuePointer<G> where G : IGroupState<G>, new()
	{

		public StackValuePointer()
		{
		}

		public StackValuePointer(IValuable<G> value)
		{
			Value = value;
		}


		public int Origin
		{
			get;
			set;
		}

		public int Depth
		{
			get;
			set;
		}
	}
}
