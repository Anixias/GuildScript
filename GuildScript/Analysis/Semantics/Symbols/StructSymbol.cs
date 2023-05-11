using System.Collections.Immutable;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class StructSymbol : TypeSymbol, ITemplateable
{
	public StructModifier StructModifier { get; }
	private readonly List<TemplateParameterSymbol> templateParameterList = new();
	private readonly Dictionary<string, TemplateParameterSymbol> templateParameters = new();

	public StructSymbol(string name, Declaration declaration, StructModifier structModifier,
						AccessModifier accessModifier) : base(name, declaration, accessModifier)

	{
		StructModifier = structModifier;
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