using System;
using System.Collections.Generic;
using System.Text;

namespace Synfron.Staxe.Matcher.Input.Patterns
{
    public class FragmentPatternMatcher : PatternMatcher
    {
        public FragmentMatcher Fragment { get; set; }

        public FragmentPatternMatcher()
        {
            IsStandard = false;
        }

        public override (bool success, int offset) IsMatch(string text, int startOffset = 0)
        {
            return (false, 0);
        }

        public override string ToString()
        {
            return $"[{Fragment.Name}]";
        }

        internal override string Generate(MatcherEngineGenerator generator)
        {
            string methodName = $"MatchFragmentPattern";
            string method = $"{methodName}({{0}}, {{1}})";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private (bool success, int offset) {methodName}(string text, int startOffset)
        {{
            return (false, 0);
        }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }
    }
}
