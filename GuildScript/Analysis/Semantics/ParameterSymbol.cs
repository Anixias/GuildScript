namespace GuildScript.Analysis.Semantics;

public sealed class ParameterSymbol : Symbol
{
	public Declaration Declaration { get; }
	public bool Resolved { get; private set; }
	public ResolvedType? Type { get; private set; } = null;
	
	public ParameterSymbol(string name, Declaration declaration) : base(name)
	{
		Declaration = declaration;
	}
}