using System.Drawing;

namespace Extensions
{
	public enum ColorStyle
	{
		Active,
		Icon,
		Green,
		Red,
		Yellow,
		Text
	}

	public static class ColorStyleExtemsions
	{
		public static Color GetColor(this ColorStyle style)
		{
			switch (style)
			{
				case ColorStyle.Active:
					return FormDesign.Design.ActiveColor;

				case ColorStyle.Text:
					return FormDesign.Design.ForeColor;

				case ColorStyle.Icon:
					return FormDesign.Design.IconColor;

				case ColorStyle.Green:
					return FormDesign.Design.GreenColor;

				case ColorStyle.Red:
					return FormDesign.Design.RedColor;

				case ColorStyle.Yellow:
					return FormDesign.Design.YellowColor;

				default:
					return FormDesign.Design.ActiveColor;
			}
		}

		public static Color GetBackColor(this ColorStyle style)
		{
			switch (style)
			{
				case ColorStyle.Active:
					return FormDesign.Design.ActiveForeColor;

				case ColorStyle.Text:
					return FormDesign.Design.BackColor;

				case ColorStyle.Icon:
					return FormDesign.Design.BackColor;

				case ColorStyle.Green:
					return FormDesign.Design.MenuColor;

				case ColorStyle.Red:
					return FormDesign.Design.BackColor;

				case ColorStyle.Yellow:
					return FormDesign.Design.MenuColor;

				default:
					return FormDesign.Design.ActiveForeColor;
			}
		}
	}
}