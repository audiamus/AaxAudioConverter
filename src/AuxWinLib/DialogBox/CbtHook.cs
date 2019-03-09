//=============================================================================
// COPYRIGHT: Prosoft-Lanz
//=============================================================================
//
// $Workfile: CbtHook.cs $
//
// PROJECT : CodeProject Components
// VERSION : 1.00
// CREATION : 19.02.2003
// AUTHOR : JCL
//
// DETAILS : This class implement the WH_CBT Windows hook mechanism.
//           From MSDN, Dino Esposito.
//           WindowCreate, WindowDestroy and WindowActivate user events.
//
// https://www.codeproject.com/Articles/9984/Centering-MessageBox-Common-DialogBox-or-Form-on-a
//
//-----------------------------------------------------------------------------
using System;
using System.Text;

namespace audiamus.aux.win.Win32API.Hook {
  ///////////////////////////////////////////////////////////////////////
  #region Enum CbtHookAction

  /// <summary>
  /// CBT hook actions.
  /// </summary>
  internal enum CbtHookAction : int
	{
		HCBT_MOVESIZE = 0,
		HCBT_MINMAX = 1,
		HCBT_QS = 2,
		HCBT_CREATEWND = 3,
		HCBT_DESTROYWND = 4,
		HCBT_ACTIVATE = 5,
		HCBT_CLICKSKIPPED = 6,
		HCBT_KEYSKIPPED = 7,
		HCBT_SYSCOMMAND = 8,
		HCBT_SETFOCUS = 9
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class CbtEventArgs

	/// <summary>
	/// Class used for WH_CBT hook event arguments.
	/// </summary>
	public class CbtEventArgs : EventArgs
	{
		/// wParam parameter.
		public IntPtr wParam;
		/// lParam parameter.
		public IntPtr lParam;
		/// Window class name.
		public string className;
		/// True if it is a dialog window.
		public bool IsDialog;

		internal CbtEventArgs(IntPtr wParam, IntPtr lParam)
		{
			// cache the parameters
			this.wParam = wParam;
			this.lParam = lParam;

			// cache the window's class name
			StringBuilder sb = new StringBuilder();
			sb.Capacity = 256;
			USER32.GetClassName(wParam, sb, 256);
			className = sb.ToString();
			IsDialog = (className == "#32770");
		}
	}

	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class CbtHook
	
	/// <summary>
	/// Class to expose the windows WH_CBT hook mechanism.
	/// </summary>
	public class CbtHook : WindowsHook
	{
		/// <summary>
		/// WH_CBT hook delegate method.
		/// </summary>
		public delegate void CbtEventHandler(object sender, CbtEventArgs e);

		/// <summary>
		/// WH_CBT create event.
		/// </summary>
		public event CbtEventHandler WindowCreate;
		/// <summary>
		/// WH_CBT destroy event.
		/// </summary>
		public event CbtEventHandler WindowDestroye;
		/// <summary>
		/// WH_CBT activate event.
		/// </summary>
		public event CbtEventHandler WindowActivate;

		/// <summary>
		/// Construct a WH_CBT hook.
		/// </summary>
		public CbtHook() : base(HookType.WH_CBT)
		{
			this.HookInvoke += new HookEventHandler(CbtHookInvoked);
		}
		/// <summary>
		/// Construct a WH_CBT hook giving a hook filter delegate method.
		/// </summary>
		/// <param name="func">Hook filter event.</param>
		public CbtHook(HookProc func) : base(HookType.WH_CBT, func)
		{
			this.HookInvoke += new HookEventHandler(CbtHookInvoked);
		}

		// handles the hook event
		private void CbtHookInvoked(object sender, HookEventArgs e)
		{
			// handle hook events (only a few of available actions)
			switch ((CbtHookAction)e.code)
			{
				case CbtHookAction.HCBT_CREATEWND:
					HandleCreateWndEvent(e.wParam, e.lParam);
					break;
				case CbtHookAction.HCBT_DESTROYWND:
					HandleDestroyWndEvent(e.wParam, e.lParam);
					break;
				case CbtHookAction.HCBT_ACTIVATE:
					HandleActivateEvent(e.wParam, e.lParam);
					break;
			}
			return;
		}

		// handle the CREATEWND hook event
		private void HandleCreateWndEvent(IntPtr wParam, IntPtr lParam)
		{
			if (WindowCreate != null)
			{
				CbtEventArgs e = new CbtEventArgs(wParam, lParam);
				WindowCreate(this, e);
			}
		}

		// handle the DESTROYWND hook event
		private void HandleDestroyWndEvent(IntPtr wParam, IntPtr lParam)
		{
			if (WindowDestroye != null)
			{
				CbtEventArgs e = new CbtEventArgs(wParam, lParam);
				WindowDestroye(this, e);
			}
		}

		// handle the ACTIVATE hook event
		private void HandleActivateEvent(IntPtr wParam, IntPtr lParam)
		{
			if (WindowActivate != null)
			{
				CbtEventArgs e = new CbtEventArgs(wParam, lParam);
				WindowActivate(this, e);
			}
		}
	}
	#endregion
}
