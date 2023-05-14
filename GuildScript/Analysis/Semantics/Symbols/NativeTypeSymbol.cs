namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class NativeTypeSymbol : TypeSymbol
{
	public override TypeSymbol? Ancestor => this == Object ? null : Object;
	
	private NativeTypeSymbol(string name) : base(name, AccessModifier.Public)
	{
	}

	public static readonly NativeTypeSymbol Int8 = new("int8");
	public static readonly NativeTypeSymbol UInt8 = new("uint8");
	public static readonly NativeTypeSymbol Int16 = new("int16");
	public static readonly NativeTypeSymbol UInt16 = new("uint16");
	public static readonly NativeTypeSymbol Int32 = new("int32");
	public static readonly NativeTypeSymbol UInt32 = new("uint32");
	public static readonly NativeTypeSymbol Int64 = new("int64");
	public static readonly NativeTypeSymbol UInt64 = new("uint64");
	public static readonly NativeTypeSymbol Single = new("single");
	public static readonly NativeTypeSymbol Double = new("double");
	public static readonly NativeTypeSymbol Char = new("char");
	public static readonly NativeTypeSymbol Bool = new("bool");
	public static readonly NativeTypeSymbol String = new("string");
	public static readonly NativeTypeSymbol Range = new("range");
	public static readonly NativeTypeSymbol Method = new("method");
	public static readonly NativeTypeSymbol Event = new("event");

	public static readonly NativeTypeSymbol Object = new("object");

	public static void Initialize()
	{
		Object.AddMember(new NativeMethodSymbol("ToString", SimpleResolvedType.String));
	}
}