// Namespaces
// module = namespace
// using

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

// Field modifiers
// shared = static

// Method/property modifiers
// prototype = virtual
// required = abstract

using module System;

module TestProgram;

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

[TParameter] <| TResult lambda = [TParameter parameter] <| expression;
[TParameter] <| TResult lambda = [TParameter parameter] <|
{
	return expression;
}

[TParameter] |> lambda = [TParameter parameter] |> statement;
[TParameter] |> lambda = [TParameter parameter] |>
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