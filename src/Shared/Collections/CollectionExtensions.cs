using System.Collections.Generic;

namespace Synfron.Staxe.Shared.Collections
{
	public static class CollectionExtension
	{
		public static T TakeLast<T>(this List<T> list)
		{
			T value = default;
			if (list.Count > 0)
			{
				value = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
			}
			return value;
		}

		public static void RemoveLast<T>(this List<T> list)
		{
			if (list.Count > 0)
			{
				list.RemoveAt(list.Count - 1);
			}
		}

		public static void SetLast<T>(this IList<T> list, T value)
		{
			if (list.Count > 0)
			{
				list[list.Count - 1] = value;
			}
			else
			{
				list.Add(value);
			}
		}

		public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dictionary, K key)
		{
			dictionary.TryGetValue(key, out V value);
			return value;
		}
	}
}
