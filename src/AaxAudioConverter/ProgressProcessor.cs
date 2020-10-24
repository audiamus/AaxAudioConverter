using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using audiamus.aaxconv.lib;
using audiamus.aux.ex;
using static audiamus.aaxconv.lib.ProgressInfo;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv {
  using R = Properties.Resources;

  class ProgressProcessor {

    class BookProgress {

      [Flags]
      enum EVerbosity {
        min = 0,
        phase = 1,
        counters = 2,
        phase_counters = 3,
        chapters = 4,
        all = 7,
      }

      class BookInfo {
        public string Title;
        public EProgressPhase Phase;
        public uint? PartNumber;
        public uint? NumberOfChapters;
        public uint? NumberOfTracks;
        public readonly List<uint> Parts = new List<uint> ();
        public readonly List<uint> Chapters = new List<uint> ();
        public readonly List<uint> Tracks = new List<uint> ();
      }

      private readonly SortedDictionary<string, BookInfo> _books = new SortedDictionary<string, BookInfo> ();
      private readonly Label _label;
      private readonly IProgBar _progbar;

      private string _logString; 

      public BookProgress (Label label, IProgBar progbar) {
        _label = label;
        _progbar = progbar;
      }

      public void Reset () {
        _books.Clear ();
        _label.Text = null;
        _logString = null;
      }

      public void Update (ProgressInfo info) {
        if (info is null)
          return;
        processInfo (info);
        SetInfoLabel ();
      }

      const int SHORTSTRING1 = 60;
      const int SHORTSTRING2 = 30;
      const int SHORTSTRING3 = 20;

      public void SetInfoLabel () {
        bool succ = buildAndSetInfoLabel (EVerbosity.all, -1);
        if (succ)
          return;
        succ = buildAndSetInfoLabel (EVerbosity.all, SHORTSTRING1);
        if (succ)
          return;
        succ = buildAndSetInfoLabel (EVerbosity.all, SHORTSTRING2);
        if (succ)
          return;
        succ = buildAndSetInfoLabel (EVerbosity.all);
        if (succ)
          return;
        succ = buildAndSetInfoLabel (EVerbosity.phase_counters);
        if (succ)
          return;
        succ = buildAndSetInfoLabel (EVerbosity.counters);
        if (succ)
          return;
        buildAndSetInfoLabel (EVerbosity.min);
      }


      private void processInfo (ProgressInfo info) {

        var titleEntry = info.Title;
        string title = titleEntry.Content;
        if (titleEntry.Cancel) {
          _books.Remove (title);
          return;
        }

        bool succ = _books.TryGetValue (title, out BookInfo bi);
        if (!succ) {
          bi = new BookInfo {
            Title = title,
          };
          _books[title] = bi;
        }

        if (info.Phase != EProgressPhase.none)
          bi.Phase = info.Phase;

        if (info.PartNumber.HasValue)
          bi.PartNumber = info.PartNumber;
        if (info.NumberOfChapters.HasValue) {
          if (info.NumberOfChapters.Value > 0)
            bi.NumberOfChapters = info.NumberOfChapters;
          else
            bi.NumberOfChapters = null;
        }
        if (info.NumberOfTracks.HasValue) {
          if (info.NumberOfTracks.Value > 0)
            bi.NumberOfTracks = info.NumberOfTracks;
          else
            bi.NumberOfTracks = null;
        }

        processItems (bi.Parts, info.Part);
        processItems (bi.Chapters, info.Chapter);
        processItems (bi.Tracks, info.Track);
        
      }

      private static void processItems (List<uint> items, ProgressEntry<uint> entry) {
        if (entry?.Content is null)
          return;

        uint itm = entry.Content;
        if (entry.Cancel) {
          items.Remove (itm);
          return;
        }

        items.Add (itm);
        items.Sort ();
      }

      private bool buildAndSetInfoLabel (EVerbosity verbosity, int maxStrLen = SHORTSTRING3) {
        string text = buildInfoLabel (verbosity, maxStrLen);
        return setInfoLabelText (text, verbosity == EVerbosity.min);
      }

      private string buildInfoLabel (EVerbosity verbosity, int maxStrLen) {
        bool doLog = Level >= 3;
        var sbLbl = new StringBuilder ();
        var sbLog = new StringBuilder ();

        sbLbl.Append ($"{R.MsgProgStep} {_progbar.AccuValue}/{_progbar.AccuMaximum}");
        sbLog.Append ($"{R.MsgProgStep} .../{_progbar.AccuMaximum}");

        foreach (var kvp in _books) {
          var bi = kvp.Value;

          string s = $"; \"{bi.Title.Shorten (maxStrLen)}\"";
          sbLbl.Append (s);
          if (doLog)
            sbLog.Append (s);

          if (verbosity.HasFlag (EVerbosity.counters)) {
            if (bi.PartNumber.HasValue) {
              s = $", {R.MsgProgPt} {bi.PartNumber}";
              sbLbl.Append (s);
              if (doLog)
                sbLog.Append (s);
            }

            if (bi.NumberOfChapters.HasValue) {
              s = $", {bi.NumberOfChapters} {R.MsgProgCh}";
              sbLbl.Append (s);
              if (doLog)
                sbLog.Append (s);
            }

            if (bi.NumberOfTracks.HasValue) {
              s = $", {bi.NumberOfTracks} {R.MsgProgTr}";
              sbLbl.Append (s);
              if (doLog)
                sbLog.Append (s);
            }
          }

          if (bi.Phase != EProgressPhase.none && verbosity.HasFlag (EVerbosity.phase)) {
            s = $", {R.ResourceManager.GetStringEx (bi.Phase.ToString ())}";
            sbLbl.Append (s);
            if (doLog)
              sbLog.Append (s);
          }

          if (verbosity.HasFlag (EVerbosity.chapters)) {
            buildInfoLabelSequence (sbLbl, bi.Parts, R.MsgProgPt);
            buildInfoLabelSequence (sbLbl, bi.Chapters, R.MsgProgCh);
            buildInfoLabelSequence (sbLbl, bi.Tracks, R.MsgProgTr);
          }
        }

        if (doLog) {
          string sLog = sbLog.ToString (); 
          if (!string.Equals (sLog, _logString)) {
            _logString = sLog;
            Log (3, this, sLog);
          }
        }

        return sbLbl.ToString();
      }

      private static void buildInfoLabelSequence (StringBuilder sb, List<uint> items, string caption) {
        if (items.Count == 0 || items[0] == 0)
          return;
        sb.Append ($", {caption} ");
        var sb2 = new StringBuilder ();
        if (items.Count <= 5)
          foreach (uint ch in items) {
            if (sb2.Length > 0)
              sb2.Append (',');
            sb2.Append (ch);
          } else {
          for (int i = 0; i < 3; i++) {
            uint ch = items[i];
            if (sb2.Length > 0)
              sb2.Append (',');
            sb2.Append (ch);
          }
          sb2.Append ('…');
          sb2.Append (items.Last ());

        }

        sb.Append (sb2);
      }

      private bool setInfoLabelText (string text, bool enforce = false) {
        Size sizLbL = new Size (_label.Width - 8, _label.Height);
        Size sizText = TextRenderer.MeasureText (text, _label.Font);
        if (sizText.Width <= sizLbL.Width || enforce) {
          _label.Text = text;
          return true;
        }
        return false;
      }


    }

    interface IProgBar {
      uint AccuMaximum { get; }
      uint AccuValue { get; }
    }

    class ProgBar : IProgBar {
      const uint _1000 = 1000;

      private readonly ProgressBar _progBar;

      private uint _accuMaximum; 
      private uint _accuValue;
      private uint _maximum; 
      private uint _value;
      private readonly uint M = 1;

      public uint AccuMaximum => (_maximum + _accuMaximum) / M;
      public uint AccuValue {
        get
        {
          uint total = _value + _accuValue;
          uint value = total / M;
          uint rem = total % M;
          if (rem > 0)
            value += 1;

          return Math.Min (value, AccuMaximum);
        }
      }

      internal (uint accu, uint cur) Value => (_accuValue, _value);

      public Action Callback { private get; set; }

      public ProgBar (ProgressBar progBar, bool perMille = false) {
        _progBar = progBar;
        if (perMille)
          M = _1000;
      }

      public void Reset (bool progBarValueOnly = false) {
        uint value = _value;
        _accuValue += _value;
        _value = 0;
        _progBar.Value = 0;

        if (progBarValueOnly) {
          reduceMaximumProgBar (value);
          return;
        }

        _accuValue = 0;
        _maximum = 0;
        _accuMaximum = 0;
        _progBar.Maximum = 1; // not 0
      }

      private void reduceMaximumProgBar (uint value) {
        uint max = _maximum - value;
        max = Math.Max (0u, max); 
        _accuMaximum += _maximum - max;
        _maximum = max;
        int val = (int)Math.Max (1u, max);
        _progBar.Maximum = val;
        Callback?.Invoke ();
      } 

      public void UpdateMaximum (int incMax) {
        int max = (int)_maximum + incMax *(int)M;
        _maximum = (uint)Math.Max (1, max);
        int val = Math.Max ((int)_maximum, _progBar.Value);
        _progBar.Maximum = val;
        Callback?.Invoke ();
      }

      public void UpdateValue (uint inc) {
        _value += inc * M;
        int val = Math.Min ((int)_value, _progBar.Maximum);
        _progBar.Value = val;
        Callback?.Invoke ();
      }

      public void UpdateValuePerMille (uint incPerMille) {
        _value += incPerMille;
        int val = Math.Min ((int)_value, _progBar.Maximum);
        _progBar.Value = val;
        Callback?.Invoke ();
#if TRACE && EXTRA
        Trace.WriteLine ($"{this.GetType().Name}.{nameof(UpdateValuePerMille)} {_progBar.Value}/{_progBar.Maximum}");
#endif
      }
    }

    private readonly ProgBar _progBarPhase;
    private readonly ProgBar _progBarStep;
    private readonly BookProgress _bookProgress;


    public ProgressProcessor (ProgressBar progBarPart, ProgressBar progBarTrack, Label labelInfo) {
      _progBarPhase = new ProgBar(progBarPart);
      _progBarStep = new ProgBar(progBarTrack, true);
      _bookProgress = new BookProgress (labelInfo, _progBarStep);
      _progBarStep.Callback = _bookProgress.SetInfoLabel;
    }

    public void Reset () {
      _progBarPhase.Reset ();
      _progBarStep.Reset ();
      _bookProgress.Reset ();
    }

    public void Update (ProgressMessage msg) {
      if (msg is null)
        return;

      if (msg.Reset) {
        Reset ();
        return;
      }

      if (msg.ResetSteps) {
        Log (3, this, () => $"{nameof(msg.ResetSteps)}, {_progBarStep.AccuValue}/{_progBarStep.AccuMaximum}");
        _progBarStep.Reset (true);
      }

      if (msg.AddTotalWeightedPhases.HasValue)
        _progBarPhase.UpdateMaximum ((int)msg.AddTotalWeightedPhases.Value);
      if (msg.IncWeightedPhases.HasValue)
        _progBarPhase.UpdateValue (msg.IncWeightedPhases.Value);

      if (msg.AddTotalSteps.HasValue) {
        Log (3, this, () => $"{nameof(msg.AddTotalSteps)}={msg.AddTotalSteps.Value} to {_progBarStep.AccuMaximum}");
        _progBarStep.UpdateMaximum (msg.AddTotalSteps.Value);
      }

      if (msg.IncSteps.HasValue)
        _progBarStep.UpdateValue (msg.IncSteps.Value);
      else if (msg.IncStepsPerMille.HasValue)
        _progBarStep.UpdateValuePerMille (msg.IncStepsPerMille.Value);

      if (msg.Info is null)
        return;

      if (msg.Info.Phase != EProgressPhase.none) 
      {
        var (accu, cur) = _progBarStep.Value;
        Log (3, this, () => $"{msg.Info.Phase}: {accu} + {cur} = {accu + cur}");
      }
      _bookProgress.Update (msg.Info);

    }
  }
}
