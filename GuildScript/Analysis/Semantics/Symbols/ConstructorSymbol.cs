namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ConstructorSymbol : MemberSymbol, ITypedSymbol, ICallable
{
	public ResolvedType? ReturnType => null;
	public ResolvedType Type => SimpleResolvedType.Method;
	public ConstructorSymbol? Initializer { get; set; }
	public IEnumerable<ResolvedExpression> InitializerArguments { get; set; } = Array.Empty<ResolvedExpression>();
	private readonly Dictionary<string, ParameterSymbol> parameters = new();
	private readonly List<ConstructorSymbol> overloads = new();

	public ConstructorSymbol(string name, Declaration declaration, AccessModifier accessModifier) : base(name,
		declaration, accessModifier)
	{
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

	public void AddOverload(ConstructorSymbol overload)
	{
		overloads.Add(overload);
	}

	public IEnumerable<ParameterSymbol> GetParameters()
	{
		return parameters.Values;
	}

	public IEnumerable<ConstructorSymbol> GetOverloads()
	{
		return overloads;
	}
	
	public ICallable? FindOverload(List<ResolvedType?> argumentTypes, int templateCount)
	{
		return FindOverload(argumentTypes);
	}

	public ICallable? FindOverload(List<ResolvedType?> argumentTypes)
	{
		var validOverloads = new List<ConstructorSymbol>();
		var localOverloads = new List<ConstructorSymbol> { this };
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
				var argumentType = argumentTypes[i];
				if (parameter.Type?.GetType() == argumentType?.GetType() &&
					parameter.Type?.TypeSymbol == argumentType?.TypeSymbol) 
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