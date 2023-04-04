using GuildScript.Analysis.Syntax;

namespace EmberConsole;

public class TreePrinter : Expression.IVisitor<object?>
{
	private readonly TextWriter writer;

	public TreePrinter(TextWriter writer)
	{
		this.writer = writer;
	}
	
	public void PrintTree(SyntaxTree tree)
	{
		if (tree.Root is not Expression expression)
		{
			return;
		}
		
		expression.AcceptVisitor(this);
		writer.WriteLine();
	}

	public object? VisitBinaryExpression(BinaryExpression expression)
	{
		writer.Write("[");
		expression.Left.AcceptVisitor(this);
		writer.Write($" {expression.OperatorToken.Text} ");
		expression.Right.AcceptVisitor(this);
		writer.Write("]");
		return null;
	}

	public object? VisitUnaryExpression(UnaryExpression expression)
	{
		writer.Write($"[{expression.OperatorToken.Text}");
		expression.Operand.AcceptVisitor(this);
		writer.Write("]");
		return null;
	}

	public object? VisitLiteralExpression(LiteralExpression expression)
	{
		writer.Write(expression.Token.Text);
		return null;
	}
}