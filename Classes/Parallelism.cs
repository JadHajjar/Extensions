using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
	public class Parallelism
	{
		public static void ForEach<TSource>(List<TSource> source, Action<TSource> body)
		{
			if (ISave.CurrentPlatform == Platform.Windows)
			{
				Parallel.ForEach(source, body);
				return;
			}

			for (var i = 0; i < source.Count; i++)
			{
				body(source[i]);
			}
		}
	}
}
