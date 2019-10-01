using Synfron.Staxe.Executor.Pointers;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Executor.Values
{
	public sealed class DefaultArrayValue<G> : DefaultCollectionValue<G> where G : IGroupState<G>, new()
	{
		private readonly List<EntryValuePointer<G>> _valuesList = null;

		public DefaultArrayValue(int? initialCapacity = null)
		{
			_valuesList = initialCapacity != null ? new List<EntryValuePointer<G>>(initialCapacity.Value) : new List<EntryValuePointer<G>>();
			IsMap = false;
		}

		public DefaultArrayValue(IEnumerable<IValuable<G>> values, IValueProvider<G> valueProvider)
		{
			int index = 0;
			_valuesList = new List<EntryValuePointer<G>>(values.Select(value => new EntryValuePointer<G>(this, new DefaultDoubleValue<G>(index), index++, valueProvider, true) { Value = value }));
			IsMap = false;
		}

		public DefaultArrayValue(IEnumerable<ValuePointer<G>> values, IValueProvider<G> valueProvider)
		{
			int index = 0;
			_valuesList = new List<EntryValuePointer<G>>(values.Select(pointer => new EntryValuePointer<G>(this, new DefaultDoubleValue<G>(index), index++, valueProvider, true) { Value = pointer.Value }));
			IsMap = false;
		}

		public DefaultArrayValue(IEnumerable<KeyValuePair<IValuable<G>, ValuePointer<G>>> entries, IValueProvider<G> valueProvider) : this(entries.Select(entry => entry.Value), valueProvider)
		{
		}

		public override int Size
		{
			get
			{
				return _valuesList.Count;
			}
		}

		public override ValuePointer<G> GetAt(IValuable<G> value, IValueProvider<G> valueProvider)
		{
			return _valuesList[((IValue)value).ToInt()];
		}

		public override ValuePointer<G> Get(IValuable<G> key, bool createNonExistent, IValueProvider<G> valueProvider)
		{
			lock (this)
			{
				EntryValuePointer<G> pointer = null;
				int keyInt = (int)((DefaultDoubleValue<G>)key).Data;
				if (keyInt >= 0 && keyInt <= _valuesList.Count)
				{
					if (keyInt >= 0 && keyInt < _valuesList.Count)
					{
						pointer = _valuesList[keyInt];
					}
					else if (createNonExistent)
					{
						pointer = new EntryValuePointer<G>(this, key, keyInt, valueProvider);
					}
				}
				return pointer;
			}
		}

		public override void Remove(IValuable<G> key, IValueProvider<G> valueProvider)
		{
			lock (this)
			{
				int keyInt = (int)((DefaultDoubleValue<G>)key).Data;
				if (keyInt >= 0 && keyInt < _valuesList.Count)
				{
					_valuesList.RemoveAt(keyInt);
					for (int i = keyInt; i < _valuesList.Count; i++)
					{
						_valuesList[i].Index = i;
						_valuesList[i].Key = new DefaultDoubleValue<G>(i);
					}
				}
			}
		}

		public override void FinalizeEntry(IValuable<G> key, EntryValuePointer<G> entryValuePointer)
		{
			lock (this)
			{
				if (entryValuePointer.Index >= _valuesList.Count)
				{
					_valuesList.Add(entryValuePointer);
				}
				else
				{
					_valuesList[entryValuePointer.Index] = entryValuePointer;
				}
			}
		}

		public override List<KeyValuePair<IValuable<G>, ValuePointer<G>>> GetEntries()
		{
			return _valuesList.Select(pointer => new KeyValuePair<IValuable<G>, ValuePointer<G>>(pointer.Key, pointer)).ToList();
		}

		public override List<ValuePointer<G>> GetValues()
		{
			return _valuesList.Cast<ValuePointer<G>>().ToList();
		}

		public override List<IValuable<G>> GetKeys()
		{
			return _valuesList.Select(pointer => pointer.Key).ToList();
		}

		public override bool ContainsKey(IValuable<G> key, IValueProvider<G> valueProvider)
		{
			return Get(key, false, valueProvider) != null;
		}

		public override bool ContainsValue(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			return _valuesList.Any(pointer => pointer.Value.Equals(valuable));
		}

		public override int IndexOf(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			return _valuesList.FindIndex(pointer => pointer.Value.Equals(valuable));
		}

		public override IValuable<G> KeyOf(IValuable<G> valuable, IValueProvider<G> valueProvider)
		{
			return _valuesList.Find(pointer => pointer.Value.Equals(valuable))?.Key;
		}
	}
}
