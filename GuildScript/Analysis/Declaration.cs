using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis;

public sealed class Declaration
{
	public SyntaxToken? SourceIdentifier { get; }
	public SyntaxNode SourceNode { get; }
		
	public Declaration(SyntaxToken? sourceIdentifier, SyntaxNode sourceNode)
	{
		SourceIdentifier = sourceIdentifier;
		SourceNode = sourceNode;
	}
}