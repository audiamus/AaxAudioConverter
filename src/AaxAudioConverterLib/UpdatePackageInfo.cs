using System;

namespace audiamus.aaxconv.lib {

  public interface IPackageInfo {
    string AppName { get; }
    Version Version { get; }
    bool Preview { get; }
    bool DefaultApp { get; }
    string Descript { get; }
  }

  public class UpdateInteractionMessage {
    public EUpdateInteract Kind { get; }
    public IPackageInfo PckInfo { get; }
    public string RefAppName { get; }
    
    public UpdateInteractionMessage (EUpdateInteract kind, IPackageInfo pckInfo, string refAppName = null) {
      Kind = kind;
      PckInfo = pckInfo;
      RefAppName = refAppName;
    }
  }

  public class PackageInfo {
    public string Url { get; }
    public string AppName { get; }
    public string Version { get; }
    public bool Preview { get; }
    public string Descript { get; }
    public string Md5 { get; }
    public string InfoLinkUrl { get; }

    //public PackageInfo (string infoLinkUrl) {
    //  InfoLinkUrl = infoLinkUrl;
    //}

    public PackageInfo (
      string url,
      string appName,
      string version,
      bool preview,
      string descript,
      string md5,
      string infoLinkUrl
    ) {
      Url = url;
      AppName = appName;
      Version = version;
      Preview = preview;
      Descript = descript;
      Md5 = md5;
      InfoLinkUrl = infoLinkUrl;
    }

    protected PackageInfo () { }
  }

  public class PackageInfoLocal : PackageInfo, IPackageInfo {
    public new Version Version { get; }
    public string SetupFile { get; set; }
    public bool DefaultApp { get; set; }

    public PackageInfoLocal () { }

    public PackageInfoLocal (PackageInfo pi) : base (pi.Url, pi.AppName, pi.Version, pi.Preview, pi.Descript, pi.Md5, pi.InfoLinkUrl) {
      Version = tryParse (pi.Version);
    }

    private static Version tryParse (string s) {
      bool succ = Version.TryParse (s, out Version version);
      return succ ? version : null;
    }

  }


}


