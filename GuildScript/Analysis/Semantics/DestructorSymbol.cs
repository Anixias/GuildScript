namespace GuildScript.Analysis.Semantics;

public sealed class DestructorSymbol : MemberSymbol
{
	public DestructorSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}