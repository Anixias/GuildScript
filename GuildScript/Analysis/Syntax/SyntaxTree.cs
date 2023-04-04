namespace GuildScript.Analysis.Syntax;

public sealed class SyntaxTree
{
	public SyntaxNode Root { get; }

	public SyntaxTree(SyntaxNode root)
	{
		Root = root;
	}
}