using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using audiamus.aux.ex;
using Newtonsoft.Json;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  class AudibleAppContentMetadata {

    internal class ContentMetadataFile {
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

    const string RGX_ASIN_AAX = @"_([0-9A-Z]{10})(_|$|\.)";
    static readonly Regex __regexAsinAax = new Regex (RGX_ASIN_AAX, RegexOptions.Compiled); 

    public void GetContentMetadata (Book.Part part, bool fileOnly = false) {
      var filename = part.AaxFileItem.FileName;
      Log (3, this, () => $"\"{filename.SubstitUser ()}\", file only={fileOnly}");

      var contentMetadataFile = findContentMetadataFile (filename);
      if (contentMetadataFile is null || contentMetadataFile.Filename is null) {
        part.AaxFileItem.ContentMetadataFile = new ContentMetadataFile (null, null);
        return;
      }

      part.AaxFileItem.ContentMetadataFile = contentMetadataFile;
      string metafile = contentMetadataFile.Filename;
      Log (3, this, () => $"\"{metafile.SubstitUser ()}\"");

      if (fileOnly)
        return;

      getContentMetaChapters (part, metafile);
    }

    private void getContentMetaChapters (Book.Part part, string metafile) {
      string json = File.ReadAllText (metafile);
      var metadata = JsonConvert.DeserializeObject<json.AppContentMetadata> (json);

      // set the chapters
      var chapters = new List<Chapter> ();
      var metaChapters = metadata.content_metadata.chapter_info.chapters;
      if (metaChapters.Count == 0)
        return;

      // handle chapters with no name. Set '.' as a placeholder
      var whites = metaChapters.Where (c => string.IsNullOrWhiteSpace (c.title)).ToList ();
      whites.ForEach (c => c.title = ".");     

      // handle chapters of zero length. Min length must be 1 ms.
      var zeros = metaChapters.Where (c => c.length_ms == 0).ToList ();
      Log (3, this, () => $"chapters: #zero={zeros.Count}, #white={whites.Count}");
      if (zeros.Count > 0) {
        zeros.ForEach (c => c.length_ms = Chapter.MS_MIN_CHAPTER_LENGTH);
        for (int i = 1; i < metaChapters.Count; i++) {
          var ch0 = metaChapters[i - 1];
          var ch = metaChapters[i];
          int chOffsNew = ch0.start_offset_ms + ch0.length_ms;
          if (ch.start_offset_ms >= chOffsNew)
            continue;
          Log (3, this, () => $"duration fix for: {ch}");
          int chLenNew = ch.length_ms + ch.start_offset_ms - chOffsNew;
          if (chLenNew <= 0)
            chLenNew = Chapter.MS_MIN_CHAPTER_LENGTH;
          ch.start_offset_ms = chOffsNew;
          ch.length_ms = chLenNew;
        }
      }

      foreach (var ch in metaChapters) {
        var chapter = new Chapter ();
        chapter.Name = ch.title.Trim ();
        chapter.Time.Begin = TimeSpan.FromMilliseconds (ch.start_offset_ms);
        chapter.Time.End = TimeSpan.FromMilliseconds (ch.start_offset_ms + ch.length_ms);
        chapters.Add (chapter);
      }
      part.Chapters2 = chapters;

      // set brandintro and brandoutro times
      part.BrandIntro = TimeSpan.FromMilliseconds (metadata.content_metadata.chapter_info.brandIntroDurationMs);
      part.BrandOutro = TimeSpan.FromMilliseconds (metadata.content_metadata.chapter_info.brandOutroDurationMs);
      part.Duration = TimeSpan.FromMilliseconds (metadata.content_metadata.chapter_info.runtime_length_ms);

      Log (3, this, () => chapterList(part));
    }

    private string chapterList (Book.Part part) {
      var sb = new StringBuilder ($"content meta chapters:");
      sb.AppendLine();
      if (!(part.Chapters2 is null))
        foreach (var ch in part.Chapters2)
          sb.AppendLine ($"  {ch}");
      sb.Append ($"  intro={part.BrandIntro.ToStringHMSm ()}, outro={part.BrandOutro.ToStringHMSm ()}, duration={part.Duration.ToStringHMSm ()}");
      return sb.ToString ();
    }


    private ContentMetadataFile findContentMetadataFile (string fileName) {
      // try to find content metadata file, either locally, relative or absolute

      // find and isolate asin
      string fn = Path.GetFileNameWithoutExtension (fileName);
      var match = __regexAsinAax.Match (fn);
      if (!match.Success) {
        Log (3, this, () => $"ASIN not found.");
        return null;
      }
      string asin = match.Groups[1].Value;
      Log (3, this, () => $"ASIN={asin}");

      string contentDir = Path.GetDirectoryName (fileName).StripUnc();
      string contentmetafile = CONTENT_METADATA + asin + JSON;

      {
        // local
        string localPath = Path.Combine (contentDir, contentmetafile).AsUncIfLong();
        if (File.Exists (localPath))
          return new ContentMetadataFile (localPath, asin);
      }
      {
        // relative
        var diLocalState = Directory.GetParent (contentDir);
        if (!(diLocalState is null)) {
          string localStateDir = diLocalState.FullName;
          string cacheDir = Path.Combine (localStateDir, FILESCACHE);

          string relativePath = Path.Combine (cacheDir, contentmetafile).AsUncIfLong ();
          if (File.Exists (relativePath))
            return new ContentMetadataFile (relativePath, asin);
        }
      }
      {
        // absolute
        var absDirs = ActivationCodeApp.GetPackageDirectories ();
        if (absDirs is null)
          return null;
        var cacheDirs = absDirs.Select (d => Path.Combine (d, FILESCACHE));
        foreach (var cacheDir in cacheDirs) {
          string absolutePath = Path.Combine (cacheDir, contentmetafile).AsUncIfLong ();
          if (File.Exists (absolutePath))
            return new ContentMetadataFile (absolutePath, asin);
        }
      }

      Log (3, this, () => $"{contentmetafile} not found.");
      return null;
    }
  }
}
