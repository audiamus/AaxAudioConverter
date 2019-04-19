using audiamus.aaxconv.lib;

namespace audiamus.aaxconv {

  using R = Properties.Resources;

  class Resources : IResources {
    private static Resources __resources;

    public static Resources Default {
      get
      {
        if (__resources is null)
          __resources = new Resources ();
        return __resources;
      }
    }

    public string MsgFFmpegVersion1 => R.MsgFFmpegVersion1;
    public string MsgFFmpegVersion2 => R.MsgFFmpegVersion2;
    public string MsgFFmpegVersion3 => R.MsgFFmpegVersion3;
    public string MsgFFmpegVersion4 => R.MsgFFmpegVersion4;
    public string MsgActivationError1 => R.MsgActivationError1;
    public string MsgActivationError2 => R.MsgActivationError2;
    public string MsgActivationError3 => R.MsgActivationError3;
    public string MsgDirectoryCreationCallback => R.MsgDirectoryCreationCallback;
    public string PartNames => R.PartNames;
    public string MsgOnlineUpdateNewVersion => R.MsgOnlineUpdateNewVersion;
    public string MsgOnlineUpdateDownload => R.MsgOnlineUpdateDownload;
    public string MsgOnlineUpdateInstall => R.MsgOnlineUpdateInstall;
    public string MsgOnlineUpdateInstallNow => R.MsgOnlineUpdateInstallNow;
    public string MsgOnlineUpdateInstallLater => R.MsgOnlineUpdateInstallLater;
  }
}
