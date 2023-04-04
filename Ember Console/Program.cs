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