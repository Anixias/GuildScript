using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public sealed class SemanticModel
{
	public List<Statement.EntryPoint> EntryPoints { get; } = new();
	private Symbol? CurrentSymbol => symbolStack.TryPeek(out var symbol) ? symbol : null;
	private readonly Stack<Symbol> symbolStack = new();
	private readonly Dictionary<string, ModuleSymbol> globalModules = new();

	public ModuleSymbol AddModule(string name)
	{
		if (GetAncestorModule() is { } moduleSymbol)
			return moduleSymbol.ContainsModule(name)
				? moduleSymbol.GetModule(name)
				: moduleSymbol.CreateModule(name);
		
		if (globalModules.TryGetValue(name, out var existingModule))
			return existingModule;

		var globalModuleSymbol = new ModuleSymbol(name);
		globalModules.Add(name, globalModuleSymbol);
		return globalModuleSymbol;
	}

	public ClassSymbol AddClass(string name, Declaration declaration)
	{
		var symbol = new ClassSymbol(name, declaration);
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
		switch (CurrentSymbol)
		{
			case TypeSymbol type:
				type.AddMember(symbol);
				return symbol;
			default:
				throw new Exception("Cannot declare fields outside of types.");
		}
	}

	public void AddEntryPoint(Statement.EntryPoint statement)
	{
		EntryPoints.Add(statement);
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