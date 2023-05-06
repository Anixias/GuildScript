using System.Diagnostics.CodeAnalysis;

namespace GuildScript.Analysis.Semantics;

public sealed class FieldSymbol : MemberSymbol
{
	public bool Resolved { get; private set; }
	public ResolvedType? Type { get; } = null;
	
	public FieldSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}
}