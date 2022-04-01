using MatcherTests.Shared;
using Newtonsoft.Json;
using Synfron.Staxe.Matcher;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Interop.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace MatcherTests.Tests.Engine
{
    public class PostGen_EngineTests : EngineTests
    {
        public override GenerationType GenerationType => GenerationType.PostGen;
    }

    public class PreGen_EngineTests : EngineTests
    {
        public override GenerationType GenerationType => GenerationType.PreGen;
    }

    public abstract class EngineTests
    {

        public abstract GenerationType GenerationType { get; }

        [Fact]
        public void LazyIndexTest()
        {
            string definition = File.ReadAllText("Files/LazyIndexTestDefinition.json");
            string script = File.ReadAllText("Files/Parsables/JibberishScript.lz");
            LanguageMatcherDefinition languageMatcherDefinition = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definition);
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);

            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
        }

        [Fact]
        public void EagerIndexTest()
        {
            string definition = File.ReadAllText("Files/EagerIndexTestDefinition2.json");
            string script = File.ReadAllText("Files/Parsables/JibberishScript.lz");
            LanguageMatcherDefinition languageMatcherDefinition = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definition);
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script.TrimEnd());

            Assert.True(result.Success);
        }

        [Fact]
        public void FragmentPatternMatcherTest()
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = IndexingMode.Eager,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "FragB",
                        Fragment = "Bs"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b",
                        IsAuxiliary = true
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "A", "[Bs]", "C" },
                        PartsMatchMode = MatchMode.Ordered
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Bs",
                        Parts = new [] { "B", "B" },
                        PartsMatchMode = MatchMode.Ordered
                    }
                }
            };
            string script = @"abbc";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>a</A><Bs><B>b</B><B>b</B></Bs><C>c</C></Start>", result.MatchData.ToXml()));
        }

        [Fact]
        public void FragmentPatternMatcherNoiseTest()
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = IndexingMode.Eager,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "FragB",
                        Fragment = "Bs",
                        IsNoise = true
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b",
                        IsAuxiliary = true
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "A", "C" },
                        PartsMatchMode = MatchMode.Ordered
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Bs",
                        Parts = new [] { "B", "B" },
                        PartsMatchMode = MatchMode.Ordered
                    }
                }
            };
            string script = @"ac";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>a</A><C>c</C></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void SimpleTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "A", "B", "C" },
                        PartsMatchMode = MatchMode.Ordered
                    }
                }
            };
            string script = @"abc";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>a</A><B>b</B><C>c</C></Start>", result.MatchData.ToXml()));
        }

        [Fact]
        public void PatternNoiseTest()
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = IndexingMode.Eager,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition 
                    {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b",
                        IsNoise = true
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "A", "C" },
                        PartsMatchMode = MatchMode.Ordered
                    }
                }
            };
            string script = @"ac";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>a</A><C>c</C></Start>", result.MatchData.ToXml()));
        }

        [Fact]
        public void MergableTest()
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = IndexingMode.Eager,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition
                    {
                        Name = "A",
                        Pattern = "a+",
                        Mergable = true
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b",
                        IsNoise = true
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    },
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "A", "C" },
                        PartsMatchMode = MatchMode.Ordered
                    }
                }
            };
            string script = @"abac";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>aa</A><C>c</C></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void BoundsAsPartsTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition
                    {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    },
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Start = "A",
                        Parts = new [] { "B" },
                        End = "C",
                        BoundsAsParts = true,
                        PartsMatchMode = MatchMode.Multiple
                    }
                }
            };
            string script = @"abc";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>a</A><B>b</B><C>c</C></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void BoundsTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition
                    {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    },
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Start = "A",
                        Parts = new [] { "B" },
                        End = "C",
                        PartsMatchMode = MatchMode.Multiple
                    }
                }
            };
            string script = @"abc";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><B>b</B></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void DiscardBoundsTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition
                    {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    },
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Start = "A",
                        Parts = new [] { "A" ,"B", "C" },
                        DiscardBounds = true,
                        PartsMatchMode = MatchMode.Ordered
                    }
                }
            };
            string script = @"abc";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>a</A><B>b</B><C>c</C></Start>", result.MatchData.ToXml()));
        }

        [Fact]
        public void PreMatchSkipAuxiliaryTest()
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = IndexingMode.Eager,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "aB",
                        Pattern = "b",
                        IsAuxiliary = true
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "A", "aB", "C", "B" },
                        PartsMatchMode = MatchMode.Multiple
                    }
                }
            };
            string script = @"abc";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>a</A><B>b</B><C>c</C></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void LessThanMinMatchedPartsTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[MoreBs]", "[Bs]" },
                        PartsMatchMode = MatchMode.One
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "MoreBs",
                        Parts = new [] { "B" },
                        PartsMatchMode = MatchMode.Multiple,
                        MinMatchedParts = 3
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Bs",
                        Parts = new [] { "B" },
                        PartsMatchMode = MatchMode.Multiple
                    }
                }
            };
            string script = @"bb";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><Bs><B>b</B><B>b</B></Bs></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void EqualToMinMatchedMultiplePartsTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[MoreBs]", "[Bs]" },
                        PartsMatchMode = MatchMode.One
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "MoreBs",
                        Parts = new [] { "B" },
                        PartsMatchMode = MatchMode.Multiple,
                        MinMatchedParts = 2
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Bs",
                        Parts = new [] { "B" },
                        PartsMatchMode = MatchMode.Multiple
                    }
                }
            };
            string script = @"bb";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><MoreBs><B>b</B><B>b</B></MoreBs></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void EqualToMinMatchedOrderedPartsTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[MoreBs]", "[Bs]" },
                        PartsMatchMode = MatchMode.One
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "MoreBs",
                        Parts = new [] { "B", "B", "B", "B" },
                        PartsMatchMode = MatchMode.Ordered,
                        MinMatchedParts = 2
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Bs",
                        Parts = new [] { "B" },
                        PartsMatchMode = MatchMode.Multiple
                    }
                }
            };
            string script = @"bb";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><MoreBs><B>b</B><B>b</B></MoreBs></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void NegateTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition
                    {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[NotBC]", "[NotCB]" },
                        PartsMatchMode = MatchMode.One
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "NotBC",
                        Parts = new [] { "[NotB]", "C" },
                        PartsMatchMode = MatchMode.Ordered
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "NotB",
                        Parts = new [] { "B" },
                        PartsMatchMode = MatchMode.One,
                        Negate = true
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "NotCB",
                        Parts = new [] { "[NotC]", "B" },
                        PartsMatchMode = MatchMode.Ordered
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "NotC",
                        Parts = new [] { "C" },
                        PartsMatchMode = MatchMode.One,
                        Negate = true
                    }
                }
            };
            string script = @"b";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><NotCB><B>b</B></NotCB></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void PartsDelimiterNotRequiredTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[WithDelimiter]", "[NoDelimiter]" },
                        PartsMatchMode = MatchMode.One
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "WithDelimiter",
                        Parts = new [] { "A", "C" },
                        PartsMatchMode = MatchMode.Ordered,
                        PartsDelimiter = "B"
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "NoDelimiter",
                        Parts = new [] { "A", "C" },
                        PartsMatchMode = MatchMode.Ordered,
                        PartsDelimiterRequired = false
                    }
                }
            };
            string script = @"ac";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><NoDelimiter><A>a</A><C>c</C></NoDelimiter></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void PartsDelimiterRequiredTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[WithDelimiter]", "[NoDelimiter]" },
                        PartsMatchMode = MatchMode.One
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "WithDelimiter",
                        Parts = new [] { "A", "C" },
                        PartsMatchMode = MatchMode.Ordered,
                        PartsDelimiter = "B"
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "NoDelimiter",
                        Parts = new [] { "A", "C" },
                        PartsMatchMode = MatchMode.Ordered
                    }
                }
            };
            string script = @"abc";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><WithDelimiter><A>a</A><C>c</C></WithDelimiter></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void FallThroughOneModeTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[AB]" },
                        PartsMatchMode = MatchMode.Multiple
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "AB",
                        Parts = new [] { "A", "B" },
                        PartsMatchMode = MatchMode.Ordered,
                        FallThroughMode = FallThroughMode.One,
                        MinMatchedParts = 1
                    },
                }
            };
            string script = @"aaba";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>a</A><AB><A>a</A><B>b</B></AB><A>a</A></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void FallThroughAllModeTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[AB]" },
                        PartsMatchMode = MatchMode.One
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "AB",
                        Parts = new [] { "A", "B" },
                        PartsMatchMode = MatchMode.Multiple,
                        FallThroughMode = FallThroughMode.All
                    },
                }
            };
            string script = @"abab";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><A>a</A><B>b</B><A>a</A><B>b</B></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void PartsPaddingTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "A",
                        Pattern = "a"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "B",
                        Pattern = "b"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "C",
                        Pattern = "c"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[Bs]", "[Cs]"  },
                        PartsMatchMode = MatchMode.Multiple
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Bs",
                        Parts = new [] { "B"  },
                        PartsMatchMode = MatchMode.Multiple,
                        PartsPadding = "A"
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Cs",
                        Parts = new [] { "C"  },
                        PartsMatchMode = MatchMode.Multiple,
                        PartsPadding = "A"
                    }
                }
            };
            string script = @"abbaccbba";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><Bs><B>b</B><B>b</B></Bs><Cs><C>c</C><C>c</C></Cs><Bs><B>b</B><B>b</B></Bs></Start>", result.MatchData.ToXml()));
        }

        [Theory]
        [InlineData(IndexingMode.Eager)]
        [InlineData(IndexingMode.Lazy)]
        [InlineData(IndexingMode.None)]
        public void BinaryTreeExpressionTest(IndexingMode indexingMode)
        {
            LanguageMatcherDefinition languageMatcherDefinition = new LanguageMatcherDefinition
            {
                Name = "Test",
                StartingFragment = "Start",
                IndexingMode = indexingMode,
                Patterns = new PatternMatcherDefinition[]
                {
                    new PatternMatcherDefinition {
                        Name = "E",
                        Pattern = "^"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "M",
                        Pattern = "\\*"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "D",
                        Pattern = "/"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "A",
                        Pattern = "\\+"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "S",
                        Pattern = "-"
                    },
                    new PatternMatcherDefinition
                    {
                        Name = "Number",
                        Pattern = "2"
                    }
                },
                Fragments = new FragmentMatcherDefinition[]
                {
                    new FragmentMatcherDefinition
                    {
                        Name = "Start",
                        Parts = new [] { "[Exponent]", "[Multiplication]", "[Division]", "[Addition]", "[Subtraction]", "Number"  },
                        ExpressionMode = ExpressionMode.BinaryTree
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Exponent",
                        Parts = new [] { "E", "Number" },
                        PartsMatchMode = MatchMode.Ordered,
                        ExpressionOrder = 1
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Multiplication",
                        Parts = new [] { "M", "Number" },
                        PartsMatchMode = MatchMode.Ordered,
                        ExpressionOrder = 2
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Division",
                        Parts = new [] { "D", "Number" },
                        PartsMatchMode = MatchMode.Ordered,
                        ExpressionOrder = 3
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Addition",
                        Parts = new [] { "A", "Number" },
                        PartsMatchMode = MatchMode.Ordered,
                        ExpressionOrder = 4
                    },
                    new FragmentMatcherDefinition
                    {
                        Name = "Subtraction",
                        Parts = new [] { "S", "Number" },
                        PartsMatchMode = MatchMode.Ordered,
                        ExpressionOrder = 5
                    }
                }
            };
            string script = @"2+2-2^2*2/2";
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);
            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);
            MatcherResult result = engine.Match(script);

            Assert.True(result.Success);
            Assert.True(CompareXml(@"<Start><Subtraction><Addition><Number>2</Number><A>+</A><Number>2</Number></Addition><S>-</S><Division><Multiplication><Exponent><Number>2</Number><E>^</E><Number>2</Number></Exponent><M>*</M><Number>2</Number></Multiplication><D>/</D><Number>2</Number></Division></Subtraction></Start>", result.MatchData.ToXml()));
        }

        //[Fact]
        public void LazyIndexPerformanceTest()
        {
            string definition = File.ReadAllText("Files/LazyIndexTestDefinition.json");
            string script = File.ReadAllText("Files/Parsables/JibberishScript.lz");
            LanguageMatcherDefinition languageMatcherDefinition = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definition);
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);

            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);


            //GC.TryStartNoGCRegion(200 * 1000 * 1000);
            Stopwatch timer = new Stopwatch();
            timer.Start();

            for (int i = 0; i < 150; i++)
            {
                engine.Match(script);
            }

            timer.Stop();
            //System.GC.EndNoGCRegion();
            Assert.Equal(0, timer.ElapsedTicks);
        }

        //[Fact]
        public void EagerIndexPerformanceTest()
        {
            string definition = File.ReadAllText("Files/EagerIndexTestDefinition2.json");
            string script = File.ReadAllText("Files/Parsables/JibberishScript.lz");
            LanguageMatcherDefinition languageMatcherDefinition = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definition);
            LanguageMatcher languageMatcher = DefinitionConverter.Convert(languageMatcherDefinition);

            ILanguageMatchEngine engine = LanguageMatchEngineFactory.Get(GenerationType, languageMatcher);


            //GC.TryStartNoGCRegion(200 * 1000 * 1000);
            Stopwatch timer = new Stopwatch();
            timer.Start();

            for (int i = 0; i < 150; i++)
            {
                engine.Match(script);
            }

            timer.Stop();
            //System.GC.EndNoGCRegion();
            Assert.Equal(0, timer.ElapsedTicks);
        }

        private bool CompareXml(string expected, string actual)
        {
            return XNode.DeepEquals(XDocument.Parse(expected), XDocument.Parse(actual));
        }
    }
}
