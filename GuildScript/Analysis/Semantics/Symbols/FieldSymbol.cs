namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class FieldSymbol : MemberSymbol
{
	public ResolvedType? Type { get; set; } = null;
	
	public FieldSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}