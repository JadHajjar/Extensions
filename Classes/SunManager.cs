using Newtonsoft.Json;

using System;
using System.IO;
using System.Net;

namespace Extensions
{
	public static class SunManager
	{
		private static DateRange sunTime;
		private static bool loadingSunTime;

		public static DateRange SunTime
		{
			get
			{
				if (sunTime.Empty)
				{
					ISave.Load(out sunTime, "SunTime.tf", "Shared");

					if (sunTime.Empty)
					{
						if (!loadingSunTime)
						{
							loadingSunTime = true;
							new Action(LoadSunTime).RunInBackground();
						}

						return sunTime = new DateRange(DateTime.Today.AddHours(7), DateTime.Today.AddHours(19));
					}
				}
				else if (sunTime.Start.Date != DateTime.Today)
				{
					sunTime = new DateRange(DateTime.Today + (sunTime.Start - sunTime.Start.Date), DateTime.Today + (sunTime.End - sunTime.End.Date));
					new Action(LoadSunTime).RunInBackground();
				}

				return sunTime;
			}
		}

		private static void LoadSunTime()
		{
			MachineInfo location;
			using (var client = new WebClient())
			{
				using (var stream = client.OpenRead($"http://api.ipstack.com/{ConnectionHandler.IpAddress}?access_key=c91ac331daae5aa3a9cf5e11aa0e8766"))
				{
					using (var reader = new StreamReader(stream))
						location = JsonConvert.DeserializeObject<MachineInfo>(reader.ReadToEnd());
				}

				using (var stream = client.OpenRead($"https://api.sunrise-sunset.org/json?lat={location.latitude}&lng={location.longitude}"))
				{
					using (var reader = new StreamReader(stream))
					{
						var response = JsonConvert.DeserializeObject<SunsetResponse>(new StreamReader(stream).ReadToEnd());

						sunTime = new DateRange(DateTime.Parse(response.results.sunrise).ToLocalTime(), DateTime.Parse(response.results.sunset).ToLocalTime());

						ISave.Save(sunTime, "SunTime.tf", appName: "Shared");
					}
				}
			}

			loadingSunTime = false;
		}
	}
}