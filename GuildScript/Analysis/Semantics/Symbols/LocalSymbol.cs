namespace GuildScript.Analysis.Semantics.Symbols;

public abstract class LocalSymbol : Symbol
{
	public bool Resolved { get; set; }
	public Declaration Declaration { get; }
	
	protected LocalSymbol(string name, Declaration declaration) : base(name)
	{
		Declaration = declaration;
	}
}