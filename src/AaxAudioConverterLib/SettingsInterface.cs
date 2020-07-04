using audiamus.aux.diagn;

namespace audiamus.aaxconv.lib {
  public interface INamingSettings : ITitleNamingSettings {
    EFileNaming FileNaming { get; set; }
    ETitleNaming TitleNaming { get; set; }
    ETrackNumbering TrackNumbering { get; set; }
    bool Narrator { get; set; }
    EGeneralNaming GenreNaming { get; set; }
    string GenreName { get; set; }
    EGeneralNamingEx ChapterNaming { get; set; }
    string ChapterName { get; set; }
  }

  public interface INamingSettingsEx : INamingSettings {
    EGeneralNaming PartNaming { get; set; }
    string PartName { get; set; }
    bool ExtraMetaFiles { get; set; }
    ENamedChapters NamedChapters { get; set; }
    bool M4B { get; set; }
  }

  public interface IActivationSettings {
    [ToString (typeof(ToStringConverterActivationCode))]
    uint? ActivationCode { get; set; }
  }

  public interface ITitleNamingSettings {
    bool SeriesTitleLeft { get; set; }
    ELongTitle LongBookTitle { get; set; }
  }


  public interface ITitleSettingsEx : ITitleNamingSettings {
    string AddnlValTitlePunct { get; set; }
  }

  public interface IUpdateSetting {
    EOnlineUpdate OnlineUpdate { get; set; }
  }

  public interface IAaxCopySettings {
    EAaxCopyMode AaxCopyMode{ get; set; }
    [ToString (typeof(ToStringConverterPath))]
    string AaxCopyDirectory{ get; set; }
  }

  public interface IConvSettings : INamingSettingsEx, IActivationSettings, ITitleSettingsEx, IUpdateSetting, IAaxCopySettings {
    bool NonParallel { get; }
    [ToString (typeof(ToStringConverterPath))]
    string InputDirectory { get; set; }
    [ToString (typeof(ToStringConverterPath))]
    string OutputDirectory { get; set; }
    [ToString (typeof(ToStringConverterPath))]
    string FFMpegDirectory { get; set; }
    EConvFormat ConvFormat { get; set; }
    EConvMode ConvMode { get; set; }
    byte TrkDurMins { get; set; }
    string PartNames { get; set; }
    string Genres { get; set; }
    bool FlatFolders { get; set; }
    EFlatFolderNaming FlatFolderNaming { get; set; }
    uint ShortChapterSec { get; set; }
    uint VeryShortChapterSec { get; set; }
    EVerifyAdjustChapters VerifyAdjustChapters { get; set; }
    bool Latin1EncodingForPlaylist { get; set; }
    bool AutoLaunchPlayer { get; set; }
    string Version { get; set; }
  }

}
