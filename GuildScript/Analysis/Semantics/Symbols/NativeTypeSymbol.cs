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
		
		Int8.AddMember(new NativeMethodSymbol($"explicit:{UInt8.Name}", SimpleResolvedType.UInt8));
		Int8.AddMember(new NativeMethodSymbol($"implicit:{Int16.Name}", SimpleResolvedType.Int16));
		Int8.AddMember(new NativeMethodSymbol($"explicit:{UInt16.Name}", SimpleResolvedType.UInt16));
		Int8.AddMember(new NativeMethodSymbol($"implicit:{Int32.Name}", SimpleResolvedType.Int32));
		Int8.AddMember(new NativeMethodSymbol($"explicit:{UInt32.Name}", SimpleResolvedType.UInt32));
		Int8.AddMember(new NativeMethodSymbol($"implicit:{Int64.Name}", SimpleResolvedType.Int64));
		Int8.AddMember(new NativeMethodSymbol($"explicit:{UInt64.Name}", SimpleResolvedType.UInt64));
		Int8.AddMember(new NativeMethodSymbol($"implicit:{Single.Name}", SimpleResolvedType.Single));
		Int8.AddMember(new NativeMethodSymbol($"implicit:{Double.Name}", SimpleResolvedType.Double));
		
		Int16.AddMember(new NativeMethodSymbol($"explicit:{Int8.Name}", SimpleResolvedType.Int8));
		Int16.AddMember(new NativeMethodSymbol($"explicit:{UInt8.Name}", SimpleResolvedType.UInt8));
		Int16.AddMember(new NativeMethodSymbol($"explicit:{UInt16.Name}", SimpleResolvedType.UInt16));
		Int16.AddMember(new NativeMethodSymbol($"implicit:{Int32.Name}", SimpleResolvedType.Int32));
		Int16.AddMember(new NativeMethodSymbol($"explicit:{UInt32.Name}", SimpleResolvedType.UInt32));
		Int16.AddMember(new NativeMethodSymbol($"implicit:{Int64.Name}", SimpleResolvedType.Int64));
		Int16.AddMember(new NativeMethodSymbol($"explicit:{UInt64.Name}", SimpleResolvedType.UInt64));
		Int16.AddMember(new NativeMethodSymbol($"implicit:{Single.Name}", SimpleResolvedType.Single));
		Int16.AddMember(new NativeMethodSymbol($"implicit:{Double.Name}", SimpleResolvedType.Double));
		
		Int32.AddMember(new NativeMethodSymbol($"explicit:{Int8.Name}", SimpleResolvedType.Int8));
		Int32.AddMember(new NativeMethodSymbol($"explicit:{UInt8.Name}", SimpleResolvedType.UInt8));
		Int32.AddMember(new NativeMethodSymbol($"explicit:{Int16.Name}", SimpleResolvedType.Int16));
		Int32.AddMember(new NativeMethodSymbol($"explicit:{UInt16.Name}", SimpleResolvedType.UInt16));
		Int32.AddMember(new NativeMethodSymbol($"explicit:{UInt32.Name}", SimpleResolvedType.UInt32));
		Int32.AddMember(new NativeMethodSymbol($"implicit:{Int64.Name}", SimpleResolvedType.Int64));
		Int32.AddMember(new NativeMethodSymbol($"explicit:{UInt64.Name}", SimpleResolvedType.UInt64));
		Int32.AddMember(new NativeMethodSymbol($"implicit:{Single.Name}", SimpleResolvedType.Single));
		Int32.AddMember(new NativeMethodSymbol($"implicit:{Double.Name}", SimpleResolvedType.Double));
		
		Int64.AddMember(new NativeMethodSymbol($"explicit:{Int8.Name}", SimpleResolvedType.Int8));
		Int64.AddMember(new NativeMethodSymbol($"explicit:{UInt8.Name}", SimpleResolvedType.UInt8));
		Int64.AddMember(new NativeMethodSymbol($"explicit:{Int16.Name}", SimpleResolvedType.Int16));
		Int64.AddMember(new NativeMethodSymbol($"explicit:{UInt16.Name}", SimpleResolvedType.UInt16));
		Int64.AddMember(new NativeMethodSymbol($"explicit:{Int32.Name}", SimpleResolvedType.Int32));
		Int64.AddMember(new NativeMethodSymbol($"explicit:{UInt32.Name}", SimpleResolvedType.UInt32));
		Int64.AddMember(new NativeMethodSymbol($"explicit:{UInt64.Name}", SimpleResolvedType.UInt64));
		Int64.AddMember(new NativeMethodSymbol($"implicit:{Single.Name}", SimpleResolvedType.Single));
		Int64.AddMember(new NativeMethodSymbol($"implicit:{Double.Name}", SimpleResolvedType.Double));
		
		UInt8.AddMember(new NativeMethodSymbol($"explicit:{Int8.Name}", SimpleResolvedType.Int8));
		UInt8.AddMember(new NativeMethodSymbol($"explicit:{Int16.Name}", SimpleResolvedType.Int16));
		UInt8.AddMember(new NativeMethodSymbol($"implicit:{UInt16.Name}", SimpleResolvedType.UInt16));
		UInt8.AddMember(new NativeMethodSymbol($"explicit:{Int32.Name}", SimpleResolvedType.Int32));
		UInt8.AddMember(new NativeMethodSymbol($"implicit:{UInt32.Name}", SimpleResolvedType.UInt32));
		UInt8.AddMember(new NativeMethodSymbol($"explicit:{Int64.Name}", SimpleResolvedType.Int64));
		UInt8.AddMember(new NativeMethodSymbol($"implicit:{UInt64.Name}", SimpleResolvedType.UInt64));
		UInt8.AddMember(new NativeMethodSymbol($"implicit:{Single.Name}", SimpleResolvedType.Single));
		UInt8.AddMember(new NativeMethodSymbol($"implicit:{Double.Name}", SimpleResolvedType.Double));
		
		UInt16.AddMember(new NativeMethodSymbol($"explicit:{Int8.Name}", SimpleResolvedType.Int8));
		UInt16.AddMember(new NativeMethodSymbol($"explicit:{UInt8.Name}", SimpleResolvedType.UInt8));
		UInt16.AddMember(new NativeMethodSymbol($"explicit:{Int16.Name}", SimpleResolvedType.Int16));
		UInt16.AddMember(new NativeMethodSymbol($"explicit:{Int32.Name}", SimpleResolvedType.Int32));
		UInt16.AddMember(new NativeMethodSymbol($"implicit:{UInt32.Name}", SimpleResolvedType.UInt32));
		UInt16.AddMember(new NativeMethodSymbol($"explicit:{Int64.Name}", SimpleResolvedType.Int64));
		UInt16.AddMember(new NativeMethodSymbol($"implicit:{UInt64.Name}", SimpleResolvedType.UInt64));
		UInt16.AddMember(new NativeMethodSymbol($"implicit:{Single.Name}", SimpleResolvedType.Single));
		UInt16.AddMember(new NativeMethodSymbol($"implicit:{Double.Name}", SimpleResolvedType.Double));
		
		UInt32.AddMember(new NativeMethodSymbol($"explicit:{Int8.Name}", SimpleResolvedType.Int8));
		UInt32.AddMember(new NativeMethodSymbol($"explicit:{UInt8.Name}", SimpleResolvedType.UInt8));
		UInt32.AddMember(new NativeMethodSymbol($"explicit:{Int16.Name}", SimpleResolvedType.Int16));
		UInt32.AddMember(new NativeMethodSymbol($"explicit:{UInt16.Name}", SimpleResolvedType.UInt16));
		UInt32.AddMember(new NativeMethodSymbol($"explicit:{Int32.Name}", SimpleResolvedType.Int32));
		UInt32.AddMember(new NativeMethodSymbol($"explicit:{Int64.Name}", SimpleResolvedType.Int64));
		UInt32.AddMember(new NativeMethodSymbol($"implicit:{UInt64.Name}", SimpleResolvedType.UInt64));
		UInt32.AddMember(new NativeMethodSymbol($"implicit:{Single.Name}", SimpleResolvedType.Single));
		UInt32.AddMember(new NativeMethodSymbol($"implicit:{Double.Name}", SimpleResolvedType.Double));
		
		UInt64.AddMember(new NativeMethodSymbol($"explicit:{Int8.Name}", SimpleResolvedType.Int8));
		UInt64.AddMember(new NativeMethodSymbol($"explicit:{UInt8.Name}", SimpleResolvedType.UInt8));
		UInt64.AddMember(new NativeMethodSymbol($"explicit:{Int16.Name}", SimpleResolvedType.Int16));
		UInt64.AddMember(new NativeMethodSymbol($"explicit:{UInt16.Name}", SimpleResolvedType.UInt16));
		UInt64.AddMember(new NativeMethodSymbol($"explicit:{Int32.Name}", SimpleResolvedType.Int32));
		UInt64.AddMember(new NativeMethodSymbol($"explicit:{UInt32.Name}", SimpleResolvedType.UInt32));
		UInt64.AddMember(new NativeMethodSymbol($"explicit:{Int64.Name}", SimpleResolvedType.Int64));
		UInt64.AddMember(new NativeMethodSymbol($"implicit:{Single.Name}", SimpleResolvedType.Single));
		UInt64.AddMember(new NativeMethodSymbol($"implicit:{Double.Name}", SimpleResolvedType.Double));
		
		Single.AddMember(new NativeMethodSymbol($"explicit:{Int8.Name}", SimpleResolvedType.Int8));
		Single.AddMember(new NativeMethodSymbol($"explicit:{UInt8.Name}", SimpleResolvedType.UInt8));
		Single.AddMember(new NativeMethodSymbol($"explicit:{Int16.Name}", SimpleResolvedType.Int16));
		Single.AddMember(new NativeMethodSymbol($"explicit:{UInt16.Name}", SimpleResolvedType.UInt16));
		Single.AddMember(new NativeMethodSymbol($"explicit:{Int32.Name}", SimpleResolvedType.Int32));
		Single.AddMember(new NativeMethodSymbol($"explicit:{UInt32.Name}", SimpleResolvedType.UInt32));
		Single.AddMember(new NativeMethodSymbol($"explicit:{Int64.Name}", SimpleResolvedType.Int64));
		Single.AddMember(new NativeMethodSymbol($"explicit:{UInt64.Name}", SimpleResolvedType.UInt64));
		Single.AddMember(new NativeMethodSymbol($"implicit:{Double.Name}", SimpleResolvedType.Double));
		
		Double.AddMember(new NativeMethodSymbol($"explicit:{Int8.Name}", SimpleResolvedType.Int8));
		Double.AddMember(new NativeMethodSymbol($"explicit:{UInt8.Name}", SimpleResolvedType.UInt8));
		Double.AddMember(new NativeMethodSymbol($"explicit:{Int16.Name}", SimpleResolvedType.Int16));
		Double.AddMember(new NativeMethodSymbol($"explicit:{UInt16.Name}", SimpleResolvedType.UInt16));
		Double.AddMember(new NativeMethodSymbol($"explicit:{Int32.Name}", SimpleResolvedType.Int32));
		Double.AddMember(new NativeMethodSymbol($"explicit:{UInt32.Name}", SimpleResolvedType.UInt32));
		Double.AddMember(new NativeMethodSymbol($"explicit:{Int64.Name}", SimpleResolvedType.Int64));
		Double.AddMember(new NativeMethodSymbol($"explicit:{UInt64.Name}", SimpleResolvedType.UInt64));
		Double.AddMember(new NativeMethodSymbol($"explicit:{Single.Name}", SimpleResolvedType.Single));
	}
}