using GuildScript.Analysis.Semantics.Symbols;
using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public sealed class Resolver : Statement.IVisitor<ResolvedStatement>, Expression.IVisitor<ResolvedExpression>
{
	private readonly SemanticModel semanticModel;
	private readonly Dictionary<TypeSymbol, SimpleResolvedType> typeSymbolLookup = new();

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
				var resolvedTemplateArguments = new List<ResolvedType?>();
				foreach (var template in templatedResolvedType.TypeArguments)
				{
					resolvedTemplateArguments.Add(ResolveTypeAlias(template));
				}
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
				{
					var newType = new SimpleResolvedType(typeSymbol);
					RegisterTypeSymbol(typeSymbol, newType);
					return newType;
				}

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

	private void RegisterTypeSymbol(TypeSymbol typeSymbol, SimpleResolvedType resolvedType)
	{
		typeSymbolLookup.Add(typeSymbol, resolvedType);
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

	private MemberSymbol? ResolveExpressionMemberSymbol(Symbol? owner, Expression expression)
	{
		switch (expression)
		{
			case Expression.Qualifier qualifier:
			{
				var symbol = owner?.GetChild(qualifier.NameToken.Text);
				if (symbol is MemberSymbol memberSymbol)
					return memberSymbol;

				throw new Exception($"The member '{qualifier.NameToken.Text}' does not exist in this context.");
			}
			case Expression.Identifier identifier:
			{
				var symbol = owner?.GetChild(identifier.NameToken.Text);
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

				var left = ResolveExpressionValueSymbol(binary.Left);
				if (left is null)
					throw new Exception($"'{binary.Left}' is not a valid access target.");
				
				semanticModel.VisitSymbol(left);
				var right = ResolveExpressionMemberSymbol(left, binary.Right);
				semanticModel.Return();

				return right;
			}
		}

		return null;
	}

	private Symbol? ResolveExpressionValueSymbol(Expression expression)
	{
		switch (expression)
		{
			case Expression.Qualifier qualifier:
			{
				var symbol = semanticModel.FindSymbol(qualifier.NameToken.Text);

				if (symbol is null)
					throw new Exception($"The symbol '{qualifier.NameToken.Text}' does not exist in this context.");
				
				return symbol switch
				{
					MemberSymbol memberSymbol => memberSymbol,
					LocalSymbol localSymbol => localSymbol,
					TypeSymbol typeSymbol => typeSymbol,
					_ => throw new Exception($"The symbol '{qualifier.NameToken.Text}' is not valid in this context.")
				};
			}
			case Expression.Identifier identifier:
			{
				var symbol = semanticModel.FindSymbol(identifier.NameToken.Text);

				if (symbol is null)
					throw new Exception($"The symbol '{identifier.NameToken.Text}' does not exist in this context.");
				
				return symbol switch
				{
					MemberSymbol memberSymbol => memberSymbol,
					LocalSymbol localSymbol => localSymbol,
					TypeSymbol typeSymbol => typeSymbol,
					_ => throw new Exception($"The symbol '{identifier.NameToken.Text}' is not valid in this context.")
				};
			}
			case Expression.Binary binary:
			{
				var operation = binary.Operator.Operation;
				if (operation != BinaryOperator.BinaryOperation.Access &&
					operation != BinaryOperator.BinaryOperation.ConditionalAccess)
					throw new Exception($"Invalid operator for member resolution: {binary.Operator}");

				var left = ResolveExpressionValueSymbol(binary.Left);
				if (left is null)
					throw new Exception($"The symbol '{binary.Left}' does not exist in this context.");
				
				semanticModel.VisitSymbol(left);
				var right = ResolveExpressionMemberSymbol(left, binary.Right);
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
		
		// Process imported modules
		foreach (var importedModule in statement.ImportedModules)
		{
			var importedModuleSymbol = semanticModel.GetModule(importedModule);
			if (importedModuleSymbol is null)
				throw new Exception($"The module '{importedModule}' does not exist.");

			semanticModel.CurrentScope?.ImportModule(importedModuleSymbol);
		}
		
		var statements = new List<ResolvedStatement>();
		foreach (var topLevelStatement in statement.Statements)
		{
			statements.Add(topLevelStatement.AcceptVisitor(this));
		}
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
		return new ResolvedStatement.EntryPoint(entryPointSymbol, entryPointSymbol.GetParameters(), body);
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
		semanticModel.EnterScope(statement);
		
		var statements = new List<ResolvedStatement>();
		foreach (var stmt in statement.Statements)
		{
			statements.Add(stmt.AcceptVisitor(this));
		}
		
		semanticModel.ExitScope();
		
		return new ResolvedStatement.Block(statements);
	}

	public ResolvedStatement VisitClassStatement(Statement.Class statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not ClassSymbol classSymbol)
			throw new Exception($"Failed to find class symbol '{statement.NameToken.Text}'.");

		semanticModel.VisitSymbol(classSymbol);
		semanticModel.EnterScope(statement);

		var members = new List<ResolvedStatement>();
		foreach (var member in statement.Members)
		{
			members.Add(member.AcceptVisitor(this));
		}

		semanticModel.ExitScope();
		semanticModel.Return();
		return new ResolvedStatement.Class(classSymbol.AccessModifier, classSymbol.ClassModifier, classSymbol,
			classSymbol.GetTemplateParameters(), classSymbol.BaseClass, members);
	}

	public ResolvedStatement VisitStructStatement(Statement.Struct statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not StructSymbol structSymbol)
			throw new Exception($"Failed to find struct symbol '{statement.NameToken.Text}'.");

		semanticModel.VisitSymbol(structSymbol);
		semanticModel.EnterScope(statement);

		var members = new List<ResolvedStatement>();
		foreach (var member in statement.Members)
		{
			members.Add(member.AcceptVisitor(this));
		}

		semanticModel.ExitScope();
		semanticModel.Return();

		return new ResolvedStatement.Struct(structSymbol.AccessModifier, structSymbol.StructModifier, structSymbol,
			members, structSymbol.GetTemplateParameters());
	}

	public ResolvedStatement VisitInterfaceStatement(Statement.Interface statement)
	{
		var symbol = semanticModel.FindSymbol(statement.NameToken.Text);
		if (symbol is not InterfaceSymbol interfaceSymbol)
			throw new Exception($"Failed to find interface symbol '{statement.NameToken.Text}'.");

		semanticModel.VisitSymbol(interfaceSymbol);
		semanticModel.EnterScope(statement);

		var members = new List<ResolvedStatement>();
		foreach (var member in statement.Members)
		{
			members.Add(member.AcceptVisitor(this));
		}

		semanticModel.ExitScope();
		semanticModel.Return();

		return new ResolvedStatement.Interface(interfaceSymbol.AccessModifier, interfaceSymbol, members,
			interfaceSymbol.GetTemplateParameters());
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

		var members = new List<ResolvedStatement.Enum.Member>();
		foreach (var member in statement.Members)
		{
			members.Add(member.Expression is null
				? new ResolvedStatement.Enum.Member(member.Identifier.Text, null)
				: new ResolvedStatement.Enum.Member(member.Identifier.Text, member.Expression.AcceptVisitor(this)));
		}

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
		return new ResolvedStatement.ExternalMethod(externalMethodSymbol, externalMethodSymbol.GetParameters());
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

		if (statement.Initializer is not null)
		{
			var argumentTypes = new List<ResolvedType?>();
			var arguments = new List<ResolvedExpression>();

			foreach (var argument in statement.ArgumentList)
			{
				var resolvedExpression = argument.AcceptVisitor(this);
				if (resolvedExpression.Type is null)
					throw new Exception("Failed to resolve initializer argument type.");
				
				arguments.Add(resolvedExpression);
				argumentTypes.Add(resolvedExpression.Type);
			}

			var lookupTarget = statement.Initializer.Text switch
			{
				"this" => semanticModel.CurrentType,
				"base" => semanticModel.CurrentType?.Ancestor,
				_      => null
			};

			if (lookupTarget is null)
				throw new Exception("Failed to resolve initializer target.");

			ConstructorSymbol? initializer = null;
			foreach (var child in lookupTarget.Children)
			{
				if (child is not ConstructorSymbol inherited)
					continue;
				
				initializer = inherited;
				break;
			}

			initializer = initializer?.FindOverload(argumentTypes) as ConstructorSymbol;
			if (initializer is null)
				throw new Exception($"Failed to find constructor initializer '{statement.Initializer.Text}'");

			constructorSymbol.Initializer = initializer;
			constructorSymbol.InitializerArguments = arguments;
		}

		constructorSymbol.Resolved = true;
		return new ResolvedStatement.Constructor(constructorSymbol.AccessModifier, constructorSymbol, body);
	}

	public ResolvedStatement VisitIndexerStatement(Statement.Indexer statement)
	{
		var symbol = semanticModel.FindSymbol("this[]");
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


		var body = new List<ResolvedStatement>();
		foreach (var bodyStatement in statement.Body)
		{
			body.Add(bodyStatement.AcceptVisitor(this));
		}

		semanticModel.ExitScope();
		semanticModel.Return();

		indexerSymbol.Resolved = true;
		return new ResolvedStatement.Indexer(indexerSymbol.AccessModifier, indexerSymbol.GetParameters(), body,
			indexerSymbol);
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

		var body = new List<ResolvedStatement>();
		foreach (var bodyStatement in statement.Body)
		{
			body.Add(bodyStatement.AcceptVisitor(this));
		}

		semanticModel.ExitScope();
		semanticModel.Return();

		propertySymbol.Type = type;
		propertySymbol.Resolved = true;
		return new ResolvedStatement.Property(propertySymbol.AccessModifier, propertySymbol.Modifiers, propertySymbol,
			body);
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

		var body = new List<ResolvedStatement>();
		foreach (var bodyStatement in statement.Body)
		{
			body.Add(bodyStatement.AcceptVisitor(this));
		}

		semanticModel.ExitScope();
		semanticModel.Return();

		propertySymbol.Resolved = true;
		propertySymbol.Type = type;
		return new ResolvedStatement.PropertySignature(propertySymbol.Modifiers, propertySymbol, body);
	}

	public ResolvedStatement VisitMethodStatement(Statement.Method statement)
	{
		var methodSymbol = semanticModel.GetMethod(statement);
		if (methodSymbol is null)
			throw new Exception($"Failed to find method symbol '{statement.NameToken.Text}'.");

		var returnType = statement.ReturnType is null ? null : ResolveType(statement.ReturnType);

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

		methodSymbol.Resolved = true;
		methodSymbol.ReturnType = returnType;
		return new ResolvedStatement.Method(methodSymbol.AccessModifier, methodSymbol.Modifiers, methodSymbol, body,
			methodSymbol.GetParameters(), statement.AsyncToken is not null, methodSymbol.GetTemplateParameters());
	}

	public ResolvedStatement VisitMethodSignatureStatement(Statement.MethodSignature statement)
	{
		var methodSymbol = semanticModel.GetMethod(statement);
		if (methodSymbol is null)
			throw new Exception($"Failed to find method symbol '{statement.NameToken.Text}'.");


		var returnType = statement.ReturnType is null ? null : ResolveType(statement.ReturnType);

		semanticModel.EnterScope(statement);

		foreach (var parameter in statement.ParameterList)
		{
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			methodSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		semanticModel.ExitScope();

		methodSymbol.Resolved = true;
		methodSymbol.ReturnType = returnType;
		return new ResolvedStatement.MethodSignature(methodSymbol.Modifiers, methodSymbol, methodSymbol.GetParameters(),
			statement.AsyncToken is not null, methodSymbol.GetTemplateParameters());
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
		return new ResolvedStatement.Field(fieldSymbol.AccessModifier, fieldSymbol.Modifiers, fieldSymbol, initializer);
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
			throw new Exception($"Failed to resolve local variable '{statement.Identifier.Text}'.");

		var type = statement.Type is null ? null : ResolveType(statement.Type);
		var initializer = statement.Initializer?.AcceptVisitor(this);

		type ??= initializer?.Type ??
				 throw new Exception($"Cannot infer type for variable '{statement.Identifier.Text}'.");

		localVariableSymbol.Type = type;
		localVariableSymbol.Resolved = true;

		return new ResolvedStatement.VariableDeclaration(localVariableSymbol, initializer);
	}

	public ResolvedStatement VisitLockStatement(Statement.Lock statement)
	{
		semanticModel.EnterScope(statement);

		var symbol = ResolveExpressionValueSymbol(statement.Expression);
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
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			methodSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		var body = statement.Body.AcceptVisitor(this);
		
		semanticModel.ExitScope();

		methodSymbol.Resolved = true;
		methodSymbol.ReturnType = returnType;
		var operatorSymbol = ResolveOperator(statement.Operator.TokenSpan.ToString());
		if (operatorSymbol is null)
			throw new Exception($"Failed to resolve operator '{statement.Operator.TokenSpan}'.");

		return new ResolvedStatement.OperatorOverload(operatorSymbol, methodSymbol, body);
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
			var parameterType = ResolveType(parameter.Type);
			if (parameterType is null)
				throw new Exception($"Failed to resolve type of parameter '{parameter.Name.Text}'.");

			methodSymbol.ResolveParameter(parameter.Name.Text, parameterType);
		}

		semanticModel.ExitScope();

		methodSymbol.Resolved = true;
		methodSymbol.ReturnType = returnType;
		var operatorSymbol = ResolveOperator(statement.Operator.TokenSpan.ToString());
		if (operatorSymbol is null)
			throw new Exception($"Failed to resolve operator '{statement.Operator.TokenSpan}'.");

		return new ResolvedStatement.OperatorOverloadSignature(operatorSymbol, methodSymbol);
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

		if (left.Type is null)
			throw new Exception("Cannot operate on void types.");

		if (expression.Operator.Operation is BinaryOperator.BinaryOperation.Access
			or BinaryOperator.BinaryOperation.ConditionalAccess)
		{
			semanticModel.VisitSymbol(left.Type.TypeSymbol);
			var resolvedRight = expression.Right.AcceptVisitor(this);
			semanticModel.Return();
			return resolvedRight;
		}

		var right = expression.Right.AcceptVisitor(this);
		if (right.Type is null)
			throw new Exception("Cannot operate on void types.");

		var simpleOperator = expression.Operator;
		BinaryOperator? assignmentOperator = null;
		var isCompoundAssignment = expression.Operator.IsCompoundAssignment;
		if (isCompoundAssignment)
		{
			(simpleOperator, assignmentOperator) = expression.Operator.Deconstruct();
		}

		var expressionType = simpleOperator.GetResultType(left.Type, right.Type);
		MethodSymbol? operatorMethod = null;
		if (expressionType is null)
		{
			// Look for operator overloads on either type
			var leftOverload = left.Type.TypeSymbol.FindOperatorOverload(left.Type, simpleOperator, right.Type);
			var rightOverload = left.Type.Equals(right.Type)
				? null
				: right.Type.TypeSymbol.FindOperatorOverload(left.Type, simpleOperator, right.Type);

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

		if (expressionType is null && expression.Operator.Operation is BinaryOperator.BinaryOperation.Equality
			or BinaryOperator.BinaryOperation.Inequality)
		{
			expressionType = SimpleResolvedType.Bool;
		}

		if (expressionType is null)
			throw new Exception(
				$"Cannot use operator '{simpleOperator}' on types '{left.Type}' and '{right.Type}'.");

		if (operatorMethod is null)
		{
			if (!isCompoundAssignment || assignmentOperator is null)
				return new ResolvedExpression.Binary(left, simpleOperator, right, expressionType);
			
			var resolvedExpression = new ResolvedExpression.Binary(left, simpleOperator, right, expressionType);
			return new ResolvedExpression.Binary(left, assignmentOperator, resolvedExpression, left.Type);
		}

		var arguments = new List<ResolvedExpression> { left, right };
		var callExpression = new ResolvedExpression.Call(operatorMethod, Array.Empty<TypeSymbol>(), arguments);

		if (isCompoundAssignment && assignmentOperator is not null)
		{
			return new ResolvedExpression.Binary(left, assignmentOperator, callExpression, left.Type);
		}
		
		return callExpression;
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
			var overload = operand.Type.TypeSymbol.FindOperatorOverload(operand.Type, expression.Operator);

			if (overload is not null)
			{
				expressionType = overload.ReturnType;
				operatorMethod = overload;
			}
		}

		return new ResolvedExpression.Unary(operand, expression.Operator, expressionType, operatorMethod);
	}

	public ResolvedExpression VisitIdentifierExpression(Expression.Identifier expression)
	{
		var symbol = semanticModel.FindSymbol(expression.NameToken.Text);
		switch (symbol)
		{
			case null:
				throw new Exception($"Failed to resolve identifier '{expression.NameToken.Text}'.");
			case ITypedSymbol typedSymbol:
			{
				if (!typedSymbol.Resolved || typedSymbol.Type is not { } type)
					throw new Exception($"Cannot reference '{expression.NameToken.Text}' before it has been resolved.");

				return new ResolvedExpression.Identifier(type, symbol);
			}
			case TypeSymbol typeSymbol:
				return new ResolvedExpression.Identifier(new SimpleResolvedType(typeSymbol), symbol);
			default:
				throw new Exception($"'{expression.NameToken.Text}' is not a valid access target.");
		}
	}

	public ResolvedExpression VisitQualifierExpression(Expression.Qualifier expression)
	{
		var symbol = semanticModel.FindSymbol(expression.NameToken.Text);
		switch (symbol)
		{
			case null:
				throw new Exception($"Failed to resolve qualifier '{expression.NameToken.Text}'.");
			case ITypedSymbol typedSymbol:
			{
				if (!typedSymbol.Resolved || typedSymbol.Type is not { } type)
					throw new Exception($"Cannot reference '{expression.NameToken.Text}' before it has been resolved.");

				return new ResolvedExpression.Qualifier(type, symbol);
			}
			case TypeSymbol typeSymbol:
				return new ResolvedExpression.Qualifier(new SimpleResolvedType(typeSymbol), symbol);
			default:
				throw new Exception($"'{expression.NameToken.Text}' is not a valid access target.");
		}
	}

	public ResolvedExpression VisitCallExpression(Expression.Call expression)
	{
		var sourceSymbol = ResolveExpressionValueSymbol(expression.Function);
		if (sourceSymbol is not ICallable callable)
			throw new Exception("Invalid call target.");

		var argumentTypes = new List<ResolvedType?>();
		var arguments = new List<ResolvedExpression>();
		foreach (var argument in expression.Arguments)
		{
			var resolvedArgument = argument.AcceptVisitor(this);
			arguments.Add(resolvedArgument);
			argumentTypes.Add(resolvedArgument.Type);
		}

		var templateArguments = new List<TypeSymbol>();
		foreach (var templateArgument in expression.TemplateArguments)
		{
			var typeSymbol = ResolveExpressionTypeSymbol(templateArgument);
			if (typeSymbol is null)
				throw new Exception("Failed to resolve template argument.");
			
			templateArguments.Add(typeSymbol);
		}

		var overload = callable.FindOverload(argumentTypes, templateArguments.Count);
		if (overload is null)
			throw new Exception("Invalid call target.");

		return new ResolvedExpression.Call(overload, templateArguments, arguments);
	}

	public ResolvedExpression VisitLiteralExpression(Expression.Literal expression)
	{
		var thisSymbol = semanticModel.CurrentType;
		ResolvedType? type;
		switch (expression.Token.Type)
		{
			case SyntaxTokenType.This:
				if (thisSymbol is null)
					throw new Exception("Invalid usage of 'this'.");

				type = typeSymbolLookup.TryGetValue(thisSymbol, out var simpleType) ? simpleType : null;
				break;
			case SyntaxTokenType.Base:
				if (thisSymbol is not ClassSymbol classSymbol)
					throw new Exception("'base' is only valid within classes.");

				if (classSymbol.BaseClass is not { } baseClass)
					throw new Exception("No base class found.");
				
				type = typeSymbolLookup.TryGetValue(baseClass, out var inheritedType) ? inheritedType : null;
				break;
			case SyntaxTokenType.Int8Constant:
				type = SimpleResolvedType.Int8;
				break;
			case SyntaxTokenType.Int16Constant:
				type = SimpleResolvedType.Int16;
				break;
			case SyntaxTokenType.Int32Constant:
				type = SimpleResolvedType.Int32;
				break;
			case SyntaxTokenType.Int64Constant:
				type = SimpleResolvedType.Int64;
				break;
			case SyntaxTokenType.UInt8Constant:
				type = SimpleResolvedType.UInt8;
				break;
			case SyntaxTokenType.UInt16Constant:
				type = SimpleResolvedType.UInt16;
				break;
			case SyntaxTokenType.UInt32Constant:
				type = SimpleResolvedType.UInt32;
				break;
			case SyntaxTokenType.UInt64Constant:
				type = SimpleResolvedType.UInt64;
				break;
			case SyntaxTokenType.SingleConstant:
				type = SimpleResolvedType.Single;
				break;
			case SyntaxTokenType.DoubleConstant:
				type = SimpleResolvedType.Double;
				break;
			case SyntaxTokenType.StringConstant:
				type = SimpleResolvedType.String;
				break;
			case SyntaxTokenType.CharacterConstant:
				type = SimpleResolvedType.Char;
				break;
			case SyntaxTokenType.True:
			case SyntaxTokenType.False:
				type = SimpleResolvedType.Bool;
				break;
			case SyntaxTokenType.Null:
				type = null;
				break;
			default:
				throw new Exception("Invalid literal expression.");
		}
		
		return new ResolvedExpression.Literal(expression.Token, type);
	}

	public ResolvedExpression VisitInstantiateExpression(Expression.Instantiate expression)
	{
		var type = ResolveType(expression.InstanceType);

		if (type is null)
			throw new Exception($"Failed to resolve type '{expression.InstanceType}'.");
		
		var arguments = new List<ResolvedExpression>();
		foreach (var argument in expression.Arguments)
		{
			arguments.Add(argument.AcceptVisitor(this));
		}
		
		semanticModel.VisitSymbol(type.TypeSymbol);
		
		var initializers = new List<ResolvedExpression>();
		foreach (var initializer in expression.Initializers)
		{
			initializers.Add(initializer.AcceptVisitor(this));
		}
		
		semanticModel.Return();
		
		return new ResolvedExpression.Instantiate(type, arguments, initializers);
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
		var sourceSymbol = ResolveExpressionValueSymbol(expression.Expression);
		if (sourceSymbol is not ITypedSymbol typedSymbol)
			throw new Exception("Invalid indexing target.");
		
		var key = expression.Key.AcceptVisitor(this);

		ResolvedType? resolvedType = null;
		var indexerSymbol = typedSymbol.Type?.TypeSymbol.FindIndexer(key.Type);
		switch (typedSymbol.Type)
		{
			case ArrayResolvedType arrayResolvedType:
				if (!Equals(key.Type, SimpleResolvedType.Int8) && !Equals(key.Type, SimpleResolvedType.UInt8) &&
					!Equals(key.Type, SimpleResolvedType.Int16) && !Equals(key.Type, SimpleResolvedType.UInt16) &&
					!Equals(key.Type, SimpleResolvedType.Int32) && !Equals(key.Type, SimpleResolvedType.UInt32) &&
					!Equals(key.Type, SimpleResolvedType.Int64) && !Equals(key.Type, SimpleResolvedType.UInt64))
					throw new Exception("Invalid indexing key.");
				
				resolvedType = arrayResolvedType.ElementType;
				break;
			case SimpleResolvedType simpleResolvedType:
				if (indexerSymbol is null)
					throw new Exception("Cannot find a matching indexer statement in indexing target.");

				resolvedType = simpleResolvedType;
				break;
			case TemplatedResolvedType templatedResolvedType:
				// @TODO
				break;
		}

		if (resolvedType is null)
			throw new Exception("Invalid indexing target.");

		return new ResolvedExpression.Index(indexerSymbol, key, expression.IsConditional, resolvedType);
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
