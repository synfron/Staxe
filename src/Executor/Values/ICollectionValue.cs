using Synfron.Staxe.Executor.Pointers;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor.Values
{
	public interface ICollectionValue<G> : IValuable<G> where G : IGroupState<G>, new()
	{
		int Mode { get; }

		bool ContainsKey(IValuable<G> key, IValueProvider<G> valueProvider);
		bool ContainsValue(IValuable<G> valuable, IValueProvider<G> valueProvider);
		List<KeyValuePair<IValuable<G>, ValuePointer<G>>> GetEntries();
		List<IValuable<G>> GetKeys();
		List<ValuePointer<G>> GetValues();
		int IndexOf(IValuable<G> valuable, IValueProvider<G> valueProvider);
		IValuable<G> KeyOf(IValuable<G> valuable, IValueProvider<G> valueProvider);
		void Remove(IValuable<G> key, IValueProvider<G> valueProvider);
		void FinalizeEntry(IValuable<G> key, EntryValuePointer<G> entryValuePointer);
	}
}