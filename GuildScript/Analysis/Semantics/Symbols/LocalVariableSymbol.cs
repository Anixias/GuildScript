using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class LocalVariableSymbol : LocalSymbol, ITypedSymbol
{
	public TypeSyntax? TypeSyntax { get; }
	public ResolvedType? Type { get; set; }

	public LocalVariableSymbol(string name, Declaration declaration, TypeSyntax? typeSyntax) : base(name, declaration)
	{
		TypeSyntax = typeSyntax;
	}
}