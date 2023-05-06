using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using GuildScript.Analysis.Syntax;
using GuildScript.Analysis.Text;

namespace GuildScript.Analysis;

public sealed class Parser
{
	public DiagnosticCollection Diagnostics { get; }
	private bool suppressErrors;
	
	private SyntaxToken Next => Peek();
	private bool EndOfFile => Next.Type == SyntaxTokenType.EndOfFile;

	private readonly ImmutableArray<SyntaxToken> tokens;
	private int position;

	public Parser(string source)
	{
		Diagnostics = new DiagnosticCollection();
		var scanner = new Lexer(source);
		tokens = ParseTokens(scanner);

		/*Console.ForegroundColor = ConsoleColor.DarkCyan;
		foreach (var token in tokens)
		{
			Console.WriteLine($"{token.Type}: {token.Text} = '{token.Value ?? "null"}'");
		}
		Console.ResetColor();*/

		Diagnostics.AppendDiagnostics(scanner.Diagnostics);
	}

	public class ParseException : Exception
	{
		public ParseException(string message) : base(message)
		{
		}
	}

	public ParseException Error(string message)
	{
		var errorMessage = "Error at " + (EndOfFile ? "end: " : $"'{Peek().Text}': ") + message;

		if (suppressErrors)
			return new ParseException(errorMessage);
		
		//Diagnostics.Report(errorMessage);
		Console.ForegroundColor = ConsoleColor.DarkRed;
		Console.WriteLine(errorMessage);
		Console.ResetColor();

		return new ParseException(errorMessage);
	}

	public SyntaxTree? Parse()
	{
		if (Diagnostics.Any())
			return null;

		try
		{
			var statement = ParseProgram();
			return new SyntaxTree(statement);
		}
		catch (ParseException)
		{
			return null;
		}
	}

	private SyntaxToken Advance()
	{
		var token = Next;

		if (position < tokens.Length)
			position++;
		
		return token;
	}

	private SyntaxToken Consume(SyntaxTokenType type)
	{
		if (!Match(out var token, type))
			throw Error($"Expected '{type}'.");

		return token;
	}

	private SyntaxToken Consume(params SyntaxTokenType[] types)
	{
		foreach (var type in types)
		{
			if (Match(out var token, type))
				return token;
		}
		
		throw Error($"Expected one of '{string.Join("', '", types)}'.");
	}
	
	private bool Check(params SyntaxTokenType[] types)
	{
		foreach (var type in types)
		{
			if (Next.Type != type)
				continue;
			
			return true;
		}

		return false;
	}

	private bool Match(params SyntaxTokenType[] types)
	{
		if (!Check(types))
			return false;
		
		Advance();
		return true;

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

	private static ImmutableArray<SyntaxToken> ParseTokens(Lexer lexer)
	{
		var builder = ImmutableArray.CreateBuilder<SyntaxToken>();

		SyntaxToken token;
		do
		{
			token = lexer.ScanToken();
			builder.Add(token);
		} while (token.Type != SyntaxTokenType.EndOfFile);

		return builder.ToImmutable();
	}

	private Statement.Program ParseProgram()
	{
		// <import-statement>*
		var importStatements = new List<ModuleName>();
		while (Match(SyntaxTokenType.Import))
		{
			var importModuleName = ParseModule();
			importStatements.Add(importModuleName);

			Consume(SyntaxTokenType.Semicolon);
		}

		// <module-statement>
		Consume(SyntaxTokenType.Module);
		var module = ParseModule();
		Consume(SyntaxTokenType.Semicolon);

		// <top-level-statement>*
		var statements = new List<Statement>();
		while (!EndOfFile)
		{
			statements.Add(ParseTopLevelStatement());
		}

		return new Statement.Program(importStatements.ToArray(), module, statements.ToArray());
	}
	
	private ModuleName ParseModule()
	{
		var identifiers = new List<string>();

		var identifierToken = Consume(SyntaxTokenType.Identifier);
		identifiers.Add(identifierToken.Text);

		while (Match(SyntaxTokenType.Dot))
		{
			identifierToken = Consume(SyntaxTokenType.Identifier);
			identifiers.Add(identifierToken.Text);
		}

		return new ModuleName(identifiers);
	}
	
	private Statement ParseTopLevelStatement()
	{
		return Next.Type switch
		{
			SyntaxTokenType.Entry  => ParseEntryPoint(),
			//SyntaxTokenType.Module => ParseModuleDeclaration(),
			SyntaxTokenType.Define => ParseDefineStatement(),
			_					   => ParseTypeDeclaration()
		};
	}
	
	private Statement.EntryPoint ParseEntryPoint()
	{
		Consume(SyntaxTokenType.Entry);
		var returnType = Match(SyntaxTokenType.Void) ? null : ParseType();
		var identifier = Consume(SyntaxTokenType.Identifier);

		Consume(SyntaxTokenType.OpenParen);

		var parameterList = new List<Variable>();
		if (!Check(SyntaxTokenType.CloseParen))
		{
			parameterList.AddRange(ParseParameterList());
		}

		Consume(SyntaxTokenType.CloseParen);

		var body = ParseBlock();

		return new Statement.EntryPoint(returnType, identifier, parameterList, body);
	}

	private Statement.Define ParseDefineStatement()
	{
		Consume(SyntaxTokenType.Define);
		var identifier = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.As);
		var type = ParseType();
		Consume(SyntaxTokenType.Semicolon);

		return new Statement.Define(identifier, type);
	}

	private Statement.Block ParseBlock()
	{
		Consume(SyntaxTokenType.OpenBrace);
		
		var statements = new List<Statement>();

		while (!EndOfFile && !Check(SyntaxTokenType.CloseBrace))
		{
			statements.Add(ParseStatement());
		}
		
		Consume(SyntaxTokenType.CloseBrace);

		return new Statement.Block(statements);
	}

	private Statement.Control ParseControlStatement()
	{
		Consume(SyntaxTokenType.If);
		
		Consume(SyntaxTokenType.OpenParen);
		var ifExpression = ParseExpression();
		Consume(SyntaxTokenType.CloseParen);

		var ifStatement = ParseStatement();

		Statement? elseStatement = null;
		if (Match(SyntaxTokenType.Else))
		{
			elseStatement = ParseStatement();
		}

		return new Statement.Control(ifExpression, ifStatement, elseStatement);
	}

	private Statement ParseLoopStatement()
	{
		if (Check(SyntaxTokenType.While))
		{
			return ParseWhileStatement();
		}
		
		if (Check(SyntaxTokenType.Do))
		{
			return ParseDoWhileStatement();
		}
		
		if (Check(SyntaxTokenType.For))
		{
			return ParseForStatement();
		}
		
		if (Check(SyntaxTokenType.Foreach))
		{
			return ParseForEachStatement();
		}
		
		if (Check(SyntaxTokenType.Repeat))
		{
			return ParseRepeatStatement();
		}

		throw Error("Invalid loop statement.");
	}

	private Statement.Lock ParseLockStatement()
	{
		Consume(SyntaxTokenType.Lock);
		Consume(SyntaxTokenType.OpenParen);
		var expression = ParseExpression();
		Consume(SyntaxTokenType.CloseParen);
		var body = ParseStatement();

		return new Statement.Lock(expression, body);
	}

	private Statement.Return ParseReturnStatement()
	{
		Consume(SyntaxTokenType.Return);

		Expression? expression = null;
		if (!Check(SyntaxTokenType.Semicolon))
		{
			expression = ParseExpression();
		}

		Consume(SyntaxTokenType.Semicolon);
		return new Statement.Return(expression);
	}

	private Statement.Throw ParseThrowStatement()
	{
		Consume(SyntaxTokenType.Return);

		Expression? expression = null;
		if (!Check(SyntaxTokenType.Semicolon))
		{
			expression = ParseExpression();
		}

		Consume(SyntaxTokenType.Semicolon);
		return new Statement.Throw(expression);
	}

	private Statement.Try ParseTryStatement()
	{
		Consume(SyntaxTokenType.Try);
		var tryStatement = ParseStatement();

		TypeSyntax? catchType = null;
		SyntaxToken? catchIdentifier = null;
		Statement? catchStatement = null;
		
		if (Match(SyntaxTokenType.Catch))
		{
			Consume(SyntaxTokenType.OpenParen);
			catchType = ParseType();
			catchIdentifier = Consume(SyntaxTokenType.Identifier);
			Consume(SyntaxTokenType.CloseParen);

			catchStatement = ParseStatement();
		}

		Statement? finallyStatement = null;
		if (Match(SyntaxTokenType.Finally))
		{
			finallyStatement = ParseStatement();
		}

		return new Statement.Try(tryStatement, catchType, catchIdentifier, catchStatement, finallyStatement);
	}

	private Statement.Seal ParseSealStatement()
	{
		Consume(SyntaxTokenType.Seal);
		var identifier = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.Semicolon);

		return new Statement.Seal(identifier);
	}

	private Statement.Switch ParseSwitchStatement()
	{
		Consume(SyntaxTokenType.Switch);
		Consume(SyntaxTokenType.OpenParen);
		var expression = ParseExpression();
		Consume(SyntaxTokenType.CloseParen);

		// Sections
		Consume(SyntaxTokenType.OpenBrace);

		var sections = new List<Statement.Switch.Section>();
		while (Match(out var caseToken, SyntaxTokenType.Case, SyntaxTokenType.Default))
		{
			var labels = new List<Statement.Switch.Label>();
			switch (caseToken.Type)
			{
				case SyntaxTokenType.Case:
					Consume(SyntaxTokenType.OpenParen);
					if (IsVariableDeclaration())
					{
						var type = ParseType();

						if (type is null)
							throw Error("Cannot use void in pattern matching.");
						
						var identifier = Match(out var token, SyntaxTokenType.Identifier) ? token : null;
						
						labels.Add(new Statement.Switch.PatternLabel(type, identifier));
					}
					else
					{
						var expressionLabel = ParseExpression();
						labels.Add(new Statement.Switch.ExpressionLabel(expressionLabel));
					}
					Consume(SyntaxTokenType.CloseParen);
					break;
				case SyntaxTokenType.Default:
					labels.Add(Statement.Switch.Label.Default);
					break;
				default:
					throw Error("Invalid case token.");
			}

			var body = ParseStatement();
			sections.Add(new Statement.Switch.Section(labels, body));
		}
		
		Consume(SyntaxTokenType.CloseBrace);
		return new Statement.Switch(expression, sections);
	}

	private Statement.VariableDeclaration ParseVariableDeclarationStatement()
	{
		var type = Match(SyntaxTokenType.Var) ? null : ParseType();
		var identifier = Consume(SyntaxTokenType.Identifier);
		var initializer = Match(SyntaxTokenType.Equal) ? ParseExpression() : null;
		Consume(SyntaxTokenType.Semicolon);

		return new Statement.VariableDeclaration(type, identifier, initializer);
	}

	private Statement.ExpressionStatement ParseExpressionStatement()
	{
		var expression = ParseExpression();
		Consume(SyntaxTokenType.Semicolon);

		return new Statement.ExpressionStatement(expression);
	}

	private Statement.While ParseWhileStatement()
	{
		Consume(SyntaxTokenType.While);
		Consume(SyntaxTokenType.OpenParen);
		var condition = ParseExpression();
		Consume(SyntaxTokenType.CloseParen);
		var body = ParseStatement();

		return new Statement.While(condition, body);
	}

	private Statement.DoWhile ParseDoWhileStatement()
	{
		Consume(SyntaxTokenType.Do);
		var body = ParseStatement();
		Consume(SyntaxTokenType.While);
		Consume(SyntaxTokenType.OpenParen);
		var condition = ParseExpression();
		Consume(SyntaxTokenType.CloseParen);
		Consume(SyntaxTokenType.Semicolon);

		return new Statement.DoWhile(body, condition);
	}

	private Statement.For ParseForStatement()
	{
		Consume(SyntaxTokenType.For);
		Consume(SyntaxTokenType.OpenParen);

		Statement? initializer = null;
		if (!Check(SyntaxTokenType.Semicolon))
		{
			initializer = ParseStatement();
		}

		Consume(SyntaxTokenType.Semicolon);
		
		Expression? condition = null;
		if (!Check(SyntaxTokenType.Semicolon))
		{
			condition = ParseExpression();
		}
		
		Consume(SyntaxTokenType.Semicolon);
		
		Statement? increment = null;
		if (!Check(SyntaxTokenType.CloseParen))
		{
			increment = ParseStatement();
		}

		Consume(SyntaxTokenType.CloseParen);

		var body = ParseStatement();

		return new Statement.For(initializer, condition, increment, body);
	}

	private Statement.ForEach ParseForEachStatement()
	{
		Consume(SyntaxTokenType.Foreach);
		Consume(SyntaxTokenType.OpenParen);

		TypeSyntax? iteratorType = null;
		if (!Match(SyntaxTokenType.Var))
		{
			iteratorType = ParseType();
		}

		var iterator = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.In);
		var enumerable = ParseExpression();
		Consume(SyntaxTokenType.CloseParen);
		var body = ParseStatement();

		return new Statement.ForEach(iteratorType, iterator, enumerable, body);
	}

	private Statement.Repeat ParseRepeatStatement()
	{
		Consume(SyntaxTokenType.Repeat);
		Consume(SyntaxTokenType.OpenParen);
		var repetitions = ParseExpression();
		Consume(SyntaxTokenType.CloseParen);
		var body = ParseStatement();

		return new Statement.Repeat(repetitions, body);
	}
	
	private Statement ParseTypeDeclaration(SyntaxToken? accessModifier)
	{
		if (Match(SyntaxTokenType.Interface))
		{
			return ParseInterface(accessModifier);
		}
		
		if (Match(SyntaxTokenType.Enum))
		{
			return ParseEnum(accessModifier);
		}

		if (Check(SyntaxTokenType.Struct) || Peek(1).Type == SyntaxTokenType.Struct)
		{
			return ParseStruct(accessModifier);
		}

		return ParseClass(accessModifier);
	}

	private Statement ParseTypeDeclaration()
	{
		var accessModifier = ParseAccessModifier();
		return ParseTypeDeclaration(accessModifier);
	}

	private SyntaxToken? ParseAccessModifier()
	{
		return Match(out var token, SyntaxTokenType.Public, SyntaxTokenType.Private, SyntaxTokenType.Protected,
			SyntaxTokenType.Internal) ? token : null;
	}

	private Statement.Interface ParseInterface(SyntaxToken? accessModifier)
	{
		var identifier = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.OpenBrace);

		var members = new List<Statement>();
		while (!EndOfFile && !Check(SyntaxTokenType.CloseBrace))
		{
			members.Add(ParseInterfaceMember());
		}
		
		Consume(SyntaxTokenType.CloseBrace);

		return new Statement.Interface(accessModifier, identifier, members);
	}
	
	private Statement.Enum ParseEnum(SyntaxToken? accessModifier)
	{
		var type = ParseNamedType();
		var identifier = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.OpenBrace);

		var members = new List<Statement.Enum.Member>();
		
		while (!EndOfFile && !Check(SyntaxTokenType.CloseBrace))
		{
			var memberIdentifier = Consume(SyntaxTokenType.Identifier);
			Expression? memberExpression = null;

			if (Match(SyntaxTokenType.Equal))
			{
				memberExpression = ParseExpression();
			}
			
			members.Add(new Statement.Enum.Member(memberIdentifier, memberExpression));

			if (!Match(SyntaxTokenType.Comma))
				break;
		}
		
		Consume(SyntaxTokenType.CloseBrace);

		return new Statement.Enum(accessModifier, identifier, members, type);
	}

	private Statement.Struct ParseStruct(SyntaxToken? accessModifier)
	{
		var structModifier = Match(out var token, SyntaxTokenType.Immutable) ? token : null;
		Consume(SyntaxTokenType.Struct);
		var identifier = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.OpenBrace);

		var members = new List<Statement>();
		while (!EndOfFile && !Check(SyntaxTokenType.CloseBrace))
		{
			members.Add(ParseMember());
		}
		
		Consume(SyntaxTokenType.CloseBrace);

		return new Statement.Struct(accessModifier, structModifier, identifier, members);
	}

	private Statement.Class ParseClass(SyntaxToken? accessModifier)
	{
		var classModifier =
			Match(out var token, SyntaxTokenType.Global, SyntaxTokenType.Template, SyntaxTokenType.Final)
				? token
				: null;
		
		Consume(SyntaxTokenType.Class);
		var identifier = Consume(SyntaxTokenType.Identifier);
		var typeParameters = new List<SyntaxToken>();

		if (Match(SyntaxTokenType.LeftAngled))
		{
			do
			{
				typeParameters.Add(Consume(SyntaxTokenType.Identifier));
			} while (Match(SyntaxTokenType.Comma));

			Consume(SyntaxTokenType.RightAngled);
		}

		TypeSyntax? baseType = null;
		if (Match(SyntaxTokenType.Colon))
		{
			baseType = ParseType();
		}
		
		Consume(SyntaxTokenType.OpenBrace);

		var members = new List<Statement>();
		while (!EndOfFile && !Check(SyntaxTokenType.CloseBrace))
		{
			members.Add(ParseMember());
		}
		
		Consume(SyntaxTokenType.CloseBrace);

		return new Statement.Class(accessModifier, classModifier, identifier, typeParameters, baseType, members);
	}

	private bool IsOperatorOverload()
	{
		var previousPosition = position;

		try
		{
			suppressErrors = true;
			Consume(SyntaxTokenType.OpenSquare);
			var tokenCount = 0;

			while (!EndOfFile && !Match(SyntaxTokenType.CloseSquare))
			{
				tokenCount++;
				Advance();
			}
			
			ParseType();

			return tokenCount > 0;
		}
		catch
		{
			return false;
		}
		finally
		{
			suppressErrors = false;
			position = previousPosition;
		}
	}

	private bool IsVariableDeclaration()
	{
		var previousPosition = position;

		try
		{
			suppressErrors = true;
			ParseType();
			return Next.Type == SyntaxTokenType.Identifier;
		}
		catch
		{
			return false;
		}
		finally
		{
			suppressErrors = false;
			position = previousPosition;
		}
	}

	private TypeSyntax? ParseType()
	{
		if (Match(SyntaxTokenType.Void))
			return null;
		
		if (Check(SyntaxTokenType.OpenSquare))
		{
			return ParseLambdaType();
		}

		var type = ParseNamedType();
		while (!EndOfFile)
		{
			if (Match(SyntaxTokenType.Question))
			{
				type = new NullableTypeSyntax(type);
			}
			else if (Match(SyntaxTokenType.OpenSquare))
			{
				type = new ArrayTypeSyntax(type);
				Consume(SyntaxTokenType.CloseSquare);
			}
			else if (type is NamedTypeSyntax namedTypeSyntax && Match(SyntaxTokenType.LeftAngled))
			{
				var templateTypes = new List<TypeSyntax>();

				do
				{
					var newType = ParseType();
					if (newType is null)
						throw Error("Cannot template void.");
					
					templateTypes.Add(newType);
				} while (Match(SyntaxTokenType.Comma));
				
				type = new TemplatedTypeSyntax(namedTypeSyntax, templateTypes);
				Consume(SyntaxTokenType.RightAngled);
			}
			else
			{
				break;
			}
		}

		return type;
	}

	private IEnumerable<Variable> ParseParameterList(bool parseInitializers = false)
	{
		var parameterList = new List<Variable>();

		do
		{
			var isReference = Match(SyntaxTokenType.Ref);
			var type = ParseType();
			var identifier = Consume(SyntaxTokenType.Identifier);

			if (type is null)
				throw Error("Cannot use void as a parameter type.");

			Expression? initializer = null;
			if (parseInitializers)
			{
				if (Match(SyntaxTokenType.Equal))
				{
					initializer = ParseExpression();
				}
			}
			
			parameterList.Add(new Variable(type, identifier, isReference, initializer));
		} while (Match(SyntaxTokenType.Comma));

		return parameterList;
	}

	private IEnumerable<Expression> ParseArgumentList()
	{
		var argumentList = new List<Expression>();

		do
		{
			var expression = ParseExpression();
			
			argumentList.Add(expression);
		} while (Match(SyntaxTokenType.Comma));

		return argumentList;
	}

	private LambdaTypeSyntax ParseLambdaType()
	{
		Consume(SyntaxTokenType.OpenSquare);

		var inputTypes = new List<TypeSyntax>();
		if (!Check(SyntaxTokenType.CloseSquare))
		{
			do
			{
				var type = ParseType();
				if (type is null)
					throw Error("Cannot use void as a lambda parameter type.");
				
				inputTypes.Add(type);
			} while (Match(SyntaxTokenType.Comma));
		}

		Consume(SyntaxTokenType.CloseSquare);

		if (Match(SyntaxTokenType.LeftTriangle))
		{
			Consume(SyntaxTokenType.OpenSquare);
			var outputType = ParseType();
			Consume(SyntaxTokenType.CloseSquare);
			
			return new LambdaTypeSyntax(inputTypes, outputType);
		}
		
		Consume(SyntaxTokenType.RightTriangle);
		return new LambdaTypeSyntax(inputTypes);
	}

	private TypeSyntax ParseNamedType()
	{
		TypeSyntax? type = Next.Type switch
		{
			SyntaxTokenType.Int8       => TypeSyntax.Int8,
			SyntaxTokenType.UInt8      => TypeSyntax.UInt8,
			SyntaxTokenType.Int16      => TypeSyntax.Int16,
			SyntaxTokenType.UInt16     => TypeSyntax.UInt16,
			SyntaxTokenType.Int32      => TypeSyntax.Int32,
			SyntaxTokenType.UInt32     => TypeSyntax.UInt32,
			SyntaxTokenType.Int64      => TypeSyntax.Int64,
			SyntaxTokenType.UInt64     => TypeSyntax.UInt64,
			SyntaxTokenType.String     => TypeSyntax.String,
			SyntaxTokenType.Single     => TypeSyntax.Single,
			SyntaxTokenType.Double     => TypeSyntax.Double,
			SyntaxTokenType.Bool       => TypeSyntax.Bool,
			SyntaxTokenType.Char       => TypeSyntax.Char,
			SyntaxTokenType.Object     => TypeSyntax.Object,
			//SyntaxTokenType.Identifier => new NamedTypeSyntax(Next.Text, false),
			_                          => null
		};

		if (type is null)
			return new ExpressionTypeSyntax(ParsePrimaryExpression());
		
		Advance();
		return type;

	}

	private SyntaxToken ParseQualifier()
	{
		return Consume(
			SyntaxTokenType.Int8,
			SyntaxTokenType.UInt8,
			SyntaxTokenType.Int16,
			SyntaxTokenType.UInt16,
			SyntaxTokenType.Int32,
			SyntaxTokenType.UInt32,
			SyntaxTokenType.Int64,
			SyntaxTokenType.UInt64,
			SyntaxTokenType.String,
			SyntaxTokenType.Single,
			SyntaxTokenType.Double,
			SyntaxTokenType.Bool,
			SyntaxTokenType.Char,
			SyntaxTokenType.Object,
			SyntaxTokenType.Identifier,
			SyntaxTokenType.This,
			SyntaxTokenType.Base);
	}

	private Statement ParseStatement()
	{
		if (Check(SyntaxTokenType.OpenBrace))
		{
			return ParseBlock();
		}

		if (Match(SyntaxTokenType.Break))
		{
			Consume(SyntaxTokenType.Semicolon);
			return new Statement.Break();
		}

		if (Match(SyntaxTokenType.Continue))
		{
			Consume(SyntaxTokenType.Semicolon);
			return new Statement.Continue();
		}

		if (Check(SyntaxTokenType.If))
		{
			return ParseControlStatement();
		}

		if (Check(SyntaxTokenType.While, SyntaxTokenType.Do, SyntaxTokenType.For, SyntaxTokenType.Foreach,
				SyntaxTokenType.Repeat))
		{
			return ParseLoopStatement();
		}

		if (Check(SyntaxTokenType.Lock))
		{
			return ParseLockStatement();
		}

		if (Check(SyntaxTokenType.Return))
		{
			return ParseReturnStatement();
		}

		if (Check(SyntaxTokenType.Throw))
		{
			return ParseThrowStatement();
		}

		if (Check(SyntaxTokenType.Try))
		{
			return ParseTryStatement();
		}

		if (Check(SyntaxTokenType.Seal))
		{
			return ParseSealStatement();
		}

		if (Check(SyntaxTokenType.Switch))
		{
			return ParseSwitchStatement();
		}

		if (Check(SyntaxTokenType.Var) || IsVariableDeclaration())
		{
			return ParseVariableDeclarationStatement();
		}

		return ParseExpressionStatement();
	}

	private Statement ParseMember()
	{
		if (Match(out var destructorToken, SyntaxTokenType.Destructor))
		{
			return ParseDestructor(destructorToken);
		}

		if (Match(SyntaxTokenType.External))
		{
			return ParseExternalMethod();
		}

		if (IsOperatorOverload())
		{
			return ParseOperatorOverload();
		}
		
		var accessModifier = ParseAccessModifier();

		if (Match(out var constructorToken, SyntaxTokenType.Constructor))
		{
			return ParseConstructor(constructorToken, accessModifier);
		}

		var peek = 0;
		var continuePeek = true;
		
		while (continuePeek)
		{
			var token = Peek(peek++);
			switch (token.Type)
			{
				case SyntaxTokenType.This:
					return ParseIndexer(accessModifier);
				case SyntaxTokenType.Event:
					return ParseEvent(accessModifier);
				case SyntaxTokenType.OpenBrace:
					return ParseProperty(accessModifier);
				case SyntaxTokenType.OpenParen:
					return ParseMethod(accessModifier);
				case SyntaxTokenType.Semicolon:
					return ParseField(accessModifier);
				case SyntaxTokenType.Class:
				case SyntaxTokenType.Struct:
				case SyntaxTokenType.Interface:
				case SyntaxTokenType.Enum:
					return ParseTypeDeclaration(accessModifier);
				case SyntaxTokenType.EndOfFile:
					continuePeek = false;
					break;
				default:
					continue;
			}
		}

		throw Error("Expected member.");
	}

	private Statement ParseInterfaceMember()
	{
		var peek = 0;
		var continuePeek = true;
		
		while (continuePeek)
		{
			var token = Peek(peek++);
			switch (token.Type)
			{
				case SyntaxTokenType.OpenBrace:
					return ParsePropertySignature();
				case SyntaxTokenType.OpenParen:
					return ParseMethodSignature();
				case SyntaxTokenType.EndOfFile:
					continuePeek = false;
					break;
				default:
					continue;
			}
		}

		throw Error("Expected member.");
	}

	private Statement.Destructor ParseDestructor(SyntaxToken destructorToken)
	{
		var body = ParseBlock();
		return new Statement.Destructor(body, destructorToken);
	}

	private Statement.ExternalMethod ParseExternalMethod()
	{
		var returnType = ParseType();
		var identifier = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.OpenParen);

		var parameterList = new List<Variable>();
		if (!Check(SyntaxTokenType.CloseParen))
		{
			parameterList.AddRange(ParseParameterList());
		}
		
		Consume(SyntaxTokenType.CloseParen);
		Consume(SyntaxTokenType.Semicolon);
		return new Statement.ExternalMethod(returnType, identifier, parameterList);
	}

	private Statement.Constructor ParseConstructor(SyntaxToken constructorToken, SyntaxToken? accessModifier)
	{
		Consume(SyntaxTokenType.OpenParen);
		var parameterList = new List<Variable>();
		if (!Check(SyntaxTokenType.CloseParen))
		{
			parameterList.AddRange(ParseParameterList());
		}
		Consume(SyntaxTokenType.CloseParen);

		SyntaxToken? initializer = null;
		var argumentList = new List<Expression>();
		if (Match(SyntaxTokenType.Colon))
		{
			initializer = Consume(SyntaxTokenType.This, SyntaxTokenType.Base);
			Consume(SyntaxTokenType.OpenParen);

			if (!Check(SyntaxTokenType.CloseParen))
				argumentList.AddRange(ParseArgumentList());
			
			Consume(SyntaxTokenType.CloseParen);
		}

		var body = ParseBlock();

		return new Statement.Constructor(accessModifier, parameterList, body, initializer, argumentList,
			constructorToken);
	}

	private Statement.Indexer ParseIndexer(SyntaxToken? accessModifier)
	{
		var type = ParseType();
		if (type is null)
			throw Error("Cannot use void as indexer type.");
		
		Consume(SyntaxTokenType.This);
		Consume(SyntaxTokenType.OpenSquare);

		var parameterList = new List<Variable>();
		if (!Check(SyntaxTokenType.CloseSquare))
		{
			parameterList.AddRange(ParseParameterList());
		}

		Consume(SyntaxTokenType.CloseSquare);
		Consume(SyntaxTokenType.OpenBrace);

		var body = new List<Statement>();
		while (!EndOfFile && !Check(SyntaxTokenType.CloseBrace))
		{
			body.Add(ParsePropertyAccessor());
		}

		Consume(SyntaxTokenType.CloseBrace);
		return new Statement.Indexer(accessModifier, type, parameterList, body);
	}

	private Statement.Event ParseEvent(SyntaxToken? accessModifier)
	{
		SyntaxToken? eventModifier = null;

		if (Match(out var modifierToken, SyntaxTokenType.Global))
		{
			eventModifier = modifierToken;
		}

		Consume(SyntaxTokenType.Event);
		var identifier = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.OpenParen);

		var parameterList = new List<Variable>();
		if (!Check(SyntaxTokenType.CloseParen))
		{
			parameterList.AddRange(ParseParameterList());
		}
		
		Consume(SyntaxTokenType.CloseParen);
		Consume(SyntaxTokenType.Semicolon);

		return new Statement.Event(accessModifier, eventModifier, identifier, parameterList);
	}

	private IEnumerable<SyntaxToken> ParseMethodModifiers()
	{
		var modifiers = new List<SyntaxToken>();

		while (Match(out var modifier, SyntaxTokenType.Global, SyntaxTokenType.Prototype, SyntaxTokenType.Required,
				   SyntaxTokenType.Immutable))
		{
			modifiers.Add(modifier);
		}

		return modifiers;
	}

	private IEnumerable<SyntaxToken> ParseFieldModifiers()
	{
		var modifiers = new List<SyntaxToken>();

		while (Match(out var modifier, SyntaxTokenType.Global, SyntaxTokenType.Constant, SyntaxTokenType.Fixed,
				   SyntaxTokenType.Immutable, SyntaxTokenType.Final))
		{
			modifiers.Add(modifier);
		}

		return modifiers;
	}

	private Statement.Property ParseProperty(SyntaxToken? accessModifier)
	{
		var modifiers = ParseMethodModifiers();
		var type = ParseType();

		if (type is null)
			throw Error("Property type cannot be void.");
		
		var identifier = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.OpenBrace);

		var body = new List<Statement>();
		while (!EndOfFile && !Check(SyntaxTokenType.CloseBrace))
		{
			body.Add(ParsePropertyAccessor());
		}
		
		Consume(SyntaxTokenType.CloseBrace);

		return new Statement.Property(accessModifier, modifiers, type, identifier, body);
	}

	private Statement.PropertySignature ParsePropertySignature()
	{
		var modifiers = ParseMethodModifiers();
		var type = ParseType();

		if (type is null)
			throw Error("Property type cannot be void.");
		
		var identifier = Consume(SyntaxTokenType.Identifier);
		Consume(SyntaxTokenType.OpenBrace);

		var body = new List<Statement>();
		while (!EndOfFile && !Check(SyntaxTokenType.CloseBrace))
		{
			body.Add(ParsePropertyAccessorSignature());
		}
		
		Consume(SyntaxTokenType.CloseBrace);

		return new Statement.PropertySignature(modifiers, type, identifier, body);
	}

	private Statement.Method ParseMethod(SyntaxToken? accessModifier)
	{
		var asyncToken = Match(out var token, SyntaxTokenType.Async) ? token : null;
		var modifiers = ParseMethodModifiers();
		var type = ParseType();

		var identifier = Consume(SyntaxTokenType.Identifier);

		var typeParameters = new List<SyntaxToken>();
		if (Match(SyntaxTokenType.LeftAngled))
		{
			do
			{
				typeParameters.Add(Consume(SyntaxTokenType.Identifier));
			} while (!EndOfFile && !Check(SyntaxTokenType.RightAngled));
			
			Consume(SyntaxTokenType.RightAngled);
		}
		
		Consume(SyntaxTokenType.OpenParen);
		var parameterList = new List<Variable>();
		if (!Check(SyntaxTokenType.CloseParen))
		{
			parameterList.AddRange(ParseParameterList());
		}
		Consume(SyntaxTokenType.CloseParen);

		var body = ParseBlock();

		return new Statement.Method(accessModifier, modifiers, type, identifier, body, parameterList, asyncToken,
			typeParameters);
	}

	private Statement.OperatorOverload ParseOperatorOverload()
	{
		Consume(SyntaxTokenType.OpenSquare);

		var operatorTokens = new List<SyntaxToken>();
		while (!EndOfFile && !Check(SyntaxTokenType.CloseSquare))
		{
			operatorTokens.Add(Advance());
		}
		
		Consume(SyntaxTokenType.CloseSquare);
		
		var returnType = ParseType();
		if (returnType is null)
			throw Error("Operator overloads must have a return type.");
		
		Consume(SyntaxTokenType.OpenParen);
		var parameterList = new List<Variable>();
		if (!Check(SyntaxTokenType.CloseParen))
		{
			parameterList.AddRange(ParseParameterList());
		}
		Consume(SyntaxTokenType.CloseParen);

		var body = ParseBlock();
		var @operator = new BinaryOperator(new SyntaxTokenSpan(operatorTokens));
		return new Statement.OperatorOverload(returnType, @operator, parameterList, body);
	}

	private Statement.MethodSignature ParseMethodSignature()
	{
		var asyncToken = Match(out var token, SyntaxTokenType.Async) ? token : null;
		var modifiers = ParseMethodModifiers();
		var type = ParseType();

		var identifier = Consume(SyntaxTokenType.Identifier);

		var typeParameters = new List<SyntaxToken>();
		if (Match(SyntaxTokenType.LeftAngled))
		{
			do
			{
				typeParameters.Add(Consume(SyntaxTokenType.Identifier));
			} while (!EndOfFile && !Check(SyntaxTokenType.RightAngled));
			
			Consume(SyntaxTokenType.RightAngled);
		}
		
		Consume(SyntaxTokenType.OpenParen);
		var parameterList = new List<Variable>();
		if (!Check(SyntaxTokenType.CloseParen))
		{
			parameterList.AddRange(ParseParameterList());
		}
		Consume(SyntaxTokenType.CloseParen);

		Consume(SyntaxTokenType.Semicolon);

		return new Statement.MethodSignature(modifiers, type, identifier, parameterList, asyncToken, typeParameters);
	}

	private Statement.Field ParseField(SyntaxToken? accessModifier)
	{
		var modifiers = ParseFieldModifiers();
		var type = ParseType();

		var identifier = Consume(SyntaxTokenType.Identifier);

		Expression? initializer = null;
		if (Match(SyntaxTokenType.Equal))
		{
			initializer = ParseExpression();
		}

		Consume(SyntaxTokenType.Semicolon);

		return new Statement.Field(accessModifier, modifiers, type, identifier, initializer);
	}

	private Statement.Accessor ParsePropertyAccessor()
	{
		var accessModifier = ParseAccessModifier();

		if (Match(out var accessorToken, SyntaxTokenType.Get, SyntaxTokenType.Set))
		{
			Consume(SyntaxTokenType.Semicolon);
			return new Statement.AccessorToken(accessModifier, accessorToken);
		}

		var lambda = ParseLambdaExpression();
		Consume(SyntaxTokenType.Semicolon);
		return new Statement.AccessorLambda(accessModifier, lambda);
	}

	private Statement.Accessor ParsePropertyAccessorSignature()
	{
		var accessModifier = ParseAccessModifier();

		if (Match(out var accessorToken, SyntaxTokenType.Get, SyntaxTokenType.Set))
		{
			Consume(SyntaxTokenType.Semicolon);
			return new Statement.AccessorToken(accessModifier, accessorToken);
		}

		var signature = ParseLambdaType();
		Consume(SyntaxTokenType.Semicolon);
		return new Statement.AccessorLambdaSignature(accessModifier, signature);
	}

	private Expression ParseExpression()
	{
		return Match(SyntaxTokenType.Await) 
			? new Expression.Await(ParseExpression())
			: ParseAssignmentExpression();
	}

	private Expression ParseAssignmentExpression()
	{
		var left = ParseRangeExpression();

		if (!Match(out var operatorToken, SyntaxTokenType.Equal, SyntaxTokenType.PlusEqual, SyntaxTokenType.MinusEqual,
				SyntaxTokenType.StarEqual, SyntaxTokenType.SlashEqual, SyntaxTokenType.StarStarEqual,
				SyntaxTokenType.PercentEqual, SyntaxTokenType.AmpEqual, SyntaxTokenType.AmpAmpEqual,
				SyntaxTokenType.PipeEqual, SyntaxTokenType.PipePipeEqual, SyntaxTokenType.CaretEqual,
				SyntaxTokenType.CaretCaretEqual, SyntaxTokenType.LeftLeftEqual, SyntaxTokenType.RightRightEqual,
				SyntaxTokenType.LeftLeftLeftEqual, SyntaxTokenType.RightRightRightEqual,
				SyntaxTokenType.QuestionQuestionEqual))
			return left;
		
		var right = ParseExpression();
		return new Expression.Binary(left, new BinaryOperator(operatorToken), right);
	}

	private Expression ParseRangeExpression()
	{
		var left = ParseConditionalExpression();

		if (!Match(out var operatorToken, SyntaxTokenType.LeftArrow, SyntaxTokenType.RightArrow,
				SyntaxTokenType.LeftArrowArrow, SyntaxTokenType.RightArrowArrow))
			return left;
		
		var right = ParseExpression();
		return new Expression.Binary(left, new BinaryOperator(operatorToken), right);
	}

	private Expression ParseConditionalExpression()
	{
		var condition = ParseLogicalOrExpression();
		if (!Match(SyntaxTokenType.Question))
			return condition;

		var trueExpression = ParseExpression();
		Consume(SyntaxTokenType.Colon);
		var falseExpression = ParseExpression();

		return new Expression.Conditional(condition, trueExpression, falseExpression);
	}

	private Expression ParseLogicalOrExpression()
	{
		var left = ParseLogicalXorExpression();

		while (Match(out var operatorToken, SyntaxTokenType.PipePipe))
		{
			var right = ParseLogicalXorExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private Expression ParseLogicalXorExpression()
	{
		var left = ParseLogicalAndExpression();

		while (Match(out var operatorToken, SyntaxTokenType.CaretCaret))
		{
			var right = ParseLogicalAndExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private Expression ParseLogicalAndExpression()
	{
		var left = ParseBitwiseOrExpression();

		while (Match(out var operatorToken, SyntaxTokenType.AmpAmp))
		{
			var right = ParseBitwiseOrExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private Expression ParseBitwiseOrExpression()
	{
		var left = ParseBitwiseXorExpression();

		while (Match(out var operatorToken, SyntaxTokenType.Pipe))
		{
			var right = ParseBitwiseXorExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private Expression ParseBitwiseXorExpression()
	{
		var left = ParseBitwiseAndExpression();

		while (Match(out var operatorToken, SyntaxTokenType.Caret))
		{
			var right = ParseBitwiseAndExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private Expression ParseBitwiseAndExpression()
	{
		var left = ParseEqualityExpression();

		while (Match(out var operatorToken, SyntaxTokenType.Amp))
		{
			var right = ParseEqualityExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private Expression ParseEqualityExpression()
	{
		var left = ParseRelationalExpression();

		while (true) 
		{
			if (Match(out var equalityOperatorToken, SyntaxTokenType.EqualEqual, SyntaxTokenType.BangEqual))
			{
				var right = ParseRelationalExpression();
				left = new Expression.Binary(left, new BinaryOperator(equalityOperatorToken), right);
			}
			else if (Match(out var typeOperatorToken, SyntaxTokenType.QuestionEqual, SyntaxTokenType.QuestionBangEqual))
			{
				if (Match(SyntaxTokenType.Var))
				{
					if (Match(out var identifier, SyntaxTokenType.Identifier))
					{
						left = new Expression.TypeRelation(left, new BinaryOperator(typeOperatorToken),
							TypeSyntax.Inferred, identifier);
					}
					else
					{
						left = new Expression.TypeRelation(left, new BinaryOperator(typeOperatorToken),
							TypeSyntax.Inferred, null);
					}
				}
				else if (Match(SyntaxTokenType.Null))
				{
					left = new Expression.TypeRelation(left, new BinaryOperator(typeOperatorToken), null, null);
				}
				else
				{
					var type = ParseType();
					if (type is null)
						throw Error("Cannot cast to void.");
					
					if (Match(out var identifier, SyntaxTokenType.Identifier))
					{
						left = new Expression.TypeRelation(left, new BinaryOperator(typeOperatorToken), type,
							identifier);
					}
					else
					{
						left = new Expression.TypeRelation(left, new BinaryOperator(typeOperatorToken), type, null);
					}
				}
			}
			else break;
		}

		return left;
	}

	private Expression ParseRelationalExpression()
	{
		var left = ParseShiftExpression();

		while (Match(out var operatorToken, SyntaxTokenType.LeftAngled, SyntaxTokenType.RightAngled,
				   SyntaxTokenType.LeftAngledEqual, SyntaxTokenType.RightAngledEqual))  
		{
			var right = ParseShiftExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private bool MatchShiftOperator([NotNullWhen(true)] out SyntaxTokenSpan? operatorTokenSpan)
	{
		operatorTokenSpan = null;
		var operatorTokens = new List<SyntaxToken>();

		if (Check(SyntaxTokenType.LeftAngled))
		{
			if (Peek(1).Type == SyntaxTokenType.LeftAngled)
			{
				if (Peek(2).Type == SyntaxTokenType.LeftAngled)
				{
					operatorTokens.Add(Advance());
					operatorTokens.Add(Advance());
					operatorTokens.Add(Advance());

					operatorTokenSpan = new SyntaxTokenSpan(operatorTokens);
					return true;
				}
				
				operatorTokens.Add(Advance());
				operatorTokens.Add(Advance());

				operatorTokenSpan = new SyntaxTokenSpan(operatorTokens);
				return true;
			}
		}

		if (!Check(SyntaxTokenType.RightAngled))
			return false;
		
		if (Peek(1).Type != SyntaxTokenType.RightAngled)
			return false;
			
		if (Peek(2).Type == SyntaxTokenType.RightAngled)
		{
			operatorTokens.Add(Advance());
			operatorTokens.Add(Advance());
			operatorTokens.Add(Advance());

			operatorTokenSpan = new SyntaxTokenSpan(operatorTokens);
			return true;
		}
				
		operatorTokens.Add(Advance());
		operatorTokens.Add(Advance());

		operatorTokenSpan = new SyntaxTokenSpan(operatorTokens);
		return true;

	}

	private Expression ParseShiftExpression()
	{
		var left = ParseAdditiveExpression();

		while (MatchShiftOperator(out var operatorTokenSpan))  
		{
			var right = ParseAdditiveExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorTokenSpan), right);
		}

		return left;
	}

	private Expression ParseAdditiveExpression()
	{
		var left = ParseMultiplicativeExpression();

		while (Match(out var operatorToken, SyntaxTokenType.Plus, SyntaxTokenType.Minus))  
		{
			var right = ParseMultiplicativeExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private Expression ParseMultiplicativeExpression()
	{
		var left = ParseExponentExpression();

		while (Match(out var operatorToken, SyntaxTokenType.Star, SyntaxTokenType.Slash, SyntaxTokenType.Percent))  
		{
			var right = ParseExponentExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private Expression ParseExponentExpression()
	{
		var left = ParseNullCoalescingExpression();

		if (!Match(out var operatorToken, SyntaxTokenType.StarStar))
			return left;
		
		var right = ParseExponentExpression();
		left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);

		return left;
	}

	private Expression ParseNullCoalescingExpression()
	{
		var left = ParseUnaryExpression();

		while (Match(out var operatorToken, SyntaxTokenType.QuestionQuestion))  
		{
			var right = ParseUnaryExpression();
			left = new Expression.Binary(left, new BinaryOperator(operatorToken), right);
		}

		return left;
	}

	private Expression ParseUnaryExpression()
	{
		if (!Match(out var operatorToken, SyntaxTokenType.Plus, SyntaxTokenType.Minus, SyntaxTokenType.Bang,
				SyntaxTokenType.Tilde, SyntaxTokenType.PlusPlus, SyntaxTokenType.MinusMinus)) 
			return ParsePrimaryExpression();
		
		var operand = ParseUnaryExpression();
		return new Expression.Unary(operand, operatorToken);
	}

	private Expression ParsePrimaryExpression()
	{
		if (Match(out var constantToken, SyntaxTokenType.IntegerConstant, SyntaxTokenType.RealConstant,
				SyntaxTokenType.StringConstant, SyntaxTokenType.True, SyntaxTokenType.False, SyntaxTokenType.Null))
		{
			return new Expression.Literal(constantToken);
		}

		if (Match(SyntaxTokenType.OpenParen))
		{
			var expression = ParseExpression();
			Consume(SyntaxTokenType.CloseParen);

			return expression;
		}

		if (Match(SyntaxTokenType.New))
		{
			return ParseInstantiationExpression();
		}

		var qualifier = ParseQualifier();
		Expression primary = new Expression.Qualifier(qualifier);

		while (true)
		{
			if (Match(out var dotToken, SyntaxTokenType.Dot))
			{
				var accessIdentifier = new Expression.Identifier(Consume(SyntaxTokenType.Identifier));
				primary = new Expression.Binary(primary, new BinaryOperator(dotToken), accessIdentifier);
			}
			else if (Match(out var questionDotToken, SyntaxTokenType.QuestionDot))
			{
				var accessIdentifier = new Expression.Identifier(Consume(SyntaxTokenType.Identifier));
				primary = new Expression.Binary(primary, new BinaryOperator(questionDotToken), accessIdentifier);
			}
			else if (Match(SyntaxTokenType.Colon))
			{
				var targetType = ParseType();
				if (targetType is null)
					throw Error("Cannot cast to void.");
				
				primary = new Expression.Cast(primary, targetType, false);
			}
			else if (Match(SyntaxTokenType.QuestionColon))
			{
				var targetType = ParseType();
				if (targetType is null)
					throw Error("Cannot cast to void.");
				
				primary = new Expression.Cast(primary, targetType, true);
			}
			else if (Match(SyntaxTokenType.OpenSquare))
			{
				var key = ParseExpression();
				Consume(SyntaxTokenType.CloseSquare);
				primary = new Expression.Index(primary, key, false);
			}
			else if (Match(SyntaxTokenType.QuestionOpenSquare))
			{
				var key = ParseExpression();
				Consume(SyntaxTokenType.CloseSquare);
				primary = new Expression.Index(primary, key, true);
			}
			else if (Match(out var postfixOperator, SyntaxTokenType.PlusPlus, SyntaxTokenType.MinusMinus,
						 SyntaxTokenType.Bang))
			{
				primary = new Expression.Unary(primary, postfixOperator, true);
			}
			else
			{
				// None of the above, check for function call with optional template params first
				if (TryParseFunctionCall(primary, out var functionCall))
				{
					primary = functionCall;
				}
				else
				{
					break;
				}
			}
		}

		return primary;
	}

	private Expression ParseInstantiationExpression()
	{
		var instanceType = ParseType();
		if (instanceType is null)
			throw Error("Cannot create an instance of type 'void'.");
		
		if (!Match(SyntaxTokenType.OpenParen))
		{
			var emptyArguments = Array.Empty<Expression>();
			var pureInitializers = ParseInstanceInitializer();
			return new Expression.Instantiate(instanceType, emptyArguments, pureInitializers);
		}

		var arguments = new List<Expression>();
		
		if (!Check(SyntaxTokenType.CloseParen))
			arguments.AddRange(ParseArgumentList());
		
		Consume(SyntaxTokenType.CloseParen);

		var initializers = new List<Expression>();
		if (Check(SyntaxTokenType.OpenBrace))
		{
			initializers.AddRange(ParseInstanceInitializer());
		}
		
		return new Expression.Instantiate(instanceType, arguments, initializers);
	}

	private IEnumerable<Expression> ParseInstanceInitializer()
	{
		Consume(SyntaxTokenType.OpenBrace);
		var initializers = new List<Expression>();
		
		if (!Check(SyntaxTokenType.CloseBrace)) do
		{
			initializers.Add(ParseExpression());
		} while (Match(SyntaxTokenType.Comma));

		Consume(SyntaxTokenType.CloseBrace);
		return initializers;
	}

	private Expression.Lambda ParseLambdaExpression()
	{
		Consume(SyntaxTokenType.OpenSquare);
		var parameterList = new List<Variable>();
		if (!Check(SyntaxTokenType.CloseSquare))
		{
			parameterList.AddRange(ParseParameterList());
		}
		Consume(SyntaxTokenType.CloseSquare);

		TypeSyntax? returnType = null;
		if (Match(SyntaxTokenType.LeftTriangle))
		{
			Consume(SyntaxTokenType.OpenSquare);
			returnType = ParseType();
			Consume(SyntaxTokenType.CloseSquare);
		}
		else Consume(SyntaxTokenType.RightTriangle);

		var body = ParseStatement();

		return new Expression.Lambda(parameterList, returnType, body);
	}

	private bool TryParseFunctionCall(Expression function, [NotNullWhen(true)] out Expression.Call? functionCall)
	{
		functionCall = null;
		var templateArgs = new List<Expression.Qualifier>();
		var lastPosition = position;
		try
		{
			suppressErrors = true;
			if (Match(SyntaxTokenType.LeftAngled))
			{
				do
				{
					templateArgs.Add(new Expression.Qualifier(ParseQualifier()));
				} while (Match(SyntaxTokenType.Comma));

				Consume(SyntaxTokenType.RightAngled);
			}

			Consume(SyntaxTokenType.OpenParen);

			List<Expression> arguments = new();
			if (!Check(SyntaxTokenType.CloseParen))
				arguments.AddRange(ParseArgumentList());

			Consume(SyntaxTokenType.CloseParen);
			functionCall = new Expression.Call(function, templateArgs, arguments);
			return true;
		}
		catch
		{
			position = lastPosition;
			return false;
		}
		finally
		{
			suppressErrors = false;
		}
	}
}