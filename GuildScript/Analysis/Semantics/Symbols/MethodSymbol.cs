using System.Collections.Immutable;
using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class MethodSymbol : MemberSymbol, ITypedSymbol, ITemplateable, ICallable
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

	public ICallable? FindOverload(List<ResolvedType?> argumentTypes)
	{
		return FindOverload(argumentTypes, 0);
	}

	public ICallable? FindOverload(List<ResolvedType?> argumentTypes, int templateCount)
	{
		var validOverloads = new List<MethodSymbol>();
		var localOverloads = new List<MethodSymbol> { this };
		localOverloads.AddRange(overloads);

		foreach (var overload in localOverloads)
		{
			var overloadParameters = overload.GetParameters().ToArray();
			if (overloadParameters.Length != argumentTypes.Count)
				continue;
		
			var typeParameters = overload.GetTemplateParameters().ToArray();
			if (typeParameters.Length != templateCount)
				continue;

			var matches = true;
			for (var i = 0; i < overloadParameters.Length; i++)
			{
				var parameter = overloadParameters[i];
				var argumentType = argumentTypes[i];
				if (parameter.Type?.GetType() == argumentType?.GetType() &&
					parameter.Type?.TypeSymbol == argumentType?.TypeSymbol) 
					continue;
			
				matches = false;
				break;
			}

			if (!matches)
				continue;

			validOverloads.Add(overload);
		}

		return validOverloads.Count switch
		{
			1   => validOverloads[0],
			> 1 => throw new Exception("Ambiguous method reference."),
			_   => null
		};
	}
}
