using System;
using System.Collections.Generic;
using System.Linq;

namespace StaxeTests.Shared.Generator
{

	public class ActionInfo
	{

		public Dictionary<string, VariableInfo> VariablesFromParent
		{
			get;
			private set;
		} = new Dictionary<string, VariableInfo>();

		public List<VariableInfo> OrderedVariablesFromParent
		{
			get;
			private set;
		} = new List<VariableInfo>();

		public ActionInfo Parent
		{
			get;
			set;
		}

		public GroupInfo Group
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public bool HasNextInstruction
		{
			get;
			set;
		}

		public Dictionary<string, VariableInfo> Variables
		{
			get;
			private set;
		} = new Dictionary<string, VariableInfo>(StringComparer.Ordinal);

		public int ActionStackLocation
		{
			get;
			set;
		}

		public int BlockDepth
		{
			get;
			set;
		}

		public bool Contains(string name)
		{
			return Variables.ContainsKey(name) || (!(Group?.Pointers.ContainsKey(name) ?? false) && (VariablesFromParent.ContainsKey(name) || (Parent?.ContainsForChild(name) ?? false)));
		}

		public bool ContainsForChild(string name)
		{
			return Variables.ContainsKey(name) || VariablesFromParent.ContainsKey(name) || (Group?.Pointers.ContainsKey(name) ?? false) || (Parent?.ContainsForChild(name) ?? false);
		}

		public VariableInfo GetVariableOrDefault(string name)
		{
			VariableInfo variable = Variables.GetValueOrDefault(name) ?? VariablesFromParent.GetValueOrDefault(name);
			if (variable == null && !(Group?.Pointers.ContainsKey(name) ?? false) &&
				Parent?.GetVariableOrDefaultForChild(name) != null)
			{
				variable = new VariableInfo
				{
					Name = name,
					Depth = 0,
					StackLocation = -(OrderedVariablesFromParent.Count + 1)
				};
				VariablesFromParent.Add(name, variable);
				OrderedVariablesFromParent.Add(variable);
			}
			return variable;
		}

		private VariableInfo GetVariableOrDefaultForChild(string name)
		{
			VariableInfo variable = Variables.GetValueOrDefault(name) ?? VariablesFromParent.GetValueOrDefault(name) ?? GetGroupVariableOrDefault(name);
			if (variable == null && Parent?.GetVariableOrDefaultForChild(name) != null)
			{
				variable = new VariableInfo
				{
					Name = name,
					Depth = 0,
					StackLocation = -(OrderedVariablesFromParent.Count + 1)
				};
				VariablesFromParent.Add(name, variable);
				OrderedVariablesFromParent.Add(variable);
			}
			return variable;
		}

		private VariableInfo GetGroupVariableOrDefault(string name)
		{
			VariableInfo variable = null;
			if (Group?.Pointers.ContainsKey(name) ?? false)
			{
				variable = new VariableInfo
				{
					Name = name,
					Depth = 0,
					StackLocation = -(OrderedVariablesFromParent.Count + 1),
					IsFromGroup = true
				};
				VariablesFromParent.Add(name, variable);
				OrderedVariablesFromParent.Add(variable);
			}
			return variable;
		}
	}
}
