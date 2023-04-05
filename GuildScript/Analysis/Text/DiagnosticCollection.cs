using System.Collections;
using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Text;

public class DiagnosticCollection : IEnumerable<Diagnostic>, IEnumerable
{
	private readonly List<Diagnostic> diagnostics = new();

	private void Report(TextSpan span, string message)
	{
		diagnostics.Add(new Diagnostic(span, message));
	}
	
	public IEnumerator<Diagnostic> GetEnumerator()
	{
		return diagnostics.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void AppendDiagnostics(DiagnosticCollection diagnosticCollection)
	{
		diagnostics.AddRange(diagnosticCollection.diagnostics);
	}

	public void ReportScannerInvalidCharacter(TextSpan span, char invalidCharacter)
	{
		var message = $"Invalid character '{invalidCharacter}'.";
		Report(span, message);
	}

	public void ReportParserExpectedExpression(TextSpan span, SyntaxToken token)
	{
		var message = $"Expected expression at '{token.Text}.'";
		Report(span, message);
	}
}