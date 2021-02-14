using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace audiamus.aux {
  public interface IProcessList {
    bool Add (Process process);
    bool Remove (Process process);
  }
}
