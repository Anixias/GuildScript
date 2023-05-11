using System.Collections.Immutable;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ClassSymbol : TypeSymbol
{
	public ClassModifier ClassModifier { get; }
	public ClassSymbol? BaseClass { get; }
	public ImmutableArray<TemplateParameterSymbol> TemplateParameters { get; set; }
	private readonly List<InterfaceSymbol> interfaces = new();

	public ClassSymbol(string name, Declaration declaration, ClassModifier classModifier, AccessModifier accessModifier)
		: this(name, null, declaration,
		classModifier, accessModifier)
	{
	}

	public ClassSymbol(string name, ClassSymbol? baseClass, Declaration declaration, ClassModifier classModifier,
					   AccessModifier accessModifier)
					   : base(name, declaration, accessModifier)
	{
		BaseClass = baseClass;
		ClassModifier = classModifier;
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