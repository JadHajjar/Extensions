using System.Drawing;

namespace Extensions;

public interface IFormDesign
{
	string Name { get; }
	bool IsDarkTheme { get; }

	Color BackColor { get; set; }
	Color ForeColor { get; set; }
	Color ButtonColor { get; set; }
	Color ButtonForeColor { get; set; }
	Color AccentColor { get; set; }
	Color MenuColor { get; set; }
	Color MenuForeColor { get; set; }
	Color LabelColor { get; set; }
	Color InfoColor { get; set; }
	Color ActiveColor { get; set; }
	Color ActiveForeColor { get; set; }
	Color RedColor { get; set; }
	Color GreenColor { get; set; }
	Color YellowColor { get; set; }
	Color IconColor { get; set; }
}