using Synfron.Staxe.Executor.Pointers;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Executor.Values
{
	public sealed class DefaultArrayMapValue<G> : DefaultCollectionValue<G> where G : IGroupState<G>, new()
	{
		private Dictionary<IValuable<G>, EntryValuePointer<G>> _valuesDictionary = null;
		private readonly List<EntryValuePointer<G>> _valuesList = null;

		public DefaultArrayMapValue(int? initialCapacity = null)
		{
			_valuesList = initialCapacity != null ? new List<EntryValuePointer<G>>(initialCapacity.Value) : new List<EntryValuePointer<G>>();
		}

		public DefaultArrayMapValue(IEnumerable<IValuable<G>> values, IValueProvider<G> valueProvider)
		{
			int index = 0;
			_valuesList = new List<EntryValuePointer<G>>(values.Select(value => new EntryValuePointer<G>(this, valueProvider.GetInt(index), index++, valueProvider, true) { Value = value }));
		}

		public DefaultArrayMapValue(IEnumerable<ValuePointer<G>> values, IValueProvider<G> valueProvider)
		{
			IsMap = false;
			int index = 0;
			_valuesList = new List<EntryValuePointer<G>>(values.Select(pointer => new EntryValuePointer<G>(this, valueProvider.GetInt(index), index++, valueProvider, true) { Value = pointer.Value }));
		}

		public DefaultArrayMapValue(IEnumerable<KeyValuePair<IValuable<G>, ValuePointer<G>>> entries, IValueProvider<G> valueProvider)
		{
			int index = 0;
			_valuesList = new List<EntryValuePointer<G>>();
			foreach (KeyValuePair<IValuable<G>, ValuePointer<G>> entry in entries)
			{
				if (!IsMap && AsArrayIndex(entry.Key) != index)
				{
					IsMap = true;
				}
				EntryValuePointer<G> valuePointer = new EntryValuePointer<G>(this, entry.Key, index++, valueProvider, true) { Value = entry.Value.Value };
				_valuesList.Add(valuePointer);

			}

			if (IsMap)
			{
				_valuesDictionary = new Dictionary<IValuable<G>, EntryValuePointer<G>>();
				foreach (EntryValuePointer<G> pointer in _valuesList)
				{
					_valuesDictionary[pointer.Key] = pointer;
				}
			}
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
				if (!IsMap)
				{
					int? keyInt = AsArrayIndex(key);
					if (keyInt != null && keyInt.Value >= 0 && keyInt.Value <= _valuesList.Count)
					{
						if (keyInt.Value >= 0 && keyInt.Value < _valuesList.Count)
						{
							pointer = _valuesList[keyInt.Value];
						}
						else if (createNonExistent)
						{
							pointer = new EntryValuePointer<G>(this, key, keyInt.Value, valueProvider);
						}
					}
					else if (createNonExistent)
					{
						IsMap = true;
						_valuesDictionary = new Dictionary<IValuable<G>, EntryValuePointer<G>>();
						for (int i = 0; i < _valuesList.Count; i++)
						{
							_valuesDictionary[valueProvider.GetInt(i)] = _valuesList[i];
						}
					}
				}
				if (IsMap && !_valuesDictionary.TryGetValue(key, out pointer))
				{
					if (createNonExistent)
					{
						pointer = new EntryValuePointer<G>(this, key, _valuesList.Count, valueProvider);
					}
				}
				return pointer;
			}
		}

		public override void Remove(IValuable<G> key, IValueProvider<G> valueProvider)
		{
			lock (this)
			{
				if (IsMap)
				{
					if (_valuesDictionary.TryGetValue(key, out EntryValuePointer<G> pointer))
					{
						int index = pointer.Index;
						_valuesDictionary.Remove(key);
						_valuesList.RemoveAt(index);
						for (int i = index; i < _valuesList.Count; i++)
						{
							_valuesList[i].Index = i;
						}
					}
				}
				else
				{
					int index = AsArrayIndex(key).Value;
					if (index >= 0 && index < _valuesList.Count)
					{
						_valuesList.RemoveAt(index);
						for (int i = index; i < _valuesList.Count; i++)
						{
							_valuesList[i].Index = i;
							_valuesList[i].Key = valueProvider.GetInt(i);
						}
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
				if (IsMap)
				{
					_valuesDictionary[key] = entryValuePointer;
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
			return IsMap ? _valuesDictionary.Keys.ToList() : _valuesList.Select(pointer => pointer.Key).ToList();
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

		private int? AsArrayIndex(IValuable<G> valuable)
		{
			return (valuable as IValue<G, int>)?.Data;
		}
	}
}
