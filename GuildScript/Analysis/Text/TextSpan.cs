namespace GuildScript.Analysis.Text;

public class TextSpan
{
	public int Start { get; }
	public int Length { get; }
	public int End => Start + Length;
	public SourceText SourceText { get; }

	public int Line => SourceText.PositionGetLine(Start);
	public int Column => SourceText.PositionGetColumn(Start);

	public TextSpan(int start, int length, SourceText sourceText)
	{
		Start = start;
		Length = length;
		SourceText = sourceText;
	}
}