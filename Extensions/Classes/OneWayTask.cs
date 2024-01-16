using System.Threading;

namespace Extensions;

public class OneWayTask
{
	private Thread currentTask;
	private readonly object lockObj = new();

	public delegate void MyAction();

	public void Run(MyAction action, bool isBackground = true, ThreadPriority threadPriority = ThreadPriority.Normal)
	{
		if (currentTask != null && currentTask.IsAlive)
		{
			currentTask.Interrupt();
			currentTask.Abort();
		}

		lock (lockObj)
		{
			currentTask = new Thread(new ThreadStart(() =>
			{
				lock (lockObj)
				{
					try
					{
						action();
					}
					catch { }
				}
			}))
			{
				IsBackground = isBackground,
				Priority = threadPriority,
				Name = $"{action.Method} {action.Target} [One Way Task]"
			};

			try
			{
				currentTask.Start();
			}
			catch { }
		}
	}
}