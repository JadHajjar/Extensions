using System;
using System.Threading;
using System.Threading.Tasks;

namespace Extensions;

public static partial class ExtensionClass
{
	/// <summary>
	/// Runs an <see cref="Action"/> in the background
	/// </summary>
	/// <param name="Priority">The <see cref="ThreadPriority"/> of the background <see cref="Thread"/></param>
	[Obsolete("Use BackgroundAction instead", true)]
	public static void RunInBackground(this Action action, ThreadPriority Priority = ThreadPriority.Normal)
	{
		RunInBackground(action);
	}

	/// <summary>
	/// Runs an <see cref="Action"/> in the background
	/// </summary>
	[Obsolete("Use BackgroundAction instead", true)]
	public static void RunInBackground(this Action action)
	{
		ThreadPool.QueueUserWorkItem(w =>
		{
			try
			{
				action();
			}
			catch { }
		});
	}

	/// <summary>
	/// Runs an <see cref="Action"/> in the background after a delay
	/// </summary>
	/// <param name="delay"><see cref="Action"/> delay in milliseconds</param>
	/// <param name="runOnce">Option to run the <see cref="Action"/> once or repeating after each <paramref name="delay"/></param>
	[Obsolete("Use BackgroundAction instead", true)]
	public static void RunInBackground(this Action action, int delay, bool runOnce = true)
	{
		var timer = new System.Timers.Timer(Math.Max(1, delay)) { AutoReset = !runOnce, Enabled = true };
		timer.Elapsed += (s, e) =>
		{
			try
			{
				action();
			}
			catch { }

			if (runOnce)
			{
				timer.Dispose();
			}
		};
	}

	/// <summary>
	/// Loops an <see cref="Action"/> in the background until the <paramref name="condition"/> is met
	/// <param name="onEnd"><see cref="Action"/> to execute at the end</param>
	/// </summary>
	public static Thread TimerLoop(this Action action, Func<bool> condition, Action onEnd = null, ThreadPriority priority = ThreadPriority.Normal)
	{
		if (action == null)
		{
			throw new ArgumentNullException(nameof(action));
		}

		if (condition == null)
		{
			throw new ArgumentNullException(nameof(condition));
		}

		var T = new Thread(() =>
		{
			while (condition())
			{
				action();
			}

			onEnd?.Invoke();
		})
		{
			IsBackground = true,
			Priority = priority,
			Name = $"{action.Method} {action.Target} [Background]"
		};

		T.Start();

		return T;
	}

#if !NET47
	public static bool WaitUntil<T>(this T elem, Func<T, bool> predicate)
	{
		bool result;
		try
		{
			while (true)
			{
				var flag = predicate(elem);
				if (flag)
				{
					break;
				}
				Thread.Sleep(1);
			}
			result = true;
		}
		catch
		{
			result = true;
		}
		return result;
	}
#else
	public static async Task<bool> WaitUntil<T>(this T elem, Func<T, bool> predicate)
	{
		while (!predicate(elem))
		{
			await Task.Delay(1);
		}

		return true;
	}
#endif
}