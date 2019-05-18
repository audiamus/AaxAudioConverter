namespace audiamus.aaxconv.lib {
  public enum EConvFormat { mp3, m4a}
  public enum EConvMode { single, chapters, splitChapters, splitTime}

  public enum EInteractionCustomCallback { none, noActivationCode }

  //  Track file naming pattern
  //<author> - <book title> - <track>
  //<track> - <book title> - <author>
  //<book title> - <track>
  //<track> - <book title>
  public enum EFileNaming {
    track,
    track_book,
    track_book_author,
    author_book_track,
    book_track,
  }

  //Track title tag naming pattern
  //<author> - <book title> - <track>
  //<track> - <book title> - <author>
  //<book title> - <track>
  //<track> - <book title>
  public enum ETitleNaming {
    track,
    track_book,
    track_book_author,
    book_author,
    author_book,
    author_book_track,
    book_track,
  }

  //Track numbering pattern (for track file and track title)
  //<track>
  //<chapter>.<track>
  public enum ETrackNumbering {
    track,
    chapter_a_track,
    track_b_chapter_c,
  }

  public enum EGeneralNaming {
    source,
    standard,
    custom,
  }

  public enum EFlatFolderNaming {
    author_book,
    book_author,
  }

}
