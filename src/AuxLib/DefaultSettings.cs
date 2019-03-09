using System.ComponentModel;
using System.Configuration;

namespace audiamus.aux {
  public class DefaultSettings {
    /// <summary>
    /// Note: ApplicationSettingsBase.Reset () appears not to do this job
    /// </summary>
    /// <param name="settings"></param>
    static public void ResetToDefault (ApplicationSettingsBase settings) {
      foreach (var o in settings.Properties) {
        if (o is SettingsProperty prop) {
          var val = TypeDescriptor.GetConverter (prop.PropertyType).ConvertFromString (prop.DefaultValue as string);
          settings[prop.Name] = val;
        }
      }
    }

  }
}

