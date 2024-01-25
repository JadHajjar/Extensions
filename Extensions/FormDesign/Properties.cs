using Newtonsoft.Json;

using System.Drawing;

namespace Extensions;

public partial class FormDesign : IFormDesign
{
	public string Name { get; set; }

	public Color BackColor { get; set; }
	public Color ForeColor { get; set; }
	public Color ButtonColor { get; set; }
	public Color ButtonForeColor { get; set; }
	public Color AccentColor { get; set; }
	public Color MenuColor { get; set; }
	public Color MenuForeColor { get; set; }
	public Color LabelColor { get; set; }
	public Color InfoColor { get; set; }
	public Color ActiveColor { get; set; }
	public Color ActiveForeColor { get; set; }
	public Color RedColor { get; set; }
	public Color GreenColor { get; set; }
	public Color YellowColor { get; set; }
	public Color IconColor { get; set; }

	[JsonIgnore]
	public bool IsDarkTheme => (BackColor.R * 0.299) + (BackColor.G * 0.587) + (BackColor.B * 0.114) <= 186;

	[JsonIgnore]
	public Color AccentBackColor => BackColor.Tint(Lum: IsDarkTheme ? 3 : -3);

	[JsonIgnore]
	public bool Temporary { get; set; }

	[JsonIgnore]
	public FormDesign DarkMode
	{
		get
		{
			if (IsDarkTheme)
			{
				return this;
			}

			if (darkMode != null)
			{
				return darkMode;
			}

			return darkMode = new FormDesign(Name, true)
			{
				BackColor = Midnight.BackColor.Tint(BackColor, -2.5F, -10),
				MenuColor = Midnight.MenuColor.Tint(MenuColor, -5F, -12),
				ActiveColor = ActiveColor.Tint(null, 0, -25),
				GreenColor = GreenColor.Tint(null, 0, -15),
				YellowColor = YellowColor.Tint(null, 0, -15),
				RedColor = RedColor.Tint(null, 0, -15),

				ActiveForeColor = Midnight.ActiveForeColor.Tint(ActiveForeColor, Sat: -20),
				ForeColor = Midnight.ForeColor.Tint(ForeColor, Sat: -20),
				ButtonColor = Midnight.ButtonColor.Tint(ButtonColor, -2F, -10),
				ButtonForeColor = Midnight.ButtonForeColor.Tint(ButtonForeColor, Sat: -20),
				AccentColor = Midnight.AccentColor.Tint(MenuColor, -25F, Sat: -70),
				MenuForeColor = Midnight.MenuForeColor.Tint(Sat: -100),
				LabelColor = Midnight.LabelColor.Tint(LabelColor, Sat: -20),
				InfoColor = Midnight.InfoColor.Tint(InfoColor, Sat: -15),
				IconColor = Midnight.IconColor.Tint(IconColor, Sat: -10)
			};
		}
		set => darkMode = value;
	}

	[JsonIgnore]
	public FormDesign LightMode
	{
		get
		{
			if (!IsDarkTheme)
			{
				return this;
			}

			if (lightMode != null)
			{
				return lightMode;
			}

			return lightMode = new FormDesign(Name, true)
			{
				BackColor = Modern.BackColor.Tint(BackColor, 0, -5),
				MenuColor = Modern.MenuColor.Tint(MenuColor, 0, -6),
				ActiveColor = ActiveColor.Tint(null, 4, -2),
				GreenColor = GreenColor.Tint(null, -2, -3),
				YellowColor = YellowColor.Tint(null, -2, -5),
				RedColor = RedColor.Tint(null, -2, -3),

				ActiveForeColor = Modern.ActiveForeColor.Tint(ActiveForeColor),
				ForeColor = Modern.ForeColor.Tint(ActiveColor),
				ButtonColor = Modern.ButtonColor.Tint(ButtonColor, 2F, 5),
				ButtonForeColor = Modern.ButtonForeColor.Tint(ButtonForeColor, Sat: -10),
				AccentColor = Modern.AccentColor.Tint(AccentColor, -5F, Sat: 5),
				MenuForeColor = Modern.MenuForeColor.Tint(MenuForeColor),
				LabelColor = Modern.LabelColor.Tint(LabelColor, Sat: -10),
				InfoColor = Modern.InfoColor.Tint(InfoColor, Sat: -5),
				IconColor = Modern.IconColor.Tint(IconColor, Sat: 0)
			};
		}
		set => lightMode = value;
	}

	private FormDesign darkMode;
	private FormDesign lightMode;

	public FormDesign()
	{

	}
}