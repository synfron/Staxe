using Synfron.Staxe.Matcher.Input.Actions;

namespace Synfron.Staxe.Matcher.Interop.Model
{
	public class MatcherActionDefinition
	{
		public string Name { get; set; }

		public MatcherActionType ActionType { get; set; }

		public string FirstVariableName { get; set; }

		public string SecondVariableName { get; set; }

		public VariableValueSource? Source { get; set; }

		public VariableUpdateAction? Change { get; set; }

		public AssertType Assert { get; set; }

		public object Value { get; set; }
	}
}
