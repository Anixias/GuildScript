namespace GuildScript.Analysis.Semantics;

public sealed class PropertySymbol : MemberSymbol
{
	public ResolvedType? Type { get; } = null;
	
	public PropertySymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}