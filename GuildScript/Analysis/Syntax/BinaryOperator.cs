using GuildScript.Analysis.Semantics;
using GuildScript.Analysis.Semantics.Symbols;
using GuildScript.Analysis.Text;

namespace GuildScript.Analysis.Syntax;

public sealed class BinaryOperator : Operator
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

	private static IEnumerable<OperationTypeMapping> GenerateIntegerMappings(
		ResolvedType left, ResolvedType right, ResolvedType result)
	{
		yield return new OperationTypeMapping(left, right, BinaryOperation.Add, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Subtract, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Multiply, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Divide, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Modulo, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.Exponent, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.BitwiseAnd, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.BitwiseOr, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.BitwiseXor, result);
		yield return new OperationTypeMapping(left, right, BinaryOperation.ShiftLeft, left);
		yield return new OperationTypeMapping(left, right, BinaryOperation.ShiftRight, left);
		yield return new OperationTypeMapping(left, right, BinaryOperation.RotateLeft, left);
		yield return new OperationTypeMapping(left, right, BinaryOperation.RotateRight, left);
	}

	private static IEnumerable<OperationTypeMapping> GenerateNumericMappings(
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

		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int8, SimpleResolvedType.Int8, SimpleResolvedType.Int8));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int8, SimpleResolvedType.UInt8, SimpleResolvedType.Int8));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int8, SimpleResolvedType.Int16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int8, SimpleResolvedType.UInt16, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int8, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int8, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int8, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int8, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Int8, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Int8, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Int8, SimpleResolvedType.Int8));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt8, SimpleResolvedType.UInt8, SimpleResolvedType.UInt8));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Int16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt8, SimpleResolvedType.UInt16, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt8, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt8, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.UInt8, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int16, SimpleResolvedType.Int8, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int16, SimpleResolvedType.UInt8, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int16, SimpleResolvedType.Int16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int16, SimpleResolvedType.UInt16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int16, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int16, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int16, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int16, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Int16, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Int16, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Int8, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt16, SimpleResolvedType.UInt8, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Int16, SimpleResolvedType.Int16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt16, SimpleResolvedType.UInt16, SimpleResolvedType.UInt16));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt16, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt16, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.UInt16, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int32, SimpleResolvedType.Int8, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int32, SimpleResolvedType.UInt8, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int32, SimpleResolvedType.Int16, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int32, SimpleResolvedType.UInt16, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int32, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int32, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int32, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int32, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Int32, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Int32, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Int8, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt32, SimpleResolvedType.UInt8, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Int16, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt32, SimpleResolvedType.UInt16, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Int32, SimpleResolvedType.Int32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt32, SimpleResolvedType.UInt32, SimpleResolvedType.UInt32));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt32, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.UInt32, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int64, SimpleResolvedType.Int8, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int64, SimpleResolvedType.UInt8, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int64, SimpleResolvedType.Int16, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int64, SimpleResolvedType.UInt16, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int64, SimpleResolvedType.Int32, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int64, SimpleResolvedType.UInt32, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int64, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.Int64, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Int64, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Int64, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Int8, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt64, SimpleResolvedType.UInt8, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Int16, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt64, SimpleResolvedType.UInt16, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Int32, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt64, SimpleResolvedType.UInt32, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Int64, SimpleResolvedType.Int64));
		mappings.AddRange(GenerateIntegerMappings(SimpleResolvedType.UInt64, SimpleResolvedType.UInt64, SimpleResolvedType.UInt64));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.UInt64, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.Int8, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.UInt8, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.Int16, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.UInt16, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.Int32, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.UInt32, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.Int64, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.UInt64, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.Single, SimpleResolvedType.Single));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Single, SimpleResolvedType.Double, SimpleResolvedType.Double));

		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.Int8, SimpleResolvedType.Double));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.UInt8, SimpleResolvedType.Double));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.Int16, SimpleResolvedType.Double));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.UInt16, SimpleResolvedType.Double));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.Int32, SimpleResolvedType.Double));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.UInt32, SimpleResolvedType.Double));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.Int64, SimpleResolvedType.Double));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.UInt64, SimpleResolvedType.Double));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.Single, SimpleResolvedType.Double));
		mappings.AddRange(GenerateNumericMappings(SimpleResolvedType.Double, SimpleResolvedType.Double, SimpleResolvedType.Double));

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

	public bool IsCompoundAssignment
	{
		get
		{
			switch (Operation)
			{
				case BinaryOperation.AddAssign:
				case BinaryOperation.SubtractAssign:
				case BinaryOperation.MultiplyAssign:
				case BinaryOperation.DivideAssign:
				case BinaryOperation.ModuloAssign:
				case BinaryOperation.ExponentAssign:
				case BinaryOperation.BitwiseAndAssign:
				case BinaryOperation.BitwiseOrAssign:
				case BinaryOperation.BitwiseXorAssign:
				case BinaryOperation.LogicalAndAssign:
				case BinaryOperation.LogicalOrAssign:
				case BinaryOperation.LogicalXorAssign:
				case BinaryOperation.NullCoalescenceAssign:
				case BinaryOperation.ShiftLeftAssign:
				case BinaryOperation.ShiftRightAssign:
				case BinaryOperation.RotateLeftAssign:
				case BinaryOperation.RotateRightAssign:
					return true;
				default:
					return false;
			}
		}
	}

	public BinaryOperator(SyntaxTokenSpan tokenSpan) : base(tokenSpan)
	{
	}

	public BinaryOperator(SyntaxToken operatorToken) : base(new SyntaxTokenSpan(operatorToken))
	{
	}

	public (BinaryOperator, BinaryOperator?) Deconstruct()
	{
		var simple = AsSimpleOperation();
		var assignment = AsAssignmentOperation();

		return (simple, assignment);
	}

	public BinaryOperator AsSimpleOperation()
	{
		var sourceSpan = TokenSpan.Tokens[0].Span;
		var span = new TextSpan(sourceSpan.Start, 1, sourceSpan.SourceText);
		switch (Operation)
		{
			case BinaryOperation.AddAssign:
			{
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Plus, span.ToString(), null, span));
			}
			case BinaryOperation.SubtractAssign:
			{
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Minus, span.ToString(), null, span));
			}
			case BinaryOperation.MultiplyAssign:
			{
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Star, span.ToString(), null, span));
			}
			case BinaryOperation.DivideAssign:
			{
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Slash, span.ToString(), null, span));
			}
			case BinaryOperation.ModuloAssign:
			{
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Percent, span.ToString(), null, span));
			}
			case BinaryOperation.ExponentAssign:
			{
				span = new TextSpan(sourceSpan.Start, 2, sourceSpan.SourceText);
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.StarStar, span.ToString(), null, span));
			}
			case BinaryOperation.BitwiseAndAssign:
			{
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Amp, span.ToString(), null, span));
			}
			case BinaryOperation.BitwiseOrAssign:
			{
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Pipe, span.ToString(), null, span));
			}
			case BinaryOperation.BitwiseXorAssign:
			{
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Caret, span.ToString(), null, span));
			}
			case BinaryOperation.LogicalAndAssign:
			{
				span = new TextSpan(sourceSpan.Start, 2, sourceSpan.SourceText);
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.AmpAmp, span.ToString(), null, span));
			}
			case BinaryOperation.LogicalOrAssign:
			{
				span = new TextSpan(sourceSpan.Start, 2, sourceSpan.SourceText);
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.PipePipe, span.ToString(), null, span));
			}
			case BinaryOperation.LogicalXorAssign:
			{
				span = new TextSpan(sourceSpan.Start, 2, sourceSpan.SourceText);
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.CaretCaret, span.ToString(), null, span));
			}
			case BinaryOperation.NullCoalescenceAssign:
			{
				span = new TextSpan(sourceSpan.Start, 2, sourceSpan.SourceText);
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.QuestionQuestion, span.ToString(), null, span));
			}
			case BinaryOperation.ShiftLeftAssign:
			{
				var first = new SyntaxToken(SyntaxTokenType.LeftAngled, span.ToString(), null, span);
				span = new TextSpan(sourceSpan.Start + 1, 1, sourceSpan.SourceText);
				var second = new SyntaxToken(SyntaxTokenType.LeftAngled, span.ToString(), null, span);
				return new BinaryOperator(new SyntaxTokenSpan(first, second));
			}
			case BinaryOperation.ShiftRightAssign:
			{
				var first = new SyntaxToken(SyntaxTokenType.RightAngled, span.ToString(), null, span);
				span = new TextSpan(sourceSpan.Start + 1, 1, sourceSpan.SourceText);
				var second = new SyntaxToken(SyntaxTokenType.RightAngled, span.ToString(), null, span);
				return new BinaryOperator(new SyntaxTokenSpan(first, second));
			}
			case BinaryOperation.RotateLeftAssign:
			{
				var first = new SyntaxToken(SyntaxTokenType.LeftAngled, span.ToString(), null, span);
				span = new TextSpan(sourceSpan.Start + 1, 1, sourceSpan.SourceText);
				var second = new SyntaxToken(SyntaxTokenType.LeftAngled, span.ToString(), null, span);
				span = new TextSpan(sourceSpan.Start + 2, 1, sourceSpan.SourceText);
				var third = new SyntaxToken(SyntaxTokenType.LeftAngled, span.ToString(), null, span);
				return new BinaryOperator(new SyntaxTokenSpan(first, second, third));
			}
			case BinaryOperation.RotateRightAssign:
			{
				var first = new SyntaxToken(SyntaxTokenType.RightAngled, span.ToString(), null, span);
				span = new TextSpan(sourceSpan.Start + 1, 1, sourceSpan.SourceText);
				var second = new SyntaxToken(SyntaxTokenType.RightAngled, span.ToString(), null, span);
				span = new TextSpan(sourceSpan.Start + 2, 1, sourceSpan.SourceText);
				var third = new SyntaxToken(SyntaxTokenType.RightAngled, span.ToString(), null, span);
				return new BinaryOperator(new SyntaxTokenSpan(first, second, third));
			}
			default:
				return this;
		}
	}

	public BinaryOperator? AsAssignmentOperation()
	{
		var sourceSpan = TokenSpan.Tokens[0].Span;
		switch (Operation)
		{
			case BinaryOperation.AddAssign:
			case BinaryOperation.SubtractAssign:
			case BinaryOperation.MultiplyAssign:
			case BinaryOperation.DivideAssign:
			case BinaryOperation.ModuloAssign:
			case BinaryOperation.BitwiseAndAssign:
			case BinaryOperation.BitwiseOrAssign:
			case BinaryOperation.BitwiseXorAssign:
			{
				var span = new TextSpan(sourceSpan.Start + 1, 1, sourceSpan.SourceText);
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Equal, span.ToString(), null, span));
			}
			case BinaryOperation.ExponentAssign:
			case BinaryOperation.LogicalAndAssign:
			case BinaryOperation.LogicalOrAssign:
			case BinaryOperation.LogicalXorAssign:
			case BinaryOperation.NullCoalescenceAssign:
			case BinaryOperation.ShiftLeftAssign:
			case BinaryOperation.ShiftRightAssign:
			{
				var span = new TextSpan(sourceSpan.Start + 2, 1, sourceSpan.SourceText);
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Equal, span.ToString(), null, span));
			}
			case BinaryOperation.RotateLeftAssign:
			case BinaryOperation.RotateRightAssign:
			{
				var span = new TextSpan(sourceSpan.Start + 3, 1, sourceSpan.SourceText);
				return new BinaryOperator(new SyntaxToken(SyntaxTokenType.Equal, span.ToString(), null, span));
			}
			case BinaryOperation.Assignment:
				return this;
			default:
				return null;
		}
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
		var operation = Operation;
		switch (operation)
		{
			case BinaryOperation.Cast:
				return rightType;
			case BinaryOperation.ConditionalCast:
				return rightType is NullableResolvedType ? rightType : new NullableResolvedType(rightType);
			case BinaryOperation.Assignment:
			case BinaryOperation.AddAssign:
			case BinaryOperation.SubtractAssign:
			case BinaryOperation.MultiplyAssign:
			case BinaryOperation.DivideAssign:
			case BinaryOperation.ModuloAssign:
			case BinaryOperation.ExponentAssign:
			case BinaryOperation.BitwiseAndAssign:
			case BinaryOperation.BitwiseOrAssign:
			case BinaryOperation.BitwiseXorAssign:
			case BinaryOperation.LogicalAndAssign:
			case BinaryOperation.LogicalOrAssign:
			case BinaryOperation.LogicalXorAssign:
			case BinaryOperation.ShiftLeftAssign:
			case BinaryOperation.ShiftRightAssign:
			case BinaryOperation.RotateLeftAssign:
			case BinaryOperation.RotateRightAssign:
			case BinaryOperation.NullCoalescenceAssign:
				return leftType;
		}
		
		if (leftType.TypeSymbol is not NativeTypeSymbol || rightType.TypeSymbol is not NativeTypeSymbol)
		{
			return null;
		}

		if (operation == BinaryOperation.Add)
		{
			if (leftType == SimpleResolvedType.String || rightType == SimpleResolvedType.String)
				return SimpleResolvedType.String;
		}

		// No operations allowed on nullable types except null coalescence and string concatenation
		if (leftType is NullableResolvedType || rightType is NullableResolvedType)
			return null;

		switch (operation)
		{
			case BinaryOperation.RangeLeftExclusive:
			case BinaryOperation.RangeLeftInclusive:
			case BinaryOperation.RangeRightExclusive:
			case BinaryOperation.RangeRightInclusive:
				return SimpleResolvedType.Range;
			case BinaryOperation.Equality:
			case BinaryOperation.Inequality:
			case BinaryOperation.LessThan:
			case BinaryOperation.GreaterThan:
			case BinaryOperation.LessEqual:
			case BinaryOperation.GreaterEqual:
			case BinaryOperation.TypeEquality:
			case BinaryOperation.TypeInequality:
			case BinaryOperation.LogicalAnd:
			case BinaryOperation.LogicalOr:
			case BinaryOperation.LogicalXor:
				return SimpleResolvedType.Bool;
			case BinaryOperation.Add:
				if (leftType == SimpleResolvedType.Char)
				{
					if (rightType == SimpleResolvedType.Int8 ||
						rightType == SimpleResolvedType.UInt8 ||
						rightType == SimpleResolvedType.Int16 ||
						rightType == SimpleResolvedType.UInt16 ||
						rightType == SimpleResolvedType.Int32 ||
						rightType == SimpleResolvedType.UInt32 ||
						rightType == SimpleResolvedType.Int64 ||
						rightType == SimpleResolvedType.UInt64)
						return SimpleResolvedType.Char;
				}

				if (rightType == SimpleResolvedType.Char)
				{
					if (leftType == SimpleResolvedType.Int8 ||
						leftType == SimpleResolvedType.UInt8 ||
						leftType == SimpleResolvedType.Int16 ||
						leftType == SimpleResolvedType.UInt16 ||
						leftType == SimpleResolvedType.Int32 ||
						leftType == SimpleResolvedType.UInt32 ||
						leftType == SimpleResolvedType.Int64 ||
						leftType == SimpleResolvedType.UInt64)
						return SimpleResolvedType.Char;
				}

				break;
			case BinaryOperation.Subtract:
				if (leftType == SimpleResolvedType.Char)
				{
					if (rightType == SimpleResolvedType.Int8 ||
						rightType == SimpleResolvedType.UInt8 ||
						rightType == SimpleResolvedType.Int16 ||
						rightType == SimpleResolvedType.UInt16 ||
						rightType == SimpleResolvedType.Int32 ||
						rightType == SimpleResolvedType.UInt32 ||
						rightType == SimpleResolvedType.Int64 ||
						rightType == SimpleResolvedType.UInt64)
						return SimpleResolvedType.Char;
				}

				break;
			case BinaryOperation.NullCoalescence:
				if (leftType is NullableResolvedType leftNullableType)
					return leftNullableType.BaseType;

				// @TODO Print warning that left type is never null
				return leftType;
		}

		foreach (var mapping in AllowedOperations)
		{
			if (mapping.Left != leftType)
				continue;

			if (mapping.Right != rightType)
				continue;

			if (mapping.Operation != operation)
				continue;

			return mapping.Result;
		}

		return null;
	}
}
