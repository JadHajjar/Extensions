using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
	public class Parallelism
	{
		public static void ForEach<TSource>(IEnumerable<TSource> source, Action<TSource> body)
		{
			if (ISave.CurrentPlatform == Platform.Windows)
			{
				Parallel.ForEach(source, body);
				return;
			}

			foreach (var item in source)
			{
				body(item);
			}
		}
	}
}
