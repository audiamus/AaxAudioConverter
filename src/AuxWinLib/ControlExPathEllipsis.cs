using System.Drawing;
using System.Windows.Forms;

namespace audiamus.aux.win.ex {
  public static class ControlExPathEllipsis {
    public static void SetTextAsPathWithEllipsis (this Control label, string text = null) {
      if (text is null)
        text = (string)label.Tag;
      else
        label.Tag = text;
      string text2 = string.Copy (text);
      Size size = new Size (label.Width - 8, label.Height);
      TextRenderer.MeasureText (text2, label.Font,
        size, TextFormatFlags.ModifyString | TextFormatFlags.PathEllipsis);
      int pos = text2.IndexOf ('\0');
      if (pos >= 0)
        text2 = text2.Substring (0, pos);
      label.Text = text2;
    }
  }
}
