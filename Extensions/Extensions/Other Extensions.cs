using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extensions;

public static partial class ExtensionClass
{
	public enum SizeLength { GB, MB, KB, B, bits }

	public static void AddIfNotExist<T>(this List<T> list, T item)
	{
		if (!list.Any(item))
		{
			list.Add(item);
		}
	}

	public static void AddIfNotExist<T>(this List<T> list, IEnumerable<T> values)
	{
		foreach (var item in values)
		{
			if (!list.Any(item))
			{
				list.Add(item);
			}
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

	public static T TryCast<T>(this int value) where T : Enum
	{
		var enumType = typeof(T);

		if (enumType.IsDefined(typeof(FlagsAttribute), false))
		{
			// For [Flags] enums, allow any combination of defined flags.
			return (T)Enum.ToObject(enumType, value);
		}
		else
		{
			// For regular enums, ensure value is defined.
			if (Enum.IsDefined(enumType, value))
			{
				return (T)Enum.ToObject(enumType, value);
			}
			else
			{
				return default;
			}
		}
	}
#endif

	/// <summary>
	/// Checks if Any of the objects in the <see cref="IEnumerable<T>"/> are equal to the <paramref name="item"/>
	/// </summary>
	public static bool AnyOf<T>(this T item, params T[] List)
	{
		return List.Any(x => x.Equals(item));
	}

	/// <summary>
	/// Checks if All of the objects in the <see cref="IEnumerable<T>"/> are equal to the <paramref name="item"/>
	/// </summary>
	public static bool AllOf<T>(this T item, params T[] List)
	{
		return List.All(x => x.Equals(item));
	}

	public static T2 As<T, T2>(this T obj, Func<T, T2> conversion)
	{
		return conversion(obj);
	}

	public static IEnumerable<Match> AsEnumerable(this MatchCollection matches)
	{
		foreach (Match item in matches)
		{
			yield return item;
		}
	}

	/// <summary>
	/// Returns <paramref name="changeValue"/> if this <see cref="{T}"/> is equal to the <paramref name="compareValue"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="compareValue">Value to compare with</param>
	/// <param name="changeValue">Value returned if the comparison is successful</param>
	public static T If<T>(this T baseValue, T compareValue, T changeValue)
	{
		return baseValue.Equals(compareValue) ? changeValue : baseValue;
	}

	public static T If<T>(this T baseValue, bool condition, T changeValue)
	{
		return condition ? changeValue : baseValue;
	}

	public static string If(this bool baseValue, string text)
	{
		return baseValue ? text : string.Empty;
	}

	public static T If<T>(this T baseValue, bool condition, Func<T, T> changeValue)
	{
		return condition ? changeValue(baseValue) : baseValue;
	}

	/// <summary>
	/// Returns <paramref name="changeValue"/> if this <see cref="{T}"/> is equal to the <paramref name="compareValue"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="compareValue">Value to compare with</param>
	/// <param name="changeValue">Value returned if the comparison is successful</param>
	public static T IfNull<T>(this T baseValue, T changeValue)
	{
		return baseValue == null ? changeValue : baseValue;
	}

	public static T IfNull<T, T2>(this T2 baseValue, T changeValue, T elseValue)
	{
		return baseValue == null ? changeValue : elseValue;
	}

	/// <summary>
	/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
	/// <param name="changeValue">Value returned if the comparison is successful</param>
	public static T If<T>(this T baseValue, Func<T, bool> compareFunc, T changeValue)
	{
		return compareFunc(baseValue) ? changeValue : baseValue;
	}

	/// <summary>
	/// Returns <paramref name="changeValue"/> if this <see cref="{T}"/> is equal to the <paramref name="compareValue"/>, else returns <paramref name="elseValue"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="compareValue">Value to compare with</param>
	/// <param name="changeValue">Value returned if the comparison is successful</param>
	public static T2 If<T, T2>(this T baseValue, T compareValue, T2 changeValue, T2 elseValue)
	{
		return baseValue.Equals(compareValue) ? changeValue : elseValue;
	}

	/// <summary>
	/// Returns <paramref name="changeValue"/> if this <see cref="{T}"/> is equal to the <paramref name="compareValue"/>, else returns <paramref name="elseValue"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="compareValue">Value to compare with</param>
	/// <param name="changeValue">Value returned if the comparison is successful</param>
	public static T If<T>(this bool baseValue, T changeValue, T elseValue)
	{
		return baseValue ? changeValue : elseValue;
	}

	/// <summary>
	/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>, else returns <paramref name="elseValue"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
	/// <param name="changeValue">Value returned if the comparison is successful</param>
	/// <param name="elseValue">Value returned if the comparison was unsuccessful</param>
	public static T2 If<T, T2>(this T baseValue, Func<T, bool> compareFunc, T2 changeValue, T2 elseValue)
	{
		return compareFunc(baseValue) ? changeValue : elseValue;
	}

	/// <summary>
	/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>, else returns <paramref name="elseValue"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
	/// <param name="changeValue">Value returned if the comparison is successful</param>
	/// <param name="elseValue">Value returned if the comparison was unsuccessful</param>
	public static T2 If<T, T2>(this T baseValue, Func<T, bool> compareFunc, T2 changeValue, Func<T, T2> elseValue)
	{
		return compareFunc(baseValue) ? changeValue : elseValue(baseValue);
	}

	/// <summary>
	/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>, else returns <paramref name="elseValue"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
	/// <param name="changeValue">Value returned if the comparison is successful</param>
	/// <param name="elseValue">Value returned if the comparison was unsuccessful</param>
	public static T2 If<T, T2>(this T baseValue, Func<T, bool> compareFunc, Func<T, T2> changeValue, T2 elseValue)
	{
		return compareFunc(baseValue) ? changeValue(baseValue) : elseValue;
	}

	/// <summary>
	/// Returns <paramref name="changeValue"/> if the <paramref name="compareFunc"/> returns <see cref="true"/>, else returns <paramref name="elseValue"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="compareFunc">Method used to test the base <see cref="{T}"/></param>
	/// <param name="changeValue">Value returned if the comparison is successful</param>
	/// <param name="elseValue">Value returned if the comparison was unsuccessful</param>
	public static T2 If<T, T2>(this T baseValue, Func<T, bool> compareFunc, Func<T, T2> changeValue, Func<T, T2> elseValue)
	{
		return compareFunc(baseValue) ? changeValue(baseValue) : elseValue(baseValue);
	}

	/// <summary>
	/// Gets the Data Size in the <see cref="SizeLength"/> format, base size must be in Bytes
	/// </summary>
	public static double Size(this long size, SizeLength sizeLength, int rounding = 2)
	{
		return sizeLength switch
		{
			SizeLength.GB => Math.Round(size / 1073741824d, rounding),
			SizeLength.MB => Math.Round(size / 1048576d, rounding),
			SizeLength.KB => Math.Round(size / 1024d, rounding),
			SizeLength.bits => Math.Round(size * 8d, rounding),
			_ => size,
		};
	}

	/// <summary>
	/// Gets the Data Size in the <see cref="SizeLength"/> format, base size must be in Bytes
	/// </summary>
	public static string SizeString(this long size, int rounding = 2)
	{
		if (size > Math.Pow(1000, 3))
		{
			return $"{size.Size(SizeLength.GB, Math.Max(2, rounding))} GB";
		}
		else if (size > Math.Pow(1000, 2))
		{
			return $"{size.Size(SizeLength.MB, rounding)} MB";
		}
		else if (size > Math.Pow(1000, 1))
		{
			return $"{size.Size(SizeLength.KB, rounding)} KB";
		}
		else if (size > 200)
		{
			return $"{size.Size(SizeLength.KB, Math.Max(2, rounding))} KB";
		}

		return $"{size.Size(SizeLength.B, rounding)} Bytes";
	}

	/// <summary>
	/// Gets the Data Size in the <see cref="SizeLength"/> format, base size must be in Bytes
	/// </summary>
	public static double Size(this ulong size, SizeLength sizeLength, int rounding = 2)
	{
		return sizeLength switch
		{
			SizeLength.GB => Math.Round(size / 1073741824d, rounding),
			SizeLength.MB => Math.Round(size / 1048576d, rounding),
			SizeLength.KB => Math.Round(size / 1024d, rounding),
			SizeLength.bits => Math.Round(size * 8d, rounding),
			_ => size,
		};
	}

	/// <summary>
	/// Gets the Data Size in the <see cref="SizeLength"/> format, base size must be in Bytes
	/// </summary>
	public static string SizeString(this ulong size, int rounding = 2)
	{
		if (size > Math.Pow(1000, 3))
		{
			return $"{size.Size(SizeLength.GB, rounding)} GB";
		}
		else if (size > Math.Pow(1000, 2))
		{
			return $"{size.Size(SizeLength.MB, rounding)} MB";
		}
		else if (size > Math.Pow(1000, 1))
		{
			return $"{size.Size(SizeLength.KB, rounding)} KB";
		}

		return $"{size.Size(SizeLength.B, rounding)} Bytes";
	}

	/// <summary>
	/// Swaps 2 variable values
	/// </summary>
	public static void Swap<T>(ref T O1, ref T O2)
	{
		var O = O1;
		O1 = O2;
		O2 = O;
	}

	public static IEnumerable<T2> ThatAre<T2, T>(this IEnumerable<T> list) where T2 : T
	{
		return list.OfType<T2>();
	}

	public static T Switch<T, T2>(this T2 item, T2 comp1, T val1, T2 comp2, T val2, T valElse)
	{
		if (item.Equals(comp1))
		{
			return val1;
		}

		if (item.Equals(comp2))
		{
			return val2;
		}

		return valElse;
	}

	public static T Switch<T, T2>(this T2 item, T2 comp1, T val1, T2 comp2, T val2, T2 comp3, T val3, T valElse)
	{
		if (item.Equals(comp1))
		{
			return val1;
		}

		if (item.Equals(comp2))
		{
			return val2;
		}

		if (item.Equals(comp3))
		{
			return val3;
		}

		return valElse;
	}

#if NET47

	public static T Switch<T, T2>(this T2 item, T valDefault, params (T2, T)[] values)
	{
		foreach (var t in values)
		{
			if (item.Equals(t.Item1))
			{
				return t.Item2;
			}
		}

		return valDefault;
	}

#endif

	public static string GetDescription(this Enum value)
	{
		var field = value.GetType().GetField(value.ToString());


		return Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is not DescriptionAttribute attribute ? value.ToString() : attribute.Description;
	}

	public static string[] GetEnumDescs(this Type value)
	{
		return Enum.GetValues(value).Cast<Enum>().Select(GetDescription).ToArray();
	}

	public static Enum GetEnumValueFromDescs(this Type type, string value)
	{
		return Enum.GetValues(type).Cast<Enum>().FirstOrDefault(x => x.GetDescription() == value);
	}

	public static bool IsPatternMatch(string input, string pattern)
	{
		// If both input and pattern are empty, return true
		if (string.IsNullOrEmpty(input) && string.IsNullOrEmpty(pattern))
		{
			return true;
		}

		// If pattern is empty and input is not, return false
		if (string.IsNullOrEmpty(pattern))
		{
			return false;
		}

		// If input is empty, check if pattern contains only '*'
		if (string.IsNullOrEmpty(input))
		{
			foreach (var c in pattern)
			{
				if (c != '*')
				{
					return false;
				}
			}

			return true;
		}

		if (!pattern.Any('*') && !pattern.Any('+'))
		{
			return IsVersionEqualOrHigher(input, pattern);
		}

		var inputIndex = 0;
		var patternIndex = 0;
		var inputSnapshot = -1;
		var patternSnapshot = -1;

		while (inputIndex < input.Length)
		{
			if (patternIndex < pattern.Length && (pattern[patternIndex] == '?' || pattern[patternIndex] == input[inputIndex]))
			{
				inputIndex++;
				patternIndex++;
			}
			else if (patternIndex < pattern.Length && pattern[patternIndex] is '*' or '+')
			{
				patternSnapshot = patternIndex;
				inputSnapshot = inputIndex;
				patternIndex++;
			}
			else if (patternSnapshot != -1)
			{
				patternIndex = patternSnapshot + 1;
				inputIndex = ++inputSnapshot;
			}
			else
			{
				return false;
			}
		}

		while (patternIndex < pattern.Length && pattern[patternIndex] is '*' or '+')
		{
			patternIndex++;
		}

		return patternIndex == pattern.Length;
	}

	public static bool IsVersionEqualOrHigher(string version1, string version2)
	{
		if (version1 == version2)
		{
			return true;
		}

		var v1Components = version1.Split('.', 'f');
		var v2Components = version2.Split('.', 'f');

		for (var i = 0; i < Math.Max(v1Components.Length, v2Components.Length); i++)
		{
			if (i >= v1Components.Length) // Version 1 has fewer components
			{
				return true;
			}

			if (i >= v2Components.Length) // Version 2 has fewer components
			{
				return false;
			}

			if (v1Components[i] != v2Components[i])
			{
				if (char.IsLetter(v1Components[i][0]) && char.IsLetter(v2Components[i][0]))
				{
					var v1LetterValue = (int)char.ToUpper(v1Components[i][0]);
					var v2LetterValue = (int)char.ToUpper(v2Components[i][0]);
					return v2LetterValue >= v1LetterValue;
				}
				else if (char.IsLetter(v1Components[i][0]))
				{
					return true; // Letter in v1 is considered higher
				}
				else if (char.IsLetter(v2Components[i][0]))
				{
					return false; // Letter in v2 is considered higher
				}
				else
				{
					var v1Number = v1Components[i].SmartParse();
					var v2Number = v2Components[i].SmartParse();
					return v2Number >= v1Number;
				}
			}
		}

		return true; // Both versions are equal
	}
}