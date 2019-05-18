using System;
using System.IO;
using System.Linq;

namespace audiamus.aaxconv.lib {
  class ExtraMetaFiles {
    private readonly IResources _resources; 
    private Book Book { get; }
    private TimeSpan Duration { get; }
    private AaxFileItem AaxFileItem { get; }
    private INamingSettings Settings { get; }
    private IResources R => _resources;

    public ExtraMetaFiles (Book book, INamingSettings settings, IResources resources) {
      Book = book;
      var part = Book?.Parts?.FirstOrDefault ();
      if (part is null || !part.AaxFileItem.Converted)
        return;

      AaxFileItem = part.AaxFileItem;
      var duration = book.Parts.SelectMany (p => p.Tracks).Select (t => t.Time.Duration.TotalSeconds).Sum ();
      Duration = TimeSpan.FromSeconds (duration);

      Settings = settings;
      _resources = resources;
    }

    public void WriteFiles (Func<Book, string> nameFunc) {
      if (Book is null || AaxFileItem is null || nameFunc is null)
        return;
      writeImageFile (nameFunc);
      writeTextFile (nameFunc);
    }

    void writeImageFile (Func<Book, string> nameFunc) {
      var afi = AaxFileItem;
      if (afi.Cover is null || string.IsNullOrWhiteSpace (afi.CoverExt))
        return;

      string filename = $"{nameFunc(Book)}{afi.CoverExt}";
      string path = Path.Combine (Book.OutDirectoryLong, filename);
      using (var osm = new FileStream (path, FileMode.Create, FileAccess.Write))
        osm.Write (afi.Cover, 0, afi.Cover.Length);
    }

    void writeTextFile (Func<Book, string> nameFunc) {
      var afi = AaxFileItem;
      string filename = $"{nameFunc(Book)}.txt";
      string path = Path.Combine (Book.OutDirectoryLong, filename);
      using (var osm = new StreamWriter (path, false)) {
        osm.WriteLine ($"{R.HdrAuthor}: {Book.AuthorTag}");
        osm.WriteLine ($"{R.HdrTitle}: {Book.TitleTag}");
        osm.WriteLine ($"{R.HdrDuration}: {Duration.ToString (@"hh\:mm\:ss")}");
        osm.WriteLine ($"{R.HdrNarrator}: {afi.Narrator}");
        osm.WriteLine ($"{R.HdrGenre}: {Book.CustomNames?.GenreTag ?? TagAndFileNamingHelper.GetGenre(Settings, afi)}");
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
        for (int i = 0; i < words.Length; i ++) {
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
    }
  }
}
