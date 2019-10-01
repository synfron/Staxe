using System.Collections.Generic;

namespace StaxeTests.TestJsonParser.Engine
{
	public interface IJsonObject : IJsonItem
	{
		IJsonItem this[string key] { get; }

		int Count { get; }
		ICollection<string> Keys { get; }

		bool ContainsKey(string key);
	}
}
