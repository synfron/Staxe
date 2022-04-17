using Synfron.Staxe.Matcher;
using Synfron.Staxe.Matcher.Data;
using Synfron.Staxe.Matcher.Input.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MatcherTests.Tests.Actions
{
    public class MatcherActionTests
    {
        [Theory]
        [InlineData(AssertType.Equals, "abc", "abc", true)]
        [InlineData(AssertType.Equals, "abc", "abd", false)]
        [InlineData(AssertType.Equals, 1, 1, true)]
        [InlineData(AssertType.Equals, 1, 2, false)]
        [InlineData(AssertType.NotEquals, "abc", "abc", false)]
        [InlineData(AssertType.NotEquals, "abc", "abd", true)]
        [InlineData(AssertType.NotEquals, 1, 1, false)]
        [InlineData(AssertType.NotEquals, 1, 2, true)]
        [InlineData(AssertType.GreaterThan, 3, 2, true)]
        [InlineData(AssertType.GreaterThan, 2, 2, false)]
        [InlineData(AssertType.GreaterThanOrEquals, 3, 2, true)]
        [InlineData(AssertType.GreaterThanOrEquals, 2, 2, true)]
        [InlineData(AssertType.GreaterThanOrEquals, 1, 2, false)]
        [InlineData(AssertType.LessThan, 1, 2, true)]
        [InlineData(AssertType.LessThan, 2, 2, false)]
        [InlineData(AssertType.LessThanOrEquals, 1, 2, true)]
        [InlineData(AssertType.LessThanOrEquals, 2, 2, true)]
        [InlineData(AssertType.LessThanOrEquals, 3, 2, false)]
        [InlineData(AssertType.Contains, 12, 2, true)]
        [InlineData(AssertType.Contains, "abc", "b", true)]
        [InlineData(AssertType.Contains, "12", 2, true)]
        [InlineData(AssertType.Contains, 12, "2", true)]
        [InlineData(AssertType.Contains, 12, 4, false)]
        [InlineData(AssertType.Contains, "abc", "d", false)]
        [InlineData(AssertType.Contains, "12", 4, false)]
        [InlineData(AssertType.Contains, 12, "4", false)]
        [InlineData(AssertType.StartsWith, 12, 1, true)]
        [InlineData(AssertType.StartsWith, "abc", "a", true)]
        [InlineData(AssertType.StartsWith, "12", 1, true)]
        [InlineData(AssertType.StartsWith, 12, "1", true)]
        [InlineData(AssertType.StartsWith, 12, 2, false)]
        [InlineData(AssertType.StartsWith, "abc", "b", false)]
        [InlineData(AssertType.StartsWith, "12", 2, false)]
        [InlineData(AssertType.StartsWith, 12, "2", false)]
        [InlineData(AssertType.EndsWith, 12, 2, true)]
        [InlineData(AssertType.EndsWith, "abc", "c", true)]
        [InlineData(AssertType.EndsWith, "12", 2, true)]
        [InlineData(AssertType.EndsWith, 12, "2", true)]
        [InlineData(AssertType.EndsWith, 12, 1, false)]
        [InlineData(AssertType.EndsWith, "abc", "b", false)]
        [InlineData(AssertType.EndsWith, "12", 1, false)]
        [InlineData(AssertType.EndsWith, 12, "1", false)]
        [InlineData(AssertType.MatchesPattern, "12", "\\d+", true)]
        [InlineData(AssertType.MatchesPattern, 12, "\\d+", true)]
        [InlineData(AssertType.MatchesPattern, "12", "\\l", false)]
        [InlineData(AssertType.MatchesPattern, 12, "\\l", false)]
        public void AssertMatcherAction_Perform(AssertType assertType, object firstValue, object secondValue, bool expected)
        {
            Span<BlobData> blobDatas = new Span<BlobData>(new BlobData[]
            {
                new BlobData
                {
                    Value = firstValue
                },
                new BlobData
                {
                    Value = secondValue
                }
            });
            AssertMatcherAction action = new AssertMatcherAction
            {
                Name = "Test",
                FirstBlobId = 0,
                SecondBlobId = 1,
                Assert = assertType
            };
            bool result = action.Perform(blobDatas, new IMatchData[0]);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(VariableUpdateAction.Set, "abc", "cba", "cba")]
        [InlineData(VariableUpdateAction.Set, "abc", 1, 1)]
        [InlineData(VariableUpdateAction.Set, 1, "cba", "cba")]
        [InlineData(VariableUpdateAction.Add, 1, 2, 3.0)]
        [InlineData(VariableUpdateAction.Add, "1", "2", 3.0)]
        [InlineData(VariableUpdateAction.Add, "abc", "cba", "abc")]
        [InlineData(VariableUpdateAction.Subtract, 3, 2, 1.0)]
        [InlineData(VariableUpdateAction.Subtract, "3", "2", 1.0)]
        [InlineData(VariableUpdateAction.Concat, 1, 2, "12")]
        [InlineData(VariableUpdateAction.Concat, "abc", "1", "abc1")]
        [InlineData(VariableUpdateAction.Concat, "abc", 1, "abc1")]
        [InlineData(VariableUpdateAction.Concat, 1, "cba", "1cba")]
        [InlineData(VariableUpdateAction.Concat, "abc", "cba", "abccba")]
        [InlineData(VariableUpdateAction.Remove, "abc", "b", "ac")]
        [InlineData(VariableUpdateAction.Remove, "abc1", "1", "abc")]
        [InlineData(VariableUpdateAction.Remove, "abc1", 1, "abc")]
        public void UpdateVariableMatcherAction_Perform(VariableUpdateAction updateAction, object firstValue, object secondValue, object expected)
        {
            Span<BlobData> blobDatas = new Span<BlobData>(new BlobData[]
            {
                new BlobData
                {
                    Value = firstValue
                },
                new BlobData
                {
                    Value = secondValue
                }
            });
            UpdateVariableMatcherAction action = new UpdateVariableMatcherAction
            {
                Name = "Test",
                TargetBlobId = 0,
                SourceBlobId = 1,
                Change = updateAction
            };
            bool result = action.Perform(blobDatas, Array.Empty<IMatchData>());
            Assert.True(result);
            Assert.Equal(expected, blobDatas[0].Value);
        }

        [Theory]
        [InlineData(VariableValueSource.StringPartsText, "abcd")]
        [InlineData(VariableValueSource.StringPartsLength, 4)]
        [InlineData(VariableValueSource.PartsCount, 3)]
        [InlineData(VariableValueSource.PartsLength, 14)]
        [InlineData(VariableValueSource.PartsXml, "<Str1>abc</Str1><Test></Test><Str2>d</Str2>")]
        [InlineData(VariableValueSource.Value, "abc")]
        public void CreateVariableMatcherAction_Perform(VariableValueSource source, object expected)
        {
            Span<BlobData> blobDatas = new Span<BlobData>(new BlobData[1]);
            CreateVariableMatcherAction action = new CreateVariableMatcherAction
            {
                Name = "Test",
                Value = "abc",
                Source = source
            };
            IMatchData[] matchDatas = new IMatchData[]
            {
                new StringMatchData
                {
                    Name = "Str1",
                    Length = 3,
                    Text = "abc"
                },
                new FragmentMatchData
                {
                    Name = "Test",
                    Length = 10
                },
                new StringMatchData
                {
                    Name = "Str2",
                    Length = 1,
                    Text = "d"
                }
            };
            bool result = action.Perform(blobDatas, matchDatas);
            Assert.True(result);
            Assert.Equal(expected, blobDatas[0].Value);
        }
    }
}
