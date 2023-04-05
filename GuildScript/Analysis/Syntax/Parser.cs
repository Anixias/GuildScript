using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace GuildScript.Analysis.Syntax;

public sealed class Parser
{
	public DiagnosticCollection Diagnostics { get; }
	
	private SyntaxToken Next => Peek();

	private readonly ImmutableArray<SyntaxToken> tokens;
	private int position;

	public Parser(string source)
	{
		Diagnostics = new DiagnosticCollection();
		var scanner = new Scanner(source);
		tokens = ParseTokens(scanner);

		Diagnostics.AppendDiagnostics(scanner.Diagnostics);
	}

	public SyntaxTree? Parse()
	{
		if (Diagnostics.Any())
			return null;
		
		var expression = ParseExpression();
		return new SyntaxTree(expression);
	}

	private SyntaxToken Advance()
	{
		var token = Next;

		if (position < tokens.Length)
			position++;
		
		return token;
	}

	private void Consume(SyntaxTokenType type)
	{
		if (!Match(type))
			throw new Exception($"Expected '{type}'.");
	}

	private bool Match(params SyntaxTokenType[] types)
	{
		foreach (var type in types)
		{
			if (Next.Type != type)
				continue;
			
			Advance();
			return true;
		}

		return false;
	}

	private bool Match([NotNullWhen(true)] out SyntaxToken? token, params SyntaxTokenType[] types)
	{
		token = null;
		
		foreach (var type in types)
		{
			if (Next.Type != type)
				continue;
			
			token = Advance();
			return true;
		}

		return false;
	}

	private SyntaxToken Peek(int offset = 0)
	{
		var index = position + offset;
		if (index < 0 || index >= tokens.Length)
			return tokens[^1];

		return tokens[index];
	}

	private static ImmutableArray<SyntaxToken> ParseTokens(Scanner scanner)
	{
		var builder = ImmutableArray.CreateBuilder<SyntaxToken>();

		SyntaxToken token;
		do
		{
			token = scanner.ScanToken();
			builder.Add(token);
		} while (token.Type != SyntaxTokenType.EndOfFile);

		return builder.ToImmutable();
	}

	private Expression ParseExpression()
	{
		return ParseTerm();
	}

	private Expression ParseTerm()
	{
		var left = ParseFactor();

		while (Match(out var operatorToken, SyntaxTokenType.Plus, SyntaxTokenType.Minus))
		{
			var right = ParseFactor();
			left = new BinaryExpression(left, operatorToken, right);
		}

		return left;
	}

	private Expression ParseFactor()
	{
		var left = ParseUnary();

		while (Match(out var operatorToken, SyntaxTokenType.Star, SyntaxTokenType.Slash))
		{
			var right = ParseUnary();
			left = new BinaryExpression(left, operatorToken, right);
		}

		return left;
	}

	private Expression ParseUnary()
	{
		if (!Match(out var operatorToken, SyntaxTokenType.Plus, SyntaxTokenType.Minus))
			return ParsePrimary();
		
		var operand = ParseUnary();
		return new UnaryExpression(operand, operatorToken);
	}

	private Expression ParsePrimary()
	{
		if (Match(out var intToken, SyntaxTokenType.IntegerLiteral))
		{
			return new LiteralExpression(intToken);
		}

		if (!Match(SyntaxTokenType.OpenParen))
		{
			var token = Advance();
			Diagnostics.ReportParserExpectedExpression(token.Span, token);
			return new LiteralExpression(token);
		}
		
		var expression = ParseExpression();
		Consume(SyntaxTokenType.CloseParen);

		return expression;
	}
}