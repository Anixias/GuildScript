using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public sealed class SemanticModel
{
	private List<MethodSymbol> EntryPoints { get; } = new();
	private Symbol? CurrentSymbol => symbolStack.TryPeek(out var symbol) ? symbol : null;
	private Scope? CurrentScope => scopeStack.TryPeek(out var scope) ? scope : null;
	private readonly Stack<Symbol> symbolStack = new();
	private readonly Stack<Scope> scopeStack = new();
	private readonly Dictionary<string, ModuleSymbol> globalModules = new();
	private readonly Dictionary<SyntaxNode, Scope> scopes = new();

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

	public ClassSymbol AddClass(string name, Declaration declaration)
	{
		var symbol = new ClassSymbol(name, declaration);
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

	public StructSymbol AddStruct(string name, Declaration declaration)
	{
		var symbol = new StructSymbol(name, declaration);
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

	public EnumSymbol AddEnum(string name, Declaration declaration)
	{
		var symbol = new EnumSymbol(name, declaration);
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

	public InterfaceSymbol AddInterface(string name, Declaration declaration)
	{
		var symbol = new InterfaceSymbol(name, declaration);
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

	public FieldSymbol AddField(string name, Declaration declaration)
	{
		var symbol = new FieldSymbol(name, declaration);
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

	public LocalVariableSymbol AddLocalVariable(string name, Declaration declaration)
	{
		var symbol = new LocalVariableSymbol(name, declaration);
		CurrentScope?.AddSymbol(symbol);
		return symbol;
	}

	public PropertySymbol AddProperty(string name, Declaration declaration)
	{
		var symbol = new PropertySymbol(name, declaration);
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

	public MethodSymbol AddMethod(string name, Declaration declaration)
	{
		var symbol = new MethodSymbol(name, declaration);
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

	public EventSymbol AddEvent(string name, Declaration declaration)
	{
		var symbol = new EventSymbol(name, declaration);
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

	public ConstructorSymbol AddConstructor(string name, Declaration declaration)
	{
		var symbol = new ConstructorSymbol(name, declaration);
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
		var symbol = new MethodSymbol(statement.Identifier.Text, new Declaration(statement.Identifier, statement));
		CurrentScope?.AddSymbol(symbol);
		EntryPoints.Add(symbol);
		return symbol;
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
}