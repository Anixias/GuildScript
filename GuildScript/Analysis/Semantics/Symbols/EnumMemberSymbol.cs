namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class EnumMemberSymbol : MemberSymbol, ITypedSymbol
{
	public ResolvedType? Type { get; set; }
	public EnumSymbol Enum { get; }
	
	public EnumMemberSymbol(string name, Declaration declaration, EnumSymbol @enum) : base(name, declaration, AccessModifier.Public)
	{
		Enum = @enum;
	}
}