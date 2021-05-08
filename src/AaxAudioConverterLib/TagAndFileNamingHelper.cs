//#define MULTIPART_TEST 
#pragma warning disable CS0642  // "Possible mistaken empty statement"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using audiamus.aaxconv.lib.ex;
using audiamus.aux;
using audiamus.aux.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  partial class TagAndFileNamingHelper {
#if MULTIPART_TEST
    private static int __numpart = 0;
    private static readonly object __lockable = new object ();
#endif

    enum ENamedChapters {
      Number = 0,
      Name = 1,
      NumberName = 2
    }

    public const string EXT_JPG = ".jpg";
    public const string EXT_PNG = ".png";
    public const string EXT_M4A = ".m4a";
    public const string EXT_M4B = ".m4b";
    public const string EXT_MP3 = ".mp3";

    const string SPACE = " ";

    // These are just fallbacks, in case every other source failed. 
    public const string GENRE = "Audiobook";
    public const string CHAPTER = "Chapter";
    public const string PART = "Part";

    // custom tags
    const string NRT = "nrt";
    const string DES = "des";
    const string PUB = "pub";
    const string RLDT = "rldt";
    const string WMRK = "wmrk";

    const string RGX_COPYRIGHT_YEAR = @"\D(\d{4})\D";
    static readonly Regex __rgxCopyrightYear = new Regex (RGX_COPYRIGHT_YEAR, RegexOptions.Compiled);

    readonly string _sepDash = Singleton<ChainPunctuationDash>.Instance.Infix[0];
    readonly string[] _seps = Singleton<ChainPunctuationDot>.Instance.Infix;

    readonly INamingAndModeSettings _settings;
    readonly IResources _resources;
    readonly AaxFileItem _aaxFileItem;
    readonly Book _book;
    readonly Book.Part _part;
    readonly ENamedChapters _namedChapters;

    bool _isFileName;

    Track _track;
    Numbers _numbers;

    //private static readonly object __lockableATL = new object (); 


    private INamingAndModeSettings Settings => _settings;
    private EConvMode ConvMode => Settings.ConvMode;
    private EConvFormat ConvFormat => Settings.ConvFormat;


    private IResources Resources => _resources;

    private string Track => trackEx ();
    private string TrackForFile => Track.Replace('/', ',');
    private string Title => titleNaming ();
    private string Chapter => chapter ();
    private string ChapterName => _track?.Chapter.Name;
    private string Part => part ();
    private string FullPath => fullPath ();
    private string File => file ();
    private static string Wmrk { get; }

    private string ExtMp4 => Settings.M4B ? EXT_M4B : EXT_M4A;

    private string PartPrefix {
      get {
        string prefix;
        switch (Settings.PartNaming) {
          case EGeneralNaming.source:
            prefix = _book.PartNameStub;
            break;
          case EGeneralNaming.custom:
            prefix = Settings.PartName;
            break;
          default:
            prefix = Resources?.PartNamePrefixStandard;
            break;
        }
        return prefix ?? PART;
      }
    }

    private string ChapterPrefix {
      get {
        string prefix;
        switch (Settings.ChapterNaming) {
          case EGeneralNamingEx.source:
            prefix = _book.ChapterNameStub;
            break;
          case EGeneralNamingEx.custom:
            prefix = Settings.ChapterName;
            break;
          default:
            prefix = Resources?.ChapterNamePrefixStandard;
            break;
        }
        return prefix ?? CHAPTER;
      }
    }

    static TagAndFileNamingHelper () =>
      Wmrk = $"Processed by {ApplEnv.ApplName} {ApplEnv.AssemblyVersion}";
 

    private TagAndFileNamingHelper (AaxFileItem aaxFileItem) => _aaxFileItem = aaxFileItem;

    private TagAndFileNamingHelper (IResources resources, INamingAndModeSettings settings, Book book, Track track) :
      this (resources, settings, book) {
      _track = track;

      var part = book.Parts.Where (p => p.Tracks?.Contains (track) ?? false).SingleOrDefault ();
      if (part is null)
        return;

      _part = part;
      _aaxFileItem = part.AaxFileItem;

      _numbers = new Numbers (track, part);

    }

    private TagAndFileNamingHelper (IResources resources, INamingAndModeSettings settings, Book.Part part) :
      this (resources, settings, part.Book) {
      _aaxFileItem = part.AaxFileItem;
      _numbers = new Numbers (null, part);
    }

    private TagAndFileNamingHelper (IResources resources, INamingAndModeSettings settings, Book book) {
      _resources = resources;
      _settings = settings;
      _book = book;
      //if (settings is IConvSettings s) {
      //  _convFormat = s.ConvFormat;
      //  _convMode = s.ConvMode;
      //}

      bool useChapterNames = settings.NamedChapters >= lib.ENamedChapters.yes && book.HasNamedChapters;
      bool useChapterNumbers = useChapterNames && settings.NamedChapters == lib.ENamedChapters.yesAlwaysWithNumbers ||
        !book.HasUniqueChapterNames (ConvMode);
      if (useChapterNames) {
        if (useChapterNumbers)
          _namedChapters = ENamedChapters.NumberName;
        else
          _namedChapters = ENamedChapters.Name;
      }

    }

    public static bool ReadMetaData (AaxFileItem aaxFileItem) =>
      new TagAndFileNamingHelper (aaxFileItem).readMetaData ();

    public static bool WriteMetaData (IResources resources, IConvSettings settings, Book book, Track track, bool withChapters = false) =>
      new TagAndFileNamingHelper (resources, settings, book, track).writeMetaDataAtl (settings, withChapters);

    public static bool WriteMetaDataChapters (IResources resources, INamingAndModeSettings settings, Book book, Track track) =>
      new TagAndFileNamingHelper (resources, settings, book, track).writeMetaDataChapters ();

    public static void SetTrackTitle (IResources resources, INamingAndModeSettings settings, Book book, Track track) =>
      new TagAndFileNamingHelper (resources, settings, book, track).setTrackTitle ();

    public static void SetFileName (IResources resources, INamingAndModeSettings settings, Book book, Track track) =>
      new TagAndFileNamingHelper (resources, settings, book, track).setFileName ();

    public static void SetFileNames (IResources resources, INamingAndModeSettings settings, Book book) =>
      new TagAndFileNamingHelper (resources, settings, book).setFileNames ();

    public static string GetPartDirectoryName (IResources resources, INamingAndModeSettings settings, Book.Part part) =>
      new TagAndFileNamingHelper (resources, settings, part).getPartDirectoryName ();

    public static string GetGenre (INamingSettings settings, AaxFileItem afi) =>
          (settings.GenreNaming == EGeneralNaming.source ? afi.Genre : settings.GenreName) ?? GENRE;

    public static string GetPartPrefix (IResources resources, INamingAndModeSettings settings, Book book) =>
      new TagAndFileNamingHelper (resources, settings, book).PartPrefix;

    public static string GetChapterPrefix (IResources resources, INamingAndModeSettings settings, Book book) {
      return new TagAndFileNamingHelper (resources, settings, book).ChapterPrefix;
    }

    private string getPartDirectoryName () {
      switch (_book.PartsType) {
        case Book.EParts.some:
          return Path.Combine (_book.OutDirectoryLong, Part);
        case Book.EParts.none:
        case Book.EParts.all:
        default:
          return _book.OutDirectoryLong;
      }
    }

    private void setFileName () => _track.FileName = FullPath;

    private void setFileNames () => _book.Parts.SelectMany (p => p.Tracks).ToList ().ForEach (setFileName);

    private void setFileName (Track track) {
      _isFileName = true;
      _track = track;
      var part = _book.Parts.Where (p => p.Tracks?.Contains (track) ?? false).SingleOrDefault ();
      if (part is null)
        return;
      _numbers = new Numbers (track, part);
      setFileName ();
    }

    private string track () {
      var n = _numbers;
      string[] p = _seps;

      switch (ConvMode) {
        default:
          return n.nTrk.Str (n.nnTrk);
        case EConvMode.chapters:
          return chapterNamedTrackForChapters ();
        case EConvMode.splitChapters:
          return chapterNamedTrackForSplitChapters ();
      };
    }

    private string trackEx () {
      string trk = track ();
      if (Settings.TotalTracks)
        return $"{trk}/{_numbers.nTrks}";
      else
        return trk;
    }

    private string chapterNamedTrackForChapters () {
      var n = _numbers;
      string trackName = n.nTrk.Str (n.nnTrk);

      // not dependent on Settings.TrackNumbering

      switch (_namedChapters) {
        default:
        case ENamedChapters.Number:
          return trackName;
        case ENamedChapters.Name:
        case ENamedChapters.NumberName: {
            string chapterName = ChapterName;
            if (_isFileName) {
              chapterName = chapterName.Prune ();
              if (_namedChapters == ENamedChapters.NumberName)
                chapterName = $"({trackName}) {chapterName}";
              else
                ; // for breakpoint               
            } else
              ; // for breakpoint

            return chapterName;
          }
      }
    }

    private string chapterNamedTrackForSplitChapters () {
      var n = _numbers;
      string trackName = n.nTrk.Str (n.nnTrk);

      if (n.nChp == 0)
        return trackName;

      string chapterName = ChapterName;
      if (_isFileName)
        chapterName = chapterName.Prune ();

      switch (Settings.TrackNumbering) {
        default:
        case ETrackNumbering.track:
          return trackName;

        case ETrackNumbering.chapter_a_track:
          string tn;
          if (_namedChapters != ENamedChapters.Number)
            tn = chapterName;
          else
            tn = n.nChp.Str (n.nnChp);

          if (n.nChTrks > 1)
            tn += _seps[0] + n.nChTrk.Str (n.nnChTrk);
          else
            ; // for breakpoint
          return tn;

        case ETrackNumbering.track_b_chapter_c:
          if (_namedChapters != ENamedChapters.Number)
            return trackName + _seps[1] + chapterName + _seps[2];
          else
            return trackName + _seps[1] + n.nChp.Str (n.nnChp) + _seps[2];
      }
    }


    private string titleNaming () {
      var n = _numbers;
      bool isSingleOutputFile = ConvMode == EConvMode.single && n.nTrks == 1;

      string p = _sepDash;
      switch (Settings.TitleNaming) {
        case ETitleNaming.track:
        default:
          return Track;
        case ETitleNaming.track_book:
          if (isSingleOutputFile)
            return _book.TitleTag;
          else
            return Track + p + _book.TitleTag;
        case ETitleNaming.track_book_author:
          if (isSingleOutputFile)
            return _book.TitleTag + p + _book.AuthorTag;
          else
            return Track + p + _book.TitleTag + p + _book.AuthorTag;
        case ETitleNaming.book_author:
          return _book.TitleTag + p + _book.AuthorTag;
        case ETitleNaming.author_book:
          return _book.AuthorTag + p + _book.TitleTag;
        case ETitleNaming.author_book_track:
          if (isSingleOutputFile)
            return _book.AuthorTag + p + _book.TitleTag;
          else
            return _book.AuthorTag + p + _book.TitleTag + p + Track;
        case ETitleNaming.book_track:
          if (isSingleOutputFile)
            return _book.TitleTag;
          else
            return _book.TitleTag + p + Track;
      }
    }

    private string chapter () {
      var n = _numbers;
      var p = SPACE;
      string chapter = ChapterPrefix;
      switch (_namedChapters) {
        default:
        case ENamedChapters.Number:
          return chapter + p + n.nChp.Str (n.nnChp);
        case ENamedChapters.Name:
        case ENamedChapters.NumberName: {
            string chapterName = ChapterName.Prune ();
            bool isNumeric = uint.TryParse (chapterName, out var _);
            if (isNumeric)
              chapterName = chapter + p + chapterName;
            else
              ;   // for breakpoint     

            if (_namedChapters == ENamedChapters.Name)
              return chapterName;
            else
              return $"({n.nChp.Str (n.nnChp)}) {chapterName}";
          }
      }

    }

    private string part () {
      var n = _numbers;
      var p = SPACE;
      string part = PartPrefix;
      return part + p + n.nPrt.Str (n.nnPrt);
    }



    private string fullPath () {
      string ext = ConvFormat == EConvFormat.mp4 ? ExtMp4 : EXT_MP3;
      string filename = File + ext;
      switch (ConvMode) {
        case EConvMode.single:
          if (Settings.ExtraMetaFiles && _book.PartsType == Book.EParts.some)
            return Path.Combine (_book.OutDirectoryLong, Part, filename);
          else
            return Path.Combine (_book.OutDirectoryLong, filename);
        case EConvMode.chapters:
        case EConvMode.splitTime:
          switch (_book.PartsType) {
            case Book.EParts.some:
              return Path.Combine (_book.OutDirectoryLong, Part, filename);
            case Book.EParts.none:
            case Book.EParts.all:
            default:
              return Path.Combine (_book.OutDirectoryLong, filename);
          }
        case EConvMode.splitChapters:
        default:
          switch (_book.PartsType) {
            case Book.EParts.some:
              if (Settings.ChapterNaming != EGeneralNamingEx._nofolders)
                return Path.Combine (_book.OutDirectoryLong, Part, Chapter, filename);
              else
                return Path.Combine (_book.OutDirectoryLong, Part, filename);
            case Book.EParts.none:
            case Book.EParts.all:
            default:
              if (Settings.ChapterNaming != EGeneralNamingEx._nofolders)
                return Path.Combine (_book.OutDirectoryLong, Chapter, filename);
              else
                return Path.Combine (_book.OutDirectoryLong, filename);
          }
      }
    }

    private string file () {
      switch (ConvMode) {
        case EConvMode.single:
          return fileSingle ();
        case EConvMode.chapters:
        case EConvMode.splitChapters:
        default:
          return fileChapters ();
      }
    }

    private string fileChapters () {
      string p = _sepDash;
      switch (Settings.FileNaming) {
        case EFileNaming.track:
        default:
          return TrackForFile;
        case EFileNaming.track_book:
          return TrackForFile + p + _book.TitleFile;
        case EFileNaming.track_book_author:
          return TrackForFile + p + _book.TitleFile + p + _book.AuthorFile;
        case EFileNaming.author_book_track:
          return _book.AuthorFile + p + _book.TitleFile + p + TrackForFile;
        case EFileNaming.book_track:
          return _book.TitleFile + p + TrackForFile;
      }
    }

    private string fileSingle () {
      switch (_book.PartsType) {
        case Book.EParts.none:
        default:
          return fileSingleNone ();
        case Book.EParts.some:
          return fileSingleSome ();
        case Book.EParts.all:
          return fileChapters ();
      }
    }

    private string fileSingleSome () {
      var n = _numbers;
      string p = _sepDash;
      string prt = n.nPrt.Str (n.nnPrt);
      switch (Settings.FileNaming) {
        case EFileNaming.track:
        default:
          return prt;
        case EFileNaming.track_book:
          return prt + p + _book.TitleFile;
        case EFileNaming.book_track:
          return _book.TitleFile + p + prt;
        case EFileNaming.track_book_author:
          return prt + p + _book.TitleFile + p + _book.AuthorFile;
        case EFileNaming.author_book_track:
          return _book.AuthorFile + p + _book.TitleFile + p + prt;
      }
    }

    private string fileSingleNone () {
      string p = _sepDash;

      switch (Settings.FileNaming) {
        case EFileNaming.track:
        case EFileNaming.track_book:
        case EFileNaming.book_track:
        default:
          return _book.TitleFile;
        case EFileNaming.track_book_author:
          return _book.TitleFile + p + _book.AuthorFile;
        case EFileNaming.author_book_track:
          return _book.AuthorFile + p + _book.TitleFile;
      }
    }

    private bool readMetaData () {
      Log0 (3, this);
      if (_aaxFileItem is null)
        return false;
      bool succ = readMetadataFFmpeg ();
      succ = succ && readMetadataAtl ();

      return succ;
    }

    private bool readMetadataAtl () {
      Log0 (3, this);

      var afi = _aaxFileItem;
      if (afi.HasExtAA)
        return readMetadataAtlAa ();
      else
        return readMetadataAtlAax ();
    }

    private bool readMetadataAtlAa () {
      Log0 (3, this);
      try {

        var afi = _aaxFileItem;
        ATL.Track tags = new ATL.Track (afi.FileName);

        afi.BookTitle = tags.Title.Decode ();
        afi.Authors = tags.Composer.Decode ().SplitTrim ();
        afi.Narrators = tags.Artist.Decode ().SplitTrim ();
        afi.Abstract = tags.Description.Decode ();

        getCoverImage (afi, tags);

        afi.Publisher = tags.Publisher.Decode ();
        afi.Copyright = tags.Copyright.Decode ();
        afi.Genre = tags.Genre?.Decode ().SplitTrim().FirstOrDefault();

        afi.PublishingDate = tags.PublishingDate;

      } catch (Exception exc) {
        Log (1, this, exc.ToShortString());
        return false;
      }

      return true;
    }

    private bool readMetadataAtlAax () {
      Log0 (3, this);

      try {

        var afi = _aaxFileItem;
        ATL.Track tags = new ATL.Track (afi.FileName);

        afi.BookTitle = tags.Title.Decode ();
        afi.Authors = tags.AlbumArtist.Decode ().SplitTrim ();

        getCoverImage (afi, tags);

        afi.Copyright = tags.Copyright.Decode ();
        try {
          afi.PublishingDate = new DateTime ((int)tags.Year, 1, 1);
        } catch (Exception) { }
        if (!afi.PublishingDate.HasValue || afi.PublishingDate.Value.Year > DateTime.Now.Year + 2) {
          DateTime date = default;
          bool succ = false;
          string rldt = atlCustomTag (tags, RLDT);
          if (!rldt.IsNullOrWhiteSpace ())
            succ = DateTime.TryParse (rldt, out date);
          if (succ)
            afi.PublishingDate = date;

          if (!afi.PublishingDate.HasValue || afi.PublishingDate.Value.Year > DateTime.Now.Year + 2 && !afi.Copyright.IsNullOrWhiteSpace ()) {
            var match = __rgxCopyrightYear.Match (afi.Copyright);
            if (match.Success) {
              succ = int.TryParse (match.Groups[1].Value, out int year);
              if (succ) {
                try {
                  afi.PublishingDate = new DateTime (year, 1, 1);
                } catch (Exception) { }
              }
            }
          }
        }

        afi.Genre = tags.Genre?.Decode ().SplitTrim ().FirstOrDefault ();

        afi.Narrators = atlCustomTag (tags, NRT).Decode ().SplitTrim ();
        afi.Abstract = atlCustomTag (tags, DES).Decode ();
        afi.Publisher = atlCustomTag (tags, PUB).Decode ();

#if MULTIPART_TEST
        {
          int pn;
          lock (__lockable) {
            pn = ++__numpart;
          }
          afi.BookTitle = $"My long book title - latest version, released, complete: Book Part {pn}: my book";
          afi.Authors = new string[] { "John Doe" };
          afi.Narrators = new string[] { "Jane Doe" };
        }
#endif


      } catch (Exception exc) {
        Log (1, this, exc.ToShortString());
        return false;
      }


      return true;
    }

    private static void getCoverImage (AaxFileItem afi, ATL.Track tags) {
      var pic = tags.EmbeddedPictures.FirstOrDefault ();
      if (pic != null) {
        afi.Cover = pic.PictureData;
        switch (pic.NativeFormat) {
          case Commons.ImageFormat.Jpeg:
            afi.CoverExt = EXT_JPG;
            break;
          case Commons.ImageFormat.Png:
            afi.CoverExt = EXT_PNG;
            break;
        }
      }
    }

    private string atlCustomTag (ATL.Track tags, string key) {
      if (key.Length == 3)
        key = "©" + key;
      bool succ = tags.AdditionalFields.TryGetValue (key, out string value);
      if (succ)
        return value;
      else
        return null;
    }


    private bool readMetadataFFmpeg () {
      var afi = _aaxFileItem;
      Log (3, this, $"\"{afi.FileName.SubstitUser()}\"");

      FFmpeg ffmpeg = new FFmpeg (afi.FileName);
      bool succ = ffmpeg.GetAudioMeta ();
      if (succ) {
        afi.Duration = ffmpeg.AudioMeta.Time.Duration;
        afi.SampleRate = ffmpeg.AudioMeta.Samplerate;
        afi.Channels = ffmpeg.AudioMeta.Channels;
        afi.AvgBitRate = ffmpeg.AudioMeta.AvgBitRate;
        afi.IsAA = ffmpeg.IsAaFile;
      }
      return succ;
    }

    private void setTrackTitle () => _track.Title = Title;


    private bool writeMetaDataAtl (IRoleTagAssigmentSettings settings, bool withChapters) {
      _isFileName = false;
      _track.Title = Title; // save for playlist

      if (_aaxFileItem is null)
        return false;
      var afi = _aaxFileItem;

      IEnumerable<Chapter> chapters = null;
      if (withChapters)
        chapters = getChapters ();

      //lock (__lockableATL) 
      {
        ATL.Track tags;
        try {

          tags = new ATL.Track (_track.FileName);

          tagSubFormat (tags);

          // album
          tags.Album = _book.TitleTag;

          // performers
          tags.Artist = buildPerformerTag (settings.TagArtist, _book.AuthorTag, afi.Narrators);
          tags.AlbumArtist = buildPerformerTag (settings.TagAlbumArtist, _book.AuthorTag, afi.Narrators);
          tags.Composer = buildPerformerTag (settings.TagComposer, _book.AuthorTag, afi.Narrators);
          tags.Conductor = buildPerformerTag (settings.TagConductor, _book.AuthorTag, afi.Narrators);

          // comment
          tags.Comment = afi.Abstract;

          // year
          tags.Year = (int)((_book.CustomNames?.YearTag ?? afi.PublishingDate)?.Year ?? 0);

          // genre
          string genre = _book.CustomNames?.GenreTag ?? getGenre (afi);
          tags.Genre = genre;

          // copyright
          tags.Copyright = afi.Copyright;

          // publisher
          tags.Publisher = afi.Publisher;

          // disc
          tags.DiscNumber = _numbers.nDsk;

          // disc count
          tags.DiscTotal = _numbers.nDsks;

          // title 
          tags.Title = Title;

          // track
          tags.TrackNumber = _numbers.nTrk;

          // track count
          tags.TrackTotal = _numbers.nTrks;

          // cover picture
          var pi = ATL.PictureInfo.fromBinaryData (afi.Cover, ATL.PictureInfo.PIC_TYPE.Front);
          tags.EmbeddedPictures.Add (pi);

          if (withChapters)
            writeChapters (tags, chapters);

          // HACK test only
          //tags.Album = "My Album ÄÖÜäöüß";
          //tags.Artist = "My Artist ÄÖÜäöüß";
          //tags.AlbumArtist = "My AlbumArtist ÄÖÜäöüß";
          //tags.Composer = "My Composer ÄÖÜäöüß";
          //tags.Conductor = "My Conductor ÄÖÜäöüß";
          //tags.Genre = "My Genre ÄÖÜäöüß";
          //tags.Copyright = "My Copyright ÄÖÜäöüß";
          //tags.Publisher = "My Publisher ÄÖÜäöüß";
          //tags.Title = "My Title ÄÖÜäöüß";

          tags.AdditionalFields[WMRK] = Wmrk;

          // HACK test only
          tags.Save ();

        } catch (Exception exc) {
          Log (1, this, exc.ToShortString ());
          return false;
        }
      }
      return true;

    }

    private void tagSubFormat (ATL.Track tags) {
      var metafmt = tags.MetadataFormats.Where (f => string.Equals (f.ShortName, "ID3v2", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault ();

      if (!metafmt.IsNull () && ApplEnv.OSVersion.Major < 10) {
        ATL.Settings.ID3v2_tagSubVersion = 3;
        Log (3, this, $"track=\"{Title}\" \"{Path.GetFileName (_track.FileName)}\": {metafmt.Name} downgraded to version {ATL.Settings.ID3v2_tagSubVersion}");
      } else
        ATL.Settings.ID3v2_tagSubVersion = 4;
    }

    private string buildPerformerTag (ERoleTagAssignment tagArtist, string authorTag, string[] narrators) {
      switch (tagArtist) {
        default:
          return string.Empty;
        case ERoleTagAssignment.author:
          return authorTag;
        //case ERoleTagAssignment.author__narrator__:
        //  if (narrator)
        //    return $"{authorTag}{ExString.SEPARATOR} {narrators.Combine()}";
        //  else
        //    return authorTag;
        case ERoleTagAssignment.author_narrator:
          return $"{authorTag}{ExString.SEPARATOR} {narrators.Combine()}";
        //case ERoleTagAssignment.__narrator__:
        //  if (narrator)
        //    return narrators.Combine ();
        //  else
        //    return string.Empty;
        case ERoleTagAssignment.narrator:
          return narrators.Combine ();
      }

    }

    private bool writeMetaDataChapters () {
      var chapters = getChapters ();

      if (chapters.IsNullOrEmpty ())
        return false;

      //lock (__lockableATL) 
      {
        ATL.Track tagFile;
        try {

          tagFile = new ATL.Track (_track.FileName);

          tagSubFormat (tagFile);
          
          writeChapters (tagFile, chapters);

          tagFile.Save ();

        } catch (Exception exc) {
          Log (1, this, exc.ToShortString ());
          return false;
        }
      }
      return true;

    }

    private IEnumerable<Chapter> getChapters () {
      if (_part is null)
        return null;

      IEnumerable<Chapter> chapters = null;
      switch (ConvMode) {
        case EConvMode.single:
          chapters = _part.Chapters2;
          break;
        default:
          chapters = _track.MetaChapters;
          break;
      }

      return chapters;
    }

    private void writeChapters (ATL.Track tagFile, IEnumerable<Chapter> chapters) {
      if (chapters.IsNullOrEmpty ())
        return;

      Log (3, this, $"\"{Path.GetFileName (_track.FileName.SubstitUser ())}\": #chapters={chapters.Count ()}");

      //ATL.Settings.ID3v2_tagSubVersion = 3;
      ATL.Settings.MP4_createNeroChapters = true;
      ATL.Settings.MP4_createQuicktimeChapters = true;
      ATL.Settings.FileBufferSize = 2_000_000;
      //ATL.Settings.ForceDiskIO = true;

      tagFile.Chapters.Clear ();


      foreach (var ch in chapters) {
        Log (3, this, ch.ToString ());
        var chi = new ATL.ChapterInfo {
          StartTime = (uint)ch.Time.Begin.TotalMilliseconds,
          EndTime = (uint)ch.Time.End.TotalMilliseconds,
          Title = ch.Name
        };
        tagFile.Chapters.Add (chi);
      }
    }

    private string getGenre (AaxFileItem afi) => GetGenre (Settings, afi);
    
  }

}

namespace audiamus.aaxconv.lib.ex {
  static class Int32Ex {
    public static string Str (this int n, int nn) {
      return n.ToString ($"D{nn}");
    } 
  } 
}

