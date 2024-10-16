using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Extensions;

public class Parallelism
{
	public static void ForEach<TSource>(List<TSource> source, Action<TSource> body, int concurrentTasks = 0)
	{
		if (concurrentTasks == 0)
		{
			concurrentTasks = (source.Count / 100).Between(1, 100);
		}

		if (CrossIO.CurrentPlatform == Platform.Windows && concurrentTasks > 1)
		{
			Parallel.ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = concurrentTasks }, body);
			return;
		}

		for (var i = 0; i < source.Count; i++)
		{
			body(source[i]);
		}
	}

	public static void ForEach(List<ExtensionClass.action> source, int concurrentTasks = 0)
	{
		if (concurrentTasks == 0)
		{
			concurrentTasks = (source.Count / 100).Between(1, 100);
		}

		if (CrossIO.CurrentPlatform == Platform.Windows && concurrentTasks > 1)
		{
			Parallel.ForEach(source, new ParallelOptions() { MaxDegreeOfParallelism = concurrentTasks }, x => x());
			return;
		}

		for (var i = 0; i < source.Count; i++)
		{
			source[i]();
		}
	}
}
