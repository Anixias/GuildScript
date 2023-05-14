using GuildScript.Analysis.Semantics.Symbols;

namespace GuildScript;

public static class Compiler
{
	public static void Initialize()
	{
		NativeTypeSymbol.Initialize();
	}
}