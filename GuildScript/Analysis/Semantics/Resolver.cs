using GuildScript.Analysis.Semantics.Symbols;
using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public sealed class Resolver : Statement.IVisitor<ResolvedStatement>, Expression.IVisitor<ResolvedExpression>
{
	private readonly SemanticModel semanticModel;

	public Resolver(SemanticModel semanticModel)
	{
		this.semanticModel = semanticModel;
	}

	public ResolvedTree? Resolve(SyntaxTree tree)
	{
		try
		{
			var resolvedRoot = Resolve(tree.Root);
			return new ResolvedTree(resolvedRoot);
		}
		catch (Exception e)
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Console.WriteLine(e.Message);
			Console.ResetColor();
			return null;
		}
	}

	public ResolvedNode Resolve(SyntaxNode node)
	{
		switch (node)
		{
			case Statement statement:
				return statement.AcceptVisitor(this);
			default:
				throw new Exception("Unexpected node type.");
		}
	}

	public void ReplaceAliases()
	{
		foreach (var symbol in semanticModel.GetAllSymbols())
		{
			switch (symbol)
			{
				case ParameterSymbol parameterSymbol:
					parameterSymbol.Type = ResolveTypeAlias(parameterSymbol.Type);
					break;
				case FieldSymbol fieldSymbol:
					fieldSymbol.Type = ResolveTypeAlias(fieldSymbol.Type);
					break;
				case LocalVariableSymbol variableSymbol:
					variableSymbol.Type = ResolveTypeAlias(variableSymbol.Type);
					break;
				case LambdaTypeSymbol lambdaTypeSymbol:
					lambdaTypeSymbol.ReturnType = ResolveTypeAlias(lambdaTypeSymbol.ReturnType);
					for (var i = 0; i < lambdaTypeSymbol.ParameterTypes.Count; i++)
					{
						lambdaTypeSymbol.ParameterTypes[i] = ResolveTypeAlias(lambdaTypeSymbol.ParameterTypes[i])!;
					}
					break;
				case MethodSymbol methodSymbol:
					methodSymbol.ReturnType = ResolveTypeAlias(methodSymbol.ReturnType);
					break;
				case PropertySymbol propertySymbol:
					propertySymbol.Type = ResolveTypeAlias(propertySymbol.Type);
					break;
			}
		}
	}

	private ResolvedType? ResolveTypeAlias(ResolvedType? type)
	{
		if (type is null)
			return null;

		switch (type)
		{
			case SimpleResolvedType { TypeSymbol: DefineSymbol defineSymbol }:
				return ResolveTypeAlias(defineSymbol.AliasedType);
			case ArrayResolvedType arrayResolvedType:
				var resolvedElementType = ResolveTypeAlias(arrayResolvedType.ElementType);
				if (resolvedElementType != arrayResolvedType.ElementType)
				{
					return new ArrayResolvedType(resolvedElementType!);
				}

				break;
			case NullableResolvedType nullableResolvedType:
				var resolvedBaseType = ResolveTypeAlias(nullableResolvedType.BaseType);
				if (resolvedBaseType != nullableResolvedType.BaseType)
				{
					return new NullableResolvedType(resolvedBaseType!);
				}

				break;
			case TemplatedResolvedType templatedResolvedType:
				var resolvedTemplateBaseType = ResolveTypeAlias(templatedResolvedType.BaseType);
				var resolvedTemplateArguments = templatedResolvedType.TypeArguments.Select(ResolveTypeAlias).ToArray();
				if (!templatedResolvedType.TypeArguments.SequenceEqual(resolvedTemplateArguments))
					return new TemplatedResolvedType(resolvedTemplateBaseType!, resolvedTemplateArguments!);
				break;
		}

		return type;
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

	private MemberSymbol? ResolveExpressionMemberSymbol(Expression expression)
	{
		switch (expression)
		{
			case Expression.Qualifier qualifier:
			{
				var symbol = semanticModel.FindSymbol(qualifier.NameToken.Text);
				if (symbol is MemberSymbol memberSymbol)
					return memberSymbol;

				throw new Exception($"The member '{qualifier.NameToken.Text}' does not exist in this context.");
			}
			case Expression.Identifier identifier:
			{
				var symbol = semanticModel.FindSymbol(identifier.NameToken.Text);
				if (symbol is MemberSymbol memberSymbol)
					return memberSymbol;

				throw new Exception($"The member '{identifier.NameToken.Text}' does not exist in this context.");
			}
			case Expression.Binary binary:
			{
				var operation = binary.Operator.Operation;
				if (operation != BinaryOperator.BinaryOperation.Access &&
					operation != BinaryOperator.BinaryOperation.ConditionalAccess)
					throw new Exception($"Invalid operator for member resolution: {binary.Operator}");

				var left = ResolveExpressionMemberSymbol(binary.Left);
				if (left is null)
					throw new Exception($"'{binary.Left}' is not a valid member.");
				semanticModel.VisitSymbol(left);
				var right = ResolveExpressionMemberSymbol(binary.Right);
				semanticModel.Return();

				return right;
			}
		}

		return null;
	}

	private LocalSymbol? ResolveExpressionLocalSymbol(Expression expression)
	{
		switch (expression)
		{
			case Expression.Qualifier qualifier:
			{
				var symbol = semanticModel.FindSymbol(qualifier.NameToken.Text);
				if (symbol is LocalSymbol localSymbol)
					return localSymbol;

				throw new Exception($"The local variable '{qualifier.NameToken.Text}' does not exist in this context.");
			}
			case Expression.Identifier identifier:
			{
				var symbol = semanticModel.FindSymbol(identifier.NameToken.Text);
				if (symbol is LocalSymbol localSymbol)
					return localSymbol;

				throw new Exception($"The local variable '{identifier.NameToken.Text}' does not exist in this context.");
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

	public ResolvedStatement VisitProgramStatement(Statement.Program statement)
	{
		ModuleSymbol? moduleSymbol = null;
		foreach (var name in statement.Module.Names)
		{
			var module = semanticModel.GetModule(name);
			moduleSymbol = module ?? throw new Exception($"Missing module '{name}' in '{statement.Module}'.");
			semanticModel.VisitSymbol(module);
		}

		if (moduleSymbol is null)
		{
			throw new Exception("Program requires module definition.");
		}

		semanticModel.EnterScope(statement);
		var statements = statement.Statements.Select(topLevelStatement => topLevelStatement.AcceptVisitor(this));
		semanticModel.ExitScope();

		for (var i = 0; i < statement.Module.Names.Length; i++)
		{
			semanticModel.Return();
		}

		return new ResolvedStatement.Program(statements, moduleSymbol);
	}

	public ResolvedStatement VisitEntryPointStatement(Statement.EntryPoint statement)
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

		var body = statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
		semanticModel.Return();

		entryPointSymbol.Resolved = true;
		return new ResolvedStatement.EntryPoint(entryPointSymbol.ReturnType, entryPointSymbol,
			entryPointSymbol.GetParameters(), body);
	}

	public ResolvedStatement VisitDefineStatement(Statement.Define statement)
	{
		var symbol = semanticModel.FindSymbol(statement.Identifier.Text);
		if (symbol is not DefineSymbol defineSymbol)
			throw new Exception($"Failed to resolve definition '{statement.Identifier.Text}'.");

		if (statement.Type is null)
			throw new Exception("Definition must supply a type.");

		defineSymbol.AliasedType = ResolveType(statement.Type);
		defineSymbol.Resolved = true;

		if (defineSymbol.AliasedType is null)
			throw new Exception($"Failed to resolve type for definition '{defineSymbol.Name}'.");

		return new ResolvedStatement.Define(defineSymbol.Name, defineSymbol.AliasedType);
	}

	public ResolvedStatement VisitBlockStatement(Statement.Block statement)
	{
		var statements = statement.Statements.Select(stmt => stmt.AcceptVisitor(this));
		return new ResolvedStatement.Block(statements);
	}

	public ResolvedStatement VisitClassStatement(Statement.Class statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not ClassSymbol classSymbol)
			throw new Exception($"Failed to find class symbol '{statement.NameToken.Text}'.");

		semanticModel.VisitSymbol(classSymbol);
		semanticModel.EnterScope(statement);

		var members = statement.Members.Select(member => member.AcceptVisitor(this));

		semanticModel.ExitScope();
		semanticModel.Return();
		return new ResolvedStatement.Class(classSymbol.AccessModifier, classSymbol.ClassModifier, classSymbol,
			classSymbol.TemplateParameters, classSymbol.BaseClass, members);
	}

	public ResolvedStatement VisitStructStatement(Statement.Struct statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not StructSymbol structSymbol)
			throw new Exception($"Failed to find struct symbol '{statement.NameToken.Text}'.");

		semanticModel.VisitSymbol(structSymbol);
		semanticModel.EnterScope(statement);

		var members = statement.Members.Select(member => member.AcceptVisitor(this));

		semanticModel.ExitScope();
		semanticModel.Return();

		return new ResolvedStatement.Struct(structSymbol.AccessModifier, structSymbol.StructModifier, structSymbol,
			members, structSymbol.TemplateParameters);
	}

	public ResolvedStatement VisitInterfaceStatement(Statement.Interface statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not InterfaceSymbol interfaceSymbol)
			throw new Exception($"Failed to find interface symbol '{statement.NameToken.Text}'.");

		semanticModel.VisitSymbol(interfaceSymbol);
		semanticModel.EnterScope(statement);

		var members = statement.Members.Select(member => member.AcceptVisitor(this));

		semanticModel.ExitScope();
		semanticModel.Return();

		return new ResolvedStatement.Interface(interfaceSymbol.AccessModifier, interfaceSymbol, members,
			interfaceSymbol.TemplateParameters);
	}

	public ResolvedStatement VisitEnumStatement(Statement.Enum statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not EnumSymbol enumSymbol)
			throw new Exception($"Failed to find enum symbol '{statement.NameToken.Text}'.");

		enumSymbol.BaseType = ResolveType(enumSymbol.BaseTypeSyntax);
		enumSymbol.Resolved = true;

		if (enumSymbol.BaseType is null)
			throw new Exception($"Failed to resolve base type for enum '{statement.NameToken.Text}'.");

		var members = statement.Members.Select(member => member.Expression is null
			? new ResolvedStatement.Enum.Member(member.Identifier.Text, null)
			: new ResolvedStatement.Enum.Member(member.Identifier.Text, member.Expression.AcceptVisitor(this)));

		return new ResolvedStatement.Enum(enumSymbol.AccessModifier, enumSymbol, members, enumSymbol.BaseType);
	}

	public ResolvedStatement VisitDestructorStatement(Statement.Destructor statement)
	{
		var symbol = semanticModel.FindSymbol("destructor");
		if (symbol is not DestructorSymbol destructorSymbol)
			throw new Exception("Failed to find destructor symbol.");

		semanticModel.VisitSymbol(destructorSymbol);
		semanticModel.EnterScope(statement);

		var body = statement.Body.AcceptVisitor(this);

		semanticModel.ExitScope();
		semanticModel.Return();

		destructorSymbol.Resolved = true;
		return new ResolvedStatement.Destructor(body, destructorSymbol);
	}

	public ResolvedStatement VisitExternalMethodStatement(Statement.ExternalMethod statement)
	{
		var symbol = semanticModel.FindSymbol(statement.Identifier.Text);
		if (symbol is not ExternalMethodSymbol externalMethodSymbol)
			throw new Exception($"Failed to find external method symbol '{statement.Identifier.Text}'.");

		semanticModel.VisitSymbol(externalMethodSymbol);
		semanticModel.EnterScope(statement);

		if (statement.ReturnType is not null)
			externalMethodSymbol.ReturnType = ResolveType(statement.ReturnType);

		foreach (var parameter in statement.ParameterList)
		{
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			externalMethodSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		semanticModel.ExitScope();
		semanticModel.Return();

		externalMethodSymbol.Resolved = true;
		return new ResolvedStatement.ExternalMethod(externalMethodSymbol.ReturnType, externalMethodSymbol,
			externalMethodSymbol.GetParameters());
	}

	public ResolvedStatement VisitConstructorStatement(Statement.Constructor statement)
	{
		var symbol = semanticModel.FindSymbol("constructor");
		if (symbol is not ConstructorSymbol constructorSymbol)
			throw new Exception("Failed to find constructor.");

		// Handle overloads
		if (constructorSymbol.Declaration.SourceNode != statement)
		{
			foreach (var overload in constructorSymbol.GetOverloads())
			{
				if (overload.Declaration.SourceNode != statement)
					continue;

				constructorSymbol = overload;
			}
		}

		if (constructorSymbol.Declaration.SourceNode != statement)
			throw new Exception("Failed to find constructor for resolution.");

		semanticModel.VisitSymbol(constructorSymbol);
		semanticModel.EnterScope(statement);

		foreach (var parameter in statement.ParameterList)
		{
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			constructorSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		var body = statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();
		semanticModel.Return();

		// @TODO Resolve initializer and arguments

		constructorSymbol.Resolved = true;
		return new ResolvedStatement.Constructor(constructorSymbol.AccessModifier, constructorSymbol,
			constructorSymbol.GetParameters(), body, null, Array.Empty<ResolvedExpression>());
	}

	public ResolvedStatement VisitIndexerStatement(Statement.Indexer statement)
	{
		var symbol = semanticModel.FindSymbol("this");
		if (symbol is not IndexerSymbol indexerSymbol)
			throw new Exception("Failed to find indexer.");

		semanticModel.VisitSymbol(indexerSymbol);
		semanticModel.EnterScope(statement);

		indexerSymbol.Type = ResolveType(statement.Type);
		if (indexerSymbol.Type is null)
			throw new Exception("Failed to resolve type of indexer.");

		foreach (var parameter in statement.ParameterList)
		{
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			indexerSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		var body = statement.Body.Select(bodyStatement => bodyStatement.AcceptVisitor(this));

		semanticModel.ExitScope();
		semanticModel.Return();

		indexerSymbol.Resolved = true;
		return new ResolvedStatement.Indexer(indexerSymbol.AccessModifier, indexerSymbol.Type,
			indexerSymbol.GetParameters(), body, indexerSymbol);
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

	public ResolvedStatement VisitAccessorTokenStatement(Statement.AccessorToken statement)
	{
		var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Public);

		var autoType = statement.Token.Type switch
		{
			SyntaxTokenType.Get => AccessorAutoType.Get,
			SyntaxTokenType.Set => AccessorAutoType.Set,
			_                   => throw new Exception($"Unexpected accessor token '{statement.Token.Text}'.")
		};

		return new ResolvedStatement.AccessorAuto(accessModifier, autoType);
	}

	public ResolvedStatement VisitAccessorLambdaStatement(Statement.AccessorLambda statement)
	{
		var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Public);

		if (statement.LambdaExpression.AcceptVisitor(this) is not ResolvedExpression.Lambda lambda)
			throw new Exception("Invalid lambda expression.");

		return new ResolvedStatement.AccessorLambda(accessModifier, lambda);
	}

	public ResolvedStatement VisitAccessorLambdaSignatureStatement(Statement.AccessorLambdaSignature statement)
	{
		var accessModifier = GetAccessModifier(statement.AccessModifier, AccessModifier.Public);

		var parameters = new List<ResolvedType>();
		foreach (var parameter in statement.LambdaSignature.InputTypes)
		{
			if (ResolveType(parameter) is not { } resolvedType)
				continue;

			parameters.Add(resolvedType);
		}

		var returnType = statement.LambdaSignature.OutputType is null
			? null
			: ResolveType(statement.LambdaSignature.OutputType);

		var symbol = new LambdaTypeSymbol(parameters, returnType);

		return new ResolvedStatement.AccessorLambdaSignature(accessModifier, symbol);
	}

	public ResolvedStatement VisitEventStatement(Statement.Event statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not EventSymbol eventSymbol)
			throw new Exception($"Failed to find event symbol '{statement.NameToken.Text}'.");

		semanticModel.VisitSymbol(eventSymbol);
		semanticModel.EnterScope(statement);

		foreach (var parameter in statement.ParameterList)
		{
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			eventSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		semanticModel.ExitScope();
		semanticModel.Return();

		eventSymbol.Resolved = true;
		return new ResolvedStatement.Event(eventSymbol.AccessModifier, eventSymbol.EventModifier, eventSymbol,
			eventSymbol.GetParameters());
	}

	public ResolvedStatement VisitEventSignatureStatement(Statement.EventSignature statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not EventSymbol eventSymbol)
			throw new Exception($"Failed to find event symbol '{statement.NameToken.Text}'.");

		semanticModel.VisitSymbol(eventSymbol);
		semanticModel.EnterScope(statement);

		foreach (var parameter in statement.ParameterList)
		{
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			eventSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		semanticModel.ExitScope();
		semanticModel.Return();

		eventSymbol.Resolved = true;
		return new ResolvedStatement.EventSignature(eventSymbol.EventModifier, eventSymbol,
			eventSymbol.GetParameters());
	}

	public ResolvedStatement VisitPropertyStatement(Statement.Property statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not PropertySymbol propertySymbol)
			throw new Exception($"Failed to find property symbol '{statement.NameToken.Text}'.");


		var type = ResolveType(statement.Type);
		if (type is null)
			throw new Exception($"Failed to resolve type of property '{statement.NameToken.Text}'.");


		semanticModel.VisitSymbol(propertySymbol);
		semanticModel.EnterScope(statement);

		var body = statement.Body.Select(bodyStatement => bodyStatement.AcceptVisitor(this));

		semanticModel.ExitScope();
		semanticModel.Return();

		propertySymbol.Resolved = true;
		return new ResolvedStatement.Property(propertySymbol.AccessModifier, propertySymbol.Modifiers, type,
			propertySymbol, body);
	}

	public ResolvedStatement VisitPropertySignatureStatement(Statement.PropertySignature statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not PropertySymbol propertySymbol)
			throw new Exception($"Failed to find property symbol '{statement.NameToken.Text}'.");


		var type = ResolveType(statement.Type);
		if (type is null)
			throw new Exception($"Failed to resolve type of property '{statement.NameToken.Text}'.");


		semanticModel.VisitSymbol(propertySymbol);
		semanticModel.EnterScope(statement);

		var body = statement.Body.Select(bodyStatement => bodyStatement.AcceptVisitor(this));

		semanticModel.ExitScope();
		semanticModel.Return();

		propertySymbol.Resolved = true;
		return new ResolvedStatement.PropertySignature(propertySymbol.Modifiers, type, propertySymbol, body);
	}

	public ResolvedStatement VisitMethodStatement(Statement.Method statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not MethodSymbol methodSymbol)
			throw new Exception($"Failed to find method symbol '{statement.NameToken.Text}'.");


		var returnType = statement.ReturnType is null ? null : ResolveType(statement.ReturnType);

		semanticModel.VisitSymbol(methodSymbol);
		semanticModel.EnterScope(statement);

		foreach (var parameter in statement.ParameterList)
		{
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			methodSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		var body = statement.Body.AcceptVisitor(this);

		semanticModel.ExitScope();
		semanticModel.Return();

		methodSymbol.Resolved = true;
		return new ResolvedStatement.Method(methodSymbol.AccessModifier, methodSymbol.Modifiers, returnType,
			methodSymbol, body, methodSymbol.GetParameters(), statement.AsyncToken is not null,
			methodSymbol.TemplateParameters);
	}

	public ResolvedStatement VisitMethodSignatureStatement(Statement.MethodSignature statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not MethodSymbol methodSymbol)
			throw new Exception($"Failed to find method symbol '{statement.NameToken.Text}'.");


		var returnType = statement.ReturnType is null ? null : ResolveType(statement.ReturnType);

		semanticModel.VisitSymbol(methodSymbol);
		semanticModel.EnterScope(statement);

		foreach (var parameter in statement.ParameterList)
		{
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			methodSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		semanticModel.ExitScope();
		semanticModel.Return();

		methodSymbol.Resolved = true;
		return new ResolvedStatement.MethodSignature(methodSymbol.Modifiers, returnType, methodSymbol,
			methodSymbol.GetParameters(), statement.AsyncToken is not null, methodSymbol.TemplateParameters);
	}

	public ResolvedStatement VisitFieldStatement(Statement.Field statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not FieldSymbol fieldSymbol)
			throw new Exception($"Failed to resolve field '{statement.NameToken.Text}'.");

		if (statement.Type is null)
			throw new Exception("Fields require explicit types.");

		fieldSymbol.Type = ResolveType(statement.Type);
		if (fieldSymbol.Type is null)
			throw new Exception($"Failed to resolve type of field '{statement.NameToken.Text}'.");

		var initializer = statement.Initializer?.AcceptVisitor(this);

		fieldSymbol.Resolved = true;

		return new ResolvedStatement.Field(fieldSymbol.AccessModifier, fieldSymbol.Modifiers, fieldSymbol.Type,
			fieldSymbol, initializer);
	}

	public ResolvedStatement VisitBreakStatement(Statement.Break statement)
	{
		return new ResolvedStatement.Break();
	}

	public ResolvedStatement VisitContinueStatement(Statement.Continue statement)
	{
		return new ResolvedStatement.Continue();
	}

	public ResolvedStatement VisitControlStatement(Statement.Control statement)
	{
		semanticModel.EnterScope(statement);

		var ifExpression = statement.IfExpression.AcceptVisitor(this);
		var ifStatement = statement.IfStatement.AcceptVisitor(this);
		var elseStatement = statement.ElseStatement?.AcceptVisitor(this);

		semanticModel.ExitScope();

		return new ResolvedStatement.Control(ifExpression, ifStatement, elseStatement);
	}

	public ResolvedStatement VisitWhileStatement(Statement.While statement)
	{
		semanticModel.EnterScope(statement);

		var condition = statement.Condition.AcceptVisitor(this);
		var body = statement.Body.AcceptVisitor(this);

		semanticModel.ExitScope();

		return new ResolvedStatement.While(condition, body);
	}

	public ResolvedStatement VisitDoWhileStatement(Statement.DoWhile statement)
	{
		semanticModel.EnterScope(statement);

		var body = statement.Body.AcceptVisitor(this);
		var condition = statement.Condition.AcceptVisitor(this);

		semanticModel.ExitScope();

		return new ResolvedStatement.DoWhile(body, condition);
	}

	public ResolvedStatement VisitForStatement(Statement.For statement)
	{
		semanticModel.EnterScope(statement);

		var initializer = statement.Initializer?.AcceptVisitor(this);
		var condition = statement.Condition?.AcceptVisitor(this);
		var increment = statement.Increment?.AcceptVisitor(this);
		var body = statement.Body.AcceptVisitor(this);

		semanticModel.ExitScope();

		return new ResolvedStatement.For(initializer, condition, increment, body);
	}

	public ResolvedStatement VisitForEachStatement(Statement.ForEach statement)
	{
		semanticModel.EnterScope(statement);

		var symbol = semanticModel.FindSymbol(statement.Iterator.Text);
		if (symbol is not LocalVariableSymbol iteratorSymbol)
			throw new Exception($"Failed to resolve iterator '{statement.Iterator.Text}'.");

		var iteratorType = statement.IteratorType is null ? null : ResolveType(statement.IteratorType);
		iteratorSymbol.Type = iteratorType;
		iteratorSymbol.Resolved = true;
		var enumerable = statement.Enumerable.AcceptVisitor(this);
		var body = statement.Body.AcceptVisitor(this);

		semanticModel.ExitScope();

		return new ResolvedStatement.ForEach(iteratorSymbol, enumerable, body);
	}

	public ResolvedStatement VisitRepeatStatement(Statement.Repeat statement)
	{
		semanticModel.EnterScope(statement);

		var repetitions = statement.Repetitions.AcceptVisitor(this);
		var body = statement.Body.AcceptVisitor(this);

		semanticModel.ExitScope();

		return new ResolvedStatement.Repeat(repetitions, body);
	}

	public ResolvedStatement VisitReturnStatement(Statement.Return statement)
	{
		var expression = statement.Expression?.AcceptVisitor(this);
		return new ResolvedStatement.Return(expression);
	}

	public ResolvedStatement VisitThrowStatement(Statement.Throw statement)
	{
		var expression = statement.Expression?.AcceptVisitor(this);
		return new ResolvedStatement.Throw(expression);
	}

	public ResolvedStatement VisitSealStatement(Statement.Seal statement)
	{
		var symbol = semanticModel.FindSymbol(statement.Identifier.Text);
		if (symbol is null)
			throw new Exception($"Failed to resolve symbol '{statement.Identifier.Text}'.");

		return new ResolvedStatement.Seal(symbol);
	}

	public ResolvedStatement VisitTryStatement(Statement.Try statement)
	{
		semanticModel.EnterScope(statement);

		var tryStatement = statement.TryStatement.AcceptVisitor(this);
		LocalVariableSymbol? catchVariableSymbol = null;

		if (statement.CatchNameToken is not null)
		{
			var declaration = new Declaration(statement.CatchNameToken, statement);
			catchVariableSymbol = new LocalVariableSymbol(statement.CatchNameToken.Text, declaration, statement.CatchType)
			{
				Type = statement.CatchType is null ? null : ResolveType(statement.CatchType),
				Resolved = true
			};
		}

		var catchStatement = statement.CatchStatement?.AcceptVisitor(this);
		var finallyStatement = statement.FinallyStatement?.AcceptVisitor(this);

		semanticModel.ExitScope();

		return new ResolvedStatement.Try(tryStatement, catchVariableSymbol, catchStatement, finallyStatement);
	}

	public ResolvedStatement VisitVariableDeclarationStatement(Statement.VariableDeclaration statement)
	{
		var symbol = semanticModel.FindSymbol(statement.Identifier.Text);
		if (symbol is not LocalVariableSymbol localVariableSymbol)
			throw new Exception($"Failed to resolved local variable '{statement.Identifier.Text}'.");

		var type = statement.Type is null ? null : ResolveType(statement.Type);
		var initializer = statement.Initializer?.AcceptVisitor(this);

		if (type is null)
		{
			if (initializer is null)
				throw new Exception($"Cannot infer type for variable '{statement.Identifier.Text}'.");

			type = initializer.Type;
		}

		localVariableSymbol.Type = type;
		localVariableSymbol.Resolved = true;

		return new ResolvedStatement.VariableDeclaration(localVariableSymbol, initializer);
	}

	public ResolvedStatement VisitLockStatement(Statement.Lock statement)
	{
		semanticModel.EnterScope(statement);

		var symbol = ResolveExpressionMemberSymbol(statement.Expression);
		if (symbol is not FieldSymbol fieldSymbol)
			throw new Exception("Invalid lock target.");

		var body = statement.Body.AcceptVisitor(this);

		semanticModel.ExitScope();

		return new ResolvedStatement.Lock(fieldSymbol, body);
	}

	public ResolvedStatement VisitSwitchStatement(Statement.Switch statement)
	{
		semanticModel.EnterScope(statement);

		var switchExpression = statement.Expression.AcceptVisitor(this);

		var sections = new List<ResolvedStatement.Switch.Section>();
		foreach (var section in statement.Sections)
		{
			semanticModel.EnterScope(section.Body);
			var body = section.Body.AcceptVisitor(this);
			var labels = new List<ResolvedStatement.Switch.Label>();
			foreach (var label in section.Labels)
			{
				switch (label)
				{
					case Statement.Switch.PatternLabel patternLabel:
						if (patternLabel.Identifier is null)
						{
							var type = ResolveType(patternLabel.Type);
							if (type is null)
								throw new Exception($"Failed to resolve type '{patternLabel.Type}'.");

							labels.Add(new ResolvedStatement.Switch.TypeMatchLabel(type));
						}
						else
						{
							var symbol = semanticModel.FindSymbol(patternLabel.Identifier.Text);
							if (symbol is not LocalVariableSymbol localVariableSymbol)
								throw new Exception(
									$"Failed to resolve local variable '{patternLabel.Identifier.Text}'.");

							localVariableSymbol.Type = ResolveType(patternLabel.Type);
							localVariableSymbol.Resolved = true;

							labels.Add(new ResolvedStatement.Switch.PatternLabel(localVariableSymbol));
						}

						break;
					case Statement.Switch.ExpressionLabel expressionLabel:
						var expression = expressionLabel.Expression.AcceptVisitor(this);
						labels.Add(new ResolvedStatement.Switch.ExpressionLabel(expression));
						break;
				}
			}
			semanticModel.ExitScope();
			sections.Add(new ResolvedStatement.Switch.Section(labels, body));
		}

		semanticModel.ExitScope();

		return new ResolvedStatement.Switch(switchExpression, sections);
	}

	public ResolvedStatement VisitExpressionStatement(Statement.ExpressionStatement statement)
	{
		var expression = statement.Expression.AcceptVisitor(this);
		return new ResolvedStatement.ExpressionStatement(expression);
	}

	private OperatorSymbol? ResolveOperator(string @operator)
	{
		if (NativeOperatorSymbol.LookupOperator(@operator) is { } nativeOperatorSymbol)
			return nativeOperatorSymbol;

		return null;
	}

	public ResolvedStatement VisitOperatorOverloadStatement(Statement.OperatorOverload statement)
	{
		var returnType = ResolveType(statement.ReturnType);
		if (returnType is null)
			throw new Exception($"Failed to resolve return type '{statement.ReturnType}'.");

		var parameters = new List<string>();
		foreach (var parameter in statement.ParameterList)
		{
			parameters.Add((parameter.IsReference ? "ref " : "") + parameter.Type);
		}

		var parameterName = string.Join(", ", parameters);
		var name = $"[{statement.Operator.TokenSpan}]({parameterName})";

		var symbol = semanticModel.FindSymbol(name);
		if (symbol is not MethodSymbol methodSymbol)
			throw new Exception("Failed to resolve operator overload.");

		semanticModel.EnterScope(statement);
		foreach (var parameter in statement.ParameterList)
		{
			var parameterDeclaration = new Declaration(parameter.Name, statement);
			semanticModel.AddSymbol(methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
				parameter.IsReference));

			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type '{parameter.Type}'.");

			methodSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		var body = statement.Body.AcceptVisitor(this);
		semanticModel.ExitScope();

		methodSymbol.Resolved = true;
		var operatorSymbol = ResolveOperator(statement.Operator.TokenSpan.ToString());
		if (operatorSymbol is null)
			throw new Exception($"Failed to resolve operator '{statement.Operator.TokenSpan}'.");

		return new ResolvedStatement.OperatorOverload(returnType, operatorSymbol, methodSymbol, body);
	}

	public ResolvedStatement VisitOperatorOverloadSignatureStatement(Statement.OperatorOverloadSignature statement)
	{var returnType = ResolveType(statement.ReturnType);
		if (returnType is null)
			throw new Exception($"Failed to resolve return type '{statement.ReturnType}'.");

		var parameters = new List<string>();
		foreach (var parameter in statement.ParameterList)
		{
			parameters.Add((parameter.IsReference ? "ref " : "") + parameter.Type);
		}

		var parameterName = string.Join(", ", parameters);
		var name = $"[{statement.Operator.TokenSpan}]({parameterName})";

		var symbol = semanticModel.FindSymbol(name);
		if (symbol is not MethodSymbol methodSymbol)
			throw new Exception("Failed to resolve operator overload.");

		semanticModel.EnterScope(statement);
		foreach (var parameter in statement.ParameterList)
		{
			var parameterDeclaration = new Declaration(parameter.Name, statement);
			semanticModel.AddSymbol(methodSymbol.AddParameter(parameter.Name.Text, parameterDeclaration,
				parameter.IsReference));

			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type '{parameter.Type}'.");

			methodSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		semanticModel.ExitScope();

		methodSymbol.Resolved = true;
		var operatorSymbol = ResolveOperator(statement.Operator.TokenSpan.ToString());
		if (operatorSymbol is null)
			throw new Exception($"Failed to resolve operator '{statement.Operator.TokenSpan}'.");

		return new ResolvedStatement.OperatorOverloadSignature(returnType, operatorSymbol, methodSymbol);
	}

	public ResolvedExpression VisitAwaitExpression(Expression.Await expression)
	{
		var resolvedExpression = expression.AcceptVisitor(this);
		return new ResolvedExpression.Await(resolvedExpression);
	}

	public ResolvedExpression VisitConditionalExpression(Expression.Conditional expression)
	{
		var condition = expression.Condition.AcceptVisitor(this);
		var trueExpression = expression.TrueExpression.AcceptVisitor(this);
		var falseExpression = expression.FalseExpression.AcceptVisitor(this);
		return new ResolvedExpression.Conditional(condition, trueExpression, falseExpression);
	}

	public ResolvedExpression VisitBinaryExpression(Expression.Binary expression)
	{
		var left = expression.Left.AcceptVisitor(this);
		var right = expression.Right.AcceptVisitor(this);

		if (left.Type is null || right.Type is null)
			throw new Exception("Cannot operate on void types.");

		var expressionType = expression.Operator.GetResultType(left.Type, right.Type);
		MethodSymbol? operatorMethod = null;
		if (expressionType is null)
		{
			// Look for operator overloads on either type
			var leftOverload = left.Type.TypeSymbol.FindOperatorOverload(left.Type, expression.Operator, right.Type);
			var rightOverload = right.Type.TypeSymbol.FindOperatorOverload(left.Type, expression.Operator, right.Type);

			if (leftOverload is not null)
			{
				if (rightOverload is not null)
					throw new Exception("Ambiguous operator overload.");

				expressionType = leftOverload.ReturnType;
				operatorMethod = leftOverload;
			}
			else if (rightOverload is not null)
			{
				expressionType = rightOverload.ReturnType;
				operatorMethod = rightOverload;
			}
		}

		if (expressionType is null)
			throw new Exception(
				$"Cannot use operator '{expression.Operator}' on types '{left.Type}' and '{right.Type}'.");

		return new ResolvedExpression.Binary(left, expression.Operator, right, expressionType, operatorMethod);
	}

	public ResolvedExpression VisitTypeRelationExpression(Expression.TypeRelation expression)
	{
		var operand = expression.Operand.AcceptVisitor(this);
		var typeQuery = expression.Type is null ? null : ResolveType(expression.Type);

		if (expression.IdentifierToken is null)
			return new ResolvedExpression.TypeRelation(operand, expression.Operator, typeQuery, null);

		var symbol = semanticModel.FindSymbol(expression.IdentifierToken.Text);
		if (symbol is not LocalVariableSymbol localVariableSymbol)
			throw new Exception($"Failed to resolve local variable '{expression.IdentifierToken.Text}'.");

		return new ResolvedExpression.TypeRelation(operand, expression.Operator, typeQuery, localVariableSymbol);
	}

	public ResolvedExpression VisitUnaryExpression(Expression.Unary expression)
	{
		var operand = expression.Operand.AcceptVisitor(this);

		if (operand.Type is null)
			throw new Exception("Cannot operate on void type.");

		ResolvedType? expressionType = null;
		MethodSymbol? operatorMethod = null;

		if (operand.Type.TypeSymbol is NativeTypeSymbol nativeType)
		{
			var resultType = expression.Operator.GetResultType(operand.Type);

			if (resultType is null)
				throw new Exception($"Cannot use operator '{expression.Operator}' on type '{nativeType.Name}'.");
		}
		else
		{
			// Search for operator overloading
			var leftOverload = left.Type.TypeSymbol.FindOperatorOverload(left.Type, expression.Operator, right.Type);
			var rightOverload = right.Type.TypeSymbol.FindOperatorOverload(left.Type, expression.Operator, right.Type);

			if (leftOverload is not null && rightOverload is not null)
				throw new Exception("Ambiguous operator overload.");

			if (leftOverload is not null)
			{
				expressionType = leftOverload.ReturnType;
				operatorMethod = leftOverload;
			}
			else if (rightOverload is not null)
			{
				expressionType = rightOverload.ReturnType;
				operatorMethod = rightOverload;
			}
		}

		return new ResolvedExpression.Unary(operand, expression.Operator, expressionType, operatorMethod);
	}

	public ResolvedExpression VisitIdentifierExpression(Expression.Identifier expression)
	{
		var symbol = semanticModel.FindSymbol(expression.NameToken.Text);
		if (symbol is null)
			throw new Exception($"Failed to resolve identifier '{expression.NameToken.Text}'.");

		// @TODO

		return new ResolvedExpression.Identifier(, symbol);
	}

	public ResolvedExpression VisitQualifierExpression(Expression.Qualifier expression)
	{
		var symbol = semanticModel.FindSymbol(expression.NameToken.Text);
		if (symbol is null)
			throw new Exception($"Failed to resolve qualifier '{expression.NameToken.Text}'.");

		// @TODO

		return new ResolvedExpression.Qualifier(, symbol);
	}

	public ResolvedExpression VisitCallExpression(Expression.Call expression)
	{
		return new ResolvedExpression.Call();
	}

	public ResolvedExpression VisitLiteralExpression(Expression.Literal expression)
	{
		return new ResolvedExpression.Literal();
	}

	public ResolvedExpression VisitInstantiateExpression(Expression.Instantiate expression)
	{
		return new ResolvedExpression.Instantiate();
	}

	public ResolvedExpression VisitCastExpression(Expression.Cast expression)
	{
		var resolvedExpression = expression.AcceptVisitor(this);
		var resolvedType = ResolveType(expression.TargetType);
		if (resolvedType is null)
			throw new Exception($"Failed to resolve type '{expression.TargetType}'.");

		return new ResolvedExpression.Cast(resolvedExpression, resolvedType, expression.IsConditional);
	}

	public ResolvedExpression VisitIndexExpression(Expression.Index expression)
	{
		return new ResolvedExpression.Index();
	}

	public ResolvedExpression VisitLambdaExpression(Expression.Lambda expression)
	{
		var lambdaSymbol = semanticModel.GetLambda(expression);
		if (lambdaSymbol is null)
			throw new Exception("Failed to resolve lambda.");

		var returnType = expression.ReturnType is null ? null : ResolveType(expression.ReturnType);
		var body = expression.Body.AcceptVisitor(this);

		return new ResolvedExpression.Lambda(lambdaSymbol, returnType, body);
	}
}
