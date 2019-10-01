using Synfron.Staxe.Executor.Values;

namespace Synfron.Staxe.Executor.Pointers
{
	public class ValuePointer<G> where G : IGroupState<G>, new()
	{
		public virtual IValuable<G> Value { get; set; }

		public string Identifier
		{
			get;
			set;
		}
	}
}
