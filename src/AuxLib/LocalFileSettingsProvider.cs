using System;
using System.Configuration;
using System.IO;
using static audiamus.aux.ApplEnv;

namespace audiamus.aux {
  /// <summary>
  /// <para>Customized version of default application settings provider.</para> 
  /// <para>Maintains a copy of user.config in AppData\Local\[company]\_nohash\[appname]. 
  /// Takes this copy, if it exists, as input by copying to the hash-name folder, before reading. 
  /// Copies, after writing, the output from the hash-name folder.</para> 
  /// <para>Pros: Independent of .exe location and version.</para>
  /// <para>Cons: Potential app name and version conflicts.</para>
  /// <para>To use: 
  /// Find Settings.Designer.cs (under Properties | Settings.settings).  
  /// Decorate "internal sealed partial class Settings" with attribute: 
  /// [System.Configuration.SettingsProvider (typeof (aux.LocalFileSettingsProvider))]
  /// </para>
  /// </summary>
  /// <seealso cref="System.Configuration.LocalFileSettingsProvider" />
  public class LocalFileSettingsProvider : System.Configuration.LocalFileSettingsProvider {
    static readonly string GEN_PATH;
    static readonly string SPEC_PATH;

    static readonly char[] INVALID_CHARS = Path.GetInvalidFileNameChars ();


    static LocalFileSettingsProvider () {
      SPEC_PATH = ConfigurationManager.OpenExeConfiguration (ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
      string dir = LocalCompanyDirectory;
      dir = Path.Combine (dir, "_nohash", ApplName);
      var path = Path.Combine (dir, Path.GetFileName (SPEC_PATH));
      GEN_PATH = path;
    }

    public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context, SettingsPropertyCollection properties) {
      try {
        if (File.Exists (GEN_PATH)) {
          Directory.CreateDirectory (Path.GetDirectoryName (SPEC_PATH));
          File.Copy (GEN_PATH, SPEC_PATH, true);
        }
      } catch (Exception) {
      }
      return base.GetPropertyValues (context, properties);
    }

    public override void SetPropertyValues (SettingsContext context, SettingsPropertyValueCollection values) {
      base.SetPropertyValues (context, values);
      try {
        if (File.Exists (SPEC_PATH)) {
          Directory.CreateDirectory (Path.GetDirectoryName (GEN_PATH));
          File.Copy (SPEC_PATH, GEN_PATH, true);
        }
      } catch (Exception) {
      }
    }
  }
}
