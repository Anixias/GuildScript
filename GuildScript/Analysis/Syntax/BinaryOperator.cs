using GuildScript.Analysis.Semantics;
using GuildScript.Analysis.Semantics.Symbols;

namespace GuildScript.Analysis.Syntax;

public sealed class BinaryOperator
{
	private struct OperationTypeMapping
	{
		public ResolvedType Left { get; }
		public ResolvedType Right { get; }
		public BinaryOperation Operation { get; }
		public ResolvedType Result { get; }
		
		public OperationTypeMapping(ResolvedType left, ResolvedType right, BinaryOperation operation,
									ResolvedType result)
		{
			Left = left;
			Right = right;
			Operation = operation;
			Result = result;
		}
	}

	private static IEnumerable<OperationTypeMapping> GenerateSimpleMappings(
		ResolvedType left, ResolvedType right, ResolvedType result)
	{
		yield return new OperationTypeMapping(left, right, BinaryOperation.Add, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Subtract, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Multiply, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Divide, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Modulo, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Exponent, result);
	}

	private static List<OperationTypeMapping> GenerateAllowedOperations()
	{
		var mappings = new List<OperationTypeMapping>();

		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.Int8, SimpleResolvedType.Int8));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.UInt8, SimpleResolvedType.Int8));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.Int16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.UInt16, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int8, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Int8, SimpleResolvedType.Int8));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.UInt8, SimpleResolvedType.UInt8));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Int16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.UInt16, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Double, SimpleResolvedType.Double));
		
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.Int8, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.UInt8, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.Int16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.UInt16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int16, SimpleResolvedType.Double, SimpleResolvedType.Double));
		
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Int8, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.UInt8, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Int16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.UInt16, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Double, SimpleResolvedType.Double));
		
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.Int8, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.UInt8, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.Int16, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.UInt16, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int32, SimpleResolvedType.Double, SimpleResolvedType.Double));
		
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Int8, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.UInt8, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Int16, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.UInt16, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Double, SimpleResolvedType.Double));
		
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.Int8, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.UInt8, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.Int16, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.UInt16, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.Int32, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.UInt32, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Int64, SimpleResolvedType.Double, SimpleResolvedType.Double));
		
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Int8, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.UInt8, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Int16, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.UInt16, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Int32, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.UInt32, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Double, SimpleResolvedType.Double));
		
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.Int8, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.UInt8, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.Int16, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.UInt16, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.Int32, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.UInt32, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.Int64, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.UInt64, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Single, SimpleResolvedType.Double, SimpleResolvedType.Double));
		
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.Int8, SimpleResolvedType.Double));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.UInt8, SimpleResolvedType.Double));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.Int16, SimpleResolvedType.Double));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.UInt16, SimpleResolvedType.Double));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.Int32, SimpleResolvedType.Double));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.UInt32, SimpleResolvedType.Double));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.Int64, SimpleResolvedType.Double));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.UInt64, SimpleResolvedType.Double));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.Single, SimpleResolvedType.Double));
		mappings.AddRange(GenerateSimpleMappings(SimpleResolvedType.Double, SimpleResolvedType.Double, SimpleResolvedType.Double));
		
		return mappings;
	}

	private static readonly List<OperationTypeMapping> AllowedOperations = GenerateAllowedOperations();

	public enum BinaryOperation
	{
		Assignment,
		AddAssign,
		SubtractAssign,
		MultiplyAssign,
		DivideAssign,
		ExponentAssign,
		ModuloAssign,
		BitwiseAndAssign,
		BitwiseOrAssign,
		BitwiseXorAssign,
		LogicalAndAssign,
		LogicalOrAssign,
		LogicalXorAssign,
		ShiftLeftAssign,
		ShiftRightAssign,
		RotateLeftAssign,
		RotateRightAssign,
		NullCoalescenceAssign,
		RangeRightExclusive,
		RangeLeftExclusive,
		RangeRightInclusive,
		RangeLeftInclusive,
		LogicalOr,
		LogicalXor,
		LogicalAnd,
		BitwiseOr,
		BitwiseXor,
		BitwiseAnd,
		Equality,
		Inequality,
		LessThan,
		GreaterThan,
		LessEqual,
		GreaterEqual,
		ShiftLeft,
		ShiftRight,
		RotateLeft,
		RotateRight,
		Add,
		Subtract,
		Multiply,
		Divide,
		Modulo,
		Exponent,
		NullCoalescence,
		Access,
		Cast,
		ConditionalCast,
		TypeEquality,
		TypeInequality,
		ConditionalAccess
	}

	public BinaryOperation? Operation => LookupBinaryOperation(TokenSpan);
	public SyntaxTokenSpan TokenSpan { get; }
	
	public BinaryOperator(SyntaxTokenSpan tokenSpan)
	{
		TokenSpan = tokenSpan;
	}
	
	public BinaryOperator(SyntaxToken operatorToken)
	{
		TokenSpan = new SyntaxTokenSpan(operatorToken);
	}

	public override string ToString()
	{
		return TokenSpan.ToString();
	}
	
	public override int GetHashCode()
	{
		return TokenSpan.GetHashCode();
	}

	public override bool Equals(object? obj)
	{
		if (obj is BinaryOperator binaryOperator)
			return Equals(binaryOperator);

		return false;
	}

	private bool Equals(BinaryOperator other)
	{
		return TokenSpan.Equals(other.TokenSpan);
	}

	public static BinaryOperation? LookupBinaryOperation(SyntaxTokenSpan tokenSpan)
	{
		return tokenSpan.Tokens.Length switch
		{
			1 => tokenSpan.Tokens[0].Type switch
			{
				SyntaxTokenType.Equal                 => BinaryOperation.Assignment,
				SyntaxTokenType.PlusEqual             => BinaryOperation.AddAssign,
				SyntaxTokenType.MinusEqual            => BinaryOperation.SubtractAssign,
				SyntaxTokenType.StarEqual             => BinaryOperation.MultiplyAssign,
				SyntaxTokenType.SlashEqual            => BinaryOperation.DivideAssign,
				SyntaxTokenType.Amp                   => BinaryOperation.BitwiseAnd,
				SyntaxTokenType.AmpAmp                => BinaryOperation.LogicalAnd,
				SyntaxTokenType.AmpEqual              => BinaryOperation.BitwiseAndAssign,
				SyntaxTokenType.AmpAmpEqual           => BinaryOperation.LogicalAndAssign,
				SyntaxTokenType.BangEqual             => BinaryOperation.Inequality,
				SyntaxTokenType.EqualEqual            => BinaryOperation.Equality,
				SyntaxTokenType.LeftAngled            => BinaryOperation.LessThan,
				SyntaxTokenType.LeftAngledEqual       => BinaryOperation.LessEqual,
				SyntaxTokenType.RightAngled           => BinaryOperation.GreaterThan,
				SyntaxTokenType.RightAngledEqual      => BinaryOperation.GreaterEqual,
				SyntaxTokenType.Plus                  => BinaryOperation.Add,
				SyntaxTokenType.Minus                 => BinaryOperation.Subtract,
				SyntaxTokenType.Star                  => BinaryOperation.Multiply,
				SyntaxTokenType.StarStar              => BinaryOperation.Exponent,
				SyntaxTokenType.StarStarEqual         => BinaryOperation.ExponentAssign,
				SyntaxTokenType.Slash                 => BinaryOperation.Divide,
				SyntaxTokenType.Caret                 => BinaryOperation.BitwiseXor,
				SyntaxTokenType.CaretCaret            => BinaryOperation.LogicalXor,
				SyntaxTokenType.CaretEqual            => BinaryOperation.BitwiseXorAssign,
				SyntaxTokenType.CaretCaretEqual       => BinaryOperation.LogicalXorAssign,
				SyntaxTokenType.Pipe                  => BinaryOperation.BitwiseOr,
				SyntaxTokenType.PipePipe              => BinaryOperation.LogicalOr,
				SyntaxTokenType.PipeEqual             => BinaryOperation.BitwiseOrAssign,
				SyntaxTokenType.PipePipeEqual         => BinaryOperation.LogicalOrAssign,
				SyntaxTokenType.Percent               => BinaryOperation.Modulo,
				SyntaxTokenType.PercentEqual          => BinaryOperation.ModuloAssign,
				SyntaxTokenType.LeftLeftEqual         => BinaryOperation.ShiftLeftAssign,
				SyntaxTokenType.LeftLeftLeftEqual     => BinaryOperation.RotateLeftAssign,
				SyntaxTokenType.RightRightEqual       => BinaryOperation.ShiftRightAssign,
				SyntaxTokenType.RightRightRightEqual  => BinaryOperation.RotateRightAssign,
				SyntaxTokenType.LeftArrow             => BinaryOperation.RangeLeftExclusive,
				SyntaxTokenType.LeftArrowArrow        => BinaryOperation.RangeLeftInclusive,
				SyntaxTokenType.RightArrow            => BinaryOperation.RangeRightExclusive,
				SyntaxTokenType.RightArrowArrow       => BinaryOperation.RangeRightInclusive,
				SyntaxTokenType.QuestionQuestion      => BinaryOperation.NullCoalescence,
				SyntaxTokenType.QuestionEqual         => BinaryOperation.TypeEquality,
				SyntaxTokenType.QuestionBangEqual     => BinaryOperation.TypeInequality,
				SyntaxTokenType.QuestionColon         => BinaryOperation.ConditionalCast,
				SyntaxTokenType.Colon                 => BinaryOperation.Cast,
				SyntaxTokenType.Dot                   => BinaryOperation.Access,
				SyntaxTokenType.QuestionQuestionEqual => BinaryOperation.NullCoalescenceAssign,
				SyntaxTokenType.QuestionDot           => BinaryOperation.ConditionalAccess,
				_                                     => null
			},
			
			2 => tokenSpan.Tokens[0].Type switch
			{
				SyntaxTokenType.LeftAngled when tokenSpan.Tokens[1].Type == SyntaxTokenType.LeftAngled =>
					BinaryOperation.ShiftLeft,
				
				SyntaxTokenType.RightAngled when tokenSpan.Tokens[1].Type == SyntaxTokenType.RightAngled =>
					BinaryOperation.ShiftRight,
				_ => null
			},
			
			3 => tokenSpan.Tokens[0].Type switch
			{
				SyntaxTokenType.LeftAngled when tokenSpan.Tokens[1].Type == SyntaxTokenType.LeftAngled &&
												tokenSpan.Tokens[2].Type == SyntaxTokenType.LeftAngled =>
					BinaryOperation.RotateLeft,
				
				SyntaxTokenType.RightAngled when tokenSpan.Tokens[1].Type == SyntaxTokenType.RightAngled &&
												 tokenSpan.Tokens[2].Type == SyntaxTokenType.RightAngled =>
					BinaryOperation.RotateRight,
				_ => null
			},
			_ => null
		};
	}
	
	public ResolvedType? GetResultType(ResolvedType leftType, ResolvedType rightType)
	{
		if (leftType.TypeSymbol is not NativeTypeSymbol)
			return null;
		
		if (rightType.TypeSymbol is not NativeTypeSymbol)
			return null;

		switch (Operation)
		{
			case BinaryOperation.Cast:
				return rightType;
			case BinaryOperation.Assignment:
				return leftType;
			case BinaryOperation.Equality:
			case BinaryOperation.Inequality:
			case BinaryOperation.LessThan:
			case BinaryOperation.GreaterThan:
			case BinaryOperation.LessEqual:
			case BinaryOperation.GreaterEqual:
				return SimpleResolvedType.Bool;
		}
		
		if (leftType == SimpleResolvedType.Int8)
		{
			if (rightType == SimpleResolvedType.Int8)
			{
				switch (Operation)
				{
					case BinaryOperation.Add:
					case BinaryOperation.AddAssign:
					case BinaryOperation.Subtract:
					case BinaryOperation.SubtractAssign:
					case BinaryOperation.Multiply:
					case BinaryOperation.MultiplyAssign:
					case BinaryOperation.Divide:
					case BinaryOperation.DivideAssign:
					case BinaryOperation.Modulo:
					case BinaryOperation.ModuloAssign:
					case BinaryOperation.Exponent:
					case BinaryOperation.ExponentAssign:
						return SimpleResolvedType.Int8;
					default:
						return null;
				}
			}
		}
	}
}