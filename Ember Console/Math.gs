module System.Math;

public global class Math
{
	public global double Abs(double number)
	{
		return number < 0.0 ? -number : number;
	}
	
	public global double Sqrt(double number)
	{
		if (number < 0.0)
			throw "Cannot calculate square root of negative values.";
		
		var guess = number / 2.0;
		var threshold = 0.0001;
		
		while (Abs(guess * guess - number) > threshold)
		{
			guess = (guess + number / guess) / 2.0;
		}
		
		return guess;
	}
}

public struct Vector2
{
    public double x;
    public double y;
    
    constructor (double x, double y)
    {
        this.x = x;
        this.y = y;
    }
    
    [+] Vector2 (Vector2 left, Vector2 right)
    {
        return new Vector2(left.x + right.x, left.y + right.y);
    }
    
    private Vector2 Test()
    {
        var a = new Vector2(1.0, 2.0);
        var b = new Vector2(-2.4, 3.14);
        
        return a + b;
    }
    
    // Implicit cast overload from Vector2 to single (for example, based on vector magnitude)
    implicit double()
    {
        return Math.Sqrt(x * x + y * y);
    }

    // Explicit cast overload from Vector2 to bool (for example, based on non-zero magnitude)
    explicit bool()
    {
        return x != 0 || y != 0;
    }
    
    private single Test2(Vector2 a)
    {
        return a.x:single;
    }
}