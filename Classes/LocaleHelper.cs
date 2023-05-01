using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Extensions
{
	public class LocaleHelper
	{
		public static event Action LanguageChanged;

		private static readonly List<LocaleHelper> _locales = new List<LocaleHelper>();

		private Dictionary<string, Dictionary<string, string>> _locale;

		public static CultureInfo CurrentCulture { get; set; }

		static LocaleHelper()
		{
			ISave.Load<string>(out var culture, "Language.tf", "Shared");

			if (!string.IsNullOrWhiteSpace(culture))
			{
				try
				{
					var cultureInfo = new CultureInfo(culture);

					SetCultureAndCalendar(cultureInfo);

					return;
				}
				catch { }
			}

			SetCultureAndCalendar(CultureInfo.CurrentCulture);
		}

		public static void SetLanguage(CultureInfo cultureInfo)
		{
			SetCultureAndCalendar(cultureInfo);

			ISave.Save(cultureInfo.IetfLanguageTag, "Language.tf", true, "Shared");

			LanguageChanged?.Invoke();
		}

		private static void SetCultureAndCalendar(CultureInfo cultureInfo)
		{
			CurrentCulture = cultureInfo;

			if (!CurrentCulture.DateTimeFormat.IsReadOnly)
			{
				CurrentCulture.DateTimeFormat.Calendar = CurrentCulture.OptionalCalendars.FirstOrDefault(x => x is GregorianCalendar g && g.CalendarType == GregorianCalendarTypes.Localized) ?? CurrentCulture.OptionalCalendars.FirstOrDefault(x => x is GregorianCalendar);
			}
		}

		protected LocaleHelper(string dictionaryResourceName)
		{
			var assembly = Assembly.GetCallingAssembly();
			using (var resourceStream = assembly.GetManifestResourceStream(dictionaryResourceName))
			{
				if (resourceStream == null)
				{
					_locale = new Dictionary<string, Dictionary<string, string>>
					{
						{ string.Empty, new Dictionary<string, string>() }
					};

					return;
				}

				using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
				{
					var lines = new List<string[]>();

					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						var values = line.Split('\t');
						lines.Add(values);
					}

					ConstructDictionary(lines);
				}

				_locales.Add(this);
			}
		}

		private void ConstructDictionary(List<string[]> lines)
		{
			var dics = new List<Dictionary<string, string>>
			{
				new Dictionary<string, string>()
			};

			_locale = new Dictionary<string, Dictionary<string, string>>
			{
				{ string.Empty, dics[0] }
			};

			for (var i = 2; i < lines[0].Length; i++)
			{
				dics.Add(new Dictionary<string, string>());

				var langKey = lines[0][i];

				if (langKey.StartsWith("\"") && langKey.EndsWith("\""))
				{
					langKey = langKey.Substring(1, langKey.Length - 2);
				}

				_locale[langKey] = dics[i - 1];
			}

			for (var i = 1; i < lines.Count; i++)
			{
				if (lines[i][0].Length == 0)
				{
					continue;
				}

				var langKey = lines[i][0];

				if (langKey.StartsWith("\"") && langKey.EndsWith("\""))
				{
					langKey = langKey.Substring(1, langKey.Length - 2);
				}

				for (var j = 1; j < lines[i].Length; j++)
				{
					var value = lines[i][j];

					if (value.StartsWith("\"") && value.EndsWith("\""))
					{
						value = value.Substring(1, value.Length - 2);
					}

					dics[j - 1][langKey] = value.Replace("\"\"", "\"").Trim();
				}
			}
		}

		public string GetText(string key)
		{
			var dic = _locale.ContainsKey(CurrentCulture.IetfLanguageTag)
				? _locale[CurrentCulture.IetfLanguageTag]
				: _locale[string.Empty];

			if (dic.ContainsKey(key))
			{
				return dic[key].Replace("\\n", "\r\n");
			}

			return key.FormatWords();
		}

		public static string GetGlobalText(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				return string.Empty;
			}

			foreach (var item in _locales)
			{
				var locale = item._locale;
				var dic = locale.ContainsKey(CurrentCulture.IetfLanguageTag)
					? locale[CurrentCulture.IetfLanguageTag]
					: locale[string.Empty];

				if (dic.ContainsKey(key))
				{
					return dic[key].Replace("\\n", "\r\n");
				}
			}

			return key;
		}

		public static IEnumerable<string> GetAvailableLanguages()
		{
			foreach (var item in _locales)
			{
				foreach (var lang in item._locale.Keys)
				{
					yield return lang.IfEmpty("en");
				}
			}
		}
	}
}
