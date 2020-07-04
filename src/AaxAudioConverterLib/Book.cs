using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using audiamus.aux.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  partial class Book {

    public enum EParts { none, some, all }

    public struct Caption {
      public readonly string Author;
      public readonly string Title;

      public Caption (string author, string title) {
        Author = author;
        Title = title;
      }

      public override string ToString () {
        return $"{nameof (Author)}=\"{Author}\", {nameof (Title)}=\"{Title}\"";
      }
    }

    public string SortingTitle { get; private set; }
    public string PartNameStub { get; set; }
    public string ChapterNameStub { get; set; }
    public bool IsNewAuthor { get; set; }
    public string OutDirectoryLong { get; set; }
    public List<Part> Parts { get; } = new List<Part> ();
    public EParts PartsType { get; private set; }

    public string AuthorTag => CustomNames?.AuthorTag ?? TagCaption.Author;
    public string AuthorFile => CustomNames?.AuthorFile ?? FileCaption.Author;
    public string TitleTag => CustomNames?.TitleTag ?? TagCaption.Title;
    public string TitleFile => CustomNames?.TitleFile ?? FileCaption.Title;

    public string DefaultAudioFile { get; internal set; }

    public Stopwatch Stopwatch {get;} = new Stopwatch ();

    internal Caption FileCaption { get; set; }
    internal Caption TagCaption { get; set; }
    internal CustomTagFileNames CustomNames { get; set; }

    public Book (string sortingtitle) => SortingTitle = sortingtitle;

    public Book (AaxFileItem fi) : this (fi.BookTitle) => AddPart (fi);

    public void AddPart (AaxFileItem fi, int part = 0) {
      Parts.Add (new Part (this, fi, part));
      if (CustomNames is null)
        CustomNames = fi.CustomNames;
      CheckParts ();
    }

    public EParts CheckParts () {
      Parts.Sort ((x, y) => x.AaxFileItem.BookTitle.CompareTo (y.AaxFileItem.BookTitle));
      bool hasParts = Parts.Where (p => p.PartNumber > 0).Any ();
      bool allParts = false;
      if (hasParts) {
        int highestPart = Parts.Select (x => x.PartNumber).Max ();
        int partsSum = Parts.Select (x => x.PartNumber).Sum ();
        int expectedSum = highestPart * (highestPart + 1) / 2;
        allParts = partsSum != 1 && partsSum == expectedSum;
      }

      if (hasParts) {
        if (allParts)
          PartsType = EParts.all;
        else
          PartsType = EParts.some;
      } else
        PartsType = EParts.none;

      return PartsType;
    }

    public bool HasUniqueChapterNames (EConvMode convMode) {
      if (convMode == EConvMode.single || convMode == EConvMode.splitTime)
        return false;

      if (!hasNamedChaptersAll ())
        return false;

      var chapterNames = Parts.SelectMany (p => p.NamedChapters).Select (c => c.Name.Prune());
      var chapterNamesDistinct = chapterNames.Distinct();

      bool unique = chapterNames.Count () == chapterNamesDistinct.Count ();
      return unique;
    }

    public bool HasNamedChapters => hasNamedChaptersAll ();

    public uint ChapterNumber (Chapter chapter) {
      if (chapter is null)
        return 0;

      var part = Parts.Where (p => p.Chapters.Contains (chapter)).FirstOrDefault ();
      if (part is null)
        return 0;

      int nChapter = part.Chapters.IndexOf (chapter) + 1;
      if (PartsType == EParts.some)
        return (uint)nChapter;

      int nChapters = Parts.Where (p => p.PartNumber < part.PartNumber).Select (p => p.Chapters.Count).Sum ();
      nChapter += nChapters;
      return (uint)nChapter;
    }

    public (string title, uint? chapters, uint? tracks, uint? part) Counts (EConvMode mode, Part part = null) {
      uint numChapters, numTracks;
      uint? partNum = null;

      // part only matters in "some" mode
      if (PartsType == EParts.some) {
        if (part is null)
          return (null, null, null, null);
        numChapters = (uint)part.Chapters?.Count;
        numTracks = (uint)part.Tracks?.Count;
        partNum = (uint)part.PartNumber;
      } else {
        numChapters = (uint)Parts.Select (p => p.Chapters.Count).Sum ();
        numTracks = (uint)Parts.Select (p => p.Tracks.Count).Sum ();

        if (mode == EConvMode.splitTime && numTracks > 0)
          numChapters = 0;

      }

      uint? chapters = null;
      if (mode != EConvMode.single)
        chapters = numChapters > 0 ? numChapters : (uint?)null;
      uint? tracks = numTracks > 0 ? numTracks : (uint?)null;

      return (TitleTag, chapters, tracks, partNum);
    }


    public void InitAuthorTitle (ITitleSettingsEx settings) {
      if (Parts?.Count == 0)
        return;

      initRegexTitle (settings);

      var fi = Parts[0].AaxFileItem;

      string title = SortingTitle;
      if (settings.LongBookTitle != ELongTitle.no) 
        title = title.Replace (" -:", ":");
      
      Match match = _rgxTitle.Match (title);
      if (match.Success)
        title = match.Groups[1].Value.Trim ();

      if (!settings.AddnlValTitlePunct.Contains ('-')) {
        int idx = title.IndexOf (" -");
        if (idx >= 0)
          title = title.Substring (0, idx);
      }

      title = makeLongBookTitle (title, settings);

      TagCaption = new Caption (fi.Author, title);
      Log (3, this, () => $"{nameof (TagCaption)}: {TagCaption}");
      title = PruneTitle (title);
      FileCaption = new Caption (TagCaption.Author.Prune(), title);
      Log (3, this, () => $"{nameof (FileCaption)}: {FileCaption}");

    }

    public static string PruneTitle (string title) {
      string replaced = title
        .Replace (':', ';')
        .Replace ('/', ';')
        .Replace (" ;", ";");
      return replaced.Prune ();
    }

    public void MergeSilences () {
      Log (3, this, () => $"\"{SortingTitle.Shorten ()}\"");

      foreach (var part in Parts) 
        part.MergeSilences ();
    }

    public void CreateCueSheet (IConvSettings settings) {
      Log (3, this, () => $"\"{SortingTitle.Shorten ()}\"");

      foreach (var part in Parts) 
        part.CreateCueSheet (settings);

    }

    public int DetermineTimeAdjustments () {
      Log (3, this, () => $"\"{this.SortingTitle.Shorten ()}\"");
      int affected = 0;
      foreach (var part in this.Parts) 
        affected += part.DetermineTimeAdjustments (this);
      Log (3, this, () => $"\"{this.SortingTitle.Shorten ()}\", #affected={affected}");
      return affected;
    }

    private string makeLongBookTitle (string title, ITitleSettingsEx settings) {
      if (settings.LongBookTitle == ELongTitle.no || settings.LongBookTitle == ELongTitle.as_is)
        return title;

      if (settings.SeriesTitleLeft && settings.LongBookTitle == ELongTitle.book_series ||
          !settings.SeriesTitleLeft && settings.LongBookTitle == ELongTitle.series_book) {

        int pos = -1;
        if (settings.SeriesTitleLeft)
          pos = title.IndexOf (':');
        else
          pos = title.LastIndexOf (':');
        if (pos > 0) {
          string title1 = title.Substring (0, pos).Trim ();
          string title2 = title.Substring (pos + 1).Trim ();
          title = title2 + ": " + title1;
        }
      }

      return title;
    }


    private bool hasNamedChaptersAll () {
      var partsWithNamedChapters = Parts.Where (p => p.HasNamedChapters);
      return partsWithNamedChapters.Count () == Parts.Count;
    }

    const string RGX_TITLE_1T = @"([\w+\s+";
    const string RGX_TITLE_1 = @"^" + RGX_TITLE_1T;
    const string RGX_TITLE_2 = @".,'/-&";
    const string RGX_TITLE_3 = @"]+)\W*";
    const string RGX_TITLE_1LA = @"^(?:[\w+\s+";
    const string RGX_TITLE_1LB = @"]+:)?" + RGX_TITLE_1T;
    const string ESC_CHARS = @"[\^$.|?*+()";
    const string ESC_CHARS_EX = ESC_CHARS + "-";

    static readonly string RGX_TITLE_1L = RGX_TITLE_1LA + escape (RGX_TITLE_2) + RGX_TITLE_1LB;

    private static readonly Regex __rgxTitle0 = new Regex (RGX_TITLE_1 + escape(RGX_TITLE_2) + RGX_TITLE_3, RegexOptions.Compiled);
    private static readonly Regex __rgxTitleL = new Regex (RGX_TITLE_1L + escape(RGX_TITLE_2) + RGX_TITLE_3, RegexOptions.Compiled);
    private Regex _rgxTitle;
    private static readonly Regex __rgxWord = new Regex (@"([\w\s])", RegexOptions.Compiled);


    private void initRegexTitle (ITitleSettingsEx settings) {

      if (settings is null) {
        _rgxTitle = __rgxTitle0;
        return;
      }

      if (string.IsNullOrWhiteSpace (settings.AddnlValTitlePunct) && settings.LongBookTitle == ELongTitle.no) {
        if (settings.SeriesTitleLeft)
          _rgxTitle = __rgxTitleL;
        else
          _rgxTitle = __rgxTitle0;
        return;
      }

      string rgxTitle1 = RGX_TITLE_1;

      char[] chars = RGX_TITLE_2.ToCharArray ();

      if (!string.IsNullOrWhiteSpace (settings.AddnlValTitlePunct)) {
        var chars1 = settings.AddnlValTitlePunct.ToCharArray ().Distinct ().ToArray ();
        chars = chars.Union (chars1).ToArray ();
      }
      if (settings.LongBookTitle != ELongTitle.no) {
        var chars2 = new[] { ':' };
        chars = chars.Union (chars2).ToArray ();
      } else if (settings.SeriesTitleLeft)
        rgxTitle1 = RGX_TITLE_1L;

      string s = new string (chars);
      while (true) {
        var match = __rgxWord.Match (s);
        if (!match.Success)
          break;
        s = s.Remove (match.Index, 1);
      }

      string addnlValTitlePunct = escape (s);

      _rgxTitle = new Regex ($@"{rgxTitle1}{addnlValTitlePunct}{RGX_TITLE_3}", RegexOptions.Compiled);
    }

    private static string escape (string s) {
      var sb = new StringBuilder ();
      foreach (char c in s) {
        if (ESC_CHARS_EX.IndexOf (c) >= 0)
          sb.Append ('\\');
        sb.Append (c);
      }

      return sb.ToString();
    }
  }
}
