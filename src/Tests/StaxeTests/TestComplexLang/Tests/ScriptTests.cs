using StaxeTests.Shared;
using StaxeTests.Shared.Executor;
using StaxeTests.Shared.Matcher;
using StaxeTests.TestComplexLang.Engine.Executor.Functions;
using StaxeTests.TestComplexLang.Engine.Executor.Values;
using StaxeTests.TestComplexLang.Engine.Generator;
using Synfron.Staxe.Executor;
using Synfron.Staxe.Executor.Instructions;
using Synfron.Staxe.Executor.Pointers;
using Synfron.Staxe.Executor.Values;
using Synfron.Staxe.Generator;
using Synfron.Staxe.Matcher;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace StaxeTests.TestComplexLang.Tests
{
	public class PostGen_ScriptTests : ScriptTests
	{
		public PostGen_ScriptTests()
		{
			LanguageMatchEngine = Synfron.Staxe.Matcher.LanguageMatchEngine.Build(LanguageMatcherProvider.GetStaxeTestComplexLangMatcher());
		}
	}
	public class PreGen_ScriptTests : ScriptTests
	{
		public PreGen_ScriptTests()
		{
			LanguageMatchEngine = GeneratedLanguageMatchEngineRegistry.GetStaxeTestComplexLangMatchEngine();
		}
	}

	public abstract class ScriptTests
	{

		protected ILanguageMatchEngine LanguageMatchEngine { get; set; }

		private ExecutionState<GroupState> Run(string code, InstructionExecutor<GroupState> executor = null)
		{
			(Synfron.Staxe.Matcher.Data.IMatchData matchData, bool success, int _, int? _, string log) = LanguageMatchEngine.Match(code);

			Assert.True(success);

			InstructionGenerator generator = new InstructionGenerator();
			IList<Instruction<GroupState>> instructions = generator.Generate(matchData);

			InstructionOptimizer<GroupState> optimizer = new InstructionOptimizer<GroupState>();
			instructions = optimizer.Optimize(instructions).ToArray();

			GroupState groupState = new GroupState();
			groupState.Group.Instructions = instructions.ToArray();
			ExecutionState<GroupState> executionState = new ExecutionState<GroupState>(groupState);

			executor = executor ?? new InstructionExecutor<GroupState>()
			{
				ExternalDynamicPointers = NativeLocator.GetNativePointer,
				ValueProvider = new ValueProvider()
			};
			//PrintDiagnostics.EnableDiagnostics(code, executor, executionState, true);

			executor.Execute(executionState);

			return executionState;
		}

		[Fact]
		public void FibonacciTest()
		{
			string fibonacci = @"
			var fibonacci = (n) {
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

			var getFib = (n) {
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
		public void FibonacciWithCommentsTest()
		{
			string fibonacci = @"//fibonacci(Number)
			var fibonacci /*: Function*/ = (n) {
				var a/*: Number*/ = 0;
				var b/*: Number*/ = 1;
				var i/*: Number*/ = 0;
				while (i < n) {
					i = 1 + i; //increment
					var temp/*: Number*/ = a;
					a = b;
					b = temp + b;
				}
				return a;
			};

			//getFib(Number)
			var getFib /*: Function*/ = (n/*: Number*/) {
				var i /*: Number*/ = 0;
				var fibonacciSum /*: Number*/ = 0;
				while (i < n) {
					/* increment */
					i = i + 1;
					fibonacciSum = fibonacciSum + fibonacci(i);
				}
				return fibonacciSum;
			};

			/*return */getFib(44);";

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
			var modifier = () {
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
			var fibonacci = (n) {
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
			var callable = () {
			};

			callable() = 7;";

			(Synfron.Staxe.Matcher.Data.IMatchData matchData, bool success, int _, int? _, string _) = LanguageMatchEngine.Match(invalidAssignment);

			Assert.True(success);

			InstructionGenerator generator = new InstructionGenerator();

			LanguageConstraintException exception = Assert.Throws<LanguageConstraintException>(() => generator.Generate(matchData));

			Assert.Contains("Cannot assign to the return of a function.", exception.Message);
			Assert.Equal(invalidAssignment.IndexOf(" = 7"), exception.Position);
		}

		[Fact]
		public void MapSetterTest()
		{
			string code = @"
var property = new [].{
	""number"" = 45,
	""value"" = 200000
};
var number = property.number;
var value = property.value;
";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(45), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number"));
			Assert.Equal(new DefaultIntValue<GroupState>(200000), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value"));
		}

		[Fact]
		public void ArraySetterTest()
		{
			string code = @"
var property = new {
	45,
	200000
};
var number = property[0];
var value = property[1];
";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(45), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number"));
			Assert.Equal(new DefaultIntValue<GroupState>(200000), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value"));
		}

		[Fact]
		public void ObjectSetterTest()
		{
			string code = @"
class Property {
	number;
	value;
}

var property = new Property().{
	""number"" = 45,
	""value"" = 200000
};
var number = property.number;
var value = property.value;
";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(45), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("number"));
			Assert.Equal(new DefaultIntValue<GroupState>(200000), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value"));
		}

		[Fact]
		public void ClassTest()
		{
			string code = @"
class Car {
	make;
	model;
	color;
				
	(make, model, color) {
		self.make = make;
		self.model = model;
		self.color = color;
	}

	getName() => make + "" "" + model;
				
}

var car = new Car(""Honda"", ""Civic"", ""Red"");
var secondCar = new Car(""Chevy"", ""Malibu"", ""Green"");

car.getName();
";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultStringValue<GroupState>("Honda Civic"), executionState.StackRegister.Last().Value);
		}

		[Fact]
		public void ClassWithOuterReferenceTest()
		{
			string code = @"
var namePrefix = ""Name: "";
class Car {
	make;
	model;
	color;
				
	(make, model, color) {
		self.make = make;
		self.model = model;
		self.color = color;
	}

	getName() => namePrefix + make + "" "" + model;
				
}

var car = new Car(""Honda"", ""Civic"", ""Red"");

car.getName();
";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultStringValue<GroupState>("Name: Honda Civic"), executionState.StackRegister.Last().Value);
		}

		[Fact]
		public void StaticClassTest()
		{
			string code = @"
static class Log {
	entryCount = 0;
	text = """";

	addRecord(record) {
		entryCount++;
		text += record + ""\n"";
	}
}

Log.addRecord(""Log 1"");
Log.addRecord(""Log 2"");

var size = Log.entryCount;
var log = Log.text;
";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(2), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("size"));
			Assert.Equal(new DefaultStringValue<GroupState>("Log 1\nLog 2\n"), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("log"));
		}

		[Fact]
		public void SubClassTest()
		{
			string code = @"
class Property {
	address;
	value;
	size;

	(address) {
		self.address = address;
		value = 0;
		size = 0;
	}
	
	getValueRatio() {
		if (size > 0) {
			return value / size;
		}
		return 0;
	}

	getType() => ""Unknown"";
}

class House : Property {
	floors;

	(address) {
		self.address = address;
		value = 0;
		size = 0;
		floors = 1;
	}

	getType() => ""House"";	
	
	getSizeRatio() {
		if (getSize() > 0) {
			return getSize() / floors;
		}
		return 0;
	}

	getSize() => size;
}

var property = new Property(""45 Grove St"").{
	value = 15000,
	size = 2000
};

var house = new House(""40 Grove St"").{
	value = 20000,
	size = 1000,
	floors = 2
};
var houseValue = house.value;
var propertyValue = property.value;
var type = house.getType();
var sizeRatio = house.getSizeRatio();
var valueRatio = house.getValueRatio();
";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(20000), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("houseValue"));
			Assert.Equal(new DefaultIntValue<GroupState>(15000), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("propertyValue"));
			Assert.Equal(new DefaultStringValue<GroupState>("House"), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("type"));
			Assert.Equal(new DefaultIntValue<GroupState>(500), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("sizeRatio"));
			Assert.Equal(new DefaultIntValue<GroupState>(20), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("valueRatio"));
		}

		[Fact]
		public void SubClassOverrideTest()
		{
			string code = @"
class C1 {
	size;
	setC1() {
		setC4();
	}
	setC4() {
		size = 40;
	}
}

class C2 : C1 {
}

class C3 : C2 {
	size;
	setC4() {
		size = 50;
	}

	getC3() {
		return size;
	}
}

var c3 = new C3();
c3.setC1();
var size = c3.getC3();
";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(50), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("size"));
		}

		[Fact]
		public void NamespaceTest()
		{
			string code = @"
namespace Geometry {
	class Line {
		a;
		b;
		() {
			a = new Point();
			b = new Point();
			Shapes.count++;
		}
	}

	class Point {
		x;
		y;
		() {
			x = 0;
			y = 0;
			Shapes.count++;
		}
	}

	static class Shapes {
		count = 0;
	}
}

using Geometry.Line;
using Geometry.Shapes;

var line = new Line();
line.a.{
	x = 10,
	y = 20
};
line.b.{
	x = 70,
	y = 80
};
var aX = line.a.x;
var aY = line.a.y;
var bX = line.b.x;
var bY = line.b.y;
var count = Shapes.count;
";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(10), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("aX"));
			Assert.Equal(new DefaultIntValue<GroupState>(20), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("aY"));
			Assert.Equal(new DefaultIntValue<GroupState>(70), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("bX"));
			Assert.Equal(new DefaultIntValue<GroupState>(80), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("bY"));
			Assert.Equal(new DefaultIntValue<GroupState>(3), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("count"));
		}

		[Fact]
		public void HostedClassTest()
		{
			string pointCode = @"
namespace Geometry {

	class Point {
		x;
		y;
		() {
			x = 0;
			y = 0;
			Shapes.count++;
		}
	}

	static class Shapes {
		count = 0;
	}
}
";
			string lineCode = @"
using Geometry.Point;
using Geometry.Shapes;

namespace Geometry {
	class Line {
		a;
		b;
		() {
			a = new Point();
			b = new Point();
			Shapes.count++;
		}
	}
}

using Geometry.Line;

var line = new Line();
line.a.{
	x = 10,
	y = 20
};
line.b.{
	x = 70,
	y = 80
};
var aX = line.a.x;
var aY = line.a.y;
var bX = line.b.x;
var bY = line.b.y;
var count = Shapes.count;
";

			InstructionExecutor<GroupState> executor = new InstructionExecutor<GroupState>()
			{
				ExternalDynamicPointers = NativeLocator.GetNativePointer,
				ValueProvider = new ValueProvider()
			};

			Run(pointCode, executor);

			ExecutionState<GroupState> executionState = Run(lineCode, executor);

			Assert.Equal(new DefaultIntValue<GroupState>(10), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("aX"));
			Assert.Equal(new DefaultIntValue<GroupState>(20), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("aY"));
			Assert.Equal(new DefaultIntValue<GroupState>(70), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("bX"));
			Assert.Equal(new DefaultIntValue<GroupState>(80), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("bY"));
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
		public void ForLoopTest()
		{
			string code = @"
			var value = 0;
			for (var i = 1, i < 10, i++) {
				value += i;
			}
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(45), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value"));
		}

		[Fact]
		public void ForEachLoopTest()
		{
			string code = @"
			var array = new { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
			var value = 0;
			foreach (var num of array) {
				value += num;
			}
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(45), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value"));
		}

		[Fact]
		public void NativeTest()
		{
			string code = @"
			var value1 = $System.Math.Max(3, 5);
			var value2 = $System.Math.PI;
			";

			ExecutionState<GroupState> executionState = Run(code);

			Assert.Equal(new DefaultIntValue<GroupState>(5), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value1"));
			Assert.Equal(new DefaultDoubleValue<GroupState>(Math.PI), executionState.GetStackValue<ExecutionState<GroupState>, GroupState>("value2"));
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
	}
}
