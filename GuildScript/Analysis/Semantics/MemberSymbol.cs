namespace GuildScript.Analysis.Semantics;

public abstract class MemberSymbol : Symbol
{
	public Declaration Declaration { get; }
	
	protected MemberSymbol(string name, Declaration declaration) : base(name)
	{
		Declaration = declaration;
	}
}