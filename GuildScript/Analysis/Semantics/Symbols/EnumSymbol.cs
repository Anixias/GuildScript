using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class EnumSymbol : TypeSymbol
{
	public bool Resolved { get; set; }
	public TypeSyntax BaseTypeSyntax { get; }
	public NativeTypeSymbol? BaseType { get; }
	
	public EnumSymbol(string name, Declaration declaration, AccessModifier accessModifier, TypeSyntax baseType)
		: base(name, declaration, accessModifier)
	{
		BaseTypeSyntax = baseType;
	}

	public EnumMemberSymbol? AddMember(string name)
	{
		if (Declaration is null)
			return null;
		
		var member = new EnumMemberSymbol(name, Declaration);
		if (!AddChild(member))
			return null;
		
		members.Add(member.Name, member);
		return member;
	}
}