using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Shared.Exceptions;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor.Values
{
	public class DefaultValueProvider<G> : IValueProvider<G> where G : IGroupState<G>, new()
	{
		private static readonly DefaultBooleanValue<G> _true = new DefaultBooleanValue<G>(true);
		private static readonly DefaultBooleanValue<G> _false = new DefaultBooleanValue<G>(false);
		private static readonly DefaultNullValue<G> _null = new DefaultNullValue<G>();

		public virtual IValue<G> True => _true;

		public virtual IValue<G> False => _false;

		public virtual INullValue<G> Null => _null;

		public virtual IActionValue<G> GetAction(G groupState, int location)
		{
			return new DefaultActionValue<G>(groupState, location);
		}

		public virtual ICollectionValue<G> GetCollection(int? initialCapacity, int? mode)
		{
			switch (mode ?? 0)
			{
				case 1:
					return new DefaultArrayValue<G>(initialCapacity);
				case 2:
					return new DefaultMapValue<G>(initialCapacity);
				default:
					return new DefaultArrayMapValue<G>(initialCapacity);
			}
		}

		public virtual ICollectionValue<G> GetCollection(IEnumerable<KeyValuePair<IValuable<G>, ValuePointer<G>>> entries, int? mode)
		{
			switch (mode ?? 0)
			{
				case 1:
					return new DefaultArrayValue<G>(entries, this);
				case 2:
					return new DefaultMapValue<G>(entries, this);
				case 0:
				default:
					return new DefaultArrayMapValue<G>(entries, this);
			}
		}

		public virtual ICollectionValue<G> GetCollection(IEnumerable<ValuePointer<G>> values, int? mode)
		{
			switch (mode ?? 0)
			{
				case 1:
					return new DefaultArrayValue<G>(values, this);
				case 2:
					throw new EngineRuntimeException("No implementation for the specified collection mode");
				case 0:
				default:
					return new DefaultArrayMapValue<G>(values, this);
			}
		}

		public virtual IValue<G> GetAsNullableString(string data)
		{
			return data != null ? GetString(data) : Null;
		}

		public virtual IValue<G> GetAsValue(object data)
		{
			switch (data)
			{
				case null:
					return Null;
				case int dataInt:
					return GetInt(dataInt);
				case bool dataBool:
					return GetBoolean(dataBool);
				case double dataDouble:
					return GetReducedValue(dataDouble);
				case long dataLong:
					return GetReducedValue(dataLong);
				case string dataString:
					return GetString(dataString);
				default:
					return GetExternal(data);
			}
		}

		public virtual IValue<G> GetExternal(object data)
		{
			throw new EngineRuntimeException("No implementation for externals");
		}

		public virtual IValue<G> GetBoolean(bool data)
		{
			return data ? True : False;
		}

		public virtual IValue<G> GetDouble(double data)
		{
			return new DefaultDoubleValue<G>(data);
		}

		public virtual IGroupValue<G> GetGroup(G groupState)
		{
			return new DefaultGroupValue<G>(groupState);
		}

		public virtual IValue<G> GetInt(int data)
		{
			return new DefaultIntValue<G>(data);
		}

		public virtual IValue<G> GetLong(long data)
		{
			return new DefaultLongValue<G>(data);
		}

		public virtual INullValue<G> GetNull()
		{
			return Null;
		}

		public virtual IValue<G> GetString(string data)
		{
			return new DefaultStringValue<G>(data);
		}

		public virtual IValue<G> GetReducedValue(long value)
		{
			return value >= int.MinValue && value <= int.MaxValue ? GetInt((int)value) : GetLong(value);
		}

		public virtual IValue<G> GetReducedValue(double value)
		{
			if (value % 1 == 0)
			{
				if (value >= int.MinValue && value <= int.MaxValue)
				{
					return GetInt((int)value);
				}
				else if (value >= long.MinValue && value <= long.MaxValue)
				{
					return GetLong((long)value);
				}
			}
			return GetDouble(value);
		}
	}
}
