using GuildScript.Analysis.Semantics.Symbols;
using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public sealed class Resolver : Statement.IVisitor, Expression.IVisitor
{
	private readonly SemanticModel semanticModel;

	public Resolver(SemanticModel semanticModel)
	{
		this.semanticModel = semanticModel;
	}

	public void Resolve(SyntaxTree tree)
	{
		try
		{
			Resolve(tree.Root);
		}
		catch (Exception e)
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine(e.Message);
			Console.ResetColor();
		}
	}

	public void Resolve(SyntaxNode node)
	{
		switch (node)
		{
			case Statement statement:
				statement.AcceptVisitor(this);
				break;
		}
	}

	private ResolvedType? ResolveType(TypeSyntax typeSyntax)
	{
		switch (typeSyntax)
		{
			case BaseTypeSyntax baseTypeSyntax:
				var nativeResolvedType = SimpleResolvedType.FindNativeType(baseTypeSyntax.ToString());
				if (nativeResolvedType is null)
					throw new Exception($"The native type '{baseTypeSyntax}' could not be resolved.");

				return nativeResolvedType;
			case ExpressionTypeSyntax expressionTypeSyntax:
				return ResolveExpressionType(expressionTypeSyntax);
			case NamedTypeSyntax namedTypeSyntax:
				var symbol = semanticModel.FindSymbol(namedTypeSyntax.Name);
				if (symbol is TypeSymbol typeSymbol)
					return new SimpleResolvedType(typeSymbol);
				
				throw new Exception($"The type '{namedTypeSyntax.Name}' does not exist in this context.");
			case ArrayTypeSyntax arrayTypeSyntax:
				var elementType = ResolveType(arrayTypeSyntax.BaseType);
				if (elementType is not null)
					return new ArrayResolvedType(elementType);
				
				throw new Exception($"The type '{arrayTypeSyntax.BaseType}' does not exist in this context.");
			case NullableTypeSyntax nullableTypeSyntax:
				var nullableBaseType = ResolveType(nullableTypeSyntax.BaseType);
				if (nullableBaseType is not null)
					return new NullableResolvedType(nullableBaseType);
				
				throw new Exception($"The type '{nullableTypeSyntax.BaseType}' does not exist in this context.");
			case TemplatedTypeSyntax templatedTypeSyntax:
				var templatedBaseType = ResolveType(templatedTypeSyntax.BaseType);
				if (templatedBaseType is null)
					throw new Exception($"The type '{templatedTypeSyntax.BaseType}' does not exist in this context.");

				var templateArguments = new List<ResolvedType>();
				foreach (var templateArgument in templatedTypeSyntax.TypeArguments)
				{
					var resolvedTemplateArgument = ResolveType(templateArgument);
					if (resolvedTemplateArgument is not null)
						templateArguments.Add(resolvedTemplateArgument);
					
					throw new Exception($"The type '{templateArgument}' does not exist in this context.");
				}

				return new TemplatedResolvedType(templatedBaseType, templateArguments);
		}

		return null;
	}
	
	private TypeSymbol? ResolveExpressionTypeSymbol(Expression expression)
	{
		switch (expression)
		{
			case Expression.Qualifier qualifier:
			{
				var symbol = semanticModel.FindSymbol(qualifier.NameToken.Text);
				if (symbol is TypeSymbol typeSymbol)
					return typeSymbol;
				
				throw new Exception($"The type '{qualifier.NameToken.Text}' does not exist in this context.");
			}
			case Expression.Identifier identifier:
			{
				var symbol = semanticModel.FindSymbol(identifier.NameToken.Text);
				if (symbol is TypeSymbol typeSymbol)
					return typeSymbol;
				
				throw new Exception($"The type '{identifier.NameToken.Text}' does not exist in this context.");
			}
			case Expression.Binary binary:
			{
				var operation = binary.Operator.Operation;
				if (operation != BinaryOperator.BinaryOperation.Access &&
					operation != BinaryOperator.BinaryOperation.ConditionalAccess)
					throw new Exception($"Invalid operator for type resolution: {binary.Operator}");

				var left = ResolveExpressionTypeSymbol(binary.Left);
				if (left is null)
					throw new Exception($"'{binary.Left}' is not a valid type.");
				semanticModel.VisitSymbol(left);
				var right = ResolveExpressionTypeSymbol(binary.Right);
				semanticModel.Return();

				return right;
			}
		}

		return null;
	}

	private ResolvedType ResolveExpressionType(ExpressionTypeSyntax expressionTypeSyntax)
	{
		if (ResolveExpressionTypeSymbol(expressionTypeSyntax.Expression) is { } typeSymbol)
			return new SimpleResolvedType(typeSymbol);

		throw new Exception("Failed to resolve type from expression.");
	}

	public void VisitProgramStatement(Statement.Program statement)
	{
		foreach (var name in statement.Module.Names)
		{
			var module = semanticModel.GetModule(name);
			if (module is null)
				throw new Exception($"Missing module '{name}' in '{statement.Module}'.");
			
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
		var entryPointSymbol = semanticModel.GetEntryPoint();
		semanticModel.VisitSymbol(entryPointSymbol);
		semanticModel.EnterScope(statement);

		if (statement.ReturnType is not null)
			entryPointSymbol.ReturnType = ResolveType(statement.ReturnType);

		foreach (var parameter in statement.ParameterList)
		{
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");
			
			entryPointSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}
		
		statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
		semanticModel.Return();

		entryPointSymbol.Resolved = true;
	}

	public void VisitDefineStatement(Statement.Define statement)
	{
		
	}

	public void VisitBlockStatement(Statement.Block statement)
	{
		
	}

	public void VisitClassStatement(Statement.Class statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is null)
			throw new Exception($"Failed to find symbol '{statement.NameToken.Text}'.");
		
		semanticModel.VisitSymbol(symbol);
		semanticModel.EnterScope(statement);

		foreach (var member in statement.Members)
		{
			member.AcceptVisitor(this);
		}
		
		semanticModel.ExitScope();
		semanticModel.Return();
	}

	public void VisitStructStatement(Statement.Struct statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is null)
			throw new Exception($"Failed to find symbol '{statement.NameToken.Text}'.");
		
		semanticModel.VisitSymbol(symbol);
		semanticModel.EnterScope(statement);

		foreach (var member in statement.Members)
		{
			member.AcceptVisitor(this);
		}
		
		semanticModel.ExitScope();
		semanticModel.Return();
	}

	public void VisitInterfaceStatement(Statement.Interface statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is null)
			throw new Exception($"Failed to find symbol '{statement.NameToken.Text}'.");
		
		semanticModel.VisitSymbol(symbol);
		semanticModel.EnterScope(statement);

		foreach (var member in statement.Members)
		{
			member.AcceptVisitor(this);
		}
		
		semanticModel.ExitScope();
		semanticModel.Return();
	}

	public void VisitEnumStatement(Statement.Enum statement)
	{
		
	}

	public void VisitDestructorStatement(Statement.Destructor statement)
	{
		
	}

	public void VisitExternalMethodStatement(Statement.ExternalMethod statement)
	{
		
	}

	public void VisitConstructorStatement(Statement.Constructor statement)
	{
		
	}

	public void VisitIndexerStatement(Statement.Indexer statement)
	{
		
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

	public void VisitEventStatement(Statement.Event statement)
	{
		
	}

	public void VisitEventSignatureStatement(Statement.EventSignature statement)
	{
		
	}

	public void VisitPropertyStatement(Statement.Property statement)
	{
		
	}

	public void VisitPropertySignatureStatement(Statement.PropertySignature statement)
	{
		
	}

	public void VisitMethodStatement(Statement.Method statement)
	{
		
	}

	public void VisitMethodSignatureStatement(Statement.MethodSignature statement)
	{
		
	}

	public void VisitFieldStatement(Statement.Field statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not FieldSymbol fieldSymbol)
			throw new Exception($"Failed to resolve field '{statement.NameToken.Text}'.");

		if (statement.Type is null)
			return;
		
		fieldSymbol.Type = ResolveType(statement.Type);
		fieldSymbol.Resolved = true;
	}

	public void VisitBreakStatement(Statement.Break statement)
	{
		
	}

	public void VisitContinueStatement(Statement.Continue statement)
	{
		
	}

	public void VisitControlStatement(Statement.Control statement)
	{
		
	}

	public void VisitWhileStatement(Statement.While statement)
	{
		
	}

	public void VisitDoWhileStatement(Statement.DoWhile statement)
	{
		
	}

	public void VisitForStatement(Statement.For statement)
	{
		
	}

	public void VisitForEachStatement(Statement.ForEach statement)
	{
		
	}

	public void VisitRepeatStatement(Statement.Repeat statement)
	{
		
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
		
	}

	public void VisitVariableDeclarationStatement(Statement.VariableDeclaration statement)
	{
		
	}

	public void VisitLockStatement(Statement.Lock statement)
	{
		
	}

	public void VisitSwitchStatement(Statement.Switch statement)
	{
		
	}

	public void VisitExpressionStatement(Statement.ExpressionStatement statement)
	{
		
	}

	public void VisitOperatorOverloadStatement(Statement.OperatorOverload statement)
	{
		
	}

	public void VisitOperatorOverloadSignatureStatement(Statement.OperatorOverloadSignature statement)
	{
		
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