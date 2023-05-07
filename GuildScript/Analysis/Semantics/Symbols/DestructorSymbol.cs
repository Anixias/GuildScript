namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class DestructorSymbol : MemberSymbol
{
	public DestructorSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}