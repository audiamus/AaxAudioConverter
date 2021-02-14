using System.Windows.Forms;

namespace audiamus.aux.win {
  public class VerticalProgressBar : ProgressBar {

    const int PBS_VERTICAL = 0x04;

    protected override CreateParams CreateParams {
      get
      {
        CreateParams cp = base.CreateParams;
        cp.Style |= PBS_VERTICAL;
        return cp;
      }
    }
  }
}
