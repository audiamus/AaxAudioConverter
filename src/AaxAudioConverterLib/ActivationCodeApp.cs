using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using audiamus.aaxconv.lib.ex;
using audiamus.aux;

namespace audiamus.aaxconv.lib {
  class ActivationCodeApp : IActivationCode {

    const string PACKAGES = "Packages";
    const string AUDIBLEINC_AUDIBLE = "AudibleInc.Audible";
    const string LOCAL_STATE = "LocalState";
    const string AUDIBLEACTIVATION_SYS = "AudibleActivation.sys";

    readonly IEnumerable<string> _activationCodes;

    public IEnumerable<string> ActivationCodes => _activationCodes;
    public bool HasActivationCode => ActivationCodes?.Count () > 0;


    public ActivationCodeApp () => _activationCodes = getCodes ();

    public static IEnumerable<string> GetPackageDirectories () {
      string path = Path.Combine (ApplEnv.LocalDirectoryRoot, PACKAGES);
      if (!Directory.Exists (path))
        return null;
      string[] dirs = Directory.GetDirectories (path, $"{AUDIBLEINC_AUDIBLE}*");
      List<DirectoryInfo> dirInfos = new List<DirectoryInfo> ();
      foreach (var dir in dirs) {
        string dirLocalState = Path.Combine (dir, LOCAL_STATE);
        if (Directory.Exists (dirLocalState))
          dirInfos.Add (new DirectoryInfo (dirLocalState));
      }
      dirInfos.Sort ((x, y) => 
        DateTime.Compare (y.LastWriteTimeUtc, x.LastWriteTimeUtc));

      return dirInfos.Select (di => di.FullName);
    } 

    private static IEnumerable<string> getCodes () {
      string path = Path.Combine (ApplEnv.LocalDirectoryRoot, PACKAGES);
      if (!Directory.Exists (path))
        return null;
      string[] dirs = Directory.GetDirectories (path, $"{AUDIBLEINC_AUDIBLE}*");
      List<FileInfo> fileInfos = new List<FileInfo>();
      foreach (var dir in dirs) {
        string file = Path.Combine (dir, LOCAL_STATE, AUDIBLEACTIVATION_SYS);
        if (File.Exists (file))
          fileInfos.Add (new FileInfo (file));
      }
      fileInfos.Sort ((x, y) => 
        DateTime.Compare (y.LastWriteTimeUtc, x.LastWriteTimeUtc));

      List<string> result = new List<string> ();
      foreach (var fi in fileInfos)
        getCode (fi.FullName, result);

      return result;
    }

    private static void getCode (string path, List<string> result) {
      using (var reader = new BinaryReader (File.OpenRead (path))) {
        try {
          uint code = reader.ReadUInt32 ();

          string hex = code.ToHexString ();

          if (result.IndexOf (hex) < 0)
            result.Add (hex);

        } catch (EndOfStreamException) { }
      }
    }
  }

}
