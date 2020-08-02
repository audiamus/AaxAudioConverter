using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using audiamus.aaxconv.lib.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  interface IActivationCode {
    IEnumerable<string> ActivationCodes { get; }
    bool HasActivationCode { get; }
  }

  class ActivationCode : IActivationCode {

    private readonly IActivationSettings _settings;
    private List<string> _activationCodes = new List<string> ();

    public IEnumerable<uint> NumericCodes => _activationCodes?.Select (s => Convert.ToUInt32 (s, 16)).ToList();
    public IEnumerable<string> ActivationCodes => _activationCodes;
    public bool HasActivationCode => ActivationCodes?.Count () > 0;

    public ActivationCode (IActivationSettings settings) {
      _settings = settings;
      init ();
    }

    private void init () {
      _activationCodes.Clear ();

      if (_settings.ActivationCode.HasValue) {
        Log (2, this, "add from user settings");
        _activationCodes.Add (_settings.ActivationCode.Value.ToHexString ());
      }

      { 
        ActivationCodeRegistry registryCodes = new ActivationCodeRegistry ();
        if (registryCodes.HasActivationCode) {
          Log (2, this, $"add from registry (#={registryCodes.ActivationCodes.Count()})");
          _activationCodes = _activationCodes.Union (registryCodes.ActivationCodes).ToList ();
        }
      }

      {
        ActivationCodeApp appCodes = new ActivationCodeApp ();
        if (appCodes.HasActivationCode) {
          Log (2, this, $"add from app (#={appCodes.ActivationCodes.Count()})");
          _activationCodes = _activationCodes.Union (appCodes.ActivationCodes).ToList ();
        }
      }

      Log (2, this, $"#unique={_activationCodes.Count}");
    }

    public bool ReinitActivationCode () {
      init ();
      return HasActivationCode;
    }
  }

  namespace ex {
    public static class ActivationCodeEx {
      public static string ToHexString (this uint code) {
        return code.ToString ("X8");
      }

      public static string ToHexDashString (this uint? code) {
        if (code.HasValue)
          return code.Value.ToHexDashString ();
        else
          return string.Empty;
      }

      public static string ToHexDashString (this uint code) {
        var bytes = BitConverter.GetBytes (code);
        Array.Reverse (bytes);
        var sb = new StringBuilder ();
        foreach (byte b in bytes) {
          if (sb.Length > 0)
            sb.Append ('-');
          sb.Append (b.ToString ("X2"));
        }
        return sb.ToString ();
      }

      public static uint ToUInt32 (this IEnumerable<string> chars) {
        if (chars.Count () == 4) {
          var sb = new StringBuilder ();
          foreach (string s in chars)
            sb.Append (s);
          try {
            return Convert.ToUInt32 (sb.ToString (), 16);
          } catch (Exception) {
            return 0;
          }
        } else
          return 0;
      }

    }
  }
}
