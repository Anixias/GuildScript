using GuildScript.Analysis.Text;

namespace GuildScript.Analysis.Syntax;

public enum SyntaxTokenType
{
	// Special Tokens
	Invalid = -1,
	EndOfFile,
	
	// Operators
	Plus,
	Minus,
	Star,
	Slash,
	OpenParen,
	CloseParen,
	
	// Literals
	IntegerLiteral
}

public sealed class SyntaxToken
{
	public SyntaxTokenType Type { get; }
	public string Text { get; }
	public object? Value { get; }
	public TextSpan Span { get; }

	public SyntaxToken(SyntaxTokenType type, string text, object? value, TextSpan span)
	{
		Type = type;
		Text = text;
		Value = value;
		Span = span;
	}
}