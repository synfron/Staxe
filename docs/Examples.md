# Examples

The tests in [StaxeTests](https://github.com/synfron/Staxe/tree/master/src/Tests/StaxeTests) provide various examples of using the Staxe.Matcher, Staxe.Generator, and Staxe.Executor libraries to build languages with a variety of features.

## JSON Parser
[Grammar](https://github.com/synfron/Staxe/blob/master/src/Tests/StaxeTests/Shared/Matcher/LanguageMatcherProvider.cs#L59)

### Features
- Provides similar feature to System.Json.JsonValue derived classes
- Library Usages: Staxe.Matcher


### Sample 

#### Language Code
```json
{
  "firstName": "John",
  "lastName": "Smith",
  "age": 25,
  "address": {
    "streetAddress": "21 2nd Street",
    "city": "New York",
    "state": "NY",
    "postalCode": "10021",
    "owner":  true
  },
  "phoneNumber": [
    {
      "type": "home",
      "number": "212 555-1234"
    },
    {
      "type": "fax",
      "number": "646 555-4567"
    }
  ],
  "gender": {
    "type": "male"
  }
}
```

#### Matcher Output (AST)
```xml
<Json>
    <Object>
        <KeyValue>
            <String>&quot;firstName&quot;</String>
            <String>&quot;John&quot;</String>
        </KeyValue>
        <KeyValue>
            <String>&quot;lastName&quot;</String>
            <String>&quot;Smith&quot;</String>
        </KeyValue>
        <KeyValue>
            <String>&quot;age&quot;</String>
            <Number>25</Number>
        </KeyValue>
        <KeyValue>
            <String>&quot;address&quot;</String>
            <Object>
                <KeyValue>
                    <String>&quot;streetAddress&quot;</String>
                    <String>&quot;21 2nd Street&quot;</String>
                </KeyValue>
                <KeyValue>
                    <String>&quot;city&quot;</String>
                    <String>&quot;New York&quot;</String>
                </KeyValue>
                <KeyValue>
                    <String>&quot;state&quot;</String>
                    <String>&quot;NY&quot;</String>
                </KeyValue>
                <KeyValue>
                    <String>&quot;postalCode&quot;</String>
                    <String>&quot;10021&quot;</String>
                </KeyValue>
                <KeyValue>
                    <String>&quot;owner&quot;</String>
                    <Boolean>true</Boolean>
                </KeyValue>
            </Object>
        </KeyValue>
        <KeyValue>
            <String>&quot;phoneNumber&quot;</String>
            <Array>
                <Object>
                    <KeyValue>
                        <String>&quot;type&quot;</String>
                        <String>&quot;home&quot;</String>
                    </KeyValue>
                    <KeyValue>
                        <String>&quot;number&quot;</String>
                        <String>&quot;212 555-1234&quot;</String>
                    </KeyValue>
                </Object>
                <Object>
                    <KeyValue>
                        <String>&quot;type&quot;</String>
                        <String>&quot;fax&quot;</String>
                    </KeyValue>
                    <KeyValue>
                        <String>&quot;number&quot;</String>
                        <String>&quot;646 555-4567&quot;</String>
                    </KeyValue>
                </Object>
            </Array>
        </KeyValue>
        <KeyValue>
            <String>&quot;gender&quot;</String>
            <Object>
                <KeyValue>
                    <String>&quot;type&quot;</String>
                    <String>&quot;male&quot;</String>
                </KeyValue>
            </Object>
        </KeyValue>
    </Object>
</Json>
```


## Expression Language
[Grammar](https://github.com/synfron/Staxe/blob/master/src/Tests/StaxeTests/TestExpressionLang/Files/StaxeTestExpressionLangDefinition.json)<br />
[Instruction Generator](https://github.com/synfron/Staxe/blob/master/src/Tests/StaxeTests/TestExpressionLang/Engine/Generator/InstructionGenerator.cs)

### Features
- Custom Instructions
- Restrict operators between Booleans and Numbers
- Library Usages: Staxe.Matcher, Staxe.Generator, Staxe.Executor

### Sample

#### Language Code
```
1 + 3 + 5 + 7! * 2 * 4 * 6 + 9 + 11^2 + 13 * 8 * 10 * 12
```

#### Matcher Output (AST)
```xml
<MathExpression>
    <AdditiveSuffix>
        <AdditiveSuffix>
            <AdditiveSuffix>
                <AdditiveSuffix>
                    <AdditiveSuffix>
                        <AdditiveSuffix>
                            <FactorialMathValuable>
                                <Number>
                                    <Number>1</Number>
                                </Number>
                                <Factorials></Factorials>
                            </FactorialMathValuable>
                            <Additive>+</Additive>
                            <FactorialMathValuable>
                                <Number>
                                    <Number>3</Number>
                                </Number>
                                <Factorials></Factorials>
                            </FactorialMathValuable>
                        </AdditiveSuffix>
                        <Additive>+</Additive>
                        <FactorialMathValuable>
                            <Number>
                                <Number>5</Number>
                            </Number>
                            <Factorials></Factorials>
                        </FactorialMathValuable>
                    </AdditiveSuffix>
                    <Additive>+</Additive>
                    <MultiplicativeSuffix>
                        <MultiplicativeSuffix>
                            <MultiplicativeSuffix>
                                <FactorialMathValuable>
                                    <Number><Number>7</Number>
                                    </Number>
                                    <Factorials><Not>!</Not>
                                    </Factorials>
                                </FactorialMathValuable>
                                <Multiplicative>*</Multiplicative>
                                <FactorialMathValuable>
                                    <Number><Number>2</Number>
                                    </Number>
                                    <Factorials></Factorials>
                                </FactorialMathValuable>
                            </MultiplicativeSuffix>
                            <Multiplicative>*</Multiplicative>
                            <FactorialMathValuable>
                                <Number>
                                    <Number>4</Number>
                                </Number>
                                <Factorials></Factorials>
                            </FactorialMathValuable>
                        </MultiplicativeSuffix>
                        <Multiplicative>*</Multiplicative>
                        <FactorialMathValuable>
                            <Number>
                                <Number>6</Number>
                            </Number>
                            <Factorials></Factorials>
                        </FactorialMathValuable>
                    </MultiplicativeSuffix>
                </AdditiveSuffix>
                <Additive>+</Additive>
                <FactorialMathValuable>
                    <Number>
                        <Number>9</Number>
                    </Number>
                    <Factorials></Factorials>
                </FactorialMathValuable>
            </AdditiveSuffix>
            <Additive>+</Additive>
            <ExponentSuffix>
                <FactorialMathValuable>
                    <Number>
                        <Number>11</Number>
                    </Number>
                    <Factorials></Factorials>
                </FactorialMathValuable>
                <Exponent>^</Exponent>
                <FactorialMathValuable>
                    <Number>
                        <Number>2</Number>
                    </Number>
                    <Factorials></Factorials>
                </FactorialMathValuable>
            </ExponentSuffix>
        </AdditiveSuffix>
        <Additive>+</Additive>
        <MultiplicativeSuffix>
            <MultiplicativeSuffix>
                <MultiplicativeSuffix>
                    <FactorialMathValuable>
                        <Number>
                            <Number>13</Number>
                        </Number>
                        <Factorials></Factorials>
                    </FactorialMathValuable>
                    <Multiplicative>*</Multiplicative>
                    <FactorialMathValuable>
                        <Number>
                            <Number>8</Number>
                        </Number>
                        <Factorials></Factorials>
                    </FactorialMathValuable>
                </MultiplicativeSuffix>
                <Multiplicative>*</Multiplicative>
                <FactorialMathValuable>
                    <Number>
                        <Number>10</Number>
                    </Number>
                    <Factorials></Factorials>
                </FactorialMathValuable>
            </MultiplicativeSuffix>
            <Multiplicative>*</Multiplicative>
            <FactorialMathValuable>
                <Number>
                    <Number>12</Number>
                </Number>
                <Factorials></Factorials>
            </FactorialMathValuable>
        </MultiplicativeSuffix>
    </AdditiveSuffix>
</MathExpression>
```

#### Optimized Instructions
```
CPR 0, 2, vr, 1, vr, 3
RPlus 
PHR 
VR 5
RPlus 
PHR 
VR 7
SPL "RFAC"
PHR 
VR 2
RMultiply 
PHR 
VR 4
RMultiply 
PHR 
VR 6
RMultiply 
RPlus 
PHR 
VR 9
RPlus 
CPR 0, 2, vr, 11, vr, 2
SPL "RPOW"
RPlus 
CPR 0, 2, vr, 13, vr, 8
RMultiply 
PHR 
VR 10
RMultiply 
PHR 
VR 12
RMultiply 
RPlus
```

## Simple Language
[Grammar](https://github.com/synfron/Staxe/blob/master/src/Tests/StaxeTests/TestSimpleLang/Files/StaxeTestSimpleLangDefinition.json)<br />
[Instruction Generator](https://github.com/synfron/Staxe/blob/master/src/Tests/StaxeTests/TestSimpleLang/Engine/Generator/InstructionGenerator.cs)

### Features
- Closures
- First-class functions
- Simplified number handling
- Library Usages: Staxe.Matcher, Staxe.Generator, Staxe.Executor

### Sample

#### Language Code
```csharp
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

getFib(44);
```

#### Matcher Output (AST)
```xml
<Script>
    <DeclarationAssignment>
        <Identifier>
            <Identifier>fibonacci</Identifier>
        </Identifier>
        <AnonymousFunction>
            <FunctionParameters>
                <Identifier>
                    <Identifier>n</Identifier>
                </Identifier>
            </FunctionParameters>
            <Block>
                <DeclarationAssignment>
                    <Identifier>
                        <Identifier>a</Identifier>
                    </Identifier>
                    <Valuable>
                        <Number>
                            <Number>0</Number>
                        </Number>
                    </Valuable>
                </DeclarationAssignment>
                <DeclarationAssignment>
                    <Identifier>
                        <Identifier>b</Identifier>
                    </Identifier>
                    <Valuable>
                        <Number>
                            <Number>1</Number>
                        </Number>
                    </Valuable>
                </DeclarationAssignment>
                <DeclarationAssignment>
                    <Identifier>
                        <Identifier>i</Identifier>
                    </Identifier>
                    <Valuable>
                        <Number>
                            <Number>0</Number>
                        </Number>
                    </Valuable>
                </DeclarationAssignment>
                <WhileBlock>
                    <RelationalSuffix>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier><Identifier>i</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                        <Relational>&lt;</Relational>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier><Identifier>n</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                    </RelationalSuffix>
                    <Block>
                        <Assignment>
                            <Valuable>
                                <DirectedValuableChain>
                                    <ValuablePrefix></ValuablePrefix>
                                    <ValuableChain><Identifier><Identifier>i</Identifier>
                                        </Identifier>
                                    </ValuableChain>
                                    <ValuableSuffix></ValuableSuffix>
                                </DirectedValuableChain>
                            </Valuable>
                            <Equal>=</Equal>
                            <AdditiveSuffix>
                                <Valuable>
                                    <Number>
                                        <Number>1</Number>
                                    </Number>
                                </Valuable>
                                <Additive>+</Additive>
                                <Valuable>
                                    <DirectedValuableChain>
                                        <ValuablePrefix></ValuablePrefix>
                                        <ValuableChain><Identifier><Identifier>i</Identifier>
                                            </Identifier>
                                        </ValuableChain>
                                        <ValuableSuffix></ValuableSuffix>
                                    </DirectedValuableChain>
                                </Valuable>
                            </AdditiveSuffix>
                        </Assignment>
                        <DeclarationAssignment>
                            <Identifier>
                                <Identifier>temp</Identifier>
                            </Identifier>
                            <Valuable>
                                <DirectedValuableChain>
                                    <ValuablePrefix></ValuablePrefix>
                                    <ValuableChain>
                                        <Identifier>
                                            <Identifier>a</Identifier>
                                        </Identifier>
                                    </ValuableChain>
                                    <ValuableSuffix></ValuableSuffix>
                                </DirectedValuableChain>
                            </Valuable>
                        </DeclarationAssignment>
                        <Assignment>
                            <Valuable>
                                <DirectedValuableChain>
                                    <ValuablePrefix></ValuablePrefix>
                                    <ValuableChain>
                                        <Identifier>
                                            <Identifier>a</Identifier>
                                        </Identifier>
                                    </ValuableChain>
                                    <ValuableSuffix></ValuableSuffix>
                                </DirectedValuableChain>
                            </Valuable>
                            <Equal>=</Equal>
                            <Valuable>
                                <DirectedValuableChain>
                                    <ValuablePrefix></ValuablePrefix>
                                    <ValuableChain>
                                        <Identifier>
                                            <Identifier>b</Identifier>
                                        </Identifier>
                                    </ValuableChain>
                                    <ValuableSuffix></ValuableSuffix>
                                </DirectedValuableChain>
                            </Valuable>
                        </Assignment>
                        <Assignment>
                            <Valuable>
                                <DirectedValuableChain>
                                    <ValuablePrefix></ValuablePrefix>
                                    <ValuableChain>
                                        <Identifier>
                                            <Identifier>b</Identifier>
                                        </Identifier>
                                    </ValuableChain>
                                    <ValuableSuffix></ValuableSuffix>
                                </DirectedValuableChain>
                            </Valuable>
                            <Equal>=</Equal>
                            <AdditiveSuffix>
                                <Valuable>
                                    <DirectedValuableChain>
                                        <ValuablePrefix></ValuablePrefix>
                                        <ValuableChain>
                                            <Identifier><Identifier>temp</Identifier>
                                            </Identifier>
                                        </ValuableChain>
                                        <ValuableSuffix></ValuableSuffix>
                                    </DirectedValuableChain>
                                </Valuable>
                                <Additive>+</Additive>
                                <Valuable>
                                    <DirectedValuableChain>
                                        <ValuablePrefix></ValuablePrefix>
                                        <ValuableChain>
                                            <Identifier><Identifier>b</Identifier>
                                            </Identifier>
                                        </ValuableChain>
                                        <ValuableSuffix></ValuableSuffix>
                                    </DirectedValuableChain>
                                </Valuable>
                            </AdditiveSuffix>
                        </Assignment>
                    </Block>
                </WhileBlock>
                <ItemReturn>
                    <Valuable>
                        <DirectedValuableChain>
                            <ValuablePrefix></ValuablePrefix>
                            <ValuableChain>
                                <Identifier>
                                    <Identifier>a</Identifier>
                                </Identifier>
                            </ValuableChain>
                            <ValuableSuffix></ValuableSuffix>
                        </DirectedValuableChain>
                    </Valuable>
                </ItemReturn>
            </Block>
        </AnonymousFunction>
    </DeclarationAssignment>
    <DeclarationAssignment>
        <Identifier>
            <Identifier>getFib</Identifier>
        </Identifier>
        <AnonymousFunction>
            <FunctionParameters>
                <Identifier>
                    <Identifier>n</Identifier>
                </Identifier>
            </FunctionParameters>
            <Block>
                <DeclarationAssignment>
                    <Identifier>
                        <Identifier>i</Identifier>
                    </Identifier>
                    <Valuable>
                        <Number>
                            <Number>0</Number>
                        </Number>
                    </Valuable>
                </DeclarationAssignment>
                <DeclarationAssignment>
                    <Identifier>
                        <Identifier>fibonacciSum</Identifier>
                    </Identifier>
                    <Valuable>
                        <Number>
                            <Number>0</Number>
                        </Number>
                    </Valuable>
                </DeclarationAssignment>
                <WhileBlock>
                    <RelationalSuffix>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier>
                                        <Identifier>i</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                        <Relational>&lt;</Relational>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier>
                                        <Identifier>n</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                    </RelationalSuffix>
                    <Block>
                        <Assignment>
                            <Valuable>
                                <DirectedValuableChain>
                                    <ValuablePrefix></ValuablePrefix>
                                    <ValuableChain>
                                        <Identifier>
                                            <Identifier>i</Identifier>
                                        </Identifier>
                                    </ValuableChain>
                                    <ValuableSuffix></ValuableSuffix>
                                </DirectedValuableChain>
                            </Valuable>
                            <Equal>=</Equal>
                            <AdditiveSuffix>
                                <Valuable>
                                    <DirectedValuableChain>
                                        <ValuablePrefix></ValuablePrefix>
                                        <ValuableChain>
                                            <Identifier><Identifier>i</Identifier>
                                            </Identifier>
                                        </ValuableChain>
                                        <ValuableSuffix></ValuableSuffix>
                                    </DirectedValuableChain>
                                </Valuable>
                                <Additive>+</Additive>
                                <Valuable>
                                    <Number>
                                        <Number>1</Number>
                                    </Number>
                                </Valuable>
                            </AdditiveSuffix>
                        </Assignment>
                        <Assignment>
                            <Valuable>
                                <DirectedValuableChain>
                                    <ValuablePrefix></ValuablePrefix>
                                    <ValuableChain>
                                        <Identifier>
                                            <Identifier>fibonacciSum</Identifier>
                                        </Identifier>
                                    </ValuableChain>
                                    <ValuableSuffix></ValuableSuffix>
                                </DirectedValuableChain>
                            </Valuable>
                            <Equal>=</Equal>
                            <AdditiveSuffix>
                                <Valuable>
                                    <DirectedValuableChain>
                                        <ValuablePrefix></ValuablePrefix>
                                        <ValuableChain>
                                            <Identifier><Identifier>fibonacciSum</Identifier>
                                            </Identifier>
                                        </ValuableChain>
                                        <ValuableSuffix></ValuableSuffix>
                                    </DirectedValuableChain>
                                </Valuable>
                                <Additive>+</Additive>
                                <Valuable>
                                    <DirectedValuableChain>
                                        <ValuablePrefix></ValuablePrefix>
                                        <ValuableChain>
                                            <Identifier><Identifier>fibonacci</Identifier>
                                            </Identifier>
                                            <ArgumentValues><Valuable><DirectedValuableChain><ValuablePrefix></ValuablePrefix><ValuableChain><Identifier><Identifier>i</Identifier>
                                                            </Identifier>
                                                        </ValuableChain>
                                                        <ValuableSuffix></ValuableSuffix>
                                                    </DirectedValuableChain>
                                                </Valuable>
                                            </ArgumentValues>
                                        </ValuableChain>
                                        <ValuableSuffix></ValuableSuffix>
                                    </DirectedValuableChain>
                                </Valuable>
                            </AdditiveSuffix>
                        </Assignment>
                    </Block>
                </WhileBlock>
                <ItemReturn>
                    <Valuable>
                        <DirectedValuableChain>
                            <ValuablePrefix></ValuablePrefix>
                            <ValuableChain>
                                <Identifier>
                                    <Identifier>fibonacciSum</Identifier>
                                </Identifier>
                            </ValuableChain>
                            <ValuableSuffix></ValuableSuffix>
                        </DirectedValuableChain>
                    </Valuable>
                </ItemReturn>
            </Block>
        </AnonymousFunction>
    </DeclarationAssignment>
    <DirectedValuableChain>
        <ValuablePrefix></ValuablePrefix>
        <ValuableChain>
            <Identifier>
                <Identifier>getFib</Identifier>
            </Identifier>
            <ArgumentValues>
                <Valuable>
                    <Number>
                        <Number>44</Number>
                    </Number>
                </Valuable>
            </ArgumentValues>
        </ValuableChain>
        <ValuableSuffix></ValuableSuffix>
    </DirectedValuableChain>
</Script>
```

#### Optimized Instructions

```
CSP "fibonacci"
PHR 
J 33
A false
LRR 1, false
RCP 1, csp, "n"
B 
VCSP 0, "a"
VCSP 1, "b"
VCSP 0, "i"
B 
L 
CPR -1, 2, spr, 0, spr, 3
RLessThan 
CPHR 
NC 27
B 
CPR -1, 3, spr, 0, vr, 1, spr, 0
RPlus 
RR 
SPCSP 3, "temp"
SPSP 2, 3
CPR -1, 3, spr, 2, spr, 0, spr, 2
RPlus 
RR 
BE 
LE 11
BE 
SPR 2
RLR 1
AE 
BE 
AE 
AR 3
RR 
CSP "getFib"
PHR 
J 73
A false
LRR 1, false
RCP 1, csp, "n"
B 
VCSP 0, "i"
VCSP 0, "fibonacciSum"
B 
L 
CPR -1, 2, spr, 1, spr, 2
RLessThan 
CPHR 
NC 67
B 
SPR 1
CPHR 
PHR 
VR 1
RPlus 
RR 
SPR 0
CPHR 
CPR 0, 2, spr, 3, spr, 1
RLR 1
RCE 
LRR 1, false
RPlus 
RR 
BE 
LE 45
BE 
SPR 0
RLR 1
AE 
BE 
AE 
AR 38
PHR 
SPR 1
RLR 1
LRAS 1
RR 
CPR -1, 2, spr, 0, vr, 44
RLR 1
RCE 
LRR 1, false
```

## Complex Language
[Grammar](https://github.com/synfron/Staxe/blob/master/src/Tests/StaxeTests/TestComplexLang/Files/StaxeTestComplexLangDefinition.json)<br />
[Instruction Generator](https://github.com/synfron/Staxe/blob/master/src/Tests/StaxeTests/TestComplexLang/Engine/Generator/InstructionGenerator.cs)

### Features
- Static and Non-Static Classes
- Inheritance
- Closures
- Enhanced Loops
- First-class functions
- .NET Class Interop
- Library Usages: Staxe.Matcher, Staxe.Generator, Staxe.Executor

### Sample

#### Language Code
```csharp
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

	getType() => "Unknown";
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

var property = new Property("45 Grove St").{
	value = 15000,
	size = 2000
};

var house = new House("40 Grove St").{
	value = 20000,
	size = 1000,
	floors = 2
};
var houseValue = house.value;
var propertyValue = property.value;
var type = house.getType();
var sizeRatio = house.getSizeRatio();
var valueRatio = house.getValueRatio();
```

#### Matcher Output (AST)
```xml
<Script>
    <Class>
        <Identifier>
            <Identifier>Property</Identifier>
        </Identifier>
        <OptionalBaseClassDeclaration></OptionalBaseClassDeclaration>
        <ClassBody>
            <PropertyDeclaration>
                <Identifier>
                    <Identifier>address</Identifier>
                </Identifier>
            </PropertyDeclaration>
            <PropertyDeclaration>
                <Identifier>
                    <Identifier>value</Identifier>
                </Identifier>
            </PropertyDeclaration>
            <PropertyDeclaration>
                <Identifier>
                    <Identifier>size</Identifier>
                </Identifier>
            </PropertyDeclaration>
            <Constructor>
                <FunctionParameters>
                    <Identifier>
                        <Identifier>address</Identifier>
                    </Identifier>
                </FunctionParameters>
                <Block>
                    <Assignment>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Self><Self>self</Self>
                                    </Self>
                                    <DotIdentifier><Identifier><Identifier>address</Identifier>
                                        </Identifier>
                                    </DotIdentifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                        <Equal>=</Equal>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier>
                                        <Identifier>address</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                    </Assignment>
                    <Assignment>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier>
                                        <Identifier>value</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                        <Equal>=</Equal>
                        <Valuable>
                            <Number>
                                <Number>0</Number>
                            </Number>
                        </Valuable>
                    </Assignment>
                    <Assignment>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier>
                                        <Identifier>size</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                        <Equal>=</Equal>
                        <Valuable>
                            <Number>
                                <Number>0</Number>
                            </Number>
                        </Valuable>
                    </Assignment>
                </Block>
            </Constructor>
            <Function>
                <Identifier>
                    <Identifier>getValueRatio</Identifier>
                </Identifier>
                <FunctionParameters></FunctionParameters>
                <Block>
                    <IfElseBlock>
                        <IfStatement>
                            <RelationalSuffix>
                                <Valuable>
                                    <DirectedValuableChain>
                                        <ValuablePrefix></ValuablePrefix>
                                        <ValuableChain><Identifier><Identifier>size</Identifier>
                                            </Identifier>
                                        </ValuableChain>
                                        <ValuableSuffix></ValuableSuffix>
                                    </DirectedValuableChain>
                                </Valuable>
                                <Relational>&gt;</Relational>
                                <Valuable>
                                    <Number>
                                        <Number>0</Number>
                                    </Number>
                                </Valuable>
                            </RelationalSuffix>
                            <Block>
                                <ItemReturn>
                                    <MultiplicativeSuffix>
                                        <Valuable>
                                            <DirectedValuableChain><ValuablePrefix></ValuablePrefix><ValuableChain><Identifier><Identifier>value</Identifier>
                                                    </Identifier>
                                                </ValuableChain>
                                                <ValuableSuffix></ValuableSuffix>
                                            </DirectedValuableChain>
                                        </Valuable>
                                        <Multiplicative>/</Multiplicative>
                                        <Valuable>
                                            <DirectedValuableChain>
                                                <ValuablePrefix></ValuablePrefix>
                                                <ValuableChain>
                                                    <Identifier><Identifier>size</Identifier>
                                                    </Identifier>
                                                </ValuableChain>
                                                <ValuableSuffix></ValuableSuffix>
                                            </DirectedValuableChain>
                                        </Valuable>
                                    </MultiplicativeSuffix>
                                </ItemReturn>
                            </Block>
                        </IfStatement>
                    </IfElseBlock>
                    <ItemReturn>
                        <Valuable>
                            <Number>
                                <Number>0</Number>
                            </Number>
                        </Valuable>
                    </ItemReturn>
                </Block>
            </Function>
            <Function>
                <Identifier>
                    <Identifier>getType</Identifier>
                </Identifier>
                <FunctionParameters></FunctionParameters>
                <ItemReturn>
                    <Valuable>
                        <StringLiteral>&quot;Unknown&quot;</StringLiteral>
                    </Valuable>
                </ItemReturn>
            </Function>
        </ClassBody>
    </Class>
    <Class>
        <Identifier>
            <Identifier>House</Identifier>
        </Identifier>
        <OptionalBaseClassDeclaration>
            <Identifier>
                <Identifier>Property</Identifier>
            </Identifier>
        </OptionalBaseClassDeclaration>
        <ClassBody>
            <PropertyDeclaration>
                <Identifier>
                    <Identifier>floors</Identifier>
                </Identifier>
            </PropertyDeclaration>
            <Constructor>
                <FunctionParameters>
                    <Identifier>
                        <Identifier>address</Identifier>
                    </Identifier>
                </FunctionParameters>
                <Block>
                    <Assignment>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Self>
                                        <Self>self</Self>
                                    </Self>
                                    <DotIdentifier>
                                        <Identifier>
                                            <Identifier>address</Identifier>
                                        </Identifier>
                                    </DotIdentifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                        <Equal>=</Equal>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier>
                                        <Identifier>address</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                    </Assignment>
                    <Assignment>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier>
                                        <Identifier>value</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                        <Equal>=</Equal>
                        <Valuable>
                            <Number>
                                <Number>0</Number>
                            </Number>
                        </Valuable>
                    </Assignment>
                    <Assignment>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier>
                                        <Identifier>size</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                        <Equal>=</Equal>
                        <Valuable>
                            <Number>
                                <Number>0</Number>
                            </Number>
                        </Valuable>
                    </Assignment>
                    <Assignment>
                        <Valuable>
                            <DirectedValuableChain>
                                <ValuablePrefix></ValuablePrefix>
                                <ValuableChain>
                                    <Identifier>
                                        <Identifier>floors</Identifier>
                                    </Identifier>
                                </ValuableChain>
                                <ValuableSuffix></ValuableSuffix>
                            </DirectedValuableChain>
                        </Valuable>
                        <Equal>=</Equal>
                        <Valuable>
                            <Number>
                                <Number>1</Number>
                            </Number>
                        </Valuable>
                    </Assignment>
                </Block>
            </Constructor>
            <Function>
                <Identifier>
                    <Identifier>getType</Identifier>
                </Identifier>
                <FunctionParameters></FunctionParameters>
                <ItemReturn>
                    <Valuable>
                        <StringLiteral>&quot;House&quot;</StringLiteral>
                    </Valuable>
                </ItemReturn>
            </Function>
            <Function>
                <Identifier>
                    <Identifier>getSizeRatio</Identifier>
                </Identifier>
                <FunctionParameters></FunctionParameters>
                <Block>
                    <IfElseBlock>
                        <IfStatement>
                            <RelationalSuffix>
                                <Valuable>
                                    <DirectedValuableChain>
                                        <ValuablePrefix></ValuablePrefix>
                                        <ValuableChain>
                                            <Identifier>
                                                <Identifier>getSize</Identifier>
                                            </Identifier>
                                            <ArgumentValues></ArgumentValues>
                                        </ValuableChain>
                                        <ValuableSuffix></ValuableSuffix>
                                    </DirectedValuableChain>
                                </Valuable>
                                <Relational>&gt;</Relational>
                                <Valuable>
                                    <Number>
                                        <Number>0</Number>
                                    </Number>
                                </Valuable>
                            </RelationalSuffix>
                            <Block>
                                <ItemReturn>
                                    <MultiplicativeSuffix>
                                        <Valuable>
                                            <DirectedValuableChain>
                                                <ValuablePrefix></ValuablePrefix>
                                                <ValuableChain>
                                                    <Identifier><Identifier>getSize</Identifier>
                                                    </Identifier>
                                                    <ArgumentValues></ArgumentValues>
                                                </ValuableChain>
                                                <ValuableSuffix></ValuableSuffix>
                                            </DirectedValuableChain>
                                        </Valuable>
                                        <Multiplicative>/</Multiplicative>
                                        <Valuable>
                                            <DirectedValuableChain>
                                                <ValuablePrefix></ValuablePrefix>
                                                <ValuableChain>
                                                    <Identifier><Identifier>floors</Identifier>
                                                    </Identifier>
                                                </ValuableChain>
                                                <ValuableSuffix></ValuableSuffix>
                                            </DirectedValuableChain>
                                        </Valuable>
                                    </MultiplicativeSuffix>
                                </ItemReturn>
                            </Block>
                        </IfStatement>
                    </IfElseBlock>
                    <ItemReturn>
                        <Valuable>
                            <Number>
                                <Number>0</Number>
                            </Number>
                        </Valuable>
                    </ItemReturn>
                </Block>
            </Function>
            <Function>
                <Identifier>
                    <Identifier>getSize</Identifier>
                </Identifier>
                <FunctionParameters></FunctionParameters>
                <ItemReturn>
                    <Valuable>
                        <DirectedValuableChain>
                            <ValuablePrefix></ValuablePrefix>
                            <ValuableChain>
                                <Identifier>
                                    <Identifier>size</Identifier>
                                </Identifier>
                            </ValuableChain>
                            <ValuableSuffix></ValuableSuffix>
                        </DirectedValuableChain>
                    </Valuable>
                </ItemReturn>
            </Function>
        </ClassBody>
    </Class>
    <DeclarationAssignment>
        <Identifier>
            <Identifier>property</Identifier>
        </Identifier>
        <Valuable>
            <DirectedValuableChain>
                <ValuablePrefix></ValuablePrefix>
                <ValuableChain>
                    <NewInstance>
                        <Identifier>
                            <Identifier>Property</Identifier>
                        </Identifier>
                        <ArgumentValues>
                            <Valuable>
                                <StringLiteral>&quot;45 Grove St&quot;</StringLiteral>
                            </Valuable>
                        </ArgumentValues>
                    </NewInstance>
                    <SetterBlock>
                        <SetterAssignment>
                            <Identifier>
                                <Identifier>value</Identifier>
                            </Identifier>
                            <Valuable>
                                <Number>
                                    <Number>15000</Number>
                                </Number>
                            </Valuable>
                        </SetterAssignment>
                        <SetterAssignment>
                            <Identifier>
                                <Identifier>size</Identifier>
                            </Identifier>
                            <Valuable>
                                <Number>
                                    <Number>2000</Number>
                                </Number>
                            </Valuable>
                        </SetterAssignment>
                    </SetterBlock>
                </ValuableChain>
                <ValuableSuffix></ValuableSuffix>
            </DirectedValuableChain>
        </Valuable>
    </DeclarationAssignment>
    <DeclarationAssignment>
        <Identifier>
            <Identifier>house</Identifier>
        </Identifier>
        <Valuable>
            <DirectedValuableChain>
                <ValuablePrefix></ValuablePrefix>
                <ValuableChain>
                    <NewInstance>
                        <Identifier>
                            <Identifier>House</Identifier>
                        </Identifier>
                        <ArgumentValues>
                            <Valuable>
                                <StringLiteral>&quot;40 Grove St&quot;</StringLiteral>
                            </Valuable>
                        </ArgumentValues>
                    </NewInstance>
                    <SetterBlock>
                        <SetterAssignment>
                            <Identifier>
                                <Identifier>value</Identifier>
                            </Identifier>
                            <Valuable>
                                <Number>
                                    <Number>20000</Number>
                                </Number>
                            </Valuable>
                        </SetterAssignment>
                        <SetterAssignment>
                            <Identifier>
                                <Identifier>size</Identifier>
                            </Identifier>
                            <Valuable>
                                <Number>
                                    <Number>1000</Number>
                                </Number>
                            </Valuable>
                        </SetterAssignment>
                        <SetterAssignment>
                            <Identifier>
                                <Identifier>floors</Identifier>
                            </Identifier>
                            <Valuable>
                                <Number>
                                    <Number>2</Number>
                                </Number>
                            </Valuable>
                        </SetterAssignment>
                    </SetterBlock>
                </ValuableChain>
                <ValuableSuffix></ValuableSuffix>
            </DirectedValuableChain>
        </Valuable>
    </DeclarationAssignment>
    <DeclarationAssignment>
        <Identifier>
            <Identifier>houseValue</Identifier>
        </Identifier>
        <Valuable>
            <DirectedValuableChain>
                <ValuablePrefix></ValuablePrefix>
                <ValuableChain>
                    <Identifier>
                        <Identifier>house</Identifier>
                    </Identifier>
                    <DotIdentifier>
                        <Identifier>
                            <Identifier>value</Identifier>
                        </Identifier>
                    </DotIdentifier>
                </ValuableChain>
                <ValuableSuffix></ValuableSuffix>
            </DirectedValuableChain>
        </Valuable>
    </DeclarationAssignment>
    <DeclarationAssignment>
        <Identifier>
            <Identifier>propertyValue</Identifier>
        </Identifier>
        <Valuable>
            <DirectedValuableChain>
                <ValuablePrefix></ValuablePrefix>
                <ValuableChain>
                    <Identifier>
                        <Identifier>property</Identifier>
                    </Identifier>
                    <DotIdentifier>
                        <Identifier>
                            <Identifier>value</Identifier>
                        </Identifier>
                    </DotIdentifier>
                </ValuableChain>
                <ValuableSuffix></ValuableSuffix>
            </DirectedValuableChain>
        </Valuable>
    </DeclarationAssignment>
    <DeclarationAssignment>
        <Identifier>
            <Identifier>type</Identifier>
        </Identifier>
        <Valuable>
            <DirectedValuableChain>
                <ValuablePrefix></ValuablePrefix>
                <ValuableChain>
                    <Identifier>
                        <Identifier>house</Identifier>
                    </Identifier>
                    <DotIdentifier>
                        <Identifier>
                            <Identifier>getType</Identifier>
                        </Identifier>
                    </DotIdentifier>
                    <ArgumentValues></ArgumentValues>
                </ValuableChain>
                <ValuableSuffix></ValuableSuffix>
            </DirectedValuableChain>
        </Valuable>
    </DeclarationAssignment>
    <DeclarationAssignment>
        <Identifier>
            <Identifier>sizeRatio</Identifier>
        </Identifier>
        <Valuable>
            <DirectedValuableChain>
                <ValuablePrefix></ValuablePrefix>
                <ValuableChain>
                    <Identifier>
                        <Identifier>house</Identifier>
                    </Identifier>
                    <DotIdentifier>
                        <Identifier>
                            <Identifier>getSizeRatio</Identifier>
                        </Identifier>
                    </DotIdentifier>
                    <ArgumentValues></ArgumentValues>
                </ValuableChain>
                <ValuableSuffix></ValuableSuffix>
            </DirectedValuableChain>
        </Valuable>
    </DeclarationAssignment>
    <DeclarationAssignment>
        <Identifier>
            <Identifier>valueRatio</Identifier>
        </Identifier>
        <Valuable>
            <DirectedValuableChain>
                <ValuablePrefix></ValuablePrefix>
                <ValuableChain>
                    <Identifier>
                        <Identifier>house</Identifier>
                    </Identifier>
                    <DotIdentifier>
                        <Identifier>
                            <Identifier>getValueRatio</Identifier>
                        </Identifier>
                    </DotIdentifier>
                    <ArgumentValues></ArgumentValues>
                </ValuableChain>
                <ValuableSuffix></ValuableSuffix>
            </DirectedValuableChain>
        </Valuable>
    </DeclarationAssignment>
</Script>
```

#### Optimized Instructions
```
CSP "$c_Property"
PHR 
G "Property"
MF 1, 3
GR 
PHR 
VR 16
RGM 
MP "address", 0
CGP "address"
PHR 
VR 0
RPM 
MP "value", 1
CGP "value"
PHR 
VR 0
RPM 
MP "size", 2
CGP "size"
PHR 
VR 0
RPM 
MP "", 3
CGP ""
MP "getValueRatio", 4
CGP "getValueRatio"
MP "getType", 5
CGP "getType"
GPR 3
PHR 
J 44
A false
LRR 1, false
RCP 1, csp, "address"
B 
GR 
PHR 
VR "address"
RVK false
PHR 
SPR 0
RR 
VGP 0, 1
VGP 0, 2
BE 
AE 
AR 29
RR 
PHR 
VR 20
RPM 
GPR 4
PHR 
J 71
A false
B 
CS 
CPR -1, 2, gpr, 2, vr, 0
RGreaterThan 
CPHR 
NC 66
B 
CPR -1, 2, gpr, 1, gpr, 2
RDivide 
RLR 1
AE 
BE 
CSE 66
VR 0
RLR 1
AE 
BE 
AE 
AR 52
RR 
PHR 
VR 20
RPM 
GPR 5
PHR 
J 84
A false
VR "Unknown"
RLR 1
AE 
AE 
AR 79
RR 
PHR 
VR 20
RPM 
CR 
GE 
RR 
PLR 
CSP "$c_House"
PHR 
G "House"
MF 1, 3
GR 
PHR 
VR 16
RGM 
MP "floors", 0
CGP "floors"
PHR 
VR 0
RPM 
MP "", 1
CGP ""
MP "getType", 2
CGP "getType"
MP "getSizeRatio", 3
CGP "getSizeRatio"
MP "getSize", 4
CGP "getSize"
GPR 1
PHR 
J 45
A false
LRR 1, false
RCP 1, csp, "address"
B 
GR 
PHR 
VR "address"
RVK false
PHR 
SPR 0
RR 
VR "value"
MPR 
PHR 
VR 0
RR 
VR "size"
MPR 
PHR 
VR 0
RR 
VGP 1, 0
BE 
AE 
AR 21
RR 
PHR 
VR 20
RPM 
GPR 2
PHR 
J 58
A false
VR "House"
RLR 1
AE 
AE 
AR 53
RR 
PHR 
VR 20
RPM 
GPR 3
PHR 
J 93
A false
B 
CS 
GPR 4
RCE 
LRR 1, false
PHR 
VR 0
RGreaterThan 
CPHR 
NC 88
B 
GPR 4
RCE 
LRR 1, false
PHR 
GPR 0
RDivide 
RLR 1
AE 
BE 
CSE 88
VR 0
RLR 1
AE 
BE 
AE 
AR 66
RR 
PHR 
VR 20
RPM 
GPR 4
PHR 
J 107
A false
VR "size"
MPR 
RLR 1
AE 
AE 
AR 101
RR 
PHR 
VR 20
RPM 
CR 
GE 
RR 
PHR 
SPR 1
MG 11
PLR 
CSP "property"
PHR 
SPR 2
CG modifiers
PHR 
VR 1
RGM 
CPHR 
PHR 
VR ""
RVK false
PHR 
VR "45 Grove St"
RLR 1
RCE 
LRR 1, false
PLR 
CPHR 
PHR 
VR "value"
RVK true
PHR 
VR 15000
RR 
PLR 
CPHR 
PHR 
VR "size"
RVK true
PHR 
VR 2000
RR 
PLR 
RR 
CSP "house"
PHR 
SPR 2
CG modifiers
PHR 
VR 1
RGM 
CPHR 
PHR 
VR ""
RVK false
PHR 
VR "40 Grove St"
RLR 1
RCE 
LRR 1, false
PLR 
CPHR 
PHR 
VR "value"
RVK true
PHR 
VR 20000
RR 
PLR 
CPHR 
PHR 
VR "size"
RVK true
PHR 
VR 1000
RR 
PLR 
CPHR 
PHR 
VR "floors"
RVK true
PHR 
VR 2
RR 
PLR 
RR 
CSP "houseValue"
CPR 0, 2, spr, 1, vr, "value"
RVK false
RR 
CSP "propertyValue"
CPR 0, 2, spr, 3, vr, "value"
RVK false
RR 
CSP "type"
CPR 0, 2, spr, 3, vr, "getType"
RVK false
RCE 
LRR 1, false
RR 
CSP "sizeRatio"
CPR 0, 2, spr, 4, vr, "getSizeRatio"
RVK false
RCE 
LRR 1, false
RR 
CSP "valueRatio"
CPR 0, 2, spr, 5, vr, "getValueRatio"
RVK false
RCE 
LRR 1, false
RR 
```