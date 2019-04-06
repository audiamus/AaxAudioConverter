using System.ComponentModel;
using System.Windows.Forms;
using audiamus.aaxconv.lib;

namespace audiamus.aaxconv {
  public interface ISet {
    void Set (AaxFileItem fileItem);
  }

  public class FileItemForm : Form, ISet {
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    [Browsable (false)]
    public IPreviewTitle Previewer { get; set; }
    public virtual void Set (AaxFileItem fileItem) { }
  }
}
