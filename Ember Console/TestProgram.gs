// Access modifiers
// public
// private
// protected
// internal
// external

// Class modifiers
// unique = static
// template = abstract
// final = sealed

// Member modifiers
// shared = static

// Method/property modifiers
// prototype = virtual
// required = abstract

public template class Entity
{
	public required string FilePath <|;
	public double Health <>;
	public prototype string Name
	{
		<|;
		private |>;
		[char input] |> input<string>;
	}
}

public final class List<T>
{
	public int FindIndex([T] <| bool match)
	{
		for (var item in items)
		{
			if (match(item))
				return iteration;
		}
		
		return -1;
	}
}

var list = new List<string>
{
	"hello",
	"world"
};

var index = list.FindIndex([item] <| item == "hello");

[TParameter] <| TResult lambda = [parameter] <| expression;
[TParameter] <| TResult lambda = [parameter] <|
{
	return expression;
}

[TParameter] |> lambda = [parameter] |> statement;
[TParameter] |> lambda = [parameter] |>
{
	statement;
}

[] <| TResult lambda = [] <| expression;
[] <| TResult lambda = [] <|
{
	return expression;
}

[] |> lambda = [] |> statement;
[] |> lambda = [] |>
{
	statement;
}