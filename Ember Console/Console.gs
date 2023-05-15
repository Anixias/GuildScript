module System.Console;

public global class Console
{
	external void ExternalWrite(string text);
	external void ExternalWriteLine(string text);
	external string ExternalReadLine();
	
	public void Write(string text)
	{
		ExternalWrite(text);
	}
	
	public void Write(object? obj)
	{
		ExternalWrite(obj?.ToString() ?? "null");
	}
	
	public void WriteLine(string text)
	{
		ExternalWriteLine(text);
	}
	
	public void WriteLine(object? obj)
	{
		ExternalWriteLine(obj?.ToString() ?? "null");
	}
	
	public string ReadLine()
	{
		return ExternalReadLine();
	}
}