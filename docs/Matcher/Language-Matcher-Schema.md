# Language Matcher Schema

```
#/languageMatcher
```

Language parser grammar

| Abstract            | Extensible | Status       | Identifiable | Custom Properties | Additional Properties | Defined In                                                                     |
| ------------------- | ---------- | ------------ | ------------ | ----------------- | --------------------- | ------------------------------------------------------------------------------ |
| Can be instantiated | No         | Experimental | No           | Forbidden         | Forbidden             | [LanguageMatcherDefinition.schema.json](LanguageMatcherDefinition.schema.json) |

# Language Matcher Properties

| Property                              | Type       | Required     | Nullable | Default  | Defined by                     |
| ------------------------------------- | ---------- | ------------ | -------- | -------- | ------------------------------ |
| [actions](#actions)                   | `array`    | Optional     | No       |          | Language Matcher (this schema) |
| [fragments](#fragments)               | `object[]` | **Required** | No       |          | Language Matcher (this schema) |
| [indexingMode](#indexingmode)         | `enum`     | Optional     | No       | `"Lazy"` | Language Matcher (this schema) |
| [logMatches](#logmatches)             | `boolean`  | **Required** | No       | `false`  | Language Matcher (this schema) |
| [name](#name)                         | `string`   | **Required** | No       |          | Language Matcher (this schema) |
| [patterns](#patterns)                 | `array`    | **Required** | No       |          | Language Matcher (this schema) |
| [startingFragment](#startingfragment) | `string`   | **Required** | No       |          | Language Matcher (this schema) |

## actions

`actions`

- is optional
- type: `array`
- defined in this schema

### actions Type

Array type: `array`

All items must be of the type:

**One** of the following _conditions_ need to be fulfilled.

#### Condition 1

`object` with following properties:

| Property            | Type           | Required     |
| ------------------- | -------------- | ------------ |
| `action`            | string         | **Required** |
| `firstVariableName` | string         | **Required** |
| `name`              | string         | **Required** |
| `source`            | string         | **Required** |
| `value`             | integer,string | Optional     |

#### action

Action type

`action`

- is **required**
- type: `enum`

The value of this property **must** be equal to one of the [known values below](#-known-values).

##### action Known Values

| Value            | Description |
| ---------------- | ----------- |
| `CreateVariable` |             |

#### firstVariableName

Name of variable to create

`firstVariableName`

- is **required**
- type: `string`

##### firstVariableName Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Var1](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Var1>)

##### firstVariableName Example

```json
Var1
```

#### name

Action reference name

`name`

- is **required**
- type: `string`

##### name Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [CreateVar](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=CreateVar>)

##### name Example

```json
CreateVar
```

#### source

Value to set in the variable

`source`

- is **required**
- type: `enum`

The value of this property **must** be equal to one of the [known values below](#-known-values).

##### source Known Values

| Value               | Description                                          |
| ------------------- | ---------------------------------------------------- |
| `Value`             | Use the value field                                  |
| `PartsCount`        | Count of fragment parts                              |
| `PartsXml`          | XML representation of the fragment parts             |
| `PartsLength`       | Length of all fragment parts together                |
| `StringPartsText`   | Count of fragment string based parts                 |
| `StringPartsLength` | Concatenated text of all fragment string based parts |

#### value

Value to set in the variable if this is set as the source

`value`

- is optional
- type: multiple

##### value Type

Unknown type `integer,string`.

```json
{
  "$id": "#/properties/actions/items/properties/createVariable/value",
  "type": ["integer", "string"],
  "description": "Value to set in the variable if this is set as the source",
  "examples": ["print", 20],
  "simpletype": "multiple"
}
```

##### value Examples

```json
print
```

```json
20
```

#### Condition 2

`object` with following properties:

| Property             | Type   | Required     |
| -------------------- | ------ | ------------ |
| `action`             | string | **Required** |
| `change`             | string | **Required** |
| `firstVariableName`  | string | **Required** |
| `name`               | string | **Required** |
| `secondVariableName` | string | **Required** |

#### action

Action type

`action`

- is **required**
- type: `enum`

The value of this property **must** be equal to one of the [known values below](#-known-values).

##### action Known Values

| Value            | Description |
| ---------------- | ----------- |
| `UpdateVariable` |             |

#### change

Operation to perform

`change`

- is **required**
- type: `enum`

The value of this property **must** be equal to one of the [known values below](#-known-values).

##### change Known Values

| Value      | Description |
| ---------- | ----------- |
| `Add`      |             |
| `Subtract` |             |
| `Concat`   |             |
| `Remove`   |             |
| `Set`      |             |

#### firstVariableName

Name of variable to update and perform a operation against

`firstVariableName`

- is **required**
- type: `string`

##### firstVariableName Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Var1](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Var1>)

##### firstVariableName Example

```json
Var1
```

#### name

Action reference name

`name`

- is **required**
- type: `string`

##### name Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [SetVar](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=SetVar>)

##### name Example

```json
SetVar
```

#### secondVariableName

Name of variable to use on the other end of the operation

`secondVariableName`

- is **required**
- type: `string`

##### secondVariableName Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Var2](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Var2>)

##### secondVariableName Example

```json
Var2
```

#### Condition 3

`object` with following properties:

| Property             | Type   | Required     |
| -------------------- | ------ | ------------ |
| `action`             | string | **Required** |
| `assert`             | string | **Required** |
| `firstVariableName`  | string | **Required** |
| `name`               | string | **Required** |
| `secondVariableName` | string | **Required** |

#### action

Action type

`action`

- is **required**
- type: `enum`

The value of this property **must** be equal to one of the [known values below](#-known-values).

##### action Known Values

| Value    | Description |
| -------- | ----------- |
| `Assert` |             |

#### assert

The condition to assert between the two variables

`assert`

- is **required**
- type: `enum`

The value of this property **must** be equal to one of the [known values below](#-known-values).

##### assert Known Values

| Value                 | Description                         |
| --------------------- | ----------------------------------- |
| `MatchesPattern`      | Matches a pattern string (e.g. \w+) |
| `Equals`              |                                     |
| `NotEquals`           |                                     |
| `GreaterThan`         |                                     |
| `GreaterThanOrEquals` |                                     |
| `LessThan`            |                                     |
| `LessThanOrEquals`    |                                     |
| `Contains`            |                                     |
| `StartsWith`          |                                     |
| `EndsWith`            |                                     |

#### firstVariableName

Name of the first variable in the condition

`firstVariableName`

- is **required**
- type: `string`

##### firstVariableName Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Var1](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Var1>)

##### firstVariableName Example

```json
Var1
```

#### name

Action reference name

`name`

- is **required**
- type: `string`

##### name Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [CheckVar](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=CheckVar>)

##### name Example

```json
CheckVar
```

#### secondVariableName

Name of the second variable in the condition

`secondVariableName`

- is **required**
- type: `string`

##### secondVariableName Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Var2](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Var2>)

##### secondVariableName Example

```json
Var2
```

## fragments

Text parsing rules

`fragments`

- is **required**
- type: `object[]`
- defined in this schema

### fragments Type

Array type: `object[]`

All items must be of the type: `object` with following properties:

| Property                 | Type    | Required     | Default      |
| ------------------------ | ------- | ------------ | ------------ |
| `actions`                | array   | Optional     |              |
| `boundsAsParts`          | boolean | Optional     | `false`      |
| `cacheable`              | boolean | Optional     | `false`      |
| `clearCache`             | boolean | Optional     | `false`      |
| `discardBounds`          | boolean | Optional     | `false`      |
| `end`                    | string  | Optional     |              |
| `expressionMode`         | string  | Optional     | `"None"`     |
| `expressionOrder`        | integer | Optional     |              |
| `fallThroughMode`        | string  | Optional     | `"None"`     |
| `isNoise`                | boolean | Optional     | `false`      |
| `minMatchedParts`        | integer | Optional     |              |
| `name`                   | string  | **Required** |              |
| `negate`                 | boolean | Optional     | `false`      |
| `parts`                  | array   | **Required** |              |
| `partsDelimiter`         | string  | Optional     |              |
| `partsDelimiterRequired` | boolean | Optional     | `true`       |
| `partsMatchMode`         | string  | Optional     | `"Multiple"` |
| `partsPadding`           | string  | Optional     |              |
| `start`                  | string  | Optional     |              |

#### actions

`actions`

- is optional
- type: `string[]`

##### actions Type

Array type: `string[]`

All items must be of the type: `string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [SetLength](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=SetLength>)
- test example: [SetExpected](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=SetExpected>)
- test example: [AssertLength](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=AssertLength>)

Name of actions to run if the fragment matched successfully

#### boundsAsParts

Add the matched start and end patterns as parts

`boundsAsParts`

- is optional
- type: `boolean`
- default: `false`

##### boundsAsParts Type

`boolean`

#### cacheable

Store the match result of this fragment to avoid the need to match against the same text segment again

`cacheable`

- is optional
- type: `boolean`
- default: `false`

##### cacheable Type

`boolean`

#### clearCache

Clear the cache if successfully matched

`clearCache`

- is optional
- type: `boolean`
- default: `false`

##### clearCache Type

`boolean`

#### discardBounds

Start and end patterns do not move the read cursor

`discardBounds`

- is optional
- type: `boolean`
- default: `false`

##### discardBounds Type

`boolean`

#### end

Name of the pattern that marks the end of the fragment. Match not added to the AST by default

`end`

- is optional
- type: `string`

##### end Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [CloseBracket](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=CloseBracket>)

##### end Example

```json
CloseBracket
```

#### expressionMode

Expression parser mode

`expressionMode`

- is optional
- type: `enum`
- default: `"None"`

The value of this property **must** be equal to one of the [known values below](#fragments-known-values).

##### expressionMode Known Values

| Value          | Description                                               |
| -------------- | --------------------------------------------------------- |
| `None`         | No expression parsing                                     |
| `BinaryTree`   | Expression tree to have a max of two child parts per node |
| `LikeNameTree` | Expression tree combines parts that are the same type     |

#### expressionOrder

Expression precedence within parent (lower number is higher precedence)

`expressionOrder`

- is optional
- type: `integer`

##### expressionOrder Type

`integer`

#### fallThroughMode

Add the children of this fragment instead of this fragment to the AST, and discard this fragment

`fallThroughMode`

- is optional
- type: `enum`
- default: `"None"`

The value of this property **must** be equal to one of the [known values below](#fragments-known-values).

##### fallThroughMode Known Values

| Value   | Description                                |
| ------- | ------------------------------------------ |
| `None`  | No fall through                            |
| `Empty` | Fall through if there are no children      |
| `One`   | Fall through if there is one or less child |
| `All`   | Always fall through                        |

#### isNoise

Do not add this part to the AST

`isNoise`

- is optional
- type: `boolean`
- default: `false`

##### isNoise Type

`boolean`

#### minMatchedParts

Minimum number of parts that must be matched

`minMatchedParts`

- is optional
- type: `integer`

##### minMatchedParts Type

`integer`

#### name

Fragment reference name

`name`

- is **required**
- type: `string`

##### name Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Script](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Script>)

##### name Example

```json
Script
```

#### negate

Negate the success or failure of the match

`negate`

- is optional
- type: `boolean`
- default: `false`

##### negate Type

`boolean`

#### parts

`parts`

- is **required**
- type: `string[]`

##### parts Type

Array type: `string[]`

All items must be of the type: `string`

All instances must conform to this regular expression

```regex
^(\w+|\[\w+\])$
```

- test example:
  [StringLiteral](<https://regexr.com/?expression=%5E(%5Cw%2B%7C%5C%5B%5Cw%2B%5C%5D)%24&text=StringLiteral>)
- test example: [[Script]](<https://regexr.com/?expression=%5E(%5Cw%2B%7C%5C%5B%5Cw%2B%5C%5D)%24&text=%5BScript%5D>)

Names of nested fragments (surrounded by '[' and ']') and/or patterns

#### partsDelimiter

Name of the pattern that separates parts. Match not added to the AST

`partsDelimiter`

- is optional
- type: `string`

##### partsDelimiter Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Whitespace](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Whitespace>)

##### partsDelimiter Example

```json
Whitespace
```

#### partsDelimiterRequired

Require that parts are separated by the delimiter

`partsDelimiterRequired`

- is optional
- type: `boolean`
- default: `true`

##### partsDelimiterRequired Type

`boolean`

#### partsMatchMode

Repetition constraint

`partsMatchMode`

- is optional
- type: `enum`
- default: `"Multiple"`

The value of this property **must** be equal to one of the [known values below](#fragments-known-values).

##### partsMatchMode Known Values

| Value      | Description                          |
| ---------- | ------------------------------------ |
| `Multiple` | Match any of the parts (one or more) |
| `Ordered`  | Match the parts in the order given   |
| `One`      | Match one of the parts               |

#### partsPadding

Name of the pattern that bounds the parts. Match not added to the AST

`partsPadding`

- is optional
- type: `string`

##### partsPadding Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Whitespace](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Whitespace>)

##### partsPadding Example

```json
Whitespace
```

#### start

Name of the pattern that marks the beginning of the fragment. Match not added to the AST by default

`start`

- is optional
- type: `string`

##### start Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [OpenBracket](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=OpenBracket>)

##### start Example

```json
OpenBracket
```

Text parsing rule

## indexingMode

Token indexing configuration (lexing)

`indexingMode`

- is optional
- type: `enum`
- default: `"Lazy"`
- defined in this schema

The value of this property **must** be equal to one of the [known values below](#indexingmode-known-values).

### indexingMode Known Values

| Value   | Description                    |
| ------- | ------------------------------ |
| `None`  | Disable lexing                 |
| `Lazy`  | Generate tokens on demand      |
| `Eager` | Generate tokens before parsing |

### indexingMode Example

```json
"Eager"
```

## logMatches

Log fragment and pattern match successes and failures while parsing text

`logMatches`

- is **required**
- type: `boolean`
- default: `false`
- defined in this schema

### logMatches Type

`boolean`

## name

The name of the language

`name`

- is **required**
- type: `string`
- defined in this schema

### name Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Lua](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Lua>)

### name Example

```json
"Lua"
```

## patterns

`patterns`

- is **required**
- type: `array`
- defined in this schema

### patterns Type

Array type: `array`

All items must be of the type:

**One** of the following _conditions_ need to be fulfilled.

#### Condition 1

`object` with following properties:

| Property      | Type    | Required     | Default |
| ------------- | ------- | ------------ | ------- |
| `isAuxiliary` | boolean | Optional     | `false` |
| `isNoise`     | boolean | Optional     | `false` |
| `name`        | string  | **Required** |         |
| `pattern`     | string  | **Required** |         |

#### isAuxiliary

Pattern won't be used in pre-parse lexing but may be referenced by a fragment

`isAuxiliary`

- is optional
- type: `boolean`
- default: `false`

##### isAuxiliary Type

`boolean`

#### isNoise

Text to be ignored while parsing

`isNoise`

- is optional
- type: `boolean`
- default: `false`

##### isNoise Type

`boolean`

#### name

Pattern reference name

`name`

- is **required**
- type: `string`

##### name Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [StringLiteral](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=StringLiteral>)

##### name Example

```json
StringLiteral
```

#### pattern

Pattern used to parse tokens from text

`pattern`

- is **required**
- type: `string`

##### pattern Type

`string`

All instances must conform to this regular expression

```regex
^(.*)$
```

- test example: [\s+](<https://regexr.com/?expression=%5E(.*)%24&text=%5Cs%2B>)

##### pattern Example

```json
\s+
```

#### Condition 2

`object` with following properties:

| Property      | Type    | Required     | Default |
| ------------- | ------- | ------------ | ------- |
| `isAuxiliary` | boolean | Optional     | `false` |
| `isNoise`     | boolean | Optional     | `false` |
| `name`        | string  | **Required** |         |
| `pattern`     | string  | Optional     |         |

#### isAuxiliary

Pattern won't be used in pre-parse lexing but may be referenced by a fragment

`isAuxiliary`

- is optional
- type: `boolean`
- default: `false`

##### isAuxiliary Type

`boolean`

#### isNoise

Text to be ignored while parsing

`isNoise`

- is optional
- type: `boolean`
- default: `false`

##### isNoise Type

`boolean`

#### name

Pattern reference name

`name`

- is **required**
- type: `string`

##### name Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [StringLiteral](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=StringLiteral>)

##### name Example

```json
StringLiteral
```

#### pattern

Fragment to run during pre-parse lexing

`pattern`

- is optional
- type: `string`

##### pattern Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [SpecialSection](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=SpecialSection>)

##### pattern Example

```json
SpecialSection
```

## startingFragment

The fragment to use to begin parsing

`startingFragment`

- is **required**
- type: `string`
- defined in this schema

### startingFragment Type

`string`

All instances must conform to this regular expression

```regex
^(\w+)$
```

- test example: [Script](<https://regexr.com/?expression=%5E(%5Cw%2B)%24&text=Script>)

### startingFragment Example

```json
"Script"
```
