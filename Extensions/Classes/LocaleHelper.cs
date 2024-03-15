using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Extensions;

public class LocaleHelper
{
	public static event Action LanguageChanged;

	private static readonly List<LocaleHelper> _locales = [];

	private readonly Dictionary<string, Dictionary<string, Translation>> _locale;

	public static CultureInfo CurrentCulture { get; set; }

	static LocaleHelper()
	{
		try
		{
			if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SlickUI")))
			{
				try
				{
					if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Shared")))
					{
						Directory.Move(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Shared"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SlickUI"));
					}
				}
				catch { }

				Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SlickUI"));
			}

			SaveHandler.Instance.Load(out string culture, "Language.tf", "SlickUI");

			if (!string.IsNullOrWhiteSpace(culture))
			{
				var cultureInfo = new CultureInfo(culture);

				SetCultureAndCalendar(cultureInfo);

				return;
			}
		}
		catch { }

		SetCultureAndCalendar(new CultureInfo("en-US"));
	}

	public static void SetLanguage(CultureInfo cultureInfo)
	{
		SetCultureAndCalendar(cultureInfo);

		SaveHandler.Instance.Save(cultureInfo.IetfLanguageTag, "Language.tf", true, "SlickUI");

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
		var assembly = GetType().Assembly;

		_locales.Add(this);
		_locale = new Dictionary<string, Dictionary<string, Translation>>
		{
			[string.Empty] = GetDictionary(dictionaryResourceName)
		};

		foreach (var name in assembly.GetManifestResourceNames())
		{
			if (name == dictionaryResourceName || !name.Contains(Path.GetFileNameWithoutExtension(dictionaryResourceName) + "."))
			{
				continue;
			}

			var key = Path.GetFileNameWithoutExtension(name);

			_locale[key.Substring(key.LastIndexOf('.') + 1)] = GetDictionary(name);
		}

		Dictionary<string, Translation> GetDictionary(string resourceName)
		{
			using var resourceStream = assembly.GetManifestResourceStream(resourceName);
			if (resourceStream == null)
			{
				return [];
			}

			using var reader = new StreamReader(resourceStream, Encoding.UTF8);
			return JsonConvert.DeserializeObject<Dictionary<string, Translation>>(reader.ReadToEnd());
		}
	}

	public Translation GetText(string key)
	{
		var dic = _locale.ContainsKey(CurrentCulture.IetfLanguageTag)
			? _locale[CurrentCulture.IetfLanguageTag]
			: _locale[string.Empty];

		if (dic.ContainsKey(key))
		{
			return dic[key];
		}

		return key.Contains(' ') ? key : key.FormatWords();
	}

	public static Translation GetGlobalText(string key)
	{
		if (string.IsNullOrWhiteSpace(key))
		{
			return string.Empty;
		}

		foreach (var item in _locales)
		{
			var locale = item._locale;
			var nonDefault = locale.ContainsKey(CurrentCulture.IetfLanguageTag);
			var dic = nonDefault
				? locale[CurrentCulture.IetfLanguageTag]
				: locale[string.Empty];

			if (dic.ContainsKey(key))
			{
				return dic[key];
			}
			else if (nonDefault && locale[string.Empty].ContainsKey(key))
			{
				return locale[string.Empty][key];
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
				yield return lang.IfEmpty("en-US");
			}
		}
	}

	public struct Translation
	{
		private string _plural;
		private string _zero;

		public string One { get; set; }
		public string Zero { readonly get => _zero ?? One; set => _zero = value; }
		public string Plural { readonly get => _plural ?? One; set => _plural = value; }

		public static implicit operator Translation(string text)
		{
			return new Translation { One = text };
		}

		public static implicit operator string(Translation translation)
		{
			return translation.One;
		}

		public readonly string Format(params object[] values)
		{
			return string.Format(One, values);
		}

		public readonly string FormatPlural(params object[] values)
		{
			return string.Format(Plural is null || values[0].Equals(1) ? One : Plural, values);
		}

		public override readonly string ToString()
		{
			return One;
		}
	}
}
