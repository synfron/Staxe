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
}
