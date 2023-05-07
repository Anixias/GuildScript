using System.Collections.Immutable;
using System.Text;

namespace GuildScript.Analysis.Syntax;

public sealed class SyntaxTokenSpan
{
	public ImmutableArray<SyntaxToken> Tokens { get; }
	
	public SyntaxTokenSpan(IEnumerable<SyntaxToken> tokens)
	{
		Tokens = tokens.ToImmutableArray();
	}

	public SyntaxTokenSpan(params SyntaxToken[] tokens)
	{
		Tokens = tokens.ToImmutableArray();
	}

	public override string ToString()
	{
		var stringBuilder = new StringBuilder();

		foreach (var token in Tokens)
		{
			stringBuilder.Append(token.Text);
		}

		return stringBuilder.ToString();
	}
	
	public override int GetHashCode()
	{
		var hash = 0b01001111011001100101010101101010;

		for (var i = 0; i < Tokens.Length; i++)
		{
			hash ^= Tokens[i].Type.GetHashCode() << i;
		}

		return hash;
	}

	public override bool Equals(object? obj)
	{
		if (obj is SyntaxTokenSpan span)
			return Equals(span);

		return false;
	}

	private bool Equals(SyntaxTokenSpan span)
	{
		if (Tokens.Length != span.Tokens.Length)
			return false;

		for (var i = 0; i < Tokens.Length; i++)
		{
			if (Tokens[i].Type != span.Tokens[i].Type)
				return false;
		}

		return true;
	}
}