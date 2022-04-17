using Synfron.Staxe.Matcher.Data;
using Synfron.Staxe.Matcher.Input.Patterns;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Synfron.Staxe.Matcher.Input.Actions
{
    public class AssertMatcherAction : MatcherAction
    {
        public int FirstBlobId { get; set; }

        public int SecondBlobId { get; set; }

        public AssertType Assert { get; set; }

        public override MatcherActionType Action => MatcherActionType.Assert;

        public override bool Perform(Span<BlobData> blobDatas, IList<IMatchData> matchDatas)
        {
            switch (Assert)
            {
                case AssertType.Equals:
                    return Equals(blobDatas[FirstBlobId].Value?.ToString(), blobDatas[SecondBlobId].Value?.ToString());
                case AssertType.NotEquals:
                    return !Equals(blobDatas[FirstBlobId].Value?.ToString(), blobDatas[SecondBlobId].Value?.ToString());
                case AssertType.GreaterThan:
                    {
                        return double.TryParse(blobDatas[FirstBlobId].Value?.ToString(), out double num1) 
                            && double.TryParse(blobDatas[SecondBlobId].Value?.ToString(), out double num2) ?
                            num1 > num2 : false;
                    }
                case AssertType.GreaterThanOrEquals:
                    {
                        return double.TryParse(blobDatas[FirstBlobId].Value?.ToString(), out double num1)
                            && double.TryParse(blobDatas[SecondBlobId].Value?.ToString(), out double num2) ?
                            num1 >= num2 : false;
                    }
                case AssertType.LessThan:
                    {
                        return double.TryParse(blobDatas[FirstBlobId].Value?.ToString(), out double num1)
                            && double.TryParse(blobDatas[SecondBlobId].Value?.ToString(), out double num2) ?
                            num1 < num2 : false;
                    }
                case AssertType.LessThanOrEquals:
                    {
                        return double.TryParse(blobDatas[FirstBlobId].Value?.ToString(), out double num1)
                            && double.TryParse(blobDatas[SecondBlobId].Value?.ToString(), out double num2) ?
                            num1 <= num2 : false;
                    }
                case AssertType.Contains:
                    return blobDatas[FirstBlobId].Value?.ToString().Contains(blobDatas[SecondBlobId].Value?.ToString() ?? string.Empty) ?? false;
                case AssertType.StartsWith:
                    return blobDatas[FirstBlobId].Value?.ToString().StartsWith(blobDatas[SecondBlobId].Value?.ToString() ?? string.Empty) ?? false;
                case AssertType.EndsWith:
                    return blobDatas[FirstBlobId].Value?.ToString().EndsWith(blobDatas[SecondBlobId].Value?.ToString() ?? string.Empty) ?? false;
                case AssertType.MatchesPattern:
                    return PatternReader.Parse(blobDatas[SecondBlobId].Value?.ToString() ?? string.Empty).IsMatch(blobDatas[FirstBlobId].Value?.ToString() ?? string.Empty).success;
            }
            return false;
        }

        internal override string Generate(MatcherEngineGenerator generator)
        {
			string methodName = $"AssertMatcherAction{GetSafeMethodName(Name)}";
			string method = $"{methodName}({{0}}, {{1}})";
			if (!generator.TryGetMethod(methodName, ref method))
			{
				generator.Add(methodName, method);
				string code = $@"private bool {methodName}(Span<BlobData> blobDatas, IList<IMatchData> matchDatas)
        {{
            {(Assert == AssertType.Equals ? $@"
            return Equals(blobDatas[{FirstBlobId}].Value?.ToString(), blobDatas[{SecondBlobId}].Value?.ToString());
            " : null)}
            {(Assert == AssertType.NotEquals ? $@"
            return !Equals(blobDatas[{FirstBlobId}].Value?.ToString(), blobDatas[{SecondBlobId}].Value?.ToString());
            " : null)}
            {(Assert == AssertType.GreaterThan ? $@"
            return double.TryParse(blobDatas[{FirstBlobId}].Value?.ToString(), out double num1) 
                && double.TryParse(blobDatas[{SecondBlobId}].Value?.ToString(), out double num2) ?
                num1 > num2 : false;
            " : null)}
            {(Assert == AssertType.GreaterThanOrEquals ? $@"
            return double.TryParse(blobDatas[{FirstBlobId}].Value?.ToString(), out double num1) 
                && double.TryParse(blobDatas[{SecondBlobId}].Value?.ToString(), out double num2) ?
                num1 >= num2 : false;
            " : null)}
            {(Assert == AssertType.LessThan ? $@"
            return double.TryParse(blobDatas[{FirstBlobId}].Value?.ToString(), out double num1) 
                && double.TryParse(blobDatas[{SecondBlobId}].Value?.ToString(), out double num2) ?
                num1 < num2 : false;
            " : null)}
            {(Assert == AssertType.LessThanOrEquals ? $@"
            return double.TryParse(blobDatas[{FirstBlobId}].Value?.ToString(), out double num1) 
                && double.TryParse(blobDatas[{SecondBlobId}].Value?.ToString(), out double num2) ?
                num1 <= num2 : false;
            " : null)}
            {(Assert == AssertType.Contains ? $@"
            return blobDatas[{FirstBlobId}].Value?.ToString().Contains(blobDatas[{SecondBlobId}].Value?.ToString() ?? string.Empty) ?? false;
            " : null)}
            {(Assert == AssertType.StartsWith ? $@"
            return blobDatas[{FirstBlobId}].Value?.ToString().StartsWith(blobDatas[{SecondBlobId}].Value?.ToString() ?? string.Empty) ?? false;
            " : null)}
            {(Assert == AssertType.EndsWith ? $@"
            return blobDatas[{FirstBlobId}].Value?.ToString().EndsWith(blobDatas[{SecondBlobId}].Value?.ToString() ?? string.Empty) ?? false;
            " : null)}
            {(Assert == AssertType.MatchesPattern ? $@"
            return Synfron.Staxe.Matcher.Input.Patterns.PatternReader.Parse(blobDatas[{SecondBlobId}].Value?.ToString() ?? string.Empty).IsMatch(blobDatas[{FirstBlobId}].Value?.ToString() ?? string.Empty).success;
            " : null)}
            {(Assert < AssertType.Equals || Assert > AssertType.MatchesPattern ? $@"
            return false;
            " : null)}
        }}";
				method = generator.Add(method, methodName, code);
			}
			return method;
		}
    }
}
