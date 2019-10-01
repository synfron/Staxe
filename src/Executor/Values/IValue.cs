namespace Synfron.Staxe.Executor.Values
{
	public interface IValue<G> : IValue, IValuable<G> where G : IGroupState<G>, new()
	{
	}

	public interface IValue<G, T> : IValue<G> where G : IGroupState<G>, new()
	{
		T Data
		{
			get;
		}

		void Deconstruct(out T data);
	}

	public interface IValue : IValuable
	{
		object GetData();

		bool HasValue
		{
			get;
		}
	}
}