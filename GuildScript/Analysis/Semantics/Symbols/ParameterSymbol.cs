namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ParameterSymbol : LocalSymbol, ITypedSymbol
{
	public ResolvedType? Type { get; set; }
	public bool IsReference { get; }

	public ParameterSymbol(string name, Declaration declaration, bool isReference) : base(name, declaration)
	{
		IsReference = isReference;
	}
}