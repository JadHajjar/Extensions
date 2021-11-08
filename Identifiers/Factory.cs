using System;
using System.Collections.Generic;
using System.Threading;

namespace Extensions
{
	public class Factory : DisableIdentifier
	{
		private readonly List<ExtensionClass.action> actions = new List<ExtensionClass.action>();
		private readonly List<Thread> runningThreads = new List<Thread>();
		private int processingPower = 1;
		private readonly object lockObj = new object();

		public event EventHandler ActionsFinished;

		public int ProcessingPower { get => processingPower; set => processingPower = Math.Max(1, value); }

		public Factory() : base()
		{ }

		public Factory(int processingPower) : base() => ProcessingPower = processingPower;

		public void Run()
		{
			while (true)
				lock (lockObj)
				{
					if (runningThreads.Count < ProcessingPower && actions.Count > 0)
						start(actions[0]);
					else break;
				}
		}

		public void Run(ExtensionClass.action action)
		{
			lock (lockObj)
			{
				if (runningThreads.Count < ProcessingPower)
					start(action);
				else
					actions.Add(action);
			}
		}

		public void Run(IEnumerable<ExtensionClass.action> action)
		{
			foreach (var item in action)
				Run(item);
		}

		public void Add(ExtensionClass.action action)
		{
			lock (lockObj)
				actions.Add(action);
		}

		public void Add(IEnumerable<ExtensionClass.action> action)
		{
			lock (lockObj)
				actions.AddRange(action);
		}

		public bool Wait()
		{
			var finished = false;

			ActionsFinished += (s, e) => finished = true;

			return this.WaitUntil(x => finished).Result;
		}

		public void Clear()
		{
			lock (lockObj)
			{
				foreach (var x in runningThreads)
				{
					try
					{
						x?.Interrupt();
						x?.Abort();
					}
					catch { }
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

		private void start(ExtensionClass.action action)
		{
			if (Disabled || action == null) return;

			if (actions.Count > 0)
				actions.Remove(action);
			Thread thread = null;

			thread = new Thread(new ThreadStart(() =>
			{
				try
				{
					try
					{
						action();
					}
					catch { }

					lock (lockObj)
					{
						runningThreads.Remove(thread);

						if (actions.Count > 0)
							start(actions[0]);
						else if (runningThreads.Count == 0)
							ActionsFinished?.Invoke(this, EventArgs.Empty);
					}
				}
				catch (ThreadInterruptedException) { }
				catch (ThreadAbortException) { }
			}))
			{ IsBackground = true, Name = $"Factory #{ID} Thread" };

			runningThreads.Add(thread);
			thread.Start();
		}
	}
}