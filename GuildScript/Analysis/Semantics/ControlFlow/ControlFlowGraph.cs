namespace GuildScript.Analysis.Semantics.ControlFlow;

public sealed class ControlFlowGraph
{
	public List<Block> Blocks { get; } = new();

	public void Add(Block block)
	{
		Blocks.Add(block);
	}
}