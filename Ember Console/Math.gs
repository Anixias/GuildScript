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
}