using System.Drawing;
using System.Windows.Forms;

using static System.Windows.Forms.Design.AxImporter;

namespace audiamus.aux.win.ex {
  public static class ControlExPathEllipsis {
    public static void SetTextAsPathWithEllipsis (this Control label, string text = null) {
      if (text is null)
        text = (string)label.Tag;
      else
        label.Tag = text;
      string text2 = Compact(text, label);
      label.Text = text2;
    }
    // adapted from https://www.codeproject.com/Articles/37503/Auto-Ellipsis
    private static string Compact(string text, Control ctrl)
    {
        using Graphics dc = ctrl.CreateGraphics();
        Size s = TextRenderer.MeasureText(dc, text, ctrl.Font);

        // control is large enough to display the whole text 
        if (s.Width <= ctrl.Width - 8)
            return text;

        int len = 0;
        int seg = text.Length;
        string fit = "";

        // find the longest string that fits into
        // the control boundaries using bisection method 
        while (seg > 1)
        {
            seg -= seg / 2;

            int left = len + seg;
            int right = text.Length;
            if (left > right)
                continue;

            // build and measure a candidate string with ellipsis
            string tst = text.Substring(0, left) +
                "..." + text.Substring(right);

            s = TextRenderer.MeasureText(dc, tst, ctrl.Font);

            // candidate string fits into control boundaries, 
            // try a longer string
            // stop when seg <= 1 
            if (s.Width <= ctrl.Width - 8)
            {
                len += seg;
                fit = tst;
            }
        }

        return len == 0 ? "..." : fit;
    }
    }
}
