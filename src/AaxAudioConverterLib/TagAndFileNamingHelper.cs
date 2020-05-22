#pragma warning disable CS0642  

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using audiamus.aaxconv.lib.ex;
using audiamus.aux;
using audiamus.aux.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  partial class TagAndFileNamingHelper {
    
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

    // apple
    const string NRT = "nrt"; 
    const string DES = "des"; 
    const string PUB = "pub";

    // audible
    const string PUBDATE = "pubdate"; 
    const string LONG_DESC = "long_description"; 

    readonly string _sepDash = Singleton<ChainPunctuationDash>.Instance.Infix[0];
    readonly string[] _seps = Singleton<ChainPunctuationDot>.Instance.Infix;

    readonly INamingSettingsEx _settings;
    readonly IResources _resources;
    readonly AaxFileItem _aaxFileItem;
    readonly Book _book;
    readonly EConvMode _convMode;
    readonly EConvFormat _convFormat;
    readonly ENamedChapters _namedChapters;

    bool _isFileName;

    Track _track;
    Numbers _numbers;

    private INamingSettingsEx Settings => _settings;
    private IResources Resources => _resources;

    private string Track => track ();
    private string Title => titleNaming ();
    private string Chapter => chapter ();
    private string ChapterName => _track?.Chapter.Name;
    private string Part => part ();
    private string FullPath => fullPath ();
    private string File => file ();

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
          case EGeneralNaming.source:
            prefix = _book.ChapterNameStub;
            break;
          case EGeneralNaming.custom:
            prefix = Settings.ChapterName;
            break;
          default:
            prefix = Resources?.ChapterNamePrefixStandard;
            break;
        }
        return prefix ?? CHAPTER;
      }
    }

    private TagAndFileNamingHelper (AaxFileItem aaxFileItem) => _aaxFileItem = aaxFileItem;

    private TagAndFileNamingHelper (IResources resources, INamingSettingsEx settings, Book book, Track track) :
      this (resources, settings, book)
    {
      _track = track;

      var part = book.Parts.Where (p => p.Tracks?.Contains (track) ?? false).SingleOrDefault ();
      if (part is null)
        return;

      _aaxFileItem = part.AaxFileItem;

      _numbers = new Numbers (book, track, part);

    }

    private TagAndFileNamingHelper (IResources resources, INamingSettingsEx settings, Book book, Book.BookPart part) :
      this (resources, settings, book)
    {
      _aaxFileItem = part.AaxFileItem;
      _numbers = new Numbers (book, null, part);
    }

    private TagAndFileNamingHelper (IResources resources, INamingSettingsEx settings, Book book) {
      _resources = resources;
      _settings = settings;
      _book = book;
      if (settings is IConvSettings s) {
        _convFormat = s.ConvFormat;
        _convMode = s.ConvMode;
      }

      bool useChapterNames = settings.NamedChaptersAndAlwaysWithNumbers.HasValue && book.HasNamedChapters;
      bool useChapterNumbers = useChapterNames && settings.NamedChaptersAndAlwaysWithNumbers.Value || !book.HasUniqueChapterNames (_convMode);
      if (useChapterNames) {
        if (useChapterNumbers)
          _namedChapters = ENamedChapters.NumberName;
        else
          _namedChapters = ENamedChapters.Name;
      }

    }

    public static bool ReadMetaData (AaxFileItem aaxFileItem) => 
      new TagAndFileNamingHelper (aaxFileItem).readMetaData ();

    public static bool WriteMetaData (IResources resources, INamingSettingsEx settings, Book book, Track track) => 
      new TagAndFileNamingHelper (resources, settings, book, track).writeMetaData ();

    public static void SetFileName (IResources resources, IConvSettings settings, Book book, Track track) => 
      new TagAndFileNamingHelper (resources, settings, book, track).setFileName ();

    public static void SetFileNames (IResources resources, IConvSettings settings, Book book) => 
      new TagAndFileNamingHelper (resources, settings, book).setFileNames ();

    public static string GetPartDirectoryName (IResources resources, INamingSettingsEx settings, Book book, Book.BookPart part) =>
      new TagAndFileNamingHelper (resources, settings, book, part).getPartDirectoryName();
    
    public static string GetGenre (INamingSettings settings, AaxFileItem afi) =>
          (settings.GenreNaming == EGeneralNaming.source ? afi.Genre : settings.GenreName) ?? GENRE;

    public static string GetPartPrefix (IResources resources, INamingSettingsEx settings, Book book) =>
      new TagAndFileNamingHelper (resources, settings, book).PartPrefix;

    public static string GetChapterPrefix (IResources resources, INamingSettingsEx settings, Book book) {
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
      _numbers = new Numbers (_book, track, part);
      setFileName ();
    }

    private string track () {
      var n = _numbers;
      string[] p = _seps;

      switch (_convMode) {
        default:
          return n.nTrk.Str (n.nnTrk);
        case EConvMode.chapters:
          return chapterNamedTrackForChapters();
        case EConvMode.splitChapters:
          return chapterNamedTrackForSplitChapters();
      };
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
        case ENamedChapters.NumberName: 
          {
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
      bool isSingleOutputFile = _convMode == EConvMode.single && n.nTrks == 1;

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
        case ENamedChapters.NumberName:
          {
            string chapterName = ChapterName.Prune();
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
      string ext = _convFormat == EConvFormat.mp4 ? ExtMp4 : EXT_MP3;
      string filename = File + ext;
      switch (_convMode) {
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
              return Path.Combine (_book.OutDirectoryLong, Part, Chapter, filename);
            case Book.EParts.none:
            case Book.EParts.all:
            default:
              return Path.Combine (_book.OutDirectoryLong, Chapter, filename);
          }
      }
    }

    private string file () {
      switch (_convMode) {
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
          return Track;
        case EFileNaming.track_book:
          return Track + p + _book.TitleFile;
        case EFileNaming.track_book_author:
          return Track + p + _book.TitleFile + p + _book.AuthorFile;
        case EFileNaming.author_book_track:
          return _book.AuthorFile + p + _book.TitleFile + p + Track;
        case EFileNaming.book_track:
          return _book.TitleFile + p + Track;
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
      if (_aaxFileItem is null)
        return false;
      bool succ = readMetadataTaglib ();
      succ = succ && readMetadataFFmpeg ();

      return succ;
    }

    private bool readMetadataTaglib () {
      var afi = _aaxFileItem;
      if (afi.AA)
        return readMetadataTaglibAudible ();
      else
        return readMetadataTaglibM4a ();
    }

    private bool readMetadataTaglibM4a () {
      var afi = _aaxFileItem;
      TagLib.File file = null;
      try {
        file = TagLib.File.Create (afi.FileName, "taglib/m4a", TagLib.ReadStyle.Average);
      } catch (Exception) {
        return readMetadataTaglibAudible ();
      }

      using (file) {
        var tags = file.Tag;

        afi.BookTitle = tags.Title.Decode ();
        afi.Authors = tags.AlbumArtists.Decode ();
        var pic = tags.Pictures.FirstOrDefault ();
        if (pic != null) {
          afi.Cover = pic.Data?.Data;
          if (afi.Cover != null && afi.Cover.Length > AaPictureExtractor.PNG_HDR.Count) {
            if (pic.Filename != null)
              afi.CoverExt = Path.GetExtension (pic.Filename).ToLower ();
            else
              imageTypeFromData ();
          }

        }
        try {
          afi.PublishingDate = new DateTime ((int)tags.Year, 1, 1);
        } catch (Exception) { }
        afi.Copyright = tags.Copyright.Decode ();
        afi.Genre = tags.Genres.FirstOrDefault ()?.Decode();

        var atags = file.GetTag (TagLib.TagTypes.Apple) as TagLib.Mpeg4.AppleTag;
        if (atags is null)
          return true;
        afi.Narrators = appleCustomTag (atags, NRT).Decode ().SplitTrim();
        afi.Abstract = appleCustomTag (atags, DES).Decode ();
        afi.Publisher = appleCustomTag (atags, PUB).Decode ();
      }

      return true;
    }

    private bool readMetadataTaglibAudible () {
      var afi = _aaxFileItem;
      TagLib.File file = null;
      try {
        file = TagLib.File.Create (afi.FileName, "audio/x-audible",TagLib.ReadStyle.Average);
      } catch (Exception) {
        return false;
      }
      using (file) {
        var tags = file.Tag as TagLib.Audible.Tag;
        if (tags is null)
          return false;

        afi.BookTitle = tags.Title.Decode ();
        afi.Authors = tags.Author.Decode ().SplitTrim();
        afi.Narrators = tags.Narrator.Decode ().SplitTrim();
        afi.Abstract = tags.Description.Decode ();
        var pic = tags.Pictures.FirstOrDefault ();
        if (pic != null) {
          afi.Cover = pic.Data?.Data;
          if (pic.Filename != null)
            afi.CoverExt = Path.GetExtension (pic.Filename);
        }
        afi.Publisher = tags.Album.Decode ();
        afi.Copyright = tags.Copyright.Decode ();
        afi.Genre = tags.Genres.FirstOrDefault ()?.Decode ();

        if (pic is null) {
          afi.Cover = AaPictureExtractor.Extract (afi.FileName);
          imageTypeFromData ();
        }

        string date = audibleCustomTag (tags, PUBDATE);
        if (!(date is null)) {
          bool succ = DateTime.TryParse (date, out var datetime);
          if (succ)
            afi.PublishingDate = datetime;          
        }

        string desc = audibleCustomTag (tags, LONG_DESC);
        if (desc?.Length > afi.Abstract?.Length)
            afi.Abstract = desc;          

        
      }
      return true;
    }

    private bool readMetadataFFmpeg () {
      var afi = _aaxFileItem;

      FFmpeg ffmpeg = new FFmpeg (afi.FileName);
      bool succ = ffmpeg.GetAudioMeta ();
      if (succ) {
        afi.Duration = ffmpeg.AudioMeta.Time.Duration;
        afi.SampleRate = ffmpeg.AudioMeta.Samplerate;
        afi.Channels = ffmpeg.AudioMeta.Channels;
        afi.AvgBitRate = ffmpeg.AudioMeta.AvgBitRate;
      }
      return succ;
    }

    private void imageTypeFromData () {
      var afi = _aaxFileItem;
      var hdr = new ArraySegment<byte> (afi.Cover, 0, AaPictureExtractor.JPG_HDR.Count);
      if (AaPictureExtractor.JPG_HDR.SequenceEqual (hdr))
        afi.CoverExt = EXT_JPG;
      else {
        hdr = new ArraySegment<byte> (afi.Cover, 0, AaPictureExtractor.PNG_HDR.Count);
        if (AaPictureExtractor.PNG_HDR.SequenceEqual (hdr))
          afi.CoverExt = EXT_PNG;
      }
    }

    private bool writeMetaData () {
      _isFileName = false;
      _track.Title = Title; // save for playlist

      if (_aaxFileItem is null)
        return false;
      var afi = _aaxFileItem;
      TagLib.File tagfile = null;
      try {
        tagfile = TagLib.File.Create (_track.FileName);
      } catch (Exception exc) {
        Log (1, this, $"{nameof (TagLib.File.Create)}, {exc.ToShortString ()}");
        return false;
      }

      using (tagfile) {
        var tags = tagfile.Tag;

        if (tags.TagTypes.HasFlag (TagLib.TagTypes.Id3v2) && ApplEnv.OSVersion.Major < 10) {
          if (tags is TagLib.NonContainer.Tag nct) {
            var t = nct.Tags.Where (k => k.TagTypes.HasFlag (TagLib.TagTypes.Id3v2)).FirstOrDefault ();
            if (t is TagLib.Id3v2.Tag id3v2) {
              id3v2.Version = 3;
              Log (3, this, $"track=\"{Title}\" \"{Path.GetFileName(_track.FileName)}\": {nameof (TagLib.Id3v2)} downgraded to version {id3v2.Version}");
            }
          }
        }

        // album
        tags.Album = _book.TitleTag;

        // album artist  
        tags.AlbumArtists = _book.AuthorTag.SplitTrim ();

        // performer/narrator
        if (Settings.Narrator) {
          var authors = new List<string> ();
          authors.AddRange (tags.AlbumArtists);
          authors.AddRange (afi.Narrators);
          tags.Performers = authors.ToArray ();

          var roles = new List<string> ();
          foreach (var _ in tags.AlbumArtists)
            roles.Add (nameof (afi.Author));
          foreach (var _ in afi.Narrators)
            roles.Add (nameof (afi.Narrator));

          tags.PerformersRole = roles.ToArray ();
        } else {
          tags.Performers = tags.AlbumArtists;
          var roles = new string[tags.AlbumArtists.Length];
          for (int i = 0; i < roles.Length; i++)
            roles[i] = nameof (afi.Author);
          tags.PerformersRole = roles;
        }

        // comment
        tags.Comment = afi.Abstract;

        // year
        tags.Year = (uint)((_book.CustomNames?.YearTag ?? afi.PublishingDate)?.Year ?? 0);

        // genre
        string genre = _book.CustomNames?.GenreTag ?? getGenre (afi);
        tags.Genres = new string[] { genre };

        // copyright
        tags.Copyright = afi.Copyright;

        // publisher
        tags.Publisher = afi.Publisher;

        // disc
        tags.Disc = (uint)_numbers.nDsk;

        // disc count
        tags.DiscCount = (uint)_numbers.nDsks;

        // title 
        tags.Title = Title;

        // track
        tags.Track = (uint)_numbers.nTrk;

        // track count
        tags.TrackCount = (uint)_numbers.nTrks;


        // cover picture
        if (!(afi.Cover is null)) {
          var pic = new TagLib.Picture (new TagLib.ByteVector (afi.Cover)) {
            Type = TagLib.PictureType.FrontCover
          };
          tags.Pictures = new TagLib.IPicture[] { pic };
        }

        try {
          tagfile.Save ();
        } catch (Exception exc) {
          Log (1, this, $"{nameof (tagfile.Save)}, {exc.ToShortString ()}");
          return false;
        }

        return true;
      }

    }

    private string getGenre (AaxFileItem afi) => GetGenre (Settings, afi);
    
    //TagLib.Mpeg4.AppleTag
    private static string appleCustomTag (TagLib.Mpeg4.AppleTag atags, string key) {
      var type = atags.GetType ();
      var mi = type.GetMethod ("FixId", BindingFlags.NonPublic | BindingFlags.Static);
      if (mi is null)
        return null;
      var bvKey = new TagLib.ByteVector (key);
      var aBvKey = mi.Invoke (atags, new object[] { bvKey }) as TagLib.ReadOnlyByteVector;
      var tag = atags.GetText (aBvKey);
      if (tag.Length > 1) {
        var sb = new StringBuilder ();
        foreach (string s in tag) {
          if (sb.Length > 0)
            sb.Append ("; ");
          sb.Append (s);
        }
        return sb.ToString ();
      } else
        return tag.FirstOrDefault();
    }

    //TagLib.Audible.Tag
    private static string audibleCustomTag (TagLib.Audible.Tag atags, string key) {
      var type = atags.GetType ();
      var mi = type.GetMethod ("getTag", BindingFlags.NonPublic | BindingFlags.Instance);
      if (mi is null)
        return null;
      var tag = mi.Invoke (atags, new object[] { key }) as string;
      return tag;
    }
  }

}

namespace audiamus.aaxconv.lib.ex {
  static class Int32Ex {
    public static string Str (this int n, int nn) {
      return n.ToString ($"D{nn}");
    } 
  } 
}

