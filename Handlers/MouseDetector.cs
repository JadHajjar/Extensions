using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Extensions
{
	public class MouseDetector : IDisposable
	{
		#region APIs

		[DllImport("gdi32")]
		private static extern uint GetPixel(IntPtr hDC, int XPos, int YPos);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool GetCursorPos(out POINT pt);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr GetWindowDC(IntPtr hWnd);

		#endregion APIs

		private readonly Timer tm = new Timer() { Interval = 10 };

		public delegate void MouseMoveDLG(object sender, Point p);

		public event MouseMoveDLG MouseMove;

		private Point lastPoint;

		public MouseDetector()
		{
			tm.Tick += new EventHandler(Tm_Tick); tm.Start();
		}

		private void Tm_Tick(object sender, EventArgs e)
		{
			GetCursorPos(out var p);

			if (p.X != lastPoint.X || p.Y != lastPoint.Y)
				MouseMove?.Invoke(this, lastPoint = new Point(p.X, p.Y));
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				tm.Dispose();
				MouseMove = null;
				GC.SuppressFinalize(this);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				X = x;
				Y = y;
			}
		}
	}
}