using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
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
}
