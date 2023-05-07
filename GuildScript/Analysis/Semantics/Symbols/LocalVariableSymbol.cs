using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class LocalVariableSymbol : LocalSymbol
{
	public TypeSyntax? TypeSyntax { get; }
	public ResolvedType? Type { get; private set; } = null;
	
	public LocalVariableSymbol(string name, Declaration declaration, TypeSyntax? typeSyntax) : base(name, declaration)
	{
		TypeSyntax = typeSyntax;
	}
}