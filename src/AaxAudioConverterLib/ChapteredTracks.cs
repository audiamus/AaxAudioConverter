using System.Collections.Generic;

namespace audiamus.aaxconv.lib {

  class ChapteredTracks {
    public class PartChapter {
      public Chapter Chapter { get; private set; }
      public uint ChapterNumber { get; private set; } 
      public List<Track> Tracks { get; } = new List<Track> ();

      public PartChapter (Chapter chapter, uint chapterNumber) {
        Chapter = chapter;
        ChapterNumber = chapterNumber;
      }
    }

    public class PartChapters {

      public Book.BookPart Part { get; private set; }
      public List<PartChapter> Chapters { get; } = new List<PartChapter> ();

      public PartChapters (Book.BookPart part) {
        Part = part;
      }
    }

    public List<PartChapters> Parts { get; } = new List<PartChapters> ();

    public ChapteredTracks (Book book) {
      foreach (var part in book.Parts) {
        var partChapters = new PartChapters (part);
        Parts.Add (partChapters);
        foreach (var track in part.Tracks) {
          var partChapt = partChapters.Chapters.Find (pc => object.ReferenceEquals (track.Chapter, pc.Chapter));
          if (partChapt is null) {
            partChapt = new PartChapter (track.Chapter, book.ChapterNumber(track.Chapter));
            partChapters.Chapters.Add (partChapt);
          }
          partChapt.Tracks.Add (track);
        }
      }
    }
  }
}

