namespace GuildScript.Analysis.Semantics;

public class ResolvedType
{
	public NameResolver.Scope Scope { get; }
	public ResolvedType? ParentType { get; private set; }
	public string Name => declaration.SourceIdentifier.Text;
	public IEnumerable<ResolvedType> NestedTypes => nestedTypes;

	private readonly Declaration declaration;
	private readonly List<ResolvedType> nestedTypes = new();

	public ResolvedType(Declaration declaration, NameResolver.Scope scope)
	{
		this.declaration = declaration;
		Scope = scope;
	}

	public ResolvedType(ResolvedType parentType, Declaration declaration, NameResolver.Scope scope)
		: this(declaration, scope)
	{
		parentType.NestType(this);
	}

	public void NestType(ResolvedType type)
	{
		if (type.ParentType is { } parent)
		{
			parent.nestedTypes.Remove(type);
		}
		
		nestedTypes.Add(type);
		type.ParentType = this;
	}
}