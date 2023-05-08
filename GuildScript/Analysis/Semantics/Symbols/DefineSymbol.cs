namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class DefineSymbol : TypeSymbol
{
	public bool Resolved { get; set; }
	public ResolvedType? AliasedType { get; set; } = null;
	
	public DefineSymbol(string name) : base(name)
	{
	}

	public DefineSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}