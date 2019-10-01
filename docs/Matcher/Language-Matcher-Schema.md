# Language Matcher Schema

Define a language grammar

| Abstract            | Extensible | Status       | Identifiable | Custom Properties | Additional Properties | Defined In                                                                     |
| ------------------- | ---------- | ------------ | ------------ | ----------------- | --------------------- | ------------------------------------------------------------------------------ |
| Can be instantiated | No         | Experimental | No           | Forbidden         | Forbidden             | [LanguageMatcherDefinition.schema.json](LanguageMatcherDefinition.schema.json) |

# Language Matcher Properties

| Property                              | Type       | Required     | Nullable | Default  | Defined by                     |
| ------------------------------------- | ---------- | ------------ | -------- | -------- | ------------------------------ |
| [fragments](#fragments)               | `object[]` | **Required** | No       |          | Language Matcher (this schema) |
| [indexingMode](#indexingmode)         | `enum`     | Optional     | No       | `"Lazy"` | Language Matcher (this schema) |
| [logMatches](#logmatches)             | `boolean`  | **Required** | No       | `false`  | Language Matcher (this schema) |
| [name](#name)                         | `string`   | **Required** | No       |          | Language Matcher (this schema) |
| [patterns](#patterns)                 | `object[]` | **Required** | No       |          | Language Matcher (this schema) |
| [startingFragment](#startingfragment) | `string`   | **Required** | No       |          | Language Matcher (this schema) |

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
| `boundsAsParts`          | boolean | Optional     | `false`      |
| `cacheable`              | boolean | Optional     | `false`      |
| `clearCache`             | boolean | Optional     | `false`      |
| `discardBounds`          | boolean | Optional     | `false`      |
| `end`                    | string  | Optional     |              |
| `expressionMode`         | string  | Optional     | `"None"`     |
| `expressionOrder`        | integer | Optional     |              |
| `fallThrough`            | boolean | Optional     | `false`      |
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

Experssion parser mode

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

Expression precendence within parent (lower number is higher precendence)

`expressionOrder`

- is optional
- type: `integer`

##### expressionOrder Type

`integer`

#### fallThrough

Add the children of this fragment instead of this fragment to the AST

`fallThrough`

- is optional
- type: `boolean`
- default: `false`

##### fallThrough Type

`boolean`

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

Names of nested fragments (surrounded by '[' and ']' and/or patterns

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

Parts treatment

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

Lexer configuration

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
- type: `object[]`
- defined in this schema

### patterns Type

Array type: `object[]`

All items must be of the type: `object` with following properties:

| Property   | Type    | Required     | Default |
| ---------- | ------- | ------------ | ------- |
| `isNoise`  | boolean | Optional     | `false` |
| `mergable` | boolean | Optional     | `false` |
| `name`     | string  | **Required** |         |
| `pattern`  | string  | **Required** |         |

#### isNoise

Text to be ignored while parsing

`isNoise`

- is optional
- type: `boolean`
- default: `false`

##### isNoise Type

`boolean`

#### mergable

Tokens surrounding ignored noise can be removed

`mergable`

- is optional
- type: `boolean`
- default: `false`

##### mergable Type

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

Patterns for generating tokens

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


Generated using [jsonschema2md](<https://github.com/adobe/jsonschema2md>)
