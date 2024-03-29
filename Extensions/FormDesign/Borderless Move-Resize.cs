public const int WM_NCLBUTTONDOWN = 0xA1;
public const int HT_CAPTION = 0x2;

[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool ReleaseCapture();

private void Form_MouseDown(object sender, MouseEventArgs e)
{
	if (e.Button == MouseButtons.Left)
	{
		ReleaseCapture();
		SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
	}
}

protected override void WndProc(ref Message m)
{
	const int RESIZE_HANDLE_SIZE = 10;

	switch (m.Msg)
	{
		case 0x0084/*NCHITTEST*/ :
			base.WndProc(ref m);

			if ((int)m.Result == 0x01/*HTCLIENT*/)
			{
				Point screenPoint = new Point(m.LParam.ToInt32());
				Point clientPoint = this.PointToClient(screenPoint);
				if (clientPoint.Y <= RESIZE_HANDLE_SIZE)
				{
					if (clientPoint.X <= RESIZE_HANDLE_SIZE)
						m.Result = (IntPtr)13/*HTTOPLEFT*/ ;
					else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
						m.Result = (IntPtr)12/*HTTOP*/ ;
					else
						m.Result = (IntPtr)14/*HTTOPRIGHT*/ ;
				}
				else if (clientPoint.Y <= (Size.Height - RESIZE_HANDLE_SIZE))
				{
					if (clientPoint.X <= RESIZE_HANDLE_SIZE)
						m.Result = (IntPtr)10/*HTLEFT*/ ;
					else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
						m.Result = (IntPtr)2/*HTCAPTION*/ ;
					else
						m.Result = (IntPtr)11/*HTRIGHT*/ ;
				}
				else
				{
					if (clientPoint.X <= RESIZE_HANDLE_SIZE)
						m.Result = (IntPtr)16/*HTBOTTOMLEFT*/ ;
					else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
						m.Result = (IntPtr)15/*HTBOTTOM*/ ;
					else
						m.Result = (IntPtr)17/*HTBOTTOMRIGHT*/ ;
				}
			}
			return;
	}
	base.WndProc(ref m);
}

protected override CreateParams CreateParams
{
	get
	{
		CreateParams cp = base.CreateParams;
		cp.Style |= 0x20000; // <--- use 0x20000
		return cp;
	}
}