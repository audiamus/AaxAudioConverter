using System.Resources;
using audiamus.aaxconv.lib;
using audiamus.aux.ex;

namespace audiamus.aaxconv {

  using R = Properties.Resources;

  class Resources : IResources {
    private static Resources __resources;
    private ResourceManager ResourceManager { get; }

    public static Resources Default {
      get
      {
        if (__resources is null) {
          __resources = new Resources ();
        }
        return __resources;
      }
    }

    private Resources () => ResourceManager = this.GetDefaultResourceManager ();

    public string MsgFFmpegVersion1 => R.MsgFFmpegVersion1;
    public string MsgFFmpegVersion2 => R.MsgFFmpegVersion2;
    public string MsgFFmpegVersion3 => R.MsgFFmpegVersion3;
    public string MsgFFmpegVersion4 => R.MsgFFmpegVersion4;
    public string MsgActivationError1 => R.MsgActivationError1;
    public string MsgActivationError2 => R.MsgActivationError2;
    public string MsgActivationError3 => R.MsgActivationError3;
    public string MsgInvalidInternalFileType => R.MsgInvalidInternalFileType;
    public string MsgDirectoryCreationCallback => R.MsgDirectoryCreationCallback;
    public string MsgDirectoryPartCreationCallback => R.MsgDirectoryPartCreationCallback;
    public string MsgDirectoryCreationCallbackForAll => R.MsgDirectoryCreationCallbackForAll;
    public string PartNames => R.PartNames;
    public string MsgOnlineUpdateNewVersion => R.MsgOnlineUpdateNewVersion;
    public string MsgOnlineUpdateDownload => R.MsgOnlineUpdateDownload;
    public string MsgOnlineUpdateInstall => R.MsgOnlineUpdateInstall;
    public string MsgOnlineUpdateInstallNow => R.MsgOnlineUpdateInstallNow;
    public string MsgOnlineUpdateInstallLater => R.MsgOnlineUpdateInstallLater;

    public string HdrAuthor => R.HdrAuthor;
    public string HdrTitle => R.HdrTitle;
    public string HdrNarrator => R.HdrNarrator;
    public string HdrDuration => R.HdrDuration;
    public string HdrYear => R.HdrYear;
    public string HdrPublisher => R.HdrPublisher;
    public string HdrCopyright => R.HdrCopyright;
    public string HdrGenre => R.HdrGenre;
    public string HdrSampleRate => R.HdrSampleRate;
    public string HdrBitRate => R.HdrBitRate;

    public string PartNamePrefixStandard => ResourceManager.GetStringEx ("Part");
    public string ChapterNamePrefixStandard => ResourceManager.GetStringEx ("Chapter");


  }
}
