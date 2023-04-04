using EmberConsole;
using GuildScript.Analysis.Syntax;

if (args.Length == 0)
{
	ShowPrompt();
}
else
{
	CompileFile(args[0]);
}

void ShowPrompt()
{
	while (true)
	{
		Console.Write("> ");
		
		var input = Console.ReadLine();
		if (string.IsNullOrWhiteSpace(input))
			break;

		var parser = new Parser(input);

		try
		{
			var tree = parser.Parse();
			var treePrinter = new TreePrinter(Console.Out);
			treePrinter.PrintTree(tree);

			var evaluator = new Evaluator();
			var value = evaluator.Evaluate(tree);
			
			if (value is not null)
				Console.WriteLine($"= {value}");
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}
}

void CompileFile(string path)
{
	
}

internal class Evaluator : Expression.IVisitor<object?>
{
	public object? Evaluate(SyntaxTree tree)
	{
		return tree.Root is not Expression expression ? null : EvaluateExpression(expression);
	}

	private object? EvaluateExpression(Expression expression)
	{
		return expression.AcceptVisitor(this);
	}

	public object? VisitBinaryExpression(BinaryExpression expression)
	{
		var left = EvaluateExpression(expression.Left);
		var right = EvaluateExpression(expression.Right);

		return left switch
		{
			int leftValue when right is int rightValue => expression.OperatorToken.Type switch
			{
				SyntaxTokenType.Plus  => leftValue + rightValue,
				SyntaxTokenType.Minus => leftValue - rightValue,
				SyntaxTokenType.Star  => leftValue * rightValue,
				SyntaxTokenType.Slash => leftValue / rightValue,
				_                     => null
			},
			_ => null
		};
	}

	public object? VisitUnaryExpression(UnaryExpression expression)
	{
		switch (expression.OperatorToken.Type)
		{
			default:
			case SyntaxTokenType.Plus:
				return EvaluateExpression(expression.Operand);
			case SyntaxTokenType.Minus:
				var value = EvaluateExpression(expression.Operand);
				if (value is int intValue)
					value = -intValue;
					
				return value;
		}
	}

	public object? VisitLiteralExpression(LiteralExpression expression)
	{
		return expression.Token.Value;
	}
}