namespace audiamus.aaxconv.lib {
  public interface IPreviewTitle {
    IConvSettings Settings { get; }
    (string tag, string file) TitlePreview (AaxFileItem fileItem);
    string PruneTitle (string title);
    string GetGenre (AaxFileItem fileItem);
  }

  public interface IAudioQuality {
    uint SampleRate { get; }
    uint AvgBitRate { get; }
  }
}
