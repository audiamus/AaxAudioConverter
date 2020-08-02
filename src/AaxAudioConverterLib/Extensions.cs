using System;
using System.Linq;
using System.Net;


namespace audiamus.aaxconv.lib.ex {
  static class HtmlDecode {
    public static string Decode (this string s) {
      if (s is null)
        return null;
      return WebUtility.HtmlDecode (s);
    }

    public static string[] Decode (this string[] ss) {
      if (ss is null)
        return new string[0];
      return ss.Select (Decode).ToArray ();
    }
  }

  public static class TrackDurationEx {
    public static (int min, int max, int val) TrackDuration (this byte value, EConvMode mode)  {
      ETrackDuration maxTrackDurMins = mode == EConvMode.splitTime ? ETrackDuration.MaxTimeSplit : ETrackDuration.MaxSplitChapter;
      int min = (int)ETrackDuration.Min;
      int max = (int)maxTrackDurMins;
      int val = Math.Max (min, Math.Min ((int)maxTrackDurMins, value));
      return (min, max, val);
    }
  }
}
