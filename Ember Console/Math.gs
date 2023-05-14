module System.Math;

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
}