namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class EnumMemberSymbol : MemberSymbol
{
	public EnumMemberSymbol(string name, Declaration declaration) : base(name, declaration, AccessModifier.Public)
	{
	}
}