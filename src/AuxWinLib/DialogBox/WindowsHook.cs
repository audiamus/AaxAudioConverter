#pragma warning disable 618
//=============================================================================
// COPYRIGHT: Prosoft-Lanz
//=============================================================================
//
// $Workfile: WindowsHook.cs $
//
// PROJECT : CodeProject Components
// VERSION : 1.00
// CREATION : 19.02.2003
// AUTHOR : JCL
//
// DETAILS : This class implement the Windows hook mechanism.
//           From MSDN, Dino Esposito.
//
//
// https://www.codeproject.com/Articles/9984/Centering-MessageBox-Common-DialogBox-or-Form-on-a
//
//-----------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;

namespace audiamus.aux.win.Win32API.Hook
{
	///////////////////////////////////////////////////////////////////////
	#region Class HookEventArgs

	/// Class used for hook event arguments.
	public class HookEventArgs : EventArgs
	{
		/// Event code parameter.
		public int code;
		/// wParam parameter.
		public IntPtr wParam;
		/// lParam parameter.
		public IntPtr lParam;

		internal HookEventArgs(int code, IntPtr wParam, IntPtr lParam)
		{
			this.code = code;
			this.wParam = wParam;
			this.lParam = lParam;
		}
	}
	
	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Enum HookType

	/// Hook Types.
	public enum HookType : int
	{
		/// <value>0</value>
		WH_JOURNALRECORD = 0,
		/// <value>1</value>
		WH_JOURNALPLAYBACK = 1,
		/// <value>2</value>
		WH_KEYBOARD = 2,
		/// <value>3</value>
		WH_GETMESSAGE = 3,
		/// <value>4</value>
		WH_CALLWNDPROC = 4,
		/// <value>5</value>
		WH_CBT = 5,
		/// <value>6</value>
		WH_SYSMSGFILTER = 6,
		/// <value>7</value>
		WH_MOUSE = 7,
		/// <value>8</value>
		WH_HARDWARE = 8,
		/// <value>9</value>
		WH_DEBUG = 9,
		/// <value>10</value>
		WH_SHELL = 10,
		/// <value>11</value>
		WH_FOREGROUNDIDLE = 11,
		/// <value>12</value>
		WH_CALLWNDPROCRET = 12,		
		/// <value>13</value>
		WH_KEYBOARD_LL = 13,
		/// <value>14</value>
		WH_MOUSE_LL = 14
	}
	#endregion

	///////////////////////////////////////////////////////////////////////
	#region Class WindowsHook

	/// <summary>
	/// Class to expose the windows hook mechanism.
	/// </summary>
	public class WindowsHook
	{
		/// <summary>
		/// Hook delegate method.
		/// </summary>
		public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

		// internal properties
		internal IntPtr hHook = IntPtr.Zero;
		internal HookProc filterFunc = null;
		internal HookType hookType;

		/// <summary>
		/// Hook delegate method.
		/// </summary>
		public delegate void HookEventHandler(object sender, HookEventArgs e);

		/// <summary>
		/// Hook invoke event.
		/// </summary>
		public event HookEventHandler HookInvoke;

		internal void OnHookInvoke(HookEventArgs e)
		{
			if (HookInvoke != null)
				HookInvoke(this, e);
		}

		/// <summary>
		/// Construct a HookType hook.
		/// </summary>
		/// <param name="hook">Hook type.</param>
		public WindowsHook(HookType hook)
		{
			hookType = hook;
			filterFunc = new HookProc(this.CoreHookProc);
		}
		/// <summary>
		/// Construct a HookType hook giving a hook filter delegate method.
		/// </summary>
		/// <param name="hook">Hook type</param>
		/// <param name="func">Hook filter event.</param>
		public WindowsHook(HookType hook, HookProc func)
		{
			hookType = hook;
			filterFunc = func; 
		}

		// default hook filter function
		internal int CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
		{
			if (code < 0)
				return CallNextHookEx(hHook, code, wParam, lParam);

			// let clients determine what to do
			HookEventArgs e = new HookEventArgs(code, wParam, lParam);
			OnHookInvoke(e);

			// yield to the next hook in the chain
			return CallNextHookEx(hHook, code, wParam, lParam);
		}

		/// <summary>
		/// Install the hook. 
		/// </summary>
		public void Install()
		{
			hHook = SetWindowsHookEx(hookType, filterFunc, IntPtr.Zero, (int)AppDomain.GetCurrentThreadId());
		}

		
		/// <summary>
		/// Uninstall the hook.
		/// </summary>
 		public void Uninstall()
		{
			if (hHook != IntPtr.Zero)
			{
				UnhookWindowsHookEx(hHook);
				hHook = IntPtr.Zero;
			}
		}

		#region Win32 Imports

		[DllImport("user32.dll")]
		internal static extern IntPtr SetWindowsHookEx(HookType code, HookProc func, IntPtr hInstance, int threadID);

		[DllImport("user32.dll")]
		internal static extern int UnhookWindowsHookEx(IntPtr hhook); 

		[DllImport("user32.dll")]
		internal static extern int CallNextHookEx(IntPtr hhook, int code, IntPtr wParam, IntPtr lParam);

		#endregion
	}
	#endregion
}
