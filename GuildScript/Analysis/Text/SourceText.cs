namespace GuildScript.Analysis.Text;

public class SourceText
{
	public int Length => source.Length;
	
	private readonly string source;

	public SourceText(string source)
	{
		this.source = source;
	}

	public string Span(TextSpan span)
	{
		return source.Substring(span.Start, span.Length);
	}

	public char CharAt(int index)
	{
		return source[index];
	}

	public int PositionGetLine(int position)
	{
		if (position < 0 || position > source.Length)
			return 0;

		var lineNumber = 1;
		var currentPosition = 0;

		while (currentPosition < position)
		{
			switch (source[currentPosition])
			{
				case '\r':
				{
					if (currentPosition + 1 < source.Length && source[currentPosition + 1] == '\n')
						currentPosition++;
				
					lineNumber++;
					break;
				}
				case '\n':
					lineNumber++;
					break;
			}

			currentPosition++;
		}

		return lineNumber;
	}

	public int PositionGetColumn(int position)
	{
		if (position < 0 || position > source.Length)
			return 0;

		var columnNumber = 1;
		var currentPosition = 0;

		while (currentPosition < position)
		{
			switch (source[currentPosition])
			{
				case '\r':
				{
					if (currentPosition + 1 < source.Length && source[currentPosition + 1] == '\n')
						currentPosition++;
				
					columnNumber = 1;
					break;
				}
				case '\n':
					columnNumber = 1;
					break;
				default:
					columnNumber++;
					break;
			}

			currentPosition++;
		}

		return columnNumber;
	}
}