namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class EnumSymbol : TypeSymbol
{
	public EnumSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}

	public bool AddMember(string name)
	{
		var member = new EnumMemberSymbol(name, Declaration);
		if (!AddChild(member))
			return false;
		
		members.Add(member.Name, member);
		return true;
	}
}