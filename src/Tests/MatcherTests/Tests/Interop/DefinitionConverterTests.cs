using Newtonsoft.Json;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Input.Actions;
using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.IO;
using Xunit;

namespace MatcherTests.Tests.Interop
{
    public class DefinitionConverterTests
    {
        [Fact]
        public void DefinitionConverter_Convert()
        {
            string definition = File.ReadAllText("Files/EagerIndexTestDefinition.json");
            LanguageMatcherDefinition languageMatcherDefinition = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definition);

            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            LanguageMatcherDefinition newLanguageDefinition = DefinitionConverter.Convert(languageMatcher);

            Assert.Equal(languageMatcherDefinition, newLanguageDefinition, new LanguageMatcherDefinitionComparer());
        }

        [Fact]
        public void DefinitionConverter_ConvertLanguageMatcher()
        {
            LanguageMatcherDefinition languageDefinition = new LanguageMatcherDefinition
            {
                Name = "TestLang",
                IndexingMode = IndexingMode.Eager,
                LogMatches = true,
                StartingFragment = "TestFrag1",
                Actions = new[]
                {
                    new MatcherActionDefinition
                    {
                        Name = "TestAction1",
                        ActionType = MatcherActionType.UpdateVariable,
                        Change = VariableUpdateAction.Subtract,
                        FirstVariableName = "Var1",
                        SecondVariableName = "Var2"
                    },
                    new MatcherActionDefinition
                    {
                        Name = "TestAction2",
                        ActionType = MatcherActionType.Assert,
                        Assert = AssertType.LessThanOrEquals,
                        FirstVariableName = "Var1",
                        SecondVariableName = "Var2"
                    },
                    new MatcherActionDefinition
                    {
                        Name = "TestAction3",
                        ActionType = MatcherActionType.CreateVariable,
                        Source = VariableValueSource.PartsText,
                        FirstVariableName = "Var1",
                        Value = 20
                    },
                    new MatcherActionDefinition
                    {
                        Name = "TestAction4",
                        ActionType = MatcherActionType.CreateVariable,
                        Source = VariableValueSource.PartsText,
                        FirstVariableName = "Var2",
                        Value = 10
                    }
                },
                Patterns = new[]
                {
                    new PatternMatcherDefinition
                    {
                        Name = "TestPattern2",
                        Pattern = "B"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "TestPattern3",
                        Pattern = "C"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "TestPattern4",
                        Pattern = "D"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "TestPattern5",
                        Pattern = "E"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "TestPattern6",
                        Fragment = "TestFrag2",
                        IsNoise = true
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "TestPattern1",
                        Pattern = "A",
                        IsAuxiliary = true,
                        IsNoise = true,
                        Mergable = true
                    }
                },
                Fragments = new[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "TestFrag1"
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "TestFrag2",
                        Actions = new[]
                        {
                            "TestAction1",
                            "TestAction2",
                            "TestAction3",
                            "TestAction4"
                        },
                        Start = "TestPattern1",
                        End = "TestPattern2",
                        ExpressionMode = ExpressionMode.BinaryTree,
                        ExpressionOrder = 1,
                        BoundsAsParts = true,
                        DiscardBounds = true,
                        PartsDelimiter = "TestPattern3",
                        PartsDelimiterRequired = true,
                        Cacheable = true,
                        ClearCache = true,
                        FallThroughMode = FallThroughMode.All,
                        IsNoise = true,
                        MinMatchedParts = 3,
                        PartsMatchMode = MatchMode.Ordered,
                        Negate = true,
                        PartsPadding = "TestPattern4",
                        Parts =
                        {
                            "[TestFrag1]",
                            "TestPattern5"
                        }
                    }
                }
            };

            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageDefinition);
            LanguageMatcherDefinition newLanguageDefinition = DefinitionConverter.Convert(languageMatcher);

            Assert.Equal(languageDefinition, newLanguageDefinition, new LanguageMatcherDefinitionComparer());
        }
    }
}
