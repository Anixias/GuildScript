namespace GuildScript.Analysis.Syntax;

public sealed class BinaryOperator
{
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

	/*public static uint LookupBinaryOperationPrecedence(BinaryOperatorOperation operation)
	{
		switch (operation)
		{
			case BinaryOperatorOperation.Assignment:
			case BinaryOperatorOperation.AddAssign:
			case BinaryOperatorOperation.SubtractAssign:
			case BinaryOperatorOperation.MultiplyAssign:
			case BinaryOperatorOperation.DivideAssign:
			case BinaryOperatorOperation.ExponentAssign:
			case BinaryOperatorOperation.ModuloAssign:
			case BinaryOperatorOperation.BitwiseAndAssign:
			case BinaryOperatorOperation.BitwiseOrAssign:
			case BinaryOperatorOperation.BitwiseXorAssign:
			case BinaryOperatorOperation.LogicalAndAssign:
			case BinaryOperatorOperation.LogicalOrAssign:
			case BinaryOperatorOperation.LogicalXorAssign:
			case BinaryOperatorOperation.ShiftLeftAssign:
			case BinaryOperatorOperation.ShiftRightAssign:
			case BinaryOperatorOperation.RotateLeftAssign:
			case BinaryOperatorOperation.RotateRightAssign:
			case BinaryOperatorOperation.NullCoalescenceAssign:
				return 0;
			case BinaryOperatorOperation.RangeLeftExclusive:
			case BinaryOperatorOperation.RangeLeftInclusive:
			case BinaryOperatorOperation.RangeRightExclusive:
			case BinaryOperatorOperation.RangeRightInclusive:
				return 1;
			
		}
	}*/
}