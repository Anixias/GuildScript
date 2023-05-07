namespace GuildScript.Analysis.Semantics;

public class ResolvedTree
{
	public ResolvedNode Root { get; }
	
	public ResolvedTree(ResolvedNode root)
	{
		Root = root;
	}
}