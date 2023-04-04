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
	
	// Literals
	IntegerLiteral
}

public sealed class SyntaxToken
{
	public SyntaxTokenType Type { get; }
	public string Text { get; }
	public int Position { get; }
	public object? Value { get; }

	public SyntaxToken(SyntaxTokenType type, string text, int position, object? value)
	{
		Type = type;
		Text = text;
		Position = position;
		Value = value;
	}
}