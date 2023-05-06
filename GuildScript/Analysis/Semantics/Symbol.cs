namespace GuildScript.Analysis.Semantics;

public abstract class Symbol
{
	public string Name { get; }

	protected Symbol(string name)
	{
		Name = name;
	}
}