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
     

      public BookProgress (Label label, IProgBar progbar) {
        _label = label;
        _progbar = progbar;
      }

      public void Reset () {
        _books.Clear ();
        _label.Text = null;
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
        if (info.NumberOfChapters.HasValue)
          bi.NumberOfChapters = info.NumberOfChapters;
        if (info.NumberOfTracks.HasValue)
          bi.NumberOfTracks = info.NumberOfTracks;

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
        var sb = new StringBuilder ();
        sb.Append ($"{R.MsgProgStep} {_progbar.Value}/{_progbar.Maximum}");
        foreach (var kvp in _books) {
          var bi = kvp.Value;
          sb.Append ($"; \"{bi.Title.Shorten(maxStrLen)}\"");

          if (verbosity.HasFlag (EVerbosity.counters)) { 
            if (bi.PartNumber.HasValue)
              sb.Append ($", {R.MsgProgPt} {bi.PartNumber}");
            if (bi.NumberOfChapters.HasValue)
              sb.Append ($", {bi.NumberOfChapters} {R.MsgProgCh}");
            if (bi.NumberOfTracks.HasValue)
              sb.Append ($", {bi.NumberOfTracks} {R.MsgProgTr}");
          }

          if (bi.Phase != EProgressPhase.none && verbosity.HasFlag (EVerbosity.phase))
            sb.Append ($", {R.ResourceManager.GetStringEx(bi.Phase.ToString())}");

          if (verbosity.HasFlag (EVerbosity.chapters)) {
            buildInfoLabelSequence (sb, bi.Parts, R.MsgProgPt);
            buildInfoLabelSequence (sb, bi.Chapters, R.MsgProgCh);
            buildInfoLabelSequence (sb, bi.Tracks, R.MsgProgTr);
          }
        }

        return sb.ToString();
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
      uint Maximum { get; }
      uint Value { get; }
    }

    class ProgBar : IProgBar{
      const uint _1000 = 1000;

      private readonly ProgressBar _progBar;

      private uint _maximum; 
      private uint _value;
      private readonly uint M = 1;

      public uint Maximum => _maximum / M;
      public uint Value => Math.Min (_value / M + 1, Maximum);

      public Action Callback { private get; set; }

      public ProgBar (ProgressBar progBar, bool perMille = false) {
        _progBar = progBar;
        if (perMille)
          M = _1000;
      }

      public void Reset () {
        _value = 0;
        _maximum = 0;
        _progBar.Value = 0;
        _progBar.Maximum = 1; // not 0
      }

      public void UpdateMaximum (int incMax) {
        int max = (int)_maximum + incMax * (int)M;
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

    private readonly ProgBar _progBarPart;
    private readonly ProgBar _progBarTrack;
    private readonly BookProgress _bookProgress;


    public ProgressProcessor (ProgressBar progBarPart, ProgressBar progBarTrack, Label labelInfo) {
      _progBarPart = new ProgBar(progBarPart);
      _progBarTrack = new ProgBar(progBarTrack, true);
      _bookProgress = new BookProgress (labelInfo, _progBarTrack);
      _progBarTrack.Callback = _bookProgress.SetInfoLabel;
    }

    public void Reset () {
      _progBarPart.Reset ();
      _progBarTrack.Reset ();
      _bookProgress.Reset ();
    }

    public void Update (ProgressMessage msg) {
      if (msg is null)
        return;

      if (msg.Reset) {
        Reset ();
        return;
      }

      if (msg.AddTotalParts.HasValue)
        _progBarPart.UpdateMaximum ((int)msg.AddTotalParts.Value);
      if (msg.IncParts.HasValue)
        _progBarPart.UpdateValue (msg.IncParts.Value);

      if (msg.AddTotalTracks.HasValue)
        _progBarTrack.UpdateMaximum (msg.AddTotalTracks.Value);
      if (msg.IncTracks.HasValue)
        _progBarTrack.UpdateValue (msg.IncTracks.Value);
      else if (msg.IncTracksPerMille.HasValue)
        _progBarTrack.UpdateValuePerMille (msg.IncTracksPerMille.Value);

      _bookProgress.Update (msg.Info);

    }
  }
}
