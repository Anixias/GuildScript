using System.Collections.Immutable;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ClassSymbol : TypeSymbol, ITemplateable
{
	public ClassModifier ClassModifier { get; }
	public ClassSymbol? BaseClass { get; }
	private readonly List<InterfaceSymbol> interfaces = new();
	private readonly List<TemplateParameterSymbol> templateParameterList = new();
	private readonly Dictionary<string, TemplateParameterSymbol> templateParameters = new();

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

	public TemplateParameterSymbol AddTemplateParameter(string name, Declaration declaration)
	{
		var parameter = new TemplateParameterSymbol(name, declaration);
		templateParameters.Add(name, parameter);
		templateParameterList.Add(parameter);
		return parameter;
	}

	public void ResolveTemplateParameter(string name, ResolvedType type)
	{
		if (!templateParameters.ContainsKey(name))
			throw new Exception($"Template parameter '{name}' does not exist.");

		var parameter = templateParameters[name];
		parameter.UnderlyingType = type;
		parameter.Resolved = true;
	}

	public IEnumerable<TemplateParameterSymbol> GetTemplateParameters()
	{
		return templateParameterList;
	}
	
	public override Symbol? GetChild(string name)
	{
		return children.TryGetValue(name, out var child) ? child : BaseClass?.GetChild(name);
	}
}