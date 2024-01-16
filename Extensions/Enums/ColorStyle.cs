using System.Drawing;

namespace Extensions;

public enum ColorStyle
{
	Active,
	Icon,
	Green,
	Red,
	Orange,
	Yellow,
	Text
}

public static class ColorStyleExtensions
{
	public static Color GetColor(this ColorStyle style)
	{
		return style switch
		{
			ColorStyle.Active => FormDesign.Design.ActiveColor,
			ColorStyle.Text => FormDesign.Design.ForeColor,
			ColorStyle.Icon => FormDesign.Design.IconColor,
			ColorStyle.Green => FormDesign.Design.GreenColor,
			ColorStyle.Red => FormDesign.Design.RedColor,
			ColorStyle.Orange => FormDesign.Design.RedColor.MergeColor(FormDesign.Design.YellowColor),
			ColorStyle.Yellow => FormDesign.Design.YellowColor,
			_ => FormDesign.Design.ActiveColor,
		};
	}

	public static Color GetBackColor(this ColorStyle style)
	{
		return style switch
		{
			ColorStyle.Active => FormDesign.Design.ActiveForeColor,
			ColorStyle.Text => FormDesign.Design.BackColor,
			ColorStyle.Icon => FormDesign.Design.BackColor,
			ColorStyle.Green => FormDesign.Design.MenuColor,
			ColorStyle.Red => FormDesign.Design.BackColor,
			ColorStyle.Orange => FormDesign.Design.BackColor,
			ColorStyle.Yellow => FormDesign.Design.MenuColor,
			_ => FormDesign.Design.ActiveForeColor,
		};
	}
}