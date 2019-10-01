using Synfron.Staxe.Executor.Values;

namespace Synfron.Staxe.Executor.Pointers
{
	public sealed class ReferenceStackValuePointer<G> : StackValuePointer<G> where G : IGroupState<G>, new()
	{
		private readonly ValuePointer<G> _pointer;

		public ReferenceStackValuePointer(ValuePointer<G> pointer)
		{
			_pointer = pointer;
			Origin = int.MaxValue;
			Identifier = pointer.Identifier;
		}

		public override IValuable<G> Value { get => _pointer.Value; set => _pointer.Value = value; }
	}
}
