using Newtonsoft.Json;

using System;
using System.IO;
using System.Net;

namespace Extensions
{
	public static class SunManager
	{
		public static DateRange SunTime => CalculateSunsetSunrise(DateTime.Now.Month);

		// Constants for the approximation formula
		private const double DEG_TO_RAD = Math.PI / 180;
		private const double RAD_TO_DEG = 180 / Math.PI;
		private const double DAYS_PER_YEAR = 365.24;

		public static DateRange CalculateSunsetSunrise(int month)
		{
			// Get the timezone of the user
			var timezone = GetTimezone();

			// Approximate the number of days since the start of the year
			var daysSinceStartOfYear = ((month - 1) * 30.5) + 15;

			// Calculate the solar declination angle
			var solarDeclination = 0.396372 - (22.91327 * Math.Cos(DEG_TO_RAD * (360 / DAYS_PER_YEAR) * daysSinceStartOfYear))
									  + (4.02543 * Math.Sin(DEG_TO_RAD * (360 / DAYS_PER_YEAR) * daysSinceStartOfYear))
									  - (0.387205 * Math.Cos(DEG_TO_RAD * (2 * (360 / DAYS_PER_YEAR) * daysSinceStartOfYear)))
									  + (0.051967 * Math.Sin(DEG_TO_RAD * (2 * (360 / DAYS_PER_YEAR) * daysSinceStartOfYear)))
									  - (0.154527 * Math.Cos(DEG_TO_RAD * (3 * (360 / DAYS_PER_YEAR) * daysSinceStartOfYear)))
									  + (0.084798 * Math.Sin(DEG_TO_RAD * (3 * (360 / DAYS_PER_YEAR) * daysSinceStartOfYear)));

			// Calculate the hour angle
			var hourAngle = RAD_TO_DEG * Math.Acos(-Math.Tan(DEG_TO_RAD * 40) * Math.Tan(DEG_TO_RAD * solarDeclination));

			// Calculate the sunset and sunrise times in hours
			var sunsetHour = 12 - (hourAngle / 15) - timezone;
			var sunriseHour = 12 + (hourAngle / 15) - timezone;

			return new DateRange(DateTime.Today.AddHours(sunsetHour), DateTime.Today.AddHours(sunriseHour));
		}

		public static double GetTimezone()
		{
			var localTimeZone = TimeZoneInfo.Local;
			return localTimeZone.BaseUtcOffset.TotalHours;
		}
	}
}