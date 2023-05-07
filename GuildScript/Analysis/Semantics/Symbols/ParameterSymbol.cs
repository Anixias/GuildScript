namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ParameterSymbol : LocalSymbol
{
	public ResolvedType? Type { get; set; } = null;
	
	public ParameterSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}