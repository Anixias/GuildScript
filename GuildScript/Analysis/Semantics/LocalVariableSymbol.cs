namespace GuildScript.Analysis.Semantics;

public sealed class LocalVariableSymbol : LocalSymbol
{
	public ResolvedType? Type { get; private set; } = null;
	
	public LocalVariableSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}