using System.Collections.Generic;

namespace Extensions;

public class PathEqualityComparer : IEqualityComparer<string>
{
	public bool Equals(string x, string y)
	{
		return x.PathEquals(y);
	}

	public int GetHashCode(string obj)
	{
		return obj
			.Replace(CrossIO.InvalidPathSeparator, CrossIO.PathSeparator)
			.ToLower()
			.GetHashCode();
	}
}
