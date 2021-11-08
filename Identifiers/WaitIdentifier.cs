using System;

namespace Extensions
{
	public class WaitIdentifier : DisableIdentifier, IDisposable
	{
		private static ulong idList = 0;
		private ulong currentTicket = 0;
		private readonly object lockObj = new object();
		private System.Timers.Timer timer;
		private bool disposedValue;

		public WaitIdentifier() => ID = idList++;

		public delegate void MyAction();

		public bool Waiting { get; private set; } = false;

		public void Refresh()
		{
			lock (lockObj)
			{
				timer.Stop();
				timer.Start();
			}
		}

		public override void Disable()
		{
			lock (lockObj)
			{
				timer?.Dispose();
				Waiting = false;
				base.Disable();
			}
		}

		public void Cancel()
		{
			lock (lockObj)
			{
				timer?.Dispose();
				currentTicket++;
				Waiting = false;
			}
		}

		public void Wait(MyAction action, int milliseconds)
		{
			try
			{
				lock (lockObj)
				{
					var ticket = ++currentTicket;
					timer?.Dispose();
					if (milliseconds > 0)
					{
						Waiting = true;
						timer = new System.Timers.Timer(milliseconds) { AutoReset = false };
						timer.Elapsed += (s, e) =>
						{
							if (ticket == currentTicket)
							{
								action();
								timer.Dispose();
								Waiting = false;
							}
							else
								(s as System.Timers.Timer)?.Dispose();
						};
						timer?.Start();
					}
					else
					{
						action();
					}
				}
			}
			catch { }
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				currentTicket++;

				if (disposing)
					timer?.Dispose();

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}