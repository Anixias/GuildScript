using System.Collections.Immutable;
using System.Text;

namespace GuildScript.Analysis.Syntax;

public class ModuleName : IEquatable<ModuleName>
{
	public ImmutableArray<string> Names => names;
	private readonly ImmutableArray<string> names;

	public ModuleName(IEnumerable<string> names)
	{
		this.names = names.ToImmutableArray();
	}

	public override string ToString()
	{
		var stringBuilder = new StringBuilder();

		for (var i = 0; i < names.Length; i++)
		{
			stringBuilder.Append(names[i]);

			if (i < names.Length - 1)
				stringBuilder.Append('.');
		}

		return stringBuilder.ToString();
	}

	public override bool Equals(object? obj)
	{
		return obj is ModuleName moduleName && Equals(moduleName);
	}

	public bool Equals(ModuleName? other)
	{
		return other is not null && names.SequenceEqual(other.names);
	}

	public override int GetHashCode()
	{
		var hashCode = 0;

		foreach (var name in names)
		{
			foreach (var c in name)
			{
				hashCode ^= c.GetHashCode();
			}
		}

		return hashCode;
	}

	public static bool operator ==(ModuleName left, ModuleName right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ModuleName left, ModuleName right)
	{
		return !(left == right);
	}
}