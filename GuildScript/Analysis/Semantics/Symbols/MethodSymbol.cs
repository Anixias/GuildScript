using System.Collections.Immutable;
using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class MethodSymbol : MemberSymbol, ITypedSymbol, ITemplateable
{
	public ResolvedType Type => SimpleResolvedType.Method;
	
	public ImmutableArray<MethodModifier> Modifiers { get; }
	public ResolvedType? ReturnType { get; set; }
	public bool IsOperator => Operator is not null;
	public Operator? Operator { get; init; }

	private readonly List<ParameterSymbol> parameterList = new();
	private readonly List<TemplateParameterSymbol> templateParameterList = new();
	private readonly Dictionary<string, ParameterSymbol> parameters = new();
	private readonly Dictionary<string, TemplateParameterSymbol> templateParameters = new();
	private readonly List<MethodSymbol> overloads = new();

	public MethodSymbol(string name, Declaration declaration, AccessModifier accessModifier,
						IEnumerable<MethodModifier> modifiers) : base(name, declaration, accessModifier)
	{
		Modifiers = modifiers.ToImmutableArray();
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration, bool isReference)
	{
		var parameter = new ParameterSymbol(name, declaration, isReference);
		parameters.Add(name, parameter);
		parameterList.Add(parameter);
		return parameter;
	}

	public TemplateParameterSymbol AddTemplateParameter(string name, Declaration declaration)
	{
		var parameter = new TemplateParameterSymbol(name, declaration);
		templateParameters.Add(name, parameter);
		templateParameterList.Add(parameter);
		return parameter;
	}

	public void AddOverload(MethodSymbol overload)
	{
		overloads.Add(overload);
	}

	public void ResolveParameter(string name, ResolvedType type)
	{
		if (!parameters.ContainsKey(name))
			throw new Exception($"Parameter '{name}' does not exist.");

		var parameter = parameters[name];
		parameter.Type = type;
		parameter.Resolved = true;
	}

	public void ResolveTemplateParameter(string name, ResolvedType type)
	{
		if (!templateParameters.ContainsKey(name))
			throw new Exception($"Template parameter '{name}' does not exist.");

		var parameter = templateParameters[name];
		parameter.UnderlyingType = type;
		parameter.Resolved = true;
	}

	public IEnumerable<ParameterSymbol> GetParameters()
	{
		return parameterList;
	}

	public IEnumerable<TemplateParameterSymbol> GetTemplateParameters()
	{
		return templateParameterList;
	}

	public IEnumerable<MethodSymbol> GetOverloads()
	{
		return overloads;
	}
}
