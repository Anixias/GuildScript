using GuildScript.Analysis.Semantics.Symbols;

namespace GuildScript.Analysis.Semantics;

public abstract class ResolvedType
{
	public TypeSymbol TypeSymbol { get; }
	
	protected ResolvedType(TypeSymbol typeSymbol)
	{
		TypeSymbol = typeSymbol;
	}
}

public sealed class NullableResolvedType : ResolvedType
{
	public ResolvedType BaseType { get; }
	
	public NullableResolvedType(ResolvedType baseType) : base(baseType.TypeSymbol)
	{
		BaseType = baseType;
	}
}

public sealed class ArrayResolvedType : ResolvedType
{
	public ResolvedType ElementType { get; }
	
	public ArrayResolvedType(ResolvedType elementType) : base(elementType.TypeSymbol)
	{
		ElementType = elementType;
	}
}

public sealed class TemplatedResolvedType : ResolvedType
{
	public ResolvedType BaseType { get; }
	public IReadOnlyList<ResolvedType> TypeArguments { get; }
	
	public TemplatedResolvedType(ResolvedType baseType, IReadOnlyList<ResolvedType> typeArguments) : base(baseType.TypeSymbol)
	{
		BaseType = baseType;
		TypeArguments = typeArguments;
	}
}

public sealed class SimpleResolvedType : ResolvedType
{
	public SimpleResolvedType(TypeSymbol typeSymbol) : base(typeSymbol)
	{
	}

	public static readonly SimpleResolvedType Int8 = new(NativeTypeSymbol.Int8);
	public static readonly SimpleResolvedType UInt8 = new(NativeTypeSymbol.UInt8);
	public static readonly SimpleResolvedType Int16 = new(NativeTypeSymbol.Int16);
	public static readonly SimpleResolvedType UInt16 = new(NativeTypeSymbol.UInt16);
	public static readonly SimpleResolvedType Int32 = new(NativeTypeSymbol.Int32);
	public static readonly SimpleResolvedType UInt32 = new(NativeTypeSymbol.UInt32);
	public static readonly SimpleResolvedType Int64 = new(NativeTypeSymbol.Int64);
	public static readonly SimpleResolvedType UInt64 = new(NativeTypeSymbol.UInt64);
	public static readonly SimpleResolvedType Single = new(NativeTypeSymbol.Single);
	public static readonly SimpleResolvedType Double = new(NativeTypeSymbol.Double);
	public static readonly SimpleResolvedType Char = new(NativeTypeSymbol.Char);
	public static readonly SimpleResolvedType Bool = new(NativeTypeSymbol.Bool);
	public static readonly SimpleResolvedType Object = new(NativeTypeSymbol.Object);
	public static readonly SimpleResolvedType String = new(NativeTypeSymbol.String);

	private static readonly Dictionary<string, SimpleResolvedType> NativeTypes = new()
	{
		{ Syntax.SyntaxTokenType.Int8.ToString(), Int8 },
		{ Syntax.SyntaxTokenType.UInt8.ToString(), UInt8 },
		{ Syntax.SyntaxTokenType.Int16.ToString(), Int16 },
		{ Syntax.SyntaxTokenType.UInt16.ToString(), UInt16 },
		{ Syntax.SyntaxTokenType.Int32.ToString(), Int32 },
		{ Syntax.SyntaxTokenType.UInt32.ToString(), UInt32 },
		{ Syntax.SyntaxTokenType.Int64.ToString(), Int64 },
		{ Syntax.SyntaxTokenType.UInt64.ToString(), UInt64 },
		{ Syntax.SyntaxTokenType.Single.ToString(), Single },
		{ Syntax.SyntaxTokenType.Double.ToString(), Double },
		{ Syntax.SyntaxTokenType.Char.ToString(), Char },
		{ Syntax.SyntaxTokenType.Bool.ToString(), Bool },
		{ Syntax.SyntaxTokenType.Object.ToString(), Object },
		{ Syntax.SyntaxTokenType.String.ToString(), String }
	};

	public static ResolvedType? FindNativeType(string token)
	{
		return NativeTypes.TryGetValue(token, out var type) ? type : null;
	}
}