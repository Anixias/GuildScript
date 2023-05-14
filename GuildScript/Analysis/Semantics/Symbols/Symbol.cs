using System.Collections.Immutable;

namespace GuildScript.Analysis.Semantics.Symbols;

public abstract class Symbol
{
	public string Name { get; }
	public ImmutableArray<Symbol> Children => children.Values.ToImmutableArray();

	protected readonly Dictionary<string, Symbol> children = new();

	protected Symbol(string name)
	{
		Name = name;
	}

	protected bool AddChild(Symbol child)
	{
		if (children.ContainsKey(child.Name))
			return false;
		
		children.Add(child.Name, child);
		return true;
	}

	public virtual Symbol? GetChild(string name)
	{
		if (this is ITypedSymbol typedSymbol)
		{
			return children.TryGetValue(name, out var child) ? child : typedSymbol.Type?.TypeSymbol.GetChild(name); 
		}
		else
		{
			return children.TryGetValue(name, out var child) ? child : null;
		}
	}
}