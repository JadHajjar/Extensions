using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Extensions
{
	public enum FormDesignType { None, Dark, Light }

	public partial class FormDesign
	{
		public delegate void DesignEventHandler(FormDesign design);

		public static event DesignEventHandler DesignChanged;

		public static bool NightModeEnabled { get; set; }
		public static bool NightMode { get; private set; } = !DateTime.Now.Hour.IsWithin(7, 20);

		public FormDesign(string name, int id, FormDesignType t, bool temp = false)
		{
			Name = name; ID = id; Type = t; Temporary = temp;
		}

		#region Current Design

		private static FormDesign design = Modern;

		public static Image Loader
		{
			get
			{
				switch (design.ID)
				{
					case 0: return Properties.Resources.Loader_0;
					case 1: return Properties.Resources.Loader_1;
					case 2: return Properties.Resources.Loader_2;
					case 3: return Properties.Resources.Loader_3;
					case 4: return Properties.Resources.Loader_4;
					case 5: case 6: return Properties.Resources.Loader_5;
					default: return Properties.Resources.Loader_0;
				}
			}
		}

		public static FormDesign Design
		{
			get
			{
				if (NightMode == SunManager.SunTime.Contains(DateTime.Now))
				{
					NightMode = !NightMode;
					ForceRefresh();
				}

				if (NightModeEnabled && NightMode)
					return design.DarkMode;

				return design;
			}

			private set
			{
				if (value != design)
				{
					design = value;
					DesignChanged?.Invoke(Design);
					if (!loadIdentifier.Disabled)
						Save();
				}
			}
		}

		#endregion Current Design

		#region Statics

		private static readonly DisableIdentifier loadIdentifier = new DisableIdentifier();

		public static void Initialize(Form form, DesignEventHandler handler = null)
		{
			if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Shared")))
				Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Shared"));

			Load();

			if (handler != null)
			{
				DesignChanged += handler;
				handler(Design);
			}

			StartListener(form);

			setUpSystemColors();
		}

		public static void Switch() => Design = List.Next(design) ?? List[0];

		public static void Switch(FormDesign newDesign, bool forceSave = false, bool forceRefresh = false)
		{
			if (newDesign != null)
			{
				forceRefresh |= design != newDesign;
				forceSave |= forceRefresh && newDesign.Name != "Custom";
				design = newDesign;

				if (forceRefresh)
					ForceRefresh();

				if (forceSave && !newDesign.Temporary)
					Save();
			}
		}

		public static bool IsCustomEligible()
			=> Custom.ID != -1 && Custom.BackColor.A != 0;

		public static void StartListener(Form form)
		{
			var watcher = new FileSystemWatcher
			{
				Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Shared"),
				NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
				Filter = "DesignMode.tf",
				EnableRaisingEvents = true
			};

			watcher.Changed += (s, e) =>
			{
				if (!loadIdentifier.Disabled)
					form.TryInvoke(Load);
			};
		}

		public static void Load()
		{
			loadIdentifier.Disable();
			try
			{
				var obj = ISave.LoadRaw("DesignMode.tf", "Shared");

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

					NightModeEnabled = (bool?)obj.NightModeEnabled ?? true;
					Design = List[(string)obj.Design];
				}
			}
			catch { }
			loadIdentifier.Enable();
		}

		public static void Save()
		{
			loadIdentifier.Disable();
			try { ISave.Save(new { Design = design.ToString(), Custom, NightModeEnabled }, "DesignMode.tf", appName: "Shared"); } catch { }
			loadIdentifier.Enable();
		}

		public static void ResetCustomTheme() => Custom = new FormDesign("Custom", -1, FormDesignType.None);

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

		public static void ForceRefresh() => DesignChanged?.Invoke(Design);

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

		public override bool Equals(object obj) => obj is FormDesign design &&
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

		public override int GetHashCode() => base.GetHashCode();

		public override string ToString() => Name;

		#endregion Overrides

		#region System

		private static void setUpSystemColors() => Assembly.GetAssembly(typeof(Color))
				.GetType("System.Drawing.KnownColorTable")
				.GetField("colorTable", BindingFlags.Static | BindingFlags.NonPublic)
				.SetValue(null, getColorTable());

		private static int[] getColorTable()
		{
			var array = new int[175];

			array[13] = Design.ActiveColor.ToArgb();
			array[14] = Design.ActiveForeColor.ToArgb();

			array[1] = SystemColorToArgb(10);
			array[2] = SystemColorToArgb(2);
			array[3] = SystemColorToArgb(9);
			array[4] = SystemColorToArgb(12);
			array[168] = SystemColorToArgb(15);
			array[169] = SystemColorToArgb(20);
			array[170] = SystemColorToArgb(16);
			array[5] = SystemColorToArgb(15);
			array[6] = SystemColorToArgb(16);
			array[7] = SystemColorToArgb(21);
			array[8] = SystemColorToArgb(22);
			array[9] = SystemColorToArgb(20);
			array[10] = SystemColorToArgb(18);
			array[11] = SystemColorToArgb(1);
			array[171] = SystemColorToArgb(27);
			array[172] = SystemColorToArgb(28);
			array[12] = SystemColorToArgb(17);
			array[15] = SystemColorToArgb(26);
			array[16] = SystemColorToArgb(11);
			array[17] = SystemColorToArgb(3);
			array[18] = SystemColorToArgb(19);
			array[19] = SystemColorToArgb(24);
			array[20] = SystemColorToArgb(23);
			array[21] = SystemColorToArgb(4);
			array[173] = SystemColorToArgb(30);
			array[174] = SystemColorToArgb(29);
			array[22] = SystemColorToArgb(7);
			array[23] = SystemColorToArgb(0);
			array[24] = SystemColorToArgb(5);
			array[25] = SystemColorToArgb(6);
			array[26] = SystemColorToArgb(8);
			array[27] = 16777215;
			array[28] = -984833;
			array[29] = -332841;
			array[30] = -16711681;
			array[31] = -8388652;
			array[32] = -983041;
			array[33] = -657956;
			array[34] = -6972;
			array[35] = -16777216;
			array[36] = -5171;
			array[37] = -16776961;
			array[38] = -7722014;
			array[39] = -5952982;
			array[40] = -2180985;
			array[41] = -10510688;
			array[42] = -8388864;
			array[43] = -2987746;
			array[44] = -32944;
			array[45] = -10185235;
			array[46] = -1828;
			array[47] = -2354116;
			array[48] = -16711681;
			array[49] = -16777077;
			array[50] = -16741493;
			array[51] = -4684277;
			array[52] = -5658199;
			array[53] = -16751616;
			array[54] = -4343957;
			array[55] = -7667573;
			array[56] = -11179217;
			array[57] = -29696;
			array[58] = -6737204;
			array[59] = -7667712;
			array[60] = -1468806;
			array[61] = -7357301;
			array[62] = -12042869;
			array[63] = -13676721;
			array[64] = -16724271;
			array[65] = -7077677;
			array[66] = -60269;
			array[67] = -16728065;
			array[68] = -9868951;
			array[69] = -14774017;
			array[70] = -5103070;
			array[71] = -1296;
			array[72] = -14513374;
			array[73] = -65281;
			array[74] = -2302756;
			array[75] = -460545;
			array[76] = -10496;
			array[77] = -2448096;
			array[78] = -8355712;
			array[79] = -16744448;
			array[80] = -5374161;
			array[81] = -983056;
			array[82] = -38476;
			array[83] = -3318692;
			array[84] = -11861886;
			array[85] = -16;
			array[86] = -989556;
			array[87] = -1644806;
			array[88] = -3851;
			array[89] = -8586240;
			array[90] = -1331;
			array[91] = -5383962;
			array[92] = -1015680;
			array[93] = -2031617;
			array[94] = -329006;
			array[95] = -2894893;
			array[96] = -7278960;
			array[97] = -18751;
			array[98] = -24454;
			array[99] = -14634326;
			array[100] = -7876870;
			array[101] = -8943463;
			array[102] = -5192482;
			array[103] = -32;
			array[104] = -16711936;
			array[105] = -13447886;
			array[106] = -331546;
			array[107] = -65281;
			array[108] = -8388608;
			array[109] = -10039894;
			array[110] = -16777011;
			array[111] = -4565549;
			array[112] = -7114533;
			array[113] = -12799119;
			array[114] = -8689426;
			array[115] = -16713062;
			array[116] = -12004916;
			array[117] = -3730043;
			array[118] = -15132304;
			array[119] = -655366;
			array[120] = -6943;
			array[121] = -6987;
			array[122] = -8531;
			array[123] = -16777088;
			array[124] = -133658;
			array[125] = -8355840;
			array[126] = -9728477;
			array[127] = -23296;
			array[128] = -47872;
			array[129] = -2461482;
			array[130] = -1120086;
			array[131] = -6751336;
			array[132] = -5247250;
			array[133] = -2396013;
			array[134] = -4139;
			array[135] = -9543;
			array[136] = -3308225;
			array[137] = -16181;
			array[138] = -2252579;
			array[139] = -5185306;
			array[140] = -8388480;
			array[141] = -65536;
			array[142] = -4419697;
			array[143] = -12490271;
			array[144] = -7650029;
			array[145] = -360334;
			array[146] = -744352;
			array[147] = -13726889;
			array[148] = -2578;
			array[149] = -6270419;
			array[150] = -4144960;
			array[151] = -7876885;
			array[152] = -9807155;
			array[153] = -9404272;
			array[154] = -1286;
			array[155] = -16711809;
			array[156] = -12156236;
			array[157] = -2968436;
			array[158] = -16744320;
			array[159] = -2572328;
			array[160] = -40121;
			array[161] = -12525360;
			array[162] = -1146130;
			array[163] = -663885;
			array[164] = -1;
			array[165] = -657931;
			array[166] = -256;
			array[167] = -6632142;

			return array;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetSysColor(int nIndex);

		// Token: 0x0600049F RID: 1183 RVA: 0x00013749 File Offset: 0x00012749
		private static int SystemColorToArgb(int index) => FromWin32Value(GetSysColor(index));

		// Token: 0x060004A0 RID: 1184 RVA: 0x00013756 File Offset: 0x00012756
		private static int Encode(int alpha, int red, int green, int blue) => red << 16 | green << 8 | blue | alpha << 24;

		// Token: 0x060004A1 RID: 1185 RVA: 0x00013767 File Offset: 0x00012767
		private static int FromWin32Value(int value) => Encode(255, value & 255, value >> 8 & 255, value >> 16 & 255);

		#endregion System
	}
}