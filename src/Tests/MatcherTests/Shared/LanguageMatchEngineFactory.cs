using Synfron.Staxe.Matcher;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MatcherTests.Shared
{
    public class LanguageMatchEngineFactory
    {
        public static ILanguageMatchEngine Get(GenerationType generationType, LanguageMatcher languageMatcher)
        {
            switch (generationType)
            {
                case GenerationType.PreGen:
                    {
                        MatcherClassGenerator generator = new MatcherClassGenerator(languageMatcher);
                        Assembly assembly = generator.GetAssembly();
                        return (ILanguageMatchEngine)Activator.CreateInstance(assembly.GetType($"Synfron.Staxe.Matcher.{languageMatcher.Name}"));
                    }
                case GenerationType.PostGen:
                    {
                        return LanguageMatchEngine.Build(languageMatcher);
                    }
                default:
                    throw new NotSupportedException($"Unsupported generation type '{generationType.ToString()}'");
            }
            
        }
    }
}
