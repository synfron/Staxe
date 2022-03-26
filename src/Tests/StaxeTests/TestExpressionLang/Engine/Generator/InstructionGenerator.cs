using StaxeTests.TestExpressionLang.Engine.Engine;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Generator;
using Synfron.Staxe.Matcher.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StaxeTests.TestExpressionLang.Engine.Generator
{
	public class InstructionGenerator : IInstructionGenerator<GroupState>
	{
		private List<Instruction<GroupState>> _instructions;
		private readonly ExpressionInstructionProvider _instructionProvider = new ExpressionInstructionProvider();

		public IList<Instruction<GroupState>> Generate(IMatchData matchData)
		{
			_instructions = new List<Instruction<GroupState>>();
			AddItem(matchData);
			return _instructions;
		}

		private void AddPartItem(FragmentMatchData matchData)
		{
			AddItem(matchData.Parts[0]);
		}

		private void AddBoolean(IMatchData matchData)
		{
			bool boolValue = bool.Parse(matchData.ToString());
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { boolValue }, sourcePosition: matchData.StartIndex));
		}

		private void AddNumber(FragmentMatchData matchData)
		{
			string numberStr = matchData.Parts.Last().ToString();
			if (matchData.Parts.Count > 1)
			{
				numberStr = $"-{numberStr}";
			}
			object numberObj = int.TryParse(numberStr, out int numberInt) ? numberInt : (object)double.Parse(numberStr);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { numberObj }, sourcePosition: matchData.StartIndex));
		}

		private void AddNum(IMatchData matchData)
		{
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.VR, new object[] { double.Parse(matchData.ToString()) }, sourcePosition: matchData.StartIndex));
		}

		private void AddOperatedBooleanValuable(FragmentMatchData matchData)
		{
			AddItem(matchData.Parts.First(part => part.Name != "Nots"));
			foreach (IMatchData not in ((FragmentMatchData)matchData.Parts.FirstOrDefault(part => part.Name == "Nots")).Parts ?? Enumerable.Empty<IMatchData>())
			{
				_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RNot, sourcePosition: not.StartIndex));
			}
		}

		private void AddFactorialMathValuable(FragmentMatchData matchData)
		{
			AddItem(matchData.Parts.First());
			foreach (IMatchData factorial in ((FragmentMatchData)matchData.Parts.FirstOrDefault(part => part.Name == "Factorials")).Parts ?? Enumerable.Empty<IMatchData>())
			{
				_instructions.Add(_instructionProvider.GetSpecialInstruction(new string[] { "RFAC" }, sourcePosition: factorial.StartIndex));
			}
		}

		private void AddSquareRootMathValuable(FragmentMatchData matchData)
		{
			AddItem(matchData.Parts.First());
			_instructions.Add(_instructionProvider.GetSpecialInstruction(new string[] { "RSQRT" }, sourcePosition: matchData.StartIndex));
		}

		private void AddTreedExpression(FragmentMatchData matchData)
		{
			IMatchData operatorData = matchData.Parts[1];
			AddItem(matchData.Parts[0]);
			_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.PHR, sourcePosition: matchData.StartIndex));
			AddItem(matchData.Parts[2]);
			switch (operatorData.ToString())
			{
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
				case "^":
					_instructions.Add(_instructionProvider.GetSpecialInstruction(new string[] { "RPOW" }, sourcePosition: operatorData.StartIndex));
					break;
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
				case "&":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.RAnd, sourcePosition: operatorData.StartIndex));
					break;
				case "|":
					_instructions.Add(InstructionProvider<GroupState>.GetInstruction(InstructionCode.ROr, sourcePosition: operatorData.StartIndex));
					break;
			}
		}

		private void AddItem(IMatchData matchData)
		{
			switch (matchData.Name)
			{
				case "Boolean":
					AddBoolean(matchData);
					break;
				case "Number":
					AddNumber((FragmentMatchData)matchData);
					break;
				case "Num":
					AddNum(matchData);
					break;
				case "MathExpression":
				case "RelationalExpression":
				case "EqualityExpression":
				case "BooleanOrExpression":
				case "BooleanAndExpression":
				case "MathOrExpression":
				case "MathAndExpression":
					AddPartItem((FragmentMatchData)matchData);
					break;
				case "FactorialMathValuable":
					AddFactorialMathValuable((FragmentMatchData)matchData);
					break;
				case "SquareRootMathValuable":
					AddSquareRootMathValuable((FragmentMatchData)matchData);
					break;
				case "OperatedBooleanValuable":
					AddOperatedBooleanValuable((FragmentMatchData)matchData);
					break;
				case "MultiplicativeSuffix":
				case "AdditiveSuffix":
				case "SubtractiveSuffix":
				case "ExponentSuffix":
				case "EqualitySuffix":
				case "RelationalSuffix":
				case "BooleanAndSuffix":
				case "BooleanOrSuffix":
				case "MathAndSuffix":
				case "MathOrSuffix":
				case "SimpleMathAndSuffix":
				case "SimpleMathOrSuffix":
					AddTreedExpression((FragmentMatchData)matchData);
					break;
				default:
					throw new NotImplementedException($"Matcher '{matchData.Name}' is unhandled");
			}
		}
	}
}
