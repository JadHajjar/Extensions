using System;
using System.Collections.Generic;
using System.Threading;

namespace Extensions
{
	public class Dispenser<In, Out> : DisableIdentifier
	{
		private readonly List<In> actions = new List<In>();
		private readonly List<Thread> runningThreads = new List<Thread>();
		private int processingPower = 1;
		private Func<In, Out> converter;
		private readonly object lockObj = new object();

		public event EventHandler ActionsFinished;

		public event EventHandler<Out> Dispense;

		public int ProcessingPower { get => processingPower; set => processingPower = Math.Max(1, value); }

		public Dispenser(Func<In, Out> converter, int processingPower = 1) : base()
		{
			this.converter = converter;
			ProcessingPower = processingPower;
		}

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

		public void Run(In action)
		{
			lock (lockObj)
			{
				if (runningThreads.Count < ProcessingPower)
					start(action);
				else
					actions.Add(action);
			}
		}

		public void Run(IEnumerable<In> action)
		{
			foreach (var item in action)
				Run(item);
		}

		public void Add(In action)
		{
			lock (lockObj)
				actions.Add(action);
		}

		public void Add(IEnumerable<In> action)
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

		private void start(In action)
		{
			if (Disabled || action == null) return;

			if (actions.Count > 0)
				actions.Remove(action);
			Thread thread = null;

			thread = new Thread(new ThreadStart(() =>
			{
				try
				{
					var ret = converter(action);

					Dispense?.Invoke(this, ret);
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
			}))
			{ IsBackground = true, Name = $"Dispenser #{ID} Thread" };

			runningThreads.Add(thread);
			thread.Start();
		}
	}
}