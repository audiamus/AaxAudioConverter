using System;
using System.Resources;

namespace audiamus.aux.ex {
  public static class ResourceManagerEx {

    public static ResourceManager GetDefaultResourceManager (this object o) {
      return o.GetType().GetDefaultResourceManager ();
    }

    public static ResourceManager GetDefaultResourceManager (this Type type) {
      string tableName = type.Namespace + ".Properties.Resources";
      var assembly = type.Assembly;
      var rm = new ResourceManager (tableName, assembly);
      return rm;
    }

    public static ResourceManager GetTypeResourceManager (this object o) {
      return o.GetType().GetTypeResourceManager();
    }

    public static ResourceManager GetTypeResourceManager (this Type type) {
      string tableName = type.FullName;
      var assembly = type.Assembly;
      var rm = new ResourceManager (tableName, assembly);
      return rm;
    }

    public static string GetStringEx (this ResourceManager rm, string val) {
      if (rm is null)
        return val;
      string s = null;
      try {
        s = rm.GetString (val.ToLowerInvariant());
      } catch (MissingManifestResourceException) {
      }
      return s ?? val;
    }
  }
}
