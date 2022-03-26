using StaxeTests.Shared;
using StaxeTests.Shared.Matcher;
using StaxeTests.TestSimpleLang.Engine.Generator;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Generator;
using Synfron.Staxe.Matcher;
using Synfron.Staxe.Shared.Exceptions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace StaxeTests.TestSimpleLang.Tests
{
	public class PostGen_ScriptTests : ScriptTests
	{
		public PostGen_ScriptTests()
		{
			LanguageMatchEngine = Synfron.Staxe.Matcher.LanguageMatchEngine.Build(LanguageMatcherProvider.GetStaxeTestSimpleLangMatcher());
		}
	}
	public class PreGen_ScriptTests : ScriptTests
	{
		public PreGen_ScriptTests()
		{
			LanguageMatchEngine = GeneratedLanguageMatchEngineRegistry.GetStaxeTestSimpleLangMatchEngine();
		}
	}

	public abstract class ScriptTests
	{

		protected ILanguageMatchEngine LanguageMatchEngine { get; set; }

		private ExecutionState<GroupState> Run(string code)
		{
			(Synfron.Staxe.Matcher.Data.IMatchData matchData, bool success, int _, int? _, string _) = LanguageMatchEngine.Match(code);

			//Assert.True(success);

			InstructionGenerator generator = new InstructionGenerator();
			IList<Instruction<GroupState>> instructions = generator.Generate(matchData);

			InstructionOptimizer<GroupState> optimizer = new InstructionOptimizer<GroupState>();
			instructions = optimizer.Optimize(instructions).ToArray();

			GroupState groupState = new GroupState();
			groupState.Group.Instructions = instructions.ToArray();
			ExecutionState<GroupState> executionState = new ExecutionState<GroupState>(groupState);

			InstructionExecutor<GroupState> executor = new InstructionExecutor<GroupState>();
			//PrintDiagnostics.EnableDiagnostics(code, executor, executionState, true);
			executor.Execute(executionState);

			return executionState;
		}

		[Fact]
		public void FibonacciTest()
		{
			string fibonacci = @"
			var fibonacci = $(n) {
				var a = 0;
				var b = 1;
				var i = 0;
				while (i < n) {
					i = 1 + i;
					var temp = a;
					a = b;
					b = temp + b;
				}
				return a;
			};

			var getFib = $(n) {
				var i = 0;
				var fibonacciSum = 0;
				while (i < n) {
					i = i + 1;
					fibonacciSum = fibonacciSum + fibonacci(i);
				}
				return fibonacciSum;
			};

			getFib(44);";

			ExecutionState<GroupState> executionState = Run(fibonacci);

			Assert.Equal(new DefaultIntValue<GroupState>(1836311902), executionState.StackRegister.Last().Value);
		}

		[Fact]
		public void ConditionTest()
		{
			string code = @"
			var greeting;
			var condition = true;

			if (condition) {
				greeting = ""Hello"";
			}
			else if (condition) {
				greeting = ""Goodbye"";
			}
			else {
				greeting = ""Greetings"";
			}

			if (!condition) {
				greeting = greeting + "" cruel"";
			}
			else if (true) {
				greeting = greeting + "" beautiful"";
			}
			else {
				greeting = greeting + "" great"";
			}

			if (false) {
				greeting = greeting + "" planet."";
			}
			else if (!condition) {
				greeting = greeting + "" earth."";
			}
			else {
				greeting = greeting + "" world."";
			}

			return greeting;";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultStringValue<GroupState>("Hello beautiful world."), executionState.GetReturn());
		}

		[Fact]
		public void ArrayTest()
		{
			string code = @"
			var array = new [];
			array[0] = 20;
			array[1] = ""world"";
			array[2] = true;
			array[3] = ""hello"";
			array[""ab""] = array[1];
			array[1] = 10.5;
			var combo = """" + array[0] + array[1] + array[2] + array[3] + array[""ab""];
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultStringValue<GroupState>("2010.5truehelloworld"), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("combo"));
		}


		[Fact]
		public void ClosureTest()
		{
			string closure = @"
			var outsider = 50;
			var modifier = $() {
				var insider = outsider;
				outsider = 60;
				return insider;
			};

			var insider = modifier();";

			ExecutionState<GroupState> executionState = Run(closure);

			IValue<GroupState>[] items = executionState.StackPointers.OfType<ValuePointer<GroupState>>().Select(pointer => pointer.Value).OfType<IValue<GroupState>>().ToArray();

			Assert.Equal(new DefaultIntValue<GroupState>(60), items[0]);
			Assert.Equal(new DefaultIntValue<GroupState>(50), items[1]);
		}


		[Fact]
		public void UndefinedVariableTest()
		{
			string undefined = @"
			var test = 50;
			var testCopy = noTest;";

			(Synfron.Staxe.Matcher.Data.IMatchData matchData, bool success, int _, int? _, string _) = LanguageMatchEngine.Match(undefined);

			Assert.True(success);

			InstructionGenerator generator = new InstructionGenerator();

			LanguageConstraintException exception = Assert.Throws<LanguageConstraintException>(() => generator.Generate(matchData));

			Assert.Contains("Variable 'noTest' is not declared.", exception.Message);
			Assert.Equal(undefined.IndexOf("noTest"), exception.Position);
		}

		[Fact]
		public void InvalidFunctionTest()
		{
			string invalidFunction = @"
			var fibonacci = $(n) {
				var a = 0;
				var b = 1;
				var i = 0;
				while (i < n) {
					i = 1 +;
					var temp = a;
					a = b;
					b = temp + b;
				}
				return a;
			};";

			(Synfron.Staxe.Matcher.Data.IMatchData _, bool success, int _, int? failureIndex, string _) = LanguageMatchEngine.Match(invalidFunction);

			Assert.False(success);
			Assert.Equal(invalidFunction.IndexOf("+;") + 1, failureIndex.Value);
		}

		[Fact]
		public void InvalidAssignmentTest()
		{
			string invalidAssignment = @"
			var callable = $() {
			};

			callable() = 7;";

			(Synfron.Staxe.Matcher.Data.IMatchData matchData, bool success, int _, int? _, string _) = LanguageMatchEngine.Match(invalidAssignment);

			Assert.True(success);

			InstructionGenerator generator = new InstructionGenerator();

			LanguageConstraintException exception = Assert.Throws<LanguageConstraintException>(() => generator.Generate(matchData));

			Assert.Contains("Cannot assign to the return of a function.", exception.Message);
			Assert.Equal(invalidAssignment.IndexOf("= 7"), exception.Position);
		}

		[Fact]
		public void PlusAssignmentTest()
		{
			string code = @"
			var number = 10;
			number += 20;
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(30), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number"));
		}

		[Fact]
		public void IncrementTest()
		{
			string code = @"
			var number1 = 10;
			var number2 = 10;
			var number3 = number1++ + ++number2;
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(11), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number1"));
			Assert.Equal(new DefaultIntValue<GroupState>(11), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number2"));
			Assert.Equal(new DefaultIntValue<GroupState>(21), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number3"));
		}

		[Fact]
		public void DecrementTest()
		{
			string code = @"
			var number1 = 10;
			var number2 = 10;
			var number3 = number1-- - --number2;
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(9), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number1"));
			Assert.Equal(new DefaultIntValue<GroupState>(9), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number2"));
			Assert.Equal(new DefaultIntValue<GroupState>(1), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number3"));
		}

		[Fact]
		public void WhileLoopTest()
		{
			string code = @"
			var value = 1;
			while (value < 10) {
				value++;
			}
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(10), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value"));
		}

		[Fact]
		public void ContinueWhileLoopTest()
		{
			string code = @"
			var value = 1;
			var i = 1;
			while (i < 10) {
				i++;
				if (i == 5) {
					continue;
				}
				value++;
			}
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(9), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value"));
		}

		[Fact]
		public void BreakWhileLoopTest()
		{
			string code = @"
			var value = 1;
			while (value < 10) {
				if (value == 5) {
					break;
				}
				value++;
			}
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(5), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value"));
		}

		//[Fact]
		public void FibonacciPerformanceTest()
		{
			Stopwatch timer = new Stopwatch();
			timer.Start();

			string fibonacci = @"
			var fibonacci = $(n) {
				var a = 0;
				var b = 1;
				var i = 0;
				while (i < n) {
					i = 1 + i;
					var temp = a;
					a = b;
					b = temp + b;
				}
				return a;
			};

			var getFib = $(n) {
				var i = 0;
				var fibonacciSum = 0;
				while (i < n) {
					i = i + 1;
					fibonacciSum = fibonacciSum + fibonacci(i);
				}
				return fibonacciSum;
			};

			var iterations = 0;
			while (iterations < 2000) {
				getFib(44);
				iterations = iterations + 1;
			}";

			Run(fibonacci);

			timer.Stop();
			Assert.Equal(0, timer.ElapsedTicks);
		}
	}
}
