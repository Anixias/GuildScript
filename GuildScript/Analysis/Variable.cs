using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis;

public sealed class Variable
{
	public TypeSyntax Type { get; }
	public SyntaxToken Name { get; }
	public bool IsReference { get; }
	public Expression? Initializer { get; }

	public Variable(TypeSyntax type, SyntaxToken name, bool isReference, Expression? initializer = null)
	{
		Type = type;
		Name = name;
		IsReference = isReference;
		Initializer = initializer;
	}

	public override string ToString()
	{
		return $"{Type} {Name.Text}";
	}
}