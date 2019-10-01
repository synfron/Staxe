using Synfron.Staxe.Executor.Values;

namespace Synfron.Staxe.Executor.Pointers
{
	public class EntryValuePointer<G> : ValuePointer<G> where G : IGroupState<G>, new()
	{

		public EntryValuePointer(ICollectionValue<G> collection, IValuable<G> key, int index, IValueProvider<G> valueProvider, bool isSet = false)
		{
			Collection = collection;
			Key = key;
			IsSet = isSet;
			Index = index;
			ValueProvider = valueProvider;

		}

		public override IValuable<G> Value
		{
			get
			{
				return base.Value;
			}
			set
			{
				base.Value = value;
				if (!IsSet)
				{
					Collection.FinalizeEntry(Key, this);
					IsSet = true;
				}
			}
		}

		public bool IsSet
		{
			get;
			private set;
		}

		public ICollectionValue<G> Collection { get; private set; }

		public IValuable<G> Key { get; set; }

		public int Index { get; set; }
		public IValueProvider<G> ValueProvider { get; private set; }

		public void Undeclare()
		{
			Collection.Remove(Key, ValueProvider);
		}
	}
}
