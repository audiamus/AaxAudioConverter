using System;
using System.Collections.Generic;
using System.Linq;
using audiamus.aaxconv.lib.ex;
using Microsoft.Win32;

namespace audiamus.aaxconv.lib {
  class ActivationCodeRegistry : IActivationCode {

    const string SOFTWARE = "SOFTWARE";
    const string WOW6432NODE = "Wow6432Node";
    const string AUDIBLE = "Audible";
    const string SWGIDMAP = "SWGIDMAP";

    readonly IEnumerable<string> _registryActivationCodes;

    public IEnumerable<uint> RegistryCodes => _registryActivationCodes?.Select (s => Convert.ToUInt32 (s, 16)).ToList();
    public IEnumerable<string> ActivationCodes => _registryActivationCodes;
    public bool HasActivationCode => ActivationCodes?.Count () > 0;


    public ActivationCodeRegistry () => _registryActivationCodes = getBytes ();

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

}
