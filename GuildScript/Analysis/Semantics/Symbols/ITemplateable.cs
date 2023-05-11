namespace GuildScript.Analysis.Semantics.Symbols;

public interface ITemplateable
{
	TemplateParameterSymbol AddTemplateParameter(string name, Declaration declaration);
	void ResolveTemplateParameter(string name, ResolvedType type);
	IEnumerable<TemplateParameterSymbol> GetTemplateParameters();
}