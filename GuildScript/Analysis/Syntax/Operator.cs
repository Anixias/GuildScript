namespace GuildScript.Analysis.Syntax;

public abstract class Operator
{
	public SyntaxTokenSpan TokenSpan { get; }

	protected Operator(SyntaxTokenSpan tokenSpan)
	{
		TokenSpan = tokenSpan;
	}

	public override string ToString()
	{
		return TokenSpan.ToString();
	}

	public override bool Equals(object? obj)
	{
		if (obj is Operator other)
			return Equals(other);

		return false;
	}

	public override int GetHashCode()
	{
		return TokenSpan.GetHashCode();
	}

	protected virtual bool Equals(Operator other)
	{
		return GetType() == other.GetType() && TokenSpan.Equals(other.TokenSpan);
	}
}
