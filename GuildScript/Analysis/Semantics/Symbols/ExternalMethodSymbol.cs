namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ExternalMethodSymbol : MemberSymbol
{
	public ResolvedType? ReturnType { get; set; } = null;
	
	private readonly Dictionary<string, ParameterSymbol> parameters = new();
	private readonly List<ExternalMethodSymbol> overloads = new();

	public ExternalMethodSymbol(string name, Declaration declaration) : base(name, declaration, AccessModifier.Private)
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

	public void AddOverload(ExternalMethodSymbol overload)
	{
		overloads.Add(overload);
	}

	public IEnumerable<ParameterSymbol> GetParameters()
	{
		return parameters.Values;
	}
}