namespace GuildScript.Analysis.Semantics;

public sealed class Scope
{
	public Scope? Parent { get; }
	public List<Symbol> Symbols { get; } = new();
	
	public Scope(Scope? parent)
	{
		Parent = parent;
	}

	public void AddSymbol(Symbol symbol)
	{
		Symbols.Add(symbol);
	}
}