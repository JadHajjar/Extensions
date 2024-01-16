using Newtonsoft.Json;

using System.Drawing;

namespace Extensions;

public partial class FormDesign
{
	public int ID { get; private set; }
	public string Name { get; private set; }
	public FormDesignType Type { get; private set; }

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
	public Color AccentBackColor => BackColor.Tint(Lum: Type.If(FormDesignType.Dark, 3, -3));

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

			return darkMode = new FormDesign(Name, ID, FormDesignType.Dark, true)
			{
				BackColor = Midnight.BackColor.Tint(ActiveColor, -2F, -50),
				MenuColor = Midnight.MenuColor.Tint(ActiveColor, -2.5F, -50),
				ActiveColor = ActiveColor.Tint(null, 0, -25),
				GreenColor = GreenColor.Tint(null, 0, -15),
				YellowColor = YellowColor.Tint(null, 0, -15),
				RedColor = RedColor.Tint(null, 0, -15),

				ActiveForeColor = Midnight.ActiveForeColor.Tint(Sat: -100),
				ForeColor = Midnight.ForeColor.Tint(Sat: -100),
				ButtonColor = Midnight.ButtonColor.Tint(ActiveColor, Sat: -20),
				ButtonForeColor = Midnight.ButtonForeColor.Tint(Sat: -100),
				AccentColor = Midnight.AccentColor.Tint(ActiveColor, -25F, Sat: -50),
				MenuForeColor = Midnight.MenuForeColor.Tint(Sat: -100),
				LabelColor = Midnight.LabelColor.Tint(ActiveColor, Sat: -20),
				InfoColor = Midnight.InfoColor.Tint(ActiveColor, Sat: -20),
				IconColor = Midnight.IconColor.Tint(ActiveColor, Sat: -20)
			};
		}
		set => darkMode = value;
	}

	private FormDesign darkMode;
}