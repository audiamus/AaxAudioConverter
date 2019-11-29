using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace audiamus.aux {
  public abstract class Encoding : System.Text.Encoding {
    public static System.Text.Encoding Latin1 => System.Text.Encoding.GetEncoding ("ISO-8859-1");
  }
}
