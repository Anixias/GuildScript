namespace GuildScript.Analysis.Semantics.Symbols;

public sealed class EventSymbol : MemberSymbol
{
	public EventModifier EventModifier { get; }
	private readonly List<ParameterSymbol> parameters = new();

	public EventSymbol(string name, Declaration declaration, AccessModifier accessModifier, EventModifier eventModifier)
		: base(name, declaration, accessModifier)
	{
		EventModifier = eventModifier;
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration, bool isReference)
	{
		var parameter = new ParameterSymbol(name, declaration, isReference);
		parameters.Add(parameter);
		return parameter;
	}
}