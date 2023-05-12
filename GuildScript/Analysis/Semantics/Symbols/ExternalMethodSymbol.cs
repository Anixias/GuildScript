namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class ExternalMethodSymbol : MemberSymbol, ITypedSymbol, ICallable
{
	public ResolvedType Type => SimpleResolvedType.Method;
	public ResolvedType? ReturnType { get; set; }
	
	private readonly Dictionary<string, ParameterSymbol> parameters = new();
	private readonly List<ExternalMethodSymbol> overloads = new();

	public ExternalMethodSymbol(string name, Declaration declaration) : base(name, declaration, AccessModifier.Private)
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

	public void AddOverload(ExternalMethodSymbol overload)
	{
		overloads.Add(overload);
	}

	public IEnumerable<ParameterSymbol> GetParameters()
	{
		return parameters.Values;
	}

	public IEnumerable<ExternalMethodSymbol> GetOverloads()
	{
		return overloads;
	}
	
	public ICallable? FindOverload(List<ResolvedType?> argumentTypes, int templateCount = 0)
	{
		if (templateCount != 0)
			return null;
		
		var validOverloads = new List<ExternalMethodSymbol>();
		var localOverloads = new List<ExternalMethodSymbol> { this };
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