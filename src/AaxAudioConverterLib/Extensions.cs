using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

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
    public static (int min, int max, int val) TrackDuration (this byte value, EConvMode mode) {
      ETrackDuration maxTrackDurMins = mode == EConvMode.splitTime ? ETrackDuration.MaxTimeSplit : ETrackDuration.MaxSplitChapter;
      int min = (int)ETrackDuration.Min;
      int max = (int)maxTrackDurMins;
      int val = Math.Max (min, Math.Min ((int)maxTrackDurMins, value));
      return (min, max, val);
    }
  }

  public static class EReducedBitRateEx {
    private static readonly Regex _rgxEnumBitrate = new Regex (@"(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static uint UInt32 (this EReducedBitRate ebitrate) {
      string sbitrate = ebitrate.ToString ();
      Match match = _rgxEnumBitrate.Match (sbitrate);
      if (match.Success) {
        if (uint.TryParse (match.Groups[1].Value, out var bitrate))
          return bitrate;
      }
      return 0;
    }
  }

  public static class BitrateEx {
    public static (uint applBitrate, uint settingsBitrate) ApplicableBitRate (this IBitRateSettings settings, uint sourceBitrate) {
      uint settingsBitrate = settings.ReducedBitRate.UInt32 ();
      if ((settingsBitrate == 0 || settingsBitrate >= sourceBitrate) && !settings.VariableBitRate)
        return (0, 0);

      uint applicableBitrate = sourceBitrate;
      if (settingsBitrate != 0)
        applicableBitrate = Math.Min (sourceBitrate, settingsBitrate);

      return (applicableBitrate, settingsBitrate);
    }
  }

  public static class ConvSettingsEx {
    public static bool UseEmbeddedChapterTimes (this IConvSettings settings) =>
      settings.PreferEmbeddedChapterTimes == EPreferEmbeddedChapterTimes.ifSilent && settings.NamedChapters != ENamedChapters.no;
  }
}
