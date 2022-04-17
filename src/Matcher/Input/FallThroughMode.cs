using System;
using System.Collections.Generic;
using System.Text;

namespace Synfron.Staxe.Matcher.Input
{
    public enum FallThroughMode
    {
        None = int.MinValue,
        Empty = 0,
        One = 1,
        All = int.MaxValue
    }

    public static class FallThroughModeExtensions
    {
        public static bool IsCountBased(this FallThroughMode mode)
        {
            return FallThroughMode.None < mode && mode < FallThroughMode.All;
        }
    }
}
