using System;
using audiamus.aux.diagn;
using audiamus.aux.ex;

namespace audiamus.aaxconv.lib {
  class ToStringConverterActivationCode : ToStringConverter {
    public override string ToString (object o, string format = null) {
      try {
        uint? ac = (uint?)o;
        return ac.HasValue ? "XXXXXXXX" : null;
      } catch ( Exception ) {
        return null;
      }
    }
  }

  class ToStringConverterPath : ToStringConverter {
    public override string ToString (object o, string format = null) {
      if (o is string s)
        return s.SubstitUser ();
      else
        return null;
    }
  }
}
