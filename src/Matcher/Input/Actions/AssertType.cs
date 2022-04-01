using System;
using System.Collections.Generic;
using System.Text;

namespace Synfron.Staxe.Matcher.Input.Actions
{
    public enum AssertType
    {
        Equals,
        NotEquals,
        GreaterThan,
        GreaterThanOrEquals,
        LessThan,
        LessThanOrEquals,
        Contains,
        StartsWith,
        EndsWith,
        MatchesPattern
    }
}
