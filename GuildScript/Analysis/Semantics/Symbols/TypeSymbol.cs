using GuildScript.Analysis.Syntax;

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

		if (member is IndexerSymbol indexer)
		{
			if (AddChild(member) && members.TryAdd(member.Name, member))
				return;

			if (children[member.Name] is IndexerSymbol existingMethod)
			{
				existingMethod.AddOverload(indexer);
			}

			return;
		}

		if (member is ExternalMethodSymbol externalMethod)
		{
			if (AddChild(member) && members.TryAdd(member.Name, member))
				return;

			if (children[member.Name] is ExternalMethodSymbol existingMethod)
			{
				existingMethod.AddOverload(externalMethod);
			}

			return;
		}

		if (member is ConstructorSymbol constructor)
		{
			if (AddChild(member) && members.TryAdd(member.Name, member))
				return;

			if (children[member.Name] is ConstructorSymbol existingMethod)
			{
				existingMethod.AddOverload(constructor);
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

	public MethodSymbol? FindOperatorOverload(ResolvedType leftType, BinaryOperator binaryOperator,
											  ResolvedType rightType)
	{
		var validOverloads = new List<MethodSymbol>();
		foreach (var member in members.Values)
		{
			if (member is not MethodSymbol method)
				continue;

			if (!method.IsOperator)
				continue;

			if (method.Operator!.Equals(binaryOperator))
				continue;

			var overloadList = new List<MethodSymbol> { method };
			overloadList.AddRange(method.GetOverloads());

			foreach (var overload in overloadList)
			{
				var parameters = overload.GetParameters().ToArray();
				if (parameters.Length != 2)
					continue;

				if (parameters[0].Type != leftType)
					continue;

				if (parameters[1].Type != rightType)
					continue;

				validOverloads.Add(overload);
			}
		}

		return validOverloads.Count switch
		{
			1   => validOverloads[0],
			> 1 => throw new Exception("Ambiguous operator overload reference."),
			_   => null
		};
	}

	public MethodSymbol? FindOperatorOverload(ResolvedType operandType, UnaryOperator unaryOperator)
	{
		var validOverloads = new List<MethodSymbol>();
		foreach (var member in members.Values)
		{
			if (member is not MethodSymbol method)
				continue;

			if (!method.IsOperator)
				continue;

			if (method.Operator!.Equals(unaryOperator))
				continue;

			var overloadList = new List<MethodSymbol> { method };
			overloadList.AddRange(method.GetOverloads());

			foreach (var overload in overloadList)
			{
				var parameters = overload.GetParameters().ToArray();
				if (parameters.Length != 1)
					continue;

				if (parameters[0].Type != operandType)
					continue;

				if (unaryOperator.IsPostfix != parameters[0].IsReference)
					continue;

				validOverloads.Add(overload);
			}
		}

		return validOverloads.Count switch
		{
			1   => validOverloads[0],
			> 1 => throw new Exception("Ambiguous operator overload reference."),
			_   => null
		};
	}

	public IndexerSymbol? FindIndexer(ResolvedType? keyType)
	{
		if (keyType is null)
			return null;
		
		var validOverloads = new List<IndexerSymbol>();
		foreach (var member in members.Values)
		{
			if (member is not IndexerSymbol indexer)
				continue;

			var overloadList = new List<IndexerSymbol> { indexer };
			overloadList.AddRange(indexer.GetOverloads());

			foreach (var overload in overloadList)
			{
				var parameters = overload.GetParameters().ToArray();
				if (parameters.Length != 1)
					continue;

				if (parameters[0].Type != keyType)
					continue;

				validOverloads.Add(overload);
			}
		}

		return validOverloads.Count switch
		{
			1   => validOverloads[0],
			> 1 => throw new Exception("Ambiguous indexer reference."),
			_   => null
		};
	}

	public MethodSymbol? FindMethod(List<ResolvedType?> argumentTypes, List<TypeSymbol> templateArguments)
	{
		var validOverloads = new List<MethodSymbol>();
		foreach (var member in members.Values)
		{
			if (member is not MethodSymbol method)
				continue;

			var overloadList = new List<MethodSymbol> { method };
			overloadList.AddRange(method.GetOverloads());

			foreach (var overload in overloadList)
			{
				var parameters = overload.GetParameters().ToArray();
				if (parameters.Length != argumentTypes.Count)
					continue;
				
				var typeParameters = overload.GetTemplateParameters().ToArray();
				if (typeParameters.Length != templateArguments.Count)
					continue;

				var matches = true;
				for (var i = 0; i < parameters.Length; i++)
				{
					var parameter = parameters[i];
					if (parameter.Type == argumentTypes[i])
						continue;
					
					matches = false;
					break;
				}

				if (!matches)
					continue;

				validOverloads.Add(overload);
			}
		}

		return validOverloads.Count switch
		{
			1   => validOverloads[0],
			> 1 => throw new Exception("Ambiguous method reference."),
			_   => null
		};
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
	public static readonly NativeTypeSymbol Range = new("range");
	public static readonly NativeTypeSymbol Method = new("method");
	public static readonly NativeTypeSymbol Event = new("event");
}
