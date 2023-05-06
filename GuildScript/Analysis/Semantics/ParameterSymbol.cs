namespace GuildScript.Analysis.Semantics;

public sealed class ParameterSymbol : LocalSymbol
{
	public ResolvedType? Type { get; private set; } = null;
	
	public ParameterSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}