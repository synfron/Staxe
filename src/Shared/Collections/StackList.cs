// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation and Joel Mueller licenses this file to you under the MIT license.
// Heavily modified for use in Staxe (https://github.com/synfron/Staxe)
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Synfron.Staxe.Shared.Collections
{
	/// <summary>
	/// A simple stack of objects.  Internally it is implemented as an array,
	/// so Push can be O(n).  Pop is O(1).
	/// </summary>
	[Serializable]
	public class StackList<T> : IEnumerable<T>, IList<T>, ICollection, IReadOnlyCollection<T>, IDisposable
	{
		[NonSerialized]
		private object _syncRoot;

		private T[] _array; // Storage for stack elements. Do not rename (binary serialization)
		private int _size; // Number of items in the stack. Do not rename (binary serialization)
		private int _version; // Used to keep enumerator in sync w/ collection. Do not rename (binary serialization)

		private const int DefaultCapacity = 10;

		#region Constructors

		/// <summary>
		/// Create a stack with the default initial capacity. 
		/// </summary>
		public StackList()
		{
			_array = new T[DefaultCapacity];
		}

		/// <summary>
		/// Create a stack with a specific initial capacity.  The initial capacity
		/// must be a non-negative number.
		/// </summary>
		public StackList(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("The number must be non-negative.");
			}
			_array = new T[DefaultCapacity];
		}

		#endregion

		/// <summary>
		/// The number of items in the stack.
		/// </summary>
		public int Count => _size;

		/// <summary>
		/// Gets or sets the element at the given index.
		/// </summary>
		T IList<T>.this[int index]
		{
			get
			{
				// Following trick can reduce the range check by one
				if ((uint)index >= (uint)_size)
				{
					throw new ArgumentOutOfRangeException("index", "Argument 'index' was out of the range of valid values.");
				}
				return _array[index];
			}

			set
			{
				if ((uint)index >= (uint)_size)
				{
					throw new ArgumentOutOfRangeException("index", "Argument 'index' was out of the range of valid values.");
				}
				_array[index] = value;
				_version++;
			}
		}

		/// <summary>
		/// Gets or sets the element at the given index.
		/// </summary>
		public ref T this[int index]
		{
			get
			{
				// Following trick can reduce the range check by one
				if ((uint)index >= (uint)_size)
				{
					throw new ArgumentOutOfRangeException("index", "Argument 'index' was out of the range of valid values.");
				}
				return ref _array[index];
			}
		}

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				}
				return _syncRoot;
			}
		}

		public bool IsReadOnly => throw new NotImplementedException();

		public int MaxSize { get; set; } = int.MaxValue;

		/// <summary>
		/// Removes all Objects from the Stack.
		/// </summary>
		public void Clear()
		{
			Array.Clear(_array, 0, _size); // clear the elements so that the gc can reclaim the references.
			_size = 0;
			_version++;
		}

		/// <summary>
		/// Compares items using the default equality comparer
		/// </summary>
		public bool Contains(T item)
		{
			// PERF: Internally Array.LastIndexOf calls
			// EqualityComparer<T>.Default.LastIndexOf, which
			// is specialized for different types. This
			// boosts performance since instead of making a
			// virtual method call each iteration of the loop,
			// via EqualityComparer<T>.Default.Equals, we
			// only make one virtual call to EqualityComparer.LastIndexOf.

			return _size != 0 && Array.LastIndexOf(_array, item, _size - 1) != -1;
		}

		/// <summary>
		/// Inserts an element into this list at a given index. The size of the list
		/// is increased by one. If required, the capacity of the list is doubled
		/// before inserting the new element.
		/// </summary>
		public void Insert(int index, T item)
		{
			// Note that insertions at the end are legal.
			if ((uint)index > (uint)_size)
			{
				throw new ArgumentOutOfRangeException("argument", "Insertion index was out of the range of valid values.");
			}

			if (_size == _array.Length) InsertWithReize(index, item);
			else if (index < _size)
			{
				Array.Copy(_array, index, _array, index + 1, _size - index);
				_array[index] = item;
				_size++;
				_version++;
			}
		}
		public void InsertRange(int index, StackList<T> items)
		{
			// Note that insertions at the end are legal.
			if ((uint)index > (uint)_size)
			{
				throw new ArgumentOutOfRangeException("argument", "Insertion index was out of the range of valid values.");
			}

			if (_size == _array.Length) InsertRangeWithReize(index, items);
			else if (index < _size)
			{
				Array.Copy(_array, index, _array, index + items._size, _size - index);
				Array.Copy(items._array, 0, _array, index, items._size);
				_size++;
				_version++;
			}
		}

		private void InsertWithReize(int index, T item)
		{
			if (_size + 1 > MaxSize)
			{
				ThrowForMaxSize();
			}
			T[] array = _array;
			T[] newArray = new T[2 * _size];
			Array.Copy(array, newArray, index);
			Array.Copy(array, index, array, index + 1, _size - index);
			_array = newArray;
			newArray[_size] = item;
			_version++;
			_size++;
		}

		private void InsertRangeWithReize(int index, StackList<T> items)
		{
			if (_size + items.Count > MaxSize)
			{
				ThrowForMaxSize();
			}
			T[] array = _array;
			T[] newArray = new T[2 * _size];
			Array.Copy(array, newArray, index);
			Array.Copy(items._array, 0, array, index, items._size);
			Array.Copy(array, index, array, index + items.Count, _size - index);
			_array = newArray;
			_version++;
			_size++;
		}

		// Copies the stack into an array.
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}

			if (array.Length - arrayIndex < _size)
			{
				throw new ArgumentException("Array plus offset too small.");
			}

			Debug.Assert(array != _array);
			int srcIndex = 0;
			int dstIndex = arrayIndex;
			while (srcIndex < _size)
			{
				array[dstIndex++] = _array[srcIndex++];
			}
		}

		public void CopyTo(Span<T> span)
		{
			if (span.Length < _size)
			{
				throw new ArgumentException("Destination too short.");
			}

			int srcIndex = 0;
			int dstIndex = 0;
			while (srcIndex < _size)
			{
				span[dstIndex++] = _array[srcIndex++];
			}
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}

			if (array.Rank != 1)
			{
				throw new ArgumentException("Multi-dimensional arrays are not supported.");
			}

			if (array.GetLowerBound(0) != 0)
			{
				throw new ArgumentException("Arrays with a non-zero lower bound are not supported.", "array");
			}

			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}

			if (array.Length - arrayIndex < _size)
			{
				throw new ArgumentException("Invalid offset length.");
			}

			try
			{
				Array.Copy(_array, 0, array, arrayIndex, _size);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Invalid array type.");
			}
		}

		/// <summary>
		/// Returns an IEnumerator for this PooledStack.
		/// </summary>
		/// <returns></returns>
		public Enumerator GetEnumerator()
			=> new Enumerator(this);

		/// <internalonly/>
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
			=> new Enumerator(this);

		IEnumerator IEnumerable.GetEnumerator()
			=> new Enumerator(this);

		public void TrimExcess()
		{
			if (_size == 0)
			{
				_array = Array.Empty<T>();
				_version++;
				return;
			}

			T[] newArray = new T[_size];
			if (newArray.Length < _array.Length)
			{
				Array.Copy(_array, newArray, _size);
				_array = newArray;
				_version++;
			}
		}

		/// <summary>
		/// Returns the top object on the stack without removing it.  If the stack
		/// is empty, Peek throws an InvalidOperationException.
		/// </summary>
		public T Last()
		{
			int size = _size - 1;
			T[] array = _array;

			if ((uint)size >= (uint)array.Length)
			{
				ThrowForEmptyStack();
			}

			return array[size];
		}

		public T LastOrDefault()
		{
			int size = _size - 1;
			T[] array = _array;

			return (uint)size >= (uint)array.Length ? (default) : array[size];
		}

		/// <summary>
		/// Pops an item from the top of the stack.  If the stack is empty, Pop
		/// throws an InvalidOperationException.
		/// </summary>
		public T TakeLast()
		{
			int size = _size - 1;
			T[] array = _array;

			// if (_size == 0) is equivalent to if (size == -1), and this case
			// is covered with (uint)size, thus allowing bounds check elimination 
			// https://github.com/dotnet/coreclr/pull/9773
			if ((uint)size >= (uint)array.Length)
			{
				ThrowForEmptyStack();
			}

			_version++;
			_size = size;
			T item = array[size];
			array[size] = default;     // Free memory quicker.
			return item;
		}

		/// <summary>
		/// Pops an item from the top of the stack.  If the stack is empty, Pop
		/// throws an InvalidOperationException.
		/// </summary>
		public void RemoveLast()
		{
			int size = _size - 1;
			T[] array = _array;

			// if (_size == 0) is equivalent to if (size == -1), and this case
			// is covered with (uint)size, thus allowing bounds check elimination 
			// https://github.com/dotnet/coreclr/pull/9773
			if ((uint)size >= (uint)array.Length)
			{
				ThrowForEmptyStack();
			}

			_version++;
			_size = size;
			array[size] = default;     // Free memory quicker.
		}

		/// <summary>
		/// Pushes an item to the top of the stack.
		/// </summary>
		public void Add(T item)
		{
			int size = _size;
			T[] array = _array;

			if ((uint)size < (uint)array.Length)
			{
				array[size] = item;
				_version++;
				_size = size + 1;
			}
			else
			{
				AddWithResize(item);
			}
		}
		public void UnsafeSet(int index, T item)
		{
			_array[index] = item;
			_version++;
		}

		public int Reserve(int count)
		{
			int originalSize = _size;
			if (count > 0)
			{
				T[] array = _array;

				if ((uint)originalSize + count < (uint)array.Length)
				{
					_size = originalSize + count;
				}
				else
				{
					if (originalSize + count > MaxSize)
					{
						ThrowForMaxSize();
					}
					_size = originalSize + count;
					T[] newArray = new T[_size * 2 + count];
					Array.Copy(array, newArray, originalSize);
					_array = array;
				}
				_version++;
			}
			return originalSize;
		}

		public void AddRange(StackList<T> items)
		{
			AddRange(items._array, items.Count);
		}

		public void AddRange(T[] items, int count)
		{
			Debug.Assert(items != _array);
			int size = _size;
			T[] array = _array;

			if ((uint)size + count < (uint)array.Length)
			{
				Array.Copy(items, 0, array, _size, count);
				_version++;
				_size = size + count;
			}
			else
			{
				AddRangeWithResize(items, count);
			}
		}

		public void SetLast(T item)
		{
			if (_size > 0)
			{
				_array[_size - 1] = item;
			}
			else
			{
				Add(item);
			}
		}

		private void AddWithResize(T item)
		{
			if (_size + 1 > MaxSize)
			{
				ThrowForMaxSize();
			}
			T[] array = _array;
			T[] newArray = new T[2 * _size];
			Array.Copy(array, newArray, _size);
			_array = newArray;
			newArray[_size] = item;
			_version++;
			_size++;
		}

		private void AddRangeWithResize(T[] items, int count)
		{
			if (_size + count > MaxSize)
			{
				ThrowForMaxSize();
			}
			T[] array = _array;
			T[] newArray = new T[2 * _size + count];
			Array.Copy(array, newArray, _size);
			Array.Copy(items, 0, newArray, _size, count);
			_array = newArray;
			_version++;
			_size += count;
		}

		/// <summary>
		/// Copies the Stack to an array
		/// </summary>
		public T[] ToArray()
		{
			if (_size == 0)
				return Array.Empty<T>();

			T[] objArray = new T[_size];
			Array.Copy(_array, 0, objArray, 0, _size);
			return objArray;
		}

		/// <summary>
		/// Removes a range of elements from this list.
		/// </summary>
		public void RemoveRange(int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index", "The number must be non-negative.");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count", "The number must be non-negative.");

			if (_size - index < count)
				throw new ArgumentException("Invalid offset length.");

			if (count > 0)
			{
				_size -= count;
				if (index < _size)
				{
					Array.Copy(_array, index + count, _array, index, _size - index);
				}

				_version++;

				Array.Clear(_array, _size, count);
			}
		}

		private void ThrowForEmptyStack()
		{
			Debug.Assert(_size == 0);
			throw new InvalidOperationException("Stack was empty.");
		}

		private void ThrowForMaxSize()
		{
			throw new InvalidOperationException("Stack size max has been reached.");
		}

		public void Dispose()
		{
			_array = new T[DefaultCapacity];
			_size = 0;
			_version++;
		}

		public int IndexOf(T item)
		{
			return Array.LastIndexOf(_array, item, _size - 1);
		}

		public void RemoveAt(int index)
		{
			RemoveRange(index, 1);
		}

		public bool Remove(T item)
		{
			int index = Array.LastIndexOf(_array, item, _size - 1);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario")]
		public struct Enumerator : IEnumerator<T>, IEnumerator
		{
			private readonly StackList<T> _stack;
			private readonly int _version;
			private int _index;
			private T _currentElement;

			internal Enumerator(StackList<T> stack)
			{
				_stack = stack;
				_version = stack._version;
				_index = -2;
				_currentElement = default;
			}

			public void Dispose()
			{
				_index = -1;
			}

			public bool MoveNext()
			{
				bool retval;
				if (_version != _stack._version) throw new InvalidOperationException("Collection was modified during enumeration.");
				if (_index == -2)
				{  // First call to enumerator.
					_index = 0;
					retval = (_index < _stack._size);
					if (retval)
						_currentElement = _stack._array[_index];
					return retval;
				}
				if (_index == _stack._size)
				{  // End of enumeration.
					return false;
				}

				retval = (++_index < _stack._size);
				_currentElement = retval ? _stack._array[_index] : (default);
				return retval;
			}

			public T Current
			{
				get
				{
					if (_index > _stack._size)
						ThrowEnumerationNotStartedOrEnded();
					return _currentElement;
				}
			}

			private void ThrowEnumerationNotStartedOrEnded()
			{
				Debug.Assert(_index == _stack._size || _index == -2);
				throw new InvalidOperationException(_index == -2 ? "Enumeration was not started." : "Enumeration has ended.");
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			void IEnumerator.Reset()
			{
				if (_version != _stack._version) throw new InvalidOperationException("Collection was modified during enumeration.");
				_index = -2;
				_currentElement = default;
			}
		}
	}
}
