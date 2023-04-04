using System;
using System.Net;
using System.Net.NetworkInformation;

using Timer = System.Timers.Timer;

namespace Extensions
{
	public enum ConnectionState { Connected, Disconnected, Connecting }

	public delegate void ConnectionEventHandler(ConnectionState newState);

	public static class ConnectionHandler
	{
		public static event ConnectionEventHandler ConnectionChanged;

		public static event ConnectionEventHandler Connected;

		private static Timer checkTimer;

		public static bool IsConnected => state == ConnectionState.Connected;

		private static ConnectionState state = ConnectionState.Connecting;
		private static string ipAddress;
		private static string host;
		private static double timer;

		public static ConnectionState State
		{
			get => state;
			private set
			{
				if (value != state)
				{
					state = value;
					ConnectionChanged?.Invoke(value);
				}
			}
		}

		public static string IpAddress => ipAddress ?? (ipAddress = new WebClient().DownloadString("http://icanhazip.com").Trim());

		public static void Start() => Start("8.8.8.8", 15000);

		public static void Start(string host, double timer)
		{
			ConnectionHandler.host = host;
			ConnectionHandler.timer = timer;
			checkTimer = new Timer(timer) { AutoReset = false };

			checkTimer.Elapsed += (s, e) => getConnectionState();

			new BackgroundAction("Checking Internet connection", getConnectionState) { CanNotBeStopped = true }.Run();
		}

		private static void getConnectionState()
		{
			if (ISave.CurrentPlatform == Platform.MacOSX)
			{
				State = ConnectionState.Connected;
				return;
			}

			try
			{
				if (State == ConnectionState.Disconnected)
					State = ConnectionState.Connecting;

				const int timeout = 4000;

				if (new Ping().Send(host, timeout, new byte[32]).Status == IPStatus.Success)
				{
					State = ConnectionState.Connected;

					Connected?.Invoke(state);
					Connected = null;
				}
				else
					State = ConnectionState.Disconnected;
			}
			catch
			{
				State = ConnectionState.Disconnected;
			}

			checkTimer.Interval = IsConnected ? timer : 2500;
			checkTimer.Start();
		}

		public static bool WhenConnected(ExtensionClass.action action)
		{
			if (State == ConnectionState.Connected)
				action();
			else
				Connected += (s) => action();

			return State == ConnectionState.Connected;
		}
	}
}