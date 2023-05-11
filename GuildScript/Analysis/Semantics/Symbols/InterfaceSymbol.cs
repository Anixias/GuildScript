using System.Collections.Immutable;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class InterfaceSymbol : TypeSymbol, ITemplateable
{
	private readonly List<InterfaceSymbol> interfaces = new();
	private readonly List<TemplateParameterSymbol> templateParameterList = new();
	private readonly Dictionary<string, TemplateParameterSymbol> templateParameters = new();

	public InterfaceSymbol(string name, Declaration declaration, AccessModifier accessModifier)
		: base(name, declaration, accessModifier)
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
}