namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class TemplateParameterSymbol : TypeSymbol
{
	public ResolvedType? UnderlyingType { get; set; }
	public bool Resolved { get; set; }
	
	public TemplateParameterSymbol(string name) : base(name, AccessModifier.Private)
	{
	}

	public TemplateParameterSymbol(string name, Declaration declaration) : base(name, declaration,
		AccessModifier.Private)

	{
	}
}