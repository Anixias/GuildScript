namespace GuildScript.Analysis.Semantics;

public sealed class InterfaceSymbol : TypeSymbol
{
	public InterfaceSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}