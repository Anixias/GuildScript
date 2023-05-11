using System.Collections.Immutable;
using GuildScript.Analysis.Syntax;
using GuildScript.Analysis.Text;

namespace GuildScript.Analysis.Semantics;

public sealed class Collector : Statement.IVisitor, Expression.IVisitor
{
	public DiagnosticCollection Diagnostics { get; } = new();
	private readonly SemanticModel semanticModel;

	public Collector(SemanticModel semanticModel)
	{
		this.semanticModel = semanticModel;
	}

	public void CollectSymbols(SyntaxTree tree)
	{
		if (tree.Root is Statement statement)
		{
			statement.AcceptVisitor(this);
		}
	}

	public void VisitProgramStatement(Statement.Program statement)
	{
		foreach (var name in statement.Module.Names)
		{
			var module = semanticModel.AddModule(name);
			semanticModel.VisitSymbol(module);
		}
		
		semanticModel.EnterScope(statement);
		foreach (var topLevelStatement in statement.Statements)
		{
			topLevelStatement.AcceptVisitor(this);
		}
		semanticModel.ExitScope();

		for (var i = 0; i < statement.Module.Names.Length; i++)
		{
			semanticModel.Return();
		}
	}

	public void VisitEntryPointStatement(Statement.EntryPoint statement)
	{
		var entryPointSymbol = semanticModel.AddEntryPoint(statement);
		semanticModel.VisitSymbol(entryPointSymbol);
		semanticModel.EnterScope(statement);
		
		foreach (var parameter in statement.ParameterList)
		{
			var parameterDeclaration = new Declaration(parameter.Name, statement);
			semanticModel.AddSymbol(entryPointSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
				parameter.IsReference));
		}
		
		statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
		semanticModel.Return();
	}

	public void VisitDefineStatement(Statement.Define statement)
	{
		semanticModel.AddDefine(statement);
	}

	public void VisitBlockStatement(Statement.Block statement)
	{
		semanticModel.EnterScope(statement);
		
		foreach (var bodyStatement in statement.Statements)
		{
			bodyStatement.AcceptVisitor(this);
		}
		
		semanticModel.ExitScope();
	}
	
	private static AccessModifier GetAccessModifier(SyntaxToken? modifierToken, AccessModifier @default)
	{
		if (modifierToken is null)
			return @default;

		return modifierToken.Type switch
		{
			SyntaxTokenType.Public    => AccessModifier.Public,
			SyntaxTokenType.Private   => AccessModifier.Private,
			SyntaxTokenType.Protected => AccessModifier.Protected,
			SyntaxTokenType.Internal  => AccessModifier.Internal,
			_                         => @default
		};
	}

	public void VisitClassStatement(Statement.Class statement)
	{
		static ClassModifier GetClassModifier(SyntaxToken? modifierToken)
		{
			if (modifierToken is null)
				return ClassModifier.None;

			return modifierToken.Type switch
			{
				SyntaxTokenType.Global   => ClassModifier.Global,
				SyntaxTokenType.Final    => ClassModifier.Final,
				SyntaxTokenType.Template => ClassModifier.Template,
				_                        => ClassModifier.None
			};
		}
		
		try
		{
			var classModifier = GetClassModifier(statement.ClassModifier);
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Internal);

			var declaration = new Declaration(statement.NameToken, statement);
			var symbol = semanticModel.AddClass(statement.NameToken.Text, declaration, classModifier, accessModifier);
			semanticModel.VisitSymbol(symbol);
			semanticModel.EnterScope(statement);

			symbol.TemplateParameters = statement.TypeParameters.Select(templateParameter =>
				semanticModel.AddTemplateParameter(templateParameter.Text,
					new Declaration(templateParameter, statement))).ToImmutableArray();

			foreach (var member in statement.Members)
			{
				member.AcceptVisitor(this);
			}
		
			semanticModel.ExitScope();
			semanticModel.Return();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitStructStatement(Statement.Struct statement)
	{
		static StructModifier GetStructModifier(SyntaxToken? modifierToken)
		{
			if (modifierToken is null)
				return StructModifier.None;
			
			return modifierToken.Type switch
			{
				SyntaxTokenType.Immutable => StructModifier.Immutable,
				_                         => StructModifier.None
			};
		}
		
		try
		{
			var structModifier = GetStructModifier(statement.StructModifier);
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Internal);

			var declaration = new Declaration(statement.NameToken, statement);
			var symbol = semanticModel.AddStruct(statement.NameToken.Text, declaration, structModifier, accessModifier);
			
			semanticModel.VisitSymbol(symbol);
			semanticModel.EnterScope(statement);
			
			symbol.TemplateParameters = statement.TypeParameters.Select(templateParameter =>
				semanticModel.AddTemplateParameter(templateParameter.Text,
					new Declaration(templateParameter, statement))).ToImmutableArray();

			foreach (var member in statement.Members)
			{
				member.AcceptVisitor(this);
			}
		
			semanticModel.ExitScope();
			semanticModel.Return();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitInterfaceStatement(Statement.Interface statement)
	{
		try
		{
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Internal);
			var declaration = new Declaration(statement.NameToken, statement);

			var symbol = semanticModel.AddInterface(statement.NameToken.Text, declaration, accessModifier);
			
			semanticModel.VisitSymbol(symbol);
			semanticModel.EnterScope(statement);
			
			symbol.TemplateParameters = statement.TypeParameters.Select(templateParameter =>
				semanticModel.AddTemplateParameter(templateParameter.Text,
					new Declaration(templateParameter, statement))).ToImmutableArray();

			foreach (var member in statement.Members)
			{
				member.AcceptVisitor(this);
			}
		
			semanticModel.ExitScope();
			semanticModel.Return();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitEnumStatement(Statement.Enum statement)
	{
		try
		{
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Internal);
			var declaration = new Declaration(statement.NameToken, statement);
			var enumSymbol = semanticModel.AddEnum(statement.NameToken.Text, declaration, accessModifier, statement.Type);
			foreach (var member in statement.Members)
			{
				if (enumSymbol.AddMember(member.Identifier.Text) is { } enumMemberSymbol)
					semanticModel.AddSymbol(enumMemberSymbol);
			}
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitDestructorStatement(Statement.Destructor statement)
	{
		try
		{
			var declaration = new Declaration(statement.DestructorToken, statement);
			semanticModel.AddDestructor(statement.DestructorToken.Text, declaration);

			semanticModel.EnterScope(statement);
			statement.Body.AcceptVisitor(this);
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.DestructorToken, e.Message);
		}
	}

	public void VisitExternalMethodStatement(Statement.ExternalMethod statement)
	{
		try
		{
			var declaration = new Declaration(statement.Identifier, statement);
			var externalMethodSymbol = semanticModel.AddExternalMethod(statement.Identifier.Text, declaration);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				semanticModel.AddSymbol(externalMethodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
					parameter.IsReference));
			}
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.Identifier, e.Message);
		}
	}

	public void VisitConstructorStatement(Statement.Constructor statement)
	{
		try
		{
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Private);
			var declaration = new Declaration(statement.ConstructorToken, statement);
			var constructorSymbol = semanticModel.AddConstructor(statement.ConstructorToken.Text, declaration, accessModifier);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				semanticModel.AddSymbol(constructorSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
					parameter.IsReference));
			}
			
			statement.Body.AcceptVisitor(this);
			
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.ConstructorToken, e.Message);
		}
	}

	public void VisitIndexerStatement(Statement.Indexer statement)
	{
		try
		{
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Private);
			var declaration = new Declaration(statement.ThisToken, statement);
			var indexerSymbol = semanticModel.AddIndexer(statement.ThisToken.Text, declaration, accessModifier);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				semanticModel.AddSymbol(indexerSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
					parameter.IsReference));
			}

			foreach (var bodyStatement in statement.Body)
			{
				bodyStatement.AcceptVisitor(this);
			}
			
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.ThisToken, e.Message);
		}
	}

	public void VisitAccessorTokenStatement(Statement.AccessorToken statement)
	{
		
	}

	public void VisitAccessorLambdaStatement(Statement.AccessorLambda statement)
	{
		
	}

	public void VisitAccessorLambdaSignatureStatement(Statement.AccessorLambdaSignature statement)
	{
		
	}
	
	private static EventModifier GetEventModifier(SyntaxToken? modifierToken)
	{
		if (modifierToken is null)
			return EventModifier.None;
			
		return modifierToken.Type switch
		{
			SyntaxTokenType.Global => EventModifier.Global,
			_                      => EventModifier.None
		};
	}

	public void VisitEventStatement(Statement.Event statement)
	{
		try
		{
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Private);
			var eventModifier = GetEventModifier(statement.EventModifier);
			var declaration = new Declaration(statement.NameToken, statement);
			var eventSymbol =
				semanticModel.AddEvent(statement.NameToken.Text, declaration, accessModifier, eventModifier);

			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				semanticModel.AddSymbol(eventSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
					parameter.IsReference));
			}
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitEventSignatureStatement(Statement.EventSignature statement)
	{
		try
		{
			var eventModifier = GetEventModifier(statement.EventModifier);
			var declaration = new Declaration(statement.NameToken, statement);
			var eventSymbol = semanticModel.AddEvent(statement.NameToken.Text, declaration, AccessModifier.Public,
				eventModifier);

			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				semanticModel.AddSymbol(eventSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
					parameter.IsReference));
			}
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	private static IEnumerable<MethodModifier> GetMethodModifiers(IEnumerable<SyntaxToken> tokens)
	{
		foreach (var token in tokens)
		{
			switch (token.Type)
			{
				case SyntaxTokenType.Global:
					yield return MethodModifier.Global;
					break;
				case SyntaxTokenType.Immutable:
					yield return MethodModifier.Immutable;
					break;
				case SyntaxTokenType.Prototype:
					yield return MethodModifier.Prototype;
					break;
				case SyntaxTokenType.Required:
					yield return MethodModifier.Required;
					break;
				default:
					continue;
			}
		}
	}

	public void VisitPropertyStatement(Statement.Property statement)
	{
		try
		{
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Private);
			var methodModifiers = GetMethodModifiers(statement.Modifiers);
			var declaration = new Declaration(statement.NameToken, statement);
			semanticModel.AddProperty(statement.NameToken.Text, declaration, accessModifier, methodModifiers);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitPropertySignatureStatement(Statement.PropertySignature statement)
	{
		try
		{
			var methodModifiers = GetMethodModifiers(statement.Modifiers);
			var declaration = new Declaration(statement.NameToken, statement);
			semanticModel.AddProperty(statement.NameToken.Text, declaration, AccessModifier.Public, methodModifiers);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitMethodStatement(Statement.Method statement)
	{
		try
		{
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Private);
			var methodModifiers = GetMethodModifiers(statement.Modifiers);
			var declaration = new Declaration(statement.NameToken, statement);
			var methodSymbol =
				semanticModel.AddMethod(statement.NameToken.Text, declaration, accessModifier, methodModifiers);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				semanticModel.AddSymbol(methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
					parameter.IsReference));
			}

			methodSymbol.TemplateParameters = statement.TypeParameters.Select(templateParameter =>
				semanticModel.AddTemplateParameter(templateParameter.Text,
					new Declaration(templateParameter, statement))).ToImmutableArray();
			
			statement.Body.AcceptVisitor(this);
			
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitMethodSignatureStatement(Statement.MethodSignature statement)
	{
		try
		{
			var methodModifiers = GetMethodModifiers(statement.Modifiers);
			var declaration = new Declaration(statement.NameToken, statement);
			var methodSymbol = semanticModel.AddMethod(statement.NameToken.Text, declaration, AccessModifier.Public,
				methodModifiers);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				semanticModel.AddSymbol(methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
					parameter.IsReference));
			}
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitFieldStatement(Statement.Field statement)
	{
		static IEnumerable<FieldModifier> GetFieldModifiers(IEnumerable<SyntaxToken> tokens)
		{
			foreach (var token in tokens)
			{
				switch (token.Type)
				{
					case SyntaxTokenType.Global:
						yield return FieldModifier.Global;
						break;
					case SyntaxTokenType.Immutable:
						yield return FieldModifier.Immutable;
						break;
					case SyntaxTokenType.Final:
						yield return FieldModifier.Final;
						break;
					case SyntaxTokenType.Constant:
						yield return FieldModifier.Constant;
						break;
					case SyntaxTokenType.Fixed:
						yield return FieldModifier.Fixed;
						break;
					default:
						continue;
				}
			}
		}
		
		try
		{
			var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Private);
			var fieldModifiers = GetFieldModifiers(statement.Modifiers);
			var declaration = new Declaration(statement.NameToken, statement);
			semanticModel.AddField(statement.NameToken.Text, declaration, accessModifier, fieldModifiers);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.NameToken, e.Message);
		}
	}

	public void VisitBreakStatement(Statement.Break statement)
	{
		
	}

	public void VisitContinueStatement(Statement.Continue statement)
	{
		
	}

	public void VisitControlStatement(Statement.Control statement)
	{
		semanticModel.EnterScope(statement);
		
		statement.IfStatement.AcceptVisitor(this);
		statement.ElseStatement?.AcceptVisitor(this);
		
		semanticModel.ExitScope();
	}

	public void VisitWhileStatement(Statement.While statement)
	{
		semanticModel.EnterScope(statement);
		statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
	}

	public void VisitDoWhileStatement(Statement.DoWhile statement)
	{
		semanticModel.EnterScope(statement);
		statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
	}

	public void VisitForStatement(Statement.For statement)
	{
		semanticModel.EnterScope(statement);
		
		statement.Initializer?.AcceptVisitor(this);
		statement.Increment?.AcceptVisitor(this);
		statement.Body.AcceptVisitor(this);
		
		semanticModel.ExitScope();
	}

	public void VisitForEachStatement(Statement.ForEach statement)
	{
		semanticModel.EnterScope(statement);

		semanticModel.AddLocalVariable(statement.Iterator.Text, new Declaration(statement.Iterator, statement),
			statement.IteratorType);
		statement.Body.AcceptVisitor(this);
		
		semanticModel.ExitScope();
	}

	public void VisitRepeatStatement(Statement.Repeat statement)
	{
		semanticModel.EnterScope(statement);
		statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
	}

	public void VisitReturnStatement(Statement.Return statement)
	{
		
	}

	public void VisitThrowStatement(Statement.Throw statement)
	{
		
	}

	public void VisitSealStatement(Statement.Seal statement)
	{
		
	}

	public void VisitTryStatement(Statement.Try statement)
	{
		semanticModel.EnterScope(statement);
		
		statement.TryStatement.AcceptVisitor(this);
		statement.CatchStatement?.AcceptVisitor(this);
		statement.FinallyStatement?.AcceptVisitor(this);
		
		semanticModel.ExitScope();
	}

	public void VisitVariableDeclarationStatement(Statement.VariableDeclaration statement)
	{
		try
		{
			var declaration = new Declaration(statement.Identifier, statement);
			semanticModel.AddLocalVariable(statement.Identifier.Text, declaration, statement.Type);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.Identifier, e.Message);
		}
	}

	public void VisitLockStatement(Statement.Lock statement)
	{
		
	}

	public void VisitSwitchStatement(Statement.Switch statement)
	{
		try
		{
			semanticModel.EnterScope(statement);

			foreach (var section in statement.Sections)
			{
				semanticModel.EnterScope(section.Body);
				foreach (var label in section.Labels)
				{
					if (label is not Statement.Switch.PatternLabel patternLabel)
						continue;

					if (patternLabel.Identifier is null)
						continue;

					semanticModel.AddLocalVariable(patternLabel.Identifier.Text,
						new Declaration(patternLabel.Identifier, statement), patternLabel.Type);
				}

				semanticModel.ExitScope();
			}

			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.SwitchToken, e.Message);
		}
	}

	public void VisitExpressionStatement(Statement.ExpressionStatement statement)
	{
		
	}

	public void VisitOperatorOverloadStatement(Statement.OperatorOverload statement)
	{
		try
		{
			// @TODO Add support for TokenSpan in Declaration
			var declaration = new Declaration(statement.Operator.TokenSpan.Tokens[0], statement);

			var parameters = new List<string>();
			foreach (var parameter in statement.ParameterList)
			{
				parameters.Add((parameter.IsReference ? "ref " : "") + parameter.Type);
			}

			var parameterName = string.Join(", ", parameters);
			var name = $"[{statement.Operator.TokenSpan}]({parameterName})";

			var modifiers = statement.Immutable
				? new[] { MethodModifier.Global, MethodModifier.Immutable }
				: new[] { MethodModifier.Global };

			var methodSymbol = semanticModel.AddOperatorOverload(name, declaration, AccessModifier.Public, modifiers,
				statement.Operator);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				semanticModel.AddSymbol(methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
					parameter.IsReference));
			}
			
			statement.Body.AcceptVisitor(this);
			
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.Operator.TokenSpan.Tokens[0], e.Message);
		}
	}

	public void VisitOperatorOverloadSignatureStatement(Statement.OperatorOverloadSignature statement)
	{
		try
		{
			// @TODO Add support for TokenSpan in Declaration
			var declaration = new Declaration(statement.Operator.TokenSpan.Tokens[0], statement);
			
			var parameters = new List<string>();
			foreach (var parameter in statement.ParameterList)
			{
				parameters.Add((parameter.IsReference ? "ref " : "") + parameter.Type);
			}

			var parameterName = string.Join(", ", parameters);
			var name = $"[{statement.Operator.TokenSpan}]({parameterName})";
			
			var modifiers = statement.Immutable
				? new[] { MethodModifier.Global, MethodModifier.Immutable }
				: new[] { MethodModifier.Global };
			
			var methodSymbol = semanticModel.AddOperatorOverload(name, declaration, AccessModifier.Public, modifiers,
				statement.Operator);

			semanticModel.EnterScope(statement);
			foreach (var parameter in statement.ParameterList)
			{
				var parameterDeclaration = new Declaration(parameter.Name, statement);
				semanticModel.AddSymbol(methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
					parameter.IsReference));
			}
			semanticModel.ExitScope();
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(statement.Operator.TokenSpan.Tokens[0], e.Message);
		}
	}

	public void VisitAwaitExpression(Expression.Await expression)
	{
	}

	public void VisitConditionalExpression(Expression.Conditional expression)
	{
	}

	public void VisitBinaryExpression(Expression.Binary expression)
	{
	}

	public void VisitTypeRelationExpression(Expression.TypeRelation expression)
	{
		if (expression.IdentifierToken is null)
			return;

		try
		{
			var declaration = new Declaration(expression.IdentifierToken, expression);
			semanticModel.AddLocalVariable(expression.IdentifierToken.Text, declaration, expression.Type);
		}
		catch (Exception e)
		{
			Diagnostics.ReportTypeCollectorException(expression.IdentifierToken, e.Message);
		}
	}

	public void VisitUnaryExpression(Expression.Unary expression)
	{
	}

	public void VisitIdentifierExpression(Expression.Identifier expression)
	{
	}

	public void VisitQualifierExpression(Expression.Qualifier expression)
	{
	}

	public void VisitCallExpression(Expression.Call expression)
	{
	}

	public void VisitLiteralExpression(Expression.Literal expression)
	{
	}

	public void VisitInstantiateExpression(Expression.Instantiate expression)
	{
	}

	public void VisitCastExpression(Expression.Cast expression)
	{
	}

	public void VisitIndexExpression(Expression.Index expression)
	{
	}

	public void VisitLambdaExpression(Expression.Lambda expression)
	{
	}
}