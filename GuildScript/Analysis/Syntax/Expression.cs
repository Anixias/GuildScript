namespace GuildScript.Analysis.Syntax;

public abstract class Expression : SyntaxNode
{
	public interface IVisitor<out T>
	{
		T VisitBinaryExpression(BinaryExpression expression);
		T VisitUnaryExpression(UnaryExpression expression);
		T VisitLiteralExpression(LiteralExpression expression);
	}
	
	public abstract T AcceptVisitor<T>(IVisitor<T> visitor);
}

public sealed class BinaryExpression : Expression
{
	public Expression Left { get; }
	public SyntaxToken OperatorToken { get; }
	public Expression Right { get; }

	public BinaryExpression(Expression left, SyntaxToken operatorToken, Expression right)
	{
		Left = left;
		OperatorToken = operatorToken;
		Right = right;
	}

	public override T AcceptVisitor<T>(IVisitor<T> visitor)
	{
		return visitor.VisitBinaryExpression(this);
	}
}

public sealed class UnaryExpression : Expression
{
	public Expression Operand { get; }
	public SyntaxToken OperatorToken { get; }

	public UnaryExpression(Expression operand, SyntaxToken operatorToken)
	{
		Operand = operand;
		OperatorToken = operatorToken;
	}

	public override T AcceptVisitor<T>(IVisitor<T> visitor)
	{
		return visitor.VisitUnaryExpression(this);
	}
}


public sealed class LiteralExpression : Expression
{
	public SyntaxToken Token { get; }

	public LiteralExpression(SyntaxToken token)
	{
		Token = token;
	}

	public override T AcceptVisitor<T>(IVisitor<T> visitor)
	{
		return visitor.VisitLiteralExpression(this);
	}
}
