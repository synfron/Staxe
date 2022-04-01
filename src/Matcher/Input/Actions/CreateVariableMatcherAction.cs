using Synfron.Staxe.Matcher.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Synfron.Staxe.Matcher.Input.Actions
{
    public class CreateVariableMatcherAction : MatcherAction
    {
        public int BlobId { get; set; }

        public VariableValueSource Source { get; set; }

        public object Value { get; set; }

        public override MatcherActionType ActionType => MatcherActionType.CreateVariable;

        public override bool Perform(Span<BlobData> blobDatas, IList<IMatchData> matchDatas)
        {
            switch (Source)
            {
                case VariableValueSource.Value:
                    blobDatas[BlobId] = new BlobData
                    {
                        Value = Value
                    };
                    break;
                case VariableValueSource.PartsText:
                    blobDatas[BlobId] = new BlobData
                    {
                        Value = matchDatas.GetText(true)
                    };
                    break;
                case VariableValueSource.PartsLength:
                    blobDatas[BlobId] = new BlobData
                    {
                        Value = matchDatas.GetLength(true)
                    };
                    break;
                case VariableValueSource.StringPartsText:
                    blobDatas[BlobId] = new BlobData
                    {
                        Value = matchDatas.GetText(false)
                    };
                    break;
                case VariableValueSource.StringPartsLength:
                    blobDatas[BlobId] = new BlobData
                    {
                        Value = matchDatas.GetLength(false)
                    };
                    break;
                case VariableValueSource.PartsCount:
                    blobDatas[BlobId] = new BlobData
                    {
                        Value = matchDatas.Count
                    };
                    break;
            }
            return true;
        }

        internal override string Generate(MatcherEngineGenerator generator)
        {
            string methodName = $"CreateVariableMatcherAction{GetSafeMethodName(Name)}";
            string method = $"{methodName}({{0}}, {{1}})";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(Span<BlobData> blobDatas, IList<IMatchData> matchDatas)
        {{
                {(Source == VariableValueSource.Value ? $@"
                blobDatas[{BlobId}] = new BlobData
                {{
                    Value = Value
                }};
                " : null)}
                {(Source == VariableValueSource.PartsText ? $@"
                blobDatas[{BlobId}] = new BlobData
                {{
                    Value = GetText(matchDatas)
                }};
                " : null)}
                {(Source == VariableValueSource.PartsLength ? $@"
                blobDatas[{BlobId}] = new BlobData
                {{
                    Value = GetLength(matchDatas)
                }};
                " : null)}
            return true;
        }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }
    }
}
