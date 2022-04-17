using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Matcher.Input.Actions;
using Synfron.Staxe.Matcher.Input.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Synfron.Staxe.Matcher.Interop.Model
{
	public static class DefinitionConverter
	{

		public static LanguageMatcher Convert(LanguageMatcherDefinition languageMatcherModel)
        {

            Dictionary<string, FragmentMatcher> fragmentMatcherMap = new Dictionary<string, FragmentMatcher>(StringComparer.Ordinal);
            Dictionary<string, PatternMatcher> patternMatcherMap = new Dictionary<string, PatternMatcher>(StringComparer.Ordinal);
            Dictionary<string, MatcherAction> matcherActionMap = new Dictionary<string, MatcherAction>(StringComparer.Ordinal);

            List<PatternMatcherDefinition> fragmentPatternModels = new List<PatternMatcherDefinition>();

            IList<string> blobs = ProcessMatcherActions(languageMatcherModel, matcherActionMap);
            List<PatternMatcher> patternMatchers = ProcessPatternMatchers(languageMatcherModel, patternMatcherMap, fragmentPatternModels);
            List<FragmentMatcher> fragmentMatchers = ProcessFragmentMatchers(languageMatcherModel, fragmentMatcherMap, patternMatcherMap, matcherActionMap);

            foreach (PatternMatcherDefinition model in fragmentPatternModels)
            {
                FragmentPatternMatcher fragmentPatternMatcher = (FragmentPatternMatcher)patternMatcherMap[model.Name];
                fragmentPatternMatcher.Fragment = fragmentMatcherMap[model.Fragment];
            }

            return new LanguageMatcher
            {
                Name = languageMatcherModel.Name,
                Fragments = fragmentMatchers.ToArray(),
                Patterns = patternMatchers.ToArray(),
                StartingFragment = fragmentMatcherMap[languageMatcherModel.StartingFragment],
                LogMatches = languageMatcherModel.LogMatches,
                IndexingMode = languageMatcherModel.IndexingMode,
                Blobs = blobs.ToArray()
            };
        }

        private static IList<string> ProcessMatcherActions(LanguageMatcherDefinition languageMatcherModel, Dictionary<string, MatcherAction> matcherActionMap)
        {
            int blobId = 0;
            List<string> blobs = new List<string>();
            if (languageMatcherModel.Actions != null)
            {
                Dictionary<string, CreateVariableMatcherAction> createVariableMap = new Dictionary<string, CreateVariableMatcherAction>();
                foreach (MatcherActionDefinition actionDefinition in languageMatcherModel.Actions)
                {
                    if (actionDefinition.Action == MatcherActionType.CreateVariable)
                    {
                        CreateVariableMatcherAction action = new CreateVariableMatcherAction
                        {
                            Name = actionDefinition.Name,
                            BlobId = blobId++,
                            Source = actionDefinition.Source ?? VariableValueSource.Value,
                            Value = actionDefinition.Value ?? string.Empty
                        };
                        blobs.Add(actionDefinition.FirstVariableName);
                        createVariableMap.Add(actionDefinition.FirstVariableName, action);
                        matcherActionMap.Add(actionDefinition.Name, action);
                    }
                }

                foreach (MatcherActionDefinition actionDefinition in languageMatcherModel.Actions)
                {
                    MatcherAction action = null;
                    switch (actionDefinition.Action)
                    {
                        case MatcherActionType.Assert:
                            {
                                action = new AssertMatcherAction
                                {
                                    Name = actionDefinition.Name,
                                    FirstBlobId = createVariableMap[actionDefinition.FirstVariableName].BlobId,
                                    SecondBlobId = createVariableMap[actionDefinition.SecondVariableName].BlobId,
                                    Assert = actionDefinition.Assert
                                };
                                matcherActionMap.Add(actionDefinition.Name, action);
                                break;
                            }
                        case MatcherActionType.UpdateVariable:
                            {
                                action = new UpdateVariableMatcherAction
                                {
                                    Name = actionDefinition.Name,
                                    Change = actionDefinition.Change ?? VariableUpdateAction.Set,
                                    TargetBlobId = createVariableMap[actionDefinition.FirstVariableName].BlobId,
                                    SourceBlobId = createVariableMap[actionDefinition.SecondVariableName].BlobId
                                };
                                matcherActionMap.Add(actionDefinition.Name, action);
                                break;
                            }
                        case MatcherActionType.CreateVariable:
                            break;
                        default:
                            throw new InvalidOperationException($"Action type {actionDefinition.Action} is not supported");
                    }
                }
            }
            return blobs;
        }

        private static List<PatternMatcher> ProcessPatternMatchers(LanguageMatcherDefinition languageMatcherModel, Dictionary<string, PatternMatcher> patternMatcherMap, List<PatternMatcherDefinition> fragmentPatternModels)
        {
            List<PatternMatcher> patternMatchers = new List<PatternMatcher>(languageMatcherModel.Patterns?.Count ?? 0);
            if (languageMatcherModel.Patterns != null)
            {
                bool isEager = languageMatcherModel.IndexingMode == IndexingMode.Eager;
                for (int patternIndex = 0; patternIndex < languageMatcherModel.Patterns.Count; patternIndex++)
                {
                    PatternMatcherDefinition model = languageMatcherModel.Patterns[patternIndex];
                    if (string.IsNullOrEmpty(model.Fragment))
                    {
                        PatternMatcher patternMatcher = isEager ? PatternReader.Parse(patternIndex + 1, model.Name, model.Pattern) : PatternReader.LazyParse(patternIndex + 1, model.Name, model.Pattern);
                        patternMatcher.IsNoise = model.IsNoise;
                        patternMatcherMap.Add(patternMatcher.Name, patternMatcher);
                        if (!model.IsAuxiliary)
                        {
                            patternMatchers.Add(patternMatcher);
                        }
                    }
                    else
                    {
                        FragmentPatternMatcher patternMatcher = new FragmentPatternMatcher()
                        {
                            Name = model.Name,
                            IsNoise = model.IsNoise

                        };
                        patternMatcherMap.Add(patternMatcher.Name, patternMatcher);
                        fragmentPatternModels.Add(model);
                        if (!model.IsAuxiliary)
                        {
                            patternMatchers.Add(patternMatcher);
                        }
                    }
                }
            }
            return patternMatchers;
        }

        private static List<FragmentMatcher> ProcessFragmentMatchers(LanguageMatcherDefinition languageMatcherModel, Dictionary<string, FragmentMatcher> fragmentMatcherMap, Dictionary<string, PatternMatcher> patternMatcherMap, Dictionary<string, MatcherAction> matcherActionMap)
        {
            List<FragmentMatcher> fragmentMatchers = new List<FragmentMatcher>(languageMatcherModel.Fragments?.Count ?? 0);
            HashSet<string> actionsReferenced = new HashSet<string>();
            if (languageMatcherModel.Fragments != null)
            {
                for (int matcherIndex = 0; matcherIndex < languageMatcherModel.Fragments.Count; matcherIndex++)
                {
                    FragmentMatcherDefinition model = languageMatcherModel.Fragments[matcherIndex];
                    FragmentMatcher fragmentMatcher = new FragmentMatcher
                    (
                        id: matcherIndex + 1,
                        name: model.Name,
                        parts: new IMatcher[0],
                        fallThroughMode: model.FallThroughMode,
                        isNoise: model.IsNoise,
                        partsDelimiterRequired: model.PartsDelimiterRequired,
                        partsMatchMode: model.PartsMatchMode,
                        minMatchedParts: model.MinMatchedParts,
                        cacheable: model.Cacheable,
                        clearCache: model.ClearCache,
                        expressionMode: model.ExpressionMode,
                        expressionOrder: model.ExpressionOrder,
                        boundsAsParts: model.BoundsAsParts,
                        discardBounds: model.DiscardBounds,
                        end: model.End != null ? patternMatcherMap[model.End] : null,
                        partsDelimiter: model.PartsDelimiter != null ? patternMatcherMap[model.PartsDelimiter] : null,
                        partsPadding: model.PartsPadding != null ? patternMatcherMap[model.PartsPadding] : null,
                        start: model.Start != null ? patternMatcherMap[model.Start] : null,
                        negate: model.Negate,
                        actions: model.Actions != null && model.Actions.Count > 0 ?
                            model.Actions.Select(actionName => matcherActionMap[actionName]).ToArray() : null
                    );
                    fragmentMatcherMap.Add(fragmentMatcher.Name, fragmentMatcher);
                    fragmentMatchers.Add(fragmentMatcher);
                    if (fragmentMatcher.Actions != null)
                    {
                        foreach (MatcherAction action in fragmentMatcher.Actions)
                        {
                            actionsReferenced.Add(action.Name);
                        }
                    }
                };

                foreach (FragmentMatcherDefinition model in languageMatcherModel.Fragments)
                {
                    FragmentMatcher fragmentMatcher = fragmentMatcherMap[model.Name];
                    fragmentMatcher.Parts = model.Parts.Select(name => GetPartMatcher(patternMatcherMap, fragmentMatcherMap, name)).ToArray();
                }

                foreach (CreateVariableMatcherAction matcherAction in matcherActionMap.Values.OfType<CreateVariableMatcherAction>())
                {
                    if (!actionsReferenced.Contains(matcherAction.Name))
                    {
                        throw new InvalidOperationException($"CreateVariable Action {matcherAction.Name} is not referenced by a Fragment");
                    }
                }
            }
            return fragmentMatchers;
        }

        private static IMatcher GetPartMatcher(Dictionary<string, PatternMatcher> patternMatcherMap, Dictionary<string, FragmentMatcher> fragmentMatcherMap, string name)
		{
            return name.StartsWith("[") ? fragmentMatcherMap[name.Substring(1, name.Length - 2)] : (IMatcher)patternMatcherMap[name];
        }

        public static LanguageMatcherDefinition Convert(LanguageMatcher languageMatcher)
		{
            Dictionary<string, MatcherActionDefinition> matcherActions = new Dictionary<string, MatcherActionDefinition>();

			HashSet<string> convertedPatterns = new HashSet<string>();
			List<PatternMatcherDefinition> patterns = new List<PatternMatcherDefinition>(languageMatcher.Patterns.Select(pattern => ConvertPattern(pattern, false)));
			List<FragmentMatcherDefinition> fragments = new List<FragmentMatcherDefinition>(languageMatcher.Fragments.Select(ConvertFragment));

			LanguageMatcherDefinition matcherDefinition = new LanguageMatcherDefinition
			{
				Name = languageMatcher.Name,
				LogMatches = languageMatcher.LogMatches,
				IndexingMode = languageMatcher.IndexingMode,
				StartingFragment = languageMatcher.StartingFragment.Name,
				Patterns = patterns,
				Fragments = fragments,
                Actions = matcherActions.Count > 0 ? matcherActions.Values.ToList() : null
			};

			return matcherDefinition;

			PatternMatcherDefinition ConvertPattern(PatternMatcher patternMatcher, bool isAuxilary)
			{
				convertedPatterns.Add(patternMatcher.Name);
				return patternMatcher is FragmentPatternMatcher fragmentPatternMatcher ?
					new PatternMatcherDefinition
					{
						Name = patternMatcher.Name,
						Fragment = fragmentPatternMatcher.Fragment.Name,
                        IsNoise = patternMatcher.IsNoise
                    } :
					new PatternMatcherDefinition
					{
                        IsAuxiliary = isAuxilary,
						IsNoise = patternMatcher.IsNoise,
						Name = patternMatcher.Name,
						Pattern = patternMatcher is GroupPatternMatcher groupPatternMatcher ? groupPatternMatcher.ToString(true) : patternMatcher.ToString()
					};
			};

            MatcherActionDefinition ConvertMatcherAction(MatcherAction matcherAction)
            {
                switch (matcherAction)
                {
                    case UpdateVariableMatcherAction updateVariableMatcherAction:
                        return new MatcherActionDefinition
                        {
                            Action = updateVariableMatcherAction.Action,
                            Name = updateVariableMatcherAction.Name,
                            Change = updateVariableMatcherAction.Change,
                            FirstVariableName = languageMatcher.Blobs[updateVariableMatcherAction.TargetBlobId],
                            SecondVariableName = languageMatcher.Blobs[updateVariableMatcherAction.SourceBlobId]
                        };
                    case CreateVariableMatcherAction createVariableMatcherAction:
                        return new MatcherActionDefinition
                        {
                            Action = createVariableMatcherAction.Action,
                            Name = createVariableMatcherAction.Name,
                            Source = createVariableMatcherAction.Source,
                            FirstVariableName = languageMatcher.Blobs[createVariableMatcherAction.BlobId],
                            Value = createVariableMatcherAction.Value
                        };
                    case AssertMatcherAction assertMatcherAction:
                        return new MatcherActionDefinition
                        {
                            Action = assertMatcherAction.Action,
                            Name = assertMatcherAction.Name,
                            Assert = assertMatcherAction.Assert,
                            FirstVariableName = languageMatcher.Blobs[assertMatcherAction.FirstBlobId],
                            SecondVariableName = languageMatcher.Blobs[assertMatcherAction.SecondBlobId]
                        };
                    default:
                        return null;
                };
            };

            string AddPatternAndGetName(PatternMatcher patternMatcher)
			{
				if (patternMatcher != null && !convertedPatterns.Contains(patternMatcher.Name))
				{
					patterns.Add(ConvertPattern(patternMatcher, true));
				}
				return patternMatcher?.Name;
			};

            string AddActionAndGetName(MatcherAction matcherAction)
            {
                if (matcherAction != null && !matcherActions.ContainsKey(matcherAction.Name))
                {
                    matcherActions.Add(matcherAction.Name, ConvertMatcherAction(matcherAction));
                }
                return matcherAction?.Name;
            };

            FragmentMatcherDefinition ConvertFragment(FragmentMatcher fragmentMatcher)
			{
				return new FragmentMatcherDefinition
				{
					BoundsAsParts = fragmentMatcher.BoundsAsParts,
					Cacheable = fragmentMatcher.Cacheable,
					ClearCache = fragmentMatcher.ClearCache,
					DiscardBounds = fragmentMatcher.DiscardBounds,
					ExpressionMode = fragmentMatcher.ExpressionMode,
					ExpressionOrder = fragmentMatcher.ExpressionOrder,
					FallThroughMode = fragmentMatcher.FallThroughMode,
					IsNoise = fragmentMatcher.IsNoise,
					MinMatchedParts = fragmentMatcher.MinMatchedParts,
					Negate = fragmentMatcher.Negate,
					Start = AddPatternAndGetName(fragmentMatcher.Start),
					End = AddPatternAndGetName(fragmentMatcher.End),
					Name = fragmentMatcher.Name,
					PartsDelimiter = AddPatternAndGetName(fragmentMatcher.PartsDelimiter),
					PartsDelimiterRequired = fragmentMatcher.PartsDelimiterRequired,
					PartsMatchMode = fragmentMatcher.PartsMatchMode,
					PartsPadding = AddPatternAndGetName(fragmentMatcher.PartsPadding),
					Parts = fragmentMatcher.Parts.Select(part =>
					{
						return part is FragmentMatcher fragmentPart ? $"[{fragmentPart.Name}]" : AddPatternAndGetName((PatternMatcher)part);
					}).ToList(),
                    Actions = fragmentMatcher.Actions?.Select(action => AddActionAndGetName(action)).ToList()
                };
			};
		}
	}
}
