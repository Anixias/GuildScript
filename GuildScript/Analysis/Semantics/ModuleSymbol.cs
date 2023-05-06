using GuildScript.Analysis.Syntax;

namespace GuildScript.Analysis.Semantics;

public sealed class ModuleSymbol : Symbol
{
	public ModuleSymbol? Parent { get; }
	private readonly Dictionary<string, TypeSymbol> types = new();
	private readonly Dictionary<string, ModuleSymbol> nestedModules = new();
	
	public ModuleSymbol(string name) : base(name)
	{
	}
	
	public ModuleSymbol(string name, ModuleSymbol parent) : this(name)
	{
		if (!parent.AddChild(this))
			throw new Exception($"Module '{parent}' already contains a symbol named '{name}'.");
		
		Parent = parent;
		parent.nestedModules.Add(name, this);
	}

	public ModuleSymbol CreateModule(string name)
	{
		return new ModuleSymbol(name, this);
	}

	public ModuleSymbol GetModule(string name)
	{
		return nestedModules[name];
	}

	public bool ContainsModule(string name)
	{
		return nestedModules.ContainsKey(name);
	}

	public override string ToString()
	{
		var result = "";
		var module = this;
		while (module is not null)
		{
			if (module == this)
				result = module.Name;
			else
				result = module.Name + "." + result;
			
			module = module.Parent;
		}

		return result;
	}

	public void AddType(TypeSymbol type)
	{
		if (!AddChild(type) || !types.TryAdd(type.Name, type))
			throw new Exception($"Type '{type.Name}' is already declared in module '{ToString()}'.");
	}
}