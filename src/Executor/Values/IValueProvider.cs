using Synfron.Staxe.Executor.Pointers;
using System.Collections.Generic;

namespace Synfron.Staxe.Executor.Values
{
	public interface IValueProvider<G> where G : IGroupState<G>, new()
	{
		IValue<G> True { get; }
		IValue<G> False { get; }
		INullValue<G> Null { get; }
		IValue<G> GetInt(int data);
		IValue<G> GetLong(long data);
		IValue<G> GetDouble(double data);
		IValue<G> GetString(string data);
		IValue<G> GetBoolean(bool data);
		IValue<G> GetExternal(object data);
		INullValue<G> GetNull();
		ICollectionValue<G> GetCollection(int? size, int? mode);
		ICollectionValue<G> GetCollection(IEnumerable<KeyValuePair<IValuable<G>, ValuePointer<G>>> entries, int? mode);
		ICollectionValue<G> GetCollection(IEnumerable<ValuePointer<G>> entries, int? mode);
		IGroupValue<G> GetGroup(G groupState);
		IActionValue<G> GetAction(G groupState, int location);
		IValue<G> GetAsValue(object data);
		IValue<G> GetAsNullableString(string data);
		IValue<G> GetReducedValue(long value);
		IValue<G> GetReducedValue(double value);

	}
}
