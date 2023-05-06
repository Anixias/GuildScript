namespace GuildScript.Analysis.Semantics;

public sealed class ClassSymbol : TypeSymbol
{
	public ClassSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}