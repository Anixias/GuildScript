namespace GuildScript.Analysis.Semantics.Symbols;

public interface ICallable
{
	ICallable? FindOverload(List<ResolvedType?> argumentTypes, int templateCount = 0);
	ResolvedType? ReturnType { get; }
}