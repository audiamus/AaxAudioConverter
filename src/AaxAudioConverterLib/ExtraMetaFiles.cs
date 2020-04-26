using System;
using System.IO;
using System.Linq;
using audiamus.aux.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  class ExtraMetaFiles {
    private readonly IResources _resources; 
    private Book Book { get; }
    private TimeSpan Duration { get; set; }
    private AaxFileItem AaxFileItem { get; set; }
    private INamingSettingsEx Settings { get; }
    private IResources R => _resources;

    public ExtraMetaFiles (Book book, INamingSettingsEx settings, IResources resources) {
      Book = book;
      Settings = settings;
      _resources = resources;
    }

    public void WriteFiles (Func<Book, string> nameFunc) {
      if (Book is null || nameFunc is null)
        return;
      Log (3, this, () => nameFunc(Book).SubstitUser());
      if (Book.PartsType != Book.EParts.some) {
        setContext ();
        writeImageFile (nameFunc, Book.OutDirectoryLong);
        writeTextFile (nameFunc, Book.OutDirectoryLong);
      } else {
        foreach (var part in Book.Parts) {
          setContext (part);
          string dir = TagAndFileNamingHelper.GetPartDirectoryName (R, Settings, Book, part);
          writeImageFile (nameFunc, dir);
          writeTextFile (nameFunc, dir);
        }
      }
    }

    private void setContext (Book.BookPart part = null) {
      if (part is null && Book.PartsType != Book.EParts.some)
        part = Book?.Parts?.FirstOrDefault ();
      if (part is null || !part.AaxFileItem.Converted)
        return;

      AaxFileItem = part.AaxFileItem;
      double duration = 0;
      if (Book.PartsType != Book.EParts.some)
        duration = Book.Parts.SelectMany (p => p.Tracks).Select (t => t.Time.Duration.TotalSeconds).Sum ();
      else
        duration = part.Tracks.Select (t => t.Time.Duration.TotalSeconds).Sum ();

      Duration = TimeSpan.FromSeconds (duration);
    }

    private void writeImageFile (Func<Book, string> nameFunc, string dir) {
      if (AaxFileItem is null)
        return;
      var afi = AaxFileItem;
      if (afi.Cover is null || string.IsNullOrWhiteSpace (afi.CoverExt))
        return;

      string filename = $"{nameFunc(Book)}{afi.CoverExt}";
      string path = Path.Combine (dir, filename);
      Log (3, this, () => $"\"{path.SubstitUser()}\"");
      try {
        using (var osm = new FileStream (path, FileMode.Create, FileAccess.Write))
          osm.Write (afi.Cover, 0, afi.Cover.Length);
      } catch (Exception exc) {
        Log (1, this, exc.ToShortString ());
      }
    }

    void writeTextFile (Func<Book, string> nameFunc, string dir) {
      if (AaxFileItem is null)
        return;
      var afi = AaxFileItem;
      string filename = $"{nameFunc (Book)}.txt";
      string path = Path.Combine (dir, filename);
      Log (3, this, () => $"\"{path.SubstitUser ()}\"");
      try {
        using (var osm = new StreamWriter (path, false)) {
          osm.WriteLine ($"{R.HdrAuthor}: {Book.AuthorTag}");
          osm.WriteLine ($"{R.HdrTitle}: {Book.TitleTag}");
          osm.WriteLine ($"{R.HdrDuration}: {Duration.ToStringHMS ()}");
          osm.WriteLine ($"{R.HdrNarrator}: {afi.Narrator}");
          osm.WriteLine ($"{R.HdrGenre}: {Book.CustomNames?.GenreTag ?? TagAndFileNamingHelper.GetGenre (Settings, afi)}");
          osm.WriteLine ($"{R.HdrYear}: {Book.CustomNames?.YearTag?.Year ?? afi.PublishingDate?.Year}");
          osm.WriteLine ($"{R.HdrPublisher}: {afi.Publisher}");
          osm.WriteLine ($"{R.HdrCopyright}: {afi.Copyright}");
          osm.WriteLine ($"{R.HdrSampleRate}: {afi.SampleRate} Hz");
          osm.WriteLine ($"{R.HdrBitRate}: {afi.AvgBitRate} kb/s");
          osm.WriteLine ();

          string[] words = afi.Abstract?.Split ();
          if (words is null)
            return;

          int n = 0;
          for (int i = 0; i < words.Length; i++) {
            string word = words[i];
            if (n + 1 + word.Length > 80) {
              osm.WriteLine ();
              n = 0;
            }

            if (n > 0) {
              osm.Write (' ');
              n++;
            }
            osm.Write (word);
            n += word.Length;
          }
          osm.WriteLine ();
        }
      } catch (Exception exc) {
        Log (1, this, exc.ToShortString ());
      }

    }
  }
}
