namespace GuildScript.Analysis;

public sealed class Module
{
	public readonly string name;
	public readonly Module? parent;
	private readonly SymbolTable symbolTable = new();

	public Module(string name, Module? parent = null)
	{
		this.name = name;
		this.parent = parent;
	}
}