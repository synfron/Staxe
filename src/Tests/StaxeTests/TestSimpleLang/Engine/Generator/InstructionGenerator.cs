using StaxeTests.Shared.Generator;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Generator;
using Synfron.Staxe.Matcher.Data;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StaxeTests.TestSimpleLang.Engine.Generator
{
	public class InstructionGenerator : IInstructionGenerator<GroupState>
	{
		private ActionInfo _action;
		private List<Instruction<GroupState>> _instructions;
		private Dictionary<InstructionCode, List<int>> _reprocessRequiredIndexes = new Dictionary<InstructionCode, List<int>>();
		private bool _isAssignmentTarget;

		public IList<Instruction<GroupState>> Generate(IMatchData matchData)
		{
			_action = new ActionInfo();
			_instructions = new List<Instruction<GroupState>>();
			_reprocessRequiredIndexes = new Dictionary<InstructionCode, List<int>>();

			foreach (IMatchData partMatchData in ((FragmentMatchData)matchData).Parts)
			{
				AddItem(partMatchData);
			}
			if (_reprocessRequiredIndexes.Count > 0)
			{
				throw new LanguageConstraintException("Invalid operation", _reprocessRequiredIndexes.Values.FirstOrDefault()?.FirstOrDefault());
			}
			return _instructions;
		}

		private void AddItem(IMatchData matchData, bool interruptable = false)
		{
			switch (matchData.Name)
			{
				case "WhileBlock":
					AddWhileBlock((FragmentMatchData)matchData);
					break;
				case "IfElseBlock":
					AddIfElseBlock((FragmentMatchData)matchData);
					break;
				case "Block":
					AddBlock((FragmentMatchData)matchData);
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
				case "ParensValuable":
					AddParensValuable((FragmentMatchData)matchData);
					break;
				case "Identifier":
					AddGetVariable((FragmentMatchData)matchData, interruptable);
					break;
				case "ValuedIndex":
					AddValuedIndex((FragmentMatchData)matchData);
					break;
				case "ArgumentValues":
					AddArgumentValues((FragmentMatchData)matchData);
					break;
				case "AnonymousFunction":
					AddAnonymousFunction((FragmentMatchData)matchData);
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

		#region Add functions

		private void AddAnonymousFunction(FragmentMatchData matchData)
		{
			FragmentMatchData functionParameters = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData body = (FragmentMatchData)matchData.Parts[1];

			ActionInfo outerAction = _action;
			ActionInfo innerAction = new ActionInfo
			{
				Parent = outerAction
			};
			_action = innerAction;

			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, payload: new object[] { "endaction" }, sourcePosition: matchData.StartIndex));

			int actionStartIndex = _instructions.Count;
			AddAction(matchData, functionParameters, body);

			_action = outerAction;

			_instructions[actionStartIndex - 1] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, payload: new object[] { _instructions.Count }, sourcePosition: matchData.StartIndex);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.AR, payload: new object[] { actionStartIndex }, sourcePosition: matchData.StartIndex));

			if (innerAction.OrderedVariablesFromParent.Count > 0)
			{
				foreach (string name in innerAction.OrderedVariablesFromParent.Select(variable => variable.Name).Reverse())
				{
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
					AddGetStackVariable(name, matchData.StartIndex, false);
				}

				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLR, new object[] { innerAction.OrderedVariablesFromParent.Count }, sourcePosition: matchData.StartIndex));
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LRAS, new object[] { innerAction.OrderedVariablesFromParent.Count }, sourcePosition: matchData.StartIndex));
			}

			if (_reprocessRequiredIndexes.Count > 0)
			{
				throw new LanguageConstraintException("Invalid operation", _reprocessRequiredIndexes.Values.FirstOrDefault()?.FirstOrDefault());
			}
		}

		private void AddAction(FragmentMatchData matchData, FragmentMatchData functionParameters, FragmentMatchData body)
		{

			// Start the declaration of the function
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.A, new object[] { false }, sourcePosition: functionParameters.StartIndex));
			AddFunctionParameters(functionParameters);

			// Add instructions for the body of the function
			AddItem(body);

			// End the function declaration
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.AE, sourcePosition: matchData.StartIndex + matchData.Length));
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
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LRR, new object[] { names.Length, false }, sourcePosition: matchData.StartIndex));
			List<object> payload = new List<object> { names.Length };
			foreach (string name in names)
			{
				payload.Add(InstructionCode.CSP);
				payload.Add(name);
				AddParameterVariable(name);
			}
			if (matchData.Parts.Count > 0)
			{
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RCP, payload.ToArray(), sourcePosition: matchData.StartIndex));
			}
		}
		#endregion

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
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: partMatchData.StartIndex));
					AddItem(partMatchData, first);
					first = false;
				}
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLR, new object[] { matchData.Parts.Count }, sourcePosition: matchData.StartIndex));
			}
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RCE, sourcePosition: matchData.StartIndex));
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LRR, new object[] { 1, false }, sourcePosition: matchData.StartIndex));
		}

		private void AddValuedIndex(FragmentMatchData matchData)
		{
			FragmentMatchData evaluable = (FragmentMatchData)matchData.Parts[0];
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			AddItem(evaluable);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RVK, new object[] { _isAssignmentTarget }, sourcePosition: matchData.StartIndex));
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
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				AddItem(evaluable);
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CVR, new object[] { true, null }, sourcePosition: matchData.StartIndex));
			}
			else
			{
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CVR, new object[] { false, null }, sourcePosition: matchData.StartIndex));
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
			foreach (FragmentMatchData partMatchData in matchData.Parts)
			{
				AddItem(partMatchData);
			}
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
						_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
						AddIncrement((StringMatchData)partMatchData);
						break;
					case "Decrement":
						_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
						AddDecrement((FragmentMatchData)partMatchData);
						break;
				}
			}
		}

		private void AddValuableSuffix(FragmentMatchData matchData)
		{
			if (matchData.Parts.ElementAtOrDefault(0) is IMatchData partMatchData)
			{
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { null }, sourcePosition: matchData.StartIndex));
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RM, new object[] { 1 }, sourcePosition: matchData.StartIndex));
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RM, new object[] { 1 }, sourcePosition: matchData.StartIndex));
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.StartIndex));
				switch (partMatchData.Name)
				{
					case "Increment":
						AddIncrement((StringMatchData)partMatchData);
						break;
					case "Decrement":
						AddDecrement((FragmentMatchData)partMatchData);
						break;
				}
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PLR, sourcePosition: matchData.StartIndex));
			}
		}

		private void AddDecrement(FragmentMatchData matchData)
		{
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { 1 }, sourcePosition: matchData.StartIndex));
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RSubtract, sourcePosition: matchData.StartIndex));
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));
		}

		private void AddIncrement(StringMatchData matchData)
		{
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { 1 }, sourcePosition: matchData.StartIndex));
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPlus, sourcePosition: matchData.StartIndex));
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: matchData.StartIndex));
		}

		private void AddNot(FragmentMatchData matchData)
		{
			foreach (StringMatchData not in matchData.Parts)
			{
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RNot, sourcePosition: not.StartIndex));
			}
		}

		private void AddItemReturn(FragmentMatchData matchData)
		{
			if (matchData.Parts.ElementAtOrDefault(0) is FragmentMatchData item)
			{
				AddItem(item, true);
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLR, new object[] { 1 }, sourcePosition: matchData.StartIndex));
			}
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.AE, sourcePosition: matchData.StartIndex, interruptable: matchData.Parts.Count > 0));
		}

		private void AddBlock(FragmentMatchData matchData)
		{
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.B, sourcePosition: matchData.StartIndex, interruptable: true));
			int origActionStackLocation = _action.ActionStackLocation;
			_action.BlockDepth++;
			foreach (IMatchData partMatchData in matchData.Parts)
			{
				AddItem(partMatchData, true);
			}
			_action.Variables.Where(pair => pair.Value.Depth >= _action.BlockDepth).Select(pair => pair.Key).ToList().ForEach(key => _action.Variables.Remove(key));
			_action.ActionStackLocation = origActionStackLocation;
			_action.BlockDepth--;
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.BE, sourcePosition: matchData.StartIndex + matchData.Length, interruptable: true));
		}

		private void AddBoolean(StringMatchData matchData)
		{
			bool boolValue = bool.Parse(matchData.ToString());
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { boolValue }, sourcePosition: matchData.StartIndex));
		}

		private void AddNull(StringMatchData matchData)
		{
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { null }, sourcePosition: matchData.StartIndex));
		}

		private void AddStringLiteral(StringMatchData matchData)
		{
			string literal = matchData.ToString();
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { Regex.Unescape(literal.Substring(1, literal.Length - 2)) }, sourcePosition: matchData.StartIndex));
		}

		private void AddNumber(FragmentMatchData matchData)
		{
			string numberStr = string.Join("", matchData.Parts.Select(part => part.ToString()));
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { double.Parse(numberStr) }, sourcePosition: matchData.StartIndex));
		}

		private void AddAssignment(FragmentMatchData matchData)
		{
			_isAssignmentTarget = true;
			FragmentMatchData target = (FragmentMatchData)matchData.Parts[0];
			StringMatchData assignmentEqual = (StringMatchData)matchData.Parts[1];
			FragmentMatchData evaluable = (FragmentMatchData)matchData.Parts[2];
			AddItem(target, true);
			_isAssignmentTarget = false;

			if (_instructions.Last().Code == InstructionCode.LRR)
			{
				throw new LanguageConstraintException($"Cannot assign to the return of a function.", target.StartIndex + target.Length);
			}

			if (assignmentEqual.Name != "Equal")
			{
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: target.StartIndex));
			}

			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: target.StartIndex));
			AddItem(evaluable);

			switch (assignmentEqual.Name)
			{
				case "MinusEqual":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RSubtract, sourcePosition: target.StartIndex));
					break;
				case "PlusEqual":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPlus, sourcePosition: target.StartIndex));
					break;
			}

			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: target.StartIndex));
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
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: target.StartIndex));
			AddItem(evaluable);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RR, sourcePosition: target.StartIndex));
		}

		#region Conditions

		private int AddIfStatement(FragmentMatchData matchData)
		{
			FragmentMatchData condition = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData body = (FragmentMatchData)matchData.Parts[1];
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CS, sourcePosition: matchData.StartIndex, interruptable: true));
			AddItem(condition);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: condition.StartIndex));
			int cbcIndex = _instructions.Count;
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, sourcePosition: condition.StartIndex));
			AddItem(body);
			int endBlockNeededIndex = _instructions.Count;
			_instructions[cbcIndex] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, new object[] { endBlockNeededIndex + 1 }, sourcePosition: matchData.Parts[1].StartIndex);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CSE, new object[] { "endif" }, sourcePosition: matchData.StartIndex + matchData.Length));
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

			int endBlockIndex = _instructions.Count;

			foreach (int cbe in cbes)
			{
				_instructions[cbe] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.CSE, new IComparable[] { endBlockIndex }, sourcePosition: matchData.StartIndex + matchData.Length);
			}
		}

		#endregion

		#region Add loops

		private void AddWhileBlock(FragmentMatchData matchData)
		{
			FragmentMatchData condition = (FragmentMatchData)matchData.Parts[0];
			FragmentMatchData block = (FragmentMatchData)matchData.Parts[1];
			int origActionStackLocation = _action.ActionStackLocation;
			_action.BlockDepth++;
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.B, sourcePosition: matchData.StartIndex));
			int lbIndex = _instructions.Count;
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.L, sourcePosition: matchData.StartIndex, interruptable: true));
			AddItem(condition);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: matchData.Parts[1].StartIndex));
			int lbcIndex = _instructions.Count;
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, sourcePosition: matchData.Parts[1].StartIndex));
			AddItem(block);
			int lbeIndex = _instructions.Count;
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.LE, new object[] { lbIndex }, sourcePosition: matchData.StartIndex + matchData.Length));
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.BE, sourcePosition: matchData.StartIndex + matchData.Length));
			_action.BlockDepth--;

			ProcessFlowControls(lbIndex, lbcIndex, lbeIndex);

			_action.ActionStackLocation = origActionStackLocation;
		}

		private void AddContinue(StringMatchData matchData)
		{
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.US, new object[] { "continue" }, sourcePosition: matchData.StartIndex, interruptable: true));
			AddReprocessInstruction(InstructionCode.J, _instructions.Count);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, new object[] { "continue" }, sourcePosition: matchData.StartIndex));
		}

		private void AddBreak(StringMatchData matchData)
		{
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.US, new object[] { "break" }, sourcePosition: matchData.StartIndex, interruptable: true));
			AddReprocessInstruction(InstructionCode.J, _instructions.Count);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, new object[] { "break" }, sourcePosition: matchData.StartIndex));
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
						_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: operatorData.StartIndex));
						shortcutInstruction = _instructions.Count;
						_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, sourcePosition: operatorData.StartIndex));
					}
					break;
				case "||":
					{
						_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CPHR, sourcePosition: operatorData.StartIndex));
						shortcutInstruction = _instructions.Count;
						_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.C, sourcePosition: operatorData.StartIndex));
					}
					break;
			}
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			AddItem((FragmentMatchData)matchData.Parts[2]);
			switch (operatorData.ToString())
			{
				case ">":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RGreaterThan, sourcePosition: operatorData.StartIndex));
					break;
				case ">=":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RGreaterThanOrEquals, sourcePosition: operatorData.StartIndex));
					break;
				case "<":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLessThan, sourcePosition: operatorData.StartIndex));
					break;
				case "<=":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RLessThanOrEquals, sourcePosition: operatorData.StartIndex));
					break;
				case "==":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.REquals, sourcePosition: operatorData.StartIndex));
					break;
				case "!=":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RNotEquals, sourcePosition: operatorData.StartIndex));
					break;
				case "&&":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RAnd, sourcePosition: operatorData.StartIndex));
					break;
				case "&":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RAnd, sourcePosition: operatorData.StartIndex));
					break;
				case "||":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.ROr, sourcePosition: operatorData.StartIndex));
					break;
				case "|":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.ROr, sourcePosition: operatorData.StartIndex));
					break;
				case "*":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RMultiply, sourcePosition: operatorData.StartIndex));
					break;
				case "/":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RDivide, sourcePosition: operatorData.StartIndex));
					break;
				case "+":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RPlus, sourcePosition: operatorData.StartIndex));
					break;
				case "-":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RSubtract, sourcePosition: operatorData.StartIndex));
					break;
			}
			if (shortcutInstruction != null)
			{
				_instructions[shortcutInstruction.Value] = InstructionProvider<GroupState>.GetInstruction(_instructions[shortcutInstruction.Value].Code, payload: new object[] { _instructions.Count }, sourcePosition: operatorData.StartIndex);
			}
		}

		private void ProcessFlowControls(int lbIndex, int lbcIndex, int lbeIndex)
		{
			_instructions[lbcIndex] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.NC, new object[] { lbeIndex + 1 }, sourcePosition: _instructions[lbcIndex].SourcePosition);

			if (_reprocessRequiredIndexes.TryGetValue(InstructionCode.J, out List<int> reprocessIndexes))
			{
				foreach (int instructionIndex in reprocessIndexes)
				{
					Instruction<GroupState> instruction = _instructions[instructionIndex];
					if ((string)instruction.Payload[0] == "break")
					{
						_instructions[instructionIndex - 1] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.US, new object[] { lbIndex }, sourcePosition: instruction.SourcePosition, interruptable: instruction.Interruptable);
						_instructions[instructionIndex] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, new object[] { lbeIndex + 1 }, sourcePosition: instruction.SourcePosition);
					}
					else if ((string)instruction.Payload[0] == "continue")
					{
						_instructions[instructionIndex - 1] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.US, new object[] { lbIndex }, sourcePosition: instruction.SourcePosition, interruptable: instruction.Interruptable);
						_instructions[instructionIndex] = InstructionProvider<GroupState>.GetInstruction(InstructionCode.J, new object[] { lbIndex }, sourcePosition: instruction.SourcePosition);
					}
				}
			}
			_reprocessRequiredIndexes.Remove(InstructionCode.J);
		}
		#endregion

		#region Add variable

		private void AddGetVariable(FragmentMatchData matchData, bool interruptable = false)
		{
			string variableName = GetIdentifierText(matchData);
			AddGetStackVariable(variableName, matchData.StartIndex, interruptable);
		}

		private void AddGetStackVariable(string name, int startIndex, bool interruptable)
		{
			if (_action.GetVariableOrDefault(name) is VariableInfo actionVariable)
			{
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.SPR, new object[] { _action.ActionStackLocation - actionVariable.StackLocation - 1 }, sourcePosition: startIndex, interruptable: interruptable));
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
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.CSP, new object[] { name }, sourcePosition: startIndex, interruptable: interruptable));

		}

		#endregion
		private string GetIdentifierText(FragmentMatchData matchData)
		{
			return matchData.Parts[0].ToString();
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
	}
}
