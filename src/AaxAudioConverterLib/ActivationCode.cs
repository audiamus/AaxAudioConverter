using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using audiamus.aaxconv.lib.ex;
using Microsoft.Win32;

namespace audiamus.aaxconv.lib {
  public class ActivationCode {

    const string SOFTWARE = "SOFTWARE";
    const string WOW6432NODE = "Wow6432Node";
    const string AUDIBLE = "Audible";
    const string SWGIDMAP = "SWGIDMAP";

    readonly IEnumerable<string> _registryActivationCodes;
    readonly IActivationSettings _settings;

    public IEnumerable<uint> RegistryCodes => _registryActivationCodes?.Select (s => Convert.ToUInt32 (s, 16)).ToList();
    public IEnumerable<string> ActivationCodes => activationCodes ();
    public bool HasActivationCode => ActivationCodes?.Count () > 0;

    private IActivationSettings Settings => _settings;

    public ActivationCode (IActivationSettings settings) {
      _settings = settings;

      _registryActivationCodes = getBytes ();
    } 

    private IEnumerable<string> activationCodes () {
      if (!Settings.ActivationCode.HasValue)
        return _registryActivationCodes;

      var list = new List<string> { Settings.ActivationCode.Value.ToHexString () };
      if (_registryActivationCodes is null)
        return list;

      list = list.Union(_registryActivationCodes).ToList ();
      return list;
    }


    private static IEnumerable<string> getBytes () {
      var activationBytes = new List<String> ();
      var rk = getKey ();
      if (rk is null)
        return null;
      if (rk.GetValueNames ().Length == 0) {
        rk.Dispose ();
        rk = getKey (true);
      }
      if (rk is null)
        return null;
      if (rk.GetValueNames ().Length == 0) {
        rk.Dispose ();
        return null;
      }

      using (rk) {
        var valNames = rk.GetValueNames ();

        for (int i = valNames.Length - 1; i >= 0; i--) {
          bool succ = uint.TryParse (valNames[i], out uint k);
          if (!succ)
            continue;
          if (k > 8)
            continue;
          byte[] bytes = rk.GetValue (valNames[i]) as byte[];
          if (bytes is null || bytes.Length < 4)
            continue;
          uint val = BitConverter.ToUInt32 (bytes, 0);
          if (val == 0xffffffff)
            continue;

          string hex = val.ToHexString ();

          if (activationBytes.IndexOf (hex) < 0)
            activationBytes.Add (hex);
        }

        return activationBytes;
      }
    }

    private static RegistryKey getKey (bool wow = false) {
      RegistryKey hklm = Registry.LocalMachine;
      string keyPath;
      if (wow)
        keyPath = $"{SOFTWARE}\\{WOW6432NODE}\\{AUDIBLE}\\{SWGIDMAP}";
      else
        keyPath = $"{SOFTWARE}\\{AUDIBLE}\\{SWGIDMAP}";
      return hklm.OpenSubKey (keyPath);
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
