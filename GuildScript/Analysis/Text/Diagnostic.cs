namespace GuildScript.Analysis.Text;

public class Diagnostic
{
	public TextSpan? Span { get; }
	public string Message { get; }

	public Diagnostic(TextSpan? span, string message)
	{
		Span = span;
		Message = message;
	}

	public override string ToString()
	{
		return Span is null ? Message : $"[{Span.Line}:{Span.Column}] {Message}";
	}
}