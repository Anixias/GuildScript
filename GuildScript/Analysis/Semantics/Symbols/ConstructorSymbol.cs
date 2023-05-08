namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ConstructorSymbol : MemberSymbol
{
	private readonly List<ParameterSymbol> parameters = new();
	
	public ConstructorSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration, bool isReference)
	{
		var parameter = new ParameterSymbol(name, declaration, isReference);
		parameters.Add(parameter);
		return parameter;
	}
}