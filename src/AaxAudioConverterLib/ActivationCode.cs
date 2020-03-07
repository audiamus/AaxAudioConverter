using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using audiamus.aaxconv.lib.ex;

namespace audiamus.aaxconv.lib {
  interface IActivationCode {
    IEnumerable<string> ActivationCodes { get; }
    bool HasActivationCode { get; }
  }

  class ActivationCode : IActivationCode {

    private readonly IActivationSettings _settings;
    private List<string> _activationCodes = new List<string> ();

    public IEnumerable<uint> RegistryCodes => _activationCodes?.Select (s => Convert.ToUInt32 (s, 16)).ToList();
    public IEnumerable<string> ActivationCodes => _activationCodes;
    public bool HasActivationCode => ActivationCodes?.Count () > 0;

    public ActivationCode (IActivationSettings settings) {
      _settings = settings;
      init ();
    }

    private void init () {
      _activationCodes.Clear ();

      if (_settings.ActivationCode.HasValue)
        _activationCodes.Add (_settings.ActivationCode.Value.ToHexString ());

      if (!HasActivationCode) {
        ActivationCodeRegistry registryCodes = new ActivationCodeRegistry ();
        if (registryCodes.HasActivationCode)
          _activationCodes = _activationCodes.Union (registryCodes.ActivationCodes).ToList ();
      }

      if (!HasActivationCode) {
        ActivationCodeApp appCodes = new ActivationCodeApp ();
        if (appCodes.HasActivationCode)
          _activationCodes = _activationCodes.Union (appCodes.ActivationCodes).ToList ();
      }
    }

    public bool GetActivationCode () {
      if (HasActivationCode)
        return true;

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
