using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace Extensions;

public static partial class WinExtensionClass
{
	private const int WM_SETREDRAW = 11;
	private static bool? isadministrator;
	private static readonly DateTime? _lastCacheCleanup;

	/// <summary>
	/// Checks if the App currently has Administrator Privileges
	/// </summary>
	public static bool IsAdministrator => (bool)(bool?)(isadministrator ??= new WindowsPrincipal(WindowsIdentity.GetCurrent())
			.IsInRole(WindowsBuiltInRole.Administrator));

	/// <summary>
	/// Associates a <see cref="ToolTip"/> with a <paramref name="control"/> and all of its children
	/// </summary>
	public static void AdvancedSetTooltip(this ToolTip toolTip, Control control, string tip)
	{
		toolTip.SetToolTip(control, tip);

		foreach (Control child in control.Controls)
		{
			AdvancedSetTooltip(toolTip, child, tip);
		}
	}

	public static Rectangle Align(this Rectangle rect, Size size, ContentAlignment alignment)
	{
		return alignment switch
		{
			ContentAlignment.TopLeft => new Rectangle(rect.X, rect.Y, size.Width, size.Height),
			ContentAlignment.TopCenter => new Rectangle(rect.X + ((rect.Width - size.Width) / 2), rect.Y, size.Width, size.Height),
			ContentAlignment.TopRight => new Rectangle(rect.X + rect.Width - size.Width, rect.Y, size.Width, size.Height),
			ContentAlignment.MiddleLeft => new Rectangle(rect.X, rect.Y + ((rect.Height - size.Height) / 2), size.Width, size.Height),
			ContentAlignment.MiddleCenter => new Rectangle(rect.X + ((rect.Width - size.Width) / 2), rect.Y + ((rect.Height - size.Height) / 2), size.Width, size.Height),
			ContentAlignment.MiddleRight => new Rectangle(rect.X + rect.Width - size.Width, rect.Y + ((rect.Height - size.Height) / 2), size.Width, size.Height),
			ContentAlignment.BottomLeft => new Rectangle(rect.X, rect.Y + rect.Height - size.Height, size.Width, size.Height),
			ContentAlignment.BottomCenter => new Rectangle(rect.X + ((rect.Width - size.Width) / 2), rect.Y + rect.Height - size.Height, size.Width, size.Height),
			ContentAlignment.BottomRight => new Rectangle(rect.X + rect.Width - size.Width, rect.Y + rect.Height - size.Height, size.Width, size.Height),
			_ => rect,
		};
	}

	public static Rectangle ClipTo(this Rectangle rectangle, int height)
	{
		return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, height);
	}

	public static Rectangle InvertPad(this Rectangle rect, Padding padding)
	{
		return Pad(rect, -padding.Left, -padding.Top, -padding.Right, -padding.Bottom);
	}

	public static Rectangle Pad(this Rectangle rect, Padding padding)
	{
		return Pad(rect, padding.Left, padding.Top, padding.Right, padding.Bottom);
	}

	public static Rectangle Pad(this Rectangle rect, int all)
	{
		return Pad(rect, all, all, all, all);
	}

	public static Rectangle Pad(this Rectangle rect, int left, int top, int right, int bottom)
	{
		return new Rectangle(rect.X + left, rect.Y + top, rect.Width - left - right, rect.Height - top - bottom);
	}

	public static RectangleF Pad(this RectangleF rect, Padding padding)
	{
		return Pad(rect, padding.Left, padding.Top, padding.Right, padding.Bottom);
	}

	public static RectangleF Pad(this RectangleF rect, float all)
	{
		return Pad(rect, all, all, all, all);
	}

	public static RectangleF Pad(this RectangleF rect, float left, float top, float right, float bottom)
	{
		return new RectangleF(rect.X + left, rect.Y + top, rect.Width - left - right, rect.Height - top - bottom);
	}

	public static Rectangle Margin(this Rectangle rect, Padding padding)
	{
		return Margin(rect, padding.Left, padding.Top, padding.Right, padding.Bottom);
	}

	public static Rectangle Margin(this Rectangle rect, int all)
	{
		return Margin(rect, all, all, all, all);
	}

	public static Rectangle Margin(this Rectangle rect, int left, int top, int right, int bottom)
	{
		return new Rectangle(rect.X + left, rect.Y + top, rect.Width + left + right, rect.Height + top + bottom);
	}

	public static RectangleF Margin(this RectangleF rect, Padding padding)
	{
		return Margin(rect, padding.Left, padding.Top, padding.Right, padding.Bottom);
	}

	public static RectangleF Margin(this RectangleF rect, float all)
	{
		return Margin(rect, all, all, all, all);
	}

	public static RectangleF Margin(this RectangleF rect, float left, float top, float right, float bottom)
	{
		return new RectangleF(rect.X + left, rect.Y + top, rect.Width + left + right, rect.Height + top + bottom);
	}

	public static Point Center(this Rectangle rect, Size controlSize)
	{
		return new Point(((rect.Width - controlSize.Width) / 2) + rect.X, ((rect.Height - controlSize.Height) / 2) + rect.Y);
	}

	public static Point Center(this Rectangle rect, int width, int height)
	{
		return new Point(((rect.Width - width) / 2) + rect.X, ((rect.Height - height) / 2) + rect.Y);
	}

	public static Point Center(this Size rect, Size controlSize)
	{
		return new Point((rect.Width - controlSize.Width) / 2, (rect.Height - controlSize.Height) / 2);
	}

	public static PointF Center(this RectangleF rect, SizeF controlSize)
	{
		return new PointF(((rect.Width - controlSize.Width) / 2) + rect.X, ((rect.Height - controlSize.Height) / 2) + rect.Y);
	}

	public static PointF Center(this SizeF rect, SizeF controlSize)
	{
		return new PointF((rect.Width - controlSize.Width) / 2, (rect.Height - controlSize.Height) / 2);
	}

	public static Rectangle CenterR(this Rectangle rect, Size controlSize)
	{
		return new Rectangle(new Point(((rect.Width - controlSize.Width) / 2) + rect.X, ((rect.Height - controlSize.Height) / 2) + rect.Y), controlSize);
	}

	public static Rectangle CenterR(this Rectangle rect, int width, int height)
	{
		return new Rectangle(new Point(((rect.Width - width) / 2) + rect.X, ((rect.Height - height) / 2) + rect.Y), new Size(width, height));
	}

	public static Rectangle CenterR(this Size rect, Size controlSize)
	{
		return new Rectangle(new Point((rect.Width - controlSize.Width) / 2, (rect.Height - controlSize.Height) / 2), controlSize);
	}

	public static RectangleF CenterR(this RectangleF rect, SizeF controlSize)
	{
		return new RectangleF(new PointF(((rect.Width - controlSize.Width) / 2) + rect.X, ((rect.Height - controlSize.Height) / 2) + rect.Y), controlSize);
	}

	public static RectangleF CenterR(this SizeF rect, SizeF controlSize)
	{
		return new RectangleF(new PointF((rect.Width - controlSize.Width) / 2, (rect.Height - controlSize.Height) / 2), controlSize);
	}

	/// <summary>
	/// Removes controls from the collection
	/// </summary>
	/// <param name="dispose">Completely Dispose of the controls</param>
	/// <param name="testFunc">Only remove controls that satisfy this method</param>
	public static void Clear(this Control.ControlCollection controls, bool dispose, Func<Control, bool> testFunc = null)
	{
		for (var ix = controls.Count - 1; ix >= 0; --ix)
		{
			if (testFunc == null || testFunc(controls[ix]))
			{
				if (dispose)
				{
					controls[ix].Dispose();
				}
				else
				{
					controls.RemoveAt(ix);
				}
			}
		}
	}

	public static IEnumerable<T> GetControls<T>(this Form form) where T : Control
	{
		return GetControls<T>(form);
	}

	public static IEnumerable<T> GetControls<T>(this Control control) where T : Control
	{
		if (control is T)
		{
			yield return control as T;
		}

		foreach (Control item in control.Controls)
		{
			foreach (var ctrl in GetControls<T>(item))
			{
				yield return ctrl;
			}
		}
	}

	/// <summary>
	/// Colors the <see cref="Image"/> of a <see cref="PictureBox"/> with the <paramref name="color"/>
	/// </summary>
	public static void Color(this PictureBox pictureBox, Color color)
	{
		pictureBox.Image = pictureBox.Image.Color(color);
	}

	/// <summary>
	/// Colors the <see cref="Image"/> with the <paramref name="color"/>
	/// </summary>
	public static Bitmap Color(this Image image, Color color, byte alpha = 255)
	{
		return (image as Bitmap).Color(color, alpha);
	}

	/// <summary>
	/// Colors the <see cref="Bitmap"/> with the <paramref name="color"/>
	/// </summary>
	public static Bitmap Color(this Bitmap bitmap, Color color, byte alpha = 255)
	{
		try
		{
			if (bitmap != null)
			{
				const int PIXEL_SIZE = 4;

				var bmd = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);

				for (var y = 0; y <= (bmd.Height - 1) * bmd.Stride; y += bmd.Stride)
				{
					for (var x = 0; x <= (bmd.Width - 1) * PIXEL_SIZE; x += PIXEL_SIZE)
					{
						Marshal.WriteByte(bmd.Scan0, y + x, color.B);
						Marshal.WriteByte(bmd.Scan0, y + x + 1, color.G);
						Marshal.WriteByte(bmd.Scan0, y + x + 2, color.R);

						if (alpha < 255)
						{
							var current = Marshal.ReadByte(bmd.Scan0, y + x + 3) * alpha / 255;
							Marshal.WriteByte(bmd.Scan0, y + x + 3, (byte)(current < 0 ? 0 : current));
						}
					}
				}

				bitmap.UnlockBits(bmd);
			}

			return bitmap;
		}
		catch
		{
			return null;
		}
	}

	public static Bitmap SafeColor(this Image image, Color color)
	{
		return (image as Bitmap).SafeColor(color);
	}

	public static Bitmap SafeColor(this Bitmap bitmap, Color color)
	{
		if (bitmap == null)
		{
			return null;
		}

		try
		{
			var W = bitmap.Width;
			var H = bitmap.Height;

			for (var i = 0; i < H; i++)
			{
				for (var j = 0; j < W; j++)
				{
					bitmap.SetPixel(j, i, System.Drawing.Color.FromArgb(bitmap.GetPixel(j, i).A, color));
				}
			}

			return bitmap;
		}
		catch
		{
			return null;
		}
	}

	public static Bitmap Alpha(this Image image, int alpha)
	{
		return (image as Bitmap).Alpha(alpha);
	}

	public static Bitmap Alpha(this Bitmap bitmap, int alpha)
	{
		try
		{
			if (bitmap != null && alpha.IsWithin(0, 255))
			{
				const int PIXEL_SIZE = 4;

				var bmd = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);

				for (var y = 0; y <= (bmd.Height - 1) * bmd.Stride; y += bmd.Stride)
				{
					for (var x = 0; x <= (bmd.Width - 1) * PIXEL_SIZE; x += PIXEL_SIZE)
					{
						var current = Marshal.ReadByte(bmd.Scan0, y + x + 3) * alpha / 255;
						Marshal.WriteByte(bmd.Scan0, y + x + 3, (byte)(current < 0 ? 0 : current));
					}
				}

				bitmap.UnlockBits(bmd);
			}

			return bitmap;
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Creates a <see cref="System.Drawing.Color"/> from a Hue, Saturation and Luminance combination
	/// </summary>
	public static Color ColorFromHSL(float hue, float saturation, float lightness)
	{
		var chroma = (1 - Math.Abs((2 * lightness) - 1)) * saturation;
		var huePrime = hue / 60.0f;
		var x = chroma * (1 - Math.Abs((huePrime % 2) - 1));

		float r1, g1, b1;
		if (huePrime >= 0 && huePrime < 1)
		{
			r1 = chroma;
			g1 = x;
			b1 = 0;
		}
		else if (huePrime >= 1 && huePrime < 2)
		{
			r1 = x;
			g1 = chroma;
			b1 = 0;
		}
		else if (huePrime >= 2 && huePrime < 3)
		{
			r1 = 0;
			g1 = chroma;
			b1 = x;
		}
		else if (huePrime >= 3 && huePrime < 4)
		{
			r1 = 0;
			g1 = x;
			b1 = chroma;
		}
		else if (huePrime >= 4 && huePrime < 5)
		{
			r1 = x;
			g1 = 0;
			b1 = chroma;
		}
		else // if (huePrime >= 5 && huePrime < 6)
		{
			r1 = chroma;
			g1 = 0;
			b1 = x;
		}

		var m = lightness - (chroma / 2.0);
		var r = (byte)((r1 + m) * 255);
		var g = (byte)((g1 + m) * 255);
		var b = (byte)((b1 + m) * 255);

		return System.Drawing.Color.FromArgb(r, g, b);
	}

	public static void RgbToHsl(this Color rgbColor, out float hue, out float saturation, out float lightness)
	{
		var r = rgbColor.R / 255.0f;
		var g = rgbColor.G / 255.0f;
		var b = rgbColor.B / 255.0f;

		var cMax = Math.Max(r, Math.Max(g, b));
		var cMin = Math.Min(r, Math.Min(g, b));
		var delta = cMax - cMin;

		// Calculate hue
		if (delta == 0)
		{
			hue = 0; // Undefined (monochromatic)
		}
		else if (cMax == r)
		{
			hue = 60 * ((g - b) / delta % 6);
		}
		else if (cMax == g)
		{
			hue = 60 * (((b - r) / delta) + 2);
		}
		else // cMax == b
		{
			hue = 60 * (((r - g) / delta) + 4);
		}

		// Calculate lightness
		lightness = (cMax + cMin) / 2;

		// Calculate saturation
		if (delta == 0)
		{
			saturation = 0;
		}
		else
		{
			saturation = delta / (1 - Math.Abs((2 * lightness) - 1));
		}

		// Ensure hue is non-negative
		hue = hue < 0 ? hue + 360 : hue;
	}

	/// <summary>
	/// Converts this <see cref="ListBox.SelectedObjectCollection"/> to an <see cref="IEnumerable{T}"/> using the conversion <paramref name="func"/>
	/// </summary>
	public static IEnumerable<T> Convert<T>(this ListBox.SelectedObjectCollection list, Func<object, T> func)
	{
		var l = new List<T>(list.Count);
		foreach (var item in list)
		{
			l.Add(func(item));
		}

		return l.AsEnumerable();
	}

	public static bool TryInvoke(this Control control, ExtensionClass.action action)
	{
		try
		{
			if (control?.InvokeRequired ?? false)
			{
				control.Invoke(new Action(action));
			}
			else
			{
				action();
			}
		}
		catch
		{
			return false;
		}

		return true;
	}

	public static bool TryBeginInvoke(this Control control, ExtensionClass.action action)
	{
		try
		{
			if (control?.IsHandleCreated ?? false)
			{
				control.BeginInvoke(new Action(() =>
				{
					try
					{
						action();
					}
					catch { }
				}));
			}
			else
			{
				action();
			}
		}
		catch
		{
			return false;
		}

		return true;
	}

	public static Control GetCurrentlyFocusedControl(this Control control)
	{
		if (control.Focused)
		{
			return control;
		}

		foreach (Control item in control.Controls)
		{
			var c = GetCurrentlyFocusedControl(item);
			if (c != null)
			{
				return c;
			}
		}

		return null;
	}

	public static void ResetFocus(this Control control)
	{
		if (control is IFirstFocus ff && ff?.FirstFocusedControl != null)
		{
			ff.FirstFocusedControl.Focus();
		}
		else if (control != null && !resetFocus(control))
		{
			control.Focus();
		}
	}

	private static bool resetFocus(Control control)
	{
		foreach (var item in control.Controls.Cast<Control>().OrderBy(x => x.TabIndex))
		{
			if (item.Controls.Count > 0 && resetFocus(item))
			{
				return true;
			}

			if (item.TabStop && item.Focus())
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Checks if a <see cref="Control"/> is ultimately visible to the User
	/// </summary>
	public static bool IsVisible(this Control control)
	{
		return !control.IsHandleCreated || (control.Visible && (control.Parent != null ? IsVisible(control.Parent) : control is Form));
	}

	/// <summary>
	/// Checks if an <see cref="Image"/> is an animated GIF
	/// </summary>
	public static bool IsAnimated(this Image img)
	{
		return img != null && img.GetFrameCount(new FrameDimension(img.FrameDimensionsList[0])) > 1;
	}

	/// <summary>
	/// Merges two Colors together
	/// </summary>
	public static Color MergeColor(this Color color, Color backColor, int Perc = 50)
	{
		if (backColor == null)
		{
			return color;
		}

		var R = (color.R * Perc / 100) + (backColor.R * (100 - Perc) / 100);
		var G = (color.G * Perc / 100) + (backColor.G * (100 - Perc) / 100);
		var B = (color.B * Perc / 100) + (backColor.B * (100 - Perc) / 100);
		return System.Drawing.Color.FromArgb(color.A, R, G, B);
	}

	/// <summary>
	/// Sorts the controls of a panel using a <paramref name="keySelector"/>
	/// </summary>
	public static void OrderBy<TKey>(this Panel panel, Func<Control, TKey> keySelector, bool suspendDrawing = true)
	{
		if (panel == null || panel.Controls == null)
		{
			return;
		}

		var controls = panel.Controls.Cast<Control>();

		var index = 0;
		if (suspendDrawing)
		{
			panel.SuspendDrawing();
		}

		foreach (var item in controls.OrderBy(keySelector))
		{
			item.TabIndex = index;
			panel.Controls.SetChildIndex(item, index++);
		}

		if (suspendDrawing)
		{
			panel.ResumeDrawing();
		}
	}

	/// <summary>
	/// Sorts the controls of a panel using a <paramref name="keySelector"/> in a descending order
	/// </summary>
	public static void OrderByDescending<TKey>(this Panel panel, Func<Control, TKey> keySelector, bool suspendDrawing = true)
	{
		if (panel == null || panel.Controls == null)
		{
			return;
		}

		var controls = panel.Controls.Cast<Control>();

		var index = 0;
		if (suspendDrawing)
		{
			panel.SuspendDrawing();
		}

		foreach (var item in controls.OrderByDescending(keySelector))
		{
			item.TabIndex = index;
			panel.Controls.SetChildIndex(item, index++);
		}

		if (suspendDrawing)
		{
			panel.ResumeDrawing();
		}
	}

	/// <summary>
	/// Sorts the controls of a panel using a <paramref name="keySelector"/>
	/// </summary>
	public static void OrderBy<TKey>(this Control.ControlCollection ctrls, Func<Control, TKey> keySelector, bool suspendDrawing = true)
	{
		if (ctrls == null)
		{
			return;
		}

		var controls = ctrls.Cast<Control>();

		if (controls.Count() > 0 && !controls.SequenceEqual(controls.OrderByDescending(keySelector)))
		{
			var panel = controls.First().Parent;

			var index = 0;
			if (suspendDrawing)
			{
				panel.SuspendDrawing();
			}

			foreach (var item in controls.OrderBy(keySelector))
			{
				item.TabIndex = index;
				panel.Controls.SetChildIndex(item, index++);
			}

			if (suspendDrawing)
			{
				panel.ResumeDrawing();
			}
		}
	}

	/// <summary>
	/// Sorts the controls of a panel using a <paramref name="keySelector"/> in a descending order
	/// </summary>
	public static void OrderByDescending<TKey>(this Control.ControlCollection ctrls, Func<Control, TKey> keySelector, bool suspendDrawing = true)
	{
		if (ctrls == null)
		{
			return;
		}

		var controls = ctrls.Cast<Control>();

		if (controls.Count() > 0 && !controls.SequenceEqual(controls.OrderByDescending(keySelector)))
		{
			var panel = controls.First().Parent;

			var index = 0;
			if (suspendDrawing)
			{
				panel.SuspendDrawing();
			}

			foreach (var item in controls.OrderByDescending(keySelector))
			{
				item.TabIndex = index;
				panel.Controls.SetChildIndex(item, index++);
			}

			if (suspendDrawing)
			{
				panel.ResumeDrawing();
			}
		}
	}

	public static void RecursiveClick(this Control control, EventHandler handler)
	{
		control.Click += handler;

		foreach (Control child in control.Controls)
		{
			RecursiveClick(child, handler);
		}
	}

	public static Color GetThemedAverageColor(this Bitmap bmp, Rectangle? rectangle = null)
	{
		var color = GetAverageColor(bmp, rectangle);

		return FormDesign.Design.BackColor.MergeColor(color, 50);
	}

	public static Color GetAverageColor(this Image bmp, Rectangle? rectangle = null)
	{
		return (bmp as Bitmap).GetAverageColor(rectangle);
	}

	public static Color GetAverageColor(this Bitmap bmp, Rectangle? rectangle = null)
	{
		var rect = rectangle ?? new Rectangle(Point.Empty, bmp.Size);
		var r = 0;
		var g = 0;
		var b = 0;

		//if (rectangle != null)
		//	rect = new Rectangle(Math.Max(0, rect.X), Math.Max(0, rect.Y), Math.Max(bmp.Width, rect.Width), Math.Max(bmp.Height, rect.Height));

		var total = 0;
		var xStep = Math.Max(1, rect.Width / 128);
		var yStep = Math.Max(1, rect.Height / 128);

		for (var x = rect.X; x < rect.X + rect.Width; x += xStep)
		{
			for (var y = rect.Y; y < rect.Y + rect.Height; y += yStep)
			{
				var clr = bmp.GetPixel(x, y);

				r += clr.R;
				g += clr.G;
				b += clr.B;

				total++;
			}
		}

		//Calculate average
		r /= total;
		g /= total;
		b /= total;

		return System.Drawing.Color.FromArgb(r, g, b);
	}

	public static Image Blur(this Image image, int? radius = null, bool doNotDispose = false, Size? preferredSize = null)
	{
		return (image as Bitmap).Blur(radius, doNotDispose, preferredSize);
	}

	public static Bitmap Blur(this Bitmap img, int? radius = null, bool doNotDispose = false, Size? preferredSize = null)
	{
		if (img == null)
		{
			return null;
		}

		if (radius == 0)
		{
			return img;
		}

		if (preferredSize != null)
		{
			using var img2 = new Bitmap(img, CalculateNewSize(img.Size, preferredSize.Value));

			if (!doNotDispose)
			{
				img.Dispose();
			}

			return Blur(img2, radius, true);
		}

		if (doNotDispose)
		{
			return new GaussianBlur(img).Process(Math.Max(img.Width, img.Height) / (101 - (radius ?? 40)).Between(1, 100));
		}

		using (img)
		{
			return new GaussianBlur(img).Process(Math.Max(img.Width, img.Height) / (101 - (radius ?? 40)).Between(1, 100));
		}

		#region OldBlur

		//var kSize = radius ?? Math.Max(img.Width, img.Height) / 20;

		//if (kSize % 2 == 0) kSize++;
		//var Hblur = new Bitmap(img);

		//var Avg = (float)1 / kSize;

		//for (var j = 0; j < img.Height; j++)
		//{
		//	var hSum = new float[] { 0F, 0F, 0F, 0F };
		//	var iAvg = new float[] { 0F, 0F, 0F, 0F };

		//	for (var x = 0; x < kSize; x++)
		//	{
		//		var tmpColor = img.GetPixel(x, j);
		//		hSum[0] += tmpColor.A;
		//		hSum[1] += tmpColor.R;
		//		hSum[2] += tmpColor.G;
		//		hSum[3] += tmpColor.B;
		//	}
		//	iAvg[0] = hSum[0] * Avg;
		//	iAvg[1] = hSum[1] * Avg;
		//	iAvg[2] = hSum[2] * Avg;
		//	iAvg[3] = hSum[3] * Avg;

		//	for (var i = 0; i < img.Width; i++)
		//	{
		//		if (i - kSize / 2 >= 0 && i + 1 + kSize / 2 < img.Width)
		//		{
		//			var tmp_pColor = img.GetPixel(i - kSize / 2, j);
		//			hSum[0] -= tmp_pColor.A;
		//			hSum[1] -= tmp_pColor.R;
		//			hSum[2] -= tmp_pColor.G;
		//			hSum[3] -= tmp_pColor.B;
		//			var tmp_nColor = img.GetPixel(i + 1 + kSize / 2, j);
		//			hSum[0] += tmp_nColor.A;
		//			hSum[1] += tmp_nColor.R;
		//			hSum[2] += tmp_nColor.G;
		//			hSum[3] += tmp_nColor.B;
		//			//
		//			iAvg[0] = hSum[0] * Avg;
		//			iAvg[1] = hSum[1] * Avg;
		//			iAvg[2] = hSum[2] * Avg;
		//			iAvg[3] = hSum[3] * Avg;
		//		}
		//		Hblur.SetPixel(i, j, System.Drawing.Color.FromArgb((int)iAvg[0], (int)iAvg[1], (int)iAvg[2], (int)iAvg[3]));
		//	}
		//}

		//var total = new Bitmap(Hblur);

		//for (var i = 0; i < Hblur.Width; i++)
		//{
		//	var tSum = new float[] { 0F, 0F, 0F, 0F };
		//	var iAvg = new float[] { 0F, 0F, 0F, 0F };

		//	for (var y = 0; y < kSize; y++)
		//	{
		//		var tmpColor = Hblur.GetPixel(i, y);
		//		tSum[0] += tmpColor.A;
		//		tSum[1] += tmpColor.R;
		//		tSum[2] += tmpColor.G;
		//		tSum[3] += tmpColor.B;
		//	}

		//	iAvg[0] = tSum[0] * Avg;
		//	iAvg[1] = tSum[1] * Avg;
		//	iAvg[2] = tSum[2] * Avg;
		//	iAvg[3] = tSum[3] * Avg;

		//	for (var j = 0; j < Hblur.Height; j++)
		//	{
		//		if (j - kSize / 2 >= 0 && j + 1 + kSize / 2 < Hblur.Height)
		//		{
		//			var tmp_pColor = Hblur.GetPixel(i, j - kSize / 2);
		//			tSum[0] -= tmp_pColor.A;
		//			tSum[1] -= tmp_pColor.R;
		//			tSum[2] -= tmp_pColor.G;
		//			tSum[3] -= tmp_pColor.B;
		//			var tmp_nColor = Hblur.GetPixel(i, j + 1 + kSize / 2);
		//			tSum[0] += tmp_nColor.A;
		//			tSum[1] += tmp_nColor.R;
		//			tSum[2] += tmp_nColor.G;
		//			tSum[3] += tmp_nColor.B;
		//			//
		//			iAvg[0] = tSum[0] * Avg;
		//			iAvg[1] = tSum[1] * Avg;
		//			iAvg[2] = tSum[2] * Avg;
		//			iAvg[3] = tSum[3] * Avg;
		//		}
		//		total.SetPixel(i, j, System.Drawing.Color.FromArgb((int)iAvg[0], (int)iAvg[1], (int)iAvg[2], (int)iAvg[3]));
		//	}
		//}

		//return total;

		#endregion OldBlur
	}

	public static Size CalculateNewSize(Size imageSize, Size preferredSize)
	{
		var widthRatio = (double)preferredSize.Width / imageSize.Width;
		var heightRatio = (double)preferredSize.Height / imageSize.Height;

		var minRatio = Math.Max(widthRatio, heightRatio);

		var newWidth = (int)(imageSize.Width * minRatio);
		var newHeight = (int)(imageSize.Height * minRatio);

		return new Size(newWidth, newHeight);
	}

	public static Color GetTextColor(this Color color)
	{
		if (color.A <= 125)
		{
			return FormDesign.Design.ForeColor;
		}

		var dark = (color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114) > 186;

		if (dark == (!FormDesign.Design.IsDarkTheme))
		{
			return FormDesign.Design.ForeColor;
		}

		var b = dark ? 0.05f : 0.95f;

		return ColorFromHSL(color.GetHue(), 0.2f, b);
	}

	/// <summary>
	/// Resumes the Drawing of a <see cref="Control"/>
	/// </summary>
	public static void ResumeDrawing(this Control parent, bool refresh = true)
	{
		try
		{
			SendMessage(parent.Handle, WM_SETREDRAW, true, 0);
			if (refresh)
			{
				parent.Refresh();
				parent.ResumeLayout(true);
			}
		}
		catch { }
	}

	public static GraphicsPath RoundedRect(this Rectangle bounds, int radius, bool topleft = true, bool topright = true, bool botright = true, bool botleft = true)
	{
		var path = new GraphicsPath();

		if (radius <= 0 || CrossIO.CurrentPlatform != Platform.Windows)
		{
			path.AddRectangle(bounds);
			return path;
		}

		var diameter = radius * 2;
		var size = new Size(diameter, diameter);
		var arc = new Rectangle(bounds.Location, size);

		// top left arc
		if (topleft)
		{
			path.AddArc(arc, 180, 90);
		}
		else
		{
			path.AddLine(bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y);
		}

		// top right arc
		arc.X = bounds.Right - diameter;
		if (topright)
		{
			path.AddArc(arc, 270, 90);
		}
		else
		{
			path.AddLine(bounds.X + bounds.Width, bounds.Y, bounds.X + bounds.Width, bounds.Y + bounds.Height);
		}

		// bottom right arc
		arc.Y = bounds.Bottom - diameter;
		if (botright)
		{
			path.AddArc(arc, 0, 90);
		}
		else
		{
			path.AddLine(bounds.X + bounds.Width, bounds.Y + bounds.Height, bounds.X, bounds.Y + bounds.Height);
		}

		// bottom left arc
		arc.X = bounds.Left;
		if (botleft)
		{
			path.AddArc(arc, 90, 90);
		}
		else
		{
			path.AddLine(bounds.X, bounds.Y + bounds.Height, bounds.X, topleft ? bounds.Y + radius : bounds.Y);
		}

		path.CloseFigure();
		return path;
	}

	public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius, bool topLeft = true, bool topRight = true, bool botRight = true, bool botLeft = true)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}

		if (pen == null)
		{
			throw new ArgumentNullException("pen");
		}

		using var path = RoundedRect(bounds, cornerRadius, topLeft, topRight, botRight, botLeft);
		graphics.DrawPath(pen, path);
	}

	public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius, bool topLeft = true, bool topRight = true, bool botRight = true, bool botLeft = true)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}

		if (brush == null)
		{
			throw new ArgumentNullException("brush");
		}

		using var path = RoundedRect(bounds, cornerRadius, topLeft, topRight, botRight, botLeft);
		graphics.FillPath(brush, path);
	}

#if NET47
	private static readonly Dictionary<int, (DateTime DateCreated, TextureBrush Brush)> _imageBrushCache = [];

	public static void DrawRoundedImage(this Graphics graphics, Image image, Rectangle bounds, int cornerRadius, Color? background = null, bool topLeft = true, bool topRight = true, bool botRight = true, bool botLeft = true)
	{
		if (image == null)
		{
			return;
		}

		// Clamp the corner radius to prevent overflow
		var effectiveRadius = Math.Min(cornerRadius, Math.Min(bounds.Width, bounds.Height) / 2);

		// Create a rounded rectangle path with the clamped radius
		using var path = RoundedRect(bounds, effectiveRadius, topLeft, topRight, botRight, botLeft);

		// Optional background color fill
		if (background.HasValue)
		{
			using var brush = new SolidBrush(background.Value);
			graphics.FillPath(brush, path);
		}

		// Save clipping region
		var currentClip = graphics.ClipBounds;

		// Clip the graphics to the rounded rectangle path
		graphics.SetClip(path);

		// Draw the image within the clipped area
		graphics.DrawImage(image, bounds.CenterR(CalculateNewSize(image.Size,bounds.Size)));

		// Reset the clipping region
		graphics.SetClip(currentClip);

		if (background.HasValue)
		{
			using var pen = new Pen(background.Value, 1.5f) { Alignment = PenAlignment.Center };
			graphics.DrawPath(pen, path);
		}
	}

#else
	public static void DrawRoundedImage(this Graphics graphics, Image image, Rectangle bounds, int cornerRadius, Color? background = null, bool topLeft = true, bool topRight = true, bool botRight = true, bool botLeft = true, bool blur = false)
	{
		if (image == null)
		{
			return;
		}

		var hash = image.GetHashCode() + bounds.GetHashCode();

		var newImage = new Bitmap(image, CalculateNewSize(image.Size, bounds.Size) + new Size(2, 2));

		using var imageGraphics = Graphics.FromImage(newImage);
		if (background != null)
		{
			imageGraphics.Clear(background.Value);
		}

		imageGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		imageGraphics.DrawImage(image, new Rectangle(Point.Empty, bounds.Size).CenterR(newImage.Size));

		using var finalImage = blur ? Blur(newImage) : newImage;
		using var textureBrush = new TextureBrush(finalImage);

		graphics.TranslateTransform(bounds.X, bounds.Y);
		graphics.FillRoundedRectangle(textureBrush, new Rectangle(Point.Empty, bounds.Size), cornerRadius, topLeft, topRight, botRight, botLeft);
		graphics.TranslateTransform(-bounds.X, -bounds.Y);
	}
#endif

	public static void DrawRoundImage(this Graphics graphics, Image image, Rectangle bounds, Color? background = null)
	{
		if (image == null)
		{
			return;
		}

		var newImage = new Bitmap(image, CalculateNewSize(image.Size, bounds.Size));
		using var imageGraphics = Graphics.FromImage(newImage);
		if (background != null)
		{
			imageGraphics.Clear(background.Value);
		}

		imageGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		imageGraphics.DrawImage(image, new Rectangle(Point.Empty, bounds.Size).CenterR(newImage.Size));

		using var textureBrush = new TextureBrush(newImage);
		graphics.TranslateTransform(bounds.X, bounds.Y);
		graphics.FillEllipse(textureBrush, new Rectangle(Point.Empty, bounds.Size));
		graphics.TranslateTransform(-bounds.X, -bounds.Y);
	}

	public static Size GetProportionalDownscaledSize(this Size originalSize, int sizeLimit, bool widthOnly = false)
	{
		// If the original size is already within the limit, return it
		if (originalSize.Width <= sizeLimit && (widthOnly || originalSize.Height <= sizeLimit))
		{
			return originalSize;
		}

		// Determine the scaling factor required to fit within the limit
		var scaleFactor = widthOnly ? (float)sizeLimit / originalSize.Width :
			Math.Min((float)sizeLimit / originalSize.Width, (float)sizeLimit / originalSize.Height);

		// Calculate the new scaled size
		var newWidth = (int)Math.Round(originalSize.Width * scaleFactor);
		var newHeight = (int)Math.Round(originalSize.Height * scaleFactor);

		return new Size(newWidth, newHeight);
	}

	/// <summary>
	/// Rotates the <see cref="Bitmap"/> using the <paramref name="flipType"/>
	/// </summary>
	public static Bitmap Rotate(this Bitmap bitmap, RotateFlipType flipType = RotateFlipType.Rotate90FlipNone)
	{
		if (bitmap == null)
		{
			return null;
		}

		bitmap.RotateFlip(flipType);
		return bitmap;
	}

	[DllImport("user32.dll")]
	private static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

	/// <summary>
	/// Shows, Brings to Front and Restores the <see cref="Form"/>
	/// </summary>
	public static T ShowUp<T>(this T form, bool initialize) where T : Form, new()
	{
		if (initialize && (form == null || form.IsDisposed))
		{
			form = new T();
		}

		if (form.WindowState == FormWindowState.Minimized)
		{
			form.WindowState = FormWindowState.Normal;
		}

		form.Show();
		form.Focus();
		form.Activate();

		return form;
	}

	/// <summary>
	/// Shows, Brings to Front and Restores the <see cref="Form"/>
	/// </summary>
	public static void ShowUp(this Form form)
	{
		if (form.WindowState == FormWindowState.Minimized)
		{
			form.WindowState = FormWindowState.Normal;
		}

		form.Show();
		form.Focus();
		form.Activate();
	}

	/// <summary>
	/// Stops all Draw events of a <see cref="Control"/>, use <see cref="ResumeDrawing(Control)"/> to revert
	/// </summary>
	public static void SuspendDrawing(this Control parent)
	{
		try
		{
			SendMessage(parent.Handle, WM_SETREDRAW, false, 0);
			parent.SuspendLayout();
		}
		catch { }
	}

	/// <summary>
	/// Tints the <see cref="Bitmap"/> with selected Luminance, Saturation and Hue
	/// </summary>
	/// <param name="Lum">Added Luminance, ranges from -100 to 100</param>
	/// <param name="Sat">Added Saturation, ranges from -100 to 100</param>
	/// <param name="Hue">Added Hue, ranges from -360 to 360</param>
	public static Bitmap Tint(this Bitmap bitmap, float? Lum = null, float? Sat = null, float? Hue = null)
	{
		if (bitmap == null)
		{
			return null;
		}

		var W = bitmap.Width;
		var H = bitmap.Height;
		float nH, nS, nL;

		for (var i = 0; i < H; i++)
		{
			for (var j = 0; j < W; j++)
			{
				var color = bitmap.GetPixel(j, i);
				color.RgbToHsl(out var cHue, out var cSat, out var cLum);

				nH = (Hue ?? cHue).Between(0, 360);
				nS = (Sat ?? cSat).Between(0, 1);
				nL = (Lum ?? cLum).Between(0, 1);

				bitmap.SetPixel(j, i, System.Drawing.Color.FromArgb(bitmap.GetPixel(j, i).A,
					ColorFromHSL(nH, nS, nL)));
			}
		}

		return bitmap;
	}

	/// <summary>
	/// Tints a <see cref="Color"/> using the Hue of a <paramref name="source"/> <see cref="Color"/>, Luminance and Saturation
	/// </summary>
	/// <param name="Lum">Added Luminance, ranges from -100 to 100</param>
	/// <param name="Sat">Added Saturation, ranges from -100 to 100</param>
	public static Color Tint(this Color color, Color source, float Lum = 0, float Sat = 0)
	{
		return color.Tint(source.GetHue(), Lum, Sat);
	}

	/// <summary>
	/// Tints the <see cref="Color"/> with selected Luminance, Saturation and Hue
	/// </summary>
	/// <param name="Lum">Added Luminance, ranges from -100 to 100</param>
	/// <param name="Sat">Added Saturation, ranges from -100 to 100</param>
	/// <param name="Hue">Added Hue, ranges from -360 to 360</param>
	public static Color Tint(this Color color, float? Hue = null, float Lum = 0, float Sat = 0)
	{
		color.RgbToHsl(out var cHue, out var cSat, out var cLum);

		return System.Drawing.Color.FromArgb(color.A, ColorFromHSL((Hue ?? cHue).Between(0, 360), (cSat + (Sat / 100f)).Between(0, 1), (cLum + (Lum / 100f)).Between(0, 1)));
	}

	public static Bitmap ToGrayscale(this Bitmap source)
	{
		int width = source.Width;
		int height = source.Height;

		// Ensure the source is in 24bppRgb format
		Bitmap source24bpp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
		using (Graphics g = Graphics.FromImage(source24bpp))
		{
			g.DrawImage(source, 0, 0, width, height);
		}

		BitmapData bmpData = source24bpp.LockBits(
			new Rectangle(0, 0, width, height),
			ImageLockMode.ReadOnly,
			PixelFormat.Format24bppRgb);

		int stride = bmpData.Stride;
		int bytes = stride * height;
		byte[] pixelBuffer = new byte[bytes];

		Marshal.Copy(bmpData.Scan0, pixelBuffer, 0, bytes);
		source24bpp.UnlockBits(bmpData);

		byte[] resultBuffer = new byte[bytes];

		for (int y = 0; y < height; y++)
		{
			int row = y * stride;

			for (int x = 0; x < width; x++)
			{
				int index = row + x * 3;

				byte b = pixelBuffer[index];
				byte g = pixelBuffer[index + 1];
				byte r = pixelBuffer[index + 2];

				// Grayscale using luminosity
				byte gray = (byte)(0.3 * r + 0.59 * g + 0.11 * b);

				resultBuffer[index] = gray;
				resultBuffer[index + 1] = gray;
				resultBuffer[index + 2] = gray;
			}
		}

		Bitmap result = new Bitmap(width, height, PixelFormat.Format24bppRgb);
		BitmapData resultData = result.LockBits(
			new Rectangle(0, 0, width, height),
			ImageLockMode.WriteOnly,
			PixelFormat.Format24bppRgb);

		Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);
		result.UnlockBits(resultData);

		return result;
	}

	/// <summary>
	/// Returns a collection of the controls that match the <paramref name="test"/>
	/// </summary>
	public static IEnumerable<Control> Where(this Control.ControlCollection controls, Func<Control, bool> predicate)
	{
		return controls.Cast<Control>().Where(predicate);
	}

	public static bool Any(this Control.ControlCollection controls, Func<Control, bool> predicate)
	{
		return controls.Cast<Control>().Any(predicate);
	}

	public static Control FirstOrDefault(this Control.ControlCollection controls, Func<Control, bool> predicate)
	{
		return controls.Cast<Control>().FirstOrDefault(predicate);
	}

	public static Control First(this Control.ControlCollection controls, Func<Control, bool> predicate)
	{
		return controls.Cast<Control>().First(predicate);
	}

	public static int Max(this Control.ControlCollection controls, Func<Control, int> predicate)
	{
		return controls.Count == 0 ? 0 : controls.Cast<Control>().Max(predicate);
	}

	public static double Max(this Control.ControlCollection controls, Func<Control, double> predicate)
	{
		return controls.Count == 0 ? 0 : controls.Cast<Control>().Max(predicate);
	}

	public static int Min(this Control.ControlCollection controls, Func<Control, int> predicate)
	{
		return controls.Count == 0 ? 0 : controls.Cast<Control>().Min(predicate);
	}

	public static double Min(this Control.ControlCollection controls, Func<Control, double> predicate)
	{
		return controls.Count == 0 ? 0 : controls.Cast<Control>().Min(predicate);
	}

	private static double GetColorComponent(double temp1, double temp2, double temp3)
	{
		if (temp3 < 0.0)
		{
			temp3 += 1.0;
		}
		else if (temp3 > 1.0)
		{
			temp3 -= 1.0;
		}

		if (temp3 < 1.0 / 6.0)
		{
			return temp1 + ((temp2 - temp1) * 6.0 * temp3);
		}
		else if (temp3 < 0.5)
		{
			return temp2;
		}
		else if (temp3 < 2.0 / 3.0)
		{
			return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
		}
		else
		{
			return temp1;
		}
	}

	public static Color GetAccentColor(this Color color)
	{
		return (color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114) > 186 ? System.Drawing.Color.Black : System.Drawing.Color.White;
	}

	public static bool IsDark(this Color color)
	{
		return (color.R * 0.299) + (color.G * 0.587) + (color.B * 0.114) <= 186;
	}

	private static float HuetoRGB(float p, float q, float t)
	{
		if (t < 0)
		{
			t += 1;
		}

		if (t > 1)
		{
			t -= 1;
		}

		if (t < 1 / 6)
		{
			return p + ((q - p) * 6 * t);
		}

		if (t < 1 / 2)
		{
			return q;
		}

		if (t < 2 / 3)
		{
			return p + ((q - p) * ((2 / 3) - t) * 6);
		}

		return p;
	}

	public static void DrawIconsOverImage(this Graphics g, Rectangle imgRect, Point mousePosition, params Bitmap[] bitmaps)
	{
		DrawIconsOverImage(g, imgRect, mousePosition, 1, bitmaps);
	}

	public static void DrawIconsOverImage(this Graphics g, Rectangle imgRect, Point mousePosition, double opacity, params Bitmap[] bitmaps)
	{
		if ((bitmaps?.Length ?? 0) == 0)
		{
			return;
		}

		imgRect = imgRect.Pad(2);

		g.FillRectangle(Gradient(imgRect, System.Drawing.Color.FromArgb((int)(150 * opacity), FormDesign.Design.BackColor), 3), imgRect);

		var baseX = 13 + imgRect.X + ((imgRect.Width - bitmaps.Sum(y => y.Width + 26)) / 2);

		for (var i = 0; i < bitmaps.Length; i++)
		{
			var drawRect = new Rectangle(new Point(baseX, imgRect.Top + ((imgRect.Height - bitmaps[i].Height) / 2)), bitmaps[i].Size);
			var hovered = drawRect.Pad(-7).Contains(mousePosition);

			g.DrawImage(new Bitmap(bitmaps[i]).Color(hovered ? FormDesign.Design.ActiveColor : FormDesign.Design.ForeColor, (byte)(int)(255 * opacity)), drawRect);

			baseX += drawRect.Width + 26;
		}
	}

	public static void DrawImage(this Graphics g, Bitmap bitmap, Rectangle rectangle, ImageSizeMode sizeMode)
	{
		if (bitmap == null)
		{
			return;
		}

		try
		{
			var bitSize = bitmap.Size;
			var bitSrc = new Rectangle(Point.Empty, bitSize);

			switch (sizeMode)
			{
				case ImageSizeMode.Fill:
				case ImageSizeMode.CenterScaled:
					if (bitSize.Width < rectangle.Width && bitSize.Height < rectangle.Height)
					{
						sizeMode = ImageSizeMode.Center; // Automatic Center
					}

					break;
			}

			switch (sizeMode)
			{
				case ImageSizeMode.Fill:
				case ImageSizeMode.FillForced:
				case ImageSizeMode.CenterScaled:
					if ((float)rectangle.Width / bitSize.Width > (float)rectangle.Height / bitSize.Height)
					{
						bitSrc.Height = bitSize.Width * rectangle.Height / rectangle.Width;
					}
					else
					{
						bitSrc.Width = bitSize.Height * rectangle.Width / rectangle.Height;
					}

					if (sizeMode == ImageSizeMode.CenterScaled)
					{
						bitSrc.X = (bitSize.Width - bitSrc.Width) / 2;
						bitSrc.Y = (bitSize.Height - bitSrc.Height) / 2;
					}

					g.CompositingMode = CompositingMode.SourceCopy;
					break;

				case ImageSizeMode.Stretch:
					g.CompositingMode = CompositingMode.SourceCopy;
					break;

				case ImageSizeMode.Center:
					rectangle = new Rectangle(rectangle.Center(bitSize), bitSize);
					break;
			}

			g.DrawImage(bitmap, rectangle, bitSrc, GraphicsUnit.Pixel);
		}
		catch { }
		finally
		{
			g.CompositingMode = CompositingMode.SourceOver;
		}
	}

	public static void DrawImage(this Graphics g, Bitmap bitmap, int x, int y, int width, int height, ImageSizeMode sizeMode)
	{
		g.DrawImage(bitmap, new Rectangle(x, y, width, height), sizeMode);
	}

	public static void DrawImage(this Graphics g, Image bitmap, int x, int y, int width, int height, ImageSizeMode sizeMode)
	{
		g.DrawImage((Bitmap)bitmap, new Rectangle(x, y, width, height), sizeMode);
	}

	public static void DrawImage(this Graphics g, Image bitmap, Rectangle rectangle, ImageSizeMode sizeMode)
	{
		g.DrawImage((Bitmap)bitmap, rectangle, sizeMode);
	}

	public static void DrawBorderedImage(this Graphics g, Bitmap bitmap, Rectangle rectangle, ImageSizeMode sizeMode = ImageSizeMode.Fill, Color? borderColor = null, bool noComposite = false)
	{
		var bitRect = new Rectangle(rectangle.X + 3, rectangle.Y + 3, rectangle.Width - 5, rectangle.Height - 5);

		using (var pen = new Pen(borderColor ?? FormDesign.Design.AccentColor, 1.5F))
		{
			g.DrawRectangle(pen, rectangle);
		}

		if (bitmap == null)
		{
			return;
		}

		var bitSize = bitmap.Size;
		var bitSrc = new Rectangle(Point.Empty, bitSize);

		if (sizeMode != ImageSizeMode.Stretch && sizeMode != ImageSizeMode.Center
			&& bitSize.Width <= bitRect.Width && bitSize.Height <= bitRect.Height)
		{
			sizeMode = ImageSizeMode.Center; // Automatic Center
		}

		switch (sizeMode)
		{
			case ImageSizeMode.Fill:
			case ImageSizeMode.CenterScaled:
				if ((float)bitRect.Width / bitSize.Width > (float)bitRect.Height / bitSize.Height)
				{
					bitSrc.Height = bitSize.Width * bitRect.Height / bitRect.Width;
				}
				else
				{
					bitSrc.Width = bitSize.Height * bitRect.Width / bitRect.Height;
				}

				if (sizeMode == ImageSizeMode.CenterScaled)
				{
					bitSrc.X = (bitSize.Width - bitSrc.Width) / 2;
					bitSrc.Y = (bitSize.Height - bitSrc.Height) / 2;
				}

				if (!noComposite)
				{
					g.CompositingMode = CompositingMode.SourceCopy;
				}

				break;

			case ImageSizeMode.Stretch:
				if (!noComposite)
				{
					g.CompositingMode = CompositingMode.SourceCopy;
				}

				break;

			case ImageSizeMode.Center:
				bitRect = new Rectangle(bitRect.Center(bitSize), bitSize);
				break;
		}

		g.DrawImage(bitmap, bitRect, bitSrc, GraphicsUnit.Pixel);
		g.CompositingMode = CompositingMode.SourceOver;
	}

	public static void DrawBorderedImage(this Graphics g, Bitmap bitmap, int x, int y, int width, int height, ImageSizeMode sizeMode = ImageSizeMode.Fill, Color? borderColor = null)
	{
		g.DrawBorderedImage(bitmap, new Rectangle(x, y, width, height), sizeMode, borderColor);
	}

	public static void DrawBorderedImage(this Graphics g, Image bitmap, int x, int y, int width, int height, ImageSizeMode sizeMode = ImageSizeMode.Fill, Color? borderColor = null)
	{
		g.DrawBorderedImage((Bitmap)bitmap, new Rectangle(x, y, width, height), sizeMode, borderColor);
	}

	public static void DrawBorderedImage(this Graphics g, Image bitmap, Rectangle rectangle, ImageSizeMode sizeMode = ImageSizeMode.Fill, Color? borderColor = null)
	{
		g.DrawBorderedImage((Bitmap)bitmap, rectangle, sizeMode, borderColor);
	}

	public static void DrawFancyText(this Graphics g, string text, Font font, Rectangle rectangle, StringFormat stringFormat = null)
	{
		g.DrawString(text, font, new SolidBrush(System.Drawing.Color.FromArgb(100, FormDesign.Design.BackColor)), rectangle, stringFormat ?? new StringFormat());
		rectangle.Y -= 2;
		rectangle.X -= 2;
		g.DrawString(text, font, new SolidBrush(System.Drawing.Color.FromArgb(40, FormDesign.Design.ForeColor)), rectangle, stringFormat ?? new StringFormat());
		rectangle.Y += 1;
		rectangle.X += 1;
		g.DrawString(text, font, rectangle.Gradient(FormDesign.Design.ActiveColor), rectangle, stringFormat ?? new StringFormat());
	}

	public static Brush Gradient(this Rectangle rect, Color color, float caliber = 0.75F)
	{
		var hue = color.GetHue();

		if (rect.Width <= 0 || rect.Height <= 0)
		{
			return new SolidBrush(color);
		}

		var length = (float)Math.Sqrt(Math.Pow(rect.Width, 2) + Math.Pow(rect.Height, 2));

		caliber *= Math.Min(length / 400, 2);

		var color1 = color.Tint(hue + (caliber * 3F), +caliber * 3F, +caliber * 2.5F);
		var color2 = color.Tint(hue - (caliber * 3F), -caliber * 3F, -caliber * 2.5F);

		return new LinearGradientBrush(rect, color1, color2, 45);
	}

	public static IEnumerable<T> ThatAre<T>(this Control.ControlCollection list) where T : Control
	{
		return list.OfType<T>();
	}

	public static string YesNo(this bool b)
	{
		return b ? "Yes" : "No";
	}

	public static bool IsDigit(this Keys keys)
	{
		return keys is (>= Keys.D0 and <= Keys.D9) or (>= Keys.NumPad0 and <= Keys.NumPad9);
	}

	public static bool IsLetter(this Keys keys)
	{
		return (keys & ~Keys.Shift) is >= Keys.A and <= Keys.Z;
	}

	public static bool IsDigitOrLetter(this Keys keys)
	{
		return keys.IsDigit() || keys.IsLetter();
	}
}
