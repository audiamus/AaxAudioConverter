using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using audiamus.aux;
using audiamus.aux.ex;
using static audiamus.aux.ApplEnv;
using static audiamus.aux.Logging;
using Encoding = audiamus.aux.Encoding;

namespace audiamus.aaxconv.lib {
  public class AaxAudioConverter : IPreviewTitle, IDisposable {
    class DegreeOfParallelism {

      private readonly ISettings _settings;
      private ISettings Settings => _settings;

      public int Book {
        get
        {
          if (Settings.ConvMode != EConvMode.single)
            return 1;
          return Math.Min (4, Environment.ProcessorCount);
        }
      }

      public int Part {
        get
        {
          if (Settings.ConvMode != EConvMode.single)
            return 1;
          return Math.Max (Environment.ProcessorCount / Book, 1);
        }
      }

      public int Chapter { get; private set; }
      public int Track { get; private set; }

      public DegreeOfParallelism (ISettings settings) {
        _settings = settings;
      }

      public void SetChapterTrack (ChapteredTracks.PartChapters partChapters) {

        switch (Settings.ConvMode) {
          case EConvMode.single:
            Chapter = 1;
            Track = 1;
            break;

          case EConvMode.chapters:
            Chapter = Environment.ProcessorCount;
            Track = 1;
            break;

          case EConvMode.splitChapters: 
            {
              // for very short chapters we can raise parallelism
              var countedTracks = partChapters.Chapters.Select (c => c.Tracks.Count);
              double avg = countedTracks.Average ();
              int min = countedTracks.Min ();
              int trackParallelism = 1;
              if (avg > 2)
                trackParallelism = 2;
              if (avg > 4 && min > 2)
                trackParallelism = 4;

              Track = Math.Min (trackParallelism, Environment.ProcessorCount);

              var part = partChapters.Part;
              bool copy = Settings.ConvFormat == EConvFormat.mp4 && !part.IsMp3Stream ||
                          Settings.ConvFormat == EConvFormat.mp3 && part.IsMp3Stream;
              FFmpeg.ETranscode modifiers = copy ? FFmpeg.ETranscode.copy : FFmpeg.ETranscode.normal;

              if (copy) {
                Track = Math.Min (2, Environment.ProcessorCount);
                Chapter = Math.Max (Environment.ProcessorCount / Track, 1);
              } else {
                Track = Math.Min (trackParallelism, Environment.ProcessorCount);
                Chapter = Math.Max (Environment.ProcessorCount / Track, 1);
              }
              break;
            }

          case EConvMode.splitTime:
            Chapter = 1;
            Track = Environment.ProcessorCount;
            break;
        }
      }
    }

    class MultiBookInitDirectoryHandling {
      public bool? HasBeenAnswered { get; set; }
      public bool? Answer { get; set; }
    }

    #region Private Fields

    const string M4A = TagAndFileNamingHelper.EXT_M4A;
    const string M4B = TagAndFileNamingHelper.EXT_M4B;
    const string MP3 = TagAndFileNamingHelper.EXT_MP3;

    private readonly object _lockable = new object ();

    private readonly Regex _rgxPart; // = new Regex (@"^(.*)\s+(\w+)\s+(\d+)$", RegexOptions.Compiled);
    private static readonly Regex _rgxChapter = new Regex (@"^(\w+)\s+", RegexOptions.Compiled);
    
    private static readonly Regex _rgxNonWord = new Regex (@"\W", RegexOptions.Compiled);
    private readonly ActivationCode _activationCode;
    private readonly ISettings _settings;
    private readonly IResources _resources;

    public static readonly Version FFMPEG_VERSION = new Version (4, 1);
    private readonly DegreeOfParallelism _degreeOfParallelismLimit;

    #endregion Private Fields
    #region Public Properties

    public IEnumerable<uint> NumericActivationCodes => _activationCode.NumericCodes;
    public bool HasActivationCode => _activationCode.HasActivationCode;
    public ISettings Settings => _settings;
    public string AppContentDirectory => getAppContentDirectory ();
    public string AutoLaunchAudioFile { get; private set; }

    #endregion Public Properties
    #region Private Properties

    private CallbackWrapper Callbacks { get; set; }

    private ParallelOptions DefaultParallelOptions { get; set; }
    private DegreeOfParallelism DegreeOfParallelismLimit => _degreeOfParallelismLimit;
    private int MaxDegreeOfParallelism => Settings.NonParallel ? 1 : Environment.ProcessorCount;
    private IResources Resources => _resources;

    private string MP4 => Settings.M4B ? M4B : M4A;

    private MultiBookInitDirectoryHandling MultiBookInitDirectoryQuestion { get; set; }

    // support long path names
    private static string TempDirectoryLong { get; } = TempDirectory.AsUnc();

    #endregion Private Properties
    #region Public Constructors

    public AaxAudioConverter (ISettings settings, IResources resources) {
      _settings = settings;
      _resources = resources;

      _degreeOfParallelismLimit = new DegreeOfParallelism (settings);

      _rgxPart = new Regex (regexPartPattern (), RegexOptions.Compiled | RegexOptions.IgnoreCase);

      _activationCode = new ActivationCode (Settings);

      initTempDirectory ();

      FFmpeg.GetFFmpegDir = getFFmpegPath;
    }
    #endregion Public Constructors

    #region Public Methods

    public void Dispose () {
      cleanTempDirectory ();
    }

    public bool ReinitActivationCode() => _activationCode.ReinitActivationCode();


    public async Task<IEnumerable<AaxFileItem>> AddFilesAsync (IEnumerable<string> filenames) {
      DefaultParallelOptions = new ParallelOptions {
        MaxDegreeOfParallelism = MaxDegreeOfParallelism
      };

      var result = await Task.Run (() => addFilesParallel (filenames));

      DefaultParallelOptions = null;
      return result;
    }

    public async Task ConvertAsync (
      IEnumerable<AaxFileItem> fileitems,
      CancellationToken cancellationToken,
      IProgress<ProgressMessage> progress,
      IInteractionCallback<InteractionMessage, bool?> callback) {
      DefaultParallelOptions = new ParallelOptions {
        CancellationToken = cancellationToken,
        MaxDegreeOfParallelism = MaxDegreeOfParallelism
      };

      Callbacks = new CallbackWrapper (cancellationToken, progress, callback);

      await Task.Run (() => convert (fileitems), cancellationToken);

      Callbacks = null;
      DefaultParallelOptions = null;
    }

    public Version VerifyFFmpegPath () {
      var ffmpeg = new FFmpeg (null);
      Log (2, this, () => $"dir=\"{getFFmpegPath ().SubstitUser ()}\"");
      bool succ = ffmpeg.VerifyFFmpegPath ();
      if (succ) {
        Log (2, this, () => $"version {ffmpeg.Version}");
        return ffmpeg.Version;
      } else {
        Log (2, this, () => "fail");
        return null;
      }
    }

    public bool VerifyFFmpegPathVersion (Func<InteractionMessage, bool?> callback) {
      var version = VerifyFFmpegPath ();
      bool succ = version != null;

      if (succ && version < FFMPEG_VERSION) {
        var sb = new StringBuilder ();
        sb.AppendLine ($"{ApplName} {Resources.MsgFFmpegVersion1} {FFMPEG_VERSION}.");
        sb.AppendLine ($"{Resources.MsgFFmpegVersion2} {version}.");
        sb.AppendLine ($"{Resources.MsgFFmpegVersion3}?");
        sb.Append ($"({Resources.MsgFFmpegVersion4}.)");

        succ = callback (new InteractionMessage { Message = sb.ToString (), Type = ECallbackType.question }) ?? false;
      }

      return succ;
    }


    public (string tag, string file) TitlePreview (AaxFileItem fileItem) {
      var (match, sortingTitle, partNameStub, part) = parseTitleForPart (fileItem.BookTitle);
      Book book = null;
      if (match) {
        book = new Book (sortingTitle);
        book.AddPart (fileItem, part);
      } else
        book = new Book (fileItem);
      book.InitAuthorTitle (Settings);
      return (book.TagCaption.Title, book.FileCaption.Title);
    }

    public string PruneTitle (string title) => Book.PruneTitle (title);

    public string GetGenre (AaxFileItem fileItem) => TagAndFileNamingHelper.GetGenre (Settings, fileItem);

    #endregion Public Methods
    #region Private Methods

    private string getFFmpegPath () => Settings.FFMpegDirectory ?? ApplDirectory;


    private string regexPartPattern () {
      // @"^(.*)\s+(\w+)\s+(\d+)$";

      var partNames = new List<string> {
        TagAndFileNamingHelper.PART
      };
      string pn = Settings.PartName;
      if (!string.IsNullOrEmpty (pn))
        partNames.Add (pn);
      partNames.AddRange (Settings.PartNames.SplitTrim ());
      partNames.AddRange (Resources.PartNames.SplitTrim ());

      partNames = partNames.Select (n => n.ToLowerInvariant ()).Distinct ().ToList ();

      var sb = new StringBuilder ();
      sb.Append (@"^(.*)\s+((");
      int i = 0;
      foreach (string name in partNames) {
        Match match = _rgxNonWord.Match (name);
        if (match.Success)
          continue;
        if (i > 0)
          sb.Append ('|');
        sb.Append (name);
        i++;
      }

      sb.Append (@")\s+(\d+))");

      return sb.ToString ();
    }

    private ParallelOptions parallelOptions (int maxDegreeOfParallelism = -1) {
      if (Settings.NonParallel)
        maxDegreeOfParallelism = 1;
      ParallelOptions paropt;
      if (Callbacks is null)
        paropt = new ParallelOptions {
          MaxDegreeOfParallelism = maxDegreeOfParallelism
        };
      else
        paropt = new ParallelOptions {
          CancellationToken = Callbacks.CancellationToken,
          MaxDegreeOfParallelism = maxDegreeOfParallelism
        };
      return paropt;
    }

    private IEnumerable<AaxFileItem> addFilesParallel (IEnumerable<string> filenames) {
      var items = new ConcurrentBag<AaxFileItem> ();

      try {
        Parallel.ForEach (filenames, DefaultParallelOptions, path =>
        {
          Log (3, this, () => $"\"{ path.SubstitUser ()}\"");
          path = path.AsUncIfLong ();
          bool succ = File.Exists (path);
          if (!succ) {
            Log (3, this, $"fail: \"{path.SubstitUser ()}\"");
            return;
          }

          string ext = Path.GetExtension (path).ToLowerInvariant ();
          succ = ext == ".aax" || ext == ".aa";
          if (!succ)
            return;

          var item = new AaxFileItem (path);
          items.Add (item);
        });
      } catch (OperationCanceledException) { }

      if (items.Count > 0)
        Settings.InputDirectory = Path.GetDirectoryName (items.Last ().FileName).StripUnc();
      return items;
    }

    private void checkActivationAndGetChapters (Book book) {
      foreach (var part in book.Parts)
        checkActivationAndGetChapters (part);
    }

    private void checkActivationAndGetChapters (Book.BookPart part) {
      lock (_lockable) {
        while (true) {
          bool succ = false;
          foreach (string code in _activationCode.ActivationCodes) {
            succ = verifyActivationAndGetChapters (part, code);
            if (succ)
              return;
          }

          if (!activationErrorCallback (part))
            return;
          _activationCode.ReinitActivationCode ();
        }
      }
    }
       
    private bool verifyActivationAndGetChapters (Book.BookPart part, string activationcode) {
      bool withChapters = true;
      FFmpeg ffmpeg = new FFmpeg (part.AaxFileItem.FileName);
      bool succ = ffmpeg.VerifyActivation (activationcode, withChapters);
      if (ffmpeg.HasNoActivation)
        succ = ffmpeg.VerifyActivation (null, withChapters);
      else
        part.ActivationCode = activationcode;
      part.IsMp3Stream = ffmpeg.IsMp3Stream;
      part.Chapters = ffmpeg.Chapters;

      Log (3, this, () => chapterList (part, "aax"));

      getContentMetadata (part);

      return succ;
    }

    private string chapterList (Book.BookPart part, string source) {
      var sb = new StringBuilder ($"{source} chapters:");
      sb.AppendLine ();
      if (!(part.Chapters is null)) {
        foreach (var ch in part.Chapters)
          sb.AppendLine ($"  {ch}");
      }
      sb.Append ($"  duration={part.AaxFileItem.Duration.ToStringHMSm()}");
      return sb.ToString ();
    }

    private void convert (IEnumerable<AaxFileItem> fileitems) {
      Log (2, this, () => $"#files={fileitems.Count ()}{(Level <= 2 ? $", format={Settings.ConvFormat}, mode={Settings.ConvMode}" : string.Empty)}");
      Log (3, this, () => Settings.ToString ());

      // make sure all progress is reset;
      Callbacks.Progress (new ProgressMessage { Reset = true });

      var books = new List<Book> ();
      Book book = null;
      foreach (var fi in fileitems) {
        // make sure we start from scratch
        fi.Converted = false;

        if (fi.BookTitle is null)
          continue;

        // part of a multipart book?
        var mpb = parseTitleForPart (fi.BookTitle);
        if (mpb.match) {
          //yes, get non-partial title and part# 
          string sortingtitle = mpb.sortingTitle;
          int part = mpb.part;

          // no previous book or different title?
          if (book is null || sortingtitle != book.SortingTitle) {
            // previous book now complete, can process it
            if (book != null)
              books.Add (book);
            // new book, save part name stub
            book = new Book (sortingtitle) { PartNameStub = mpb.partNameStub };
          }
          // same book
          book.AddPart (fi, part);

        } else {
          // not a multipart book
          // any previous book now complete, can process it
          if (book != null) {
            books.Add (book);
            book = null;
          }

          book = new Book (fi);
        }
      }
      if (book != null)
        books.Add (book);

      convertBooksParallel (books);

      setAutoLaunchAudioFile (books);
    }

    private void setAutoLaunchAudioFile (List<Book> books) {
      AutoLaunchAudioFile = null;

      if (!Settings.AutoLaunchPlayer || books.Count == 0)
        return;

      AutoLaunchAudioFile = books.First ().DefaultAudioFile;
    }

    private (bool match, string sortingTitle, string partNameStub, int part) parseTitleForPart (string bookTitle) {
      Match match = _rgxPart.Match (bookTitle);
      if (!match.Success)
        return (false, null, null, 0);

      string partNameStub = match.Groups[3].Value;
      int.TryParse (match.Groups[4].Value, out int part);

      //yes, get non-partial title by removing "part #"
      string sortingTitle = bookTitle.Remove (match.Groups[2].Index, match.Groups[2].Length);
      sortingTitle = sortingTitle
        .Replace ("  ", " ")
        .Replace (" :", ":")
        .Trim();

      return (true, sortingTitle, partNameStub, part);
    }

    private void convertBooksParallel (IEnumerable<Book> books) {
      Log (2, this, () => $"#books={books.Count()}, #parts={books.SelectMany(b => b.Parts).Count()}");

      MultiBookInitDirectoryQuestion = books.Count () > 1 ? new MultiBookInitDirectoryHandling () : null;

      // init progress
      int numInitialTracks = books.Select (b => b.Parts.Count).Sum ();
      uint numProcessingParts = (uint)numInitialTracks;
      if (Settings.ConvMode == EConvMode.splitChapters)
        numProcessingParts *= 2;
      Callbacks.Progress (new ProgressMessage {
        AddTotalParts = numProcessingParts,
        AddTotalTracks = numInitialTracks // number of tracks, initially just as many parts
      });

      if (Settings.AaxCopyMode != default && Directory.Exists(Settings.AaxCopyDirectory))
        Callbacks.Progress (new ProgressMessage {
          AddTotalTracks = numInitialTracks // one more for each part
        });


      // parallel only in "single" mode
      try {
        Parallel.ForEach (books, parallelOptions (DegreeOfParallelismLimit.Book), b => convertBook (b));
      } catch (OperationCanceledException) { }

      cleanTempDirectory ();
    }

    private void convertBook (Book book) {
      Log (2, this, () => $"\"{book.SortingTitle.Shorten()}\", #parts={book.Parts.Count()}");

      // Check activation codes
      //   and generally decodable format 
      // Also retrieve chapters on the fly
      checkActivationAndGetChapters (book);

      book.InitAuthorTitle (Settings);

      preProcessChapters (book);

      switch (Settings.ConvMode) {
        case EConvMode.single:
          convertSingleMode (book);
          break;

        case EConvMode.chapters:
          convertChapterMode (book);
          break;

        case EConvMode.splitChapters:
          convertSplitChapterMode (book);
          break;

        case EConvMode.splitTime:
          convertSplitTimeMode (book);
          break;
      }

      if (!(book.OutDirectoryLong is null)) {
        // On cancel remove book output, otherwise make playlist
        if (Callbacks?.Cancelled ?? false)
          deleteOutDirectory (book);
        else {
          makePlaylist (book);
          extraMetaFiles (book);
          copyAaxFiles (book);
        }
      }

      if (Settings.ConvMode != EConvMode.single)
        cleanTempDirectory ();
    }

    private void extraMetaFiles (Book book) {
      if (Settings.ExtraMetaFiles) {
        var emf = new ExtraMetaFiles (book, Settings, Resources);
        emf.WriteFiles (flatDir);
      }
    }

    private void copyAaxFiles (Book book) {
      if (Settings.AaxCopyMode == default)
        return;

      if (!Directory.Exists (Settings.AaxCopyDirectory)) {
        Log (2, this, () => $"Dest dir does not exist: \"{Settings.AaxCopyDirectory.SubstitUser ()}\", copy skipped.");
        return;
      }

      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, EProgressPhase.copying) :
            ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
        }))) {
        var afc = new AaxFileCopier (book, Settings, Resources, Callbacks);
        afc.Copy ();
      }
    }


    private void preProcessChapters (Book book) {

      foreach (var part in book.Parts) {
        if (part.Chapters is null)
          part.Chapters = new List<Chapter> ();
        if (part.Chapters.Count == 0)
          part.Chapters.Add (new Chapter (part.AaxFileItem.Duration));
      }

      if (book.ChapterNameStub is null) {
        var match = _rgxChapter.Match (book.Parts?[0].Chapters?[0].Name ?? string.Empty);
        if (match.Success)
          book.ChapterNameStub = match.Groups[1].Value;
      }

      var shortChapter = TimeSpan.FromSeconds (Settings.ShortChapterSec);
      var veryShortChapter = TimeSpan.FromSeconds (Settings.VeryShortChapterSec);

      for (int i = 0; i < book.Parts.Count; i++) {
        var part = book.Parts[i];

        preprocessEmbeddedChapters (book, part, i, shortChapter, veryShortChapter);
        if (part.HasNamedChapters)
          preprocessNamedChapters (book, part);

        if (Settings.ConvMode == EConvMode.single)
          prepareChaptersSingleMode (part);

        // done with one part, now knowing the number of chapters.
        // adding chapters to track progress, minus one for the part itself which has already been accounted for.
        // Also set counters.
        var cnt = book.Counts (Settings.ConvMode, part);
        Callbacks.Progress (new ProgressMessage {
          AddTotalTracks = (Settings.ConvMode != EConvMode.single) ? part.Chapters.Count - 1 : (int?)null,
          Info = ProgressInfo.ProgressInfoBookCounters (cnt.title, cnt.chapters, cnt.tracks, cnt.part)
        });
      }
    }

    private void prepareChaptersSingleMode (Book.BookPart part) {
      Log0 (3, this);
      var chapters = new List<Chapter> ();
      var chapter = new Chapter (part.AaxFileItem.Duration);
      chapters.Add (chapter);
      part.SwapChapters ();
      part.Chapters = chapters;

      if (part.Chapters2 is null || part.Chapters2.Count == 0)
        return;

      var begin = part.Chapters2.First().Time.Begin;
      var end = part.Chapters2.Last().Time.End;
      chapter.Time.Begin = begin;
      chapter.Time.End = end;
      Log (3, this, () => chapterList (part, "single"));

      foreach (var ch in part.Chapters2) {
        ch.Time.Begin -= begin;
        ch.Time.End -= begin;
      }

      var ffmetadata = new FFMetaData (part.Chapters2);
      string filename = ffmetafilename (part);
      bool succ = ffmetadata.Write (filename);
      Log (3, this, () => $"ffmetafile=\"{filename.SubstitUser()}\", #chapters={ffmetadata.Chapters.Count}, succ={succ}");
    }

    private string ffmetafilename (Book.BookPart part) =>
      Path.Combine (TempDirectoryLong, $"{part.AaxFileItem.Author} - {part.AaxFileItem.BookTitle}_meta.txt".Prune());


    private void preprocessEmbeddedChapters (Book book, Book.BookPart part, int i, TimeSpan shortChapter, TimeSpan veryShortChapter) {
      if (veryShortChapter >= TimeSpan.Zero && (!part.HasNamedChapters || (book.PartsType == Book.EParts.all && i > 0))) {
        var chapter = part.Chapters.LastOrDefault ();
        if (chapter?.Time.Duration <= veryShortChapter) {
          Log (3, this, () => $"Remove last very short ({veryShortChapter.TotalSeconds}s): {chapter}");
          part.Chapters.Remove (chapter);
        }
        chapter = part.Chapters.FirstOrDefault ();
        if (chapter?.Time.Duration <= veryShortChapter) {
          Log (3, this, () => $"Remove first very short ({veryShortChapter.TotalSeconds}s): {chapter}");
          part.Chapters.Remove (chapter);
        }
      }

      if (book.PartsType == Book.EParts.all && shortChapter >= TimeSpan.Zero) {
        // check for short last if i < Count - 1
        bool lastInMiddlePart = i < book.Parts.Count - 1 && part.Chapters.Count > 0;
        if (lastInMiddlePart) {
          var chapter = part.Chapters.LastOrDefault ();
          if (chapter?.Time.Duration <= shortChapter) {
            Log (3, this, () => $"Remove last in middle part ({shortChapter.TotalSeconds}s): {chapter}");
            part.Chapters.Remove (chapter);
          }
        }

        // check for short first if i > 0
        bool firstInMiddlePart = i > 0 && part.Chapters.Count > 0;
        if (firstInMiddlePart) {
          var chapter = part.Chapters.FirstOrDefault ();
          if (chapter?.Time.Duration <= shortChapter) {
            Log (3, this, () => $"Remove first in middle part ({shortChapter.TotalSeconds}s): {chapter}");
            part.Chapters.Remove (chapter);
          }
        }
      }
    }

    private void preprocessNamedChapters (Book book, Book.BookPart part) {
      if (Settings.ShortChapterSec > 0) {
        // we will have the true start and end points now.
        TimeSpan tBeg = part.Chapters.FirstOrDefault ()?.Time.Begin ?? TimeSpan.Zero;
        TimeSpan tEnd = part.Chapters.LastOrDefault ()?.Time.End ?? part.Duration;
        adjustNamedChapters (part, tBeg, tEnd);
      }

      if (Settings.VeryShortChapterSec > 0) {
        TimeSpan tBeg = part.BrandIntro;
        TimeSpan tEnd = part.AaxFileItem.Duration - part.BrandOutro;
        adjustNamedChapters (part, tBeg, tEnd);
      } else {
        TimeSpan tBeg = TimeSpan.Zero;
        TimeSpan tEnd = part.AaxFileItem.Duration;
        adjustNamedChapters (part, tBeg, tEnd, book);
      }

      // finally
      part.SwapChapters ();
    }

    private void adjustNamedChapters (Book.BookPart part, TimeSpan tBeg, TimeSpan tEnd, Book book = null) {
      if (!(book is null)) {
        var lastChapter = part.Chapters2.LastOrDefault ();
        if (lastChapter is null)
          return;

        if (book.Parts.LastOrDefault () == part && tEnd > lastChapter.Time.End) {
          lastChapter.Time.End = tEnd;
          return;
        }
      }

      var toBeRemoved = new List<int> ();
      for (int i = part.Chapters2.Count - 1; i >= 0; i--) {
        var ch = part.Chapters2[i];
        if (ch.Time.Begin >= tEnd) {
          Log (3, this, () => $"Remove last: {ch}");
          toBeRemoved.Add (i);
        } else {
          if (ch.Time.End > tEnd) {
            Log (3, this, () => $"Alter last : {ch}, -> {tEnd.ToStringHMSm()}");
            ch.Time.End = tEnd;
          } else
            break;
        }
      }

      for (int i = 0; i < part.Chapters2.Count; i++) {
        var ch = part.Chapters2[i];
        if (ch.Time.End <= tBeg) {
          Log (3, this, () => $"Remove first: {ch}");
          toBeRemoved.Add (i);
        } else {
          if (ch.Time.Begin < tBeg) {
            Log (3, this, () => $"Alter first: {ch}, {tBeg.ToStringHMSm()} ->");
            ch.Time.Begin = tBeg;
          } else
            break;
        }
      }

      foreach (int idx in toBeRemoved)
        part.Chapters2.RemoveAt (idx);
    }


    private void convertSingleMode (Book book) {
      // Mode single
      //   Define author and book title folder
      //   Transcode per part, one track per part.
      Log (2, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      bool succ = prepareTrackFilesSingleMode (book);
      if (!succ)
        return;

      transcodeTracks (book);

    }

    private void convertChapterMode (Book book) {
      // Mode chapters
      //   Define author and book title folder
      //   Transcode per part and per chapter, one track per chapter
      Log (2, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      bool succ = prepareTrackFilesChapterMode (book);
      if (!succ)
        return;

      transcodeTracks (book);
    }

    private void convertSplitChapterMode (Book book) {
      // Mode chapters with timed tracks
      //   Define author and book title folder
      //   Split into chapters, detect silence per chapter
      //   Create cue sheet per chapter
      //   Transcode per part, per chapter and per cue sheet item, group by chapter subfolder, several tracks per chapter 
      Log (2, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      bool succ = prepareTrackFilesSplitChapterMode (book);
      if (!succ)
        return;

      transcodeTracks (book);

    }

    private void convertSplitTimeMode (Book book) {
      // Mode timed tracks without chapters
      //   Define author and book title folder
      //   Split into chapters, detect silence per chapter
      //   Create cue sheet per chapter
      //   Combine all chapter cue sheets into single chapter
      //   Transcode per part, per chapter and per cue sheet item, no subfolders, tracks per book 

      Log (2, this, () => $"\"{book.SortingTitle.Shorten()}\"");
      bool succ = prepareTrackFilesSplitTimeMode (book);
      if (!succ)
        return;

      transcodeTracks (book);

    }

    private bool prepareTrackFilesSingleMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      string outDir = initDirectory (book);

      if (outDir is null)
        return false;

      foreach (var part in book.Parts) {

        if (Callbacks.Cancelled)
          return false;

        var track = new Track (part.AaxFileItem.Duration);
        part.Tracks.Add (track);
      }

      TagAndFileNamingHelper.SetFileNames (Resources, Settings, book);
      return true;
    }

    private bool prepareTrackFilesChapterMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");
      string outDir = initDirectory (book);
      if (outDir is null)
        return false;

      nameTrackFilesChapterMode (book);
      return true;
    }

    private bool prepareTrackFilesSplitChapterMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");
      string outDir = initDirectory (book);
      if (outDir is null)
        return false;

      bool succ = silenceAndCueSheet (book, null);
      if (!succ)
        return false;

      nameTrackFilesSplitChapterMode (book);
      return true;
    }

    private bool prepareTrackFilesSplitTimeMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");
      string outDir = initDirectory (book);
      if (outDir is null)
        return false;

      bool succ = silenceAndCueSheet (book, mergeSilences);
      if (!succ)
        return false;
      
      nameTrackFilesSplitChapterMode (book);
      return true;
    }

    private void nameTrackFilesChapterMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      foreach (var part in book.Parts) {

        foreach (var chapter in part.Chapters) {

          var t = chapter.Time;
          var track = new Track (t.Begin, t.End) { Chapter = chapter };
          part.Tracks.Add (track);

        }
      }

      TagAndFileNamingHelper.SetFileNames (Resources, Settings, book);
    }


    private bool silenceAndCueSheet (Book book, Action<Book> callback) {

      // silence detection will first split into chapters and then detect silence in each chapter.
      // hence add twice the total number of chapters to track progress.
      int totalNumChapters = book.Parts.Select (p => p.Chapters.Count).Sum ();
      Callbacks.Progress (new ProgressMessage {
        AddTotalTracks = totalNumChapters * 2
      });

      bool succ = detectSilence (book);
      if (!succ) {
       Log (2, this, () => $"{nameof(detectSilence)} \"{book.SortingTitle.Shorten()}\", succ={succ}");
       return false;
      }
      Log (3, this, () => 
        $"{nameof (detectSilence)} for \"{book.SortingTitle.Shorten()}\" found {book.Parts.SelectMany (p => p.Chapters.Select(c => c.Silences.Count)).Sum ()} positions total");

      callback?.Invoke (book);

      createCueSheet (book);

      // we now have all the tracks. Add them minus the number of chapters which have alredy been accounted for
      // this can be negative in split time mode with long tracks
      int totalNumTracks = book.Parts.Select (p => p.Tracks.Count).Sum ();
      if (totalNumTracks != totalNumChapters)
        Callbacks.Progress (new ProgressMessage {
          AddTotalTracks = totalNumTracks - totalNumChapters
        });

      Log (3, this, () => $"{nameof (createCueSheet)} for \"{book.SortingTitle.Shorten()}\" determined {totalNumTracks} tracks");

      return true;
    }


    private bool detectSilence (Book book) {

      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, EProgressPhase.silence) :
            ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
        }))) {

        string filestub = book.AuthorFile + " - " + book.TitleFile;

        // always serial
        foreach (var part in book.Parts) {
          Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}");
          
          // Chapter-wise split to temp folder, for shorter files,
          // because of limited precison (6 digit, "g6") of FFmpeg silence detection filter output.
          // We aim for 10ms resolution. (May still not work this way with very long chapters.)
          detectSilenceChapterParallel (book, part, filestub);
          bool fail = part.Chapters.Where (c => c.Silences is null).Any ();
          if (fail) {
            Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}, succ={!fail}");
            return false;
          }
          Callbacks.Progress (new ProgressMessage { IncParts = 1 });
        }
        return true;
      }
    }

    private void detectSilenceChapterParallel (Book book, Book.BookPart part, string filestub) {
      string ext = part.IsMp3Stream ? MP3 : MP4;

      try {
        Parallel.For (0, part.Chapters.Count, DefaultParallelOptions, i =>
        {

          if (Callbacks.Cancelled)
            return;

          var chapter = part.Chapters[i];

          uint ch = book.ChapterNumber (chapter);

          using (new ResourceGuard (f => Callbacks.Progress (
            new ProgressMessage {
              Info = f ?
              ProgressInfo.ProgressInfoChapter (book.TitleTag, ch) :
              ProgressInfo.ProgressInfoChapterCancel (book.TitleTag, ch)
            }))) {

            string filename = $"{filestub} - {part.PartNumber}.{i}{ext}";
            chapter.TmpFileName = Path.Combine (TempDirectoryLong, filename);

            // Extract chapter to temp file
            using (var threadProg = new ThreadProgress (Callbacks.Progress)) {

              var t = chapter.Time;
              FFmpeg ffmpeg1 = new FFmpeg (part.AaxFileItem.FileName) {
                Cancel = Callbacks.Cancel,
                Progress = threadProg.Report
              };
              Log (3, this, () => $"{nameof (ffmpeg1.Transcode)} in=\"{part.AaxFileItem.FileName.SubstitUser()}\", out=\"{chapter.TmpFileName.SubstitUser()}\"");
              bool succ = ffmpeg1.Transcode (chapter.TmpFileName,
                FFmpeg.ETranscode.copy | FFmpeg.ETranscode.noChapters,
                part.ActivationCode, t.Begin, t.End);
              if (!succ) {
                Log (3, this, () => $"{nameof (ffmpeg1.Transcode)} \"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}, chapter=\"{chapter.Name}\", succ={succ}");
                return;
              }
            }

            if (Callbacks.Cancelled)
              return;

            // Now, find silences in temp. chapter file
            using (var threadProg = new ThreadProgress (Callbacks.Progress)) {
              FFmpeg ffmpeg2 = new FFmpeg (chapter.TmpFileName) {
                Cancel = Callbacks.Cancel,
                Progress = threadProg.Report
              };
              bool succ = ffmpeg2.DetectSilence (part.ActivationCode);
              if (!succ) {
                Log (3, this, () => $"{nameof(ffmpeg2.DetectSilence)} \"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}, chapter=\"{chapter.Name}\", succ={succ}");
                return;
              }
              chapter.Silences = ffmpeg2.Silences;
              Log (3, this, () => $"{nameof (ffmpeg2.DetectSilence)} in \"{chapter.TmpFileName.SubstitUser()}\" found {chapter.Silences.Count} positions");
            }
          }
        });
      } catch (OperationCanceledException) { }
    }

    private void createCueSheet (Book book) {
      foreach (var part in book.Parts) {

        // do not trust the settings range check. Enforce track length between 3 and 15 min. 
        int trackDurMins = Math.Max (3, Math.Min (15, (int)Settings.TrkDurMins));

        // expects all chapters in individual files, each starting at time 0

        // tracks per chapter
        foreach (var chapter in part.Chapters) {

          // chapter duration in sec
          double chLenSec = chapter.Time.Duration.TotalSeconds;

          // rounded number of tracks in chapter, observing desired track duration 
          int numTracks = Math.Max ((int)(chLenSec / (Settings.TrkDurMins * 60) + 0.5), 1);

          // average track length in sec
          double aveTrackLen = chLenSec / numTracks;

          // average track length as TimeSpan
          var tsAveTrackLen = TimeSpan.FromSeconds (aveTrackLen);

          // search range for silence at desired end of track is +/- 1/3 ave track length
          var tsSearchRange = TimeSpan.FromSeconds (aveTrackLen / 3);

          // start 1st track at zero 
          // unless in time split mode where we start at the actual beginning of the chapter
          var tsStart = TimeSpan.Zero;
          if (Settings.ConvMode == EConvMode.splitTime)
            tsStart = chapter.Time.Begin;

          // desired end of 1st track
          var tsEnd = tsStart + tsAveTrackLen;

          // max end is chapter duration unless in time split mode
          var tsChEnd = chapter.Time.Duration;
          if (Settings.ConvMode == EConvMode.splitTime)
            tsChEnd = chapter.Time.End;

          // while chapter length has not been reached, will be checked in loop
          while (true) {

            if (tsEnd < tsChEnd) {
              // upper search limit for silence
              var tsUpper = tsEnd + tsSearchRange;
              // lower search limit for silence
              var tsLower = tsEnd - tsSearchRange;

              // take the easy road using LINQ, find nearest silence or none, above and below
              var silUp = chapter.Silences.Where (t => t.Begin >= tsEnd && t.Begin < tsUpper).FirstOrDefault ();
              var silDn = chapter.Silences.Where (t => t.End > tsLower && t.End <= tsEnd).LastOrDefault ();

              // which silence shall it be
              TimeInterval sil = null;
              if (!(silUp is null || silDn is null)) {
                // up and down found, use nearest
                var deltaUp = silUp.Begin - tsEnd;
                var deltaDn = tsEnd - silDn.End;
                if (deltaUp < deltaDn)
                  sil = silUp;
                else
                  sil = silDn;
              } else {
                // not both found, use any present
                if (!(silUp is null))
                  sil = silUp;
                else if (!(silDn is null))
                  sil = silDn;
              }

              // silence has been found
              if (!(sil is null)) {
                // actual track end
                // add half the silence interval, cut in stream will not be precise.
                var tsActualEnd = sil.Begin + TimeSpan.FromTicks( sil.Duration.Ticks / 2); 

                // check for overshooting
                if (tsActualEnd > tsChEnd)
                  tsActualEnd = tsChEnd;

                // actual difference
                var tsDelta = tsActualEnd - tsEnd;

                // create new track item
                var track = new Track (tsStart, tsActualEnd) {
                  Chapter = chapter
                };
                part.Tracks.Add (track);

                // set for next track
                tsStart = tsActualEnd;
                tsEnd = tsStart + tsAveTrackLen - tsDelta;
                if (tsEnd + tsSearchRange > tsChEnd)
                  tsEnd = tsChEnd;

              } else {
                // silence not found, extend track
                tsEnd += tsAveTrackLen;
              }
            }

            // at the end of the chapter
            if (tsEnd >= tsChEnd) {
              // last track without silence search
              var track = new Track (tsStart, tsChEnd) {
                Chapter = chapter
              };
              part.Tracks.Add (track);

              // done
              break;
            }
          }
        }
      }
    }


    private void mergeSilences (Book book) {
      foreach (var part in book.Parts) {
        if (part.Chapters?.Count == 0)
          continue;

        var chapter0 = part.Chapters[0];
        TimeSpan startTimeThisChapter = chapter0.Time.Begin;
        if (startTimeThisChapter != TimeSpan.Zero) {
          var shifted = new List<TimeInterval> ();
          foreach (var ivl in chapter0.Silences)
            shifted.Add (ivl.Shifted (startTimeThisChapter));
          chapter0.Silences = shifted;
        }

        for (int i = 1; i < part.Chapters.Count; i++) {
          var chapter = part.Chapters[i];
          if (chapter.Silences is null)
            continue;

          startTimeThisChapter = chapter.Time.Begin;

          for (int j = 0; j < chapter.Silences.Count; j++) {
            var ivl = chapter.Silences[j];

            ivl = ivl.Shifted (startTimeThisChapter);

            if (j == 0) {
              if (ivl.Begin <= chapter0.Silences.Last().End) {
                ivl = new TimeInterval (chapter0.Silences.Last ().Begin, ivl.End);
                chapter0.Silences.RemoveAt (chapter0.Silences.Count - 1);
              }
            }

            chapter0.Silences.Add (ivl);
          }
        }

        chapter0.Time.End = part.Chapters.Last ().Time.End;
        chapter0.TmpFileName = part.AaxFileItem.FileName;
        part.Chapters.Clear ();
        part.Chapters.Add (chapter0);
      }
    }


    private void nameTrackFilesSplitChapterMode (Book book) => TagAndFileNamingHelper.SetFileNames (Resources, Settings, book);

    private void transcodeTracks (Book book) {
      if (Callbacks.Cancelled)
        return;
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, EProgressPhase.transcoding) :
            ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
        }))) {

        // by chapter
        var chapterTracks = new ChapteredTracks (book);

        transcodeTracksByPartAndChapterParallel (book, chapterTracks);
      }
    }

    private void transcodeTracksByPartAndChapterParallel (Book book, ChapteredTracks chapterTracks) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\", #parts={chapterTracks.Parts.Count()}");

      // parts parallel only in "single" mode
      try {
        Parallel.ForEach (chapterTracks.Parts, parallelOptions (DegreeOfParallelismLimit.Part), part =>
        {

          // will only affect status when in "single" mode
          uint partNum = (Settings.ConvMode == EConvMode.single && book.PartsType != Book.EParts.none) ? (uint)part.Part.PartNumber : 0;
          using (new ResourceGuard (f => Callbacks.Progress (
            new ProgressMessage {
              Info = f ?
                ProgressInfo.ProgressInfoPart (book.TitleTag, partNum) :
                ProgressInfo.ProgressInfoPartCancel (book.TitleTag, partNum)
            }))) {

            // update counters. Number of tracks is known
            var cnt = book.Counts (Settings.ConvMode, part.Part);
            Callbacks.Progress (new ProgressMessage {
              Info = ProgressInfo.ProgressInfoBookCounters (cnt.title, cnt.chapters, cnt.tracks, cnt.part)
            });

            transcodeChaptersParallel (book, part);

            int nfail = part.Part.Tracks.Where (t => t.State != ETrackState.complete).Count ();
            if (nfail > 0) {
              Log (1, this, () => $"\"{book.SortingTitle.Shorten()}\", #parts={chapterTracks.Parts.Count ()}, #failed tracks={nfail}");
              return;
            }
            part.Part.AaxFileItem.Converted = true;
            Callbacks.Progress (new ProgressMessage { IncParts = 1 });
          }
        });
      } catch (OperationCanceledException) { }
    }

    private void transcodeChaptersParallel (Book book, ChapteredTracks.PartChapters part) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\", part#={part.Part.PartNumber}, #chaps={part.Chapters.Count()}");

      DegreeOfParallelismLimit.SetChapterTrack (part);

      try {
        Parallel.ForEach (part.Chapters, parallelOptions (DegreeOfParallelismLimit.Chapter), chapter =>
        {
          if (Callbacks.Cancelled)
            return;

          using (new ResourceGuard (f => {
            if (Settings.ConvMode != EConvMode.splitTime)
              Callbacks.Progress (
                new ProgressMessage {
                  Info = f ?
                    ProgressInfo.ProgressInfoChapter (book.TitleTag, chapter.ChapterNumber) :
                    ProgressInfo.ProgressInfoChapterCancel (book.TitleTag, chapter.ChapterNumber)
                });
          })) 
          {

            transcodeTracksParallel (book, part.Part, chapter);

          }

        });
      } catch (OperationCanceledException) { }
    }


    private void transcodeTracksParallel (Book book, Book.BookPart part, ChapteredTracks.PartChapter partChapter) {
      bool copy = Settings.ConvFormat == EConvFormat.mp4 && !part.IsMp3Stream ||
                  Settings.ConvFormat == EConvFormat.mp3 && part.IsMp3Stream;
      FFmpeg.ETranscode modifiers = copy ? FFmpeg.ETranscode.copy : FFmpeg.ETranscode.normal;

      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}, chap#={partChapter.ChapterNumber}, #tracks={partChapter.Tracks.Count ()}");

      string inFile = part.AaxFileItem.FileName;
      Log (3, this, () => $"in=\"{inFile.SubstitUser()}\"");
      
      try {
        Parallel.For (0, partChapter.Tracks.Count, parallelOptions (DegreeOfParallelismLimit.Track), i =>
        {
          if (Callbacks.Cancelled)
            return;

          var track = partChapter.Tracks[i];

          using (new ResourceGuard (f =>
          {
            if (Settings.ConvMode == EConvMode.splitTime)
              Callbacks.Progress (
                new ProgressMessage {
                  Info = f ?
                    ProgressInfo.ProgressInfoTrack (book.TitleTag, partChapter.TrackNumberOffset + (uint)i) :
                    ProgressInfo.ProgressInfoTrackCancel (book.TitleTag, partChapter.TrackNumberOffset + (uint)i)
                });
          })) {

            using (var threadProg = new ThreadProgress (Callbacks.Progress)) {
              bool succ = false;

              string outFile = track.FileName;

              track.State = ETrackState.current;

              if (Settings.ConvMode == EConvMode.single) 
                succ = transcodeTrackSingle (part, inFile, outFile, modifiers, threadProg);
              else 
                succ = transcodeTrackMulti (part, track, inFile, outFile, modifiers, threadProg);
              

              if (succ)
                succ = updateTags (book, track);

              track.State = succ ? ETrackState.complete : ETrackState.aborted;
            }
          }
        });
      } catch (OperationCanceledException) { }
    }

    private bool transcodeTrackMulti (Book.BookPart part, Track track, string inFile, string outFile, FFmpeg.ETranscode modifiers, ThreadProgress threadProg) {
      string dir = Path.GetDirectoryName (outFile);
      Directory.CreateDirectory (dir);
      if (Settings.ConvMode > EConvMode.chapters && !(track.Chapter is null))
        inFile = track.Chapter.TmpFileName;

      FFmpeg ffmpeg = new FFmpeg (inFile) {
        Cancel = Callbacks.Cancel,
        Progress = threadProg.Report
      };

      var t = track.Time;
      string actcode = null;
      if (Settings.ConvMode == EConvMode.chapters || Settings.ConvMode == EConvMode.splitTime)
        actcode = part.ActivationCode;
      bool succ = ffmpeg.Transcode (outFile, modifiers, actcode, t.Begin, t.End);
      Log (3, this, () => $"out=\"{outFile.SubstitUser ()}\", mod={modifiers}, ac={!string.IsNullOrEmpty (part.ActivationCode)}" +
          $", {t.Begin.ToStringHMSm ()}->{t.End.ToStringHMSm ()}, succ={succ}");
      return succ;
    }

    private bool transcodeTrackSingle (Book.BookPart part, string inFile, string outFile, FFmpeg.ETranscode modifiers, ThreadProgress threadProg) {
      if (part.Chapters is null || part.Chapters.Count == 0)
        return false;

      string metafile = Settings.ConvFormat == EConvFormat.mp4 ? ffmetafilename (part) : null;

      FFmpeg ffmpeg = new FFmpeg (inFile, metafile) {
        Cancel = Callbacks.Cancel,
        Progress = threadProg.Report
      };

      var t = part.Chapters.First().Time;
      bool succ = ffmpeg.Transcode (outFile, modifiers, part.ActivationCode, t.Begin, t.End);
      Log (3, this, () => $"out=\"{outFile.SubstitUser ()}\", mod={modifiers}, ac={!string.IsNullOrEmpty (part.ActivationCode)}, succ={succ}");
      return succ;
    }

    private void getContentMetadata (Book.BookPart part) {
      if (!Settings.NamedChaptersAndAlwaysWithNumbers.HasValue)
        return;

      var appMetadadata = new AudibleAppContentMetadata ();
      appMetadadata.GetContentMetadata (part);
    }

    private bool updateTags (Book book, Track track) {
      bool succ = TagAndFileNamingHelper.WriteMetaData (Resources, Settings, book, track);
      Log (3, this, () => $"track=\"{track.Title}\", succ={succ}"); 
      return succ;
    }

    private void makePlaylist (Book book) {
      const string EXTM3U = "#EXTM3U";

      if (book.Parts is null)
        return;

      int nTracks = book.Parts.Select (p => p.Tracks?.Count () ?? 0).Sum ();
      if (nTracks < 2) {
        if (nTracks == 1)
          book.DefaultAudioFile = book.Parts.First()?.Tracks?.First()?.FileName;
        return; // no playlist for single file
      }

      if (book.PartsType == Book.EParts.some && Settings.ConvMode == EConvMode.single)
        return;

      var encoding = Settings.Latin1EncodingForPlaylist ? Encoding.Latin1 : Encoding.UTF8;
      string filename = flatDir (book) + ".m3u";
      try {
        if (book.PartsType != Book.EParts.some) {
          string path = Path.Combine (book.OutDirectoryLong, filename);
          using (var wr = new StreamWriter (new FileStream (path, FileMode.Create, FileAccess.Write), encoding)) {
            wr.WriteLine (EXTM3U);
            foreach (var part in book.Parts)
              writePlaylistItems (wr, book, part);
            wr.WriteLine ();
          }
          book.DefaultAudioFile = path;
        } else {
          foreach (var part in book.Parts) {
            string partDir = TagAndFileNamingHelper.GetPartDirectoryName (Resources, Settings, book, part); 
            string path = Path.Combine (partDir, filename);
            using (var wr = new StreamWriter (new FileStream (path, FileMode.Create, FileAccess.Write))) {
              wr.WriteLine (EXTM3U);
              writePlaylistItems (wr, book, part);
              wr.WriteLine ();
            }
          }
        }
      } catch (Exception exc) {
        exceptionCallback (exc);
        Callbacks.Cancel ();
      }
    }

    private void writePlaylistItems (TextWriter wr, Book book, Book.BookPart part) {
      const string EXTINF = "#EXTINF";
      if (part.Tracks is null)
        return;
      string partDir = TagAndFileNamingHelper.GetPartDirectoryName (Resources, Settings, book, part);

      foreach (var track in part.Tracks) {
        bool hasDir = track.FileName.StartsWith (partDir);
        if (!hasDir)
          continue;
        string subpath = track.FileName.Substring (partDir.Length + 1);
        string title = track.Title ?? Path.GetFileNameWithoutExtension (subpath);
        int duration = (int)track.Time.Duration.TotalSeconds;

        wr.WriteLine ($"{EXTINF}:{duration}, {title}");
        wr.WriteLine (subpath);
      }
    }

    private void deleteOutDirectory (Book book) {
      try {
        DirectoryInfo dia = Directory.GetParent (book.OutDirectoryLong);
        DirectoryInfo dib = new DirectoryInfo (book.OutDirectoryLong);
        if (book.PartsType != Book.EParts.some) {
          dib.Delete (true);
        } else {
          foreach (var part in book.Parts) {
            string dir = TagAndFileNamingHelper.GetPartDirectoryName (Resources, Settings, book, part);
            if (Directory.Exists (dir)) {
              DirectoryInfo dic = new DirectoryInfo (dir);
              dic.Delete (true);
            }
          }
          int n = dib.GetDirectories ().Count ();
          if (n == 0)
            dib.Delete (true);
        }

        if (!Settings.FlatFolders && book.IsNewAuthor) {
          int n = dia.GetDirectories ().Count ();
          if (n == 0)
            dia.Delete (true);
        }
      } catch (Exception exc) {
        exceptionCallback (exc);
      }
      foreach (var part in book.Parts)
        part.AaxFileItem.Converted = false;
    }

    private void cleanTempDirectory () {
      try {
        Log (3, this, () => $"\"{TempDirectoryLong.SubstitUser()}\"");
        DirectoryInfo di = new DirectoryInfo (TempDirectoryLong);
        di.Clear ();
      } catch (Exception exc) {
        exceptionCallback (exc);
      }
    }

    private string initDirectory (Book book) {
      string outDir;
      if (Settings.FlatFolders)
        outDir = initDirectory (book, null, flatDir (book));
      else
        outDir = initDirectory (book, book.AuthorFile, book.TitleFile);
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\", outDir=\"{outDir.SubstitUser ()}\"");
      return outDir;
    }

    private string flatDir (Book book) {
      if (Settings.FlatFolders)
        switch (Settings.FlatFolderNaming) {
          default:
          case EFlatFolderNaming.author_book:
            return $"{book.AuthorFile} - {book.TitleFile}";
          case EFlatFolderNaming.book_author:
            return $"{book.TitleFile} - {book.AuthorFile}";
        } else
        return $"{book.AuthorFile} - {book.TitleFile}";
    }

    private string initDirectory (Book book, string author, string title) {

      string rootDirLong = Settings.OutputDirectory.AsUnc();

      string outDirLong;
      string outDirAuthorLong;
      try {
        if (string.IsNullOrWhiteSpace (author)) {
          outDirAuthorLong = rootDirLong;
          outDirLong = Path.Combine (rootDirLong, title);
        } else {
          outDirAuthorLong = Path.Combine (rootDirLong, author);
          if (!Directory.Exists (outDirAuthorLong))
            book.IsNewAuthor = true;
          outDirLong = Path.Combine (outDirAuthorLong, title);
        }

        bool newFolder = false;
        if (Directory.Exists (outDirLong)) {

          int cnt = Directory.GetDirectories (outDirLong).Length;
          cnt += Directory.GetFiles (outDirLong).Length;
          if (cnt > 0) {
            bool? result = initDirectoryInteraction (book, outDirLong);
            if (result is null)
              return null;
            newFolder = result.Value;
          }
        }

        string dirtitle = title;
        if (newFolder) {
          int n = 2;
          while (true) {
            dirtitle = $"{title} ({n})";
            outDirLong = Path.Combine (
              outDirAuthorLong,
              dirtitle
            );
            if (!Directory.Exists (outDirLong))
              break;
            n++;
          }
        }

        book.OutDirectoryLong = outDirLong;

        Directory.CreateDirectory (outDirLong);
        DirectoryInfo di = new DirectoryInfo (outDirLong);
        if (newFolder || book.PartsType != Book.EParts.some)
          di.Clear ();
        else
          createPartDirectories (book);


      } catch (Exception exc) {
        exceptionCallback (exc);
        return null;
      }

      return outDirLong;

    }

    private bool? initDirectoryInteraction (Book book, string outDirLong) {

      lock (_lockable) {
        Log (3, this, () => $"\"{outDirLong}\", dir not empty");
        bool? result = null;
        if (!(MultiBookInitDirectoryQuestion is null) && (MultiBookInitDirectoryQuestion.HasBeenAnswered ?? false)) {
          result = MultiBookInitDirectoryQuestion.Answer;
          Log (3, this, () => $"\"{outDirLong}\", dir not empty, using previous answer={result}");
        } else {
          string outDir = outDirLong.StripUnc();
          result = directoryCreationCallback (outDir, book.PartsType);
          Log (3, this, () => $"\"{outDirLong}\", dir not empty, answer={result}");

          if (!(MultiBookInitDirectoryQuestion is null) && MultiBookInitDirectoryQuestion.HasBeenAnswered is null) {
            MultiBookInitDirectoryQuestion.HasBeenAnswered =
              Callbacks.Interact (Resources.MsgDirectoryCreationCallbackForAll, ECallbackType.question);
            if (MultiBookInitDirectoryQuestion.HasBeenAnswered ?? false)
              MultiBookInitDirectoryQuestion.Answer = result;
            Log (3, this, () => $"\"{outDirLong}\", dir not empty, save answer={MultiBookInitDirectoryQuestion.HasBeenAnswered}");
          }

        }
        if (!result.HasValue) {
          Callbacks.Progress (new ProgressMessage {
            Info = ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
          });
          return null;
        }
        bool newFolder = !result.Value;
        return newFolder;
      }
    }

    private void createPartDirectories (Book book) {
      if (book.PartsType != Book.EParts.some)
        return;

      bool usePartDirs = Settings.ConvMode != EConvMode.single || Settings.ExtraMetaFiles;
      if (usePartDirs)
        foreach (var part in book.Parts) {
          string dirname = TagAndFileNamingHelper.GetPartDirectoryName (Resources, Settings, book, part);
          Directory.CreateDirectory (dirname);
          DirectoryInfo di = new DirectoryInfo (dirname);
          di.Clear ();
        }

      DirectoryInfo dib = new DirectoryInfo (book.OutDirectoryLong);
      var dis = dib.GetDirectories ();
      var fis = dib.GetFiles ();
      string ptPrefix = TagAndFileNamingHelper.GetPartPrefix (Resources, Settings, book);

      foreach (var di in dis)
        if (!di.Name.StartsWith (ptPrefix) || !usePartDirs)
          di.Delete (true);

      string ext = book.Parts?.First().IsMp3Stream ?? false ? MP3 : MP4;
      foreach (var fi in fis) {
        string fext = fi.Extension.ToLower (); 
        if (usePartDirs || ext != fext)
          fi.Delete ();
      }
    }


    private void initTempDirectory () {
      try {
        Directory.CreateDirectory (TempDirectoryLong);
        cleanTempDirectory ();
      } catch (Exception exc) {
        exceptionCallback (exc);
      }
    }

    private string getAppContentDirectory () {
      var dirLocalState = ActivationCodeApp.GetPackageDirectories ()?.FirstOrDefault ();
      if (dirLocalState is null)
        return null;

      string contentDir = Path.Combine (dirLocalState, AudibleAppContentMetadata.CONTENT);
      if (Directory.Exists (contentDir))
        return contentDir;

      return null;
    }

    private bool activationErrorCallback (Book.BookPart part) {
      string message = $"{Resources.MsgActivationError1} \r\n\"{part.AaxFileItem.BookTitle}\"\r\n{Resources.MsgActivationError2}";
      bool? result = Callbacks.Interact (message, ECallbackType.errorQuestion);
      if (result ?? false)
        return Callbacks.Interact (EInteractionCustomCallback.noActivationCode) ?? false;
      else
        Callbacks.Interact (Resources.MsgActivationError3, ECallbackType.warning);
      return false;
    }

    private bool? directoryCreationCallback (string outDir, Book.EParts partsType) {
      string r = partsType == Book.EParts.some ? Resources.MsgDirectoryPartCreationCallback : Resources.MsgDirectoryCreationCallback;
      string msg = $"\"{outDir}\"\r\n{r}";
      return Callbacks.Interact (msg, ECallbackType.question3);
    }

    private void exceptionCallback (Exception exc) {
      string message = exc.ToShortString(true);
      Callbacks.Interact (message, ECallbackType.error);
    }

    #endregion Private Methods
  }
}
