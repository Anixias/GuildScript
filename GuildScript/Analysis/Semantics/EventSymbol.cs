namespace GuildScript.Analysis.Semantics;

public sealed class EventSymbol : MemberSymbol
{
	public bool Resolved { get; private set; }
	private readonly List<ParameterSymbol> parameters = new();
	
	public EventSymbol(string name, Declaration declaration) : base(name, declaration)
	{
	}

	public ParameterSymbol AddParameter(string name, Declaration declaration)
	{
		var parameter = new ParameterSymbol(name, declaration);
		parameters.Add(parameter);
		return parameter;
	}
}