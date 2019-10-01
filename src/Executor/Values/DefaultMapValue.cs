using Synfron.Staxe.Executor.Pointers;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Executor.Values
{
	public sealed class DefaultMapValue<G> : DefaultCollectionValue<G> where G : IGroupState<G>, new()
	{
		private readonly Dictionary<IValuable<G>, EntryValuePointer<G>> _valuesDictionary = null;

		public DefaultMapValue(int? initialCapacity = null)
		{
			IsMap = true;
			_valuesDictionary = initialCapacity != null ? new Dictionary<IValuable<G>, EntryValuePointer<G>>(initialCapacity.Value) : new Dictionary<IValuable<G>, EntryValuePointer<G>>();
		}

		public DefaultMapValue(IEnumerable<KeyValuePair<IValuable<G>, ValuePointer<G>>> entries, IValueProvider<G> valueProvider)
		{
			IsMap = true;
			int index = 0;
			_valuesDictionary = entries.ToDictionary(entry => entry.Key, entry => new EntryValuePointer<G>(this, entry.Key, index++, valueProvider, true) { Value = entry.Value.Value });
		}

		public override int Size
		{
			get
			{
				return _valuesDictionary.Count;
			}
		}

		public override ValuePointer<G> GetAt(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			return _valuesDictionary.ElementAt(((IValue)value).ToInt()).Value;
		}

		public override ValuePointer<G> Get(IValuable<G> key, bool createNonExistent, IValueProvider<G> valueProvider)
		{
			lock (this)
			{
				EntryValuePointer<G> pointer = null;
				if (IsMap && !_valuesDictionary.TryGetValue(key, out pointer))
				{
					if (createNonExistent)
					{
						pointer = new EntryValuePointer<G>(this, key, _valuesDictionary.Count, valueProvider);
					}
				}
				return pointer;
			}
		}

		public override void Remove(IValuable<G> key, IValueProvider<G> valueProvider)
		{
			lock (this)
			{
				if (_valuesDictionary.TryGetValue(key, out EntryValuePointer<G> pointer))
				{
					_valuesDictionary.Remove(key);
				}
			}
		}

		public override void FinalizeEntry(IValuable<G> key, EntryValuePointer<G> entryValuePointer)
		{
			lock (this)
			{
				_valuesDictionary[key] = entryValuePointer;
			}
		}

		public override List<KeyValuePair<IValuable<G>, ValuePointer<G>>> GetEntries()
		{
			return _valuesDictionary.Values.Select(pointer => new KeyValuePair<IValuable<G>, ValuePointer<G>>(pointer.Key, pointer)).ToList();
		}

		public override List<ValuePointer<G>> GetValues()
		{
			return _valuesDictionary.Values.Cast<ValuePointer<G>>().ToList();
		}

		public override List<IValuable<G>> GetKeys()
		{
			return _valuesDictionary.Keys.ToList();
		}

		public override bool ContainsKey(IValuable<G> key, IValueProvider<G> valueProvider)
		{
			return Get(key, false, valueProvider) != null;
		}

		public override bool ContainsValue(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			return _valuesDictionary.Values.Any(pointer => pointer.Value.Equals(valuable));
		}

		public override int IndexOf(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			int index = 0;
			foreach (EntryValuePointer<G> pointer in _valuesDictionary.Values)
			{
				if (pointer.Value.Equals(valuable))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public override IValuable<G> KeyOf(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			return _valuesDictionary.Values.FirstOrDefault(pointer => pointer.Value.Equals(valuable))?.Key;
		}
	}
}
