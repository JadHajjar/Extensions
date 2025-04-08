using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Timer = System.Timers.Timer;

namespace Extensions;

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

	private static readonly string[] _dnsIps = new[]
	{
		"1.1.1.1",
		"8.8.8.8",
		"8.8.4.4",
		"208.67.222.222",
		"208.67.220.220"
	};

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

	public static string IpAddress => ipAddress ??= new WebClient().DownloadString("http://icanhazip.com").Trim();

	public static bool AssumeInternetConnectivity { get; set; }

	public static void Start()
	{
		Start(_dnsIps[0], 15000);
	}

	public static void Start(string host, double timer)
	{
		ConnectionHandler.host = host;
		ConnectionHandler.timer = timer;

		checkTimer?.Dispose();

		checkTimer = new Timer(timer) { AutoReset = false };

		checkTimer.Elapsed += (s, e) => GetConnectionState();

#if SIMPLE
		Task.Run(GetConnectionState);
#else
		new BackgroundAction("Checking Internet connection", GetConnectionState) { CanNotBeStopped = true }.Run();
#endif
	}

	private static void GetConnectionState()
	{
		if (AssumeInternetConnectivity || CrossIO.CurrentPlatform == Platform.MacOSX)
		{
			State = ConnectionState.Connected;
			return;
		}

		try
		{
			if (State == ConnectionState.Disconnected)
			{
				State = ConnectionState.Connecting;
			}

			const int timeout = 4000;

			if (new Ping().Send(host, timeout, new byte[32]).Status == IPStatus.Success)
			{
				State = ConnectionState.Connected;

				Connected?.Invoke(state);
				Connected = null;
			}
			else
			{
				State = ConnectionState.Disconnected;
			}
		}
		catch
		{
			State = ConnectionState.Disconnected;
		}

		if (State == ConnectionState.Disconnected)
		{
			host = _dnsIps.Next(host, true);
		}

		checkTimer.Interval = IsConnected ? timer : 2500;
		checkTimer.Start();
	}

	public static bool CheckConnection()
	{
		checkTimer.Stop();

		GetConnectionState();

		return IsConnected;
	}

	public static bool WhenConnected(ExtensionClass.action action)
	{
		if (State == ConnectionState.Connected)
		{
			action();

			return true;
		}

		Connected += (s) => action();

		return false;
	}

#if NET47
	public static async Task<bool> WhenConnected(Func<Task> task)
	{
		if (State == ConnectionState.Connected)
		{
			await task();

			return true;
		}

		Connected += async (s) => await task();

		return State == ConnectionState.Connected;
	}
#endif
}