using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using audiamus.aux.ex;
using static audiamus.aux.EnumUtil;

namespace audiamus.aux {

  public class EnumConverter<TEnum> : TypeConverter 
    where TEnum : struct, Enum 
  {

    protected IList<TEnum> Values { get; }

    private ResourceManager _resourceManager;

    protected ResourceManager ResourceManager {
      get => _resourceManager;
      set
      {
        _resourceManager = value;
        initReverseLookup ();
      }
    }

    Dictionary<string, TEnum> _reverseLookup;

    public EnumConverter () {
      Values = GetValues<TEnum> ().ToArray ();
    }

    public override bool GetStandardValuesSupported (ITypeDescriptorContext context) => true;

    public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context) {
      StandardValuesCollection svc = new StandardValuesCollection (Values.ToArray());
      return svc;
    }


    public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType) {
      if (destinationType != typeof (string))
        return base.CanConvertTo (context, destinationType);
      else
        return true;
    }

    public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType) {
      if (sourceType != typeof (string))
        return base.CanConvertFrom (context, sourceType);
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
        case TEnum enm:
          return toDisplayString (enm);
      }
    }

    public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value) {
#if TRACE && EXTRA
      Trace.WriteLine ($"{nameof (ConvertFrom)}: \"{value}\", from: {value.GetType ().Name}");
#endif
      if (!(_reverseLookup is null)) {
        if (value is string s) {
          bool succ = _reverseLookup.TryGetValue (s, out TEnum e);
          if (succ)
            return e;
        }
      }
      return base.ConvertFrom (context, culture, value);

    }

    private string toDisplayString (TEnum value) => ResourceManager.GetStringEx (value.ToString ());

    private void initReverseLookup () {
      _reverseLookup = new Dictionary<string, TEnum> ();
      foreach (var v in Values)
        _reverseLookup.Add (toDisplayString (v), v);
    }

  }


}
