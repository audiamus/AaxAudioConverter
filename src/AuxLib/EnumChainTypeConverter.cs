using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using audiamus.aux.ex;
using static audiamus.aux.EnumUtil;

namespace audiamus.aux {
  public class EnumChainConverter<TEnum, TPunct> : TypeConverter
    where TEnum : struct, Enum 
    where TPunct : class, IChainPunctuation, new() 
  {
    //const char USCORE = '_';
    readonly TEnum[] _values;
    //readonly IChainPunctuation _punct;

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

    public EnumChainConverter () {
      //_punct = Singleton<TPunct>.Instance;
      _values = GetValues<TEnum> ().ToArray();
    }

    public override bool GetStandardValuesSupported (ITypeDescriptorContext context) => true;

    public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context) {
      StandardValuesCollection svc = new StandardValuesCollection (_values);
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
          return toDisplayString(enm);
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

    //static readonly byte __a = Convert.ToByte ('a');

    private string toDisplayString (TEnum value) => value.ToDisplayString<TEnum, TPunct> (ResourceManager);

    //private string toDisplayString (TEnum value) {
    //  string sval = value.ToString ();
    //  string[] parts = sval.Split (USCORE);

    //  bool noSubstitutes = parts.Select (s => s.Length).Min () > 1;
    //  StringBuilder sb = new StringBuilder ();
    //  if (noSubstitutes) {
    //    for (int i = 0; i < parts.Length; i++)
    //      parts[i] = _punct.Prefix + ResourceManager.GetStringEx (parts[i]) + _punct.Suffix;
    //    foreach (string s in parts) {
    //      if (sb.Length > 0)
    //        sb.Append (_punct.Infix?[0]);
    //      sb.Append (s);
    //    }
    //  } else {
    //    for (int i = 0; i < parts.Length; i++) {
    //      if (parts[i].Length > 1)
    //        parts[i] = _punct.Prefix + ResourceManager.GetStringEx (parts[i]) + _punct.Suffix;
    //      else {
    //        byte x = Convert.ToByte (parts[i][0]);
    //        try {
    //          parts[i] = _punct.Infix?[x - __a];
    //        } catch (IndexOutOfRangeException) {
    //          parts[i] = string.Empty;
    //        }
    //      }
    //    }
    //    foreach (string s in parts)
    //      sb.Append (s);
    //  }
    //  return sb.ToString ();
    //}

    private void initReverseLookup () {
      _reverseLookup = new Dictionary<string, TEnum> ();
      foreach (var v in _values)
        initReverseLookup (v);
    }

    private void initReverseLookup (TEnum value) => _reverseLookup.Add (toDisplayString (value), value);

  }



}
