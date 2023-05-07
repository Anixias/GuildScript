namespace GuildScript.Analysis.Semantics.Symbols;

public abstract class MemberSymbol : Symbol
{
	public bool Resolved { get; set; }
	public Declaration Declaration { get; }
	
	protected MemberSymbol(string name, Declaration declaration) : base(name)
	{
		Declaration = declaration;
	}
}