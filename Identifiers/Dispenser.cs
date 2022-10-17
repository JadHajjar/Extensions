using System;
using System.Collections.Generic;
using System.Threading;

namespace Extensions
{
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
				object obj = lockObj;
				lock (obj)
				{
					bool flag2 = runningThreads.Count < ProcessingPower && actions.Count > 0;
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
			object obj = lockObj;
			lock (obj)
			{
				bool flag2 = runningThreads.Count < ProcessingPower;
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
			foreach (In action2 in action)
			{
				Run(action2);
			}
		}

		public void Add(In action)
		{
			object obj = lockObj;
			lock (obj)
			{
				actions.Add(action);
			}
		}

		public void Add(IEnumerable<In> action)
		{
			object obj = lockObj;
			lock (obj)
			{
				actions.AddRange(action);
			}
		}

		public bool Wait()
		{
			object obj = lockObj;
			lock (obj)
			{
				bool flag2 = actions.Count == 0 && runningThreads.Count == 0;
				if (flag2)
				{
					return true;
				}
			}
			bool finished = false;
			ActionsFinished += delegate (object s, EventArgs e)
			{
				finished = true;
			};
			return this.WaitUntil((Dispenser<In, Out> x) => finished);
		}

		public void Clear()
		{
			object obj = lockObj;
			lock (obj)
			{
				foreach (Thread thread in runningThreads)
				{
					try
					{
						if (thread != null)
						{
							thread.Interrupt();
						}
						if (thread != null)
						{
							thread.Abort();
						}
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
			bool flag = base.Disabled || action == null;
			if (!flag)
			{
				bool flag2 = actions.Count > 0;
				if (flag2)
				{
					actions.Remove(action);
				}
				Thread thread = null;
				thread = new Thread(delegate ()
				{
					try
					{
						Out e = converter(action);
						EventHandler<Out> dispense = Dispense;
						if (dispense != null)
						{
							dispense(this, e);
						}
					}
					catch
					{
					}
					object obj = lockObj;
					lock (obj)
					{
						runningThreads.Remove(thread);
						bool flag4 = actions.Count > 0;
						if (flag4)
						{
							start(actions[0]);
						}
						else
						{
							bool flag5 = runningThreads.Count == 0;
							if (flag5)
							{
								EventHandler actionsFinished = ActionsFinished;
								if (actionsFinished != null)
								{
									actionsFinished(this, EventArgs.Empty);
								}
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

		private readonly List<In> actions = new List<In>();

		private readonly List<Thread> runningThreads = new List<Thread>();

		private int processingPower = 1;

		private readonly Func<In, Out> converter;

		private readonly object lockObj = new object();
	}
}