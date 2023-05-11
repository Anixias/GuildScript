namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class DestructorSymbol : MemberSymbol, ITypedSymbol
{
	public ResolvedType Type => SimpleResolvedType.Method;

	public DestructorSymbol(string name, Declaration declaration) : base(name, declaration, AccessModifier.Public)
	{
	}
}