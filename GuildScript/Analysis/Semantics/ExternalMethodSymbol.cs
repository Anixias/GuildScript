namespace GuildScript.Analysis.Semantics;

public sealed class ExternalMethodSymbol : MemberSymbol
{
	public ResolvedType? ReturnType { get; private set; } = null;
	
	private readonly List<ParameterSymbol> parameters = new();
	private readonly List<ExternalMethodSymbol> overloads = new();
	
	public ExternalMethodSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration)
	{
		var parameter = new ParameterSymbol(name, declaration);
		parameters.Add(parameter);
		return parameter;
	}

	public void AddOverload(ExternalMethodSymbol overload)
	{
		overloads.Add(overload);
	}
}