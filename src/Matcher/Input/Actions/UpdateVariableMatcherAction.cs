using Synfron.Staxe.Matcher.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Synfron.Staxe.Matcher.Input.Actions
{
    public class UpdateVariableMatcherAction : MatcherAction
    {
        public int TargetBlobId { get; set; }

        public VariableUpdateAction Change { get; set; }

        public int SourceBlobId { get; set; }

        public override MatcherActionType Action => MatcherActionType.UpdateVariable;

        public override bool Perform(Span<BlobData> blobDatas, IList<IMatchData> matchDatas)
        {
            try
            {
                switch (Change)
                {
                    case VariableUpdateAction.Add:
                        {
                            string value1 = blobDatas[TargetBlobId].Value?.ToString();
                            string value2 = blobDatas[SourceBlobId].Value?.ToString();
                            blobDatas[TargetBlobId].Value = double.TryParse(value1, out double num1) 
                                && double.TryParse(value2, out double num2) ?
                                num1 + num2 : blobDatas[TargetBlobId].Value;
                            break;
                        }
                    case VariableUpdateAction.Subtract:
                        {
                            string value1 = blobDatas[TargetBlobId].Value?.ToString();
                            string value2 = blobDatas[SourceBlobId].Value?.ToString();
                            blobDatas[TargetBlobId].Value = double.TryParse(value1, out double num1)
                                && double.TryParse(value2, out double num2) ?
                                num1 - num2 : blobDatas[TargetBlobId].Value;
                            break;
                        }
                    case VariableUpdateAction.Concat:
                        {
                            string value1 = blobDatas[TargetBlobId].Value?.ToString();
                            string value2 = blobDatas[SourceBlobId].Value?.ToString();
                            blobDatas[TargetBlobId].Value = value1 + value2;
                            break;
                        }
                    case VariableUpdateAction.Remove:
                        {
                            string value1 = blobDatas[TargetBlobId].Value?.ToString();
                            string value2 = blobDatas[SourceBlobId].Value?.ToString();
                            blobDatas[TargetBlobId].Value = value1?.Replace(value2 ?? string.Empty, "");
                            break;
                        }
                    case VariableUpdateAction.Set:
                        {
                            blobDatas[TargetBlobId].Value = blobDatas[SourceBlobId].Value;
                            break;
                        }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal override string Generate(MatcherEngineGenerator generator)
        {
            string methodName = $"UpdateVariableMatcherAction{GetSafeMethodName(Name)}";
            string method = $"{methodName}({{0}}, {{1}})";
            if (!generator.TryGetMethod(methodName, ref method))
            {
                generator.Add(methodName, method);
                string code = $@"private bool {methodName}(Span<BlobData> blobDatas, IList<IMatchData> matchDatas)
        {{
            try {{
                {(Change == VariableUpdateAction.Add ? $@"
                string value1 = blobDatas[{TargetBlobId}].Value?.ToString();
                string value2 = blobDatas[{SourceBlobId}].Value?.ToString();
                blobDatas[{TargetBlobId}].Value = double.TryParse(value1, out double num1) 
                    && double.TryParse(value2, out double num2) ?
                    num1 + num2 : blobDatas[{TargetBlobId}].Value;
                " : null)}
                {(Change == VariableUpdateAction.Subtract ? $@"
                string value1 = blobDatas[{TargetBlobId}].Value?.ToString();
                string value2 = blobDatas[{SourceBlobId}].Value?.ToString();
                blobDatas[{TargetBlobId}].Value = double.TryParse(value1, out double num1)
                    && double.TryParse(value2, out double num2) ?
                    num1 - num2 : blobDatas[{TargetBlobId}].Value;
                " : null)}
                {(Change == VariableUpdateAction.Concat ? $@"
                string value1 = blobDatas[{TargetBlobId}].Value?.ToString();
                string value2 = blobDatas[{SourceBlobId}].Value?.ToString();
                blobDatas[{TargetBlobId}].Value = value1 + value2;
                " : null)}
                {(Change == VariableUpdateAction.Remove ? $@"
                string value1 = blobDatas[{TargetBlobId}].Value?.ToString();
                string value2 = blobDatas[{SourceBlobId}].Value?.ToString();
                blobDatas[{TargetBlobId}].Value = value1?.Replace(value2, """");
                " : null)}
                {(Change == VariableUpdateAction.Set ? $@"
                blobDatas[{TargetBlobId}].Value = blobDatas[{SourceBlobId}].Value;
                " : null)}
                return true;
            }}
            catch
            {{
                return false;
            }}
        }}";
                method = generator.Add(method, methodName, code);
            }
            return method;
        }
    }
}
