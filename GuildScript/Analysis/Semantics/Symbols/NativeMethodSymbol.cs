namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class NativeMethodSymbol : MemberSymbol, ITypedSymbol, ICallable
{
	public ResolvedType Type => SimpleResolvedType.Method;
	public ResolvedType? ReturnType { get; }
	
	private readonly Dictionary<string, ParameterSymbol> parameters = new();
	private readonly List<NativeMethodSymbol> overloads = new();

	public NativeMethodSymbol(string name, ResolvedType? returnType) : base(name, Declaration.Empty, AccessModifier.Public)
	{
		ReturnType = returnType;
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration, bool isReference)
	{
		var parameter = new ParameterSymbol(name, declaration, isReference);
		parameters.Add(name, parameter);
		return parameter;
	}

	public void ResolveParameter(string name, ResolvedType type)
	{
		if (!parameters.ContainsKey(name))
			throw new Exception($"Parameter '{name}' does not exist.");

		var parameter = parameters[name];
		parameter.Type = type;
		parameter.Resolved = true;
	}

	public void AddOverload(NativeMethodSymbol overload)
	{
		overloads.Add(overload);
	}

	public IEnumerable<ParameterSymbol> GetParameters()
	{
		return parameters.Values;
	}

	public IEnumerable<NativeMethodSymbol> GetOverloads()
	{
		return overloads;
	}

	public ICallable? FindOverload(List<ResolvedType?> argumentTypes)
	{
		return FindOverload(argumentTypes, 0);
	}
	
	public ICallable? FindOverload(List<ResolvedType?> argumentTypes, int templateCount)
	{
		if (templateCount != 0)
			return null;
		
		var validOverloads = new List<NativeMethodSymbol>();
		var localOverloads = new List<NativeMethodSymbol> { this };
		localOverloads.AddRange(overloads);

		foreach (var overload in localOverloads)
		{	
			var overloadParameters = overload.GetParameters().ToArray();
			if (overloadParameters.Length != argumentTypes.Count)
				continue;

			var matches = true;
			for (var i = 0; i < overloadParameters.Length; i++)
			{
				var parameter = overloadParameters[i];
				if (parameter.Type == argumentTypes[i])
					continue;
			
				matches = false;
				break;
			}

			if (!matches)
				continue;

			validOverloads.Add(overload);
		}

		return validOverloads.Count switch
		{
			1   => validOverloads[0],
			> 1 => throw new Exception("Ambiguous method reference."),
			_   => null
		};
	}
}