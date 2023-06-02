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
				var years = days / 365; days -= years * 365;
				var months = days / 30; days -= months * 30;
				if (TS.TotalDays >= 1) days += hours > 0 ? 1 : 0;
				if (days > 30) { days -= 30; months++; }
				var Out = "";
				if (years > 0)
					Out = (years > 0 ? years + " year" + (years != 1 ? "s" : "") + ", " : "") + (months > 0 ? months + " month" + (months != 1 ? "s" : "") + ", " : "");
				else if (TS.TotalDays >= 1)
					Out = (months > 0 ? months + " month" + (months != 1 ? "s" : "") + ", " : "") + (days > 0 ? days + " day" + (days != 1 ? "s" : "") + ", " : "");
				else
					Out = (days > 0 ? days + " day" + (days != 1 ? "s" : "") + ", " : "") + (hours > 0 ? hours + " hour" + (hours != 1 ? "s" : "") + ", " : "");
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
				return DT.ToString("d");

			var day = "th";
			var month = DT.ToString(fullMonth ? "MMMM" : "MMM");

			if (DT.Day < 4 || DT.Day > 20)
				switch (DT.Day % 10)
				{
					case 1: day = "st"; break;
					case 2: day = "nd"; break;
					case 3: day = "rd"; break;
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
					var years = days / 365; days -= years * 365;
					var months = days / 30; days -= months * 30;
					var large = years > 0 || months > 1;

					if (large)
					{
						if (years != 0)
							sb.Add(years + " year".Plural(years));
						if (months != 0)
							sb.Add(months + " month".Plural(months));
						if (days != 0)
							sb.Add(days + " day".Plural(days));
					}
					else
					{
						if (TS.Days != 0)
							sb.Add(TS.Days + (!longWords ? "d" : " day".Plural(TS.Days)));
						if (TS.Hours != 0)
							sb.Add(TS.Hours + (!longWords ? "h" : " hour".Plural(TS.Hours)));
						if (TS.Minutes != 0)
							sb.Add(TS.Minutes + (!longWords ? "m" : " minute".Plural(TS.Minutes)));
						if ((TS.Seconds != 0 || TS.TotalSeconds < 1) && (!shorter || TS.TotalSeconds < 60))
							sb.Add((TS.TotalSeconds < 1 ? Math.Round(TS.TotalSeconds, 2) : TS.Seconds) + (!longWords ? "s" : " second".Plural(TS.Seconds)));
					}

					if (shorter) return string.Join(", ", sb.Take(2));

					return string.Join(", ", sb);
				}
			}
			catch
			{ }

			return !longWords ? "0s" : "0 seconds";
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
					sb.Add(TS.Hours + "h");
				if (TS.Minutes != 0)
					sb.Add(TS.Minutes + "m");
				if (TS.TotalSeconds != 0)
					sb.Add(Math.Round(TS.TotalSeconds % 60, 2) + "s");

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
					return ts.ToReadableString(shorter, longWords) + " ago";
				return "in " + ts.ToReadableString(shorter, longWords);
			}
			else if (dt.Date == today)
			{
				return $"Today at {dt:h:mm tt}";
			}
			else if (dt.Date.AnyOf(today.AddDays(1), today.AddDays(-1)))
			{
				return (past ? "Yesterday at " : "Tomorrow at ") + dt.ToString("h:mm tt");
			}
			else if (ts.TotalDays < 7)
			{
				return $"{(past ? "Last " : "Next ")}{dt:dddd} at {dt:h:mm tt}";
			}

			var days = ts.Days;
			var years = days / 365;
			days -= years * 365;
			var months = days / 30;

			if (years > 0)
				return dt.ToReadableString(true, DateFormat.DMY);
			//return (past ? $"{years} years ago" : $"in {years} years") + $" on {dt.ToReadableString(true, DateFormat.TDMY)}";

			if (months > 0)
				return (past ? $"{months} months ago" : $"in {months} months") + $" on {dt.ToReadableString(false, DateFormat.MDY)}";

			return (past ? $"{days} days ago" : $"in {days} days");
		}
	}
}