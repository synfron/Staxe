namespace Synfron.Staxe.Executor.Values
{
	public interface IGroupValue<G> : IValuable<G> where G : IGroupState<G>, new()
	{
		G State { get; }
	}
}