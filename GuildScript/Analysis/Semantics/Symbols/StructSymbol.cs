namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class StructSymbol : TypeSymbol
{
	public StructModifier StructModifier { get; }
	
	public StructSymbol(string name, Declaration declaration, StructModifier structModifier,
						AccessModifier accessModifier) : base(name, declaration, accessModifier)
	{
		StructModifier = structModifier;
	}
}