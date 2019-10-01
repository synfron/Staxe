using StaxeTests.Shared.Matcher;
using StaxeTests.TestExpressionLang.Engine.Generator;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Generator;
using Synfron.Staxe.Matcher;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StaxeTests.TestExpressionLang.Tests
{
	public class PostGen_ExpressionTests : ExpressionTests
	{
		public PostGen_ExpressionTests()
		{
			LanguageMatchEngine = Synfron.Staxe.Matcher.LanguageMatchEngine.Build(LanguageMatcherProvider.GetStaxeTestExpressionLangMatcher());
		}
	}
	public class PreGen_ExpressionTests : ExpressionTests
	{
		public PreGen_ExpressionTests()
		{
			LanguageMatchEngine = GeneratedLanguageMatchEngineRegistry.GetStaxeTestExpressionLangMatchEngine();
		}
	}

	public abstract class ExpressionTests
	{

		protected ILanguageMatchEngine LanguageMatchEngine { get; set; }

		[Theory]
		[InlineData("true", true)]
		[InlineData("false", false)]
		[InlineData("false | true", true)]
		[InlineData("false | false", false)]
		[InlineData("false & true", false)]
		[InlineData("true & true", true)]
		[InlineData("true | true & false", true)]
		[InlineData("true | false & false", true)]
		[InlineData("3 == 3", true)]
		[InlineData("true == 3 == 3", false)]
		[InlineData("6 == 2 | true", true)]
		[InlineData("true & true | false", true)]
		[InlineData("true & false | false", false)]
		[InlineData("true & true & false", false)]
		[InlineData("(true | true | true) | true", true)]
		[InlineData("(true & true & true) & true", true)]
		public void BooleanTests(string expression, bool expectedResult)
		{
			(Synfron.Staxe.Matcher.Data.IMatchData matchData, bool success, int _, int? _, string _) = LanguageMatchEngine.Match(expression);

			Assert.True(success);

			InstructionGenerator generator = new InstructionGenerator();
			IList<Instruction<GroupState>> instructions = generator.Generate(matchData);

			InstructionOptimizer<GroupState> optimizer = new InstructionOptimizer<GroupState>();
			instructions = optimizer.Optimize(instructions);
			GroupState groupState = new GroupState();
			groupState.Group.Instructions = instructions.ToArray();
			ExecutionState<GroupState> executionState = new ExecutionState<GroupState>(groupState);
			InstructionExecutor<GroupState> executor = new InstructionExecutor<GroupState>();
			executor.Execute(executionState);

			Assert.Equal(expectedResult, ((DefaultBooleanValue<GroupState>)Enumerable.Last(executionState.StackRegister).Value).Data);
		}

		[Theory]
		[InlineData("64", 64)]
		[InlineData("64/2", 32)]
		[InlineData("64/4*2", 32)]
		[InlineData("2+3*2+25-4*-20/2-6*4-8-16+8/2-10-2-4+64/4*2", 45)]
		[InlineData("-2+3*((2+(25-4)*-20)/2-(6*4--8-16+8/2-10-2-4+64)/4)*2", -1358)]
		[InlineData("2^8", 256)]
		[InlineData("5!", 120)]
		[InlineData("2|4", 6)]
		[InlineData("2&3", 2)]
		[InlineData("4--4", 8)]
		[InlineData("(1 | 2 | 3) | 4", 7)]
		[InlineData("(1 & 2 & 3) & 4", 0)]
		[InlineData("1 + 3 + 5 + 7 * 2 * 4 * 6 + 9 + 11 + 13 * 8 * 10 * 12", 12845)]
		public void NumberTests(string expression, object expectedResult)
		{
			(Synfron.Staxe.Matcher.Data.IMatchData matchData, bool success, int _, int? _, string _) = LanguageMatchEngine.Match(expression);

			Assert.True(success);

			InstructionGenerator generator = new InstructionGenerator();
			IList<Instruction<GroupState>> instructions = generator.Generate(matchData);

			InstructionOptimizer<GroupState> optimizer = new InstructionOptimizer<GroupState>();
			instructions = optimizer.Optimize(instructions);

			GroupState groupState = new GroupState();
			groupState.Group.Instructions = instructions.ToArray();
			ExecutionState<GroupState> executionState = new ExecutionState<GroupState>(groupState);
			InstructionExecutor<GroupState> executor = new InstructionExecutor<GroupState>();
			executor.Execute(executionState);

			Assert.Equal(expectedResult, ((DefaultIntValue<GroupState>)Enumerable.Last(executionState.StackRegister).Value).Data);
		}

		[Theory]
		[InlineData("64 +")]
		[InlineData("64 + true")]
		[InlineData("true + true")]
		[InlineData("5 | 6 | true")]
		[InlineData("(1 | 2 | 3) | true")]
		[InlineData("(1 & 2 & 3) & true")]
		public void InvalidExpressionTests(string expression)
		{
			(Synfron.Staxe.Matcher.Data.IMatchData _, bool success, int _, int? _, string _) = LanguageMatchEngine.Match(expression);

			Assert.False(success);
		}
	}
}
