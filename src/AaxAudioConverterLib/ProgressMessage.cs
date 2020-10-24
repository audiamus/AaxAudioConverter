namespace audiamus.aaxconv.lib {
  public enum EProgressPhase { none, silence, adjust, transcoding, copying }

  public class ProgressInfo {
    public class ProgressEntry<T> {
      public T Content { get; private set; }
      public bool Cancel { get; private set; }

      public ProgressEntry (T content, bool cancel = false) {
        Content = content;
        Cancel = cancel;
      }
    }

    public ProgressEntry<string> Title { get; private set; }
    public EProgressPhase Phase { get; private set; }
    public uint? PartNumber { get; private set; }
    public uint? NumberOfChapters { get; private set; }
    public uint? NumberOfTracks { get; private set; }
    public ProgressEntry<uint> Chapter { get; private set; }
    public ProgressEntry<uint> Part { get; private set; }
    public ProgressEntry<uint> Track { get; private set; }

    private ProgressInfo (string title, bool cancel = false) {
      Title = new ProgressEntry<string> (title, cancel);
    }

    public static ProgressInfo ProgressInfoBook (string title, EProgressPhase phase) {
      var info = new ProgressInfo (title) { 
        Phase = phase
      };
      return info;
    }

    public static ProgressInfo ProgressInfoBookCancel (string title) {
      var info = new ProgressInfo (title, true);
      return info;
    }

    public static ProgressInfo ProgressInfoChapter (string title, uint numChapter) {
      var info = new ProgressInfo (title) {
        Chapter = new ProgressEntry<uint> (numChapter)
      };
      return info;
    }

    public static ProgressInfo ProgressInfoChapterCancel (string title, uint numChapter) {
      var info = new ProgressInfo (title) {
        Chapter = new ProgressEntry<uint> (numChapter, true)
      };
      return info;
    }


    public static ProgressInfo ProgressInfoTrack (string title, uint numTrack) {
      var info = new ProgressInfo (title) {
        Track = new ProgressEntry<uint> (numTrack)
      };
      return info;
    }

    public static ProgressInfo ProgressInfoTrackCancel (string title, uint numTrack) {
      var info = new ProgressInfo (title) {
        Track = new ProgressEntry<uint> (numTrack, true)
      };
      return info;
    }

    public static ProgressInfo ProgressInfoPart (string title, uint numPart) {
      var info = new ProgressInfo (title) {
        Part = new ProgressEntry<uint> (numPart)
      };
      return info;
    }

    public static ProgressInfo ProgressInfoPartCancel (string title, uint numPart) {
      var info = new ProgressInfo (title) {
        Part = new ProgressEntry<uint> (numPart, true)
      };
      return info;
    }

    public static ProgressInfo ProgressInfoBookCounters (string title, uint? numChapters, uint? numTracks = null, uint? partNum = null) {
      var info = new ProgressInfo (title) {
        PartNumber = partNum,
        NumberOfChapters = numChapters,
        NumberOfTracks = numTracks
      };
      return info;
    }


  }

  public class ProgressMessage {
    public bool Reset;
    public bool ResetSteps;
    public uint? AddTotalWeightedPhases;
    public uint? IncWeightedPhases;
    public int? AddTotalSteps; // Note: can be negative
    public uint? IncSteps;
    public uint? IncStepsPerMille;
    public ProgressInfo Info;
  }

}
