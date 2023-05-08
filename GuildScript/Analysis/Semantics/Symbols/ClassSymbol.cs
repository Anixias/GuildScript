namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ClassSymbol : TypeSymbol
{
	public ClassSymbol? BaseClass { get; }
	private readonly List<InterfaceSymbol> interfaces = new();
	
	public ClassSymbol(string name, Declaration declaration) : this(name, null, declaration)
	{
	}
	
	public ClassSymbol(string name, ClassSymbol? baseClass, Declaration declaration) : base(name, declaration)
	{
		BaseClass = baseClass;
	}
	
	public void AddInterface(InterfaceSymbol symbol)
	{
		if (!interfaces.Contains(symbol))
			interfaces.Add(symbol);
	}

	public bool ImplementsInterface(InterfaceSymbol symbol)
	{
		return interfaces.Contains(symbol);
	}

	public override Symbol? FindMember(string name)
	{
		if (members.TryGetValue(name, out var member))
			return member;

		if (BaseClass is not null)
			return BaseClass.FindMember(name);
		
		foreach (var @interface in interfaces)
		{
			if (@interface.FindMember(name) is { } interfaceMember)
				return interfaceMember;
		}

		return null;
	}
}