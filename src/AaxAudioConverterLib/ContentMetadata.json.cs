using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace audiamus.aaxconv.lib.json {
  public class Chapter {
    public int length_ms { get; set; }
    public int start_offset_ms { get; set; }
    public int start_offset_sec { get; set; }
    public string title { get; set; }
  }

  public class ChapterInfo {
    public int brandIntroDurationMs { get; set; }
    public int brandOutroDurationMs { get; set; }
    public List<Chapter> chapters { get; set; }
    public int runtime_length_ms { get; set; }
    public int runtime_length_sec { get; set; }
  }

  public class ContentReference {
    public string acr { get; set; }
    public string asin { get; set; }
    public string content_format { get; set; }
    public long content_size_in_bytes { get; set; }
    public string marketplace { get; set; }
    public string sku { get; set; }
    public string tempo { get; set; }
    public string version { get; set; }
  }

  public class ContentMetadata {
    public ChapterInfo chapter_info { get; set; }
    public ContentReference content_reference { get; set; }
  }

  public class AppContentMetadata {
    public ContentMetadata content_metadata { get; set; }
    public List<string> response_groups { get; set; }
  }
}
