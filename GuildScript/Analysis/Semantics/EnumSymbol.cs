namespace GuildScript.Analysis.Semantics;

public sealed class EnumSymbol : TypeSymbol
{
	public EnumSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}