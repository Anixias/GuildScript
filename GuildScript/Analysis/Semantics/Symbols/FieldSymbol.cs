using System.Collections.Immutable;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class FieldSymbol : MemberSymbol
{
	public ImmutableArray<FieldModifier> Modifiers { get; }
	public ResolvedType? Type { get; set; }

	public FieldSymbol(string name, Declaration declaration, AccessModifier accessModifier,
					   IEnumerable<FieldModifier> modifiers) : base(name, declaration, accessModifier)

	{
		Modifiers = modifiers.ToImmutableArray();
	}
}