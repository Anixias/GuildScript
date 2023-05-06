namespace GuildScript.Analysis.Semantics;

public sealed class MethodSymbol : MemberSymbol
{
	public ResolvedType? ReturnType { get; private set; } = null;
	
	private readonly List<ParameterSymbol> parameters = new();
	
	public MethodSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration)
	{
		var parameter = new ParameterSymbol(name, declaration);
		parameters.Add(parameter);
		return parameter;
	}
}