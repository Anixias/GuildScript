namespace GuildScript.Analysis.Semantics.Symbols;

public abstract class OperatorSymbol : Symbol
{
	protected OperatorSymbol(string name) : base(name)
	{
	}
}

public sealed class NativeOperatorSymbol : OperatorSymbol
{
	public static NativeOperatorSymbol? LookupOperator(string name)
	{
		return NativeOperatorSymbols.TryGetValue(name, out var nativeOperatorSymbol) ? nativeOperatorSymbol : null;
	}
	
	private static readonly Dictionary<string, NativeOperatorSymbol> NativeOperatorSymbols = new();

	private NativeOperatorSymbol(string name) : base(name)
	{
		NativeOperatorSymbols.Add(name, this);
	}
	
	// Binary Operations
	public static readonly NativeOperatorSymbol Assignment = new("=");
	public static readonly NativeOperatorSymbol AddAssign = new("+=");
	public static readonly NativeOperatorSymbol SubtractAssign = new("-=");
	public static readonly NativeOperatorSymbol MultiplyAssign = new("*=");
	public static readonly NativeOperatorSymbol DivideAssign = new("/=");
	public static readonly NativeOperatorSymbol ExponentAssign = new("**=");
	public static readonly NativeOperatorSymbol ModuloAssign = new("%=");
	public static readonly NativeOperatorSymbol BitwiseAndAssign = new("&=");
	public static readonly NativeOperatorSymbol BitwiseOrAssign = new("|=");
	public static readonly NativeOperatorSymbol BitwiseXorAssign = new("^=");
	public static readonly NativeOperatorSymbol LogicalAndAssign = new("&&=");
	public static readonly NativeOperatorSymbol LogicalOrAssign = new("||=");
	public static readonly NativeOperatorSymbol LogicalXorAssign = new("^^=");
	public static readonly NativeOperatorSymbol ShiftLeftAssign = new("<<=");
	public static readonly NativeOperatorSymbol ShiftRightAssign = new(">>=");
	public static readonly NativeOperatorSymbol RotateLeftAssign = new("<<<=");
	public static readonly NativeOperatorSymbol RotateRightAssign = new(">>>=");
	public static readonly NativeOperatorSymbol NullCoalescenceAssign = new("??=");
	public static readonly NativeOperatorSymbol RangeRightExclusive = new("->");
	public static readonly NativeOperatorSymbol RangeLeftExclusive = new("<-");
	public static readonly NativeOperatorSymbol RangeRightInclusive = new("->>");
	public static readonly NativeOperatorSymbol RangeLeftInclusive = new("<<-");
	public static readonly NativeOperatorSymbol LogicalOr = new("||");
	public static readonly NativeOperatorSymbol LogicalXor = new("^^");
	public static readonly NativeOperatorSymbol LogicalAnd = new("&&");
	public static readonly NativeOperatorSymbol BitwiseOr = new("|");
	public static readonly NativeOperatorSymbol BitwiseXor = new("^");
	public static readonly NativeOperatorSymbol BitwiseAnd = new("&");
	public static readonly NativeOperatorSymbol Equality = new("==");
	public static readonly NativeOperatorSymbol Inequality = new("!=");
	public static readonly NativeOperatorSymbol LessThan = new("<");
	public static readonly NativeOperatorSymbol GreaterThan = new(">");
	public static readonly NativeOperatorSymbol LessEqual = new("<=");
	public static readonly NativeOperatorSymbol GreaterEqual = new(">=");
	public static readonly NativeOperatorSymbol ShiftLeft = new("<<");
	public static readonly NativeOperatorSymbol ShiftRight = new(">>");
	public static readonly NativeOperatorSymbol RotateLeft = new("<<<");
	public static readonly NativeOperatorSymbol RotateRight = new(">>>");
	public static readonly NativeOperatorSymbol Add = new("+");
	public static readonly NativeOperatorSymbol Subtract = new("-");
	public static readonly NativeOperatorSymbol Multiply = new("*");
	public static readonly NativeOperatorSymbol Divide = new("/");
	public static readonly NativeOperatorSymbol Modulo = new("%");
	public static readonly NativeOperatorSymbol Exponent = new("**");
	public static readonly NativeOperatorSymbol NullCoalescence = new("??");
	public static readonly NativeOperatorSymbol Access = new(".");
	public static readonly NativeOperatorSymbol Cast = new(":");
	public static readonly NativeOperatorSymbol ConditionalCast = new("?:");
	public static readonly NativeOperatorSymbol TypeEquality = new("?=");
	public static readonly NativeOperatorSymbol TypeInequality = new("?!=");
	public static readonly NativeOperatorSymbol ConditionalAccess = new("?.");
}