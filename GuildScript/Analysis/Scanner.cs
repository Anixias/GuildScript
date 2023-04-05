using GuildScript.Analysis.Syntax;
using GuildScript.Analysis.Text;

namespace GuildScript.Analysis;

public sealed class Scanner
{
	public DiagnosticCollection Diagnostics { get; }
	
	private bool EndOfFile => position >= source.Length;
	private char Current => position < 0 || EndOfFile ? '\0' : source[position];
	private int Length => position - start;
	private TextSpan Span => new(start, Length);

	private int start;
	private SyntaxTokenType type;
	private string text = "\0";
	private object? value;
	private int position;
	private readonly string source;

	public Scanner(string source)
	{
		Diagnostics = new DiagnosticCollection();
		this.source = source;
	}

	private void Reset()
	{
		start = position;
		type = SyntaxTokenType.Invalid;
		text = "\0";
		value = null;
	}

	public SyntaxToken ScanToken()
	{
		if (EndOfFile)
		{
			return new SyntaxToken(SyntaxTokenType.EndOfFile, "\0", null, new TextSpan(position, 0));
		}

		if (char.IsWhiteSpace(Current))
		{
			SkipWhiteSpace();
		}
			
		Reset();
			
		if (char.IsDigit(Current))
		{
			ScanNumber();
		}
		else switch (Current)
		{
			case '+':
				ScanOperator(SyntaxTokenType.Plus);
				break;
			case '-':
				ScanOperator(SyntaxTokenType.Minus);
				break;
			case '*':
				ScanOperator(SyntaxTokenType.Star);
				break;
			case '/':
				ScanOperator(SyntaxTokenType.Slash);
				break;
			case '(':
				ScanOperator(SyntaxTokenType.OpenParen);
				break;
			case ')':
				ScanOperator(SyntaxTokenType.CloseParen);
				break;
			default:
				ScanInvalidCharacter();
				break;
		}
			
		text = source.Substring(start, Length);
		return new SyntaxToken(type, text, value, Span);
	}

	private void SkipWhiteSpace()
	{
		while (char.IsWhiteSpace(Current))
		{
			Advance();
		}
	}

	private void Advance()
	{
		position++;
	}

	private void ScanInvalidCharacter()
	{
		var invalidCharacter = Current;
		Advance();
		type = SyntaxTokenType.Invalid;

		Diagnostics.ReportScannerInvalidCharacter(new TextSpan(start, Length), invalidCharacter);
	}

	private void ScanNumber()
	{
		while (char.IsDigit(Current))
		{
			Advance();
		}

		type = SyntaxTokenType.IntegerLiteral;
		if (int.TryParse(source.AsSpan(start, Length), out var intValue))
			value = intValue;
	}

	private void ScanOperator(SyntaxTokenType operatorType, int characterCount = 1)
	{
		while (Length < characterCount)
		{
			Advance();
		}

		type = operatorType;
	}
}