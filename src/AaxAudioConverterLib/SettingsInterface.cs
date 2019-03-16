namespace audiamus.aaxconv.lib {
  public interface INamingSettings {
    EFileNaming FileNaming { get; set; }
    ETitleNaming TitleNaming { get; set; }
    ETrackNumbering TrackNumbering { get; set; }
    bool Narrator { get; set; }
    EGeneralNaming GenreNaming { get; set; }
    string GenreName { get; set; }
    EGeneralNaming ChapterNaming { get; set; }
    string ChapterName { get; set; }
    EGeneralNaming PartNaming { get; set; }
    string PartName { get; set; }
  }

  public interface IActivationSettings {
    uint? ActivationCode { get; set; }
  }

  public interface ITitlePunctuationSettings {
    string AddnlValTitlePunct { get; set; }
  }

  public interface ISettings : INamingSettings, IActivationSettings, ITitlePunctuationSettings {
    bool NonParallel { get; }
    string InputDirectory { get; set; }
    string OutputDirectory { get; set; }
    string FFMpegDirectory { get; set; }
    EConvFormat ConvFormat { get; set; }
    EConvMode ConvMode { get; set; }
    byte TrkDurMins { get; set; }
    string PartNames { get; set; }
  }

}
