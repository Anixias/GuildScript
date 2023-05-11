namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class IndexerSymbol : MemberSymbol
{
	public ResolvedType? Type { get; set; }
	public bool Resolved { get; set; }
	private readonly Dictionary<string, ParameterSymbol> parameters = new();
	private readonly List<IndexerSymbol> overloads = new();
	
	public IndexerSymbol(string name, Declaration declaration, AccessModifier accessModifier)
		: base(name, declaration, accessModifier)
	{
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration, bool isReference)
	{
		var parameter = new ParameterSymbol(name, declaration, isReference);
		parameters.Add(name, parameter);
		return parameter;
	}

	public void AddOverload(IndexerSymbol overload)
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

	public IEnumerable<ParameterSymbol> GetParameters()
	{
		return parameters.Values;
	}

	public IEnumerable<IndexerSymbol> GetOverloads()
	{
		return overloads;
	}
}