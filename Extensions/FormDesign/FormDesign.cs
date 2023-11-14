using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Extensions
{
	public enum FormDesignType { None, Dark, Light }

	public partial class FormDesign
	{
		public delegate void DesignEventHandler(FormDesign design);

		public static event DesignEventHandler DesignChanged;

		public static bool WindowsButtons { get; set; } = true;
		public static bool NightModeEnabled { get; set; }
		public static bool NightMode { get; private set; } = !DateTime.Now.Hour.IsWithin(7, 20);

		public static bool UseSystemTheme { get; set; } = true;
		public static bool IsDarkMode { get; set; } = IsSystemDark();

		public FormDesign(string name, int id, FormDesignType t, bool temp = false)
		{
			Name = name;
			ID = id;
			Type = t;
			Temporary = temp;
		}

		#region Current Design

		private static FormDesign design = Modern;
		private static bool Initialized;
		private static Form _form;

		public static FormDesign Design
		{
			get
			{
				if (!Initialized)
				{
					return Modern;
				}

				if (UseSystemTheme && IsDarkMode != IsSystemDark())
				{
					IsDarkMode = !IsDarkMode;
					ForceRefresh();
				}

				if (NightMode == SunManager.SunTime.Contains(DateTime.Now))
				{
					NightMode = !NightMode;
					ForceRefresh();
				}

				if (NightModeEnabled && NightMode && design.Type != FormDesignType.Dark)
				{
					return design.DarkMode;
				}

				if (UseSystemTheme && IsDarkMode && design.Type != FormDesignType.Dark)
				{
					return design.DarkMode;
				}

				return design;
			}

			private set
			{
				if (value != design)
				{
					design = value;
					DesignChanged?.Invoke(Design);
					if (!loadIdentifier.Disabled)
					{
						Save();
					}
				}
			}
		}

		#endregion Current Design

		#region Statics

		private static readonly DisableIdentifier loadIdentifier = new DisableIdentifier();

		public static void Initialize(Form form, DesignEventHandler handler = null)
		{
			Initialized = true;

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

			Load();

			if (handler != null)
			{
				DesignChanged += handler;
				handler(Design);
			}

			StartListener(form);
		}

		public static void Switch()
		{
			Design = List.Next(design) ?? List[0];
		}

		public static void Switch(FormDesign newDesign, bool forceSave = false, bool forceRefresh = false)
		{
			if (newDesign != null)
			{
				forceRefresh |= design != newDesign;
				forceSave |= forceRefresh && newDesign.Name != "Custom";
				design = newDesign;

				if (forceRefresh)
				{
					ForceRefresh();
				}

				if (forceSave && !newDesign.Temporary)
				{
					Save();
				}
			}
		}

		public static bool IsCustomEligible()
		{
			return Custom.ID != -1 && Custom.BackColor.A != 0;
		}

		public static void StartListener(Form form)
		{
			_form = form;

			var watcher = new FileSystemWatcher
			{
				Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SlickUI"),
				NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
				Filter = "*.*",
			};

			watcher.Changed += new FileSystemEventHandler(FileChanged);
			watcher.Created += new FileSystemEventHandler(FileChanged);
			watcher.Deleted += new FileSystemEventHandler(FileChanged);

			watcher.EnableRaisingEvents = true;
		}

		private static void FileChanged(object sender, FileSystemEventArgs e)
		{
			if (Path.GetFileName(e.FullPath).StartsWith("DesignMode.tf") && !loadIdentifier.Disabled)
			{
				_form.TryInvoke(Load);
			}
		}

		public static bool IsSystemDark()
		{
			const string keyName = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
			const string valueName = "AppsUseLightTheme";

			try
			{
				var value = Registry.GetValue(keyName, valueName, null);

				if (value != null)
				{
					return (int)value == 0;
				}
			}
			catch { }

			return false;
		}

		public static void Load()
		{
			loadIdentifier.Disable();
			try
			{
				var obj = ISave.LoadRaw("DesignMode.tf", "SlickUI");

				if (obj != null)
				{
					Custom = new FormDesign((string)obj.Custom.Name, (int)obj.Custom.ID, (FormDesignType)obj.Custom.Type)
					{
						BackColor = obj.Custom.BackColor,
						ButtonForeColor = obj.Custom.ButtonForeColor,
						MenuForeColor = obj.Custom.MenuForeColor,
						ForeColor = obj.Custom.ForeColor,
						ButtonColor = obj.Custom.ButtonColor,
						AccentColor = obj.Custom.AccentColor,
						MenuColor = obj.Custom.MenuColor,
						LabelColor = obj.Custom.LabelColor,
						InfoColor = obj.Custom.InfoColor,
						ActiveColor = obj.Custom.ActiveColor,
						ActiveForeColor = obj.Custom.ActiveForeColor,
						RedColor = obj.Custom.RedColor,
						GreenColor = obj.Custom.GreenColor,
						YellowColor = obj.Custom.YellowColor,
						IconColor = obj.Custom.IconColor
					};

					WindowsButtons = (bool?)obj.WindowsButtons ?? true;
					NightModeEnabled = (bool?)obj.NightModeEnabled ?? true;
					UseSystemTheme = (bool?)obj.UseSystemTheme ?? true;
					Design = List[(string)obj.Design];
				}
			}
			catch { }
			loadIdentifier.Enable();
		}

		public static void Save()
		{
			loadIdentifier.Disable();
			try
			{ ISave.Save(new { Design = design.ToString(), Custom, NightModeEnabled, UseSystemTheme, WindowsButtons }, "DesignMode.tf", appName: "SlickUI"); }
			catch { }
			loadIdentifier.Enable();
		}

		public static void ResetCustomTheme()
		{
			Custom = new FormDesign("Custom", -1, FormDesignType.None);
		}

		public static void SetCustomBaseDesign(FormDesign design)
		{
			if (IsCustomEligible())
			{
				Custom.Type = design.Type;
				Custom.ID = design.ID;
			}
			else
			{
				Custom = new FormDesign("Custom", design.ID, design.Type, true)
				{
					BackColor = design.BackColor,
					ForeColor = design.ForeColor,
					ButtonColor = design.ButtonColor,
					ButtonForeColor = design.ButtonForeColor,
					AccentColor = design.AccentColor,
					MenuColor = design.MenuColor,
					MenuForeColor = design.MenuForeColor,
					LabelColor = design.LabelColor,
					InfoColor = design.InfoColor,
					ActiveColor = design.ActiveColor,
					ActiveForeColor = design.ActiveForeColor,
					RedColor = design.RedColor,
					GreenColor = design.GreenColor,
					YellowColor = design.YellowColor,
					IconColor = design.IconColor
				};
			}
		}

		public static void ForceRefresh()
		{
			DesignChanged?.Invoke(Design);
		}

		[DllImport("user32.dll")]
		private static extern bool SetSysColors(int cElements, int[] lpaElements, uint[] lpaRgbValues);

		private static void ChangeSelectColour()
		{
			const int COLOR_HIGHLIGHT = 13;
			const int COLOR_HIGHLIGHTTEXT = 14;
			// You will have to set the HighlightText colour if you want to change that as well.

			//array of elements to change
			int[] elements = { COLOR_HIGHLIGHT, COLOR_HIGHLIGHTTEXT };

			var colours = new List<uint>
			{
				(uint)ColorTranslator.ToWin32(FormDesign.Design.ActiveColor),
				(uint)ColorTranslator.ToWin32(FormDesign.Design.ActiveForeColor)
			};

			//set the desktop color using p/invoke
			SetSysColors(elements.Length, elements, colours.ToArray());
		}

		#endregion Statics

		#region Overrides

		public override bool Equals(object obj)
		{
			return obj is FormDesign design &&
					 ID == design.ID &&
					 Type == design.Type &&
					 Name == design.Name &&
					 EqualityComparer<Color>.Default.Equals(BackColor, design.BackColor) &&
					 EqualityComparer<Color>.Default.Equals(ForeColor, design.ForeColor) &&
					 EqualityComparer<Color>.Default.Equals(AccentColor, design.AccentColor) &&
					 EqualityComparer<Color>.Default.Equals(MenuColor, design.MenuColor) &&
					 EqualityComparer<Color>.Default.Equals(LabelColor, design.LabelColor) &&
					 EqualityComparer<Color>.Default.Equals(InfoColor, design.InfoColor) &&
					 EqualityComparer<Color>.Default.Equals(ActiveColor, design.ActiveColor) &&
					 EqualityComparer<Color>.Default.Equals(ActiveForeColor, design.ActiveForeColor) &&
					 EqualityComparer<Color>.Default.Equals(RedColor, design.RedColor) &&
					 EqualityComparer<Color>.Default.Equals(GreenColor, design.GreenColor) &&
					 EqualityComparer<Color>.Default.Equals(YellowColor, design.YellowColor) &&
					 EqualityComparer<Color>.Default.Equals(IconColor, design.IconColor);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return Name;
		}

		#endregion Overrides
	}
}