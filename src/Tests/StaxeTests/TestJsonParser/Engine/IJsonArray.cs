namespace StaxeTests.TestJsonParser.Engine
{
	public interface IJsonArray : IJsonItem
	{
		IJsonItem this[int index] { get; }

		int Count { get; }
	}
}
