namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class TemplateParameterSymbol : TypeSymbol
{
	public TemplateParameterSymbol(string name) : base(name, AccessModifier.Private)
	{
	}

	public TemplateParameterSymbol(string name, Declaration declaration) : base(name, declaration,
		AccessModifier.Private)

	{
	}
}