using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using audiamus.aux.ex;
using Newtonsoft.Json;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  class AudibleAppContentMetadata {

    struct ContentMetadataFile {
      public readonly string Filename;
      public readonly string ASIN;

      public ContentMetadataFile (string filename, string asin) {
        Filename = filename;
        ASIN = asin;
      }
    }

    public const string CONTENT = "Content";
    const string FILESCACHE = "filescache";
    const string CONTENT_METADATA = "content_metadata_";
    const string JSON = ".json";

    const string REGEX = CONTENT_METADATA + @"(\w+)"; 
    static readonly Regex __regexAsin = new Regex (REGEX, RegexOptions.Compiled); 

    public void GetContentMetadata (Book.BookPart part) {

      string contentMetadataFile = findContentMetadataFile (part.AaxFileItem.FileName);
      if (contentMetadataFile is null)
        return;

      string json = File.ReadAllText (contentMetadataFile);
      var metadata = JsonConvert.DeserializeObject<json.AppContentMetadata> (json);

      // set the chapters
      var chapters = new List<Chapter> ();
      var metaChapters = metadata.content_metadata.chapter_info.chapters;
      if (metaChapters.Count == 0)
        return;

      // handle chapters with no name. Set '.' as a placeholder
      var whites = metaChapters.Where (c => string.IsNullOrWhiteSpace (c.title)).ToList();
      whites.ForEach (c => c.title = ".");



      // handle chapters of zero length. Min length must be 1 ms.
      var zeros = metaChapters.Where (c => c.length_ms == 0).ToList();
      Log (3, this, () => $"chapters: #zero={zeros.Count}, #white={whites.Count}");
      if (zeros.Count > 0) {
        zeros.ForEach (c => c.length_ms = 1);
        for (int i = 1; i < metaChapters.Count; i++) {
          var ch0 = metaChapters[i - 1];
          var ch = metaChapters[i];
          int chOffsNew = ch0.start_offset_ms + ch0.length_ms;
          if (ch.start_offset_ms >= chOffsNew)
            continue;
          int chLenNew = ch.length_ms + ch.start_offset_ms - chOffsNew;
          if (chLenNew <= 0)
            chLenNew = 1;
          ch.start_offset_ms = chOffsNew;
          ch.length_ms = chLenNew;
        }
      }

      foreach (var ch in metaChapters) {
        var chapter = new Chapter ();
        chapter.Name = ch.title.Trim();
        chapter.Time.Begin = TimeSpan.FromMilliseconds (ch.start_offset_ms);
        chapter.Time.End = TimeSpan.FromMilliseconds (ch.start_offset_ms + ch.length_ms);
        Log (3, this, () => chapter.ToString());
        chapters.Add (chapter);
      }
      part.Chapters2 = chapters;

      // set brandintro and brandoutro times
      part.BrandIntro = TimeSpan.FromMilliseconds (metadata.content_metadata.chapter_info.brandIntroDurationMs);
      part.BrandOutro = TimeSpan.FromMilliseconds (metadata.content_metadata.chapter_info.brandOutroDurationMs);
      part.Duration = TimeSpan.FromMilliseconds (metadata.content_metadata.chapter_info.runtime_length_ms);

      Log (3, this, () => $"intro={part.BrandIntro.ToStringHMSm ()}, outro={part.BrandOutro.ToStringHMSm ()}, duration={part.Duration.ToStringHMSm ()}");
    }

    private string findContentMetadataFile (string fileName) {
      // try to find content metadata files, either relative or absolute
      Log (3, this, () => $"\"{fileName.SubstitUser ()}\"");

      // relative
      string cntDir = Path.GetDirectoryName (fileName);
      var diLocalState = Directory.GetParent (cntDir);
      string localStateDir = diLocalState.FullName;
      string cacheDir = Path.Combine (localStateDir, FILESCACHE);
      var dirs = new List<string> ();
      if (Directory.Exists (cacheDir))
        dirs.Add (cacheDir);
      else {
        // absolute
        var absDirs = ActivationCodeApp.GetPackageDirectories ();
        if (absDirs is null)
          return null;
        foreach (var absDir in absDirs) {
          cacheDir = Path.Combine (absDir, FILESCACHE);
          if (Directory.Exists (cacheDir))
            dirs.Add (cacheDir);
        }
      }

      if (dirs.Count == 0)
        return null;

      // get all content metadata filenames
      var files = dirs.SelectMany (d => Directory.GetFiles (d, $"{CONTENT_METADATA}*{JSON}")).Distinct ();

      // isolate asin
      var asinFiles = new List<ContentMetadataFile> ();
      foreach (string file in files) {
        var fn = Path.GetFileNameWithoutExtension (file);
        var match = __regexAsin.Match (fn);
        if (!match.Success)
          continue;
        string asin = match.Groups[1].Value;
        asinFiles.Add (new ContentMetadataFile (file, asin));
      }

      // find matching asin in our filename
      var filnam = Path.GetFileNameWithoutExtension (fileName);
      string contentMetafile = asinFiles.Where (k => filnam.Contains (k.ASIN)).Select (k => k.Filename).FirstOrDefault ();

      Log (3, this, () => $"\"{contentMetafile.SubstitUser ()}\"");

      return contentMetafile;
    }
  }
}
