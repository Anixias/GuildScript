using System.Collections.Immutable;

namespace GuildScript.Analysis.Semantics.Symbols;

public abstract class MemberSymbol : Symbol
{
	public bool Resolved { get; set; }
	public Declaration Declaration { get; }
	public AccessModifier AccessModifier { get; }

	protected MemberSymbol(string name, Declaration declaration, AccessModifier accessModifier) : base(name)
	{
		Declaration = declaration;
		AccessModifier = accessModifier;
	}
}