using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace audiamus.aux.win {
  /// <summary>
  /// Extends the system menu of a window with additional commands.
  /// Copyright Yves Goergen
  /// https://github.com/ygoe/FieldLog/blob/master/LogSubmit/Unclassified/UI/SystemMenu.cs
  /// </summary>
  public class SystemMenu {
    #region Native methods

    private const int WM_SYSCOMMAND = 0x112;
    private const int MF_STRING = 0x0;
    private const int MF_SEPARATOR = 0x800;

    [DllImport ("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetSystemMenu (IntPtr hWnd, bool bRevert);

    [DllImport ("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool AppendMenu (IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

    [DllImport ("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool InsertMenu (IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);

    #endregion Native methods

    #region Private data

    private Form _form;
    private IntPtr _hSysMenu;
    private int _lastId = 0;
    private List<Action> _actions = new List<Action> ();
    private List<CommandInfo> _pendingCommands;

    #endregion Private data

    #region Constructors

    /// <summary>
    /// Initialises a new instance of the <see cref="SystemMenu"/> class for the specified
    /// <see cref="Form"/>.
    /// </summary>
    /// <param name="form">The window for which the system menu is expanded.</param>
    public SystemMenu (Form form) {
      this._form = form;
      if (!form.IsHandleCreated) {
        form.HandleCreated += onHandleCreated;
      } else {
        onHandleCreated (null, null);
      }
    }

    #endregion Constructors

    #region Public methods

    /// <summary>
    /// Adds a command to the system menu.
    /// </summary>
    /// <param name="text">The displayed command text.</param>
    /// <param name="action">The action that is executed when the user clicks on the command.</param>
    /// <param name="separatorBeforeCommand">Indicates whether a separator is inserted before the command.</param>
    public void AddCommand (string text, Action action, bool separatorBeforeCommand) {
      int id = ++_lastId;
      if (!_form.IsHandleCreated) {
        // The form is not yet created, queue the command for later addition
        if (_pendingCommands == null) {
          _pendingCommands = new List<CommandInfo> ();
        }
        _pendingCommands.Add (new CommandInfo {
          Id = id,
          Text = text,
          Action = action,
          Separator = separatorBeforeCommand
        });
      } else {
        // The form is created, add the command now
        if (separatorBeforeCommand) {
          AppendMenu (_hSysMenu, MF_SEPARATOR, 0, "");
        }
        AppendMenu (_hSysMenu, MF_STRING, id, text);
      }
      _actions.Add (action);
    }

    /// <summary>
    /// Tests a window message for system menu commands and executes the associated action. This
    /// method must be called from within the Form's overridden WndProc method because it is not
    /// publicly accessible.
    /// </summary>
    /// <param name="msg">The window message to test.</param>
    public void HandleMessage (ref Message msg) {
      // This method is kept short and simple to allow inlining (verified) for improving
      // performance (unverified). It will be called for every single message that is sent to
      // the window.
      if (msg.Msg == WM_SYSCOMMAND) {
        onSysCommandMessage (ref msg);
      }
    }

    #endregion Public methods

    #region Private methods

    private void onHandleCreated (object sender, EventArgs args) {
      _form.HandleCreated -= onHandleCreated;
      _hSysMenu = GetSystemMenu (_form.Handle, false);

      // Add all queued commands now
      if (_pendingCommands != null) {
        foreach (CommandInfo command in _pendingCommands) {
          if (command.Separator) {
            AppendMenu (_hSysMenu, MF_SEPARATOR, 0, "");
          }
          AppendMenu (_hSysMenu, MF_STRING, command.Id, command.Text);
        }
        _pendingCommands = null;
      }
    }

    private void onSysCommandMessage (ref Message msg) {
      if ((long)msg.WParam > 0 && (long)msg.WParam <= _lastId) {
        _actions[(int)msg.WParam - 1] ();
      }
    }

    #endregion Private methods

    #region Classes

    private class CommandInfo {
      public int Id { get; set; }
      public string Text { get; set; }
      public Action Action { get; set; }
      public bool Separator { get; set; }
    }

    #endregion Classes
  }
}
