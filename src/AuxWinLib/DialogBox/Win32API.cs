//=============================================================================
// COPYRIGHT: Prosoft-Lanz
//=============================================================================
//
// $Workfile: Win32API.cs $
//
// PROJECT : CodeProject Components
// VERSION : 1.00
// CREATION : 19.02.2003
// AUTHOR : JCL
//
// DETAILS : This class implement Win32 API calls
//           and the contants used for these calls.
//
//
// https://www.codeproject.com/Articles/9984/Centering-MessageBox-Common-DialogBox-or-Form-on-a
//
//-----------------------------------------------------------------------------
using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace audiamus.aux.win.Win32API
{
	///////////////////////////////////////////////////////////////////////
	#region Generic declarations

	/// <summary>
	/// Rectangle parameters exposed as a structure.
	/// </summary>
	public struct RECT
	{
		/// <summary>
		/// Rectangle members.
		/// </summary>
		public int left, top, right, bottom;
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Util class

	/// <summary>
	/// Utility functions.
	/// </summary>
	public sealed class API
	{
		private API() {}	// To remove the constructor from the documentation!

		/// <summary>
		/// Get true multiscreen size.
		/// </summary>
		public static Rectangle TrueScreenRect
		{
			get
			{
				// get the biggest screen area
				Rectangle rectScreen = Screen.PrimaryScreen.WorkingArea;
				int left = rectScreen.Left;
				int top = rectScreen.Top;
				int right = rectScreen.Right;
				int bottom = rectScreen.Bottom;
				foreach (Screen screen in Screen.AllScreens)
				{
					left = Math.Min(left, screen.WorkingArea.Left);
					right = Math.Max(right, screen.WorkingArea.Right);
					top = Math.Min(top, screen.WorkingArea.Top);
					bottom = Math.Max(bottom, screen.WorkingArea.Bottom);
				}
				return new Rectangle(left, top, right-left, bottom-top);
			}
		}
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region USER32 class

	/// <summary>
	/// Class to expose USER32 API functions.
	/// </summary>
	public sealed class USER32
	{
		private USER32() {}	// To remove the constructor from the documentation!

		[DllImport("user32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		internal static extern int GetWindowRect(IntPtr hWnd, ref RECT rect);

		[DllImport("user32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		internal static extern int MoveWindow(IntPtr hWnd, int x, int y, int w, int h, int repaint);

		[DllImport("user32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		internal static extern IntPtr GetActiveWindow();

		[DllImport("user32", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		internal static extern int GetClassName(IntPtr hwnd, StringBuilder lpClassName, int nMaxCount);
	}
	#endregion
}
