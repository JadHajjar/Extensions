using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Extensions
{
	public static partial class ExtensionClass
	{
		public delegate void action();

		public delegate void DynamicAction<T>(T t);

		public static void AddIfNotNull<T>(this IList<T> list, T item) where T : class
		{
			if (item != null)
			{
				list.Add(item);
			}
		}

		public static T FirstOrAny<T>(this IEnumerable<T> values, Func<T, bool> predicate) where T : class
			=> values == null ? default : values.FirstOrDefault(predicate) ?? values.FirstOrDefault();

		public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
		{
			while (source.Any())
			{
				yield return source.Take(chunkSize);
				source = source.Skip(chunkSize);
			}
		}

		public static IEnumerable<TSource> SelectWhereNotNull<TSource>(this IEnumerable<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			foreach (var item in source)
			{
				if (item != null)
					yield return item;
			}
		}

		public static IEnumerable<TResult> SelectWhereNotNull<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}

			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}

			foreach (var item in source)
			{
				var obj = selector(item);

				if (obj != null)
					yield return obj;
			}
		}

		public static List<T> ToList<T, TSource>(this IEnumerable<TSource> values, Func<TSource, T> conversion)
		{
			var list = new List<T>();

			foreach (var item in values)
			{
				list.Add(conversion(item));
			}

			return list;
		}

		public static T[] ToArray<T, TSource>(this IEnumerable<TSource> values, Func<TSource, T> conversion)
		{
			return values.Select(conversion).ToArray();
		}

		public static List<T> AllWhere<T>(this IEnumerable<T> values, Predicate<T> predicate)
		{
			var list = new List<T>();

			foreach (var item in values)
			{
				if (predicate(item))
				{
					list.Add(item);
				}
			}

			return list;
		}

		/// <summary>
		/// Adds items from an <see cref="IEnumerable{T2}"/> into the List after a set Conversion
		/// </summary>
		/// <param name="Source">IEnumerable Data Source</param>
		/// <param name="Conversion">Conversion function from T2 to T1</param>
		public static void AddRange<T, T2>(this List<T> list, IEnumerable<T2> Source, Func<T2, T> Conversion)
		{
			if (Source != null)
			{
				foreach (var item in Source)
				{
					list.Add(Conversion(item));
				}
			}
		}

		/// <summary>
		/// Checks if any of the elements in the first <see cref="IEnumerable{T}"/> is equal to any element in the second
		/// </summary>
		public static bool Any<T>(this IEnumerable<T> l, IEnumerable<T> list)
		{
			return l.Any(x => list.Any(y => x.Equals(y)));
		}

		/// <summary>
		/// Checks if any of the elements in the first <see cref="IEnumerable{T}"/> is equal to one element
		/// </summary>
		public static bool Any<T>(this IEnumerable<T> l, T item)
		{
			return l.Any(x => x?.Equals(item) ?? item == null);
		}

		/// <summary>
		/// Checks if any of the elements in the first <see cref="IEnumerable{T}"/> is equal to one element
		/// </summary>
		public static bool Any<T>(this IEnumerable<T> l, T[] list)
		{
			if (list.Length == 0)
			{
				foreach (var item in l)
				{
					return true;
				}

				return false;
			}

			return list.Any(x => l.Any(item => x.Equals(item)));
		}

		/// <summary>
		/// Converts this <see cref="IEnumerable{T}"/> to an <see cref="IEnumerable{T2}"/> using the conversion <paramref name="func"/>
		/// </summary>
		public static IEnumerable<T2> Convert<T, T2>(this IEnumerable<T> list, Func<T, T2> func)
		{
			return list.Select(func);
		}

		/// <summary>
		/// Converts this <see cref="MatchCollection"/> to an <see cref="IEnumerable{T}"/> using the conversion <paramref name="func"/>
		/// </summary>
		public static IEnumerable<T> Convert<T>(this MatchCollection list, Func<Match, T> func)
		{
			foreach (var item in list)
			{
				yield return func(item as Match);
			}
		}

		/// <summary>
		/// Converts this <see cref="IEnumerable{T}"/> to an <see cref="IEnumerable{T2}"/> using the conversion <paramref name="func"/>
		/// </summary>
		public static IEnumerable<T2> ConvertEnumerable<T, T2>(this IEnumerable<T> list, Func<T, IEnumerable<T2>> func)
		{
			if (list != null)
			{
				foreach (var item in list)
				{
					foreach (var item2 in func(item))
					{
						yield return item2;
					}
				}
			}
		}

		/// <summary>
		/// Cuts out all elements of the <see cref="List{T}"/> outside the selected range
		/// </summary>
		public static void Cut<T>(this List<T> list, int startIndex, int count)
		{
			for (var i = startIndex + count; i < list.Count; i++)
			{
				list.RemoveAt(i);
			}

			for (var i = 0; i < startIndex; i++)
			{
				list.RemoveAt(i);
			}
		}

		public static IEnumerable<T> Distinct<T>(this IEnumerable<T> list, Func<T, T, bool> comparer)
		{
			var items = new List<T>();

			foreach (var item in list)
			{
				if (!items.Any(x => comparer(x, item)))
				{
					items.Add(item);
					yield return item;
				}
			}
		}

		public static IEnumerable<T> Distinct<T, T2>(this IEnumerable<T> list, Func<T, T2> convert)
		{
			var items = new List<T2>();

			foreach (var item in list)
			{
				var c = convert(item);
				if (!items.Contains(c))
				{
					items.Add(c);
					yield return item;
				}
			}
		}

		/// <summary>
		/// Runs an <paramref name="action"/> for each item in the <see cref="IEnumerable{T}"/>
		/// </summary>
		public static void Foreach<T>(this IEnumerable<T> list, DynamicAction<T> action)
		{
			foreach (var item in list)
			{
				action(item);
			}
		}

		/// <summary>
		/// Checks if the <see cref="IEnumerable{T}"/> only contains one element being <paramref name="item"/>
		/// </summary>
		public static bool IsOnly<T>(this IEnumerable<T> list, T item)
		{
			return list.Count() == 1 && list.First().Equals(item);
		}

		/// <summary>
		/// Returns the last element in the <see cref="IEnumerable{T}"/> that satisfies the <paramref name="predictate"/>
		/// </summary>
		public static T LastThat<T>(this IEnumerable<T> list, Func<T, bool> predictate)
		{
			return list.LastOrDefault(predictate);
		}

		public static T Next<T>(this IEnumerable<T> enumerable, T item, bool circleBack = false)
		{
			if (!enumerable?.Any() ?? true)
			{
				return item;
			}

			var found = false;

			foreach (var it in enumerable)
			{
				if (found)
				{
					return it;
				}

				if (it.Equals(item))
				{
					found = true;
				}
			}

			return circleBack ? enumerable.FirstOrDefault() : default;
		}

		public static T Previous<T>(this IEnumerable<T> enumerable, T item, bool circleBack = false)
		{
			if (!enumerable?.Any() ?? true)
			{
				return item;
			}

			var found = false;

			foreach (var it in enumerable.Reverse())
			{
				if (found)
				{
					return it;
				}

				if (it.Equals(item))
				{
					found = true;
				}
			}

			return circleBack ? enumerable.LastOrDefault() : default;
		}

		public static T TryGet<T>(this IEnumerable<T> enumerable, int index)
		{
			try
			{
				if (enumerable != null && enumerable.Count() > index)
				{
					return enumerable.ElementAtOrDefault(index);
				}
			}
			catch { }

			return default;
		}

		public static T2 TryGet<T, T2>(this Dictionary<T, T2> enumerable, T index)
		{
			try
			{
				if (enumerable.ContainsKey(index))
				{
					return enumerable[index];
				}
			}
			catch { }

			return default;
		}

		public static T2 GetOrAdd<T, T2>(this Dictionary<T, T2> enumerable, T index) where T2 : new()
		{
			if (enumerable.TryGetValue(index, out var val))
			{
				return val;
			}

			var newVal = new T2();

			enumerable[index] = newVal;

			return newVal;
		}

		/// <summary>
		/// Removes occurences of the <paramref name="source"/> collection from the <see cref="List{T}"/>
		/// </summary>
		public static void RemoveRange<T>(this List<T> list, IEnumerable<T> source)
		{
			foreach (var item in source)
			{
				list.Remove(item);
			}
		}

		public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
		{
			return source.Skip(Math.Max(0, source.Count() - count));
		}

		public static IEnumerable<T> ThatAre<T>(this IEnumerable<object> source)
		{
			return source.OfType<T>();
		}

		public static IEnumerable<T> Trim<T>(this IEnumerable<T> list, Func<T, bool> comparer)
		{
			var items = new List<T>(list);

			while (items.Any() && comparer(items[0]))
			{
				items.RemoveAt(0);
			}

			while (items.Any() && comparer(items[items.Count - 1]))
			{
				items.RemoveAt(items.Count - 1);
			}

			return items;
		}

		public static IEnumerable<T> Trim<T>(this IEnumerable<T> list, T comparVal)
		{
			var items = new List<T>(list);

			while (items.Any() && comparVal.Equals(items[0]))
			{
				items.RemoveAt(0);
			}

			while (items.Any() && comparVal.Equals(items[items.Count - 1]))
			{
				items.RemoveAt(items.Count - 1);
			}

			return items;
		}

		/// <summary>
		/// Returns the items from the collection within the selected range
		/// </summary>
		public static IEnumerable<T> Trim<T>(this IEnumerable<T> list, int startIndex, int count)
		{
			if (startIndex > list.Count())
			{
				throw new IndexOutOfRangeException();
			}

			foreach (var item in list)
			{
				if (startIndex-- <= 0)
				{
					yield return item;
					if (--count <= 0)
					{
						break;
					}
				}
			}
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
		{
			var buffer = source as List<T> ?? source.ToList();
			for (var i = 0; i < buffer.Count; i++)
			{
				var j = RNG.Next(i, buffer.Count);
				yield return buffer[j];

				buffer[j] = buffer[i];
			}
		}
	}
}