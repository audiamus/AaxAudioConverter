using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using audiamus.aux;
using audiamus.aux.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  public class FFmpeg : ProcessHost {

    [Flags]
    public enum ETranscode {
      normal = 0,
      copy = 1,
      noChapters = 2
    }

    #region const
    public const string FFMPEG_EXE = "ffmpeg.exe";

    const string OUTPUT = "<OUTPUT>";
    const string INPUT = "<INPUT>";
    const string INPUT2 = "<INPUT2>";

    const string COPY = "<COPY>";
    const string ACTIVATION = "<ACTIVATION>";
    const string ACT_BYTES = "<ACT_BYTES>";
    const string BEGIN = "<BEGIN>";
    const string END = "<END>";
    const string TS_FROM = "<TS_FROM>";
    const string TS_TO = "<TS_TO>";

    // -activation_bytes 0f01f010 -i myfile.aax -vn -c:a copy myfile.m4a
    const string FFMPEG_TRANSCODE = @"-hide_banner <ACTIVATION> -y <BEGIN> <END> -i ""<INPUT>"" -map_metadata -1 -map_chapters -1 <COPY> ""<OUTPUT>""";
    const string FFMPEG_TRANSCODE2 = @"-hide_banner <ACTIVATION> -y <BEGIN> <END> -i ""<INPUT>"" -i ""<INPUT2>"" -map_metadata 1 -map_chapters 1 <COPY> ""<OUTPUT>""";
    const string FFMPEG_EXTRACTMETA = @"-hide_banner -y -i ""<INPUT>"" -f ffmetadata ""<OUTPUT>""";
     
    const string FFMPEG_VERSION = @"-version";

    const string FFMPEG_PROBE = @"-hide_banner <ACTIVATION> -i ""<INPUT>""";

    const string FFMPEG_SILENCE = @"-hide_banner -y <ACTIVATION> -i ""<INPUT>"" -af silencedetect=noise=-30dB:d=0.5 -f null -";

    
    public const string ACTIVATION_BYTES = @"-activation_bytes";
    const string ACTIVATION_PARAM = ACTIVATION_BYTES + @" <ACT_BYTES>";
    const string BEGIN_PARAM = @"-ss <TS_FROM>";
    const string END_PARAM = @"-to <TS_TO>";
    const string COPY_PARAM = @"-vn -c:a copy";

    #endregion
    #region private fields

    private readonly string _filenameIn;
    private readonly string _filenameMeta;

    private readonly object _lockable = new object ();

    bool _success;
    bool _aborted;

    bool _listComplete;
    #endregion
    #region props
    private static string FFmpegExePath {
      get
      {
        string dir = FFmpegDir;
        if (!Directory.Exists (dir))
          dir = GetFFmpegDir?.Invoke ();
        if (!Directory.Exists (dir))
          return FFMPEG_EXE;
        return Path.Combine (dir, FFMPEG_EXE);
      }
    }

    public static Func<string> GetFFmpegDir { get; set; }
    public static string FFmpegDir { get; set; }

    internal Func<bool> Cancel { private get; set; }
    internal Action<double> Progress { private get; set; }

    internal AudioMeta AudioMeta { get; } = new AudioMeta ();

    internal List<Chapter> Chapters { get; private set; }
    internal List<TimeInterval> Silences { get; private set; }

    internal bool IsMp3Stream { get; private set; }
    internal bool HasNoActivation { get; private set; }

    internal Version Version { get; private set; }

    #endregion
    #region ctor

    public FFmpeg (string filenameIn, string filenameMeta = null) {
      _filenameIn = filenameIn;
      _filenameMeta = filenameMeta;
    }

    #endregion
    #region public methods

    public bool VerifyFFmpegPath () {
      string param = FFMPEG_VERSION;
      _success = false;

      Log (4, this, () => param.SubstitUser ());
      string result = runProcess (FFmpegExePath, param, false, ffMpegAsyncHandlerVersion);

      return _success;
    }

    public bool GetAudioMeta () {
      _success = true;
      _aborted = false;

      string param = FFMPEG_PROBE;
      param = param.Replace (ACTIVATION, string.Empty);
      param = param.Replace (INPUT, _filenameIn);

      Log (4, this, () => param.SubstitUser ());
      string result = runProcess (FFmpegExePath, param, true, ffMpegAsyncHandlerAudioMeta);

      return _success;

    }

    public bool VerifyActivation (string actBytes, bool withChapters = false) {
      _success = true;
      _aborted = false;
      HasNoActivation = false;

      string param = FFMPEG_PROBE;
      if (actBytes is null)
        param = param.Replace (ACTIVATION, string.Empty);
      else {
        param = param.Replace (ACTIVATION, ACTIVATION_PARAM);
        param = param.Replace (ACT_BYTES, actBytes);
      }

      param = param.Replace (INPUT, _filenameIn);

      if (withChapters)
        Chapters = new List<Chapter> ();

      Log (4, this, () => param.SubstitUser ().SubstitActiv ());
      string result = runProcess (FFmpegExePath, param, true, ffMpegAsyncHandlerActivation);

      return _success;
    }

    public bool Transcode (string filenameOut, ETranscode modifiers, string actBytes = null, TimeSpan? from = null, TimeSpan? to = null) {
      _success = false;
      _aborted = false;
      AudioMeta.Time.Duration = TimeSpan.Zero;

      string param = FFMPEG_TRANSCODE;
      if (!(_filenameMeta is null))
        param = FFMPEG_TRANSCODE2;
      if (actBytes is null)
        param = param.Replace (ACTIVATION, string.Empty);
      else {
        param = param.Replace (ACTIVATION, ACTIVATION_PARAM);
        param = param.Replace (ACT_BYTES, actBytes);
      }

      if (from.HasValue) {
        param = param.Replace (BEGIN, BEGIN_PARAM);
        string secs = from.Value.TotalSeconds.ToString ("f3");
        param = param.Replace (TS_FROM, secs);
        AudioMeta.Time.Begin = from.Value;
      } else
        param = param.Replace (BEGIN, string.Empty);

      if (to.HasValue) {
        param = param.Replace (END, END_PARAM);
        string secs = to.Value.TotalSeconds.ToString ("f3");
        param = param.Replace (TS_TO, secs);
        AudioMeta.Time.End = to.Value;
      } else
        param = param.Replace (END, string.Empty);

      param = param.Replace (INPUT, _filenameIn);
      param = param.Replace (OUTPUT, filenameOut);

      if (modifiers.HasFlag(ETranscode.copy))
        param = param.Replace (COPY, COPY_PARAM);
      else
        param = param.Replace (COPY, string.Empty);

      if (!(_filenameMeta is null))
        param = param.Replace (INPUT2, _filenameMeta);

      Log (4, this, () => param.SubstitUser ().SubstitActiv ());
      string result = runProcess (FFmpegExePath, param, true, ffMpegAsyncHandlerTranscode);

      return _success && !_aborted;
    }

    public bool ExtractMeta (string filenameOut) {
      _success = true;
      _aborted = false;
      AudioMeta.Time.Duration = TimeSpan.Zero;

      string param = FFMPEG_EXTRACTMETA;
      param = param.Replace (INPUT, _filenameIn);
      param = param.Replace (OUTPUT, filenameOut);

      Log (4, this, () => param.SubstitUser ());
      string result = runProcess (FFmpegExePath, param, true, ffMpegAsyncHandlerMeta);

      return _success && !_aborted;
    }

    public bool DetectSilence (string actBytes = null) {
      _success = false;
      _aborted = false;
      AudioMeta.Time.Duration = TimeSpan.Zero;
      Silences = new List<TimeInterval> ();

      string param = FFMPEG_SILENCE;
      if (actBytes is null)
        param = param.Replace (ACTIVATION, string.Empty);
      else {
        param = param.Replace (ACTIVATION, ACTIVATION_PARAM);
        param = param.Replace (ACT_BYTES, actBytes);
      }
      param = param.Replace (INPUT, _filenameIn);

      Log (4, this, () => param.SubstitUser ().SubstitActiv ());
      string result = runProcess (FFmpegExePath, param, true, ffMpegAsyncHandlerSilence);

      return _success && !_aborted;
    }



    #endregion
    #region private methods

    //frame=    5 fps=0.5 q=1.6 size=N/A time=00:05:00.00 bitrate=N/A  
    private static readonly Regex _rgxTimestamp1 = new Regex (@"frame=\s*?\d+.*time=(\d+:\d+:\d+\.\d+)", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxTimestamp2 = new Regex (@"size=\s*?\d+.*time=(\d+:\d+:\d+\.\d+)", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex _rgxDuration = new Regex (@"Duration:\s*?(\d+:\d+:\d+\.\d+)", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxDurationEx = new Regex (@"Duration:\s*?(\d+:\d+:\d+\.\d+).*bitrate:\s+(\d+)\s+kb/s",
      RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex _rgxVersion = new Regex (@"^ffmpeg version\s+([\d.]+)\s+", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxMuxFinal = new Regex (@"video.*audio.*muxing overhead", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxErrorHeader = new Regex (@"error reading header$", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxErrorActivationOption = new Regex (@"Error setting option activation_bytes", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxInvalid = new Regex (@"Invalid data found when processing input$", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxNoActivatonOpt = new Regex (@"Option activation_bytes not found", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxAudioStream = new Regex (@"^\s+Stream.*audio:\s([a-z0-9]+)", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxAudioStreamEx = new Regex (@"^\s+Stream.*audio:\s([a-z0-9]+).*,\s+(\d+)\s+Hz,\s+(\w+),.*,\s+(\d+)\s+kb/s", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxAnyStream = new Regex (@"^\s+Stream\s+", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex _rgxChapter = new Regex (@"^\s+Chapter.*start\s+(\d+\.?\d*).*end\s+(\d+\.?\d*)$", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxChapterMeta = new Regex (@"^\s+Metadata:", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxChapterTitle = new Regex (@"^\s+title.*:\s+(\w+.*)$", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);

    //[silencedetect @ 0000000002a54400] silence_start: 4602.43
    //[silencedetect @ 0000000002a54400] silence_end: 4603.68 | silence_duration: 1.24948
    private static readonly Regex _rgxSilenceStart = new Regex (@"^\[silencedetect.*\]\s+silence_start:\s+(\d+\.?\d*)$", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxSilenceEnd = new Regex (@"^\[silencedetect.*\]\s+silence_end:\s+(\d+\.?\d*).*silence_duration:\s+(\d+\.?\d*)$", 
      RegexOptions.Compiled | RegexOptions.IgnoreCase);


    private void ffMpegAsyncHandlerVersion (object sendingProcess, DataReceivedEventArgs outLine) {
      if (outLine.Data is null)
        return;

#if TRACE && EXTRA
      Trace.WriteLine (outLine.Data);
#endif
      Log (4, this, () => outLine.Data.SubstitUser ());


      Match match = _rgxVersion.Match (outLine.Data);
      if (!match.Success)
        return;

      _success = true;
      bool succ = Version.TryParse (match.Groups[1].Value, out Version version);

      if (succ)
        Version = version;
    }

    private void ffMpegAsyncHandlerAudioMeta (object sendingProcess, DataReceivedEventArgs outLine) {
      if (outLine.Data is null)
        return;

#if TRACE && EXTRA
      Trace.WriteLine (outLine.Data);
#endif
      Log (4, this, () => outLine.Data.SubstitUser ());

      Match match = _rgxDurationEx.Match (outLine.Data);
      if (match.Success) {
        AudioMeta.Time.Duration = tryParseTimestamp (match);
        AudioMeta.AvgBitRate = tryParseUInt (match, 2);
      } else {
        match = _rgxAudioStreamEx.Match (outLine.Data);
        if (match.Success) {
          AudioMeta.Samplerate = tryParseUInt (match, 2);
          string sc = match.Groups[3].Value.ToLower();
          if (sc == "mono")
            AudioMeta.Channels = 1;
          else if (sc == "stereo")
            AudioMeta.Channels = 2;
          _success = true;
        }
      }
    }

    private void ffMpegAsyncHandlerActivation (object sendingProcess, DataReceivedEventArgs outLine) {
      onCancel ();

      if (outLine.Data is null)
        return;

#if TRACE && EXTRA
      Trace.WriteLine (outLine.Data);
#endif
      Log (4, this, () => outLine.Data.SubstitUser ());

      Match match = null;
      if (Chapters != null && !_listComplete) {
        match = _rgxChapter.Match (outLine.Data);
        if (match.Success) {
          var chapter = new Chapter ();
          Chapters.Add (chapter);
          chapter.Time.Begin = tryParseSeconds (match);
          chapter.Time.End = tryParseSeconds (match, 2);
          return;
        } else {
          match = _rgxAnyStream.Match (outLine.Data);
          if (match.Success)
            _listComplete = true;
          else {
            var chapter = Chapters.LastOrDefault ();
            if (chapter != null) {
              match = _rgxChapterMeta.Match (outLine.Data);
              if (match.Success) {
                chapter.Name = string.Empty;
                return;
              } else {
                if (chapter.Name == string.Empty) {
                  match = _rgxChapterTitle.Match (outLine.Data);
                  if (match.Success) {
                    chapter.Name = match.Groups[1].Value;
                  }
                  return;
                }
              }
            }
          }
        }
      }

      match = _rgxErrorHeader.Match (outLine.Data);
      if (match.Success)
        _success = false;
      else {
        match = _rgxErrorActivationOption.Match (outLine.Data);
        if (match.Success) {
          _success = false;
        } else {
          match = _rgxInvalid.Match (outLine.Data);
          if (match.Success) {
            _success = false;
          } else {
            match = _rgxNoActivatonOpt.Match (outLine.Data);
            if (match.Success) {
              HasNoActivation = true;
            } else {
              match = _rgxAudioStream.Match (outLine.Data);
              if (match.Success) {
                string format = match.Groups[1].Value;
                IsMp3Stream = format.ToLowerInvariant () == "mp3";
              }
            }
          }
        }
      }
    }

    private void ffMpegAsyncHandlerMeta (object sendingProcess, DataReceivedEventArgs outLine) {
      onCancel ();

      if (outLine.Data is null)
        return;

#if TRACE && EXTRA
      Trace.WriteLine (outLine.Data);
#endif
      Log (4, this, () => outLine.Data.SubstitUser ());
    }

    private void ffMpegAsyncHandlerSilence (object sendingProcess, DataReceivedEventArgs outLine) {
      onCancel ();

      if (outLine.Data is null)
        return;

#if TRACE && EXTRA
      Trace.WriteLine (outLine.Data);
#endif
      Log (4, this, () => outLine.Data.SubstitUser ());

      var t = AudioMeta.Time;

      Match match;
      if (t.Duration == TimeSpan.Zero) {

        match = _rgxDuration.Match (outLine.Data);
        if (match.Success) {
          TimeSpan duration = tryParseTimestamp (match);
          t.Duration = duration;
        }

      } else {

        match = _rgxSilenceStart.Match (outLine.Data);
        if (match.Success) {
          var ivl = Silences.LastOrDefault ();
          if (ivl != null && ivl.End == TimeSpan.Zero)
            Silences.RemoveAt (Silences.Count - 1);
          var start = tryParseSeconds (match);
          Silences.Add (new TimeInterval (start, TimeSpan.Zero));
          return;

        } else {

          match = _rgxSilenceEnd.Match (outLine.Data);
          if (match.Success) {
            var ivl = Silences.LastOrDefault ();
            if (ivl == null)
              return;
            if (ivl.End != TimeSpan.Zero) {
              Silences.RemoveAt (Silences.Count - 1);
              return;
            }
            var end = tryParseSeconds (match);
            var duration = tryParseSeconds (match, 2);
            ivl.End = end;
            if (Math.Abs ((ivl.Duration - duration).TotalSeconds) > 0.01)
              Silences.RemoveAt (Silences.Count - 1);
            return;

          } else {

            match = _rgxTimestamp1.Match (outLine.Data);

            if (!match.Success) {
              match = _rgxMuxFinal.Match (outLine.Data);
              if (match.Success)
                _success = true;
              return;

            }
          }
        }
      }


      if (!match.Success)
        return;

      if (t.Duration == TimeSpan.Zero)
        return;

      TimeSpan ts = tryParseTimestamp (match);
      double progress = ts.TotalSeconds / t.Duration.TotalSeconds;

      Progress?.Invoke (progress);


    }

    private void ffMpegAsyncHandlerTranscode (object sendingProcess, DataReceivedEventArgs outLine) {
      onCancel ();

      if (outLine.Data is null)
        return;

#if TRACE && EXTRA
      Trace.WriteLine (outLine.Data);
#endif
      Log (4, this, () => outLine.Data.SubstitUser ());

      var t = AudioMeta.Time;

      Match matchTs = _rgxTimestamp1.Match (outLine.Data);
      if (!matchTs.Success)
        matchTs = _rgxTimestamp2.Match (outLine.Data);
      if (!matchTs.Success) {

        if (t.Duration == TimeSpan.Zero) {
          Match matchDur = _rgxDuration.Match (outLine.Data);
          if (matchDur.Success) {
            TimeSpan duration = tryParseTimestamp (matchDur);
            if (t.Begin == TimeSpan.Zero)
              t.Duration = duration;
            else
              t.Duration = duration - t.Begin;
            return;
          }
        } else {
          Match matchFinal = _rgxMuxFinal.Match (outLine.Data);
          if (matchFinal.Success) 
            _success = true;
        }
      }


      if (!matchTs.Success)
        return;

      if (t.Duration == TimeSpan.Zero)
        return;

      TimeSpan ts = tryParseTimestamp (matchTs);
      double progress = ts.TotalSeconds / t.Duration.TotalSeconds;

#if TRACE && EXTRA
      Trace.WriteLine ($"{this.GetType ().Name}.{nameof (ffMpegAsyncHandlerTranscode)} {ts.TotalSeconds:f0}/{t.Duration.TotalSeconds:f0}");
#endif
      Log (4, this, () => $"{ts.TotalSeconds:f0}/{t.Duration.TotalSeconds:f0}");

      Progress?.Invoke (progress);


    }

    private void onCancel () {
      if (Cancel != null && Cancel ()) {
        _aborted = true;
        if (Process != null && !Process.HasExited)
          Process.StandardInput.Write ('q');
      }
    }

    private static uint tryParseUInt (Match match, int idx = 1) {
      bool succ = uint.TryParse (match.Groups[idx].Value, out uint n);
      return n;
    }

    private static TimeSpan tryParseTimestamp (Match match, int idx = 1) {
      string sDuration = match.Groups[idx].Value;
      bool succ = TimeSpan.TryParse (sDuration, out TimeSpan timespan);
      if (!succ) {
        int n = sDuration.Where (c => c == ':').Count ();
        if (n == 2) {
          int p = sDuration.IndexOf (':');
          string sHours = sDuration.Substring (0, p);
          string sMinSec = sDuration.Substring (p);
          int.TryParse (sHours, out int hours);
          sDuration = "00" + sMinSec;
          succ = TimeSpan.TryParse (sDuration, out timespan);
          timespan += TimeSpan.FromHours (hours);
        }
      }
      return timespan;
    }

    private static TimeSpan tryParseSeconds (Match match, int idx = 1) {
      double.TryParse (match.Groups[idx].Value, out double seconds);
      return TimeSpan.FromSeconds (seconds);
    }

    #endregion
  }

  static class StringEx {
    public static string SubstitActiv (this string s) {
      if (s is null)
        return null;
      int pos0 = s.IndexOf (FFmpeg.ACTIVATION_BYTES);
      if (pos0 >= 0) {
        int pos1 = s.IndexOf (' ', pos0 + 1);
        int pos2 = s.IndexOf (' ', pos1 + 1);
        if (pos1 > 0 && pos2 > 0) {
          s = s.Remove (pos1 + 1, pos2 - pos1 - 1);
          s = s.Insert (pos1 + 1, "XXXXXXXX");
        }  
      }
      return s;
    }
  }
}
