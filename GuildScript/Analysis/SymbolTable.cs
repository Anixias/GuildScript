using System.Collections.Generic;

namespace GuildScript.Analysis;

public sealed class SymbolTable
{
	private readonly Dictionary<string, Variable> variables = new();
}