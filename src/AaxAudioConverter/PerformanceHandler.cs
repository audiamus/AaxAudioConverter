using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using audiamus.perf;

namespace audiamus.aaxconv {
  class PerformanceHandler {
    private readonly ProgressBar _pbProcesses;
    private readonly ProgressBar _pbLoadPercent;
    private readonly ToolTip _tt;
    private readonly string _tttProcesses;
    private readonly string _tttLoad;

    public PerformanceHandler (ProgressBar pbProcesses, ProgressBar pbLoadPercent, ToolTip tt = null) {
      _pbProcesses = pbProcesses;
      _pbLoadPercent = pbLoadPercent;

      _tt = tt;
      _tttProcesses = tt?.GetToolTip (pbProcesses);
      _tttLoad = tt?.GetToolTip (pbLoadPercent);
    }

    public void Reset () {
      _pbProcesses.Value = 0;
      _pbLoadPercent.Value = 0;
      _tt.SetToolTip (_pbProcesses, _tttProcesses);
      _tt.SetToolTip (_pbLoadPercent, _tttLoad);
    }

    public void Update (IPerfCallback callback) {
      update (callback.Processes, _pbProcesses, _tttProcesses);
      update (callback.LoadPercent, _pbLoadPercent, _tttLoad, "%");
    } 

    private void update (IValueMax vm, ProgressBar pb, string ttt, string suffix = null) {
      if (vm is null)
        return;

      if (vm.Max != pb.Maximum)
        pb.Maximum = vm.Max;

      int val = Math.Min (vm.Value, pb.Maximum);
      pb.Value = val;
      if (_tt is null || ttt is null)
        return;

      var caption = $"{ttt} : {val}{suffix}";
      _tt.SetToolTip (pb, caption);
    }
  }
}
