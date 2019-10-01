using System;

namespace Synfron.Staxe.Shared
{
	public static class EngineUtils
	{
		public static int GetLineNumber(string text, int position)
		{
			if (position > text.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(position));
			}
			int lineCount = 1;
			for (int i = 0; i < text.Length && i < position; i++)
			{
				if (text[i] == '\n')
				{
					lineCount++;
				}
			}
			return lineCount;
		}
		public static (int, int) GetLineNumberAndColumn(string text, int position)
		{
			if (position > text.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(position));
			}
			int lineCount = 1;
			int lineStartPosition = 0;
			int i = 0;
			for (; i < text.Length && i < position; i++)
			{
				if (text[i] == '\n')
				{
					lineCount++;
					lineStartPosition = i + 1;
				}
			}
			return (lineCount, position - lineStartPosition + 1);
		}

		public static string GetLine(string text, int lineNumber, int trimLength = int.MaxValue)
		{
			int lineCount = 1;
			int position = 0;
			while (position < text.Length && lineCount < lineNumber)
			{
				if (text[position++] == '\n')
				{
					lineCount++;
				}
			}
			if (lineCount != lineNumber)
			{
				throw new ArgumentOutOfRangeException(nameof(lineNumber));
			}
			if (position == text.Length)
			{
				return string.Empty;
			}
			int nextPosition = position;
			while (nextPosition < text.Length)
			{
				if (text[nextPosition++] == '\n')
				{
					break;
				}
			}
			return text.Substring(position, Math.Min(nextPosition - position, trimLength));
		}

		public static string GetLineAtPosition(string text, int position, int trimLength = int.MaxValue)
		{
			int lineStartPosition = 0;
			int i = 0;
			for (; i < text.Length && i < position; i++)
			{
				if (text[i] == '\n')
				{
					lineStartPosition = i + 1;
				}
			}
			int nextPosition = lineStartPosition;
			while (nextPosition < text.Length)
			{
				if (text[nextPosition++] == '\n')
				{
					break;
				}
			}
			return text.Substring(lineStartPosition, Math.Min(Math.Max(nextPosition - lineStartPosition - 1, 0), trimLength));
		}
	}
}
