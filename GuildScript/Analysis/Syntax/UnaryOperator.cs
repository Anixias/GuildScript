using GuildScript.Analysis.Semantics;
using GuildScript.Analysis.Semantics.Symbols;

namespace GuildScript.Analysis.Syntax;

public sealed class UnaryOperator : Operator
{
	public enum UnaryOperation
	{
		Identity,
		Negation,
		BitwiseNot,
		LogicalNot,
		NullSuppression
	}
	
	public bool IsPostfix { get; }
	public UnaryOperation? Operation => LookupUnaryOperation(TokenSpan, IsPostfix);
	
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
		var operation = Operation;

		if (operation == UnaryOperation.NullSuppression)
		{
			return operand is NullableResolvedType nullableResolvedType ? nullableResolvedType.BaseType : operand;
		}
		
		if (operand is not SimpleResolvedType)
			return null;
		
		switch (operation)
		{
			default:
				return null;
			case UnaryOperation.Identity:
			case UnaryOperation.Negation:
				if (operand.TypeSymbol == NativeTypeSymbol.Int8) return SimpleResolvedType.Int8;
				if (operand.TypeSymbol == NativeTypeSymbol.UInt8) return SimpleResolvedType.UInt8;
				if (operand.TypeSymbol == NativeTypeSymbol.Int16) return SimpleResolvedType.Int16;
				if (operand.TypeSymbol == NativeTypeSymbol.UInt16) return SimpleResolvedType.UInt16;
				if (operand.TypeSymbol == NativeTypeSymbol.Int32) return SimpleResolvedType.Int32;
				if (operand.TypeSymbol == NativeTypeSymbol.UInt32) return SimpleResolvedType.UInt32;
				if (operand.TypeSymbol == NativeTypeSymbol.Int64) return SimpleResolvedType.Int64;
				if (operand.TypeSymbol == NativeTypeSymbol.UInt64) return SimpleResolvedType.UInt64;
				if (operand.TypeSymbol == NativeTypeSymbol.Single) return SimpleResolvedType.Single;
				return operand.TypeSymbol == NativeTypeSymbol.Double ? SimpleResolvedType.Double : null;
			case UnaryOperation.BitwiseNot:
				if (operand.TypeSymbol == NativeTypeSymbol.Int8) return SimpleResolvedType.Int8;
				if (operand.TypeSymbol == NativeTypeSymbol.UInt8) return SimpleResolvedType.UInt8;
				if (operand.TypeSymbol == NativeTypeSymbol.Int16) return SimpleResolvedType.Int16;
				if (operand.TypeSymbol == NativeTypeSymbol.UInt16) return SimpleResolvedType.UInt16;
				if (operand.TypeSymbol == NativeTypeSymbol.Int32) return SimpleResolvedType.Int32;
				if (operand.TypeSymbol == NativeTypeSymbol.UInt32) return SimpleResolvedType.UInt32;
				if (operand.TypeSymbol == NativeTypeSymbol.Int64) return SimpleResolvedType.Int64;
				return operand.TypeSymbol == NativeTypeSymbol.UInt64 ? SimpleResolvedType.UInt64 : null;
			case UnaryOperation.LogicalNot:
				return operand.TypeSymbol == NativeTypeSymbol.Bool ? SimpleResolvedType.Bool : null;
		}
	}

	public static UnaryOperation? LookupUnaryOperation(SyntaxTokenSpan tokenSpan, bool isPostfix)
	{
		if (tokenSpan.Tokens.Length != 1)
			return null;

		return tokenSpan.Tokens[0].Type switch
		{
			SyntaxTokenType.Plus  => UnaryOperation.Identity,
			SyntaxTokenType.Minus => UnaryOperation.Negation,
			SyntaxTokenType.Tilde => UnaryOperation.BitwiseNot,
			SyntaxTokenType.Bang  => isPostfix ? UnaryOperation.NullSuppression : UnaryOperation.LogicalNot,
			_                     => null
		};
	}
}