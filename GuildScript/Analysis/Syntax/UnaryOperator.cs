using GuildScript.Analysis.Semantics;

namespace GuildScript.Analysis.Syntax;

public sealed class UnaryOperator : Operator
{
	public bool IsPostfix { get; }
	
	public UnaryOperator(SyntaxTokenSpan tokenSpan, bool isPostfix) : base(tokenSpan)
	{
		IsPostfix = isPostfix;
	}
	
	public UnaryOperator(SyntaxToken token, bool isPostfix) : base(new SyntaxTokenSpan(token))
	{
		IsPostfix = isPostfix;
	}

	protected override bool Equals(Operator other)
	{
		if (other is not UnaryOperator unaryOperator)
			return false;
		
		return IsPostfix == unaryOperator.IsPostfix && base.Equals(other);
	}
	
	public override int GetHashCode()
	{
		return TokenSpan.GetHashCode() ^ (IsPostfix.GetHashCode() << 1);
	}

	public ResolvedType? GetResultType(ResolvedType operand)
	{
		
	}
}