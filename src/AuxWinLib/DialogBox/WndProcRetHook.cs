//=============================================================================
// COPYRIGHT: Prosoft-Lanz
//=============================================================================
//
// $Workfile: WndProcRetHook.cs $
//
// PROJECT : CodeProject Components
// VERSION : 1.00
// CREATION : 19.02.2003
// AUTHOR : JCL
//
// DETAILS : This class implement the WH_CALLWNDPROCRET Windows hook mechanism.
//           From MSDN, Dino Esposito.
//
//           WindowCreate, WindowDestroye and WindowActivate user events.
//
//
// https://www.codeproject.com/Articles/9984/Centering-MessageBox-Common-DialogBox-or-Form-on-a
//
//-----------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace audiamus.aux.win.Win32API.Hook {
  ///////////////////////////////////////////////////////////////////////
  #region Enum WndMessage

  /// <summary>
  /// windows message.
  /// </summary>
  public enum WndMessage : int
	{
		/// Sent to the dialog procedure immediately before the dialog is displayed.
		WM_INITDIALOG = 0x0110,
		/// Sent to the dialog procedure immediately before the dialog is displayed.
		WM_UNKNOWINIT = 0x0127
	}
	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class WndProcRetEventArgs

	/// Class used for WH_CALLWNDPROCRET hook event arguments.
	public class WndProcRetEventArgs : EventArgs
	{
		/// wParam parameter.
		public IntPtr wParam;
		/// lParam parameter.
		public IntPtr lParam;
		/// CWPRETSTRUCT structure.
		public CwPRetStruct cw;

		internal WndProcRetEventArgs(IntPtr wParam, IntPtr lParam)
		{
			this.wParam = wParam;
			this.lParam = lParam;
			cw = new CwPRetStruct();
			Marshal.PtrToStructure(lParam, cw);
		}
	}

	/// <summary>
	/// CWPRETSTRUCT structure.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public class CwPRetStruct
	{
		/// Return value.
		public IntPtr lResult;
		/// lParam parameter.
		public IntPtr lParam;
		/// wParam parameter.
		public IntPtr wParam;
		/// Specifies the message.
		public WndMessage message;
		/// Handle to the window that processed the message.
		public IntPtr hwnd;
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class WndProcRetHook
	
	/// <summary>
	/// Class to expose the windows WH_CALLWNDPROCRET hook mechanism.
	/// </summary>
	public class WndProcRetHook : WindowsHook
	{
		/// <summary>
		/// WH_CALLWNDPROCRET hook delegate method.
		/// </summary>
		public delegate void WndProcEventHandler(object sender, WndProcRetEventArgs e);

		private IntPtr hWndHooked;

		/// <summary>
		/// Window procedure event.
		/// </summary>
		public event WndProcEventHandler WndProcRet;

		/// <summary>
		/// Construct a WH_CALLWNDPROCRET hook.
		/// </summary>
		/// <param name="hWndHooked">
		/// Handle of the window to be hooked. IntPtr.Zero to hook all window.
		/// </param>
		public WndProcRetHook(IntPtr hWndHooked) : base(HookType.WH_CALLWNDPROCRET)
		{
			this.hWndHooked = hWndHooked;
			this.HookInvoke += new HookEventHandler(WndProcRetHookInvoked);
		}
		/// <summary>
		/// Construct a WH_CALLWNDPROCRET hook giving a hook filter delegate method.
		/// </summary>
		/// <param name="hWndHooked">
		/// Handle of the window to be hooked. IntPtr.Zero to hook all window.
		/// </param>
		/// <param name="func">Hook filter event.</param>
		public WndProcRetHook(IntPtr hWndHooked, HookProc func) : base(HookType.WH_CALLWNDPROCRET, func)
		{
			this.hWndHooked = hWndHooked;
			this.HookInvoke += new HookEventHandler(WndProcRetHookInvoked);
		}

		// handles the hook event
		private void WndProcRetHookInvoked(object sender, HookEventArgs e)
		{
			WndProcRetEventArgs wpe = new WndProcRetEventArgs(e.wParam, e.lParam);
			if ((hWndHooked == IntPtr.Zero || wpe.cw.hwnd == hWndHooked) && WndProcRet != null)
				WndProcRet(this, wpe);
			return;
		}
	}
	#endregion
}
