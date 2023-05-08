using System.Collections.Immutable;

namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class LambdaTypeSymbol : TypeSymbol
{
	public List<ResolvedType> ParameterTypes { get; }
	public ResolvedType? ReturnType { get; set; }

	public LambdaTypeSymbol(List<ResolvedType> parameterTypes, ResolvedType? returnType)
		: base(BuildName(parameterTypes, returnType))
	{
		ParameterTypes = parameterTypes;
		ReturnType = returnType;
	}

	public LambdaTypeSymbol(List<ResolvedType> parameterTypes, ResolvedType? returnType,
							Declaration declaration) : base(BuildName(parameterTypes, returnType), declaration)
	{
		ParameterTypes = parameterTypes;
		ReturnType = returnType;
	}

	private static string BuildName(IEnumerable<ResolvedType> parameterTypes, ResolvedType? returnType)
	{
		var parameterTypeNames = string.Join(", ", parameterTypes.Select(t => t.TypeSymbol.Name));

		return returnType is not null
			? $"Lambda: [{parameterTypeNames}] <| [{returnType.TypeSymbol.Name}]"
			: $"Lambda: [{parameterTypeNames}]";
	}
}