using Synfron.Staxe.Matcher.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Synfron.Staxe.Matcher.Input.Actions
{
    public abstract class MatcherAction
    {
        public string Name { get; set; }

        public abstract MatcherActionType ActionType { get; }

        public abstract bool Perform(Span<BlobData> blobDatas, IList<IMatchData> matchDatas);

        internal abstract string Generate(MatcherEngineGenerator generator);

        protected static string GetSafeMethodName(string name)
        {
            return Regex.Replace(name, @"\W", (match) => $"x{(int)match.Value[0]}");
        }
    }
}
