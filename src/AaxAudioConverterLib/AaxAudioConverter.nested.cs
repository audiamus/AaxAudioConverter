using System;
using System.Linq;
using System.Text;
using static audiamus.aux.ApplEnv;

namespace audiamus.aaxconv.lib {
  public partial class AaxAudioConverter : IPreviewTitle, IDisposable {
    #region nested classes
    class DegreeOfParallelism {

      private readonly IConvSettings _settings;
      private IConvSettings Settings => _settings;

      public int Book {
        get
        {
          if (Settings.ConvMode != EConvMode.single)
            return 1;
          return Math.Min (4, ProcessorCount);
        }
      }

      public int Part {
        get
        {
          if (Settings.ConvMode != EConvMode.single)
            return 1;
          return Math.Max (ProcessorCount / Book, 1);
        }
      }

      public int Chapter { get; private set; }
      public int Track { get; private set; }

      public DegreeOfParallelism (IConvSettings settings) {
        _settings = settings;
      }

      public void SetChapterTrack (ChapteredTracks.PartChapters partChapters) {

        switch (Settings.ConvMode) {
          case EConvMode.single:
            Chapter = 1;
            Track = 1;
            break;

          case EConvMode.chapters:
            Chapter = ProcessorCount;
            Track = 1;
            break;

          case EConvMode.splitChapters: 
            {
              // for very long chapters we can raise track parallelism
              var countedTracks = partChapters.Chapters.Select (c => c.Tracks.Count);
              double avg = countedTracks.Average ();
              int min = countedTracks.Min ();
              int trackParallelism = 1;
              if (avg > 2)
                trackParallelism = 2;
              if (avg > 4 && min > 2)
                trackParallelism = 4;
              if (avg > 4 && ProcessorCount >= 4) {
                trackParallelism = ProcessorCount;
                while (true) {
                  if (trackParallelism > avg)
                    trackParallelism /= 2;
                  else
                    break;
                }
              }

              Track = Math.Min (trackParallelism, ProcessorCount);
              Chapter = Math.Max (ProcessorCount / Track, 1);

              //var part = partChapters.Part;
              //bool transcode = part.IsTrancodeConversion (_settings);

              //if (transcode) {
              //  Track = Math.Min (trackParallelism, ProcessorCount);
              //  Chapter = Math.Max (ProcessorCount / Track, 1);
              //} else {
              //  Track = Math.Min (2, ProcessorCount);
              //  Chapter = Math.Max (ProcessorCount / Track, 1);
              //}
              break;
            }

          case EConvMode.splitTime:
            Chapter = 1;
            Track = ProcessorCount;
            break;
        }
      }
    }

    class MultiBookInitDirectoryHandling {
      public bool? HasBeenAnswered { get; set; }
      public bool? Answer { get; set; }
    }

    class FileError {
      public string SourceFile { get; }
      public IChapter Chapter { get; }
      public ITrack Track { get; }

      public FileError (string sourceFile, IChapter chapter, ITrack track) {
        SourceFile = sourceFile;
        Chapter = chapter;
        Track = track;
      }

      public string ToShortString () {
        var sb = new StringBuilder ();
        sb.Append (SourceFile);
        if (Chapter != null)
          sb.Append ($",{Environment.NewLine}  {Chapter.Name}");
        else if (Track?.Chapter != null)
          sb.Append ($",{Environment.NewLine}  \"{Track.Chapter.Name}\"");
        return sb.ToString ();
      }

      public override string ToString () {
        var sb = new StringBuilder ();
        sb.Append ($"\"{SourceFile}\"");
        if (Chapter != null) {
          sb.Append ($",{Environment.NewLine}  \"{Chapter.Name}\"");
          sb.Append ($",{Environment.NewLine}  {Chapter.Time}");
        }
        if (Track != null) {
          if (Track.Chapter != null) {
            sb.Append ($",{Environment.NewLine}  \"{Track.Chapter.Name}\"");
            sb.Append ($",{Environment.NewLine}  {Track.Chapter.Time}");
          }
          sb.Append ($",{Environment.NewLine}  {Track.Time}");
        }
        return sb.ToString ();
      }
    }

  }
  #endregion nested classes
}
