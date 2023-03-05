﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Extensions
{
	public class LocaleHelper
	{
		private static readonly List<LocaleHelper> _locales = new List<LocaleHelper>();

		private Dictionary<string, Dictionary<string, string>> _locale;

		static LocaleHelper()
		{
			ISave.Load<string>(out var culture, "Language.tf", "Shared");

			try
			{ CultureInfo.CurrentUICulture = new CultureInfo(culture); }
			catch { }
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

				_locale[lines[0][i]] = dics[i - 1];
			}

			for (var i = 1; i < lines.Count; i++)
			{
				if (lines[i][0].Length == 0)
					continue;

				for (var j = 1; j < lines[i].Length; j++)
				{
					var value = lines[i][j];

					if (value.StartsWith("\"") && value.EndsWith("\""))
						value = value.Substring(1, value.Length - 2);

					dics[j - 1][lines[i][0]] = value.Trim();
				}
			}
		}

		public string GetText(string key)
		{
			var dic = _locale.ContainsKey(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
				? _locale[CultureInfo.CurrentUICulture.TwoLetterISOLanguageName]
				: _locale[string.Empty];

			if (dic.ContainsKey(key))
				return dic[key];

			return key.FormatWords();
		}

		public static string GetGlobalText(string key)
		{
			foreach (var item in _locales)
			{
				var locale = item._locale;
				var dic = locale.ContainsKey(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
					? locale[CultureInfo.CurrentUICulture.TwoLetterISOLanguageName]
					: locale[string.Empty];

				if (dic.ContainsKey(key))
					return dic[key];
			}

			return key;
		}
	}
}