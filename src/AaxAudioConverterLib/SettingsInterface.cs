namespace audiamus.aaxconv.lib {
  public interface INamingSettings : ITitleNamingSettings {
    EFileNaming FileNaming { get; set; }
    ETitleNaming TitleNaming { get; set; }
    ETrackNumbering TrackNumbering { get; set; }
    bool Narrator { get; set; }
    EGeneralNaming GenreNaming { get; set; }
    string GenreName { get; set; }
    EGeneralNaming ChapterNaming { get; set; }
    string ChapterName { get; set; }
  }

  public interface INamingSettingsEx : INamingSettings {
    EGeneralNaming PartNaming { get; set; }
    string PartName { get; set; }
    bool ExtraMetaFiles { get; set; }
    bool? NamedChaptersAndAlwaysWithNumbers { get; set; }
  }

  public interface IActivationSettings {
    uint? ActivationCode { get; set; }
  }

  public interface ITitleNamingSettings {
    bool SeriesTitleLeft { get; set; }
    bool LongBookTitle { get; set; }
  }


  public interface ITitleSettingsEx : ITitleNamingSettings {
    string AddnlValTitlePunct { get; set; }
  }

  public interface IUpdateSetting {
    bool? OnlineUpdate { get; set; }
  }

  public interface ISettings : INamingSettingsEx, IActivationSettings, ITitleSettingsEx, IUpdateSetting {
    bool NonParallel { get; }
    string InputDirectory { get; set; }
    string OutputDirectory { get; set; }
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
    bool Latin1EncodingForPlaylist { get; set; }
    bool AutoLaunchPlayer { get; set; }
  }

}
