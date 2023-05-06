using System.Collections.Immutable;

namespace GuildScript.Analysis.Syntax;

public abstract class TypeSyntax
{
	public abstract bool IsNullable { get; }

	public static readonly BaseTypeSyntax Int8 = new(SyntaxTokenType.Int8);
	public static readonly BaseTypeSyntax UInt8 = new(SyntaxTokenType.UInt8);
	public static readonly BaseTypeSyntax Int16 = new(SyntaxTokenType.Int16);
	public static readonly BaseTypeSyntax UInt16 = new(SyntaxTokenType.UInt16);
	public static readonly BaseTypeSyntax Int32 = new(SyntaxTokenType.Int32);
	public static readonly BaseTypeSyntax UInt32 = new(SyntaxTokenType.UInt32);
	public static readonly BaseTypeSyntax Int64 = new(SyntaxTokenType.Int64);
	public static readonly BaseTypeSyntax UInt64 = new(SyntaxTokenType.UInt64);
	public static readonly BaseTypeSyntax Single = new(SyntaxTokenType.Single);
	public static readonly BaseTypeSyntax Double = new(SyntaxTokenType.Double);
	public static readonly BaseTypeSyntax Char = new(SyntaxTokenType.Char);
	public static readonly BaseTypeSyntax Bool = new(SyntaxTokenType.Bool);
	public static readonly BaseTypeSyntax Object = new(SyntaxTokenType.Object);
	public static readonly BaseTypeSyntax String = new(SyntaxTokenType.String);
	public static readonly BaseTypeSyntax Inferred = new(SyntaxTokenType.Var);
	
	public override string ToString()
	{
		return "(??)";
	}

	public override bool Equals(object? obj)
	{
		if (obj is TypeSyntax other)
			return Equals(other);

		return false;
	}

	private bool Equals(TypeSyntax other)
	{
		if (IsNullable != other.IsNullable)
			return false;

		return GetType() == other.GetType() && GetHashCode() == other.GetHashCode();
	}

	public override int GetHashCode()
	{
		return 0;
	}
}

public class NullableTypeSyntax : TypeSyntax
{
	public override bool IsNullable => true;

	public TypeSyntax BaseType { get; }

	public NullableTypeSyntax(TypeSyntax baseType)
	{
		BaseType = baseType;
	}
	
	public override string ToString()
	{
		return $"{BaseType}?";
	}
	
	public override int GetHashCode()
	{
		var hash = 0;
		hash ^= IsNullable.GetHashCode();
		hash ^= BaseType.GetHashCode() << 2;
		return hash;
	}
}

public class ArrayTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;

	public TypeSyntax BaseType { get; }

	public ArrayTypeSyntax(TypeSyntax baseType)
	{
		BaseType = baseType;
	}
	
	public override string ToString()
	{
		return $"{BaseType}[]";
	}
	
	public override int GetHashCode()
	{
		var hash = 0;
		hash ^= IsNullable.GetHashCode();
		hash ^= BaseType.GetHashCode() << 1;
		return hash;
	}
}

public class TemplatedTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;

	public NamedTypeSyntax BaseType { get; }
	public IReadOnlyList<TypeSyntax> TypeArguments { get; }

	public TemplatedTypeSyntax(NamedTypeSyntax baseType, IEnumerable<TypeSyntax> typeArguments)
	{
		BaseType = baseType;
		TypeArguments = typeArguments.ToImmutableArray();
	}
	
	public override string ToString()
	{
		var typeArgs = TypeArguments.Count > 0 ? $"<{string.Join(", ", TypeArguments)}>" : "";
		return $"{BaseType}{typeArgs}";
	}
	
	public override int GetHashCode()
	{
		var hash = 0;
		hash ^= IsNullable.GetHashCode();
		hash ^= BaseType.GetHashCode() << 1;
		hash ^= TypeArguments.GetHashCode() << 2;
		return hash;
	}
}

public class BaseTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;

	public SyntaxTokenType TokenType { get; }

	public BaseTypeSyntax(SyntaxTokenType tokenType)
	{
		TokenType = tokenType;
	}
	
	public override string ToString()
	{
		return TokenType.ToString();
	}
	
	public override int GetHashCode()
	{
		var hash = 0;
		hash ^= IsNullable.GetHashCode();
		hash ^= TokenType.GetHashCode() << 1;
		return hash;
	}
}

public class NamedTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;

	public string Name { get; }

	public NamedTypeSyntax(string name)
	{
		Name = name;
	}
	
	public override string ToString()
	{
		return Name;
	}
	
	public override int GetHashCode()
	{
		var hash = 0;
		hash ^= IsNullable.GetHashCode();
		hash ^= Name.GetHashCode() << 1;
		return hash;
	}
}

public class ExpressionTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;

	public Expression Expression { get; }

	public ExpressionTypeSyntax(Expression expression)
	{
		Expression = expression;
	}
	
	public override string ToString()
	{
		return Expression.ToString() ?? "(Expression)";
	}
	
	public override int GetHashCode()
	{
		var hash = 0;
		hash ^= IsNullable.GetHashCode();
		hash ^= Expression.GetHashCode() << 1;
		return hash;
	}
}

public class LambdaTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;

	public IReadOnlyList<TypeSyntax> InputTypes { get; }
	public TypeSyntax? OutputType { get; }

	public LambdaTypeSyntax(IEnumerable<TypeSyntax> inputTypes, TypeSyntax? outputType = null)
	{
		InputTypes = inputTypes.ToImmutableArray();
		OutputType = outputType;
	}
	
	public override string ToString()
	{
		var inputTypesStr = string.Join(", ", InputTypes);
		return OutputType is null ? $"[{inputTypesStr}] |> {{}}" : $"[{inputTypesStr}] <| [{OutputType}] {{}}";
	}
	
	public override int GetHashCode()
	{
		var hash = 0;
		hash ^= IsNullable.GetHashCode();
		hash ^= InputTypes.GetHashCode() << 1;
		hash ^= OutputType?.GetHashCode() << 2 ?? 0;
		return hash;
	}
}