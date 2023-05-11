using System.Collections;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ConstructorSymbol : MemberSymbol, ITypedSymbol
{
	public ResolvedType Type => SimpleResolvedType.Method;
	private readonly Dictionary<string, ParameterSymbol> parameters = new();
	private readonly List<ConstructorSymbol> overloads = new();

	public ConstructorSymbol(string name, Declaration declaration, AccessModifier accessModifier) : base(name,
		declaration, accessModifier)
	{
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration, bool isReference)
	{
		var parameter = new ParameterSymbol(name, declaration, isReference);
		parameters.Add(name, parameter);
		return parameter;
	}

	public void ResolveParameter(string name, ResolvedType type)
	{
		if (!parameters.ContainsKey(name))
			throw new Exception($"Parameter '{name}' does not exist.");

		var parameter = parameters[name];
		parameter.Type = type;
		parameter.Resolved = true;
	}

	public void AddOverload(ConstructorSymbol overload)
	{
		overloads.Add(overload);
	}

	public IEnumerable<ParameterSymbol> GetParameters()
	{
		return parameters.Values;
	}

	public IEnumerable<ConstructorSymbol> GetOverloads()
	{
		return overloads;
	}
}