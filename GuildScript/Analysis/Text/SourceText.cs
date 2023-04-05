namespace GuildScript.Analysis.Text;

public class SourceText
{
	private readonly string source;

	public SourceText(string source)
	{
		this.source = source;
	}

	public string Span(TextSpan span)
	{
		return source.Substring(span.Start, span.Length);
	}
}