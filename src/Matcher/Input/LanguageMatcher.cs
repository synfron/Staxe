using Synfron.Staxe.Matcher.Input.Patterns;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Synfron.Staxe.Matcher.Input
{
	public class LanguageMatcher
	{
		public string Name { get; set; }

		public IList<PatternMatcher> Patterns { get; set; } = new PatternMatcher[0];

		public FragmentMatcher StartingFragment { get; set; }

		public IList<FragmentMatcher> Fragments { get; set; } = new FragmentMatcher[0];

		public bool LogMatches { get; set; }

		public IndexingMode IndexingMode { get; set; } = IndexingMode.Lazy;

		public IList<string> Blobs { get; set; } = new string[0];
	}
}
