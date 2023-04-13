using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Extensions
{
	public static partial class ExtensionClass
	{
		/// <summary>
		/// Checks if either one of the strings are Abbreviations of the other
		/// </summary>
		public static bool AbbreviationCheck(this string string1, string string2)
		{
			if (string.IsNullOrWhiteSpace(string1) || string.IsNullOrWhiteSpace(string1))
			{
				return false;
			}

			string1 = string1.ToLower().Replace("'s ", " ");
			string2 = string2.ToLower().Replace("'s ", " ");

			var abbreviation2 = string2.GetAbbreviation();
			var abbreviation1 = string1.GetAbbreviation();

			return (abbreviation2.StartsWith(string1.Where(x => x != ' ')) && abbreviation2.Length > 2)
				|| (abbreviation1.StartsWith(string2.Where(x => x != ' ')) && abbreviation1.Length > 2);
		}

		public static bool Contains(this string s, string text, StringComparison comparison)
		{
			return s.IndexOf(text, comparison) != -1;
		}

		public static IEnumerable<string> WhereNotEmpty(this IEnumerable<string> enumerable)
		{
			return enumerable.Where(x => !string.IsNullOrWhiteSpace(x));
		}

		public static string Plural(this string @base, bool plural, string addition = "s")
		{
			return plural ? $"{@base}{addition}" : @base;
		}

		public static string Plural(this string @base, bool plural, string ifplural, string ifnot)
		{
			return plural ? $"{@base}{ifplural}" : $"{@base}{ifnot}";
		}

		public static string Plural(this string @base, int plural, string addition = "s")
		{
			return plural != 1 ? $"{@base}{addition}" : @base;
		}

		public static string Plural(this string @base, int plural, string ifplural, string ifnot)
		{
			return plural != 1 ? $"{@base}{ifplural}" : $"{@base}{ifnot}";
		}

		public static string Plural<T>(this string @base, IEnumerable<T> plural, string addition = "s")
		{
			return (plural?.Count() ?? 0) != 1 ? $"{@base}{addition}" : @base;
		}

		public static string Plural<T>(this string @base, IEnumerable<T> plural, string ifplural, string ifnot)
		{
			return (plural?.Count() ?? 0) != 1 ? $"{@base}{ifplural}" : $"{@base}{ifnot}";
		}

		/// <summary>
		/// Gets what's between the <paramref name="Start"/> <see cref="char"/> and the <paramref name="End"/> <see cref="string"/> in the given <see cref="string"/>
		/// </summary>
		public static string Between(this string S, char Start, string End)
		{
			return S.Between(Start.ToString(), End);
		}

		/// <summary>
		/// Gets what's between the <paramref name="Start"/> <see cref="string"/> and the <paramref name="End"/> <see cref="char"/> in the given <see cref="string"/>
		/// </summary>
		public static string Between(this string S, string Start, char End)
		{
			return S.Between(Start, End.ToString());
		}

		/// <summary>
		/// Gets what's between the <paramref name="Start"/> <see cref="char"/> and the <paramref name="End"/> <see cref="char"/> in the given <see cref="string"/>
		/// </summary>
		public static string Between(this string S, char Start, char End)
		{
			return S.Between(Start.ToString(), End.ToString());
		}

		/// <summary>
		/// Gets what's between the <paramref name="Start"/> <see cref="string"/> and the <paramref name="End"/> <see cref="string"/> in the given <see cref="string"/>
		/// </summary>
		/// <param name="IgnoreCase">Ignores case sensitivity</param>
		public static string Between(this string S, string Start, string End, bool IgnoreCase = false)
		{
			var RX = IgnoreCase ? new Regex($@"(?<={Regex.Escape(Start)}).+?(?={Regex.Escape(End)})", RegexOptions.IgnoreCase) : new Regex($@"(?<={Regex.Escape(Start)}).+?(?={Regex.Escape(End)})");
			return RX.IsMatch(S) ? RX.Match(S).Value : S;
		}

		/// <summary>
		/// Checks if the <see cref="string"/> contains a specific Word
		/// </summary>
		public static bool ContainsWord(this string S, string Word, bool ignoreCase = false)
		{
			return Regex.IsMatch(S, @"\b" + Regex.Escape(Word) + @"\b", ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
		}

		public static string Copy(this string str, int times)
		{
			var sb = new StringBuilder(str.Length * times);

			for (var i = 0; i < times; i++)
			{
				sb.Append(str);
			}

			return sb.ToString();
		}

		/// <summary>
		/// Gets the Abbreviation of the <see cref="string"/>
		/// </summary>
		public static string GetAbbreviation(this string S)
		{
			var SB = new StringBuilder();
			foreach (var item in S.GetWords(true))
			{
				SB.Append(item.All(x => char.IsDigit(x)) ? item : item[0].ToString());
			}

			if (Regex.IsMatch(SB.ToString(), "^[A-z]+[0-9]+$"))
			{
				var match = Regex.Match(SB.ToString(), "^([A-z]+)([0-9]+)$");
				return $"{match.Groups[1]} {match.Groups[2]}";
			}

			return SB.ToString();
		}

		/// <summary>
		/// Gets an <see cref="string[]"/> containing all the Words in the <see cref="string"/>
		/// </summary>
		public static string[] GetWords(this string S, bool includeNumbers = false)
		{
			return string.IsNullOrWhiteSpace(S) ? new string[0] : Regex.Matches(S, @"\b" + (includeNumbers ? "" : "(?![0-9])") + @"(\w+)(?:'\w+)?\b")
						.Convert(x => x.Groups[1].Value).ToArray();
		}

		public static string FormatWords(this string str, bool forceUpper = false)
		{
			str = str.RegexReplace(@"([a-z])([A-Z])", x => $"{x.Groups[1].Value} {x.Groups[2].Value}", false)
				.RegexReplace(@"(\b)(?<!')([a-z])", x => $"{x.Groups[1].Value}{x.Groups[2].Value.ToUpper()}", false);

			if (forceUpper)
			{
				str = str.RegexReplace(@"(^[a-z])|(\ [a-z])", x => x.Value.ToUpper());
			}

			return str;
		}

		/// <summary>
		/// Changes the value of the string to <paramref name="value"/> if the string is null or white space
		/// </summary>
		public static string IfEmpty(this string S, string value, string @else = null)
		{
			return string.IsNullOrWhiteSpace(S) ? value : (@else ?? S);
		}

		/// <summary>
		/// Lists all the strings in the <see cref="IEnumerable{string}"/> next to each other in one <see cref="string"/>
		/// </summary>
		public static string ListStrings<T>(this IEnumerable<T> list, string seperator)
		{
			return list.ListStrings(x => x + seperator, false);
		}

		/// <summary>
		/// Lists all the strings in the <see cref="IEnumerable{string}"/> next to each other in one <see cref="string"/>
		/// </summary>
		public static string ListStrings<T>(this IEnumerable<T> list, Func<T, string> Format, string seperator)
		{
			return list.Select(Format).ListStrings(x => x + seperator, false);
		}

		/// <summary>
		/// Lists all the strings in the <see cref="IEnumerable{string}"/> with a set <paramref name="Format"/>
		/// </summary>
		/// <param name="Format">Format to change each string, null leaves the strings as they are</param>
		public static string ListStrings<T>(this IEnumerable<T> list, Func<T, string> Format = null, bool applyToLast = true)
		{
			if (list == null)
			{
				return string.Empty;
			}

			var SB = new StringBuilder();
			var arr = list.ToArray();

			for (var i = 0; i < arr.Length; i++)
			{
				if (!applyToLast && i == arr.Length - 1)
				{
					SB.Append(arr[i]);
				}
				else
				{
					SB.Append(Format?.Invoke(arr[i]) ?? arr[i].ToString());
				}
			}

			return SB.ToString();
		}

		/// <summary>
		/// Removes all occurrence of a <see cref="Regex"/> <paramref name="pattern"/> in the <see cref="string"/>
		/// </summary>
		/// <param name="pattern"><see cref="Regex"/> Pattern</param>
		/// <param name="group">Specifies which <paramref name="group"/> in the <see cref="Regex"/> match to remove</param>
		/// <param name="ignoreCase">IgnoreCase <see cref="Regex"/> option</param>
		public static string RegexRemove(this string @base, string pattern, int group = 0, bool ignoreCase = true)
		{
			return @base == null ? null : ignoreCase ? Regex.Replace(@base, pattern, x => x.Value.Remove(x.Groups[group].Value), RegexOptions.IgnoreCase) : Regex.Replace(@base, pattern, x => x.Value.Remove(x.Groups[group].Value), RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Replaces all occurrence of a <see cref="Regex"/> <paramref name="pattern"/> with a <paramref name="replacement"/>
		/// </summary>
		/// <param name="pattern"><see cref="Regex"/> Pattern</param>
		/// <param name="replacement">Text Replacement</param>
		/// <param name="ignoreCase">IgnoreCase <see cref="Regex"/> option</param>
		public static string RegexReplace(this string @base, string pattern, string replacement = "", bool ignoreCase = true)
		{
			return @base == null ? null : ignoreCase ? Regex.Replace(@base, pattern, replacement, RegexOptions.IgnoreCase) : Regex.Replace(@base, pattern, replacement, RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Replaces all occurrence of a <see cref="Regex"/> <paramref name="pattern"/> with the result of the <paramref name="replacement"/> method
		/// </summary>
		/// <param name="pattern"><see cref="Regex"/> Pattern</param>
		/// <param name="replacement">Method that converts the matched string</param>
		/// <param name="ignoreCase">IgnoreCase <see cref="Regex"/> option</param>
		public static string RegexReplace(this string @base, string pattern, MatchEvaluator replacement = null, bool ignoreCase = true)
		{
			return ignoreCase ? Regex.Replace(@base, pattern, replacement, RegexOptions.IgnoreCase) : Regex.Replace(@base, pattern, replacement);
		}

		/// <summary>
		/// Removes the first occurrence of the text in the <see cref="string"/>
		/// </summary>
		public static string Remove(this string S, string text)
		{
			return S == null ? null : (S.IndexOf(text) >= 0 ? S.Remove(S.IndexOf(text), text.Length) : S);
		}

		/// <summary>
		/// Removes the character at the specified index
		/// </summary>
		public static string RemoveAt(this string S, int index)
		{
			return S.Remove(index, 1);
		}

		/// <summary>
		/// Removes any multiple spaces after each other and any starting or ending spaces
		/// </summary>
		public static string RemoveDoubleSpaces(this string S)
		{
			return S.RegexReplace(@" {2,}", " ")?.Trim() ?? string.Empty;
		}

		/// <summary>
		/// Parses the <see cref="string"/> into an <see cref="int"/> disregarding non-digit characters
		/// </summary>
		/// <returns>Returns 0 if no digits are found</returns>
		public static int SmartParse(this string S, int defaultValue = 0)
		{
			return !string.IsNullOrWhiteSpace(S) && int.TryParse(S.Where(c => char.IsDigit(c)), out var i) ? i : defaultValue;
		}

		public static double SmartParseD(this string S, double defaultValue = 0)
		{
			return Regex.Match(S, @"[-+]?[0-9]*[,\.]?[0-9]+([eE][-+]?[0-9]+)?").If(x => x.Success, x => double.TryParse(x.Value, out var d) ? d : defaultValue, defaultValue);
		}

		public static float SmartParseF(this string S, float defaultValue = 0)
		{
			return Regex.Match(S, @"[-+]?[0-9]*[,\.]?[0-9]+([eE][-+]?[0-9]+)?").If(x => x.Success, x => float.TryParse(x.Value, out var f) ? f : defaultValue, defaultValue);
		}

		/// <summary>
		/// Parses the <see cref="string"/> into an <see cref="int"/> disregarding non-digit characters
		/// </summary>
		/// <returns>Returns 0 if no digits are found</returns>
		public static int SmartParse(this string S, int defaultValue, bool strict)
		{
			if (string.IsNullOrEmpty(S))
			{
				return defaultValue;
			}

			var i = 0;

			if (strict)
			{
				return int.TryParse(S, out i) ? i : defaultValue;
			}

			return int.TryParse(S.Where(c => char.IsDigit(c)), out i) ? i : defaultValue;
		}

		/// <summary>
		/// Checks if the <see cref="string"/> matches <paramref name="s2"/> with a <paramref name="MaxErrors"/> margin
		/// </summary>
		/// <param name="s2"><see cref="string"/> to match against</param>
		/// <param name="maxErrors">Maximum amount of differences between the two strings</param>
		/// <param name="caseCheck">Option to match the strings with Case Sensitivity</param>
		/// <returns></returns>
		public static bool SpellCheck(this string s1, string s2, int maxErrors = 2, bool caseCheck = true)
		{
			return SpellCheck(s1, s2, caseCheck) <= maxErrors;
		}

		/// <summary>
		/// Checks if the <see cref="string"/> matches <paramref name="termToBeSearched"/> with a <paramref name="MaxErrors"/> margin
		/// </summary>
		/// <param name="termToBeSearched"><see cref="string"/> to match against</param>
		/// <param name="maxErrors">Maximum amount of differences between the two strings</param>
		/// <param name="caseCheck">Option to match the strings with Case Sensitivity</param>
		/// <returns></returns>
		public static bool SearchCheck(this string searchTerm, string termToBeSearched, bool caseCheck = false)
		{
			if (string.IsNullOrWhiteSpace(searchTerm) && string.IsNullOrWhiteSpace(termToBeSearched))
				return true;

			if (string.IsNullOrWhiteSpace(searchTerm) || string.IsNullOrWhiteSpace(termToBeSearched))
				return false;

			if (SpellCheck(searchTerm, termToBeSearched, caseCheck) <= (int)Math.Ceiling((termToBeSearched.Length - 4) / 4.5M))
				return true;

			if (termToBeSearched.IndexOf(searchTerm, caseCheck ? StringComparison.CurrentCulture : StringComparison.InvariantCultureIgnoreCase) >= 0)
				return true;

			if (searchTerm.AbbreviationCheck(termToBeSearched))
				return true;

			if (searchTerm.Contains(' '))
			{
				var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				if (terms.All(x => termToBeSearched.IndexOf(x, caseCheck ? StringComparison.CurrentCulture : StringComparison.InvariantCultureIgnoreCase) >= 0))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns the total amount of differences between the two strings
		/// </summary>
		/// <param name="S2"><see cref="string"/> to match against</param>
		/// <param name="caseCheck">Option to match the strings with Case Sensitivity</param>
		public static int SpellCheck(this string s1, string s2, bool caseCheck = true)
		{
			s1 = s1.RemoveDoubleSpaces();
			s2 = s2.RemoveDoubleSpaces();

			if (!caseCheck)
			{
				s1 = s1.ToLower();
				s2 = s2.ToLower();
			}

			// Levenshtein Algorithm
			var n = s1.Length;
			var m = s2.Length;
			var d = new int[n + 1, m + 1];

			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			for (var i = 0; i <= n; d[i, 0] = i++)
			{ }

			for (var j = 0; j <= m; d[0, j] = j++)
			{ }

			for (var i = 1; i <= n; i++)
			{
				for (var j = 1; j <= m; j++)
				{
					var cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;

					d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						 d[i - 1, j - 1] + cost);
				}
			}
			return d[n, m];
		}

		/// <summary>
		/// Changes the <see cref="string"/> to a Capitalize Each Word <see cref="string"/>
		/// </summary>
		/// <param name="ForceLowerCase">Forces all existing upper-case into lower-case before conversion</param>
		public static string ToCapital(this string S, bool ForceLowerCase = true)
		{
			return Regex.Replace(ForceLowerCase ? S.ToLower() : S, @"\b(([A-z])(\w+)?)", new MatchEvaluator(x => ToUpper(x, S)));
		}

		/// <summary>
		/// Changes all strings in the <see cref="IEnumerable{string}"/> to Lowercase
		/// </summary>
		public static IEnumerable<string> ToLower(this IEnumerable<string> IE)
		{
			var array = IE.ToArray();

			for (var i = 0; i < array.Count(); i++)
			{
				array[i] = array[i].ToLower();
			}

			return array.AsEnumerable();
		}

		/// <summary>
		/// Removes <paramref name="count"/> characters from the end of a <see cref="string"/>
		/// </summary>
		public static string TrimEnd(this string s, int count)
		{
			return s.Length >= count ? s.Substring(0, s.Length - count) : string.Empty;
		}

		/// <summary>
		/// Filters the <see cref="string"/> based on the <paramref name="Test"/> for each <see cref="char"/>
		/// </summary>
		/// <param name="S"></param>
		/// <param name="Test"></param>
		/// <returns></returns>
		public static string Where(this string S, Func<char, bool> Test)
		{
			var Out = new StringBuilder(S.Length);
			foreach (var c in S)
			{
				if (Test(c))
				{
					Out.Append(c);
				}
			}

			return Out.ToString();
		}

		/// <summary>
		/// Changes all strings in the <see cref="IEnumerable{string}"/> to Uppercase
		/// </summary>
		private static string ToUpper(Match match, string Base)
		{
			var low = match.Value.ToLower();
			if (!(match.Index > 0 && char.IsLetter(low[0]) && Base[match.Index - 1] == '\'') &&
				((match.Index == 0) || (!(low == "the" || low == "in" || low == "of" || low == "at" || low == "and"))))
			{
				return match.Value[0].ToString().ToUpper() + (match.Length == 0 ? "" : match.Value.Substring(1));
			}

			return match.Value;
		}

		public static string GetString(this Version version)
		{
			if (version.Revision > 0)
			{
				return version.ToString(4);
			}
			else if (version.Build > 0)
			{
				return version.ToString(3);
			}
			else
			{
				return version.ToString(2);
			}
		}
	}
}