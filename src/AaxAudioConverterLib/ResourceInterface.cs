namespace audiamus.aaxconv.lib {
  public interface IResources {
    string MsgFFmpegVersion1 { get; }// AaxAudioConverter has been tested against FFmpeg version xxxx 
    string MsgFFmpegVersion2 { get; }// The located version is xxxx
    string MsgFFmpegVersion3 { get; }// Try with this version?
    string MsgFFmpegVersion4 { get; }// It can always be altered in the basic settings (via the system menu). 

    string MsgActivationError1 { get; } //Activation Error for
    string MsgActivationError2 { get; } //Will be skipped.
    string MsgActivationError3 { get; } //Will be skipped.

    string MsgDirectoryCreationCallback { get; } //exists and is not empty.\r\n" +
                                              //"Override content, new folder or skip book?\r\n" +
                                              //"[Yes]: Override content\r\n" +
                                              //"[No]: New folder\r\n" +
                                              //"[Cancel]: Skip book"

    string PartNames { get; }

    string MsgOnlineUpdateNewVersion { get; }
    string MsgOnlineUpdateDownload { get; }
    string MsgOnlineUpdateInstall { get; }
    string MsgOnlineUpdateInstallNow { get; }
    string MsgOnlineUpdateInstallLater { get; }

  }
}
