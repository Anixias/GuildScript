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

		var scanner = new Scanner(input);
		SyntaxToken token;

		do
		{
			token = scanner.ScanToken();
			Console.WriteLine($"{token.Type}: {token.Text}");
		} while (token.Type != SyntaxTokenType.EndOfFile);
	}
}

void CompileFile(string path)
{
	
}