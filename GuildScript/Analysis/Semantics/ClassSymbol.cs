namespace GuildScript.Analysis.Semantics;

public sealed class ClassSymbol : TypeSymbol
{
	private readonly ClassSymbol? baseClass;
	private readonly List<InterfaceSymbol> interfaces = new();
	
	public ClassSymbol(string name, Declaration declaration) : this(name, null, declaration)
	{
	}
	
	public ClassSymbol(string name, ClassSymbol? baseClass, Declaration declaration) : base(name, declaration)
	{
		this.baseClass = baseClass;
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

		if (baseClass is not null)
			return baseClass.FindMember(name);
		
		foreach (var @interface in interfaces)
		{
			if (@interface.FindMember(name) is { } interfaceMember)
				return interfaceMember;
		}

		return null;
	}
}