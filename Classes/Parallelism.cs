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
			if (ISave.CurrentPlatform == Platform.Windows && source.Count > 10)
			{
				Parallel.ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = (source.Count / 100).Between(1, 100) }, body);
				return;
			}

			for (var i = 0; i < source.Count; i++)
			{
				body(source[i]);
			}
		}

		public static void ForEach(List<ExtensionClass.action> source)
		{
			if (ISave.CurrentPlatform == Platform.Windows && source.Count > 10)
			{
				Parallel.ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = (source.Count / 100).Between(1, 100) }, x => x());
				return;
			}

			for (var i = 0; i < source.Count; i++)
			{
				source[i]();
			}
		}
	}
}
