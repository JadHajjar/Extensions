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
				if (darkMode == null)
					darkMode = new FormDesign(Name, ID, FormDesignType.Dark, true)
					{
						BackColor = Dark.BackColor.Tint(ActiveColor, -3),
						MenuColor = Dark.MenuColor.Tint(ActiveColor, -6),
						ActiveColor = ActiveColor,
						GreenColor = GreenColor,
						YellowColor = YellowColor,
						RedColor = RedColor,

						ActiveForeColor = Dark.ActiveForeColor.Tint(ActiveColor),
						ForeColor = Dark.ForeColor.Tint(ActiveColor),
						ButtonColor = Dark.ButtonColor.Tint(ActiveColor),
						ButtonForeColor = Dark.ButtonForeColor.Tint(ActiveColor),
						AccentColor = Dark.AccentColor.Tint(ActiveColor),
						MenuForeColor = Dark.MenuForeColor.Tint(ActiveColor),
						LabelColor = Dark.LabelColor.Tint(ActiveColor),
						InfoColor = Dark.InfoColor.Tint(ActiveColor),
						IconColor = Dark.IconColor.Tint(ActiveColor)
					};

				return darkMode;
			}
			set => darkMode = value;
		}

		private FormDesign darkMode;
	}
}