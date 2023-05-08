namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class EnumSymbol : TypeSymbol
{
	public EnumSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}

	public EnumMemberSymbol? AddMember(string name)
	{
		if (Declaration is null)
			return null;
		
		var member = new EnumMemberSymbol(name, Declaration);
		if (!AddChild(member))
			return null;
		
		members.Add(member.Name, member);
		return member;
	}
}