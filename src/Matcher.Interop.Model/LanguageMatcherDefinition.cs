using Synfron.Staxe.Matcher.Input;
using System.Collections.Generic;

namespace Synfron.Staxe.Matcher.Interop.Model
{
	public class LanguageMatcherDefinition
	{

		public IList<PatternMatcherDefinition> Patterns { get; set; }

		public string StartingFragment { get; set; }

		public IList<FragmentMatcherDefinition> Fragments { get; set; }

		public IList<MatcherActionDefinition> Actions { get; set; }

		public string Name { get; set; }

		public bool LogMatches { get; set; }

		public IndexingMode IndexingMode { get; set; } = IndexingMode.Lazy;
	}
}
