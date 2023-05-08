namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class MethodSymbol : MemberSymbol
{
	public ResolvedType? ReturnType { get; set; } = null;
	
	private readonly Dictionary<string, ParameterSymbol> parameters = new();
	private readonly List<MethodSymbol> overloads = new();
	
	public MethodSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration, bool isReference)
	{
		var parameter = new ParameterSymbol(name, declaration, isReference);
		parameters.Add(name, parameter);
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
}

