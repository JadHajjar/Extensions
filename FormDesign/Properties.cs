using Newtonsoft.Json;

using System.Drawing;

namespace Extensions
{
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
					return darkMode;
					
				return darkMode = new FormDesign(Name, ID, FormDesignType.Dark, true)
				{
					BackColor = Dark.BackColor.Tint(ActiveColor, 0.25F, -50),
					MenuColor = Dark.MenuColor.Tint(ActiveColor, 0.5F, -50),
					ActiveColor = ActiveColor.Tint(null, 0, -25),
					GreenColor = GreenColor.Tint(null, 0, -15),
					YellowColor = YellowColor.Tint(null, 0, -15),
					RedColor = RedColor.Tint(null, 0, -15),

					ActiveForeColor = Dark.ActiveForeColor.Tint(Sat: -100),
					ForeColor = Dark.ForeColor.Tint(Sat: -100),
					ButtonColor = Dark.ButtonColor.Tint(ActiveColor, Sat: -20),
					ButtonForeColor = Dark.ButtonForeColor.Tint(Sat: -100),
					AccentColor = Dark.AccentColor.Tint(ActiveColor, Sat: -20),
					MenuForeColor = Dark.MenuForeColor.Tint(Sat: -100),
					LabelColor = Dark.LabelColor.Tint(ActiveColor, Sat: -20),
					InfoColor = Dark.InfoColor.Tint(ActiveColor, Sat: -20),
					IconColor = Dark.IconColor.Tint(ActiveColor, Sat: -20)
				};
			}
			set => darkMode = value;
		}

		private FormDesign darkMode;
	}
}