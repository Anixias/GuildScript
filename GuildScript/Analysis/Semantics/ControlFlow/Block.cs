namespace GuildScript.Analysis.Semantics.ControlFlow;

public sealed class Block
{
	public bool ControlExits { get; set; }
	public List<ResolvedStatement> Statements { get; } = new();
	public List<Block> Successors { get; } = new();

	public void AddStatement(ResolvedStatement statement)
	{
		Statements.Add(statement);
	}
}