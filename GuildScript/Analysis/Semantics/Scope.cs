using GuildScript.Analysis.Semantics.Symbols;

namespace GuildScript.Analysis.Semantics;

public sealed class Scope
{
	public Scope? Parent { get; }
	public List<Symbol> Symbols { get; } = new();
	public List<ModuleSymbol> Imports { get; } = new();
	
	public Scope(Scope? parent)
	{
		Parent = parent;
	}

	public void AddSymbol(Symbol symbol)
	{
		Symbols.Add(symbol);
	}

	public Symbol? FindSymbol(string name)
	{
		foreach (var symbol in Symbols)
		{
			if (symbol.Name == name)
				return symbol;
		}

		if (Parent?.FindSymbol(name) is { } parentSymbol)
			return parentSymbol;

		foreach (var import in Imports)
		{
			foreach (var importedChild in import.Children)
			{
				if (importedChild.Name == name)
					return importedChild;
			}
		}

		return null;
	}

	public void ImportModule(ModuleSymbol importedModuleSymbol)
	{
		Imports.Add(importedModuleSymbol);
	}
}