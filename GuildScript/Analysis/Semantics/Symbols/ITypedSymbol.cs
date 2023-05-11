namespace GuildScript.Analysis.Semantics.Symbols;

public interface ITypedSymbol
{
	public ResolvedType? Type { get; }
	public bool Resolved { get; set; }
}