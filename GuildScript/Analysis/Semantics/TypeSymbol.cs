namespace GuildScript.Analysis.Semantics;

public abstract class TypeSymbol : Symbol
{
	public Declaration Declaration { get; }
	protected readonly Dictionary<string, TypeSymbol> nestedTypes = new();
	protected readonly Dictionary<string, MemberSymbol> members = new();
	
	protected TypeSymbol(string name, Declaration declaration) : base(name)
	{
		Declaration = declaration;
	}

	public void NestType(TypeSymbol type)
	{
		if (!nestedTypes.TryAdd(type.Name, type))
			throw new Exception($"The type '{type.Name}' is already declared in '{Name}'.");
	}

	public void AddMember(MemberSymbol member)
	{
		if (!members.TryAdd(member.Name, member))
			throw new Exception($"The member '{member.Name}' is already declared in '{Name}'.");
	}
}