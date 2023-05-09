using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ParameterSymbol : LocalSymbol
{
	public ResolvedType? Type { get; set; }
	public bool IsReference { get; }

	public ParameterSymbol(string name, Declaration declaration, bool isReference) : base(name, declaration)
	{
		IsReference = isReference;
	}
}