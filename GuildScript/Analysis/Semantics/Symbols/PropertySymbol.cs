using System.Collections.Immutable;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class PropertySymbol : MemberSymbol, ITypedSymbol
{
	public ImmutableArray<MethodModifier> Modifiers { get; }
	public ResolvedType? Type { get; set; }

	public PropertySymbol(string name, Declaration declaration, AccessModifier accessModifier,
						  IEnumerable<MethodModifier> modifiers) : base(name, declaration, accessModifier)

	{
		Modifiers = modifiers.ToImmutableArray();
	}
}