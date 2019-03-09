using System;
using System.Windows.Forms;

namespace audiamus.aux.win {
  public class InteractionCallbackHandler : IInteractionCallback<InteractionMessage, bool?> {
    private readonly Control _parent;
    private Control Parent => _parent;
    
    public InteractionCallbackHandler (Control parent) {
      _parent = parent;
    }

    public virtual bool? Interact (InteractionMessage im) {
      switch (im.Type) {
        default:
          MsgBox.Show (Parent, im.Message, Parent.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
          break;
        case ECallbackType.info:
          MsgBox.Show (Parent, im.Message, Parent.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
          break;
        case ECallbackType.infoCancel:
          return MsgBox.Show (Parent, im.Message, Parent.Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK;
        case ECallbackType.warning:
          MsgBox.Show (Parent, im.Message, Parent.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
          break;
        case ECallbackType.error:
          MsgBox.Show (Parent, im.Message, Parent.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
          break;
        case ECallbackType.errorQuestion:
          return MsgBox.Show (Parent, im.Message, Parent.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes;
        case ECallbackType.question:
          return MsgBox.Show (Parent, im.Message, Parent.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        case ECallbackType.question3: {
            var result = MsgBox.Show (Parent, im.Message, Parent.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            switch (result) {
              default:
                return null;
              case DialogResult.Yes:
                return true;
              case DialogResult.No:
                return false;
            }
          }
      }

      return null;
    }

  }

  public class InteractionCallbackHandler<T> : InteractionCallbackHandler where T : struct, Enum {

    private readonly Func<InteractionMessage<T>, bool?> _customHandler;
    private Func<InteractionMessage<T>, bool?> CustomHandler => _customHandler;
    

    public InteractionCallbackHandler (Control parent, Func<InteractionMessage<T>, bool?> customHandler) : base (parent) {
      _customHandler = customHandler;
    }

    public override bool? Interact (InteractionMessage im) {
      switch (im) {
        default:
          return base.Interact (im);
        case InteractionMessage<T> imt:
          switch (imt.Type) {
            default:
              return base.Interact (imt);
            case ECallbackType.custom:
              return CustomHandler?.Invoke (imt);
          }
      }
    }
  }
}
