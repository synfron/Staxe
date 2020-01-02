using StaxeTests.Shared.Generator;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Instructions.Flags;
using Synfron.Staxe.Generator;
using Synfron.Staxe.Matcher.Data;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StaxeTests.TestComplexLang.Engine.Generator
{
	public class InstructionGenerator : IInstructionGenerator<GroupState>
	{
		private GroupInfo _class;
		private NamespaceInfo _namespace;
		private ActionInfo _action;
		private readonly Dictionary<InstructionCode, List<int>> _reprocessRequiredIndexes = new Dictionary<InstructionCode, List<int>>();
		private bool _isAssignmentTarget;
		private int _forEachCount = 0;

		private List<Instruction<GroupState>> Instructions
		{
			get
			{
				return _class.Instructions;
			}
		}


		public IList<Instruction<GroupState>> Generate(IMatchData matchData)
		{
			_class = new GroupInfo()
			{
				Namespace = _namespace
			};
			_action = new ActionInfo()
			{
				Group = _class
			};
			foreach (IMatchData partMatchData in ((FragmentMatchData)matchData).Parts)
			{
				AddItem(partMatchData);
			}
			if (_reprocessRequiredIndexes.Count > 0)
			{
				throw new LanguageConstraintException("Invalid operation", _reprocessRequiredIndexes.Values.FirstOrDefault()?.FirstOrDefault());
			}
			return _class.Instructions;
		}

		private void AddItem(IMatchData matchData, bool interruptable = false)
		{
			switch (matchData.Name)
			{
				case "Namespace":
					AddNamespace((FragmentMatchData)matchData);
					break;
				case "StaticClass":
					AddStaticClass((FragmentMatchData)matchData);
					break;
				case "Class":
					AddClass((FragmentMatchData)matchData);
					break;
				case "UsingStatement":
					AddUsingStatement((FragmentMatchData)matchData);
					break;
				case "WhileBlock":
					AddWhileBlock((FragmentMatchData)matchData);
					break;
				case "ForBlock":
					AddForBlock((FragmentMatchData)matchData);
					break;
				case "ForEachBlock":
					AddForEachBlock((FragmentMatchData)matchData);
					break;
				case "IfElseBlock":
					AddIfElseBlock((FragmentMatchData)matchData);
					break;
				case "Block":
					AddBlock((FragmentMatchData)matchData);
					break;
				case "SetterBlock":
					AddSetterBlock((FragmentMatchData)matchData);
					break;
				case "ArrayBlock":
					AddArrayBlock((FragmentMatchData)matchData);
					break;
				case "ItemReturn":
					AddItemReturn((FragmentMatchData)matchData);
					break;
				case "Break":
					AddBreak((StringMatchData)matchData);
					break;
				case "Continue":
					AddContinue((StringMatchData)matchData);
					break;
				case "DeclarationAssignment":
					AddDeclarationAssignment((FragmentMatchData)matchData);
					break;
				case "Declaration":
					AddDeclaration((FragmentMatchData)matchData);
					break;
				case "Assignment":
					AddAssignment((FragmentMatchData)matchData);
					break;
				case "DirectedValuableChain":
					AddDirectedValuableChain((FragmentMatchData)matchData);
					break;
				case "NewArray":
					AddNewArray((FragmentMatchData)matchData);
					break;
				case "NewFilledArray":
					AddNewFilledArray((FragmentMatchData)matchData);
					break;
				case "NewInstance":
					AddNewInstance((FragmentMatchData)matchData);
					break;
				case "ParensValuable":
					AddParensValuable((FragmentMatchData)matchData);
					break;
				case "Identifier":
					AddGetVariable((FragmentMatchData)matchData, interruptable);
					break;
				case "NativeIdentifier":
					AddNativeIdentifier((FragmentMatchData)matchData);
					break;
				case "ValuedIndex":
					AddValuedIndex((FragmentMatchData)matchData);
					break;
				case "DotIdentifier":
					AddDotIdentifier((FragmentMatchData)matchData);
					break;
				case "ArgumentValues":
					AddArgumentValues((FragmentMatchData)matchData);
					break;
				case "AnonymousFunction":
					AddAnonymousFunction((FragmentMatchData)matchData);
					break;
				case "Function":
					AddFunction((FragmentMatchData)matchData);
					break;
				case "Constructor":
					AddConstructor((FragmentMatchData)matchData);
					break;
				case "Boolean":
					AddBoolean((StringMatchData)matchData);
					break;
				case "Null":
					AddNull((StringMatchData)matchData);
					break;
				case "StringLiteral":
					AddStringLiteral((StringMatchData)matchData);
					break;
				case "Number":
					AddNumber((FragmentMatchData)matchData);
					break;
				case "Valuable":
				case "Value":
				case "OpenEndedStatements":
					AddInnerItem((FragmentMatchData)matchData);
					break;
				case "Self":
					AddSelf((FragmentMatchData)matchData);
					break;
				case "MultiplicativeSuffix":
				case "AdditiveSuffix":
				case "SubtractiveSuffix":
				case "EqualitySuffix":
				case "RelationalSuffix":
				case "AndSuffix":
				case "OrSuffix":
				case "BitwiseAndSuffix":
				case "BitwiseOrSuffix":
					AddExpressionSuffix((FragmentMatchData)matchData);
					break;
				default:
					throw new NotImplementedException($"Matcher '{matchData.Name}' is unhandled");
			}
		}

		private void AddNativeIdentifier(FragmentMatchData matchData)
		{
			string[] identifiers = matchData.Parts.Cast<StringMatchData>().Select(part => part.ToString()).ToArray();
			string location = string.Join(".", identifiers.Take(identifiers.Length - 1));
			string name = identifiers.Last();
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.DPR, new object[] { location, name }, sourcePosition: matchData.StartIndex));
		}

		private void AddNamespace(FragmentMatchData matchData)
		{
			NamespaceInfo outerNamespace = _namespace;
			string namespaceIdentifier = GetNamespaceIdentifier((FragmentMatchData)matchData.Parts[0]);
			FragmentMatchData namespaceBody = (FragmentMatchData)matchData.Parts[1];
			_namespace = new NamespaceInfo
			{
				Name = namespaceIdentifier
			};
			int origActionStackLocation = _action.ActionStackLocation;
			_action.BlockDepth++;

			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.B, sourcePosition: matchData.StartIndex));

			foreach (FragmentMatchData fragment in namespaceBody.Parts)
			{
				switch (fragment.Name)
				{
					case "Class":
						PreAddClass(fragment);
						break;
					case "StaticClass":
						PreAddStaticClass(fragment);
						break;
				}
			}

			AddNamespaceBody(namespaceBody);

			foreach (GroupInfo classInfo in _namespace.Groups.Values)
			{
				if (classInfo.Dependencies.Count > 0)
				{
					AddGetStackVariable($"$c_{classInfo.ShortName}", matchData.StartIndex + matchData.Length, false);
					foreach (string className in classInfo.Dependencies.OrderBy(entry => entry.Value).Select(entry => entry.Key))
					{
						Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
						AddGetStackVariable($"$c_{className}", matchData.StartIndex + matchData.Length, false);
						Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RGD, sourcePosition: matchData.StartIndex));
					}
				}
			}

			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.BE, sourcePosition: matchData.StartIndex + matchData.Length));
			_action.ActionStackLocation = origActionStackLocation;
			_action.BlockDepth--;
			foreach (KeyValuePair<string, VariableInfo> variablePair in _action.Variables.ToList())
			{
				if (variablePair.Value.Depth > _action.BlockDepth)
				{
					_action.Variables.Remove(variablePair.Key);
				}
			}

			_namespace = outerNamespace;
		}

		private void AddNamespaceBody(FragmentMatchData namespaceBody)
		{

			foreach (FragmentMatchData fragment in namespaceBody.Parts)
			{
				switch (fragment.Name)
				{
					case "Class":
						AddClass(fragment, isNamespaceChild: true);
						break;
					case "StaticClass":
						AddStaticClass(fragment, true);
						break;
				}
			}
		}

		private void PreAddStaticClass(FragmentMatchData matchData)
		{
			FragmentMatchData classPart = (FragmentMatchData)matchData.Parts[0];
			string className = GetIdentifierText((FragmentMatchData)classPart.Parts[0]);
			GroupInfo classInfo = new GroupInfo
			{
				Namespace = _namespace,
				ShortName = className,
				Name = $"{(_namespace != null ? $"{_namespace.Name}." : null)}{className}",
				IsStatic = true
			};
			_namespace.Groups.Add(className, classInfo);

			AddGetOrNewStackVariable($"$c_{className}", matchData.StartIndex, false);
		}

		private void PreAddClass(FragmentMatchData matchData)
		{
			string className = GetIdentifierText((FragmentMatchData)matchData.Parts[0]);
			GroupInfo classInfo = new GroupInfo
			{
				Namespace = _namespace,
				ShortName = className,
				Name = $"{(_namespace != null ? $"{_namespace.Name}." : null)}{className}"
			};
			_namespace.Groups.Add(className, classInfo);

			AddGetOrNewStackVariable($"$c_{className}", matchData.StartIndex, false);
		}

		private void AddClass(FragmentMatchData matchData, bool isStatic = false, bool isNamespaceChild = false)
		{
			string className = GetIdentifierText((FragmentMatchData)matchData.Parts[0]);
			FragmentMatchData optionalBaseClassDeclaration = (FragmentMatchData)matchData.Parts[1];
			FragmentMatchData classBody = (FragmentMatchData)matchData.Parts[2];

			AddGetOrNewStackVariable($"$c_{className}", matchData.StartIndex, false);

			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));

			GroupInfo outerClass = _class;
			GroupInfo innerClass = isNamespaceChild ? _namespace.Groups[className] : new GroupInfo
			{
				Namespace = _namespace,
				ShortName = className,
				Name = className,
				IsStatic = isStatic,
				IsSubClass = optionalBaseClassDeclaration.Parts.Count > 0
			};
			ActionInfo innerAction = new ActionInfo()
			{
				Group = _class,
				Parent = _action
			};

			_class = innerClass;
			_action = innerAction;
			AddClassBody(classBody);

			_class = outerClass;
			_action = _action?.Parent;

			outerClass.Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.G, new object[] { innerClass.Name }, sourcePosition: matchData.StartIndex));
			outerClass.Instructions.AddRange(innerClass.Instructions);
			outerClass.Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.GE, sourcePosition: matchData.StartIndex + matchData.Length));

			if (isNamespaceChild)
			{
				outerClass.Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RGH, new object[] { innerClass.Name }, sourcePosition: matchData.StartIndex + matchData.Length));
			}
			outerClass.Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex + matchData.Length));

			AddOptionalBaseClassDeclaration(optionalBaseClassDeclaration);

			outerClass.Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PLR, sourcePosition: matchData.StartIndex));
		}

		private void AddOptionalBaseClassDeclaration(FragmentMatchData matchData)
		{
			if (matchData.Parts.ElementAtOrDefault(0) is FragmentMatchData identifier)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				AddGetClassVariable(identifier, false);
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.MG, new object[] { (int)(GroupMerge.MapPointers | GroupMerge.OverrideDependencyPointers | GroupMerge.AsComponentDependency | GroupMerge.Dependencies | GroupMerge.CloneNewDependencies) }, sourcePosition: matchData.StartIndex));
			}
		}

		private void AddStaticClass(FragmentMatchData matchData, bool isNamespaceChild = false)
		{
			FragmentMatchData classData = (FragmentMatchData)matchData.Parts[0];
			AddClass(classData, true, isNamespaceChild);
		}

		private void AddClassBody(FragmentMatchData matchData)
		{
			if (_action.Parent?.Variables.Count > 0)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.MF, new object[] { 1, (int)(FrameMerge.Register | FrameMerge.Stack) }, sourcePosition: matchData.StartIndex));
				foreach (KeyValuePair<string, VariableInfo> entry in _action.Parent.Variables)
				{
					_action.Variables.Add(entry.Key, entry.Value);
				}
				_action.ActionStackLocation += _action.Parent.Variables.Count;
			}
			if (_class.IsStatic)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.GR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { (int)Modifiers.Static }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RGM, sourcePosition: matchData.StartIndex));
			}
			else
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.GR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { (int)Modifiers.Component }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RGM, sourcePosition: matchData.StartIndex));
			}
			foreach (FragmentMatchData fragmentPart in matchData.Parts.Cast<FragmentMatchData>())
			{
				switch (fragmentPart.Name)
				{
					case "PropertyDeclaration":
						AddPropertyDeclaration(fragmentPart);
						break;
					case "PropertyDeclarationAssignment":
						AddPropertyDeclarationAssignment(fragmentPart);
						break;
					case "Constructor":
						PreAddConstructor(fragmentPart);
						break;
					case "Function":
						PreAddFunction(fragmentPart);
						break;
				}
			}
			foreach (FragmentMatchData fragmentPart in matchData.Parts.Cast<FragmentMatchData>())
			{
				switch (fragmentPart.Name)
				{
					case "Constructor":
						AddConstructor(fragmentPart);
						break;
					case "Function":
						AddFunction(fragmentPart);
						break;
				}
			}
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CR, sourcePosition: matchData.StartIndex));
		}

		private void AddUsingStatement(FragmentMatchData matchData)
		{
			string namespaceIdentifier = GetNamespaceIdentifier((FragmentMatchData)matchData.Parts[0]);
			string className = namespaceIdentifier.Split('.').Last();
			AddGetOrNewStackVariable($"$c_{className}", matchData.StartIndex, true);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.HGR, new object[] { namespaceIdentifier }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));
		}


		#region Add functions

		private void PreAddConstructor(FragmentMatchData matchData)
		{
			AddNewGroupVariable("", matchData.StartIndex, false);
		}

		private void AddConstructor(FragmentMatchData matchData)
		{
			if (_class.IsStatic)
			{
				throw new LanguageConstraintException("A static class cannot have a constructor", matchData.StartIndex);
			}
			FragmentMatchData functionParameters = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData body = (FragmentMatchData)matchData.Parts[1];

			AddFunction("", matchData, functionParameters, body);
		}

		private void PreAddFunction(FragmentMatchData matchData)
		{
			FragmentMatchData identifier = (FragmentMatchData)matchData.Parts[0];

			string functionName = GetIdentifierText(identifier);
			AddNewGroupVariable(functionName, matchData.StartIndex, false);
		}

		private void AddFunction(FragmentMatchData matchData)
		{
			FragmentMatchData identifier = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData functionParameters = (FragmentMatchData)matchData.Parts[1];
			FragmentMatchData body = (FragmentMatchData)matchData.Parts[2];
			string functionName = GetIdentifierText(identifier);

			AddFunction(functionName, matchData, functionParameters, body);
		}

		private void AddFunction(string functionName, FragmentMatchData matchData, FragmentMatchData functionParameters, FragmentMatchData body)
		{
			int actionStartIndex = Instructions.Count + 3;
			_class.Actions.Add(functionName, actionStartIndex);
			AddGetGroupVariable(functionName, matchData.StartIndex, false);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));

			AddFunctionBody(matchData, functionParameters, body, functionName);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));

			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { (int)(Modifiers.WriteRestricted | Modifiers.Component) }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPM, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.MI, new object[] { functionName, actionStartIndex }, sourcePosition: matchData.StartIndex));

			if (_reprocessRequiredIndexes.Count > 0)
			{
				throw new LanguageConstraintException("Invalid operation", _reprocessRequiredIndexes.Values.FirstOrDefault()?.FirstOrDefault());
			}
		}

		private void AddAnonymousFunction(FragmentMatchData matchData)
		{
			FragmentMatchData functionParameters = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData body = (FragmentMatchData)matchData.Parts[1];

			AddFunctionBody(matchData, functionParameters, body, null);
		}

		private void AddFunctionBody(FragmentMatchData matchData, FragmentMatchData functionParameters, FragmentMatchData body, string functionName)
		{
			ActionInfo innerAction = new ActionInfo
			{
				Group = _class,
				Name = functionName,
				Parent = _action
			};
			_action = innerAction;

			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, payload: new object[] { "endaction" }, sourcePosition: matchData.StartIndex));

			int actionStartIndex = Instructions.Count;

			AddAction(matchData, functionParameters, body);

			_action = innerAction.Parent;

			Instructions[actionStartIndex - 1] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, payload: new object[] { Instructions.Count }, sourcePosition: matchData.StartIndex);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.AR, payload: new object[] { actionStartIndex }, sourcePosition: matchData.StartIndex));

			if (innerAction.OrderedVariablesFromParent.Count > 0)
			{
				foreach (string name in innerAction.OrderedVariablesFromParent.Select(variable => variable.Name).Reverse())
				{
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
					AddGetVariable(name, matchData.StartIndex, false);
				}

				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLR, new object[] { innerAction.OrderedVariablesFromParent.Count }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LRAS, new object[] { innerAction.OrderedVariablesFromParent.Count }, sourcePosition: matchData.StartIndex));
			}
		}

		private void AddAction(FragmentMatchData matchData, FragmentMatchData functionParameters, FragmentMatchData body)
		{

			// Start the declaration of the function
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.A, new object[] { false }, sourcePosition: functionParameters.StartIndex));
			AddFunctionParameters(functionParameters);

			// Add instructions for the body of the function
			AddItem(body);

			// End the function declaration
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.AE, sourcePosition: matchData.StartIndex + matchData.Length));
		}

		private void AddParameterVariable(string name)
		{
			_action.Variables.Add(name, new VariableInfo
			{
				Name = name,
				StackLocation = _action.ActionStackLocation++,
				Depth = _action.BlockDepth
			});
		}

		private void AddFunctionParameters(FragmentMatchData matchData)
		{
			string[] names = matchData.Parts.Select(part => GetIdentifierText((FragmentMatchData)part)).ToArray();
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LRR, new object[] { names.Length, false }, sourcePosition: matchData.StartIndex));
			List<object> payload = new List<object> { names.Length };
			foreach (string name in names)
			{
				payload.Add(InstructionCode.CSP);
				payload.Add(name);
				AddParameterVariable(name);
			}
			if (matchData.Parts.Count > 0)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RCP, payload.ToArray(), sourcePosition: matchData.StartIndex));
			}
		}
		#endregion

		private void AddSelf(FragmentMatchData matchData)
		{
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.GR, sourcePosition: matchData.StartIndex));
		}

		private void AddNewFilledArray(FragmentMatchData matchData)
		{
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CVR, new object[] { false, null }, sourcePosition: matchData.StartIndex));
			FragmentMatchData arraySetter = (FragmentMatchData)matchData.Parts[0];
			AddItem(arraySetter);
		}

		private void AddInnerItem(FragmentMatchData matchData)
		{
			AddItem(matchData.Parts[0]);
		}

		private void AddArgumentValues(FragmentMatchData matchData)
		{
			if (matchData.Parts.Count > 0)
			{
				bool first = true;
				foreach (FragmentMatchData partMatchData in matchData.Parts)
				{
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: partMatchData.StartIndex));
					AddItem(partMatchData, first);
					first = false;
				}
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLR, new object[] { matchData.Parts.Count }, sourcePosition: matchData.StartIndex));
			}
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RCE, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LRR, new object[] { 1, false }, sourcePosition: matchData.StartIndex));
		}

		private void AddValuedIndex(FragmentMatchData matchData)
		{
			FragmentMatchData evaluable = (FragmentMatchData)matchData.Parts[0];
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			AddItem(evaluable);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RVK, new object[] { _isAssignmentTarget }, sourcePosition: matchData.StartIndex));
		}

		private void AddParensValuable(FragmentMatchData matchData)
		{
			FragmentMatchData evaluable = (FragmentMatchData)matchData.Parts[0];
			AddItem(evaluable);
		}

		private void AddNewArray(FragmentMatchData matchData)
		{
			FragmentMatchData arrayInitializer = (FragmentMatchData)matchData.Parts[0];
			if (arrayInitializer.Parts.ElementAtOrDefault(0) is FragmentMatchData evaluable)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				AddItem(evaluable);
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CVR, new object[] { true, null }, sourcePosition: matchData.StartIndex));
			}
			else
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CVR, new object[] { false, null }, sourcePosition: matchData.StartIndex));
			}
		}

		private void AddDirectedValuableChain(FragmentMatchData matchData)
		{
			FragmentMatchData valuablePrefix = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData valuableChain = (FragmentMatchData)matchData.Parts[1];
			FragmentMatchData valuableSuffix = (FragmentMatchData)matchData.Parts[2];
			AddValuableChains(valuableChain);
			AddValuableSuffix(valuableSuffix);
			AddValuablePrefix(valuablePrefix);
		}

		private void AddValuableChains(FragmentMatchData matchData)
		{
			int chainSkips = 0;
			if (matchData.Parts[0] is FragmentMatchData firstPart
				&& firstPart.Name == "Identifier"
				&& GetIdentifierText(firstPart) is string className
				&& IsClassVariable(className))
			{
				if (matchData.Parts.Any(part => part.Name == "DotIdentifier" || part.Name == "ValuedIndex"))
				{
					AddGetClassVariable(firstPart);
					chainSkips++;
				}
				else
				{
					throw new LanguageConstraintException($"Invalid access of class '{className}'", matchData.StartIndex);
				}
			}
			foreach (FragmentMatchData partMatchData in matchData.Parts.Skip(chainSkips))
			{
				AddItem(partMatchData);
			}
		}

		private bool IsClassVariable(string name)
		{
			return (_namespace?.Groups.ContainsKey(name) ?? false) || (_action.Contains($"$c_{name}"));
		}

		private void AddValuablePrefix(FragmentMatchData matchData)
		{
			if (matchData.Parts.ElementAtOrDefault(0) is IMatchData partMatchData)
			{
				switch (partMatchData.Name)
				{
					case "Not":
						AddNot((FragmentMatchData)partMatchData);
						break;
					case "Increment":
						Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
						AddIncrement((StringMatchData)partMatchData);
						break;
					case "Decrement":
						Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
						AddDecrement((FragmentMatchData)partMatchData);
						break;
				}
			}
		}

		private void AddValuableSuffix(FragmentMatchData matchData)
		{
			if (matchData.Parts.ElementAtOrDefault(0) is IMatchData partMatchData)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { null }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RM, new object[] { 1 }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RM, new object[] { 1 }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
				switch (partMatchData.Name)
				{
					case "Increment":
						AddIncrement((StringMatchData)partMatchData);
						break;
					case "Decrement":
						AddDecrement((FragmentMatchData)partMatchData);
						break;
				}
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PLR, sourcePosition: matchData.StartIndex));
			}
		}

		private void AddDecrement(FragmentMatchData matchData)
		{
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { 1 }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RSubtract, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));
		}

		private void AddIncrement(StringMatchData matchData)
		{
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { 1 }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPlus, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));
		}

		private void AddNot(FragmentMatchData matchData)
		{
			foreach (StringMatchData not in matchData.Parts)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RNot, sourcePosition: not.StartIndex));
			}
		}

		private void AddItemReturn(FragmentMatchData matchData)
		{
			if (matchData.Parts.ElementAtOrDefault(0) is FragmentMatchData item)
			{
				AddItem(item, true);
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLR, new object[] { 1 }, sourcePosition: matchData.StartIndex));
			}
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.AE, sourcePosition: matchData.StartIndex, interruptable: matchData.Parts.Count > 0));
		}

		private void AddBlock(FragmentMatchData matchData)
		{
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.B, sourcePosition: matchData.StartIndex, interruptable: true));
			int origActionStackLocation = _action.ActionStackLocation;
			_action.BlockDepth++;
			foreach (IMatchData partMatchData in matchData.Parts)
			{
				AddItem(partMatchData, true);
			}
			_action.Variables.Where(pair => pair.Value.Depth >= _action.BlockDepth).Select(pair => pair.Key).ToList().ForEach(key => _action.Variables.Remove(key));
			_action.ActionStackLocation = origActionStackLocation;
			_action.BlockDepth--;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.BE, sourcePosition: matchData.StartIndex + matchData.Length, interruptable: true));
		}

		private void AddSetterBlock(FragmentMatchData matchData)
		{
			foreach (FragmentMatchData partMatchData in matchData.Parts)
			{
				AddSetterAssignment(partMatchData);
			}
		}

		private void AddArrayBlock(FragmentMatchData matchData)
		{
			int index = 0;
			foreach (FragmentMatchData partMatchData in matchData.Parts)
			{
				FragmentMatchData evaluable = (FragmentMatchData)partMatchData.Parts[0];
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { index }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RVK, new object[] { true }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				AddItem(evaluable);
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PLR, sourcePosition: matchData.StartIndex));
				index++;
			}
		}

		private void AddBoolean(StringMatchData matchData)
		{
			bool boolValue = bool.Parse(matchData.ToString());
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { boolValue }, sourcePosition: matchData.StartIndex));
		}

		private void AddNull(StringMatchData matchData)
		{
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { null }, sourcePosition: matchData.StartIndex));
		}

		private void AddStringLiteral(StringMatchData matchData)
		{
			string literal = matchData.ToString();
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { Regex.Unescape(literal.Substring(1, literal.Length - 2)) }, sourcePosition: matchData.StartIndex));
		}

		private void AddNumber(FragmentMatchData matchData)
		{
			string numberStr = string.Join("", matchData.Parts.Select(part => part.ToString()));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { double.Parse(numberStr) }, sourcePosition: matchData.StartIndex));
		}

		private void AddDotIdentifier(FragmentMatchData matchData)
		{
			FragmentMatchData identifier = (FragmentMatchData)matchData.Parts[0];
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { GetIdentifierText(identifier) }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RVK, new object[] { false }, sourcePosition: matchData.StartIndex));
		}

		private void AddNewInstance(FragmentMatchData matchData)
		{
			FragmentMatchData identifier = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData argumentValues = (FragmentMatchData)matchData.Parts[1];

			AddGetClassVariable(identifier, true);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CG, new object[] { Copy.Modifiers }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { (int)Modifiers.Static }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RGM, sourcePosition: matchData.StartIndex));

			if (argumentValues.Parts.Count > 0)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { "" }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RVK, new object[] { false }, sourcePosition: matchData.StartIndex));
				AddArgumentValues(argumentValues);
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PLR, sourcePosition: matchData.StartIndex));
			}
			else
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { "" }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RVK, new object[] { false }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.ID, sourcePosition: matchData.StartIndex));
				int cbcIndex = Instructions.Count;
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, new object[] { "nextcondition" }, sourcePosition: matchData.StartIndex));
				AddArgumentValues(argumentValues);
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PLR, sourcePosition: matchData.StartIndex));
				Instructions[cbcIndex].Payload[0] = Instructions.Count + 1;
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CSE, new object[] { Instructions.Count + 2 }, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PLR, sourcePosition: matchData.StartIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CSE, new object[] { Instructions.Count + 1 }, sourcePosition: matchData.StartIndex));
			}
		}

		private void AddReprocessInstruction(InstructionCode code, int location)
		{
			if (!_reprocessRequiredIndexes.TryGetValue(code, out List<int> locations))
			{
				locations = new List<int>();
				_reprocessRequiredIndexes.Add(code, locations);
			}
			locations.Add(location);
		}

		private void AddPropertyDeclaration(FragmentMatchData matchData)
		{
			string variableName = GetIdentifierText((FragmentMatchData)matchData.Parts[0]);
			AddNewGroupVariable(variableName, matchData.StartIndex, false);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { (int)Modifiers.None }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPM, sourcePosition: matchData.StartIndex));
		}

		private void AddPropertyDeclarationAssignment(FragmentMatchData matchData)
		{
			if (!_class.IsStatic)
			{
				throw new LanguageConstraintException("Cannot set default property values on non-static classes", matchData.StartIndex);
			}
			FragmentMatchData target = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData value = (FragmentMatchData)matchData.Parts[1];
			string variableName = GetIdentifierText(target);
			AddNewGroupVariable(variableName, matchData.StartIndex, false);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { (int)Modifiers.None }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPM, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: target.StartIndex));
			AddItem(value);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: target.StartIndex));
		}

		private void AddSetterAssignment(FragmentMatchData matchData)
		{
			IMatchData setterTarget = matchData.Parts[0];
			FragmentMatchData evaluable = (FragmentMatchData)matchData.Parts[1];
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			if (setterTarget.Name == "Identifier")
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { GetIdentifierText((FragmentMatchData)setterTarget) }, sourcePosition: matchData.StartIndex));
			}
			else
			{
				AddItem(setterTarget);
			}
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RVK, new object[] { true }, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: evaluable.StartIndex));
			AddItem(evaluable);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PLR, sourcePosition: matchData.StartIndex));
		}

		private void AddAssignment(FragmentMatchData matchData)
		{
			_isAssignmentTarget = true;
			FragmentMatchData target = (FragmentMatchData)matchData.Parts[0];
			StringMatchData assignmentEqual = (StringMatchData)matchData.Parts[1];
			FragmentMatchData evaluable = (FragmentMatchData)matchData.Parts[2];
			AddItem(target, true);
			_isAssignmentTarget = false;

			if (Instructions.Last().Code == InstructionCode.LRR)
			{
				throw new LanguageConstraintException($"Cannot assign to the return of a function.", target.StartIndex + target.Length);
			}

			if (assignmentEqual.Name != "Equal")
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: target.StartIndex));
			}

			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: target.StartIndex));
			AddItem(evaluable);

			switch (assignmentEqual.Name)
			{
				case "MinusEqual":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RSubtract, sourcePosition: target.StartIndex));
					break;
				case "PlusEqual":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPlus, sourcePosition: target.StartIndex));
					break;
			}

			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: target.StartIndex));
		}

		private void AddDeclaration(FragmentMatchData matchData)
		{
			FragmentMatchData target = (FragmentMatchData)matchData.Parts[0];
			AddNewStackVariable(target);
		}

		private void AddDeclarationAssignment(FragmentMatchData matchData)
		{
			FragmentMatchData target = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData evaluable = (FragmentMatchData)matchData.Parts[1];
			AddNewStackVariable(target);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: target.StartIndex));
			AddItem(evaluable);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: target.StartIndex));
		}

		#region Conditions

		private int AddIfStatement(FragmentMatchData matchData)
		{
			FragmentMatchData condition = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData body = (FragmentMatchData)matchData.Parts[1];
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CS, sourcePosition: matchData.StartIndex, interruptable: true));
			AddItem(condition);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: condition.StartIndex));
			int cbcIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, sourcePosition: condition.StartIndex));
			AddItem(body);
			int endBlockNeededIndex = Instructions.Count;
			Instructions[cbcIndex] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, new object[] { endBlockNeededIndex + 1 }, sourcePosition: matchData.Parts[1].StartIndex);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CSE, new object[] { "endif" }, sourcePosition: matchData.StartIndex + matchData.Length));
			return endBlockNeededIndex;
		}

		private int AddElseIfStatement(FragmentMatchData matchData)
		{
			return AddIfStatement((FragmentMatchData)matchData.Parts[0]);
		}

		private void AddElseStatement(FragmentMatchData matchData)
		{
			AddItem((FragmentMatchData)matchData.Parts[0]);
		}

		private void AddIfElseBlock(FragmentMatchData matchData)
		{
			List<int> cbes = new List<int>();
			foreach (FragmentMatchData partMatchData in matchData.Parts)
			{
				switch (partMatchData.Name)
				{
					case "IfStatement":
						cbes.Add(AddIfStatement(partMatchData));
						break;
					case "ElseIfStatement":
						cbes.Add(AddElseIfStatement(partMatchData));
						break;
					case "ElseStatement":
						AddElseStatement(partMatchData);
						break;
				}
			}

			int endBlockIndex = Instructions.Count;

			foreach (int cbe in cbes)
			{
				Instructions[cbe] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.CSE, new IComparable[] { endBlockIndex }, sourcePosition: matchData.StartIndex + matchData.Length);
			}
		}

		#endregion

		#region Add loops

		private void AddForBlock(FragmentMatchData matchData)
		{
			FragmentMatchData forParams = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData body = (FragmentMatchData)matchData.Parts[1];
			FragmentMatchData declaration = (FragmentMatchData)forParams.Parts[0];
			FragmentMatchData condition = (FragmentMatchData)forParams.Parts[1];
			FragmentMatchData incrementer = (FragmentMatchData)forParams.Parts[2];
			int origActionStackLocation = _action.ActionStackLocation;
			_action.BlockDepth++;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.B, sourcePosition: matchData.StartIndex, interruptable: true));
			AddItem(declaration);
			int reloopIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.L, sourcePosition: matchData.StartIndex));
			AddItem(condition);
			int lbcIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, sourcePosition: condition.StartIndex));
			AddItem(body);
			AddItem(incrementer);
			int lbeIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LE, new object[] { reloopIndex }, sourcePosition: matchData.StartIndex + matchData.Length));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.BE, sourcePosition: matchData.StartIndex + matchData.Length));
			_action.BlockDepth--;

			ProcessFlowControls(reloopIndex, lbcIndex, lbeIndex);

			_action.ActionStackLocation = origActionStackLocation;

		}

		private void AddForEachBlock(FragmentMatchData matchData)
		{
			FragmentMatchData forEachDeclaration = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData targetDeclaration = (FragmentMatchData)forEachDeclaration.Parts[0];
			FragmentMatchData source = (FragmentMatchData)forEachDeclaration.Parts[1];
			FragmentMatchData body = (FragmentMatchData)matchData.Parts[1];
			string targetName = GetIdentifierText((FragmentMatchData)targetDeclaration.Parts[0]);
			int origActionStackLocation = _action.ActionStackLocation;
			_action.BlockDepth++;
			int id = ++_forEachCount;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.B, sourcePosition: matchData.StartIndex, interruptable: true));
			AddNewStackVariable(targetName, targetDeclaration.StartIndex, false);
			AddNewStackVariable($"$currentIndex{id}", matchData.StartIndex, false);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: targetDeclaration.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { 0 }, sourcePosition: targetDeclaration.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: targetDeclaration.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: targetDeclaration.StartIndex));
			AddNewStackVariable($"$source{id}", source.StartIndex, false);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: targetDeclaration.StartIndex));
			AddItem(source);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: targetDeclaration.StartIndex));
			int loopStartIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.L, sourcePosition: targetDeclaration.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RVS, sourcePosition: source.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLessThan, sourcePosition: source.StartIndex));
			int lbcIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, sourcePosition: source.StartIndex));
			AddGetStackVariable(targetName, targetDeclaration.StartIndex, false);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: source.StartIndex));
			AddGetStackVariable($"$source{id}", source.StartIndex, false);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: source.StartIndex));
			AddGetStackVariable($"$currentIndex{id}", targetDeclaration.StartIndex, false);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RVI, sourcePosition: source.StartIndex, interruptable: true));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: targetDeclaration.StartIndex));
			AddItem(body);
			int reloopIndex = Instructions.Count;
			AddGetStackVariable($"$currentIndex{id}", source.StartIndex, false);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: source.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: source.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { 1 }, sourcePosition: source.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPlus, sourcePosition: source.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: source.StartIndex));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: source.StartIndex));
			AddGetStackVariable($"$source{id}", source.StartIndex, false);
			int lbeIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LE, new object[] { loopStartIndex }, sourcePosition: matchData.StartIndex + matchData.Length));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.BE, sourcePosition: matchData.StartIndex + matchData.Length));

			ProcessFlowControls(reloopIndex, lbcIndex, lbeIndex);

			_action.ActionStackLocation = origActionStackLocation;
			_action.BlockDepth--;
		}

		private void AddWhileBlock(FragmentMatchData matchData)
		{
			FragmentMatchData condition = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData block = (FragmentMatchData)matchData.Parts[1];
			int origActionStackLocation = _action.ActionStackLocation;
			_action.BlockDepth++;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.B, sourcePosition: matchData.StartIndex));
			int lbIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.L, sourcePosition: matchData.StartIndex, interruptable: true));
			AddItem(condition);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.Parts[1].StartIndex));
			int lbcIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, sourcePosition: matchData.Parts[1].StartIndex));
			AddItem(block);
			int lbeIndex = Instructions.Count;
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LE, new object[] { lbIndex }, sourcePosition: matchData.StartIndex + matchData.Length));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.BE, sourcePosition: matchData.StartIndex + matchData.Length));

			ProcessFlowControls(lbIndex, lbcIndex, lbeIndex);

			_action.ActionStackLocation = origActionStackLocation;
			_action.BlockDepth--;
		}

		private void AddContinue(StringMatchData matchData)
		{
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.US, new object[] { "continue" }, sourcePosition: matchData.StartIndex, interruptable: true));
			AddReprocessInstruction(InstructionCode.J, Instructions.Count);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, new object[] { "continue" }, sourcePosition: matchData.StartIndex));
		}

		private void AddBreak(StringMatchData matchData)
		{
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.US, new object[] { "break" }, sourcePosition: matchData.StartIndex, interruptable: true));
			AddReprocessInstruction(InstructionCode.J, Instructions.Count);
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, new object[] { "break" }, sourcePosition: matchData.StartIndex));
		}

		private void AddExpressionSuffix(FragmentMatchData matchData)
		{
			IMatchData operatorData = matchData.Parts[1];
			AddItem((FragmentMatchData)matchData.Parts[0]);
			int? shortcutInstruction = null;
			switch (operatorData.ToString())
			{
				case "&&":
					{
						Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: operatorData.StartIndex));
						shortcutInstruction = Instructions.Count;
						Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, sourcePosition: operatorData.StartIndex));
					}
					break;
				case "||":
					{
						Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: operatorData.StartIndex));
						shortcutInstruction = Instructions.Count;
						Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.C, sourcePosition: operatorData.StartIndex));
					}
					break;
			}
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			AddItem((FragmentMatchData)matchData.Parts[2]);
			switch (operatorData.ToString())
			{
				case ">":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RGreaterThan, sourcePosition: operatorData.StartIndex));
					break;
				case ">=":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RGreaterThanOrEquals, sourcePosition: operatorData.StartIndex));
					break;
				case "<":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLessThan, sourcePosition: operatorData.StartIndex));
					break;
				case "<=":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLessThanOrEquals, sourcePosition: operatorData.StartIndex));
					break;
				case "==":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.REquals, sourcePosition: operatorData.StartIndex));
					break;
				case "!=":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RNotEquals, sourcePosition: operatorData.StartIndex));
					break;
				case "&&":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RAnd, sourcePosition: operatorData.StartIndex));
					break;
				case "&":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RAnd, sourcePosition: operatorData.StartIndex));
					break;
				case "||":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.ROr, sourcePosition: operatorData.StartIndex));
					break;
				case "|":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.ROr, sourcePosition: operatorData.StartIndex));
					break;
				case "*":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RMultiply, sourcePosition: operatorData.StartIndex));
					break;
				case "/":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RDivide, sourcePosition: operatorData.StartIndex));
					break;
				case "+":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPlus, sourcePosition: operatorData.StartIndex));
					break;
				case "-":
					Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RSubtract, sourcePosition: operatorData.StartIndex));
					break;
			}
			if (shortcutInstruction != null)
			{
				Instructions[shortcutInstruction.Value] = InstructionProvider<GroupState>.GetInstruction(Instructions[shortcutInstruction.Value].Code, payload: new object[] { Instructions.Count }, sourcePosition: operatorData.StartIndex);
			}
		}

		private void ProcessFlowControls(int lbIndex, int lbcIndex, int lbeIndex)
		{
			Instructions[lbcIndex] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, new object[] { lbeIndex + 1 }, sourcePosition: Instructions[lbcIndex].SourcePosition);

			if (_reprocessRequiredIndexes.TryGetValue(InstructionCode.J, out List<int> reprocessIndexes))
			{
				foreach (int instructionIndex in reprocessIndexes)
				{
					Instruction<GroupState> instruction = Instructions[instructionIndex];
					if ((string)instruction.Payload[0] == "break")
					{
						Instructions[instructionIndex - 1] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.US, new object[] { lbIndex }, sourcePosition: instruction.SourcePosition, interruptable: instruction.Interruptable);
						Instructions[instructionIndex] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, new object[] { lbeIndex + 1 }, sourcePosition: instruction.SourcePosition);
					}
					else if ((string)instruction.Payload[0] == "continue")
					{
						Instructions[instructionIndex - 1] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.US, new object[] { lbIndex }, sourcePosition: instruction.SourcePosition, interruptable: instruction.Interruptable);
						Instructions[instructionIndex] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, new object[] { lbIndex }, sourcePosition: instruction.SourcePosition);
					}
				}
			}
			_reprocessRequiredIndexes.Remove(InstructionCode.J);
		}
		#endregion

		#region Add variable

		private void AddGetClassVariable(FragmentMatchData matchData, bool interruptable = false)
		{
			string variableName = GetIdentifierText(matchData);
			if (_namespace?.Groups.ContainsKey(variableName) ?? false)
			{
				AddGetClass(variableName, matchData.StartIndex, interruptable);
			}
			else if (_action.Contains($"$c_{variableName}"))
			{
				AddGetStackVariable($"$c_{variableName}", matchData.StartIndex, interruptable);
			}
			else
			{
				throw new LanguageConstraintException($"Class '{variableName}' not found.", matchData.StartIndex);
			}
		}

		private void AddGetClass(string variableName, int startIndex, bool interruptable)
		{
			if (_namespace?.Groups.ContainsKey(variableName) ?? false)
			{
				int location = _class.AddOrGetDependency(variableName);
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.GDR, new object[] { location }, sourcePosition: startIndex, interruptable: interruptable));
			}
			else
			{
				throw new LanguageConstraintException($"Class '{variableName}' not found.", startIndex);
			}
		}

		private void AddGetVariable(FragmentMatchData matchData, bool interruptable = false)
		{
			string variableName = GetIdentifierText(matchData);
			AddGetVariable(variableName, matchData.StartIndex, interruptable);
		}

		private void AddGetVariable(string variableName, int startIndex, bool interruptable)
		{
			if (_action.Contains(variableName))
			{
				AddGetStackVariable(variableName, startIndex, interruptable);
			}
			else if (_class?.Pointers.ContainsKey(variableName) ?? false)
			{
				AddGetGroupVariable(variableName, startIndex, interruptable);
			}
			else if (_class?.IsSubClass ?? false)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { variableName }, startIndex));
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.MPR, sourcePosition: startIndex, interruptable: interruptable));
			}
			else
			{
				throw new LanguageConstraintException($"Variable '{variableName}' is not declared.", startIndex);
			}
		}

		public void AddGetGroupVariable(string name, int startIndex, bool interruptable)
		{
			if (_class.Pointers.TryGetValue(name, out int location))
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.GPR, new object[] { location }, sourcePosition: startIndex, interruptable: interruptable));
			}
			else
			{
				throw new LanguageConstraintException($"Variable '{name}' is not declared.", startIndex);
			}
		}

		private void AddGetStackVariable(string name, int startIndex, bool interruptable)
		{
			if (_action.GetVariableOrDefault(name) is VariableInfo actionVariable)
			{
				Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.SPR, new object[] { _action.ActionStackLocation - actionVariable.StackLocation - 1 }, sourcePosition: startIndex, interruptable: interruptable));
			}
			else
			{
				throw new LanguageConstraintException($"Variable '{name}' is not declared.", startIndex);
			}
		}

		private void AddNewStackVariable(FragmentMatchData fragmentMatchData)
		{
			string name = GetIdentifierText(fragmentMatchData);
			AddNewStackVariable(name, fragmentMatchData.StartIndex, true);
		}

		private void AddNewStackVariable(string name, int startIndex, bool interruptable)
		{
			if (_action.Variables.ContainsKey(name))
			{
				throw new LanguageConstraintException($"Variable '{name}' is already declared.", startIndex);
			}
			_action.Variables[name] = new VariableInfo
			{
				Name = name,
				StackLocation = _action.ActionStackLocation++,
				Depth = _action.BlockDepth
			};
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CSP, new object[] { name }, sourcePosition: startIndex, interruptable: interruptable));
		}

		private void AddGetOrNewStackVariable(string name, int startIndex, bool interruptable)
		{
			if (_action.Variables.ContainsKey(name))
			{
				AddGetStackVariable(name, startIndex, interruptable);
			}
			else
			{
				AddNewStackVariable(name, startIndex, interruptable);
			}
		}

		public void AddNewGroupVariable(string name, int startIndex, bool interruptable)
		{
			if (_class.Pointers.ContainsKey(name))
			{
				throw new LanguageConstraintException($"Variable '{name}' is already declared.", startIndex);
			}
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.MP, payload: new object[] { name, _class.Pointers.Count }, sourcePosition: startIndex, interruptable: false));
			Instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CGP, payload: new object[] { name }, sourcePosition: startIndex, interruptable: interruptable));
			_class.Pointers[name] = _class.Pointers.Count;
		}

		#endregion

		private string GetIdentifierText(FragmentMatchData matchData)
		{
			return matchData.Parts[0].ToString();
		}

		private string GetNamespaceIdentifier(FragmentMatchData matchData)
		{
			return string.Join('.', matchData.Parts.Cast<StringMatchData>().Select(part => part.ToString()));
		}
	}
}
