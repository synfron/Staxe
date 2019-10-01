using Synfron.Staxe.Matcher.Data;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Input.Patterns;
using Synfron.Staxe.Shared.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Synfron.Staxe.Matcher.Interop.Bnf
{
	public class EbnfConverter
	{
		private readonly EbnfMatchEngine _ebnfMatchEngine = new EbnfMatchEngine();
		private readonly CharacterClassMatchEngine _characterClassEngine = new CharacterClassMatchEngine();
		private LanguageMatcher _languageMatcher;
		private Dictionary<string, PatternMatcher> _patternMatcherMap;
		private Dictionary<string, ValueTuple<FragmentMatcher, FragmentMatchData>> _fragmentMatcherMap;
		private string _ebnfText = null;

		public List<FragmentMatcher> DefaultFragments
		{
			get;
			set;
		} = new List<FragmentMatcher>();

		public LanguageMatcher Convert(string name, string ebnfText)
		{
			_ebnfText = ebnfText;
			_patternMatcherMap = new Dictionary<string, PatternMatcher>(StringComparer.Ordinal);
			_fragmentMatcherMap = new Dictionary<string, ValueTuple<FragmentMatcher, FragmentMatchData>>(StringComparer.Ordinal);
			_languageMatcher = new LanguageMatcher()
			{
				Name = name,
				Patterns = new List<PatternMatcher>(),
				Fragments = new List<FragmentMatcher>()

			};

			MatcherResult matcherResult = _ebnfMatchEngine.Match(ebnfText);

			if (!matcherResult.Success)
			{
				throw new LanaguageSyntaxException($"Unsupported EBNF syntax", matcherResult.FailureIndex.Value);
			}

			foreach (FragmentMatcher defaultFragment in DefaultFragments)
			{
				AddFragmentMatcherAndParts(defaultFragment);
			}

			AddRules((FragmentMatchData)matcherResult.MatchData);

			_languageMatcher.IndexingMode = IndexingMode.Lazy;
			_languageMatcher.StartingFragment = _languageMatcher.Fragments.FirstOrDefault();

			return _languageMatcher;
		}

		private void AddRules(FragmentMatchData matchData)
		{
			foreach (FragmentMatchData fragmentData in matchData.Parts.Cast<FragmentMatchData>())
			{
				string name = ((FragmentMatchData)fragmentData.Parts[0]).Parts[0].ToString();
				FragmentMatcher fragment = new FragmentMatcher(_fragmentMatcherMap.Count + 1, name);
				_fragmentMatcherMap[name] = (fragment, fragmentData);
				_languageMatcher.Fragments.Add(fragment);
			}

			foreach (KeyValuePair<string, (FragmentMatcher, FragmentMatchData)> fragmentPair in _fragmentMatcherMap.ToArray())
			{
				AddRule(fragmentPair);
			}
		}

		private void AddRule(KeyValuePair<string, (FragmentMatcher, FragmentMatchData)> ruleFragmentPair)
		{
			FragmentMatcher rule = ruleFragmentPair.Value.Item1;
			FragmentMatchData ruleData = ruleFragmentPair.Value.Item2;
			switch (GetMatcher(rule.Name, 1, ruleData.Parts[1]))
			{
				case FragmentMatcher fragmentPart:
					{
						rule.Parts = fragmentPart.Parts;
						rule.PartsMatchMode = fragmentPart.PartsMatchMode;
						rule.MinMatchedParts = fragmentPart.MinMatchedParts;
						break;
					}
				case PatternMatcher patternPart:
					{
						rule.Parts = new IMatcher[] { patternPart };
						rule.PartsMatchMode = MatchMode.One;
						break;
					}
			}
		}

		private IMatcher GetMatcher(string baseName, int id, IMatchData matchData)
		{
			switch (matchData.Name)
			{
				case "RepetitionGroup":
				case "ZeroOrMoreItem":
					return GetRepetitionGroup(baseName, id, (FragmentMatchData)matchData);
				case "OneOrMoreItem":
					return GetOneOrMoreItem(baseName, id, (FragmentMatchData)matchData);
				case "OptionalGroup":
					return GetOptionalGroup(baseName, id, (FragmentMatchData)matchData);
				case "ZeroOrOneItem":
					return GetZeroOrOneItem(baseName, id, (FragmentMatchData)matchData);
				case "Expression":
					return GetExpression(baseName, id, (FragmentMatchData)matchData);
				case "OrSuffix":
					return GetOr(baseName, id, (FragmentMatchData)matchData);
				case "CommaSuffix":
					return GetOrdered(baseName, id, (FragmentMatchData)matchData);
				case "RepetitionSuffix":
					return GetCounted(baseName, id, (FragmentMatchData)matchData);
				case "RuleName":
					return GetRule((FragmentMatchData)matchData);
				case "DoubleQuoteLiteral":
				case "SingleQuoteLiteral":
					return GetLiteral((StringMatchData)matchData);
				case "Item":
					return GetItem(baseName, id, (FragmentMatchData)matchData);
				case "SpecialGroup":
					return GetSpecialGroup((StringMatchData)matchData);
				case "HexChar":
					return GetHexChar((StringMatchData)matchData);
				default:
					throw new LanguageConstraintException($"Unsupported fragment {matchData.Name}", matchData.StartIndex);
			}
		}

		private IMatcher GetOneOrMoreItem(string baseName, int id, FragmentMatchData matchData)
		{
			string name = $"{baseName}Multiple{id}";

			return AddFragmentMatcher(new FragmentMatcher(
				id: _fragmentMatcherMap.Count + 1,
				name: name,
				partsMatchMode: MatchMode.Multiple,
				parts: new List<IMatcher> { GetMatcher(name, 1, matchData.Parts[0]) },
				fallThrough: true
			));
		}

		private IMatcher GetZeroOrOneItem(string baseName, int id, FragmentMatchData matchData)
		{
			string name = $"{baseName}Optional{id}";

			return AddFragmentMatcher(new FragmentMatcher(
				id: _fragmentMatcherMap.Count + 1,
				name: name,
				partsMatchMode: MatchMode.One,
				parts: new List<IMatcher> { GetMatcher(name, 1, matchData.Parts[0]) },
				minMatchedParts: 0,
				fallThrough: true
			));
		}

		private IMatcher GetHexChar(StringMatchData matchData)
		{
			string literal = ((char)System.Convert.ToUInt32(matchData.ToString().Substring(2), 16)).ToString();
			return GetPatternMatcher(literal, PatternReader.Escape(literal));
		}

		private IMatcher GetSpecialGroup(StringMatchData matchData)
		{
			return GetSpecialGroupAsPatternMatcher(matchData.ToString(), matchData.StartIndex);
		}

		private IMatcher GetOptionalGroup(string baseName, int id, FragmentMatchData matchData)
		{
			try
			{
				return GetZeroOrOneItem(baseName, id, matchData);
			}
			catch (KeyNotFoundException)
			{
				return GetSpecialGroupAsPatternMatcher($"{_ebnfText.Substring(matchData.StartIndex, matchData.Length)}", matchData.StartIndex);
			}
		}

		private IMatcher GetSpecialGroupAsPatternMatcher(string specialGroup, int position)
		{
			MatcherResult result = _characterClassEngine.Match(specialGroup);
			if (!result.Success)
			{
				throw new LanaguageSyntaxException($"Unsupported EBNF syntax", position);
			}

			string pattern = GetSpecialGroupItem(result.MatchData);

			return GetPatternMatcher(pattern, pattern);
		}

		private string GetSpecialGroupItem(IMatchData matchData)
		{
			switch (matchData.Name)
			{
				case "NotCharacterClassBody":
					return GetNotCharacterClassBody((FragmentMatchData)matchData);
				case "CharacterClassBody":
					return GetCharacterClassBody((FragmentMatchData)matchData);
				case "HexChar":
					return GetHexCharString((StringMatchData)matchData);
				case "HexRange":
				case "CharacterRange":
					return GetCharacterRange((FragmentMatchData)matchData);
				case "AnyChar":
					return GetAnyChar((StringMatchData)matchData);
				default:
					throw new LanguageConstraintException($"Unsupported character class {matchData.Name}", matchData.StartIndex);
			}
		}

		private string GetCharacterRange(FragmentMatchData matchData)
		{
			return $"[{GetSpecialGroupItem(matchData.Parts[0])}-{GetSpecialGroupItem(matchData.Parts[1])}]";
		}

		private string GetAnyChar(StringMatchData matchData)
		{
			return matchData.ToString();
		}

		private string GetHexCharString(StringMatchData matchData)
		{
			string hexValue = matchData.ToString().Substring(2);
			return ((char)int.Parse(hexValue, NumberStyles.HexNumber)).ToString();
		}

		private string GetNotCharacterClassBody(FragmentMatchData matchData)
		{
			return $"({GetSpecialGroupItem(matchData)})!.";
		}

		private string GetCharacterClassBody(FragmentMatchData matchData)
		{
			return string.Join(" | ", matchData.Parts.Select(GetSpecialGroupItem));
		}

		private IMatcher GetItem(string baseName, int id, FragmentMatchData matchData)
		{
			return GetMatcher(baseName, id, matchData.Parts[0]);
		}

		private IMatcher GetRepetitionGroup(string baseName, int id, FragmentMatchData matchData)
		{
			string name = $"{baseName}Multiple{id}";

			return AddFragmentMatcher(new FragmentMatcher(
				id: _fragmentMatcherMap.Count + 1,
				name: name,
				partsMatchMode: MatchMode.Multiple,
				parts: new List<IMatcher> { GetMatcher(name, 1, matchData.Parts[0]) },
				minMatchedParts: 0,
				fallThrough: true
			));
		}

		private IMatcher GetExpression(string baseName, int id, FragmentMatchData matchData)
		{
			return GetMatcher(baseName, id, matchData.Parts[0]);
		}

		private IMatcher GetOrdered(string baseName, int id, FragmentMatchData matchData)
		{
			string name = $"{baseName}Ordered{id}";
			IList<IMatcher> parts = GetSuffixParts(name, matchData);

			return AddFragmentMatcher(new FragmentMatcher(
				id: _fragmentMatcherMap.Count + 1,
				name: name,
				partsMatchMode: MatchMode.Ordered,
				parts: parts,
				fallThrough: true
			));
		}

		private IMatcher GetCounted(string baseName, int id, FragmentMatchData matchData)
		{
			if (matchData.Parts.Count > 3)
			{
				throw new LanguageConstraintException($"Unsupported syntax", matchData.Parts[3].StartIndex);
			}

			int count = int.Parse(((FragmentMatchData)matchData.Parts[0]).Parts[0].ToString());

			string name = $"{baseName}Counted{id}";
			IList<IMatcher> parts = Enumerable.Repeat(GetMatcher(name, 1, matchData.Parts[2]), count).ToList();


			return AddFragmentMatcher(new FragmentMatcher(
				id: _fragmentMatcherMap.Count + 1,
				name: name,
				partsMatchMode: MatchMode.Ordered,
				parts: parts,
				fallThrough: true
			));
		}

		private IMatcher GetRule(FragmentMatchData matchData)
		{
			return _fragmentMatcherMap[matchData.Parts[0].ToString()].Item1;
		}

		private IMatcher GetLiteral(StringMatchData matchData)
		{
			string literal = GetInnerText(matchData.ToString());
			return GetPatternMatcher(literal, PatternReader.EscapeNonWhitespace(literal));
		}

		private FragmentMatcher GetOr(string baseName, int id, FragmentMatchData matchData)
		{
			string name = $"{baseName}Or{id}";
			IList<IMatcher> parts = GetSuffixParts(name, matchData);

			return AddFragmentMatcher(new FragmentMatcher(
				id: _fragmentMatcherMap.Count + 1,
				name: name,
				partsMatchMode: MatchMode.One,
				parts: parts,
				fallThrough: true
			));
		}

		private IList<IMatcher> GetSuffixParts(string baseName, FragmentMatchData matchData)
		{
			List<IMatcher> parts = new List<IMatcher>();
			for (int i = 0; i < matchData.Parts.Count; i++)
			{
				if (i % 2 == 0)
				{
					parts.Add(GetMatcher(baseName, i / 2 + 1, matchData.Parts[i]));
				}
			}
			return parts;
		}

		private string GetInnerText(string text)
		{
			return text.Substring(1, text.Length - 2);
		}

		private PatternMatcher GetPatternMatcher(string name, string pattern)
		{
			if (!_patternMatcherMap.TryGetValue(name, out PatternMatcher patternMatcher))
			{
				patternMatcher = PatternReader.LazyParse(_patternMatcherMap.Count + 1, name, pattern);
				_patternMatcherMap[name] = patternMatcher;
				_languageMatcher.Patterns.Add(patternMatcher);
			}
			return patternMatcher;
		}

		private void AddPatternMatcher(PatternMatcher patternMatcher)
		{
			if (!_patternMatcherMap.ContainsKey(patternMatcher.Name))
			{
				_patternMatcherMap[patternMatcher.Name] = patternMatcher;
				_languageMatcher.Patterns.Add(patternMatcher);
			}
		}

		private FragmentMatcher AddFragmentMatcherAndParts(FragmentMatcher fragmentMatcher)
		{
			if (!_fragmentMatcherMap.ContainsKey(fragmentMatcher.Name))
			{
				_fragmentMatcherMap[fragmentMatcher.Name] = (fragmentMatcher, null);
				_languageMatcher.Fragments.Add(fragmentMatcher);

				foreach (IMatcher matcher in fragmentMatcher.Parts)
				{
					switch (matcher)
					{
						case FragmentMatcher fragmentPart:
							AddFragmentMatcherAndParts(fragmentPart);
							break;
						case PatternMatcher patternPart:
							AddPatternMatcher(patternPart);
							break;
					}
				}
			}
			return fragmentMatcher;
		}

		private FragmentMatcher AddFragmentMatcher(FragmentMatcher fragmentMatcher)
		{
			if (!_fragmentMatcherMap.ContainsKey(fragmentMatcher.Name))
			{
				_fragmentMatcherMap[fragmentMatcher.Name] = (fragmentMatcher, null);
				_languageMatcher.Fragments.Add(fragmentMatcher);
			}
			return fragmentMatcher;
		}
	}
}
