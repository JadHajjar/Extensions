using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
	public static partial class ExtensionClass
	{
		public enum DateFormat { DMY, MDY, TDMY }

		public static string ToReadableBigString(this TimeSpan TS)
		{
			string result;
			try
			{
				var hours = TS.Hours;
				var num = TS.Days;
				var num2 = num / 365;
				num -= num2 * 365;
				var num3 = num / 30;
				num -= num3 * 30;
				var flag = TS.TotalDays >= 1.0;
				if (flag)
				{
					num += ((hours > 0) ? 1 : 0);
				}
				var flag2 = num > 30;
				if (flag2)
				{
					num -= 30;
					num3++;
				}
				var flag3 = num2 > 0;
				string text;
				if (flag3)
				{
					text = ((num2 > 0) ? (num2.ToString() + " year" + ((num2 != 1) ? "s" : "") + ", ") : "") + ((num3 > 0) ? (num3.ToString() + " month" + ((num3 != 1) ? "s" : "") + ", ") : "");
				}
				else
				{
					var flag4 = TS.TotalDays >= 1.0;
					if (flag4)
					{
						text = ((num3 > 0) ? (num3.ToString() + " month" + ((num3 != 1) ? "s" : "") + ", ") : "") + ((num > 0) ? (num.ToString() + " day" + ((num != 1) ? "s" : "") + ", ") : "");
					}
					else
					{
						text = ((num > 0) ? (num.ToString() + " day" + ((num != 1) ? "s" : "") + ", ") : "") + ((hours > 0) ? (hours.ToString() + " hour" + ((hours != 1) ? "s" : "") + ", ") : "");
					}
				}
				result = text.Substring(0, text.Length - 2);
			}
			catch (Exception)
			{
				result = "Today";
			}
			return result;
		}

		public static string ToReadableString(this DateTime DT, bool AddYear = true, ExtensionClass.DateFormat format = ExtensionClass.DateFormat.DMY, bool fullMonth = true)
		{
			var text = "th";
			var text2 = DT.ToString(fullMonth ? "MMMM" : "MMM");
			var flag = DT.Day < 4 || DT.Day > 20;
			if (flag)
			{
				switch (DT.Day % 10)
				{
					case 1:
						text = "st";
						break;
					case 2:
						text = "nd";
						break;
					case 3:
						text = "rd";
						break;
				}
			}
			string result;
			switch (format)
			{
				default:
					result = string.Format("{0}{1} of {2}{3}", new object[]
					{
					DT.Day,
					text,
					text2,
					AddYear ? string.Format(" {0}", DT.Year) : ""
					});
					break;
				case ExtensionClass.DateFormat.MDY:
					result = string.Format("{0} {1}{2}{3}", new object[]
					{
					text2,
					DT.Day,
					text,
					AddYear ? string.Format(" of {0}", DT.Year) : ""
					});
					break;
				case ExtensionClass.DateFormat.TDMY:
					result = string.Format("the {0}{1} of {2}{3}", new object[]
					{
					DT.Day,
					text,
					text2,
					AddYear ? string.Format(" {0}", DT.Year) : ""
					});
					break;
			}
			return result;
		}

		public static string ToReadableString(this TimeSpan TS, bool shorter = false, bool longWords = true)
		{
			try
			{
				var flag = TS.Ticks != 0L;
				if (flag)
				{
					var list = new List<string>();
					var num = TS.Days;
					var num2 = num / 365;
					num -= num2 * 365;
					var num3 = num / 30;
					num -= num3 * 30;
					var flag2 = num2 > 0 || num3 > 1;
					var flag3 = flag2;
					if (flag3)
					{
						var flag4 = num2 != 0;
						if (flag4)
						{
							list.Add(num2.ToString() + " year".Plural(num2, "s"));
						}
						var flag5 = num3 != 0;
						if (flag5)
						{
							list.Add(num3.ToString() + " month".Plural(num3, "s"));
						}
						var flag6 = num != 0;
						if (flag6)
						{
							list.Add(num.ToString() + " day".Plural(num, "s"));
						}
					}
					else
					{
						var flag7 = TS.Days != 0;
						if (flag7)
						{
							list.Add(TS.Days.ToString() + ((!longWords) ? "d" : " day".Plural(TS.Days, "s")));
						}
						var flag8 = TS.Hours != 0;
						if (flag8)
						{
							list.Add(TS.Hours.ToString() + ((!longWords) ? "h" : " hour".Plural(TS.Hours, "s")));
						}
						var flag9 = TS.Minutes != 0;
						if (flag9)
						{
							list.Add(TS.Minutes.ToString() + ((!longWords) ? "m" : " minute".Plural(TS.Minutes, "s")));
						}
						var flag10 = (TS.Seconds != 0 || TS.TotalSeconds < 1.0) && (!shorter || TS.TotalSeconds < 60.0);
						if (flag10)
						{
							list.Add(((TS.TotalSeconds < 1.0) ? Math.Round(TS.TotalSeconds, 2) : TS.Seconds).ToString() + ((!longWords) ? "s" : " second".Plural(TS.Seconds, "s")));
						}
					}
					if (shorter)
					{
						return string.Join(", ", list.Take(2));
					}
					return string.Join(", ", list);
				}
			}
			catch
			{
			}
			return (!longWords) ? "0s" : "0 seconds";
		}

		public static string ToTinyString(this TimeSpan TS)
		{
			string result;
			try
			{
				var list = new List<string>();
				var flag = TS.Hours != 0;
				if (flag)
				{
					list.Add(TS.Hours.ToString() + "h");
				}
				var flag2 = TS.Minutes != 0;
				if (flag2)
				{
					list.Add(TS.Minutes.ToString() + "m");
				}
				var flag3 = TS.TotalSeconds != 0.0;
				if (flag3)
				{
					list.Add(Math.Round(TS.TotalSeconds % 60.0, 2).ToString() + "s");
				}
				result = ((list.Count == 0) ? "0s" : string.Join(", ", list));
			}
			catch
			{
				result = "0s";
			}
			return result;
		}

		public static string ToRelatedString(this DateTime dt, bool shorter = false, bool longWords = true, bool utc = false)
		{
			var t = utc ? DateTime.UtcNow : DateTime.Now;
			var ts = new TimeSpan(Math.Abs(dt.Ticks - t.Ticks));
			var flag = dt < t;
			var flag2 = ts.TotalHours < 5.0;
			string result;
			if (flag2)
			{
				var flag3 = flag;
				if (flag3)
				{
					result = ts.ToReadableString(shorter, longWords) + " ago";
				}
				else
				{
					result = "in " + ts.ToReadableString(shorter, longWords);
				}
			}
			else
			{
				var flag4 = dt.Date == DateTime.Today;
				if (flag4)
				{
					result = string.Format("Today at {0:h:mm tt}{1}", dt, utc.If(" UTC"));
				}
				else
				{
					var flag5 = dt.Date.AnyOf(new DateTime[]
					{
						DateTime.Today.AddDays(1.0),
						DateTime.Today.AddDays(-1.0)
					});
					if (flag5)
					{
						result = string.Format("{0} at {1:h:mm tt}{2}", flag ? "Yesterday" : "Tomorrow", dt, utc.If(" UTC"));
					}
					else
					{
						var flag6 = ts.TotalDays < 7.0;
						if (flag6)
						{
							result = string.Format("{0} {1}", flag ? "last" : "next", dt.DayOfWeek) + shorter.If("", string.Format(" at {0:h:mm tt}{1}", dt, utc.If(" UTC")));
						}
						else
						{
							var num = ts.Days;
							var num2 = num / 365;
							num -= num2 * 365;
							var num3 = num / 30;
							var flag7 = num2 > 0;
							if (flag7)
							{
								result = (flag ? string.Format("{0} years ago", num2) : string.Format("in {0} years", num2)) + " on " + dt.ToReadableString(true, ExtensionClass.DateFormat.TDMY, true);
							}
							else
							{
								var flag8 = num3 > 0;
								if (flag8)
								{
									result = (flag ? string.Format("{0} months ago", num3) : string.Format("in {0} months", num3)) + " on " + dt.ToReadableString(false, ExtensionClass.DateFormat.TDMY, true);
								}
								else
								{
									result = (flag ? string.Format("{0} days ago", num) : string.Format("in {0} days", num)) + " on " + dt.ToReadableString(false, ExtensionClass.DateFormat.TDMY, true);
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}