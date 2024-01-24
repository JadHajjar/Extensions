using Newtonsoft.Json;

using System;
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
			if (darkMode != null)
			{
				return darkMode;
			}

			return darkMode = new FormDesign(Name, true)
			{
				BackColor = Midnight.BackColor.Tint(MenuColor, -2.5F, -10),
				MenuColor = Midnight.MenuColor.Tint(MenuColor, -5F, -12),
				ActiveColor = ActiveColor.Tint(null, 0, -25),
				GreenColor = GreenColor.Tint(null, 0, -15),
				YellowColor = YellowColor.Tint(null, 0, -15),
				RedColor = RedColor.Tint(null, 0, -15),

				ActiveForeColor = Midnight.ActiveForeColor.Tint(Sat: -100),
				ForeColor = Midnight.ForeColor.Tint(Sat: -100),
				ButtonColor = Midnight.ButtonColor.Tint(MenuColor, -2F, -10),
				ButtonForeColor = Midnight.ButtonForeColor.Tint(Sat: -100),
				AccentColor = Midnight.AccentColor.Tint(MenuColor, -25F, Sat: -70),
				MenuForeColor = Midnight.MenuForeColor.Tint(Sat: -100),
				LabelColor = Midnight.LabelColor.Tint(ActiveColor, Sat: -40),
				InfoColor = Midnight.InfoColor.Tint(ActiveColor, Sat: -30),
				IconColor = Midnight.IconColor.Tint(ActiveColor, Sat: -20)
			};
		}
		set => darkMode = value;
	}

	private FormDesign darkMode;

    public FormDesign()
    {
        
    }
}