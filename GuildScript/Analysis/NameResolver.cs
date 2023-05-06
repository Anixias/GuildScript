using System.Diagnostics.CodeAnalysis;
using GuildScript.Analysis.Semantics;
using GuildScript.Analysis.Syntax;
using GuildScript.Analysis.Text;

namespace GuildScript.Analysis;

/*public sealed class NameResolver : Statement.IVisitor, Expression.IVisitor
{
	public DiagnosticCollection Diagnostics { get; }
	public Statement.EntryPoint? EntryPoint { get; private set; }

	public class Scope
	{
		public Scope? EnclosingScope { get; }
		public IEnumerable<Scope> NestedScopes => nestedScopes;
		
		private readonly DeclarationCollection declarations = new();
		private readonly Dictionary<string, ResolvedType> resolvedTypes = new();
		private readonly List<Scope> nestedScopes = new();

		public Scope(Scope? enclosingScope)
		{
			EnclosingScope = enclosingScope;
			enclosingScope?.nestedScopes.Add(this);
		}
		
		public Declaration? Resolve(string identifier)
		{
			return declarations.TryGetDeclaration(identifier, out var declaration)
				? declaration
				: EnclosingScope?.Resolve(identifier);
		}

		public bool Declare(Declaration declaration)
		{
			return declarations.Declare(declaration.SourceIdentifier.Text, declaration);
		}

		public ResolvedType? RegisterType(Declaration declaration)
		{
			var resolvedType = new ResolvedType(declaration, this);
			return !resolvedTypes.TryAdd(resolvedType.Name, resolvedType) ? null : resolvedType;
		}

		public ResolvedType? RegisterType(ResolvedType parent, Declaration declaration)
		{
			var resolvedType = new ResolvedType(parent, declaration, this);
			return !resolvedTypes.TryAdd(resolvedType.Name, resolvedType) ? null : resolvedType;
		}

		public ResolvedType? ResolveType(string identifier)
		{
			return resolvedTypes.TryGetValue(identifier, out var type) ? type : null;
		}
	}

	private class DeclarationCollection
	{
		private readonly Dictionary<string, Declaration> collection = new();

		public bool Declare(string identifier, Declaration declaration)
		{
			return collection.TryAdd(identifier, declaration);
		}

		public bool TryGetDeclaration(string identifier, [MaybeNullWhen(false)] out Declaration declaration)
		{
			declaration = null;
			if (!collection.TryGetValue(identifier, out var output))
				return false;
			
			declaration = output;
			return true;

		}
	}

	public Scope GlobalScope { get; } = new(null);
	private Scope currentScope;
	private readonly SemanticModel semanticModel;
	private readonly Dictionary<ModuleName, Scope> modules = new();
	private readonly Stack<Scope> scopeStack = new();
	private readonly Stack<ResolvedType> typeStack = new();
	private ResolvedType? CurrentType => typeStack.TryPeek(out var type) ? type : null;

	public NameResolver(SemanticModel semanticModel)
	{
		this.semanticModel = semanticModel;
		Diagnostics = new DiagnosticCollection();
		currentScope = GlobalScope;
	}

	public void AppendDiagnostics(DiagnosticCollection diagnostics)
	{
		Diagnostics.AppendDiagnostics(diagnostics);
	}

	public void Finish()
	{
		if (EntryPoint is null)
		{
			Diagnostics.ReportNameResolverEntryPointNotDefined();
		}
	}

	public void Resolve(IEnumerable<Statement> statements)
	{
		foreach (var statement in statements)
		{
			Resolve(statement);
		}
	}

	private void Resolve(Statement statement)
	{
		statement.AcceptVisitor(this);
	}

	private void Resolve(Expression expression)
	{
		expression.AcceptVisitor(this);
	}

	public void Resolve(SyntaxNode node)
	{
		switch (node)
		{
			case Statement statement:
				Resolve(statement);
				break;
			case Expression expression:
				Resolve(expression);
				break;
			default:
				throw new InvalidOperationException("Unexpected node type.");
		}
	}

	private void Declare(Declaration declaration)
	{
		if (!currentScope.Declare(declaration))
			Diagnostics.ReportNameResolverDuplicateDeclaration(declaration.SourceIdentifier);
	}

	private ResolvedType? ResolveType(Scope? scope, SyntaxToken identifier)
	{
		while (scope is not null)
		{
			if (scope.ResolveType(identifier.Text) is { } type)
				return type;

			scope = scope.EnclosingScope;
		}

		Diagnostics.ReportNameResolverUnresolvedType(identifier);
		return null;
	}

	private ResolvedType? DeclareType(Declaration declaration)
	{
		if (!currentScope.Declare(declaration))
		{
			Diagnostics.ReportNameResolverDuplicateDeclaration(declaration.SourceIdentifier);
			return null;
		}

		var parentType = CurrentType;
		var newType = parentType is null
			? currentScope.RegisterType(declaration)
			: currentScope.RegisterType(parentType, declaration);

		if (newType is null)
		{
			Diagnostics.ReportNameResolverDuplicateDeclaration(declaration.SourceIdentifier);
		}
		
		return newType;
	}

	private void BeginModule(ModuleName module)
	{
		Scope moduleScope;
		if (modules.TryGetValue(module, out var existingScope))
		{
			moduleScope = existingScope;
		}
		else
		{
			moduleScope = new Scope(currentScope);
			modules.Add(module, moduleScope);
		}

		scopeStack.Push(currentScope);
		currentScope = moduleScope;
	}

	private void EndModule()
	{
		currentScope = scopeStack.Pop();
	}

	private void BeginScope()
	{
		currentScope = new Scope(currentScope);
	}

	private void EndScope()
	{
		currentScope = currentScope.EnclosingScope ?? throw new InvalidOperationException("Cannot end global scope.");
	}
	
	public void VisitProgramStatement(Statement.Program statement)
	{
		// Module scope
		BeginModule(statement.Module);

		foreach (var member in statement.Statements)
		{
			Resolve(member);
		}
		
		EndModule();
	}

	public void VisitEntryPointStatement(Statement.EntryPoint statement)
	{
		if (EntryPoint is not null)
		{
			Diagnostics.ReportNameResolveEntryPointRedefined(statement.Identifier);
			return;
		}
		
		EntryPoint = statement;
		Declare(new Declaration(statement.Identifier, statement));
		
		BeginScope();

		foreach (var parameter in statement.ParameterList)
		{
			Declare(new Declaration(parameter.Name, statement));
		}
		
		Resolve(statement.Body);
		EndScope();
	}

	public void VisitDefineStatement(Statement.Define statement)
	{
		DeclareType(new Declaration(statement.Identifier, statement));
	}

	public void VisitBlockStatement(Statement.Block statement)
	{
		BeginScope();

		foreach (var child in statement.Statements)
		{
			Resolve(child);
		}
		
		EndScope();
	}

	public void VisitClassStatement(Statement.Class statement)
	{
		var type = DeclareType(new Declaration(statement.NameToken, statement));
		if (type is null)
			return;
		
		BeginScope();
		typeStack.Push(type);

		foreach (var typeParameter in statement.TypeParameters)
		{
			Declare(new Declaration(typeParameter, statement));
		}

		Resolve(statement.Members);
		typeStack.Pop();
		EndScope();
	}

	public void VisitStructStatement(Statement.Struct statement)
	{
		var type = DeclareType(new Declaration(statement.NameToken, statement));
		if (type is null)
			return;
		
		BeginScope();
		typeStack.Push(type);
		Resolve(statement.Members);
		typeStack.Pop();
		EndScope();
	}

	public void VisitInterfaceStatement(Statement.Interface statement)
	{
		var type = DeclareType(new Declaration(statement.NameToken, statement));
		if (type is null)
			return;
		
		BeginScope();
		typeStack.Push(type);
		Resolve(statement.Members);
		typeStack.Pop();
		EndScope();
	}

	public void VisitEnumStatement(Statement.Enum statement)
	{
		var type = DeclareType(new Declaration(statement.NameToken, statement));
		if (type is null)
			return;
		
		BeginScope();
		typeStack.Push(type);
		
		foreach (var member in statement.Members)
		{
			Declare(new Declaration(member.Identifier, statement));
		}
		
		typeStack.Pop();
		EndScope();
	}

	public void VisitDestructorStatement(Statement.Destructor statement)
	{
		BeginScope();
		Resolve(statement.Body);
		EndScope();
	}

	public void VisitExternalMethodStatement(Statement.ExternalMethod statement)
	{
		Declare(new Declaration(statement.Identifier, statement));
	}

	public void VisitConstructorStatement(Statement.Constructor statement)
	{
		BeginScope();
		
		foreach (var parameter in statement.ParameterList)
		{
			Declare(new Declaration(parameter.Name, statement));
		}
		
		Resolve(statement.Body);
		
		EndScope();
	}

	public void VisitIndexerStatement(Statement.Indexer statement)
	{
		BeginScope();
		
		foreach (var parameter in statement.ParameterList)
		{
			Declare(new Declaration(parameter.Name, statement));
		}
		
		Resolve(statement.Body);
		
		EndScope();
	}

	public void VisitAccessorTokenStatement(Statement.AccessorToken statement)
	{
		
	}

	public void VisitAccessorLambdaStatement(Statement.AccessorLambda statement)
	{
		BeginScope();
		Resolve(statement.LambdaExpression);
		EndScope();
	}

	public void VisitAccessorLambdaSignatureStatement(Statement.AccessorLambdaSignature statement)
	{
		
	}

	public void VisitEventStatement(Statement.Event statement)
	{
		Declare(new Declaration(statement.NameToken, statement));
		
		BeginScope();
		
		foreach (var parameter in statement.ParameterList)
		{
			Declare(new Declaration(parameter.Name, statement));
		}
		
		EndScope();
	}

	public void VisitEventSignatureStatement(Statement.EventSignature statement)
	{
		Declare(new Declaration(statement.NameToken, statement));
		
		BeginScope();
		
		foreach (var parameter in statement.ParameterList)
		{
			Declare(new Declaration(parameter.Name, statement));
		}
		
		EndScope();
	}

	public void VisitPropertyStatement(Statement.Property statement)
	{
		Declare(new Declaration(statement.NameToken, statement));
	}

	public void VisitPropertySignatureStatement(Statement.PropertySignature statement)
	{
		Declare(new Declaration(statement.NameToken, statement));
	}

	public void VisitMethodStatement(Statement.Method statement)
	{
		Declare(new Declaration(statement.NameToken, statement));
		BeginScope();
		
		foreach (var parameter in statement.ParameterList)
		{
			Declare(new Declaration(parameter.Name, statement));
		}
		
		Resolve(statement.Body);
		
		EndScope();
	}

	public void VisitMethodSignatureStatement(Statement.MethodSignature statement)
	{
		Declare(new Declaration(statement.NameToken, statement));
		BeginScope();
		
		foreach (var parameter in statement.ParameterList)
		{
			Declare(new Declaration(parameter.Name, statement));
		}

		EndScope();
	}

	public void VisitFieldStatement(Statement.Field statement)
	{
		Declare(new Declaration(statement.NameToken, statement));
	}

	public void VisitBreakStatement(Statement.Break statement)
	{
		
	}

	public void VisitContinueStatement(Statement.Continue statement)
	{
		
	}

	public void VisitControlStatement(Statement.Control statement)
	{
		Resolve(statement.IfExpression);

		BeginScope();
		Resolve(statement.IfStatement);
		EndScope();

		if (statement.ElseStatement is null)
			return;
		
		Resolve(statement.ElseStatement);
	}

	public void VisitWhileStatement(Statement.While statement)
	{
		BeginScope();
		Resolve(statement.Condition);
		Resolve(statement.Body);
		EndScope();
	}

	public void VisitDoWhileStatement(Statement.DoWhile statement)
	{
		BeginScope();
		Resolve(statement.Body);
		Resolve(statement.Condition);
		EndScope();
	}

	public void VisitForStatement(Statement.For statement)
	{
		BeginScope();
		
		if (statement.Initializer is not null)
			Resolve(statement.Initializer);
		
		if (statement.Condition is not null)
			Resolve(statement.Condition);
		
		if (statement.Increment is not null)
			Resolve(statement.Increment);
		
		Resolve(statement.Body);
		EndScope();
	}

	public void VisitForEachStatement(Statement.ForEach statement)
	{
		BeginScope();
		Declare(new Declaration(statement.Iterator, statement));
		Resolve(statement.Enumerable);
		Resolve(statement.Body);
		EndScope();
	}

	public void VisitRepeatStatement(Statement.Repeat statement)
	{
		BeginScope();
		Resolve(statement.Repetitions);
		Resolve(statement.Body);
		EndScope();
	}

	public void VisitReturnStatement(Statement.Return statement)
	{
		if (statement.Expression is null)
			return;
		
		Resolve(statement.Expression);
	}

	public void VisitThrowStatement(Statement.Throw statement)
	{
		if (statement.Expression is null)
			return;
		
		Resolve(statement.Expression);
	}

	public void VisitSealStatement(Statement.Seal statement)
	{
		
	}

	public void VisitTryStatement(Statement.Try statement)
	{
		BeginScope();
		Resolve(statement.TryStatement);
		EndScope();

		if (statement.CatchStatement is not null)
		{
			BeginScope();
			
			if (statement.CatchNameToken is not null)
			{
				Declare(new Declaration(statement.CatchNameToken, statement.CatchStatement));
			}
			
			Resolve(statement.CatchStatement);

			EndScope();
		}

		if (statement.FinallyStatement is null)
			return;
		
		BeginScope();
		Resolve(statement.FinallyStatement);
		EndScope();
	}

	public void VisitVariableDeclarationStatement(Statement.VariableDeclaration statement)
	{
		Declare(new Declaration(statement.Identifier, statement));
	}

	public void VisitLockStatement(Statement.Lock statement)
	{
		BeginScope();
		Resolve(statement.Expression);
		Resolve(statement.Body);
		EndScope();
	}

	public void VisitSwitchStatement(Statement.Switch statement)
	{
		BeginScope();
		Resolve(statement.Expression);

		foreach (var section in statement.Sections)
		{
			BeginScope();
			foreach (var label in section.Labels)
			{
				switch (label)
				{
					case Statement.Switch.PatternLabel patternLabel:
						if (patternLabel.Identifier is not null)
						{
							Declare(new Declaration(patternLabel.Identifier, section.Body));
						}
						break;
					case Statement.Switch.ExpressionLabel expressionLabel:
						Resolve(expressionLabel.Expression);
						break;
				}
			}
			
			Resolve(section.Body);
			EndScope();
		}
		
		EndScope();
	}

	public void VisitExpressionStatement(Statement.ExpressionStatement statement)
	{
		Resolve(statement.Expression);
	}

	public void VisitOperatorOverloadStatement(Statement.OperatorOverload statement)
	{
		BeginScope();
		foreach (var parameter in statement.ParameterList)
		{
			Declare(new Declaration(parameter.Name, statement));
		}
		
		Resolve(statement.Body);
		EndScope();
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
		
		Declare(new Declaration(expression.IdentifierToken, expression));
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
	{I apologize for the confusion. Here is the updated ResolveType method that works with the provided TypeSyntax tree hierarchy:

csharp

public ResolvedType ResolveType(TypeSyntax typeSyntax)
{
    switch (typeSyntax)
    {
        case BaseTypeSyntax baseTypeSyntax:
            var typeSymbol = FindTypeSymbol(baseTypeSyntax.ToString());
            if (typeSymbol == null)
                throw new InvalidOperationException($"Type '{baseTypeSyntax}' not found.");
            return new SimpleResolvedType(typeSymbol);
        case NamedTypeSyntax namedTypeSyntax:
            var namedTypeSymbol = FindTypeSymbol(namedTypeSyntax.Name);
            if (namedTypeSymbol == null)
                throw new InvalidOperationException($"Type '{namedTypeSyntax.Name}' not found.");
            return new SimpleResolvedType(namedTypeSymbol);
        case NullableTypeSyntax nullableTypeSyntax:
            var underlyingType = ResolveType(nullableTypeSyntax.BaseType);
            return new NullableResolvedType(underlyingType);
        case ArrayTypeSyntax arrayTypeSyntax:
            var elementType = ResolveType(arrayTypeSyntax.BaseType);
            return new ArrayResolvedType(elementType);
        case TemplatedTypeSyntax templatedTypeSyntax:
            var baseType = ResolveType(templatedTypeSyntax.BaseType) as SimpleResolvedType;
            if (baseType == null)
                throw new InvalidOperationException($"Base type '{templatedTypeSyntax.BaseType}' not found or not supported in template.");

            var typeArguments = templatedTypeSyntax.TypeArguments.Select(ResolveType).ToList();
            return new TemplatedResolvedType(baseType, typeArguments);
        // Handle other TypeSyntax cases here (e.g., ExpressionTypeSyntax, LambdaTypeSyntax)
        default:
            throw new NotSupported
		BeginScope();

		BeginScope();

		foreach (var parameter in expression.ParameterList)
		{
			Declare(new Declaration(parameter.Name, expression));
		}
		
		Resolve(expression.Body);
		
		EndScope();
	}
}*/