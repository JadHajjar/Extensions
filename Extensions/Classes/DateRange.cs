using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace Extensions;

public struct DateRange
{
	public DateTime Start { get; }
	public DateTime End { get; }

	[JsonIgnore]
	public bool Empty => Start.Ticks + End.Ticks == 0;

	public DateRange(DateTime start, DateTime end)
	{
		Start = start;
		End = end;
	}

	public bool Contains(DateTime date)
	{
		return date >= Start && date <= End;
	}

	public bool ContainsExclusive(DateTime date)
	{
		return date > Start && date < End;
	}

	public override bool Equals(object obj)
	{
		return obj is DateRange range &&
			   Start == range.Start &&
			   End == range.End;
	}

	public override int GetHashCode()
	{
		var hashCode = -1676728671;
		hashCode = (hashCode * -1521134295) + Start.GetHashCode();
		hashCode = (hashCode * -1521134295) + End.GetHashCode();
		return hashCode;
	}

	public override string ToString()
	{
		return $"{Start} - {End}";
	}

	public static bool operator ==(DateRange left, DateRange right)
	{
		return EqualityComparer<DateRange>.Default.Equals(left, right);
	}

	public static bool operator !=(DateRange left, DateRange right)
	{
		return !(left == right);
	}
}