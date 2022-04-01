using System;
using System.Collections.Generic;
using System.Text;

namespace Synfron.Staxe.Matcher.Data
{
    internal static class Extensions
    {

        public static int GetEndIndex(this IMatchData matchData)
        {
            if (matchData != null)
            {
                return matchData.StartIndex + matchData.Length;
            }
            return 0;
        }


        public static int GetLength(this IList<IMatchData> matchDatas, bool includeFragments)
        {
            int length = 0;
            foreach (IMatchData matchData in matchDatas)
            {
                if (includeFragments || matchData is StringMatchData)
                {
                    length += matchData.Length;
                }
            }
            return length;
        }

        public static string GetText(this IList<IMatchData> matchDatas, bool includeFragments)
        {
            StringBuilder textBuilder = new StringBuilder();
            foreach (IMatchData matchData in matchDatas)
            {
                if (includeFragments || matchData is StringMatchData)
                {
                    textBuilder.Append(matchData.ToString());
                }
            }
            return textBuilder.ToString();
        }
    }
}
