using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class EnumSymbol : TypeSymbol
{
	public bool Resolved { get; set; }
	public TypeSyntax BaseTypeSyntax { get; }

	public ResolvedType? BaseType
	{
		get => baseType;
		set
		{
			baseType = value;

			foreach (var member in enumMembers)
			{
				member.Type = value;
				member.Resolved = true;
			}
		}
	}

	private readonly List<EnumMemberSymbol> enumMembers = new();
	private ResolvedType? baseType;

	public EnumSymbol(string name, Declaration declaration, AccessModifier accessModifier, TypeSyntax baseType)
		: base(name, declaration, accessModifier)
	{
		BaseTypeSyntax = baseType;
	}

	public EnumMemberSymbol? AddMember(string name)
	{
		if (Declaration is null)
			return null;
		
		var member = new EnumMemberSymbol(name, Declaration, this);
		if (!AddChild(member))
			return null;
		
		members.Add(member.Name, member);
		enumMembers.Add(member);
		return member;
	}

	public IEnumerable<MemberSymbol> GetMembers()
	{
		return enumMembers;
	}
}