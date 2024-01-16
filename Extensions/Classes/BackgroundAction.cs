using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Extensions;

public class BackgroundAction
{
	private static readonly List<BackgroundAction> _runningLoads = [];

	public static IEnumerable<BackgroundAction> RunningLoads
	{
		get
		{
			lock (_runningLoads)
			{
				foreach (var item in _runningLoads)
				{
					yield return item;
				}
			}
		}
	}

	public static event Action<BackgroundAction, Exception> BackgroundTaskError;
	public static event Action<BackgroundAction> LoadingStarted;
	public static event Action<BackgroundAction> LoadingEnded;
	public event Action<BackgroundAction> Started;
	public event Action<BackgroundAction> Ended;
	public event Action<BackgroundAction, Exception> Error;

	public delegate void Method();

	public Method Action { get; }
	public bool IgnoreErrors { get; }
	public string Name { get; }
	public Form Form { get; }
	public Thread Thread { get; private set; }
	public bool RequiresCompletion { get; set; }
	public bool CanNotBeStopped { get; set; }
	public bool IsRunning => Thread != null;

	public BackgroundAction(Method action, bool ignoreErrors = false)
	{
		Action = action;
		IgnoreErrors = ignoreErrors;
	}

	public BackgroundAction(string name, Method action, bool ignoreErrors = false)
	{
		Name = name;
		Action = action;
		IgnoreErrors = ignoreErrors;
	}

	public BackgroundAction(string name, Form form, Method action, bool ignoreErrors = false)
	{
		Form = form;
		Name = name;
		Action = action;
		IgnoreErrors = ignoreErrors;
	}

	public Thread Run()
	{
		if (Thread != null)
		{
			throw new Exception("Action is already running");
		}

		Thread = new Thread(new ThreadStart(() =>
		{
			lock (_runningLoads)
			{
				_runningLoads.Add(this);
			}

			restart:
			try
			{
				Started?.Invoke(this);
				LoadingStarted?.Invoke(this);

				Action();
			}
			catch (ThreadAbortException) { }
			catch (ThreadInterruptedException) { }
			catch (Exception ex)
			{
				if (IgnoreErrors)
				{
					return;
				}

				if (Error == null)
				{
					BackgroundTaskError?.Invoke(this, ex);
				}
				else
				{
					Error(this, ex);
				}

				if (CanNotBeStopped)
				{
					goto restart;
				}
			}
			finally
			{
				Thread = null;

				lock (_runningLoads)
				{
					_runningLoads.Remove(this);
				}

				Ended?.Invoke(this);
				LoadingEnded?.Invoke(this);
			}
		}))
		{ IsBackground = true };

		Thread.Start();

		return Thread;
	}

	public void RunIn(int milliseconds)
	{
		var timer = new System.Timers.Timer(milliseconds) { AutoReset = false };

		timer.Elapsed += (s, e) =>
		{
			Run();
			timer.Dispose();
		};
		timer.Start();
	}

	public System.Timers.Timer RunEvery(int milliseconds, bool runNow = false)
	{
		var timer = new System.Timers.Timer(milliseconds);

		timer.Elapsed += (s, e) => Run();
		timer.Start();

		if (runNow)
		{
			Run();
		}

		return timer;
	}

	public void Stop()
	{
		if (!IsRunning)
		{
			return;
		}

		Thread?.Interrupt();
		Thread?.Abort();
		Thread = null;

		lock (_runningLoads)
		{
			_runningLoads.Remove(this);
		}

		Ended?.Invoke(this);
		LoadingEnded?.Invoke(this);
	}

	public override string ToString()
	{
		return $"[{Form?.Text ?? "Other Actions"}] {Name}";
	}
}
