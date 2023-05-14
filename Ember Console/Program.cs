using EmberConsole;
using GuildScript;
using GuildScript.Analysis;
using GuildScript.Analysis.Semantics;
using GuildScript.Analysis.Syntax;
using GuildScript.Analysis.Text;

if (args.Length == 0)
{
	ShowPrompt();
	return;
}

Compiler.Initialize();

var position = 0;
var arguments = new Dictionary<string, Action>
{
	{ "-in", ArgumentIn },
	{ "-out", ArgumentOut }
};

string ReadArgument()
{
	return args[position++];
}

bool IsValidFilename(string path)
{
	return !string.IsNullOrWhiteSpace(path) && path.IndexOfAny(Path.GetInvalidPathChars()) < 0;
}

string? inputPath = null;
void ArgumentIn()
{
	inputPath = ReadArgument();
}

string? outputPath = null;
void ArgumentOut()
{
	outputPath = ReadArgument();

	if (!IsValidFilename(outputPath))
		throw new Exception("Invalid output path.");

	if (Path.GetExtension(outputPath) != ".gsx")
		throw new Exception("Output file extension must be \"*.gsx\".");
}

try
{
	ExecuteCommand();
}
catch (Exception e)
{
	Console.ForegroundColor = ConsoleColor.DarkRed;
	Console.WriteLine(e.Message);
	Console.ResetColor();
}

void ShowPrompt()
{
	while (true)
	{
		Console.Write("> ");
		
		var input = Console.ReadLine();
		if (string.IsNullOrWhiteSpace(input))
			break;

		var source = new SourceText(input);
		var parser = new Parser(source);

		try
		{
			var tree = parser.Parse();
			if (parser.Diagnostics.Any() || tree is null)
			{
				Console.ForegroundColor = ConsoleColor.DarkRed;

				foreach (var diagnostic in parser.Diagnostics)
				{
					Console.WriteLine(diagnostic);
				}
				
				Console.ResetColor();
			}
			else
			{
				//var treePrinter = new TreePrinter(Console.Out);
				//treePrinter.PrintTree(tree);

				//var evaluator = new Evaluator();
				//var value = evaluator.Evaluate(tree);
			
				//if (value is not null)
				//	Console.WriteLine($"= {value}");
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
		}
	}
}

SyntaxTree? AnalyzeFile(string path, SemanticModel semanticModel)
{
	var input = File.ReadAllText(path);
	var source = new SourceText(input);
	var parser = new Parser(source);
	var collector = new Collector(semanticModel);

	try
	{
		var tree = parser.Parse();
		
		if (parser.Diagnostics.Any() || tree is null)
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;

			foreach (var diagnostic in parser.Diagnostics)
			{
				Console.WriteLine(diagnostic);
			}
				
			Console.ResetColor();
			return null;
		}

		var treePrinter = new TreePrinter(Console.Out);
		treePrinter.PrintTree(tree);

		collector.CollectSymbols(tree);
		if (!collector.Diagnostics.Any())
			return tree;
		
		Console.ForegroundColor = ConsoleColor.DarkRed;

		foreach (var diagnostic in collector.Diagnostics)
		{
			Console.WriteLine(diagnostic);
		}
			
		Console.ResetColor();
		return null;
	}
	catch (Exception e)
	{
		Console.ForegroundColor = ConsoleColor.DarkRed;
		Console.WriteLine(e);
		Console.ResetColor();
		return null;
	}
}

ResolvedTree? LinkTree(SyntaxTree tree, SemanticModel semanticModel)
{
	var resolver = new Resolver(semanticModel);
	return resolver.Resolve(tree);
}

void CompileFolder()
{
	var files = Directory.GetFiles(inputPath, "*.gs", SearchOption.AllDirectories);
	var semanticModel = new SemanticModel();

	var trees = new List<SyntaxTree>();
	
	foreach (var file in files)
	{
		if (AnalyzeFile(file, semanticModel) is { } tree)
			trees.Add(tree);
	}

	// @TODO Move to SemanticAnalyzer
	try
	{
		semanticModel.VerifyEntryPoint();
	}
	catch (Exception e)
	{
		Console.ForegroundColor = ConsoleColor.DarkRed;
		Console.WriteLine(e.Message);
		Console.ResetColor();
	}

	var resolver = new Resolver(semanticModel);
	resolver.ReplaceAliases();

	var resolvedTrees = new List<ResolvedTree>();

	foreach (var tree in trees)
	{
		if (LinkTree(tree, semanticModel) is { } resolvedTree)
			resolvedTrees.Add(resolvedTree);
	}
	
	
}

/*
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

	public object? VisitBinaryExpression(Expression.Binary expression)
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
				SyntaxTokenType.Hat => (int)Math.Pow(leftValue, rightValue),
				_                     => null
			},
			_ => null
		};
	}

	public object? VisitUnaryExpression(Expression.Unary expression)
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

	public object? VisitLiteralExpression(Expression.Literal expression)
	{
		return expression.Token.Value;
	}
}*/

void ExecuteCommand()
{
	while (position < args.Length)
	{
		if (arguments.TryGetValue(ReadArgument(), out var action))
		{
			action();
		}
	}

	if (inputPath is null)
		throw new Exception("Expected \"-in <path>\" argument.");

	if (outputPath is null)
		throw new Exception("Expected \"-out <path>\" argument.");

	if (File.Exists(inputPath))
	{
		/*var nameResolver = new NameResolver();
		CompileFile(inputPath, nameResolver);
		nameResolver.Finish();
		
		Console.ForegroundColor = ConsoleColor.DarkRed;
		foreach (var diagnostic in nameResolver.Diagnostics)
		{
			Console.WriteLine(diagnostic);
		}
		Console.ResetColor();*/
	}
	else if (Directory.Exists(inputPath))
		CompileFolder();
	else
		throw new Exception("The input is not an existing file or directory.");
}