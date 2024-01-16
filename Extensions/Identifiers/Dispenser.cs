using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Extensions;

public class Dispenser<In, Out> : DisableIdentifier
{
	public event EventHandler ActionsFinished;
	public event EventHandler<Out> Dispense;

	public int ProcessingPower
	{
		get => processingPower;
		set => processingPower = Math.Max(1, value);
	}

	public Dispenser(Func<In, Out> converter, int processingPower = 1)
	{
		this.converter = converter;
		ProcessingPower = processingPower;
	}

	public void Run()
	{
		for (; ; )
		{
			var obj = lockObj;
			lock (obj)
			{
				var flag2 = runningThreads.Count < ProcessingPower && actions.Count > 0;
				if (!flag2)
				{
					break;
				}

				start(actions[0]);
			}
		}
	}

	public void Run(In action)
	{
		var obj = lockObj;
		lock (obj)
		{
			var flag2 = runningThreads.Count < ProcessingPower;
			if (flag2)
			{
				start(action);
			}
			else
			{
				actions.Add(action);
			}
		}
	}

	public void Run(IEnumerable<In> action)
	{
		foreach (var action2 in action)
		{
			Run(action2);
		}
	}

	public void Add(In action)
	{
		var obj = lockObj;
		lock (obj)
		{
			actions.Add(action);
		}
	}

	public void Add(IEnumerable<In> action)
	{
		var obj = lockObj;
		lock (obj)
		{
			actions.AddRange(action);
		}
	}

#if NET47
	public async Task<bool> Wait()
#else
	public bool Wait()
#endif
	{
		var obj = lockObj;
		lock (obj)
		{
			var flag2 = actions.Count == 0 && runningThreads.Count == 0;
			if (flag2)
			{
				return true;
			}
		}

		var finished = false;
		ActionsFinished += delegate (object s, EventArgs e)
		{
			finished = true;
		};
#if NET47
		return await this.WaitUntil((Dispenser<In, Out> x) => finished);
#else
		return this.WaitUntil((Dispenser<In, Out> x) => finished);
#endif
	}

	public void Clear()
	{
		var obj = lockObj;
		lock (obj)
		{
			foreach (var thread in runningThreads)
			{
				try
				{
					thread?.Interrupt();
					thread?.Abort();
				}
				catch
				{
				}
			}

			actions.Clear();
			runningThreads.Clear();
		}
	}

	public override void Enable()
	{
		base.Enable();
		Run();
	}

	private void start(In action)
	{
		var flag = base.Disabled || action == null;
		if (!flag)
		{
			var flag2 = actions.Count > 0;
			if (flag2)
			{
				actions.Remove(action);
			}

			Thread thread = null;
			thread = new Thread(delegate ()
			{
				try
				{
					var e = converter(action);
					Dispense?.Invoke(this, e);
				}
				catch
				{
				}

				var obj = lockObj;
				lock (obj)
				{
					runningThreads.Remove(thread);
					var flag4 = actions.Count > 0;
					if (flag4)
					{
						start(actions[0]);
					}
					else
					{
						var flag5 = runningThreads.Count == 0;
						if (flag5)
						{
							ActionsFinished?.Invoke(this, EventArgs.Empty);
						}
					}
				}
			})
			{
				IsBackground = true,
				Name = string.Format("Dispenser #{0} Thread", base.ID)
			};
			runningThreads.Add(thread);
			thread.Start();
		}
	}

	private readonly List<In> actions = [];

	private readonly List<Thread> runningThreads = [];

	private int processingPower = 1;

	private readonly Func<In, Out> converter;

	private readonly object lockObj = new();
}