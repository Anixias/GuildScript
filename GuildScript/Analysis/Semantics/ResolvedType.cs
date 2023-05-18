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

	public override bool Equals(object? obj)
	{
		if (obj is NullableResolvedType nullableResolvedType)
			return Equals(nullableResolvedType);

		return false;
	}

	public override int GetHashCode()
	{
		const int nullableMask = 0x7F4391A2;
		return BaseType.GetHashCode() ^ nullableMask;
	}

	private bool Equals(NullableResolvedType nullableResolvedType)
	{
		return BaseType.Equals(nullableResolvedType.BaseType);
	}
}

public sealed class ArrayResolvedType : ResolvedType
{
	public ResolvedType ElementType { get; }
	
	public ArrayResolvedType(ResolvedType elementType) : base(elementType.TypeSymbol)
	{
		ElementType = elementType;
	}

	public override bool Equals(object? obj)
	{
		if (obj is ArrayResolvedType arrayResolvedType)
			return Equals(arrayResolvedType);

		return false;
	}

	public override int GetHashCode()
	{
		const int arrayMask = 0x163CBB9D;
		return ElementType.GetHashCode() ^ arrayMask;
	}

	private bool Equals(ArrayResolvedType arrayResolvedType)
	{
		return ElementType.Equals(arrayResolvedType.ElementType);
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

	public override bool Equals(object? obj)
	{
		if (obj is TemplatedResolvedType templatedResolvedType)
			return Equals(templatedResolvedType);

		return false;
	}

	public override int GetHashCode()
	{
		const int templateMask = 0x332A7AFD;
		var mask = BaseType.GetHashCode() ^ templateMask;

		var index = 1;
		foreach (var template in TypeArguments)
		{
			mask ^= template.GetHashCode() << index++;
		}
		
		return mask;
	}

	private bool Equals(TemplatedResolvedType templatedResolvedType)
	{
		if (TypeArguments.Count != templatedResolvedType.TypeArguments.Count)
			return false;
		
		if (!BaseType.Equals(templatedResolvedType.BaseType))
			return false;
		
		for (var i = 0; i < TypeArguments.Count; i++)
		{
			if (!TypeArguments[i].Equals(templatedResolvedType.TypeArguments[i]))
				return false;
		}

		return true;
	}
}

public sealed class SimpleResolvedType : ResolvedType
{
	public SimpleResolvedType(TypeSymbol typeSymbol) : base(typeSymbol)
	{
	}

	public override string ToString()
	{
		return TypeSymbol.Name;
	}

	public override bool Equals(object? obj)
	{
		if (obj is SimpleResolvedType simpleResolvedType)
			return Equals(simpleResolvedType);

		return false;
	}

	public override int GetHashCode()
	{
		return TypeSymbol.GetHashCode();
	}

	private bool Equals(SimpleResolvedType simpleResolvedType)
	{
		return TypeSymbol == simpleResolvedType.TypeSymbol;
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
	public static readonly SimpleResolvedType Range = new(NativeTypeSymbol.Range);
	public static readonly SimpleResolvedType Method = new(NativeTypeSymbol.Method);
	public static readonly SimpleResolvedType Event = new(NativeTypeSymbol.Event);

	private static readonly Dictionary<Syntax.SyntaxTokenType, SimpleResolvedType> NativeTypes = new()
	{
		{ Syntax.SyntaxTokenType.Int8, Int8 },
		{ Syntax.SyntaxTokenType.UInt8, UInt8 },
		{ Syntax.SyntaxTokenType.Int16, Int16 },
		{ Syntax.SyntaxTokenType.UInt16, UInt16 },
		{ Syntax.SyntaxTokenType.Int32, Int32 },
		{ Syntax.SyntaxTokenType.UInt32, UInt32 },
		{ Syntax.SyntaxTokenType.Int64, Int64 },
		{ Syntax.SyntaxTokenType.UInt64, UInt64 },
		{ Syntax.SyntaxTokenType.Single, Single },
		{ Syntax.SyntaxTokenType.Double, Double },
		{ Syntax.SyntaxTokenType.Char, Char },
		{ Syntax.SyntaxTokenType.Bool, Bool },
		{ Syntax.SyntaxTokenType.Object, Object },
		{ Syntax.SyntaxTokenType.String, String }
	};

	public static ResolvedType? FindNativeType(Syntax.SyntaxTokenType token)
	{
		return NativeTypes.TryGetValue(token, out var type) ? type : null;
	}
}