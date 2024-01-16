using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extensions
{
	public static partial class ExtensionClass
	{
		public static Random RNG { get; } = new Random(Guid.NewGuid().GetHashCode());

		public enum SizeLength { GB, MB, KB, B, bits }

		public static void AddIfNotExist<T>(this List<T> list, T item)
		{
			if (!list.Any(item))
				list.Add(item);
		}

		public static void AddIfNotExist<T>(this List<T> list, IEnumerable<T> values)
		{
			foreach (T item in values)
			{
				if (!list.Any(item))
					list.Add(item);
			}
		}

#if NET47
		public static IEnumerable<T> GetValues<T>(this T @enum) where T : Enum
		{
			if (@enum.Equals(default(T)))
			{
				yield return @enum;
				yield break;
			}

			foreach (T value in Enum.GetValues(typeof(T)))
			{
				if (!value.Equals(default(T)) && @enum.HasFlag(value))
				{
					yield return value;
				}
			}
		}

		public static bool HasAnyFlag<T>(this T @enum, params T[] values) where T : Enum
		{
			foreach (var value in values)
			{
				if (@enum.HasFlag(value))
				{
					return true;
				}
			}

			return false;
		}

		public static bool HasAllFlag<T>(this T @enum, params T[] values) where T : Enum
		{
			foreach (var value in values)
			{
				if (!@enum.HasFlag(value))
				{
					return false;
				}
			}

			return true;
		}
#endif

		/// <summary>
		/// Checks if Any of the objects in the <see cref="IEnumerable<T>"/> are equal to the <paramref name="item"/>
		/// </summary>
		public static bool AnyOf<T>(this T item, params T[] List)
			=> List.Any(x => x.Equals(item));

		/// <summary>
		/// Checks if All of the objects in the <see cref="IEnumerable<T>"/> are equal to the <paramref name="item"/>
		/// </summary>
		public static bool AllOf<T>(this T item, params T[] List)
			=> List.All(x => x.Equals(item));

		public static T2 As<T, T2>(this T obj, Func<T, T2> conversion) => conversion(obj);

		public static IEnumerable<Match> AsEnumerable(this MatchCollection matches)
		{
			foreach (Match item in matches)
				yield return item;
		}

		/// <summary>
		/// Returns <paramref name="changeValue"/> if this <see cref="{T}"/> is equal to the <paramref name="compareValue"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="compareValue">Value to compare with</param>
		/// <param name="changeValue">Value returned if the comparison is successful</param>
		public static T If<T>(this T baseValue, T compareValue, T changeValue)
			=> baseValue.Equals(compareValue) ? changeValue : baseValue;

		public static T If<T>(this T baseValue, bool condition, T changeValue)
			=> condition ? changeValue : baseValue;

		public static string If(this bool baseValue, string text)
			=> baseValue ? text : string.Empty;

		public static T If<T>(this T baseValue, bool condition, Func<T, T> changeValue)
			=> condition ? changeValue(baseValue) : baseValue;

		/// <summary>
		/// Returns <paramref name="changeValue"/> if this <see cref="{T}"/> is equal to the <paramref name="compareValue"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="compareValue">Value to compare with</param>
		/// <param name="changeValue">Value returned if the comparison is successful</param>
		public static T IfNull<T>(this T baseValue, T changeValue)
			=> baseValue == null ? changeValue : baseValue;

		public static T IfNull<T, T2>(this T2 baseValue, T changeValue, T elseValue)
			=> baseValue == null ? changeValue : elseValue;

		/// <summary>
		/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
		/// <param name="changeValue">Value returned if the comparison is successful</param>
		public static T If<T>(this T baseValue, Func<T, bool> compareFunc, T changeValue)
			=> compareFunc(baseValue) ? changeValue : baseValue;

		/// <summary>
		/// Returns <paramref name="changeValue"/> if this <see cref="{T}"/> is equal to the <paramref name="compareValue"/>, else returns <paramref name="elseValue"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="compareValue">Value to compare with</param>
		/// <param name="changeValue">Value returned if the comparison is successful</param>
		public static T2 If<T, T2>(this T baseValue, T compareValue, T2 changeValue, T2 elseValue)
			=> baseValue.Equals(compareValue) ? changeValue : elseValue;

		/// <summary>
		/// Returns <paramref name="changeValue"/> if this <see cref="{T}"/> is equal to the <paramref name="compareValue"/>, else returns <paramref name="elseValue"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="compareValue">Value to compare with</param>
		/// <param name="changeValue">Value returned if the comparison is successful</param>
		public static T If<T>(this bool baseValue, T changeValue, T elseValue)
			=> baseValue ? changeValue : elseValue;

		/// <summary>
		/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>, else returns <paramref name="elseValue"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
		/// <param name="changeValue">Value returned if the comparison is successful</param>
		/// <param name="elseValue">Value returned if the comparison was unsuccessful</param>
		public static T2 If<T, T2>(this T baseValue, Func<T, bool> compareFunc, T2 changeValue, T2 elseValue)
			=> compareFunc(baseValue) ? changeValue : elseValue;

		/// <summary>
		/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>, else returns <paramref name="elseValue"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
		/// <param name="changeValue">Value returned if the comparison is successful</param>
		/// <param name="elseValue">Value returned if the comparison was unsuccessful</param>
		public static T2 If<T, T2>(this T baseValue, Func<T, bool> compareFunc, T2 changeValue, Func<T, T2> elseValue)
			=> compareFunc(baseValue) ? changeValue : elseValue(baseValue);

		/// <summary>
		/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>, else returns <paramref name="elseValue"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
		/// <param name="changeValue">Value returned if the comparison is successful</param>
		/// <param name="elseValue">Value returned if the comparison was unsuccessful</param>
		public static T2 If<T, T2>(this T baseValue, Func<T, bool> compareFunc, Func<T, T2> changeValue, T2 elseValue)
			=> compareFunc(baseValue) ? changeValue(baseValue) : elseValue;

		/// <summary>
		/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>, else returns <paramref name="elseValue"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
		/// <param name="changeValue">Value returned if the comparison is successful</param>
		/// <param name="elseValue">Value returned if the comparison was unsuccessful</param>
		public static T2 If<T, T2>(this T baseValue, Func<T, bool> compareFunc, Func<T, T2> changeValue, Func<T, T2> elseValue)
			=> compareFunc(baseValue) ? changeValue(baseValue) : elseValue(baseValue);

		/// <summary>
		/// Returns a random item in the <see cref="List{T}"/>
		/// </summary>
		public static T Random<T>(this List<T> L)
			=> L[RNG.Next(L.Count)];

		/// <summary>
		/// Returns a random item in the <see cref="Array"/>
		/// </summary>
		public static T Random<T>(this T[] A)
			=> A[RNG.Next(A.Length)];

		/// <summary>
		/// Gets the Data Size in the <see cref="SizeLength"/> format, base size must be in Bytes
		/// </summary>
		public static double Size(this long size, SizeLength sizeLength, int rounding = 2)
		{
			switch (sizeLength)
			{
				case SizeLength.GB:
					return Math.Round(size / 1073741824d, rounding);

				case SizeLength.MB:
					return Math.Round(size / 1048576d, rounding);

				case SizeLength.KB:
					return Math.Round(size / 1024d, rounding);

				case SizeLength.bits:
					return Math.Round(size * 8d, rounding);

				default:
					return size;
			}
		}

		/// <summary>
		/// Gets the Data Size in the <see cref="SizeLength"/> format, base size must be in Bytes
		/// </summary>
		public static string SizeString(this long size, int rounding = 2)
		{
			if (size > Math.Pow(1000, 3))
				return $"{size.Size(SizeLength.GB, rounding)} GB";
			else if (size > Math.Pow(1000, 2))
				return $"{size.Size(SizeLength.MB, rounding)} MB";
			else if (size > Math.Pow(1000, 1))
				return $"{size.Size(SizeLength.KB, rounding)} KB";

			return $"{size.Size(SizeLength.B, rounding)} Bytes";
		}

		/// <summary>
		/// Gets the Data Size in the <see cref="SizeLength"/> format, base size must be in Bytes
		/// </summary>
		public static double Size(this ulong size, SizeLength sizeLength, int rounding = 2)
		{
			switch (sizeLength)
			{
				case SizeLength.GB:
					return Math.Round(size / 1073741824d, rounding);

				case SizeLength.MB:
					return Math.Round(size / 1048576d, rounding);

				case SizeLength.KB:
					return Math.Round(size / 1024d, rounding);

				case SizeLength.bits:
					return Math.Round(size * 8d, rounding);

				default:
					return size;
			}
		}

		/// <summary>
		/// Gets the Data Size in the <see cref="SizeLength"/> format, base size must be in Bytes
		/// </summary>
		public static string SizeString(this ulong size, int rounding = 2)
		{
			if (size > Math.Pow(1000, 3))
				return $"{size.Size(SizeLength.GB, rounding)} GB";
			else if (size > Math.Pow(1000, 2))
				return $"{size.Size(SizeLength.MB, rounding)} MB";
			else if (size > Math.Pow(1000, 1))
				return $"{size.Size(SizeLength.KB, rounding)} KB";

			return $"{size.Size(SizeLength.B, rounding)} Bytes";
		}

		/// <summary>
		/// Swaps 2 variable values
		/// </summary>
		public static void Swap<T>(ref T O1, ref T O2)
		{ var O = O1; O1 = O2; O2 = O; }

		public static IEnumerable<T2> ThatAre<T2, T>(this IEnumerable<T> list) where T2 : T
			=> list.OfType<T2>();

		public static IEnumerable<T> ThatAre<T>(this Control.ControlCollection list) where T : Control
			=> list.OfType<T>();

		public static string YesNo(this bool b)
			=> b ? "Yes" : "No";

		public static bool IsDigit(this Keys keys)
			=> (keys >= Keys.D0 && keys <= Keys.D9) || (keys >= Keys.NumPad0 && keys <= Keys.NumPad9);

		public static bool IsLetter(this Keys keys)
			=> ((keys & ~Keys.Shift) >= Keys.A && (keys & ~Keys.Shift) <= Keys.Z);

		public static bool IsDigitOrLetter(this Keys keys)
			=> keys.IsDigit() || keys.IsLetter();

		public static T Switch<T, T2>(this T2 item, T2 comp1, T val1, T2 comp2, T val2, T valElse)
		{
			if (item.Equals(comp1))
				return val1;

			if (item.Equals(comp2))
				return val2;

			return valElse;
		}

		public static T Switch<T, T2>(this T2 item, T2 comp1, T val1, T2 comp2, T val2, T2 comp3, T val3, T valElse)
		{
			if (item.Equals(comp1))
				return val1;

			if (item.Equals(comp2))
				return val2;

			if (item.Equals(comp3))
				return val3;

			return valElse;
		}

#if NET47

		public static T Switch<T, T2>(this T2 item, T valDefault, params (T2, T)[] values)
		{
			foreach (var t in values)
				if (item.Equals(t.Item1))
					return t.Item2;

			return valDefault;
		}

#endif

		public static string GetDescription(this Enum value)
		{
			var field = value.GetType().GetField(value.ToString());

			var attribute
					= Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
						as DescriptionAttribute;

			return attribute == null ? value.ToString() : attribute.Description;
		}

		public static string[] GetEnumDescs(this Type value) => Enum.GetValues(value).Cast<Enum>().Select(GetDescription).ToArray();

		public static Enum GetEnumValueFromDescs(this Type type, string value) => Enum.GetValues(type).Cast<Enum>().FirstOrDefault(x => x.GetDescription() == value);
	}
}