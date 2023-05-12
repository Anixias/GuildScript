using GuildScript.Analysis.Semantics.Symbols;
using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public sealed class SemanticModel
{
	private struct LambdaTypeData
	{
		public LambdaTypeSyntax lambdaTypeSyntax;
		public Symbol? symbol;
		public Scope? scope;

		public LambdaTypeData(LambdaTypeSyntax lambdaTypeSyntax, Symbol? symbol, Scope? scope)
		{
			this.lambdaTypeSyntax = lambdaTypeSyntax;
			this.symbol = symbol;
			this.scope = scope;
		}
	}

	public IEnumerable<TypeSymbol> TypeSymbols => typeSymbols;

	private readonly List<Symbol> symbols = new();
	private List<MethodSymbol> EntryPoints { get; } = new();
	public Symbol? CurrentSymbol => symbolStack.TryPeek(out var symbol) ? symbol : null;
	private Scope? CurrentScope => scopeStack.TryPeek(out var scope) ? scope : null;
	private readonly Stack<Symbol> symbolStack = new();
	private readonly Stack<Scope> scopeStack = new();
	private readonly Dictionary<string, ModuleSymbol> globalModules = new();
	private readonly Dictionary<SyntaxNode, Scope> scopes = new();
	private readonly List<TypeSymbol> typeSymbols = new();
	private readonly List<LambdaTypeData> lambdaTypeQueue = new();
	private readonly Dictionary<SyntaxNode, LambdaSymbol> lambdaSymbols = new();
	private readonly Dictionary<SyntaxNode, MethodSymbol> methodSymbols = new();

	public SemanticModel()
	{
		scopeStack.Push(new Scope(null));
	}

	public Scope EnterScope(SyntaxNode node)
	{
		if (scopes.TryGetValue(node, out var existingScope))
		{
			scopeStack.Push(existingScope);
			return existingScope;
		}

		var scope = new Scope(CurrentScope);
		scopes.Add(node, scope);
		scopeStack.Push(scope);
		return scope;
	}

	public void ExitScope()
	{
		scopeStack.Pop();
	}

	public void ResolveLambdas()
	{
		while (lambdaTypeQueue.Count > 0)
		{
			var data = lambdaTypeQueue[0];
			lambdaTypeQueue.RemoveAt(0);

			// @TODO
		}
	}

	public ModuleSymbol AddModule(string name)
	{
		if (GetAncestorModule() is { } moduleSymbol)
			return moduleSymbol.ContainsModule(name)
				? moduleSymbol.GetModule(name)
				: moduleSymbol.CreateModule(name);

		if (globalModules.TryGetValue(name, out var existingModule))
			return existingModule;

		var globalModuleSymbol = new ModuleSymbol(name);
		CurrentScope?.AddSymbol(globalModuleSymbol);
		globalModules.Add(name, globalModuleSymbol);
		symbols.Add(globalModuleSymbol);
		return globalModuleSymbol;
	}

	public ModuleSymbol? GetModule(string name)
	{
		if (GetAncestorModule() is { } moduleSymbol)
			return moduleSymbol.ContainsModule(name)
				? moduleSymbol.GetModule(name)
				: moduleSymbol.CreateModule(name);

		return globalModules.TryGetValue(name, out var existingModule) ? existingModule : null;
	}

	public ClassSymbol AddClass(string name, Declaration declaration, ClassModifier classModifier,
								AccessModifier accessModifier)
	{
		var symbol = new ClassSymbol(name, declaration, classModifier, accessModifier);
		symbols.Add(symbol);
		typeSymbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case ModuleSymbol module:
				module.AddType(symbol);
				return symbol;
			case TypeSymbol type:
				type.NestType(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare types in global module.");
		}
	}

	public StructSymbol AddStruct(string name, Declaration declaration, StructModifier structModifier,
								  AccessModifier accessModifier)
	{
		var symbol = new StructSymbol(name, declaration, structModifier, accessModifier);
		symbols.Add(symbol);
		typeSymbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case ModuleSymbol module:
				module.AddType(symbol);
				return symbol;
			case TypeSymbol type:
				type.NestType(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare types in global module.");
		}
	}

	public EnumSymbol AddEnum(string name, Declaration declaration, AccessModifier accessModifier,
							  TypeSyntax baseType)
	{
		var symbol = new EnumSymbol(name, declaration, accessModifier, baseType);
		symbols.Add(symbol);
		typeSymbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case ModuleSymbol module:
				module.AddType(symbol);
				return symbol;
			case TypeSymbol type:
				type.NestType(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare types in global module.");
		}
	}

	public InterfaceSymbol AddInterface(string name, Declaration declaration, AccessModifier accessModifier)
	{
		var symbol = new InterfaceSymbol(name, declaration, accessModifier);
		symbols.Add(symbol);
		typeSymbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case ModuleSymbol module:
				module.AddType(symbol);
				return symbol;
			case TypeSymbol type:
				type.NestType(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare types in global module.");
		}
	}

	public FieldSymbol AddField(string name, Declaration declaration, AccessModifier accessModifier,
								IEnumerable<FieldModifier> fieldModifiers)
	{
		var symbol = new FieldSymbol(name, declaration, accessModifier, fieldModifiers);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare fields outside of types.");
		}
	}

	public LocalVariableSymbol AddLocalVariable(string name, Declaration declaration, TypeSyntax? typeSyntax)
	{
		var symbol = new LocalVariableSymbol(name, declaration, typeSyntax);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		return symbol;
	}

	public PropertySymbol AddProperty(string name, Declaration declaration, AccessModifier accessModifier,
									  IEnumerable<MethodModifier> methodModifiers)
	{
		var symbol = new PropertySymbol(name, declaration, accessModifier, methodModifiers);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare properties outside of types.");
		}
	}

	public MethodSymbol AddMethod(string name, Declaration declaration, AccessModifier accessModifier,
								  IEnumerable<MethodModifier> methodModifiers)
	{
		var symbol = new MethodSymbol(name, declaration, accessModifier, methodModifiers);
		methodSymbols.Add(declaration.SourceNode!, symbol);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare methods outside of types.");
		}
	}

	public MethodSymbol AddOperatorOverload(string name, Declaration declaration, AccessModifier accessModifier,
								  IEnumerable<MethodModifier> methodModifiers, Operator @operator)
	{
		var symbol = new MethodSymbol(name, declaration, accessModifier, methodModifiers)
		{
			Operator = @operator
		};

		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare operator overloads outside of types.");
		}
	}

	public EventSymbol AddEvent(string name, Declaration declaration, AccessModifier accessModifier,
								EventModifier eventModifier)
	{
		var symbol = new EventSymbol(name, declaration, accessModifier, eventModifier);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare events outside of types.");
		}
	}

	public ExternalMethodSymbol AddExternalMethod(string name, Declaration declaration)
	{
		var symbol = new ExternalMethodSymbol(name, declaration);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare external methods outside of types.");
		}
	}

	public ConstructorSymbol AddConstructor(string name, Declaration declaration, AccessModifier accessModifier)
	{
		var symbol = new ConstructorSymbol(name, declaration, accessModifier);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare constructors outside of types.");
		}
	}

	public DestructorSymbol AddDestructor(string name, Declaration declaration)
	{
		var symbol = new DestructorSymbol(name, declaration);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare destructors outside of types.");
		}
	}

	public MethodSymbol AddEntryPoint(Statement.EntryPoint statement)
	{
		var symbol = new MethodSymbol(statement.Identifier.Text, new Declaration(statement.Identifier, statement),
			AccessModifier.Public, Array.Empty<MethodModifier>());
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		EntryPoints.Add(symbol);
		return symbol;
	}

	public DefineSymbol AddDefine(Statement.Define statement)
	{
		var symbol = new DefineSymbol(statement.Identifier.Text, new Declaration(statement.Identifier, statement));
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case ModuleSymbol module:
				module.AddType(symbol);
				return symbol;
			case TypeSymbol type:
				type.NestType(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare define statements in global scope.");
		}
	}

	public void AddLambdaType(LambdaTypeSyntax type)
	{
		lambdaTypeQueue.Add(new LambdaTypeData(type, CurrentSymbol, CurrentScope));
	}

	public void VerifyEntryPoint()
	{
		switch (EntryPoints.Count)
		{
			case 0:
				throw new Exception("Entry point has not been defined.");
			case > 1:
				throw new Exception("Entry point may only be defined once.");
		}
	}

	public void VisitSymbol(Symbol symbol)
	{
		symbolStack.Push(symbol);
	}

	public void Return()
	{
		symbolStack.Pop();
	}

	private ModuleSymbol? GetAncestorModule()
	{
		foreach (var symbol in symbolStack)
		{
			if (symbol is ModuleSymbol moduleSymbol)
				return moduleSymbol;
		}

		return null;
	}

	public Symbol? FindSymbol(string name)
	{
		if (CurrentScope?.FindSymbol(name) is { } symbol)
			return symbol;

		return CurrentSymbol?.GetChild(name);
	}

	public MethodSymbol GetEntryPoint()
	{
		return EntryPoints[0];
	}

	public IEnumerable<Symbol> GetAllSymbols()
	{
		return symbols;
	}

	public void AddSymbol(Symbol symbol)
	{
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
	}

	public LambdaSymbol AddLambda(Declaration declaration)
	{
		var symbol = new LambdaSymbol("Lambda:" + Guid.NewGuid(), declaration);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		lambdaSymbols.Add(declaration.SourceNode!, symbol);
		return symbol;
	}

	public LambdaSymbol? GetLambda(SyntaxNode node)
	{
		return lambdaSymbols.TryGetValue(node, out var lambda) ? lambda : null;
	}

	public MethodSymbol? GetMethod(SyntaxNode node)
	{
		return methodSymbols.TryGetValue(node, out var method) ? method : null;
	}

	public IndexerSymbol AddIndexer(string name, Declaration declaration, AccessModifier accessModifier)
	{
		var symbol = new IndexerSymbol(name, declaration, accessModifier);
		symbols.Add(symbol);
		CurrentScope?.AddSymbol(symbol);
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare indexers outside of types.");
		}
	}
}
