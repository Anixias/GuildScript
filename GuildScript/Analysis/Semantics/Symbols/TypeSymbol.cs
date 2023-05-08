namespace GuildScript.Analysis.Semantics.Symbols;

public abstract class TypeSymbol : Symbol
{
	public AccessModifier AccessModifier { get; }
	protected Declaration? Declaration { get; }
	protected readonly Dictionary<string, TypeSymbol> nestedTypes = new();
	protected readonly Dictionary<string, MemberSymbol> members = new();
	
	protected TypeSymbol(string name, AccessModifier accessModifier) : base(name)
	{
		AccessModifier = accessModifier;
		Declaration = null;
	}
	
	protected TypeSymbol(string name, Declaration declaration, AccessModifier accessModifier) : base(name)
	{
		Declaration = declaration;
		AccessModifier = accessModifier;
	}

	public void NestType(TypeSymbol type)
	{
		if (!AddChild(type) || !nestedTypes.TryAdd(type.Name, type))
			throw new Exception($"The type '{type.Name}' is already declared in '{Name}'.");
	}

	public void AddMember(MemberSymbol member)
	{
		if (member is MethodSymbol method)
		{
			if (AddChild(member) && members.TryAdd(member.Name, member))
				return;
			
			if (children[member.Name] is MethodSymbol existingMethod)
			{
				existingMethod.AddOverload(method);
			}

			return;
		}
		
		if (!AddChild(member) || !members.TryAdd(member.Name, member))
			throw new Exception($"The member '{member.Name}' is already declared in '{Name}'.");
	}

	public virtual Symbol? FindMember(string name)
	{
		return members.TryGetValue(name, out var member) ? member : null;
	}
	
	public bool InheritsFrom(TypeSymbol ancestor)
	{
		// All types implicitly inherit from Object
		if (ancestor == NativeTypeSymbol.Object)
			return true;

		if (this == ancestor)
			return true;

		// Only classes can inherit (excluding inheriting from Object)
		if (this is not ClassSymbol classSymbol)
			return false;

		// Only classes and interfaces can be inherited
		switch (ancestor)
		{
			case ClassSymbol ancestorClass:
			{
				var baseClass = classSymbol.BaseClass;
				while (baseClass is not null)
				{
					if (baseClass == ancestorClass)
						return true;

					baseClass = baseClass.BaseClass;
				}
			}
				break;
			case InterfaceSymbol ancestorInterface:
			{
				var baseClass = classSymbol;
				while (baseClass is not null)
				{
					if (baseClass.ImplementsInterface(ancestorInterface))
						return true;

					baseClass = baseClass.BaseClass;
				}
			}
				break;
		}

		return false;
	}
}

public sealed class NativeTypeSymbol : TypeSymbol
{
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
	public static readonly NativeTypeSymbol Object = new("object");
	public static readonly NativeTypeSymbol String = new("string");
}