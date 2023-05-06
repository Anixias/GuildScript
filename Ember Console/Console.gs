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
	
	public void Write(object obj)
	{
	    ExternalWrite(obj.ToString());
	}
	
	public void WriteLine(string text)
    {
        ExternalWriteLine(text);
    }
	
	public string ReadLine()
	{
		return ExternalReadLine();
	}
}