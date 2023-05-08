namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class InterfaceSymbol : TypeSymbol
{
	private readonly List<InterfaceSymbol> interfaces = new();

	public InterfaceSymbol(string name, Declaration declaration, AccessModifier accessModifier) : base(name,
		declaration, accessModifier)
	{
	}
	
	public void AddInterface(InterfaceSymbol symbol)
	{
		if (!interfaces.Contains(symbol))
			interfaces.Add(symbol);
	}
	
	public override Symbol? FindMember(string name)
	{
		if (members.TryGetValue(name, out var member))
			return member;
		
		foreach (var @interface in interfaces)
		{
			if (@interface.FindMember(name) is { } interfaceMember)
				return interfaceMember;
		}

		return null;
	}
}