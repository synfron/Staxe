# Matcher

Context-free language parsing library

## Basic Usage

The code below shows basic usage of the Matcher libraries to generate an AST from a snippet of code.

```csharp
string code = "1 + 3 + 5 + 7 * 2 * 4 * 6 + 9 + 11 + 13 * 8 * 10 * 12";
string definitionJson = File.ReadAllText("./ExpressionDefinition.json");
LanguageMatcherDefinition definition = JsonConvert.DeserializeObject<LanguageMatcherDefinition>(definitionJson);
LanguageMatcher matcher = DefinitionConverter.Convert(definition);
ILanguageMatchEngine matchEngine = MatcherEngineGenerator.GenerateEngine(matcher);
IMatchData matchData = matchEngine.Match(code).MatchData;
```
Here's a breakdown of some of the classes used above:<br />
**[LanguageMatcherDefinition](https://github.com/synfron/Staxe/blob/master/src/Matcher.Interop.Model/LanguageMatcherDefinition.cs)** - Model class to allow easy deserialization of the grammar from JSON, XML, etc. [See Schema Details](https://synfron.github.io/Staxe/Matcher/Language-Matcher-Schema.html)<br />
**[LanguageMatcher](https://github.com/synfron/Staxe/blob/master/src/Matcher/Input/LanguageMatcher.cs)** - Internal form of the grammar used to generate a parser<br />
**[MatcherEngineGenerator](https://github.com/synfron/Staxe/blob/master/src/Matcher/MatcherEngineGenerator.cs)** - Dynamic parser generator<br />
**[ILanguageMatchEngine](https://github.com/synfron/Staxe/blob/master/src/Matcher/ILanguageMatchEngine.cs)** - Parser<br />
**[IMatchData](https://github.com/synfron/Staxe/blob/master/src/Matcher/Input/IMatcher.cs)** - Represents the Abstract Syntax Tree.

## Sub-Libraries

### Matcher.Interop.Model

Provides grammar model classes and a conversion utility to convert the models into a structure understood by the engine.

#### Patterns

Patterns are written using a syntax very similar to Regex, that supports the most popular operations, but also provides performance benefits.

| Feature          | Symbols         | Compare To (Regex)          | Example Usage | Example Match                      |
|------------------|-----------------|-----------------------------|---------------|------------------------------------|
| Or               | |               | |                           | a|b           | b                                  |
| Grouping         | ( )             | ( )                         | a(b|c)        | ab                                 |
| Character Range  | [ ]             | [ ] (Note: Only for ranges) | [a-f]-[1-5]   | c-2                                |
| Repeat Count     | { }             | { }                         | a{2} a{2, 4}  | aa aaa                             |
| Any Character    | .               | .                           | a.e           | are                                |
| Zero or More     | *               | *                           | a*b           | b aaaab                            |
| One or More      | +               | +                           | a+b           | aaab                               |
| Zero or One      | ?               | ?                           | a?b           | ab b                               |
| Not              | !               | (?! )                       | a!.c          | avc                                |
| Escape Character | \               | \                           | a\.e          | a.e                                |
| New Line         | \n              | \n                          | a\nb          | a... b                             |
| Carriage Return  | \r              | \r                          |               |                                    |
| Digit            | \d              | \d                          | a\d           | a2                                 |
| Letter           | \l              | \p{L}                       | a\l           | ab                                 |
| Tab              | \t              | \t                          | a\tb          | a    b                             |
| Whitespace       | \s              | \s                          | a\sb          | a b                                |
| Word Character   | \w              | \w                          | a\w\wb        | ae2b                               |
| Whole Word       | \` (prefix only) | \b \b                       | `var          | %var% (If already at char index 1) |
| Case Insensitive | ~ (prefix only) | (?i)                        | ~size         | SiZe                               |

### Matcher.Interop.Ebnf

Provides a utility to convert a grammar written in a form of BNF/EBNF to the internal representation of the grammar understood by the parser generator. There are many features that the parser generator supports that cannot be represented in BNF/EBNF, therefore using this is not recommended.

### Matcher.CLI

Program for generating a parser that can be outputed as a class or assembly using the command line.

```
Usage:
  ./matchercli [<json definition file path>[ [<output directory>[ <output type>]]] [options]
	options:
	  --def		The json definition file path. Default: Read from stdin
	  --out		The directory to output generated file. Not applicable for 'stdout' output type. Default: Current directory
	  --type	Output type (i.e. class, assembly, stdout). Default: class
```