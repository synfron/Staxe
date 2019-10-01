namespace StaxeTests.TestJsonParser.Engine
{
	public interface IJsonValue : IJsonItem
	{
		object Value { get; }
	}
}
