using StaxeTests.Shared.Matcher;
using StaxeTests.TestComplexLang.Engine.Generator;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Generator;
using Synfron.Staxe.Matcher;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StaxeTests.TestSimpleLang.Tests
{
	public class PostGen_ExpressionTests : ExpressionTests
	{
		public PostGen_ExpressionTests()
		{
			LanguageMatchEngine = Synfron.Staxe.Matcher.LanguageMatchEngine.Build(LanguageMatcherProvider.GetStaxeTestComplexLangMatcher());
		}
	}
	public class PreGen_ExpressionTests : ExpressionTests
	{
		public PreGen_ExpressionTests()
		{
			LanguageMatchEngine = GeneratedLanguageMatchEngineRegistry.GetStaxeTestComplexLangMatchEngine();
		}
	}

	public abstract class ExpressionTests
	{

		protected ILanguageMatchEngine LanguageMatchEngine { get; set; }

		[Theory]
		[InlineData("return true;", true)]
		[InlineData("return false;", false)]
		[InlineData("return false | true;", true)]
		[InlineData("return false || true;", true)]
		[InlineData("return false | false;", false)]
		[InlineData("return false || false;", false)]
		[InlineData("return false & true;", false)]
		[InlineData("return false && true;", false)]
		[InlineData("return true & true;", true)]
		[InlineData("return true && true;", true)]
		[InlineData("return true | true & false;", true)]
		[InlineData("return true || true && false;", true)]
		[InlineData("return true | false & false;", true)]
		[InlineData("return true || false && false;", true)]
		[InlineData("return 3 == 3;", true)]
		[InlineData("return true == 3 == 3;", false)]
		[InlineData("return 6 == 2 | true;", true)]
		[InlineData("return 6 == 2 || true;", true)]
		[InlineData("return true & true | false;", true)]
		[InlineData("return true && true || false;", true)]
		[InlineData("return true & false | false;", false)]
		[InlineData("return true && false || false;", false)]
		[InlineData("return true & true & false;", false)]
		[InlineData("return true && true && false;", false)]
		[InlineData("return (true | true | true) | true;", true)]
		[InlineData("return (true || true || true) || true;", true)]
		[InlineData("return (true & true & true) & true;", true)]
		[InlineData("return (true && true && true) && true;", true)]
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

			Assert.Equal(expectedResult, ((DefaultBooleanValue<GroupState>)executionState.ListRegister.Last().Value).Data);
		}

		[Theory]
		[InlineData("return 64;", 64)]
		[InlineData("return 64/2;", 32)]
		[InlineData("return 64/4*2;", 32)]
		[InlineData("return 2+3*2+25-4*-20/2-6*4-8-16+8/2-10-2-4+64/4*2;", 45)]
		[InlineData("return -2+3*((2+(25-4)*-20)/2-(6*4--8-16+8/2-10-2-4+64)/4)*2;", -1358)]
		[InlineData("return 2|4;", 6)]
		[InlineData("return 2&3;", 2)]
		[InlineData("return 4--4;", 8)]
		[InlineData("return (1 | 2 | 3) | 4;", 7)]
		[InlineData("return (1 & 2 & 3) & 4;", 0)]
		[InlineData("return 1 + 3 + 5 + 7 * 2 * 4 * 6 + 9 + 11 + 13 * 8 * 10 * 12;", 12845)]
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

			Assert.Equal(expectedResult, ((DefaultIntValue<GroupState>)executionState.ListRegister.Last().Value).Data);
		}
	}
}
