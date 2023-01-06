using audiamus.aux.diagn;

namespace audiamus.aaxconv.lib {
  public interface INamingSettings : ITitleNamingSettings {
    EFileNaming FileNaming { get; set; }
    ETitleNaming TitleNaming { get; set; }
    ETrackNumbering TrackNumbering { get; set; }
    bool TotalTracks { get; set; }
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
    string OnlineUpdateOthersDeclined { get; set; }
  }

  public interface IAaxCopySettings {
    EAaxCopyMode AaxCopyMode{ get; set; }
    [ToString (typeof(ToStringConverterPath))]
    string AaxCopyDirectory{ get; set; }
  }

  public interface IModeSettings {
    EConvFormat ConvFormat { get; set; }
    EConvMode ConvMode { get; set; }
  }

  public interface INamingAndModeSettings : INamingSettingsEx, IModeSettings {
  }

  public interface IBitRateSettings : IModeSettings {
    bool VariableBitRate { get; set; }
    EReducedBitRate ReducedBitRate { get; set; }
  }

  public interface IRoleTagAssigmentSettings {
    bool? Narrator { get; set; }

    ERoleTagAssignment TagArtist { get; set; }
    ERoleTagAssignment TagAlbumArtist { get; set; }
    ERoleTagAssignment TagComposer { get; set; }
    ERoleTagAssignment TagConductor { get; set; }
  }

  public interface IConvSettings :
    INamingAndModeSettings, IActivationSettings, ITitleSettingsEx,
    IUpdateSetting, IAaxCopySettings, IBitRateSettings,
    IRoleTagAssigmentSettings {
    bool NonParallel { get; }
    int FFmpeg64bitHours { get; }
    [ToString (typeof (ToStringConverterPath))]
    string InputDirectory { get; set; }
    [ToString (typeof (ToStringConverterPath))]
    string OutputDirectory { get; set; }
    [ToString (typeof (ToStringConverterPath))]
    string FFMpegDirectory { get; set; }
    bool RelaxedFFmpegVersionCheck { get; set; }
    bool ConvertByFileDate { get; set; }
    byte TrkDurMins { get; set; }
    string PartNames { get; set; }
    string Genres { get; set; }
    bool FlatFolders { get; set; }
    EFlatFolderNaming FlatFolderNaming { get; set; }
    bool WithSeriesTitle { get; set; }
    byte NumDigitsSeriesSeqNo { get; set; }
    bool FullCaptionBookFolder { get; set; }
    EOutFolderConflict OutFolderConflict {get; set;}
    uint ShortChapterSec { get; set; }
    uint VeryShortChapterSec { get; set; }
    EVerifyAdjustChapterMarks VerifyAdjustChapterMarks { get; set; }
    EPreferEmbeddedChapterTimes PreferEmbeddedChapterTimes { get; set; }
    bool Latin1EncodingForPlaylist { get; set; }
    bool IntermedCopySingle { get; set; }
    EFixAACEncoding FixAACEncoding { get; set; }
    bool AutoLaunchPlayer { get; set; }
    string Version { get; set; }
  }

}
