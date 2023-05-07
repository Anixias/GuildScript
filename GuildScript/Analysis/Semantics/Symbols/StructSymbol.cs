namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class StructSymbol : TypeSymbol
{
	public StructSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}