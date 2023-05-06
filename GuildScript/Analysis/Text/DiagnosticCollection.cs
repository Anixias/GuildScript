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

	public void ReportScannerInvalidCharacterConstant(TextSpan span, string text)
	{
		var message = $"Invalid character constant '{text}'.";
		Report(span, message);
	}

	public void ReportParserExpectedExpression(TextSpan span, SyntaxToken token)
	{
		var message = $"Expected expression at '{token.Text}'.";
		Report(span, message);
	}

	public void ReportTypeCollectorException(SyntaxToken identifier, string message)
	{
		Report(identifier.Span, message);
	}

	public void ReportNameResolverDuplicateDeclaration(SyntaxToken identifier)
	{
		var message = $"Duplicate declaration: \"{identifier.Text}\" is already defined in this context.";
		Report(identifier.Span, message);
	}

	public void ReportNameResolverUnresolvedType(SyntaxToken identifier)
	{
		var message = $"The type '{identifier.Text}' does not exist in this scope.";
		Report(identifier.Span, message);
	}

	public void ReportNameResolverEntryPointNotDefined()
	{
		const string message = "Entry point must be defined.";
		Report(new TextSpan(0, 0), message);
	}

	public void ReportNameResolveEntryPointRedefined(SyntaxToken identifier)
	{
		const string message = "Entry point is already defined.";
		Report(identifier.Span, message);
	}
}