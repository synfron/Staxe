using StaxeTests.Shared.Matcher;
using StaxeTests.TestJsonParser.Engine;
using Synfron.Staxe.Matcher;
using System.IO;
using Xunit;

namespace StaxeTests.TestJsonParser.Tests
{
	public class PostGen_JsonParserTests : JsonParserTests
	{
		public PostGen_JsonParserTests()
		{
			LanguageMatchEngine = Synfron.Staxe.Matcher.LanguageMatchEngine.Build(LanguageMatcherProvider.GetJsonMatcher());
		}
	}
	public class PreGen_JsonParserTests : JsonParserTests
	{
		public PreGen_JsonParserTests()
		{
			LanguageMatchEngine = GeneratedLanguageMatchEngineRegistry.GetJsonMatchEngine();
		}
	}

	public abstract class JsonParserTests
	{

		protected ILanguageMatchEngine LanguageMatchEngine { get; set; }

		[Fact]
		public void GetValuesTest()
		{
			string json = File.ReadAllText("TestJsonParser/Files/PersonInfo.json");
			IJsonItem jsonItem = new JsonParser(LanguageMatchEngine).Parse(json);

			IJsonObject personInfo = (IJsonObject)jsonItem;
			IJsonObject address = (IJsonObject)personInfo["address"];
			IJsonArray phoneNumbers = (IJsonArray)personInfo["phoneNumber"];
			IJsonObject faxNumber2 = (IJsonObject)phoneNumbers[1];

			Assert.Equal("Smith", ((IJsonValue)personInfo["lastName"]).Value);
			Assert.Equal(25d, ((IJsonValue)personInfo["age"]).Value);
			Assert.True((bool)((IJsonValue)address["owner"]).Value);
			Assert.Equal("646 555-4567", ((IJsonValue)faxNumber2["number"]).Value);
		}
	}
}
