namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class LambdaSymbol : LocalSymbol
{
	private readonly List<ParameterSymbol> parameterList = new();
	private readonly Dictionary<string, ParameterSymbol> parameters = new();
	
	public LambdaSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
	
	public ParameterSymbol AddParameter(string name, Declaration declaration, bool isReference)
	{
		var parameter = new ParameterSymbol(name, declaration, isReference);
		parameters.Add(name, parameter);
		parameterList.Add(parameter);
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

	public IEnumerable<ParameterSymbol> GetParameters()
	{
		return parameterList;
	}
}