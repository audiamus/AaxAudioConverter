using System;
using System.IO;
using System.Linq;
using audiamus.aux.ex;

namespace audiamus.aaxconv.lib {
  public class AaxFileItem : IEquatable<AaxFileItem> {
    public const string EXT_JPG = ".jpg";
    public const string EXT_PNG = ".png";


    public string FileName { get; private set; }
    public bool AA { get; private set; }
    public long FileSize { get; private set; }
    public string BookTitle { get; internal set; }
    public string [] Authors { get; internal set; }
    public string Author => Authors.Combine (); 
    public string [] Narrators { get; internal set; }
    public string Narrator => Narrators.Combine ();
    public TimeSpan Duration { get; internal set; }
    public string Abstract { get; internal set; }
    public string Genre { get; internal set; }
    public byte[] Cover { get; internal set; }
    public string CoverExt { get; internal set; }
    public string Publisher { get; internal set; }
    public DateTime? PublishingDate { get; internal set; }
    public string Copyright { get; internal set; }
    public uint Channels { get; internal set; }
    public uint SampleRate { get; internal set; }
    public uint AvgBitRate { get; internal set; }
    public uint NumChapters { get; set; }
    public bool Converted { get; set; }


    public AaxFileItem (string path) {
      FileName = path;
      string ext = Path.GetExtension (path).ToLowerInvariant ();
      AA = ext == ".aa";
      FileSize = new FileInfo (path).Length;

      TagAndFileNamingHelper.ReadMetaData (this);
    }

    public override string ToString () {
      return $"{Author} - {BookTitle}";
    }

    public bool Equals (AaxFileItem other) {
      //Check whether the compared objects reference the same data.
      if (Object.ReferenceEquals (this, other))
        return true;

      //Check whether any of the compared objects is null.
      if (other is null)
        return false;

      //Check whether the items' properties are equal.
      return string.Equals (this.FileName, other.FileName, StringComparison.InvariantCultureIgnoreCase);
    }

    public override int GetHashCode () {
      //Get hash code for the FileName field if it is not null.
      int hashFileName = FileName is null ? 0 : FileName.GetHashCode ();

      //Calculate the hash code for the product.
      return hashFileName;
    }

  }

}
