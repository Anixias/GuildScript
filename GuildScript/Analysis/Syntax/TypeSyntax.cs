using System.Collections.Immutable;

namespace GuildScript.Analysis.Syntax;

public abstract class TypeSyntax
{
	public abstract bool IsNullable { get; }
	public abstract bool IsReferenceType { get; }

	public static readonly BaseTypeSyntax Int8 = new(SyntaxTokenType.Int8, false);
	public static readonly BaseTypeSyntax UInt8 = new(SyntaxTokenType.UInt8, false);
	public static readonly BaseTypeSyntax Int16 = new(SyntaxTokenType.Int16, false);
	public static readonly BaseTypeSyntax UInt16 = new(SyntaxTokenType.UInt16, false);
	public static readonly BaseTypeSyntax Int32 = new(SyntaxTokenType.Int32, false);
	public static readonly BaseTypeSyntax UInt32 = new(SyntaxTokenType.UInt32, false);
	public static readonly BaseTypeSyntax Int64 = new(SyntaxTokenType.Int64, false);
	public static readonly BaseTypeSyntax UInt64 = new(SyntaxTokenType.UInt64, false);
	public static readonly BaseTypeSyntax Single = new(SyntaxTokenType.Single, false);
	public static readonly BaseTypeSyntax Double = new(SyntaxTokenType.Double, false);
	public static readonly BaseTypeSyntax Char = new(SyntaxTokenType.Char, false);
	public static readonly BaseTypeSyntax Bool = new(SyntaxTokenType.Bool, false);
	public static readonly BaseTypeSyntax Object = new(SyntaxTokenType.Object, true);
	public static readonly BaseTypeSyntax String = new(SyntaxTokenType.String, true);
	public static readonly BaseTypeSyntax Inferred = new(SyntaxTokenType.Var, true);
	
	public override string ToString()
	{
		return "(??)";
	}
}

public class NullableTypeSyntax : TypeSyntax
{
	public override bool IsNullable => true;
	public override bool IsReferenceType => BaseType.IsReferenceType;

	public TypeSyntax BaseType { get; }

	public NullableTypeSyntax(TypeSyntax baseType)
	{
		BaseType = baseType;
	}
	
	public override string ToString()
	{
		return $"{BaseType}?";
	}
}

public class ArrayTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;
	public override bool IsReferenceType => true;

	public TypeSyntax BaseType { get; }

	public ArrayTypeSyntax(TypeSyntax baseType)
	{
		BaseType = baseType;
	}
	
	public override string ToString()
	{
		return $"{BaseType}[]";
	}
}

public class TemplatedTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;
	public override bool IsReferenceType => BaseType.IsReferenceType;

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
}



public class BaseTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;
	public override bool IsReferenceType { get; }

	public SyntaxTokenType TokenType { get; }

	public BaseTypeSyntax(SyntaxTokenType tokenType, bool isReferenceType)
	{
		TokenType = tokenType;
		IsReferenceType = isReferenceType;
	}
	
	public override string ToString()
	{
		return TokenType.ToString();
	}
}

public class NamedTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;
	public override bool IsReferenceType { get; }

	public string Name { get; }

	public NamedTypeSyntax(string name, bool isReferenceType)
	{
		Name = name;
		IsReferenceType = isReferenceType;
	}
	
	public override string ToString()
	{
		return Name;
	}
}

public class ExpressionTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;
	public override bool IsReferenceType { get; }

	public Expression Expression { get; }

	public ExpressionTypeSyntax(Expression expression)
	{
		Expression = expression;
		IsReferenceType = false;
	}
	
	public override string ToString()
	{
		return Expression.ToString() ?? "(Expression)";
	}
}

public class LambdaTypeSyntax : TypeSyntax
{
	public override bool IsNullable => false;
	public override bool IsReferenceType => true;

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
}