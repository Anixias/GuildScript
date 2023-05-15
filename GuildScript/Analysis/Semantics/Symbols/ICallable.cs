namespace GuildScript.Analysis.Semantics.Symbols;

public interface ICallable
{
	ICallable? FindOverload(List<ResolvedType?> argumentTypes);
	ICallable? FindOverload(List<ResolvedType?> argumentTypes, int templateCount);
	ResolvedType? ReturnType { get; }
}