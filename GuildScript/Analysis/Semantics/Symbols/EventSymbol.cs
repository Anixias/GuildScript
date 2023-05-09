namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class EventSymbol : MemberSymbol
{
	public EventModifier EventModifier { get; }
	private readonly Dictionary<string, ParameterSymbol> parameters = new();

	public EventSymbol(string name, Declaration declaration, AccessModifier accessModifier, EventModifier eventModifier)
		: base(name, declaration, accessModifier)
	{
		EventModifier = eventModifier;
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

	public IEnumerable<ParameterSymbol> GetParameters()
	{
		return parameters.Values;
	}
}