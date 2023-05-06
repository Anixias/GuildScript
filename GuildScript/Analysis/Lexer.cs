using System.Text;
using GuildScript.Analysis.Syntax;
using GuildScript.Analysis.Text;

namespace GuildScript.Analysis;

public sealed class Lexer
{
	public DiagnosticCollection Diagnostics { get; }
	
	private bool EndOfFile => position >= source.Length;
	private char Current => Peek();
	private char Next => Peek(1);
	private int Length => position - start;
	private TextSpan Span => new(start, Length);

	private int start;
	private SyntaxTokenType type;
	private string Text => source.Substring(start, Length);
	private object? value;
	private int position;
	private readonly string source;

	public Lexer(string source)
	{
		Diagnostics = new DiagnosticCollection();
		this.source = source;
	}

	private void Reset()
	{
		start = position;
		type = SyntaxTokenType.Invalid;
		value = null;
	}

	private char Peek(int offset = 0)
	{
		var index = position + offset;
		if (index < 0 || index >= source.Length)
			return '\0';

		return source[index];
	}

	public SyntaxToken ScanToken()
	{
		SkipDecorations();
		
		if (EndOfFile)
		{
			return new SyntaxToken(SyntaxTokenType.EndOfFile, "\0", null, new TextSpan(position, 0));
		}
		
		Reset();
			
		switch (Current)
        {
            case '+':
                switch (Next)
                {
                    case '+':
                        ScanOperator(SyntaxTokenType.PlusPlus, 2);
                        break;
					case '=':
						ScanOperator(SyntaxTokenType.PlusEqual, 2);
						break;
                    default:
                        ScanOperator(SyntaxTokenType.Plus);
                        break;
                }
                break;
            case '-':
                switch (Next)
                {
                    case '-':
                        ScanOperator(SyntaxTokenType.MinusMinus, 2);
                        break;
                    case '=':
                        ScanOperator(SyntaxTokenType.MinusEqual, 2);
                        break;
                    case '>':
                        switch (Peek(2))
                        {
                            case '>':
                                ScanOperator(SyntaxTokenType.RightArrowArrow, 3);
                                break;
                            default:
                                ScanOperator(SyntaxTokenType.RightArrow, 2);
                                break;
                        }
                        break;
                    default:
                        ScanOperator(SyntaxTokenType.Minus);
                        break;
                }
                break;
            case '*':
                switch (Next)
                {
                    case '=':
                        ScanOperator(SyntaxTokenType.StarEqual, 2);
                        break;
					case '*':
						switch (Peek(2))
						{
							case '=':
								ScanOperator(SyntaxTokenType.StarStarEqual, 3);
								break;
							default:
								ScanOperator(SyntaxTokenType.StarStar, 2);
								break;
						}
						break;
                    default:
                        ScanOperator(SyntaxTokenType.Star);
                        break;
                }
                break;
            case '/':
                switch (Next)
                {
                    case '=':
                        ScanOperator(SyntaxTokenType.SlashEqual, 2);
                        break;
                    default:
                        ScanOperator(SyntaxTokenType.Slash);
                        break;
                }
                break;
            case '^':
                switch (Next)
                {
                    case '=':
                        ScanOperator(SyntaxTokenType.CaretEqual, 2);
                        break;
					case '^':
						switch (Peek(2))
						{
							case '=':
								ScanOperator(SyntaxTokenType.CaretCaretEqual, 3);
								break;
							default:
								ScanOperator(SyntaxTokenType.CaretCaret, 2);
								break;
						}
						break;
                    default:
                        ScanOperator(SyntaxTokenType.Caret);
                        break;
                }
                break;
            case '&':
                switch (Next)
                {
                    case '=':
                        ScanOperator(SyntaxTokenType.AmpEqual, 2);
                        break;
					case '&':
						switch (Peek(2))
						{
							case '=':
								ScanOperator(SyntaxTokenType.AmpAmpEqual, 3);
								break;
							default:
								ScanOperator(SyntaxTokenType.AmpAmp, 2);
								break;
						}
						break;
                    default:
                        ScanOperator(SyntaxTokenType.Amp);
                        break;
                }
                break;
            case '%':
                switch (Next)
                {
                    case '=':
						ScanOperator(SyntaxTokenType.PercentEqual, 2);
                        break;
                    default:
                        ScanOperator(SyntaxTokenType.Percent);
                        break;
                }
                break;
            case '(':
                ScanOperator(SyntaxTokenType.OpenParen);
                break;
            case ')':
                ScanOperator(SyntaxTokenType.CloseParen);
                break;
            case '{':
                ScanOperator(SyntaxTokenType.OpenBrace);
                break;
            case '}':
                ScanOperator(SyntaxTokenType.CloseBrace);
                break;
            case '[':
                ScanOperator(SyntaxTokenType.OpenSquare);
                break;
            case ']':
                ScanOperator(SyntaxTokenType.CloseSquare);
                break;
            case ',':
                ScanOperator(SyntaxTokenType.Comma);
                break;
            case '.':
                ScanOperator(SyntaxTokenType.Dot);
                break;
            case ';':
                ScanOperator(SyntaxTokenType.Semicolon);
                break;
			case ':':
				ScanOperator(SyntaxTokenType.Colon);
				break;
			case '~':
				ScanOperator(SyntaxTokenType.Tilde);
				break;
            case '<':
                switch (Next)
                {
                    case '|':
                        ScanOperator(SyntaxTokenType.LeftTriangle, 2);
                        break;
                    case '=':
                        ScanOperator(SyntaxTokenType.LeftAngledEqual, 2);
                        break;
                    case '-':
                        ScanOperator(SyntaxTokenType.LeftArrow, 2);
                        break;
                    case '<':
                        switch (Peek(2))
                        {
                            case '-':
                                ScanOperator(SyntaxTokenType.LeftArrowArrow, 3);
                                break;
							case '=':
								ScanOperator(SyntaxTokenType.LeftLeftEqual, 3);
								break;
							case '<':
								switch (Peek(3))
								{
									case '=':
										ScanOperator(SyntaxTokenType.LeftLeftLeftEqual, 4);
										break;
									default:
										ScanOperator(SyntaxTokenType.LeftAngled);
										break;
								}
								break;
                            default:
                                ScanOperator(SyntaxTokenType.LeftAngled);
                                break;
                        }
                        break;
                    default:
                        ScanOperator(SyntaxTokenType.LeftAngled);
                        break;
                }
                break;
            case '>':
                switch (Next)
                {
					case '>':
						switch (Peek(2))
						{
							case '=':
								ScanOperator(SyntaxTokenType.RightRightEqual, 3);
								break;
							case '>':
								switch (Peek(3))
								{
									case '=':
										ScanOperator(SyntaxTokenType.RightRightRightEqual, 4);
										break;
									default:
										ScanOperator(SyntaxTokenType.RightAngled);
										break;
								}
								break;
							default:
								ScanOperator(SyntaxTokenType.RightAngled);
								break;
						}
						break;
                    case '=':
                        ScanOperator(SyntaxTokenType.RightAngledEqual, 2);
                        break;
                    default:
                        ScanOperator(SyntaxTokenType.RightAngled);
                        break;
                }
                break;
            case '!':
                switch (Next)
                {
                    case '=':
                        ScanOperator(SyntaxTokenType.BangEqual, 2);
                        break;
					case '.':
						ScanOperator(SyntaxTokenType.BangDot, 2);
						break;
                    default:
                        ScanOperator(SyntaxTokenType.Bang);
                        break;
                }
                break;
            case '=':
                switch (Next)
                {
                    case '=':
                        ScanOperator(SyntaxTokenType.EqualEqual, 2);
                        break;
                    default:
                        ScanOperator(SyntaxTokenType.Equal);
                        break;
                }
                break;
            case '|':
                switch (Next)
                {
                    case '>':
                        ScanOperator(SyntaxTokenType.RightTriangle, 2);
                        break;
					case '=':
						ScanOperator(SyntaxTokenType.PipeEqual, 2);
						break;
					case '|':
						switch (Peek(2))
						{
							case '=':
								ScanOperator(SyntaxTokenType.PipePipeEqual, 3);
								break;
							default:
								ScanOperator(SyntaxTokenType.PipePipe, 2);
								break;
						}
						break;
					default:
						ScanOperator(SyntaxTokenType.Pipe);
						break;
                }
                break;
			case '?':
				switch (Next)
				{
					case '?':
						switch (Peek(2))
						{
							case '=':
								ScanOperator(SyntaxTokenType.QuestionQuestionEqual, 3);
								break;
							default:
								ScanOperator(SyntaxTokenType.QuestionQuestion, 2);
								break;
						}
						break;
					case '!':
						switch (Peek(2))
						{
							case '=':
								ScanOperator(SyntaxTokenType.QuestionBangEqual, 3);
								break;
							default:
								ScanOperator(SyntaxTokenType.Question);
								break;
						}
						break;
					case '.':
						ScanOperator(SyntaxTokenType.QuestionDot, 2);
						break;
					case '=':
						ScanOperator(SyntaxTokenType.QuestionEqual, 2);
						break;
					case ':':
						ScanOperator(SyntaxTokenType.QuestionColon, 2);
						break;
					case '[':
						ScanOperator(SyntaxTokenType.QuestionOpenSquare, 2);
						break;
					default:
						ScanOperator(SyntaxTokenType.Question);
						break;
				}
				break;
            case '"':
                ScanString();
                break;
            case '\'':
                ScanCharacter();
                break;
            default:
                if (char.IsDigit(Current))
                {
                    ScanNumber();
                }
                else if (char.IsLetter(Current))
                {
                    ScanIdentifier();
                }
                else
				{
					ScanInvalidCharacter();
				}
                break;
        }
			
		return new SyntaxToken(type, Text, value, Span);
	}
	
	private void SkipDecorations()
	{
		do
		{
			Reset();
			SkipComments();
			SkipWhiteSpace();
		} while (position > start);
	}

	private void SkipWhiteSpace()
	{
		while (char.IsWhiteSpace(Current))
		{
			Advance();
		}
	}
	
	private void SkipComments()
	{
		if (Current != '/')
			return;

		switch (Next)
		{
			// Match double slash as line comment
			case '/':
			{
				Advance();
				Advance();

				while (!EndOfFile)
				{
					if (Current is '\r' or '\n')
						break;

					Advance();
				}

				break;
			}
			// Match /* as a block comment */
			case '*':
			{
				Advance();
				Advance();
				
				var nestLevel = 1;
				while (!EndOfFile)
				{
					if (Current == '*' && Next == '/')
					{
						nestLevel--;
						Advance();
						Advance();

						if (nestLevel <= 0)
							break;
					}
					else if (Current == '/' && Next == '*')
					{
						nestLevel++;
						Advance();
						Advance();
						continue;
					}

					Advance();
				}

				break;
			}
		}
	}

	private void Advance()
	{
		position++;
	}

	private void ScanInvalidCharacter()
	{
		var invalidCharacter = Current;
		Advance();
		type = SyntaxTokenType.Invalid;

		Diagnostics.ReportScannerInvalidCharacter(new TextSpan(start, Length), invalidCharacter);
	}

	private void ScanNumber()
	{
		type = SyntaxTokenType.IntegerConstant;
		while (char.IsDigit(Current))
		{
			Advance();
		}

		if (Current == '.')
		{
			type = SyntaxTokenType.RealConstant;
			Advance();
			
			while (char.IsDigit(Current))
			{
				Advance();
			}
			
			if (double.TryParse(Text, out var doubleValue))
				value = doubleValue;
		}
		else if (int.TryParse(Text, out var intValue))
			value = intValue;
	}
	
	private void ScanIdentifier()
	{
		while (char.IsLetterOrDigit(Current) || Current == '_')
		{
			Advance();
		}

		type = SyntaxToken.LookupIdentifier(Text);
		value = null;
	}

	private void ScanOperator(SyntaxTokenType operatorType, int characterCount = 1)
	{
		while (Length < characterCount)
		{
			Advance();
		}

		type = operatorType;
	}
	
	private void ScanString()
	{
		var escaped = false;
		var stringBuilder = new StringBuilder();

		while (true)
		{
			Advance();

			if (escaped)
			{
				escaped = false;
				switch (Current)
				{
					case '\\':
						stringBuilder.Append('\\');
						break;
					case 'n':
						stringBuilder.Append('\n');
						break;
					case 'r':
						stringBuilder.Append('\r');
						break;
					case 't':
						stringBuilder.Append('\t');
						break;
					case '0':
						stringBuilder.Append('\0');
						break;
					case 'v':
						stringBuilder.Append('\v');
						break;
					case '"':
						stringBuilder.Append('"');
						break;
					case '\'':
						stringBuilder.Append('\'');
						break;
				}
			}
			else
			{
				if (Current == '"')
				{
					Advance();
					break;
				}

				if (Current == '\\')
					escaped = !escaped;
				else
					stringBuilder.Append(Current);
			}
		}

		type = SyntaxTokenType.StringConstant;
		value = stringBuilder.ToString();
	}
	
	private void ScanCharacter()
	{
		var escaped = false;
		var stringBuilder = new StringBuilder();

		while (true)
		{
			Advance();

			if (escaped)
			{
				escaped = false;
				switch (Current)
				{
					case '\\':
						stringBuilder.Append('\\');
						break;
					case 'n':
						stringBuilder.Append('\n');
						break;
					case 'r':
						stringBuilder.Append('\r');
						break;
					case 't':
						stringBuilder.Append('\t');
						break;
					case '0':
						stringBuilder.Append('\0');
						break;
					case 'v':
						stringBuilder.Append('\v');
						break;
					case '"':
						stringBuilder.Append('"');
						break;
					case '\'':
						stringBuilder.Append('\'');
						break;
				}
			}
			else
			{
				if (Current == '\'')
				{
					Advance();
					break;
				}

				if (Current == '\\')
					escaped = !escaped;
				else
					stringBuilder.Append(Current);
			}
		}

		type = SyntaxTokenType.CharacterConstant;
		var text = stringBuilder.ToString();
		value = text.Length > 0 ? text[0] : "\0";

		if (text.Length != 1)
		{
			Diagnostics.ReportScannerInvalidCharacterConstant(new TextSpan(start, Length), text);
		}
	}
}