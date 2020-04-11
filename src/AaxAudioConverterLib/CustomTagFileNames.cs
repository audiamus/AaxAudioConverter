using System;
using System.Text;

namespace audiamus.aaxconv.lib {
  public class CustomTagFileNames {
    public string AuthorTag { get; set; } 
    public string AuthorFile { get; set; } 
    public string TitleTag { get; set; } 
    public string TitleFile { get; set; } 
    public DateTime? YearTag { get; set; } 
    public string GenreTag { get; set; }

    public override string ToString () {
      var sb = new StringBuilder ();
      sb.AppendLine ($"  {nameof (AuthorTag)}=\"{AuthorTag}\"");
      sb.AppendLine ($"  {nameof (AuthorFile)}=\"{AuthorFile}\"");
      sb.AppendLine ($"  {nameof (TitleTag)}=\"{TitleTag}\"");
      sb.AppendLine ($"  {nameof (TitleFile)}=\"{TitleFile}\"");
      sb.AppendLine ($"  {nameof (YearTag)}={YearTag?.Year}");
      sb.Append     ($"  {nameof (GenreTag)}=\"{GenreTag}\"");
      return sb.ToString ();
    }
  }
}
