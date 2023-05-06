namespace GuildScript.Analysis.Semantics;

public sealed class FieldSymbol : MemberSymbol
{
	public ResolvedType? Type { get; private set; } = null;
	
	public FieldSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}