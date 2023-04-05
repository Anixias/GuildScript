namespace GuildScript.Analysis.Syntax;

public static class SyntaxInfo
{
	private static readonly SyntaxTokenType[][] BinaryOperatorPrecedence =
	{
		new[] { SyntaxTokenType.Plus, SyntaxTokenType.Minus },
		new[] { SyntaxTokenType.Star, SyntaxTokenType.Slash },
		new[] { SyntaxTokenType.Hat }
	};

	public static int GetBinaryOperatorPrecedence(this SyntaxTokenType type)
	{
		for (var i = 0; i < BinaryOperatorPrecedence.Length; i++)
		{
			for (var j = 0; j < BinaryOperatorPrecedence[i].Length; j++)
			{
				if (BinaryOperatorPrecedence[i][j] == type)
					return i;
			}
		}

		return -1;
	}
	
	public static int GetBinaryOperatorAssociativity(this SyntaxTokenType operatorType)
	{
		return operatorType switch
		{
			SyntaxTokenType.Hat => 1,
			_                   => -1
		};
	}
}