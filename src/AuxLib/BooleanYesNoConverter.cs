using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using audiamus.aux.ex;

namespace audiamus.aux {
  public class BooleanYesNoConverter : BooleanConverter {

    const string TRUE = "Yes";
    const string FALSE = "No";

    private ResourceManager _resourceManager;

    protected ResourceManager ResourceManager {
      get => _resourceManager;
      set
      {
        _resourceManager = value;
        initReverseLookup ();
      }
    }

    Dictionary<string, bool> _reverseLookup;

    public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType) {
      if (destinationType != typeof (string))
        return base.CanConvertTo (context, destinationType);
      else
        return true;
    }


    public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
#if TRACE && EXTRA
      Trace.WriteLine ($"{nameof (ConvertTo)}: \"{value}\", from: {value.GetType ().Name}, to: {destinationType.Name}");
#endif
      switch (value) {
        default:
          return base.ConvertTo (context, culture, value, destinationType);
        case bool val:
          return toDisplayString (val);
      }
    }

    public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value) {
#if TRACE && EXTRA
      Trace.WriteLine ($"{nameof (ConvertFrom)}: \"{value}\", from: {value.GetType ().Name}");
#endif
      if (!(_reverseLookup is null)) {
        if (value is string s) {
          bool succ = _reverseLookup.TryGetValue (s, out bool b);
          if (succ)
            return b;
        }
      }
      return base.ConvertFrom (context, culture, value);

    }

    private string toDisplayString (bool value) {
      string s = value ? TRUE : FALSE;
      return ResourceManager.GetStringEx (s);
    }

    private void initReverseLookup () {
      _reverseLookup = new Dictionary<string, bool> ();
      initReverseLookup (false);
      initReverseLookup (true);
    }

    private void initReverseLookup (bool value) => _reverseLookup.Add (toDisplayString (value), value); 

  }


}
