namespace GuildScript.Analysis.Semantics;

public sealed class EnumMemberSymbol : MemberSymbol
{
	public EnumMemberSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}