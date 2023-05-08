using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extensions
{
	public class Parallelism
	{
		public static void ForEach<TSource>(List<TSource> source, Action<TSource> body, int concurrentTasks = 0)
		{
			if (ISave.CurrentPlatform == Platform.Windows && source.Count > 10)
			{
				Parallel.ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = concurrentTasks == 0 ? (source.Count / 100).Between(1, 100) : concurrentTasks }, body);
				return;
			}

			for (var i = 0; i < source.Count; i++)
			{
				body(source[i]);
			}
		}

		public static void ForEach(List<ExtensionClass.action> source, int concurrentTasks = 0)
		{
			if (ISave.CurrentPlatform == Platform.Windows && source.Count > 10)
			{
				Parallel.ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = concurrentTasks == 0 ? (source.Count / 100).Between(1, 100) : concurrentTasks }, x => x());
				return;
			}

			for (var i = 0; i < source.Count; i++)
			{
				source[i]();
			}
		}
	}
}
