using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
	public static partial class ExtensionClass
	{
		public enum DateFormat { DMY, MDY, TDMY }

		/// <summary>
		/// Converts the <see cref="TimeSpan"/> into a readable large <see cref="string"/>
		/// </summary>
		/// <returns>x years?, x months?, x days?, x hours? : 'Today'</returns>
		public static string ToReadableBigString(this TimeSpan TS)
		{
			try
			{
				var hours = TS.Hours;
				var days = TS.Days;
				var years = days / 365;
				days -= years * 365;
				var months = days / 30;
				days -= months * 30;
				if (TS.TotalDays >= 1)
				{
					days += hours > 0 ? 1 : 0;
				}

				if (days > 30)
				{ days -= 30; months++; }
				var Out = "";
				if (years > 0)
				{
					Out = (years > 0 ? years + " year" + (years != 1 ? "s" : "") + ", " : "") + (months > 0 ? months + " month" + (months != 1 ? "s" : "") + ", " : "");
				}
				else if (TS.TotalDays >= 1)
				{
					Out = (months > 0 ? months + " month" + (months != 1 ? "s" : "") + ", " : "") + (days > 0 ? days + " day" + (days != 1 ? "s" : "") + ", " : "");
				}
				else
				{
					Out = (days > 0 ? days + " day" + (days != 1 ? "s" : "") + ", " : "") + (hours > 0 ? hours + " hour" + (hours != 1 ? "s" : "") + ", " : "");
				}

				return Out.Substring(0, Out.Length - 2);
			}
			catch (Exception)
			{ return "Today"; }
		}

		/// <summary>
		/// Converts the <see cref="DateTime"/> to a readable long date
		/// </summary>
		/// <param name="AddYear">Adds the Year in the output</param>
		/// <returns>the 'xx'th of 'month' 'year'</returns>
		public static string ToReadableString(this DateTime DT, bool AddYear = true, DateFormat format = DateFormat.DMY, bool fullMonth = true)
		{
			if (LocaleHelper.CurrentCulture?.TwoLetterISOLanguageName != "en")
			{
				return DT.ToString("d");
			}

			var day = "th";
			var month = DT.ToString(fullMonth ? "MMMM" : "MMM");

			if (DT.Day < 4 || DT.Day > 20)
			{
				switch (DT.Day % 10)
				{
					case 1:
						day = "st";
						break;
					case 2:
						day = "nd";
						break;
					case 3:
						day = "rd";
						break;
				}
			}

			switch (format)
			{
				case DateFormat.DMY:
				default:
					return $"{DT.Day}{day} of {month}{(AddYear ? $" {DT.Year}" : "")}";

				case DateFormat.MDY:
					return $"{month} {DT.Day}{day}{(AddYear ? $" of {DT.Year}" : "")}";

				case DateFormat.TDMY:
					return $"the {DT.Day}{day} of {month}{(AddYear ? $" {DT.Year}" : "")}";
			}
		}

		/// <summary>
		/// Converts the <see cref="TimeSpan"/> into a readable <see cref="string"/>
		/// </summary>
		/// <returns>x years?, x months?, x days?, x hours?, x minutes?, x seconds? : '0 seconds'</returns>
		public static string ToReadableString(this TimeSpan TS, bool shorter = false, bool longWords = true)
		{
			try
			{
				if (TS.Ticks != 0)
				{
					var sb = new List<string>();
					var days = TS.Days;
					var years = days / 365;
					days -= years * 365;
					var months = days / 30;
					days -= months * 30;
					var large = years > 0 || months > 1;

					if (large)
					{
						if (years != 0)
						{
							sb.Add(LocaleHelper.GetGlobalText("{0} year").FormatPlural(years));
						}

						if (months != 0)
						{
							sb.Add(LocaleHelper.GetGlobalText("{0} month").FormatPlural(months));
						}

						if (days != 0)
						{
							sb.Add(LocaleHelper.GetGlobalText("{0} day").FormatPlural(days));
						}
					}
					else
					{
						if (TS.Days != 0)
						{
							sb.Add(LocaleHelper.GetGlobalText(longWords ? "{0} day" : "{0}d").FormatPlural(TS.Days));
						}

						if (TS.Hours != 0)
						{
							sb.Add(LocaleHelper.GetGlobalText(longWords ? "{0} hour" : "{0}h").FormatPlural(TS.Hours));
						}

						if (TS.Minutes != 0)
						{
							sb.Add(LocaleHelper.GetGlobalText(longWords ? "{0} minute" : "{0}m").FormatPlural(TS.Minutes));
						}

						if ((TS.Seconds != 0 || TS.TotalSeconds < 1) && (!shorter || TS.TotalSeconds < 60))
						{
							sb.Add(LocaleHelper.GetGlobalText(longWords ? "{0} second" : "{0}s").FormatPlural((TS.TotalSeconds < 1 ? Math.Round(TS.TotalSeconds, 2) : TS.Seconds)));
						}
					}

					if (shorter)
					{
						return string.Join(", ", sb.Take(2));
					}

					return string.Join(", ", sb);
				}
			}
			catch
			{ }

			return LocaleHelper.GetGlobalText(longWords ? "{0} second" : "{0}s").FormatPlural(0);
		}

		/// <summary>
		/// Converts the <see cref="TimeSpan"/> into a readable <see cref="string"/>
		/// </summary>
		/// <returns>x years?, x months?, x days?, x hours?, x minutes?, x seconds? : '0 seconds'</returns>
		public static string ToTinyString(this TimeSpan TS)
		{
			try
			{
				var sb = new List<string>();

				if (TS.Hours != 0)
				{
					sb.Add(TS.Hours + "h");
				}

				if (TS.Minutes != 0)
				{
					sb.Add(TS.Minutes + "m");
				}

				if (TS.TotalSeconds != 0)
				{
					sb.Add(Math.Round(TS.TotalSeconds % 60, 2) + "s");
				}

				return sb.Count == 0 ? "0s" : string.Join(", ", sb);
			}
			catch
			{ return "0s"; }
		}

		public static string ToRelatedString(this DateTime dt, bool shorter = false, bool longWords = true, bool utc = false)
		{
			var ts = new TimeSpan(Math.Abs(dt.Ticks - DateTime.Now.Ticks));
			var past = dt < (utc ? DateTime.UtcNow : DateTime.Now);
			var today = (utc ? DateTime.UtcNow : DateTime.Now).Date;

			if (ts.TotalHours < 5)
			{
				if (past)
				{
					return LocaleHelper.GetGlobalText("{0} ago").Format(ts.ToReadableString(shorter, longWords));
				}

				return LocaleHelper.GetGlobalText("in {0}").Format(ts.ToReadableString(shorter, longWords));
			}
			else if (dt.Date == today)
			{
				return LocaleHelper.GetGlobalText("Today at {0}").Format(dt.ToString("t"));
			}
			else if (dt.Date.AnyOf(today.AddDays(1), today.AddDays(-1)))
			{
				return LocaleHelper.GetGlobalText(past ? "Yesterday at {0}" : "Tomorrow at {0}").Format(dt.ToString("t"));
			}
			else if (ts.TotalDays < 7)
			{
				return LocaleHelper.GetGlobalText(past ? "Last {0} at {1}" : "Next {0} at {1}").Format(dt.ToString("dddd"), dt.ToString("t"));
			}

			var days = ts.Days;
			var years = days / 365;
			days -= years * 365;
			var months = days / 30;

			if (years > 0)
			{
				return dt.ToReadableString(true, DateFormat.DMY);
			}

			if (months > 0)
			{
				if (shorter)
				return LocaleHelper.GetGlobalText(past ? "{0} month ago" : "in {0} month").FormatPlural(months);
				return LocaleHelper.GetGlobalText(past ? "{0} month ago on {1}" : "in {0} month on {1}").FormatPlural(months, dt.ToReadableString(false, DateFormat.MDY));
			}

			return LocaleHelper.GetGlobalText(past ? "{0} day ago" : "in {0} day").FormatPlural(days);
		}
	}
}