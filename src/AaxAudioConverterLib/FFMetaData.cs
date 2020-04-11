using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace audiamus.aaxconv.lib {

  class FFMetaData {
    class TimeBase {
      public readonly double Denominator = 1;
      public readonly double Nominator = 1_000_000_000;
      public TimeBase () { }
      public TimeBase (double denom, double nom) {
        Denominator = denom;
        Nominator = nom;
      }
    }

    public List<Chapter> Chapters {get; }

    private TimeBase _timebase;
    private Chapter _chapter;

    private static string[] Tags { get; } = new string[] { "TIMEBASE", "START", "END", "title" };
    private static char[] Escapes { get; } = new char[] { '=', ';', '#', '\\' };
    const string CHAPTER = "[CHAPTER]";
    const string FFMETADATA = ";FFMETADATA1";

    public FFMetaData (List<Chapter> chapters = null) {
      if (chapters is null)
        Chapters = new List<Chapter> ();
      else
        Chapters = chapters;
    }

    public bool Read (string filename) {
      if (Chapters.Count > 0)
        return false;
      using (var ism = new StreamReader (filename)) {
        int i = 0;
        while (!ism.EndOfStream) {
          string s = ism.ReadLine ().Trim ();
          if (s.StartsWith ("[")) {
            terminateChapter ();
            if (s.Equals (CHAPTER, StringComparison.InvariantCultureIgnoreCase)) {
              _timebase = new TimeBase ();
              _chapter = new Chapter ();
              i = 0;
            }
          } else
            parseChapter (s, ref i);

        }
        terminateChapter ();
      }
      return true;
    }


    public bool Write (string filename) {
      if (Chapters is null || Chapters.Count == 0)
        return false;
      using (var osm = new StreamWriter (filename)) {
        osm.WriteLine (FFMETADATA);
        foreach (var ch in Chapters) {
          osm.WriteLine (CHAPTER);
          osm.WriteLine ($"{Tags[0]}=1/1000");
          osm.WriteLine ($"{Tags[1]}={(int)ch.Time.Begin.TotalMilliseconds}");
          osm.WriteLine ($"{Tags[2]}={(int)ch.Time.End.TotalMilliseconds}");
          osm.WriteLine ($"{Tags[3]}={escape (ch.Name)}");
        }
      }
      return true;
    }

    private void parseChapter (string s, ref int i) {
      if (_chapter is null)
        return;

      if (i >= Tags.Length)
        return;

      string arg = findArg (Tags[i], s);
      if (!(arg is null)) {
        switch (i) {
          case 0: {
              int pos = arg.IndexOf ('/');
              if (pos > 0) {
                double denom = 1, nom = 1;
                bool succ = double.TryParse (arg.Substring (0, pos), out denom);
                succ = succ && double.TryParse (arg.Substring (pos + 1), out nom);
                if (succ)
                  _timebase = new TimeBase (denom, nom);
              }
              break;
            }
          case 1: {
              bool succ = long.TryParse (arg, out long tim);
              if (succ)
                _chapter.Time.Begin = TimeSpan.FromSeconds (tim * _timebase.Denominator / _timebase.Nominator);
              break;
            }
          case 2: {
              bool succ = long.TryParse (arg, out long tim);
              if (succ)
                _chapter.Time.End = TimeSpan.FromSeconds (tim * _timebase.Denominator / _timebase.Nominator);
              break;
            }
          case 3:
            _chapter.Name = unescape (arg);
            break;
        }
        i++;
      }
    }

    private string findArg (string tag, string s) {
      string key = tag + "=";
      if (s.StartsWith (key, StringComparison.InvariantCultureIgnoreCase) && s.Length > key.Length)
        return s.Substring (key.Length);
      return null;
    }


    private void terminateChapter () {
      if (!(_chapter is null)) {
        Chapters.Add (_chapter);
        _chapter = null;
      }
    }

    private string escape (string s) {
      if (s is null)
        return null;
      var chars = new List <char>();
      foreach (char c in s) {
        if (Escapes.Contains(c))
          chars.Add ('\\');
        chars.Add (c);
      }
      return new string (chars.ToArray());
    }

    private string unescape (string s) {
      if (s is null)
        return null;
      var chars = new List <char>();
      bool bsl = false;
      foreach (char c in s) { 
        if (c == '\\' && !bsl) {
          bsl = true;
          continue;
        }
        chars.Add (c);
        bsl = false;
      }
      return new string (chars.ToArray());
    }
  }
}
