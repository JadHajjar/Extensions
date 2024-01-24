using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extensions;
public class DesignSettings  : ISave
{
	public string Design { get; set; }
	public FormDesign Custom { get; set; }
	public bool NightModeEnabled { get; set; } = true;
	public bool UseSystemTheme { get; set; } = true;
	public bool WindowsButtons { get; set; } = true;
}