using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions;

public static partial class ExtensionClass
{
	#region Dictionary Extensions

	/// <summary>
	/// Adds the <paramref name="valuePair"/> to the <see cref="Dictionary{T, T2}"/>
	/// </summary>
	public static void Add<T, T2>(this Dictionary<T, T2> dic, KeyValuePair<T, T2> valuePair)
	{
		if (!dic.ContainsKey(valuePair.Key))
		{
			dic.Add(valuePair.Key, valuePair.Value);
		}
	}

	/// <summary>
	/// Adds the <see cref="IEnumerable{KeyValuePair}"/> <paramref name="valuePair"/> to the <see cref="Dictionary{T, T2}"/>
	/// </summary>
	public static void AddRange<T, T2>(this Dictionary<T, T2> dictionary, IEnumerable<KeyValuePair<T, T2>> valuesPairs)
	{
		foreach (var item in valuesPairs)
		{
			dictionary.Add(item);
		}
	}

	/// <summary>
	/// Converts this collection of <see cref="KeyValuePair{TK, TV}"/> to a <see cref="Dictionary{TK, TV}"/>
	/// </summary>
	public static Dictionary<TK, TV> AsDictionary<TK, TV>(this IEnumerable<KeyValuePair<TK, TV>> list)
	{
		return list.ConvertDictionary(x => x);
	}

	/// <summary>
	/// Converts this <see cref="IEnumerable{T}"/> to a <see cref="Dictionary{TK, TV}"/> using the conversion <paramref name="func"/>
	/// </summary>
	public static Dictionary<TK, TV> ConvertDictionary<T, TK, TV>(this IEnumerable<T> list, Func<T, KeyValuePair<TK, TV>> func)
	{
		var dic = new Dictionary<TK, TV>();

		foreach (var item in list)
		{
			var kvp = func(item);

			dic[kvp.Key] = kvp.Value;
		}

		return dic;
	}

	/// <summary>
	/// Converts this <see cref="IEnumerable{T}"/> to a <see cref="Dictionary{TK, TV}"/> using the conversion <paramref name="func"/>
	/// </summary>
	public static Dictionary<TK, TV> ConvertDictionary<TK, TV>(this IEnumerable<TV> list, Func<TV, TK> func)
	{
		var dic = new Dictionary<TK, TV>();

		foreach (var item in list)
		{
			dic[func(item)] = item;
		}

		return dic;
	}

	/// <summary>
	/// Adds the specified key and value to the dictionary or updates the key with the new value
	/// </summary>
	public static void TryAdd<TK, TV>(this Dictionary<TK, TV> dic, TK key, TV val, Func<TK, TK, bool> comparer = null)
	{
		if (comparer != null)
		{
			var newkey = dic.Keys.FirstOrDefault(x => comparer(x, key));
			if (newkey != null)
			{
				key = newkey;
			}
		}

		if (key != null && dic.ContainsKey(key))
		{
			dic[key] = val;
		}
		else
		{
			dic.Add(key, val);
		}
	}

	#endregion Dictionary Extensions
}