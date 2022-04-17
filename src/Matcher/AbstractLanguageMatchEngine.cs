using Synfron.Staxe.Matcher.Data;
using Synfron.Staxe.Matcher.Input;
using Synfron.Staxe.Shared.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synfron.Staxe.Matcher
{
    public abstract class AbstractLanguageMatchEngine : ILanguageMatchEngine
	{
		protected ref struct State
		{
			public int CurrentIndex;
			public List<StringMatchData> DistinctStringMatches;
			public int Id;
			public StringBuilder MatchLogBuilder;
			public string Code;
			public int DistinctIndex;
			public int MaxDistinctIndex;
			public Dictionary<ValueTuple<string, int>, FragmentMatchData> MatchCache;
			public int? FailureIndex;
			public Span<BlobData> BlobDatas;
            public bool PreMatchSuccess;
        }

		public abstract MatcherResult Match(string code, string fragmentMatcher, bool matchFullText = true);

		public abstract MatcherResult Match(string code, bool matchFullText = true);

		protected (bool success, int offset) MatchAny(string text, int startOffset = 0)
		{
			return startOffset < text.Length ? (true, 1) : (false, 0);
		}

		protected (bool success, int offset) MatchDigit(string text, int startOffset = 0)
		{
			return startOffset < text.Length && char.IsDigit(text[startOffset]) ? (true, 1) : (false, 0);
		}

		protected (bool success, int offset) MatchLetter(string text, int startOffset = 0)
		{
			return startOffset < text.Length && char.IsLetter(text[startOffset]) ? (true, 1) : (false, 0);
		}

		protected (bool success, int offset) MatchWhitespace(string text, int startOffset = 0)
		{
			return startOffset < text.Length && char.IsWhiteSpace(text[startOffset]) ? (true, 1) : (false, 0);
		}

		protected (bool success, int offset) MatchWordChar(string text, int startOffset = 0)
		{
			return startOffset < text.Length && char.IsLetterOrDigit(text[startOffset]) ? (true, 1) : (false, 0);
		}

		protected int GetLength(IList<IMatchData> matchDatas)
		{
			int length = 0;
			foreach (IMatchData matchData in matchDatas)
			{
				length += matchData.Length;
			}
			return length;
		}

		protected string GetText(IList<IMatchData> matchDatas)
		{
			StringBuilder textBuilder = new StringBuilder();
			foreach (IMatchData matchData in matchDatas)
			{
				textBuilder.Append(matchData.ToString());
			}
			return textBuilder.ToString();
		}

		protected void ConvertToExpressionTree(FragmentMatchData matchData, ExpressionMode expressionMode)
		{
			IMatchData rootPart = matchData.Parts.ElementAtOrDefault(0);
			switch (expressionMode)
			{
				case ExpressionMode.BinaryTree:
					rootPart = ConvertToBinaryTree(matchData, rootPart);
					break;
				case ExpressionMode.LikeNameTree:
					rootPart = ConvertToLikeNameTree(matchData, rootPart);
					break;
			}
			matchData.Parts.Clear();
			if (rootPart != null)
			{
				matchData.Parts.Add(rootPart);
			}
		}

		private IMatchData ConvertToBinaryTree(FragmentMatchData matchData, IMatchData rootPart)
		{
			foreach (ExpressionPartInfo partInfo in ExtractExpressionParts(matchData).OrderBy(part => part.ExpressionOrder ?? int.MaxValue))
			{
				if (partInfo.ExpressionOrder != null)
				{
					FragmentMatchData fragment = (FragmentMatchData)partInfo.Part;
					fragment.Parts.Add(partInfo.Next.Part);
					fragment.Parts.Insert(0, partInfo.Previous.Part);
					if (partInfo.Previous.Previous is ExpressionPartInfo newPreviousPartInfo)
					{
						newPreviousPartInfo.Next = partInfo;
						partInfo.Previous = newPreviousPartInfo;
					}
					if (partInfo.Next.Next is ExpressionPartInfo newNextPartInfo)
					{
						newNextPartInfo.Previous = partInfo;
						partInfo.Next = newNextPartInfo;
					}
					rootPart = fragment;
				}
			}

			return rootPart;
		}

		private IMatchData ConvertToLikeNameTree(FragmentMatchData matchData, IMatchData rootPart)
		{
			foreach (ExpressionPartInfo partInfo in ExtractExpressionParts(matchData).OrderBy(part => part.ExpressionOrder ?? int.MaxValue))
			{
				if (partInfo.ExpressionOrder != null)
				{
					FragmentMatchData fragment = (FragmentMatchData)partInfo.Part;
					if (partInfo.Previous.Part is FragmentMatchData previousFragment && previousFragment.Name == fragment.Name)
					{
						fragment.Parts.InsertRange(0, previousFragment.Parts);
					}
					else
					{
						fragment.Parts.Insert(0, partInfo.Previous.Part);
					}
					fragment.Parts.Add(partInfo.Next.Part);
					if (partInfo.Previous.Previous is ExpressionPartInfo newPreviousPartInfo)
					{
						newPreviousPartInfo.Next = partInfo;
						partInfo.Previous = newPreviousPartInfo;
					}
					if (partInfo.Next.Next is ExpressionPartInfo newNextPartInfo)
					{
						newNextPartInfo.Previous = partInfo;
						partInfo.Next = newNextPartInfo;
					}
					rootPart = fragment;
				}
			}

			return rootPart;
		}

		protected IEnumerable<ExpressionPartInfo> ExtractExpressionParts(FragmentMatchData matchData)
		{
			ExpressionPartInfo partInfo = null;
			foreach (IMatchData partMatchData in matchData.Parts)
			{
				ExpressionPartInfo oldPartInfo = partInfo;
				partInfo = new ExpressionPartInfo
				{
					Previous = partInfo,
					ExpressionOrder = (partMatchData as FragmentMatchData)?.ExpressionOrder,
					Part = partMatchData
				};
				if (oldPartInfo != null)
				{
					oldPartInfo.Next = partInfo;
				}
				if (partInfo.ExpressionOrder != null)
				{
					yield return partInfo;
					FragmentMatchData fragmentPart = (FragmentMatchData)partInfo.Part;
					IMatchData lastPart = fragmentPart.Parts.TakeLast();
					oldPartInfo = partInfo;
					partInfo = new ExpressionPartInfo
					{
						Previous = partInfo,
						Part = lastPart
					};
					oldPartInfo.Next = partInfo;
				}
				yield return partInfo;

			}
		}
	}
}
