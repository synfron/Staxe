using Newtonsoft.Json;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Interop.Model;
using System.IO;
using Xunit;

namespace MatcherTests.Interop.Model.Tests
{
	public class DefinitionConverterTests
	{
		[Fact]
		public void DefinitionConverter_Convert()
		{
			string definition = File.ReadAllText("Interop/Json/Files/TestDefinition.json");
			LanguageMatcherDefinition languageMatcherDefinition = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definition);

			LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
			LanguageMatcherDefinition newLanguageDefinition = DefinitionConverter.Convert(languageMatcher);

			Assert.Equal(languageMatcherDefinition, newLanguageDefinition, new LanguageMatcherDefinitionComparer());
		}
	}
}
