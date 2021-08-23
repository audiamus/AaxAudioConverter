﻿//#define MANY_CHAPTERS_TEST
//#define MP3_CHAPTERS_BY_FFMPEG

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using audiamus.aaxconv.lib.ex;
using audiamus.aux;
using audiamus.aux.diagn;
using audiamus.aux.ex;
using static audiamus.aux.ApplEnv;
using static audiamus.aux.Logging;
using Encoding = audiamus.aux.Encoding;
using AACM = audiamus.aaxconv.lib.AudibleAppContentMetadata;

namespace audiamus.aaxconv.lib {
  public partial class AaxAudioConverter : IPreviewTitle, IDisposable {

    #region Private Fields
    const string AAX = AaxFileItem.EXT_AAX;
    const string AA = AaxFileItem.EXT_AA;
    const string AAC = ".aac";
    const string M4A = TagAndFileNamingHelper.EXT_M4A;
    const string M4B = TagAndFileNamingHelper.EXT_M4B;
    const string MP3 = TagAndFileNamingHelper.EXT_MP3;

    const int SAMPLE_RATE_44100 = 44100;

    private readonly object _lockable = new object ();

    private readonly Regex _rgxPart; // = new Regex (@"^(.*)\s+(\w+)\s+(\d+)$", RegexOptions.Compiled);
    private static readonly Regex _rgxChapter = new Regex (@"^(\w+)\s+", RegexOptions.Compiled);
    
    private static readonly Regex _rgxNonWord = new Regex (@"\W", RegexOptions.Compiled);
    private readonly ActivationCode _activationCode;
    private readonly IConvSettings _settings;
    private readonly IResources _resources;

    public static readonly Version FFMPEG_VERSION = new Version (4, 1);
    private readonly DegreeOfParallelism _degreeOfParallelismLimit;
    private readonly List<FileError> _fileErrors = new List<FileError> ();

    #endregion Private Fields
    #region Public Properties

    public IEnumerable<uint> NumericActivationCodes => _activationCode.NumericCodes;
    public bool HasActivationCode => _activationCode.HasActivationCode;
    public IConvSettings Settings => _settings;
    public string AppContentDirectory => getAppContentDirectory ();
    public string AutoLaunchAudioFile { get; private set; }
    public TimeSpan StopwatchElapsed => Stopwatch.Elapsed;
    public IEnumerable<string> FileErrors {
      get
      {
        lock (_lockable)
          return _fileErrors.Select(fe => fe.ToShortString()).ToArray ();
      }
    }

    public bool VerifiedFFmpegPath { get; private set; }

    #endregion Public Properties
    #region Private Properties

    private CallbackWrapper Callbacks { get; set; }

    private ParallelOptions DefaultParallelOptions { get; set; }
    private DegreeOfParallelism DegreeOfParallelismLimit => _degreeOfParallelismLimit;
    private int MaxDegreeOfParallelism => Settings.NonParallel ? 1 : ProcessorCount;
    private IResources Resources => _resources;

    private string MP4 => Settings.M4B ? M4B : M4A;

    private MultiBookInitDirectoryHandling MultiBookInitDirectoryQuestion { get; set; }

    // support long path names
    private static string TempDirectoryLong { get; } = TempDirectory.AsUnc();

    private Stopwatch Stopwatch { get; } = new Stopwatch ();
    
    #endregion Private Properties
    #region Public Constructors

    public AaxAudioConverter (IConvSettings settings, IResources resources) {
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


    public async Task<IEnumerable<AaxFileItem>> AddFilesAsync (IEnumerable<string> filenames, IInteractionCallback<InteractionMessage, bool?> callback) {
      DefaultParallelOptions = new ParallelOptions {
        MaxDegreeOfParallelism = MaxDegreeOfParallelism
      };

      Callbacks = new CallbackWrapper (default, null, callback);

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

    public string VerifyFFmpegPath (bool? relaxedFFmpegVersionCheck = null) {
      var ffmpeg = new FFmpeg (null);
      Log (2, this, () => $"dir=\"{getFFmpegPath ().SubstitUser ()}\"");
      bool succ = ffmpeg.VerifyFFmpegPath (relaxedFFmpegVersionCheck ?? Settings.RelaxedFFmpegVersionCheck);
      VerifiedFFmpegPath = succ;
      if (succ) {
        Log (2, this, () => $"version {ffmpeg.Version}");
        return ffmpeg.Version;
      } else {
        Log (2, this, () => "fail");
        return null;
      }
    }

    public async Task<bool> VerifyFFmpegPathVersionAsync (
      Func<InteractionMessage, bool?> callback, bool? relaxedFFmpegVersionCheck = null, bool autoAcceptMismatch = false
    ) {
      return await Task.Run (() => VerifyFFmpegPathVersion (callback, relaxedFFmpegVersionCheck, autoAcceptMismatch));
    }

    public bool VerifyFFmpegPathVersion (Func<InteractionMessage, bool?> callback, bool? relaxedFFmpegVersionCheck = null, bool autoAcceptMismatch = false) {
      var version = VerifyFFmpegPath (relaxedFFmpegVersionCheck);

      bool succ = Version.TryParse(version, out Version parsedVersion);

      if ((succ && parsedVersion < FFMPEG_VERSION) || (!succ && relaxedFFmpegVersionCheck == true)) {
        var sb = new StringBuilder ();
        sb.AppendLine ($"{ApplName} {Resources.MsgFFmpegVersion1} {FFMPEG_VERSION}.");
        sb.AppendLine ($"{Resources.MsgFFmpegVersion2} {version}.");
        sb.AppendLine ($"{Resources.MsgFFmpegVersion3}?");
        sb.Append ($"({Resources.MsgFFmpegVersion4}.)");

        succ = autoAcceptMismatch || (callback(new InteractionMessage { Message = sb.ToString (), Type = ECallbackType.question }) ?? false);
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

    public string SortingTitleWithPart (AaxFileItem fileItem) {
      var (match, sortingTitle, partNameStub, part) = parseTitleForPart (fileItem.BookTitle);
      string helper;
      if (match)
        helper = $"{sortingTitle} {partNameStub} {part:d4}";
      else
        helper = sortingTitle;
      return helper;
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
            Log (3, this, () => $"fail: \"{path.SubstitUser ()}\"");
            return;
          }

          string ext = Path.GetExtension (path).ToLowerInvariant ();
          succ = ext == AAX || ext == AA;
          if (!succ)
            return;

          try {
            var item = new AaxFileItem (path);
            Log (3, this, () => $"\"{item}\"");
            items.Add (item);
          } catch (Exception exc) {
            exceptionCallback (exc);
          }
        });
      } catch (OperationCanceledException) { }

      if (items.Count > 0)
        Settings.InputDirectory = Path.GetDirectoryName (items.Last ().FileName).StripUnc();
      return items;
    }

    private bool checkActivationAndGetChapters (Book book) {
      foreach (var part in book.Parts) {
        bool succ = checkActivationAndGetChapters (part);
        if (!succ)
          return false;
      }
      return true;
    }

    private bool checkActivationAndGetChapters (Book.Part part) {
      lock (_lockable) {
        while (true) {
          bool? succ = null;
          foreach (string code in _activationCode.ActivationCodes) {
            succ = verifyActivationAndGetChapters (part, code);
            if (!succ.HasValue)
              return false;
            if (succ.Value)
              return true;
          }

          if (!activationErrorCallback (part))
            return false;
          _activationCode.ReinitActivationCode ();
        }
      }
    }
       
    private bool? verifyActivationAndGetChapters (Book.Part part, string activationcode) {
      bool withChapters = true;
      Log (3, this, () => $"\"{part.AaxFileItem.FileName.SubstitUser()}\"");
      FFmpeg ffmpeg = new FFmpeg (part.AaxFileItem.FileName);
      bool succ = ffmpeg.VerifyActivation (activationcode, withChapters);
      if (ffmpeg.HasNoActivation)
        succ = ffmpeg.VerifyActivation (null, withChapters);
      else
        part.ActivationCode = activationcode;

      if (succ) {
        if (!(ffmpeg.IsAaFile || ffmpeg.IsAaxFile)) {
          Callbacks.Interact (Resources.MsgInvalidInternalFileType, ECallbackType.error);
          return null;
        }

        part.IsMp3Stream = ffmpeg.IsMp3Stream;
        part.Chapters = ffmpeg.Chapters;
#if MANY_CHAPTERS_TEST
        makeManyChapters (part);
#endif
        part.ComplementChapterNames ();

        Log (3, this, () => chapterList (part, "aax/aa"));

        getContentMetadata (part);
      }

      return succ;
    }

#if MANY_CHAPTERS_TEST
    private void makeManyChapters (Book.Part part) {
      double totalDuration = part.AaxFileItem.Duration.TotalSeconds;
      const int NUM_CHAPTERS = 378;
      double durationPerChapter = totalDuration / NUM_CHAPTERS;
      TimeSpan dpch = TimeSpan.FromSeconds (durationPerChapter);
      var manyChapters = new List<Chapter> ();
      for (int i = 0; i < NUM_CHAPTERS; i++) {
        TimeSpan bgn = TimeSpan.Zero;
        if (i > 0)
          bgn = manyChapters.Last ().Time.End;
        TimeSpan end = bgn + dpch;
        var ch = new Chapter (bgn, end) { Name = $"Test chapter {i + 1}" };
        manyChapters.Add (ch);
      }
      part.Chapters = manyChapters;
    }
#endif

    private string chapterList (Book.Part part, string source) {
      var sb = new StringBuilder ($"{source} chapters:");
      sb.AppendLine ();
      if (!(part.Chapters is null)) {
        foreach (var ch in part.Chapters)
          sb.AppendLine ($"  {ch.ToStringEx()}");
      }
      sb.Append ($"  duration={part.AaxFileItem.Duration.ToStringHMSm()}");
      return sb.ToString ();
    }

    private void convert (IEnumerable<AaxFileItem> fileitems) {
      Log (2, this, () => $"#files={fileitems.Count ()}{(Level <= 2 ? $", format={Settings.ConvFormat}, mode={Settings.ConvMode}" : string.Empty)}");
      Log (3, this, () => Settings.Dump (EDumpFlags.byInterface | EDumpFlags.inherInterfaceAttribs | EDumpFlags.inclNullVals));

      using (new ResourceGuard (f =>
      {
        if (f) {
          Stopwatch.Restart ();
        } else {
          Stopwatch.Stop ();
          Log (2, this, () => $"Done. #files={fileitems.Count ()} (format={Settings.ConvFormat}, mode={Settings.ConvMode}), Elapsed={Stopwatch.Elapsed.ToStringHMSm()}");
        }
      })) {
        lock (_lockable)
          _fileErrors.Clear ();

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
              if (!book.IsNull ())
                books.Add (book);
              // new book, save part name stub
              book = new Book (sortingtitle) { PartNameStub = mpb.partNameStub };
            }
            // same book
            book.AddPart (fi, part);

          } else {
            // not a multipart book
            // any previous book now complete, can process it
            if (!book.IsNull ()) {
              books.Add (book);
              book = null;
            }

            book = new Book (fi);
          }
        }
        if (!book.IsNull ())
          books.Add (book);


        if (Settings.ConvertByFileDate)
          books = books
            .OrderBy (b => b.Parts.FirstOrDefault ()?.AaxFileItem.FileDate ?? default)
            .ToList ();

        convertBooksParallel (books);

        setAutoLaunchAudioFile (books);
      }

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
      Log (2, this, () => $"#books={books.Count ()}, #parts={books.SelectMany (b => b.Parts).Count ()}");

      initMultiBookInitDirectoryQuestion (books);

      initProgress (books);

      // parallel only in "single" mode
      try {
        Parallel.ForEach (books, parallelOptions (DegreeOfParallelismLimit.Book), b => convertBook (b));
      } catch (OperationCanceledException) { }

      cleanTempDirectory ();
    }

    private void initMultiBookInitDirectoryQuestion (IEnumerable<Book> books) {
      bool preAnswered = Settings.OutFolderConflict != EOutFolderConflict.ask;
      bool needsMultiBookInitDirectoryQuestion = books.Count () > 1 || preAnswered;

      MultiBookInitDirectoryQuestion = needsMultiBookInitDirectoryQuestion ? new MultiBookInitDirectoryHandling () : null;
      if (MultiBookInitDirectoryQuestion != null && preAnswered) {
        MultiBookInitDirectoryQuestion.HasBeenAnswered = true;
        bool? answer = null;
        switch (Settings.OutFolderConflict) {
          case EOutFolderConflict.overwrite:
            answer = true;
            break;
          case EOutFolderConflict.new_folder:
            answer = false;
            break;
        }
        MultiBookInitDirectoryQuestion.Answer = answer;
      }
    }

    private void initProgress (IEnumerable<Book> books) {
      int numInitialTracks = books.Select (b => b.Parts.Count).Sum ();

      foreach (var book in books) {

        List<EProgressPhase> phases = new List<EProgressPhase> ();

        if (Settings.FixAACEncoding == EFixAACEncoding.allModes && 
            Settings.ConvFormat == EConvFormat.mp4 && 
            book.Parts.Where (p => !p.IsMp3Stream && p.AaxFileItem.SampleRate == SAMPLE_RATE_44100).Any()) {
          phases.Add (EProgressPhase.copying);
        }

        switch (Settings.ConvMode) {
          case EConvMode.single:
            if (Settings.VerifyAdjustChapterMarks >= EVerifyAdjustChapterMarks.allModes || 
                Settings.UseEmbeddedChapterTimes())
              phases.Add (EProgressPhase.silence);
            break;
          case EConvMode.chapters:
            if (Settings.VerifyAdjustChapterMarks >= EVerifyAdjustChapterMarks.allModes ||
                Settings.UseEmbeddedChapterTimes ()) { 
              phases.Add (EProgressPhase.silence);
              phases.Add (EProgressPhase.adjust);
            } else
              phases.Add (EProgressPhase.copying);
            break;
          case EConvMode.splitChapters:
            phases.Add (EProgressPhase.silence);
            if (Settings.VerifyAdjustChapterMarks >= EVerifyAdjustChapterMarks.splitChapterOrTimeMode ||
                Settings.UseEmbeddedChapterTimes ())
              phases.Add (EProgressPhase.adjust);
            break;
          case EConvMode.splitTime:
            phases.Add (EProgressPhase.copying);
            phases.Add (EProgressPhase.silence);
            break;
        }

        phases.Add (EProgressPhase.transcoding);
        if (Settings.AaxCopyMode != EAaxCopyMode.no)
          phases.Add (EProgressPhase.copying);

        book.InitProgress (Settings);
        book.Progress.AddRange (phases);
      }

      uint numProcessingPartsWeighted = (uint)(books.Select (b => b.Progress.TotalPhaseWeights ()).Sum ());

      Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={numInitialTracks} (basic)");
      Callbacks.Progress (new ProgressMessage {
        AddTotalWeightedPhases = numProcessingPartsWeighted,
        AddTotalSteps = numInitialTracks // number of tracks, initially just as many parts
      });

      if (Settings.AaxCopyMode != default && Directory.Exists (Settings.AaxCopyDirectory)) {
        Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={numInitialTracks} (aaxcopy)");
        Callbacks.Progress (new ProgressMessage {
          AddTotalSteps = numInitialTracks // one more for each part
        });
      }
    }

    private void convertBook (Book book) {
      using (new ResourceGuard (f =>
      {
        if (f) {
          book.Stopwatch.Start ();
        } else {
          book.Stopwatch.Stop ();
          //Callbacks.Progress (new ProgressMessage { AddTotalSteps = 0 }); // To induce a log message

          Log (2, this, () =>
            $"\"{book.SortingTitle.Shorten ()}\", #parts={book.Parts.Count ()} (format={Settings.ConvFormat}, mode={Settings.ConvMode}): " +
            $"Done. Elapsed={book.Stopwatch.Elapsed.ToStringHMSm ()}");
        }
      })) {

        Log (2, this, () => $"\"{book.SortingTitle.Shorten ()}\", #parts={book.Parts.Count ()}");

        if (Settings.ConvMode != EConvMode.single) {
          Log (3, this, () => $"{nameof (ProgressMessage.ResetSteps)}");
          Callbacks.Progress (new ProgressMessage { ResetSteps = true });
        }

        // long books, single mode: default 32bit FFmpeg may run out of memory
        book.Parts.ForEach (p => p.SetFFmpegPrefer64Bit (Settings));

        // Check activation codes
        //   and generally decodable format 
        // Also retrieve chapters on the fly
        bool succ = checkActivationAndGetChapters (book);

        if (succ) {
          book.InitAuthorTitle (Settings);

          preProcessChapters (book);

          string outDir = initDirectory (book);
          if (outDir is null)
            return;
          
          fixAACEncoding (book);

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
              if (Callbacks?.Cancelled ?? false)
                deleteOutDirectory (book);
            }
          }

          if (Settings.ConvMode != EConvMode.single)
            cleanTempDirectory ();
        } else {
          Callbacks.Progress (new ProgressMessage {
            //IncParts = book.NumProgressPhases,
            IncWeightedPhases = (uint)book.Progress.TotalPhaseWeights (),
            IncSteps = (uint)book.Parts.Count 
          });

        }
      }
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

      // EProgressPhase.copying;
      EProgressPhase phase = book.Progress.NextPhase ();
      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, phase) :
            ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
        }))) {
        var afc = new AaxFileCopier (book, Settings, Resources, Callbacks);
        afc.Copy (Callbacks.Progress);
        Callbacks.Progress (new ProgressMessage { IncWeightedPhases = (uint)book.Progress.CurrentPhaseWeightBook });
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

        preprocessEmbeddedChapters (part, i, shortChapter, veryShortChapter);
        if (part.HasNamedChapters)
          preprocessNamedChapters (part);

        preProcessLastChapterResidue (part);

        int numPaddingChapters = 0;

        if (Settings.ConvMode == EConvMode.single && 
            Settings.VerifyAdjustChapterMarks < EVerifyAdjustChapterMarks.allModes &&
            !Settings.UseEmbeddedChapterTimes ())
          combineChaptersSingleMode (part);
        else if (Settings.VerifyAdjustChapterMarks > EVerifyAdjustChapterMarks.no || 
                 Settings.UseEmbeddedChapterTimes ())
          numPaddingChapters = part.InsertPaddingChapters ();

        // done with one part, now knowing the number of chapters.
        // adding chapters to track progress, minus one for the part itself which has already been accounted for.
        // Also set counters.
        var cnt = book.Counts (Settings.ConvMode, false, part);
        int? addSteps = null;
        if (Settings.ConvMode != EConvMode.single) {
          addSteps = part.Chapters.Count - 1 - numPaddingChapters;
          Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={addSteps} (chapters)");
        }
        Callbacks.Progress (new ProgressMessage {
          AddTotalSteps = addSteps,
          Info = ProgressInfo.ProgressInfoBookCounters (cnt.title, cnt.chapters, cnt.tracks, cnt.part)
        });
      }
    }

    private void preProcessLastChapterResidue (Book.Part part) {
      // This is to handle inaccurate last chapter marks from content_metadata only.
      
      if (Settings.VeryShortChapterSec == 0 || !part.HasNamedChapters)
        return;

      // do we have a case? 
      var lastChapter = part.Chapters.Last ();
      if (lastChapter.Time.Duration > Chapter.TS_MIN_LAST_CHAPTER_LENGTH)
        return;
      
      // double check, trust the aax embedded chapters. 
      // Chapters have already been swapped, so aax chapters are in Chapters2
      if (part.Chapters2.Last ().Time.Duration < Chapter.TS_MIN_LAST_CHAPTER_LENGTH)
        return;

      Log (3, this, () => $"Remove last chapter, shorter than {Chapter.TS_MIN_LAST_CHAPTER_LENGTH.TotalSeconds}s: {lastChapter}");
      part.Chapters.Remove (lastChapter);
    }

    private void combineChaptersSingleMode (Book.Part part) {
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

    }


    private void preprocessEmbeddedChapters (Book.Part part, int i, TimeSpan shortChapter, TimeSpan veryShortChapter) {
      Book book = part.Book;
      if (veryShortChapter >= TimeSpan.Zero && (!part.HasNamedChapters || (book.PartsType == Book.EParts.all && i > 0))) {
        var chapter = part.Chapters.LastOrDefault ();
        if (chapter?.Time.Duration <= veryShortChapter) {
          Log (3, this, () => $"Remove last very short ({veryShortChapter.TotalSeconds}s): {chapter}");
          part.Chapters.Remove (chapter);
          part.ChaptersCurtailed = true;
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
            part.ChaptersCurtailed = true;
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

    private void preprocessNamedChapters (Book.Part part) {
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
        adjustNamedChapters (part, tBeg, tEnd, part.Book);
      }

      // finally
      part.SwapChapters ();
    }

    private void adjustNamedChapters (Book.Part part, TimeSpan tBeg, TimeSpan tEnd, Book book = null) {
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
          part.ChaptersCurtailed = true;
        } else {
          if (ch.Time.End > tEnd) {
            Log (3, this, () => $"Alter last : {ch}, -> {tEnd.ToStringHMSm()}");
            ch.Time.End = tEnd;
            part.ChaptersCurtailed = true;
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


    private bool fixAACEncoding (Book book) {
      if (Settings.ConvFormat == EConvFormat.mp3 || Settings.FixAACEncoding != EFixAACEncoding.allModes)
        return true;

      var parts = new List<Book.Part> ();

      foreach (var part in book.Parts) {
        if (part.AaxFileItem.SampleRate != SAMPLE_RATE_44100)
          continue;

        string filestub = book.AuthorFile + " - " + book.TitleFile;
        string filename = $"{filestub} - {part.PartNumber}{AAX}";
        part.TmpFileNameAACFixed = Path.Combine (TempDirectoryLong, filename);
        parts.Add (part);
      }

      int addSteps = parts.Count;
      if (addSteps == 0)
        return true;

      Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={addSteps} (fix aac)");
      Callbacks.Progress (new ProgressMessage {
        AddTotalSteps = addSteps,
        Info = ProgressInfo.ProgressInfoBookCounters (book.TitleTag, 0)
      });



      // EProgressPhase.copying;
      EProgressPhase phase = book.Progress.NextPhase ();
      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, phase) :
            ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
        }))) {


        try {
          Parallel.ForEach (parts, DefaultParallelOptions, part =>
          {
            if (Callbacks.Cancelled)
              return;

            using (new ResourceGuard (f =>
            {
              Callbacks.Progress (
                new ProgressMessage {
                  Info = f ?
                    ProgressInfo.ProgressInfoPart (book.TitleTag, (uint)part.PartNumber) :
                    ProgressInfo.ProgressInfoPartCancel (book.TitleTag, (uint)part.PartNumber)
                });
            })) {

              bool succ = copyFixAACEncoding (part.AaxFileItem.FileName, part.TmpFileNameAACFixed);
              if (!succ)
                part.TmpFileNameAACFixed = null;
            }

          });
        } catch (OperationCanceledException) { }
      }

      Callbacks.Progress (new ProgressMessage { IncWeightedPhases = (uint)book.Progress.CurrentPhaseWeightBook });

      bool fail = parts.Where (p => p.TmpFileNameAACFixed is null).Any ();
      return !fail;
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

      if (Settings.VerifyAdjustChapterMarks == EVerifyAdjustChapterMarks.allModes ||
          Settings.UseEmbeddedChapterTimes ()) {
        bool succ = preprocessAudioChapterMode (book);
        if (!succ)
          return false;
      }

      foreach (var part in book.Parts) {

        if (Callbacks.Cancelled)
          return false;

        if (Settings.VerifyAdjustChapterMarks == EVerifyAdjustChapterMarks.allModes || 
            Settings.UseEmbeddedChapterTimes ())
          combineChaptersSingleMode (part);

        if (Settings.IntermedCopySingle && Settings.ConvFormat == EConvFormat.mp4 && !part.IsMp3Stream) {
          int addSteps = 1;         
          if (part.AaxFileItem.SampleRate == SAMPLE_RATE_44100 && Settings.FixAACEncoding == EFixAACEncoding.withIntermediateCopy)
            addSteps += 1;
         
          Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={addSteps} (intermed copy / fix aac)");

          Callbacks.Progress (new ProgressMessage { AddTotalSteps = addSteps });

          string filestub = book.AuthorFile + " - " + book.TitleFile;
          string filenameStub = $"{filestub} - {part.PartNumber}";
          part.TmpFileNameIntermedCopy = Path.Combine (TempDirectoryLong, filenameStub + AAC);
        }

        Track track = new Track (part.AaxFileItem.Duration);

        part.Tracks.Add (track);
      }

      TagAndFileNamingHelper.SetFileNames (Resources, Settings, book);
      return true;
    }

    private bool prepareTrackFilesChapterMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      bool succ = preprocessAudioChapterMode (book);
      if (!succ)
        return false;

      nameTrackFilesChapterMode (book);

      book.SetMetaChapters (Settings);

      return true;
    }

    private bool prepareTrackFilesSplitChapterMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      bool succ = silenceAndCueSheet (book, null);
      if (!succ)
        return false;
       
      nameTrackFilesSplitChapterMode (book);

      book.SetMetaChapters (Settings);

      return true;
    }

    private bool prepareTrackFilesSplitTimeMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      bool succ = copyBookTempParallel (book, false);
      if (!succ)
        return false;

      succ = silenceAndCueSheet (book, mergeSilences);
      if (!succ)
        return false;

      nameTrackFilesSplitChapterMode (book);

      book.SetMetaChapters (Settings);

      return true;
    }

    private bool preprocessAudioChapterMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");

      bool succ = true;

      if (Settings.VerifyAdjustChapterMarks < EVerifyAdjustChapterMarks.allModes && 
          !Settings.UseEmbeddedChapterTimes ()) {
        succ = copyBookTempParallel (book, true);
      } else {
        int totalNumChapters = detectSilenceBook (book);
        if (totalNumChapters >= 0) {
          if (Settings.VerifyAdjustChapterMarks == EVerifyAdjustChapterMarks.allModes) {
            verifyAdjustChapterMarks (book);
          } else if (Settings.UseEmbeddedChapterTimes ()) {
            if (Settings.ConvMode == EConvMode.single)
              book.PreferEmbeddedChapterTimes (Settings.PreferEmbeddedChapterTimes);
            else if (Settings.ConvMode == EConvMode.chapters)
              preferEmbeddedChapterTimesWithRetranscoding (book);
          }
        }
      }

      book.RemovePaddingChapters ();

      return succ;
    }

    private void nameTrackFilesChapterMode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");

      foreach (var part in book.Parts) {

        foreach (var chapter in part.Chapters) {

          var t = chapter.Time;
          Track track;
          if (Settings.VerifyAdjustChapterMarks >= EVerifyAdjustChapterMarks.allModes ||
              Settings.UseEmbeddedChapterTimes ()) 
            track = new Track (t.Duration) { Chapter = chapter };
          else
            track = new Track (t.Begin, t.End) { Chapter = chapter };
          part.Tracks.Add (track);

        }
      }

      TagAndFileNamingHelper.SetFileNames (Resources, Settings, book);
    }

    private bool copyBookTempParallel (Book book, bool standalone) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");

      int addSteps = book.Parts.Count;
      Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={addSteps} (tmpcpy)");
      Callbacks.Progress (new ProgressMessage {
        AddTotalSteps = addSteps
      });

      // EProgressPhase.copying
      EProgressPhase phase = book.Progress.NextPhase ();
      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, phase) :
            ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
        }))) {

        string filestub = book.AuthorFile + " - " + book.TitleFile;

        Parallel.For (0, book.Parts.Count, DefaultParallelOptions, i =>
        {
          var part = book.Parts[i];
          Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\", part#={part.PartNumber}");

          copyPartTemp (part, filestub);
          
          Callbacks.Progress (new ProgressMessage { IncWeightedPhases = (uint)book.Progress.CurrentPhaseWeightPart });
        });

        bool succ = !book.Parts.Where (p => p.TmpFileName is null).Any ();
        return succ;
      }
    }

    private void copyPartTemp (Book.Part part, string filestub) {
      Book book = part.Book;
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");

      string ext = part.IsMp3Stream ? MP3 : MP4;

      try {

        if (Callbacks.Cancelled)
          return;

        using (new ResourceGuard (f => Callbacks.Progress (
          new ProgressMessage {
            Info = f ?
            ProgressInfo.ProgressInfoPart (book.TitleTag, (uint)part.PartNumber) :
            ProgressInfo.ProgressInfoPartCancel (book.TitleTag, (uint)part.PartNumber)
          }))) {

          string filename = $"{filestub} - {part.PartNumber}{ext}";
          part.TmpFileName = Path.Combine (TempDirectoryLong, filename);

          // copy part to temp file
          using (var threadProg = new ThreadProgress (Callbacks.Progress)) {

            FFmpeg ffmpeg1 = new FFmpeg (part.AaxFileItem.FileName) {
              Cancel = Callbacks.Cancel,
              Progress = threadProg.Report,
              PreferInternal64Bit = part.FFmpegPrefer64Bit
            };
            Log (3, this, () => $"{nameof (ffmpeg1.Transcode)} in=\"{part.AaxFileItem.FileName.SubstitUser ()}\", out=\"{part.TmpFileName.SubstitUser ()}\"");
            bool succ = ffmpeg1.Transcode (part.TmpFileName, FFmpeg.ETranscode.copy | FFmpeg.ETranscode.noChapters, part.ActivationCode);
            if (!succ) {
              if (ffmpeg1.TranscodingError)
                noteTranscodingError (part, null, null);

              part.TmpFileName = null;
              Log (3, this, () => $"{nameof (ffmpeg1.Transcode)}, succ={succ}, \"{book.SortingTitle.Shorten ()}\", part#={part.PartNumber}");
              return;
            }
          }
        }
      } catch (OperationCanceledException) { }
    }



    private bool silenceAndCueSheet (Book book, Action<Book> callback) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");

      int totalNumChapters = detectSilenceBook (book);
      if (totalNumChapters < 0)
        return false;

      if (Settings.ConvMode >= EConvMode.chapters &&
          Settings.VerifyAdjustChapterMarks >= EVerifyAdjustChapterMarks.splitChapterOrTimeMode)
        verifyAdjustChapterMarks (book);
      else if (Settings.ConvMode == EConvMode.splitChapters && 
               Settings.UseEmbeddedChapterTimes())
        preferEmbeddedChapterTimesWithRetranscoding (book);
      else
        book.PreferEmbeddedChapterTimes (Settings.PreferEmbeddedChapterTimes);

      book.RemovePaddingChapters ();
      //preferEmbeddedChapterTimes (book);

      totalNumChapters = book.Parts.Select (p => p.Chapters.Count).Sum ();

      callback?.Invoke (book);

      book.CreateCueSheet (Settings);

      // we now have all the tracks. Add them minus the number of chapters which have already been accounted for
      // this can be negative in split time mode with long tracks
      int totalNumTracks = book.Parts.Select (p => p.Tracks.Count).Sum ();
      if (totalNumTracks != totalNumChapters) {
        int addSteps = totalNumTracks - totalNumChapters;
        Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={addSteps} (tracks)");
        Callbacks.Progress (new ProgressMessage {
          AddTotalSteps = addSteps
        });
      }

      Log (3, this, () => $"{nameof (book.CreateCueSheet)} for \"{book.SortingTitle.Shorten()}\" determined {totalNumTracks} tracks");

      return true;
    }


    // returns total number of chapters if success, otherwise -1
    private int detectSilenceBook (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");

      // silence detection will first split into chapters and then detect silence in each chapter.
      // hence add twice the total number of chapters to track progress.
      int totalNumChapters = book.Parts.Select (p => p.Chapters.Count).Sum ();
      int addSteps = totalNumChapters * 2;

      Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={addSteps} (silence)");
      Callbacks.Progress (new ProgressMessage {
        AddTotalSteps = addSteps,
        Info = ProgressInfo.ProgressInfoBookCounters (book.TitleTag, (uint)totalNumChapters)
      });

      bool succ = detectSilenceParts (book);
      if (!succ) {
       Log (2, this, () => $"{nameof(detectSilenceParts)}, succ={succ}, \"{book.SortingTitle.Shorten()}\"");
       return -1;
      }
      Log (3, this, () => 
        $"{nameof (detectSilenceParts)} for \"{book.SortingTitle.Shorten()}\" found {book.Parts.SelectMany (p => p.Chapters.Select(c => c.Silences.Count)).Sum ()} positions total");

      return totalNumChapters;
    }

    private void verifyAdjustChapterMarks (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");

      // EProgressPhase.adjust;
      bool isAnyChapterMode = Settings.ConvMode == EConvMode.chapters || Settings.ConvMode == EConvMode.splitChapters;
      EProgressPhase phase = isAnyChapterMode ? book.Progress.NextPhase() : EProgressPhase.adjust;
      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, phase) :
            ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
        }))) 
      {
        int numAffected = book.DetermineTimeAdjustments ();
        if (numAffected == 0 && isAnyChapterMode) {
          Callbacks.Progress (new ProgressMessage { IncWeightedPhases = (uint)book.Progress.CurrentPhaseWeightBook });
          return;
        }

        if (!isAnyChapterMode) {
          adjustTimes (book);
          // no progress report in these cases
          return;
        }

        Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={numAffected} (affected)");
        Callbacks.Progress (new ProgressMessage {
          AddTotalSteps = numAffected,
          Info = ProgressInfo.ProgressInfoBookCounters (book.TitleTag, (uint)numAffected, null, null)
        });

        adjustTimesAndRetranscode (book);
      }
    }


    private void adjustTimesAndRetranscode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");

      foreach (var part in book.Parts) {

        var chapters = part.Chapters.Where (c => !c.TimeAdjustment.IsNull () && !c.IsPaddingChapter).ToList ();
        if (chapters.Count > 0)
          adjustTimesAndRetranscode (part, chapters);

        Callbacks.Progress (new ProgressMessage { IncWeightedPhases = (uint)book.Progress.CurrentPhaseWeightPart });
      }

    }

    private void adjustTimes (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");
      book.AdjustTimesWithPreference (Settings.PreferEmbeddedChapterTimes);
    }

    private void adjustTimesAndRetranscode (Book.Part part, IReadOnlyList<Chapter> chapters) {
      Book book = part.Book;

      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\", part#={part.PartNumber}");
      
      var addChapters = part.AdjustTimesWithPreference (Settings.PreferEmbeddedChapterTimes);
      int cnt1 = chapters.Count;
      if (!addChapters.IsNullOrEmpty ()) {
        var allChapters = chapters.ToList ();
        allChapters.AddRange (addChapters);
        allChapters = allChapters.Distinct ().ToList ();
        allChapters.Sort ((x, y) => TimeSpan.Compare (x.Time.Begin, y.Time.Begin));
        int cnt2 = allChapters.Count;
        if (cnt2 > cnt1) {
          int delta = cnt2 - cnt1;
          chapters = allChapters;
          Callbacks.Progress (new ProgressMessage {
            AddTotalSteps = delta,
            Info = ProgressInfo.ProgressInfoBookCounters (book.TitleTag, (uint)delta, null, null)
          });
        }
      }

      retranscodeParallel (part, chapters);
    }

    private void preferEmbeddedChapterTimesWithRetranscoding (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");
      bool isAnyChapterMode = Settings.ConvMode == EConvMode.chapters || Settings.ConvMode == EConvMode.splitChapters;
      EProgressPhase phase = isAnyChapterMode ? book.Progress.NextPhase () : EProgressPhase.adjust;
      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, phase) :
            ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
        }))) {

        preferEmbeddedChapterTimesAndRetranscode (book);
      }
    }

    private void preferEmbeddedChapterTimesAndRetranscode (Book book) {
      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");

      foreach (var part in book.Parts) {

        var chapters = part.PreferEmbeddedChapterTimes (Settings.PreferEmbeddedChapterTimes == EPreferEmbeddedChapterTimes.always);
        if (!chapters.IsNullOrEmpty ()) {
          int cnt = chapters.Count;
          Log (3, this, () => $"{nameof (ProgressMessage.AddTotalSteps)}={cnt} (affected)");
          Callbacks.Progress (new ProgressMessage {
            AddTotalSteps = cnt,
            Info = ProgressInfo.ProgressInfoBookCounters (book.TitleTag, (uint)cnt, null, null)
          });
          retranscodeParallel (part, chapters);
        }
        Callbacks.Progress (new ProgressMessage { IncWeightedPhases = (uint)book.Progress.CurrentPhaseWeightPart });
      }
    }


    private void retranscodeParallel (Book.Part part, IReadOnlyList<Chapter> chapters) {
      Book book = part.Book;
      string infile = part.ApplicableInFileNameForTranscode;
      string actcode = part.TmpFileName is null ? part.ActivationCode : null;

      Log (3, this, () => $"{nameof (FFmpeg.Transcode)} in=\"{infile.SubstitUser ()}\"");
      try {
        Parallel.For (0, chapters.Count, DefaultParallelOptions, i =>
        {

          if (Callbacks.Cancelled)
            return;

          var chapter = chapters[i];

          uint ch = book.ChapterNumber (chapter);

          using (new ResourceGuard (f => Callbacks.Progress (
            new ProgressMessage {
              Info = f ?
              ProgressInfo.ProgressInfoChapter (book.TitleTag, ch) :
              ProgressInfo.ProgressInfoChapterCancel (book.TitleTag, ch)
            }))) {

            // Extract chapter to temp file
            using (var threadProg = new ThreadProgress (Callbacks.Progress)) {

              var t = chapter.Time;
              FFmpeg ffmpeg1 = new FFmpeg (infile) {
                Cancel = Callbacks.Cancel,
                Progress = threadProg.Report,
                PreferInternal64Bit = part.FFmpegPrefer64Bit
              };
              var modifiers = FFmpeg.ETranscode.copy | FFmpeg.ETranscode.noChapters;
              Log (3, this, () => $"{nameof (FFmpeg.Transcode)} out=\"{chapter.TmpFileName.SubstitUser ()}\", ac={!string.IsNullOrEmpty (actcode)}, {t}");
              bool succ = ffmpeg1.Transcode (chapter.TmpFileName, modifiers, actcode, t.Begin, t.End);
              if (!succ) {
                Log (3, this, () => $"{nameof (FFmpeg.Transcode)}, succ={succ}, \"{book.SortingTitle.Shorten ()}\", part#={part.PartNumber}, chapter=\"{chapter.Name}\"");
                if (ffmpeg1.TranscodingError)
                  noteTranscodingError (part, chapter, null);
                return;
              }
            }

            if (Callbacks.Cancelled)
              return;

          }
        });
      } catch (OperationCanceledException) { }
    }

    private void noteTranscodingError (Book.Part part, Chapter chapter, ITrack track) {
      string filename = Path.GetFileName (part.AaxFileItem.FileName);
      var fe = new FileError (filename, chapter, track);
      Log (3, this, () => fe.ToString());
      lock (_lockable)
        _fileErrors.Add (fe);
    }

    private bool detectSilenceParts (Book book) {
      Log (3, this, $"\"{book.SortingTitle.Shorten ()}\"");

      // EProgressPhase.silence;
      EProgressPhase phase = book.Progress.CurrrentPhase;
      if (phase != EProgressPhase.silence)
        phase = book.Progress.NextPhase ();     
      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, phase) :
            ProgressInfo.ProgressInfoBookCancel (book.TitleTag)
        }))) {

        string filestub = book.AuthorFile + " - " + book.TitleFile;

        // always serial
        foreach (var part in book.Parts) {
                           
          Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}");
          
          // Chapter-wise split to temp folder, for shorter files,
          // because of limited precison (6 digit, "g6") of FFmpeg silence detection filter output.
          // We aim for 10ms resolution. (May still not work this way with very long chapters.)
          detectSilenceChapterParallel (part, filestub);
          bool fail = part.Chapters.Where (c => c.Silences is null).Any ();
          if (fail) {
            Log (3, this, () => $"succ={!fail}, \"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}");
            return false;
          }
          Callbacks.Progress (new ProgressMessage { IncWeightedPhases = (uint)book.Progress.CurrentPhaseWeightPart });
        }
        return true;
      }
    }

    private void detectSilenceChapterParallel (Book.Part part, string filestub) {
      Book book = part.Book;

      Log (3, this, () => $"\"{book.SortingTitle.Shorten ()}\"");

      string ext = part.IsMp3Stream ? MP3 : MP4;

      string infile = part.ApplicableInFileNameForTranscode;
      string actcode = part.TmpFileName is null ? part.ActivationCode : null;

      Log (3, this, () => $"{nameof (FFmpeg.Transcode)} in=\"{infile.SubstitUser()}\", ac={!string.IsNullOrEmpty (actcode)}");
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

            string filename = $"{filestub} - {part.ChapterId(chapter)}{ext}";
            chapter.TmpFileName = Path.Combine (TempDirectoryLong, filename);

            // Extract chapter to temp file
            using (var threadProg = new ThreadProgress (Callbacks.Progress)) {

              var t = chapter.Time;
              FFmpeg ffmpeg1 = new FFmpeg (infile) {
                Cancel = Callbacks.Cancel,
                Progress = threadProg.Report,
                PreferInternal64Bit = part.FFmpegPrefer64Bit
              };
              var modifiers = FFmpeg.ETranscode.copy | FFmpeg.ETranscode.noChapters;
              Log (3, this, () => $"{nameof (ffmpeg1.Transcode)} out=\"{chapter.TmpFileName.SubstitUser()}\", {t}");
              bool succ = ffmpeg1.Transcode (chapter.TmpFileName, modifiers, actcode, t.Begin, t.End);
              if (!succ) {
                Log (3, this, () => $"{nameof (ffmpeg1.Transcode)}, succ={succ}, \"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}, chapter=\"{chapter.Name}\"");
                if (ffmpeg1.TranscodingError)
                  noteTranscodingError (part, chapter, null);
                return;
              }
            }

            if (Callbacks.Cancelled)
              return;

            // Now, find silences in temp. chapter file
            using (var threadProg = new ThreadProgress (Callbacks.Progress)) {
              FFmpeg ffmpeg2 = new FFmpeg (chapter.TmpFileName) {
                Cancel = Callbacks.Cancel,
                Progress = threadProg.Report,
                PreferInternal64Bit = part.FFmpegPrefer64Bit
              };
              bool succ = ffmpeg2.DetectSilence (part.ActivationCode);
              if (!succ) {
                Log (3, this, () => $"{nameof(ffmpeg2.DetectSilence)}, succ={succ}, \"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}, chapter=\"{chapter.Name}\"");
                return;
              }
              chapter.Silences = ffmpeg2.Silences;
              Log (3, this, () => $"{nameof (ffmpeg2.DetectSilence)} in \"{chapter.TmpFileName.SubstitUser()}\" found {chapter.Silences.Count} positions");
            }
          }
        });
      } catch (OperationCanceledException) { }
    }


    private void mergeSilences (Book book) => book.MergeSilences ();

    private void nameTrackFilesSplitChapterMode (Book book) => TagAndFileNamingHelper.SetFileNames (Resources, Settings, book);

    private void transcodeTracks (Book book) {
      if (Callbacks.Cancelled)
        return;
      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\"");


      if (Settings.ConvMode == EConvMode.single && 
           (Settings.VerifyAdjustChapterMarks >= EVerifyAdjustChapterMarks.allModes ||
            Settings.UseEmbeddedChapterTimes ())) {
        Log (3, this, () => $"{nameof (ProgressMessage.ResetSteps)}");
        Callbacks.Progress (new ProgressMessage {
          ResetSteps = true
        });
      }

      // EProgressPhase.transcoding;
      EProgressPhase phase = book.Progress.NextPhase ();
      using (new ResourceGuard (f => Callbacks.Progress (
        new ProgressMessage {
          Info = f ?
            ProgressInfo.ProgressInfoBook (book.TitleTag, phase) :
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
            var cnt = book.Counts (Settings.ConvMode, false, part.Part);
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
            Callbacks.Progress (new ProgressMessage { IncWeightedPhases = (uint)book.Progress.CurrentPhaseWeightPart });
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


    private void transcodeTracksParallel (Book book, Book.Part part, ChapteredTracks.PartChapter partChapter) {
      bool copy = Settings.ConvFormat == EConvFormat.mp4 && !part.IsMp3Stream ||
                  Settings.ConvFormat == EConvFormat.mp3 && part.IsMp3Stream;
      FFmpeg.ETranscode modifiers = copy ? FFmpeg.ETranscode.copy : FFmpeg.ETranscode.normal;

      Log (3, this, () => $"\"{book.SortingTitle.Shorten()}\", part#={part.PartNumber}, chap#={partChapter.ChapterNumber}, #tracks={partChapter.Tracks.Count ()}");

      string inFile = partChapter.Chapter?.TmpFileName ?? part.ApplicableInFileNameForTranscode;
      Log (3, this, () => $"in=\"{inFile.SubstitUser ()}\"");

      try {
        Parallel.For (0, partChapter.Tracks.Count, parallelOptions (DegreeOfParallelismLimit.Track), i =>
        {
          if (Callbacks.Cancelled)
            return;

          var track = partChapter.Tracks[i];

          TagAndFileNamingHelper.SetTrackTitle (Resources, Settings, book, track);

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

              track.State = ETrackState.current;

              if (Settings.ConvMode == EConvMode.single) 
                succ = transcodeTrackSingle (part, inFile, track.FileName, modifiers, threadProg);
              else 
                succ = transcodeTrackMulti (part, track, inFile, modifiers, threadProg);
              

              if (succ)
                succ = updateTags (book, track);

              track.State = succ ? ETrackState.complete : ETrackState.aborted;
            }
          }
        });
      } catch (OperationCanceledException) { }
    }

    private bool transcodeTrackMulti (Book.Part part, Track track, string inFile, FFmpeg.ETranscode modifiers, ThreadProgress threadProg) {

      string outFile = track.FileName;
      string dir = Path.GetDirectoryName (outFile);
      Directory.CreateDirectory (dir);

      bool withActivationCode = part.TmpFileName is null && track.Chapter?.TmpFileName is null;

      if (inFile is null) {
        Log (1, this, () => $"no in file");
        return false;
      }

      string actcode = null;
      if (withActivationCode)
        actcode = part.ActivationCode;
#if MP3_CHAPTERS_BY_FFMPEG
      if (Settings.ConvFormat == EConvFormat.mp3)
        return transcodeTrackMultiWithChapter (part, track, inFile, modifiers, actcode, threadProg);
      else
#endif
      return transcodeTrackMulti (part, track, inFile, modifiers, actcode, threadProg);
    }

    private bool transcodeTrackMultiWithChapter (Book.Part part, Track track, string inFile, FFmpeg.ETranscode modifiers, string actcode, ThreadProgress threadProg) {
      string metafile = ffmpegChapters (part, track);
      if (metafile is null)
        return false;

      FFmpeg ffmpeg = createFFmpegForTranscoding (threadProg, part, inFile, metafile);
      return transcodeTrack (ffmpeg, modifiers, actcode, track.Time, track.FileName, part, track);
    }

    private FFmpeg createFFmpegForTranscoding (ThreadProgress threadProg, Book.Part part, string inFile, string metafile = null) {
      var ffmpeg = new FFmpeg (inFile, metafile) {
        Cancel = Callbacks.Cancel,
        Progress = threadProg.Report,
        PreferInternal64Bit = part.FFmpegPrefer64Bit
      };
      bool applyBitrateAnyway = Settings.ConvFormat == EConvFormat.mp3 && !part.IsMp3Stream;
      ffmpeg.SetBitRate (Settings, part.AaxFileItem, applyBitrateAnyway);
      return ffmpeg;
    }


    private bool transcodeTrackMulti (Book.Part part, Track track, string inFile, FFmpeg.ETranscode modifiers, string actcode, ThreadProgress threadProg) {
      FFmpeg ffmpeg = createFFmpegForTranscoding (threadProg, part, inFile);
      return transcodeTrack (ffmpeg, modifiers, actcode, track.Time, track.FileName, part, track);
    }


    private bool transcodeTrackSingle (Book.Part part, string inFile, string outFile, FFmpeg.ETranscode modifiers, ThreadProgress threadProg) {
      if (part.Chapters is null || part.Chapters.Count == 0)
        return false;

      var tim = part.Chapters.First ().Time;

      //extra intermediate copy for iTunes, single mode m4b
      bool intermediateCopy = 
           Settings.ConvMode == EConvMode.single
        && Settings.ConvFormat == EConvFormat.mp4
        && Settings.IntermedCopySingle
        && !part.IsMp3Stream;

      if (intermediateCopy) {
        return makeIntermediateCopy (part, inFile, outFile, modifiers, tim, threadProg);
      } else {
#if MP3_CHAPTERS_BY_FFMPEG
        if (Settings.ConvFormat == EConvFormat.mp3)
          return transcodeTrackSingleWithChapters (part, inFile, outFile, modifiers, threadProg, tim);
        else
#endif
          return transcodeTrackSingle (part, inFile, outFile, modifiers, threadProg, tim);
      }

    }

    private bool transcodeTrackSingleWithChapters (Book.Part part, string inFile, string outFile, FFmpeg.ETranscode modifiers, ThreadProgress threadProg, TimeInterval tim) {
      string metafile = ffmpegChapters (part, part.Tracks.FirstOrDefault());
      if (metafile is null)
        return false;

      FFmpeg ffmpeg = createFFmpegForTranscoding (threadProg, part, inFile, metafile);
      return transcodeTrack (ffmpeg, modifiers, part.ActivationCode, tim, outFile, part, null);
    }

    private bool transcodeTrackSingle (Book.Part part, string inFile, string outFile, FFmpeg.ETranscode modifiers, ThreadProgress threadProg, TimeInterval tim) {
      FFmpeg ffmpeg = createFFmpegForTranscoding (threadProg, part, inFile);
      return transcodeTrack (ffmpeg, modifiers, part.ActivationCode, tim, outFile, part, null);
    }

    private string ffmpegChapters (Book.Part part, Track track) {
      if (track is null)
        return null;

      IEnumerable<Chapter> chapters = null;
      switch (Settings.ConvMode) {
        case EConvMode.single:
          chapters = part.Chapters2;
          break;
        default:
          chapters = track.MetaChapters;
          break;
      }

      // unique filename required
      string metafile = Path.GetFileNameWithoutExtension (track.FileName) + $" ({part.TrackId(track)}) meta.txt";
      metafile = Path.Combine (TempDirectoryLong, metafile);

      var ffmetadata = new FFMetaData (chapters);
      bool succ = ffmetadata.Write (metafile);
      Log (3, this, () => $"succ={succ}, ffmetafile=\"{metafile.SubstitUser ()}\", #chapters={ffmetadata.Chapters.Count}");
      if (!succ)
        return null;

      return metafile;
    }


    private bool makeIntermediateCopy (Book.Part part, string inFile, string outFile, FFmpeg.ETranscode modifiers, TimeInterval tim, ThreadProgress threadProg) {
      Log (3, this, () => $"out=\"{outFile.SubstitUser ()}\"");

      bool succ = false;
      string sourceFile = inFile;     
      var intermediateFile = part.TmpFileNameIntermedCopy;

      if (part.AaxFileItem.SampleRate == SAMPLE_RATE_44100 && Settings.FixAACEncoding == EFixAACEncoding.withIntermediateCopy) {
        string dir = Path.GetDirectoryName (intermediateFile);
        string filenameStub = Path.GetFileNameWithoutExtension (intermediateFile);
        sourceFile = Path.Combine (dir, filenameStub + AAX);

        Log (3, this, () => $"Fix AAC, out=\"{sourceFile.SubstitUser ()}\"");
        succ = copyFixAACEncoding (inFile, sourceFile);
        if (!succ)
          return false;
      }

      FFmpeg ffmpeg = new FFmpeg (sourceFile) {
        Cancel = Callbacks.Cancel,
        Progress = threadProg.Report,
        PreferInternal64Bit = part.FFmpegPrefer64Bit
      };
      
      succ = transcodeTrack (ffmpeg, modifiers, part.ActivationCode, tim, intermediateFile, part, null);
      if (!succ) {
        return false;
      }
      using (var threadProg2 = new ThreadProgress (Callbacks.Progress)) {
        FFmpeg ffmpeg2 = createFFmpegForTranscoding (threadProg2, part, intermediateFile);
        return transcodeTrack (ffmpeg2, modifiers, null, null, outFile, part, null);
      }
    }

    private bool copyFixAACEncoding (string inFile, string outFile) {
      Log (3, this, () => $"out=\"{outFile.SubstitUser ()}\"");
      var fixAAC = Singleton<FixAtomESDS>.Instance;
      try {
        FileEx.Copy (inFile, outFile, true, fixAAC, Callbacks.Progress, Callbacks.Cancel);
      } catch (Exception exc) {
        Log (1, this, () => exc.ToShortString ());
        exceptionCallback (exc);
        return false;
      }
      return true;
    }

    private bool transcodeTrack (FFmpeg ffmpeg, FFmpeg.ETranscode modifiers, string actcode, TimeInterval t, string outFile, Book.Part part, ITrack track) {
      bool succ;

      // safety catch
      if (!(track?.HasMinDuration ?? Track.MatchesMinDuration(t))) {
        Log (3, this, () => $"Min Duration error, out=\"{outFile.SubstitUser ()}\", mod={modifiers}, ac={!string.IsNullOrEmpty (actcode)}, {(t is null ? "<null>" : t.ToString ())}");
        return true;
      }

      if (t is null)
        succ = ffmpeg.Transcode (outFile, modifiers, actcode, null, null);
      else
        succ = ffmpeg.Transcode (outFile, modifiers, actcode, t.Begin, t.End);
      Log (3, this, () => $"succ={succ}, out=\"{outFile.SubstitUser ()}\", mod={modifiers}, ac={!string.IsNullOrEmpty (actcode)}, {(t is null ? "<null>" : t.ToString())}");
      if (ffmpeg.TranscodingError)
        noteTranscodingError (part, null, track);
      return succ;
    }

    private void getContentMetadata (Book.Part part) {
      bool namedChapters = Settings.NamedChapters != ENamedChapters.no;
      bool withSeries = Settings.WithSeriesTitle;
      if (!(namedChapters || withSeries))
        return;

      AACM.EFlags flags = default;
      if (withSeries && !namedChapters) 
        flags = AACM.EFlags.skuOnly;

      var appMetadadata = new AudibleAppContentMetadata ();
      appMetadadata.GetContentMetadata (part, flags);

      if (!withSeries)
        return;

      var simsBySeries = new AudibleAppSimsBySeries ();
      simsBySeries.GetSimsBySeries (part);
    }

    private bool updateTags (Book book, Track track) {
      bool succ = false;
#if MP3_CHAPTERS_BY_FFMPEG
      if (Settings.ConvFormat == EConvFormat.mp3)
        succ = TagAndFileNamingHelper.WriteMetaData (Resources, Settings, book, track);
      else
#endif
        succ = TagAndFileNamingHelper.WriteMetaData (Resources, Settings, book, track, true);

        Log (3, this, () => $"succ={succ}, track=\"{track.Title}\" \"{Path.GetFileName(track.FileName)}\""); 
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
          Log (3, this, () => "parts none or all"); 
          string path = Path.Combine (book.OutDirectoryLong, filename);
          using (var wr = new StreamWriter (new FileStream (path, FileMode.Create, FileAccess.Write), encoding)) {
            wr.WriteLine (EXTM3U);
            foreach (var part in book.Parts)
              writePlaylistItems (wr, part);
            wr.WriteLine ();
          }
          book.DefaultAudioFile = path;
        } else {
          Log (3, this, () => "parts some"); 
          foreach (var part in book.Parts) {
            string partDir = TagAndFileNamingHelper.GetPartDirectoryName (Resources, Settings, part); 
            string path = Path.Combine (partDir, filename);
            using (var wr = new StreamWriter (new FileStream (path, FileMode.Create, FileAccess.Write))) {
              wr.WriteLine (EXTM3U);
              writePlaylistItems (wr, part);
              wr.WriteLine ();
            }
            if (book.DefaultAudioFile.IsNullOrEmpty())
              book.DefaultAudioFile = path;
          }
        }
      } catch (Exception exc) {
        exceptionCallback (exc);
        Callbacks.Cancel ();
      }
    }

    private void writePlaylistItems (TextWriter wr, Book.Part part) {
      const string EXTINF = "#EXTINF";
      if (part.Tracks is null)
        return;
      string partDir = TagAndFileNamingHelper.GetPartDirectoryName (Resources, Settings, part);

      foreach (var track in part.Tracks) {
        if (!track.HasMinDuration)
          continue;

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
        DirectoryInfo diParent = Directory.GetParent (book.OutDirectoryLong);
        DirectoryInfo diBook = new DirectoryInfo (book.OutDirectoryLong);
        if (book.PartsType != Book.EParts.some) {
          diBook.Delete (true);
        } else {
          foreach (var part in book.Parts) {
            string dir = TagAndFileNamingHelper.GetPartDirectoryName (Resources, Settings, part);
            if (Directory.Exists (dir)) {
              DirectoryInfo dic = new DirectoryInfo (dir);
              dic.Delete (true);
            }
          }
          int n = diBook.GetDirectories ().Count ();
          if (n == 0)
            diBook.Delete (true);
        }

        if (!Settings.FlatFolders && !(diParent is null)) {
          if (book.IsNewSeries) {
            DirectoryInfo diParentAuthor = diParent.Parent;
            int n = diParent.GetDirectories ().Count ();
            if (n == 0) {
              diParent.Delete (true);
              diParent = diParentAuthor;
            }
          }
          if (book.IsNewAuthor && !(diParent is null)) {
            int n = diParent.GetDirectories ().Count ();
            if (n == 0) {
              diParent.Delete (true);
            }
          }
        }

      } catch (Exception exc) {
        exceptionCallback (exc);
      }
      book.Parts.ForEach ( p => p.AaxFileItem.Converted = false);
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
      Log (3, this, () => $"book:\"{book.SortingTitle.Shorten()}\", author={(author is null ? "<null>" : $"\"{author}\"")}, title=\"{title}\"");

      string rootDirLong = Settings.OutputDirectory.AsUnc();

      string outDirLong = rootDirLong;
      string outDirBookParentLong = rootDirLong;
      string series = string.Empty;
      string seriesSeqNo = string.Empty;

      try {

        // author + series + title
        
        // do we have an author?
        if (!(Settings.FlatFolders || author.IsNullOrWhiteSpace ())) {
          outDirLong = Path.Combine (outDirLong, author);
          if (!Directory.Exists (outDirLong))
            book.IsNewAuthor = true;
          outDirBookParentLong = outDirLong;
        }

        // do we have a series?
        if (Settings.WithSeriesTitle && book.HasSeries) {
          series = book.ExternalMeta.SeriesString().Prune();
          seriesSeqNo = book.ExternalMeta.SeqStringFramed (Settings.NumDigitsSeriesSeqNo).Prune();
          outDirLong = Path.Combine (outDirLong, series);
          if (!Directory.Exists (outDirLong))
            book.IsNewSeries = true;
          outDirBookParentLong = outDirLong;
        }

        // we always should have a title

        // full caption for title folder?
        if (Settings.FullCaptionBookFolder) {
          var sb = new StringBuilder ();
          if (!author.IsNullOrWhiteSpace ()) { 
            sb.Append (author);
            sb.Append (" - ");
          }
          if (!series.IsNullOrWhiteSpace ()) {
            sb.Append (series);
            if (!seriesSeqNo.IsNullOrWhiteSpace ())
              sb.Append ($" {seriesSeqNo}");
            sb.Append (" - ");
          }
          sb.Append (title);
          title = sb.ToString ();
        } else if (!seriesSeqNo.IsNullOrWhiteSpace ())
          title = $"{seriesSeqNo} {title}";

        outDirLong = Path.Combine (outDirLong, title);

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
              outDirBookParentLong,
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
          using (new ResourceGuard (f =>
          {
            if (f) {
              Stopwatch.Stop ();
              book.Stopwatch.Stop ();
            } else {
              Stopwatch.Start ();
              book.Stopwatch.Start ();
            }
          })) {

            string outDir = outDirLong.StripUnc ();
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
          string dirname = TagAndFileNamingHelper.GetPartDirectoryName (Resources, Settings, part);
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

    private bool activationErrorCallback (Book.Part part) {
      lock (part.Book) {
        using (new ResourceGuard (f =>
        {
          if (f) {
            Stopwatch.Stop ();
            part.Book.Stopwatch.Stop ();
          } else {
            Stopwatch.Start ();
            part.Book.Stopwatch.Start ();
          }
        })) {
          string message = $"{Resources.MsgActivationError1} \r\n\"{part.AaxFileItem.BookTitle}\"\r\n{Resources.MsgActivationError2}";
          Log (3, this, () => message);
          bool? result = Callbacks.Interact (message, ECallbackType.errorQuestion3);
          switch (result) {
            default:
              // cancelled, skip
              Callbacks.Interact (Resources.MsgActivationError3, ECallbackType.warning);
              return false;
            case true:
              // ask for new custom code, can be cancelled in dialog
              {
                bool succ = Callbacks.Interact (EInteractionCustomCallback.noActivationCode) ?? false;
                if (!succ)
                  Callbacks.Interact (Resources.MsgActivationError3, ECallbackType.warning);
                return succ;
              }
            case false:
              // just retry
              return true;
          }
        }
      }
    }

    private bool? directoryCreationCallback (string outDir, Book.EParts partsType) {
      string r = partsType == Book.EParts.some ? Resources.MsgDirectoryPartCreationCallback : Resources.MsgDirectoryCreationCallback;
      string msg = $"\"{outDir}\"\r\n{r}";
      return Callbacks.Interact (msg, ECallbackType.question3);
    }

    private void exceptionCallback (Exception exc) {
      string message = exc.ToShortString(true);
      Log (3, this, () => message);
      Callbacks.Interact (message, ECallbackType.error);
    }

    #endregion Private Methods
  }
}
