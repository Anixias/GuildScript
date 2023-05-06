namespace GuildScript.Analysis.Semantics;

public abstract class LocalSymbol : Symbol
{
	public bool Resolved { get; private set; }
	public Declaration Declaration { get; }
	
	protected LocalSymbol(string name, Declaration declaration) : base(name)
	{
		Declaration = declaration;
	}
}