using System.Linq;
using audiamus.aux.ex;

namespace audiamus.aaxconv.lib {
  partial class TagAndFileNamingHelper {
    class Numbers {
      public readonly int nTrk = -1;      // current track (in this part/all parts)
      public readonly int nTrks = -1;     // total tracks (in this part/all parts)
      public readonly int nnTrk = -1;     // track digits (in this part/all parts)

      public readonly int nChTrk = -1;    // current track in chapter
      public readonly int nnChTrk = -1;   // chapter track digits
      public readonly int nChTrks = -1;   // chapter track digits

      public readonly int nDsk = -1;      // current disk
      public readonly int nDsks = -1;     // total disks

      public readonly int nChp = -1;      // current chapter (in this part/all parts)
      public readonly int nnChp = -1;     // chapter digits (in this part/all parts)

      public readonly int nPrt = -1;      // current part
      public readonly int nnPrt = -1;     // part digits

      public Numbers (Book book, Track track, Book.BookPart part) {
        this.nPrt = part.PartNumber;
        this.nnPrt = book.Parts.Select (p => p.PartNumber).Max ().Digits ();

        if (!(track is null)) { 
          var chapterTracks = part.Tracks?.Where (t => !(track.Chapter is null) && object.ReferenceEquals (t.Chapter, track.Chapter)).ToList ();
          this.nChTrk = (chapterTracks?.IndexOf (track) ?? -1) + 1;
          this.nChTrks = chapterTracks?.Count ?? 0;
          this.nnChTrk = this.nChTrks.Digits ();
        }

        if (book.PartsType == Book.EParts.some) {
          this.nTrk = (part.Tracks?.IndexOf (track) ?? -1) + 1;
          this.nTrks = part.Tracks?.Count ?? 0;

          this.nDsk = part.PartNumber;
          this.nDsks = 0;

          this.nChp = (part.Chapters?.IndexOf (track?.Chapter) ?? -1) + 1;
          this.nnChp = part.Chapters?.Count.Digits () ?? 0;

        } else {

          this.nTrk = book.Parts.Select (pt =>
          {
            if (pt != part) {
              if (pt.PartNumber < part.PartNumber)
                return pt.Tracks?.Count ?? 0;
              else
                return 0;
            } else
              return (part.Tracks?.IndexOf (track) ?? -1) + 1;
          }).Sum ();

          this.nTrks = book.Parts.Select (p => p.Tracks?.Count () ?? 0).Sum ();

          this.nDsks = book.Parts.Count;


          this.nChp = book.Parts.Select (pt =>
          {
            if (pt != part) {
              if (pt.PartNumber < part.PartNumber)
                return pt.Chapters?.Count ?? 0;
              else
                return 0;
            } else {
              if (track?.Chapter is null)
                return 0;
              else
                return (pt.Chapters?.IndexOf (track?.Chapter) ?? -1) + 1;
            }
          }).Sum ();

          this.nnChp = book.Parts.Select (p => p.Chapters?.Count ?? 0).Sum ().Digits ();
          this.nDsk = book.PartsType == Book.EParts.none ? 1 : part.PartNumber;
        }

        this.nnTrk = this.nTrks.Digits ();

      }

    }
  }
}

