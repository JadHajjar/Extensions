using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Extensions
{
	public class Factory : DisableIdentifier
	{
		public event EventHandler ActionsFinished;

		public int ProcessingPower
		{
			get => processingPower;
			set => processingPower = Math.Max(1, value);
		}

		public Factory()
		{
		}

		public Factory(int processingPower)
		{
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

		public void Run(ExtensionClass.action action)
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

		public void Run(IEnumerable<ExtensionClass.action> action)
		{
			foreach (ExtensionClass.action action2 in action)
			{
				Run(action2);
			}
		}

		public void Add(ExtensionClass.action action)
		{
			object obj = lockObj;
			lock (obj)
			{
				actions.Add(action);
			}
		}

		public void Add(IEnumerable<ExtensionClass.action> action)
		{
			object obj = lockObj;
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
#if NET47
			return await this.WaitUntil((Factory x) => finished);
#else
			return this.WaitUntil((Factory x) => finished);
#endif
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

		private void start(ExtensionClass.action action)
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
						try
						{
							action();
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
					}
					catch (ThreadInterruptedException)
					{
					}
					catch (ThreadAbortException)
					{
					}
				})
				{
					IsBackground = true,
					Name = string.Format("Factory #{0} Thread", base.ID)
				};
				runningThreads.Add(thread);
				thread.Start();
			}
		}

		private readonly List<ExtensionClass.action> actions = new List<ExtensionClass.action>();

		private readonly List<Thread> runningThreads = new List<Thread>();

		private int processingPower = 1;

		private readonly object lockObj = new object();
	}
}