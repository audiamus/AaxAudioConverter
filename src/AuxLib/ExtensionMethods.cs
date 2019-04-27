using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace audiamus.aux.ex {
  public static class ExtDirInfo {
    public static void Clear (this DirectoryInfo di) {
      foreach (DirectoryInfo dir in di.GetDirectories ()) {
        try {
          dir.Clear ();
          dir.Delete (true);
        } catch (IOException) {
        }
      }
      foreach (FileInfo file in di.GetFiles ()) {
        try {
          file.Delete ();
        } catch (IOException) {
        }
      }
    }
  }


  public static class ExInt {
    public static int Digits (this Int32 n) =>
          n == 0 ? 1 : 1 + (int)Math.Log10 (Math.Abs (n));
    public static int Digits (this UInt32 n) =>
          n == 0 ? 1 : 1 + (int)Math.Log10 (Math.Abs (n));
  }


  public static class ExString {
    public static string Combine (this IEnumerable<string> values, char separator = ';') {
      if (values is null)
        return null;
      var sb = new StringBuilder ();
      foreach (string v in values) {
        if (string.IsNullOrWhiteSpace (v))
          continue;
        if (sb.Length > 0)
          sb.Append ($"{separator} ");
        sb.Append (v);
      }
      return sb.ToString ();
    }

    public static string[] SplitTrim (this string value, char separator) => value.SplitTrim (new[] { separator });

    public static string[] SplitTrim (this string value, char[] separators = null) {
      if (string.IsNullOrWhiteSpace (value))
        return new string[0];
      if (separators is null)
        separators = new [] { ',', ';' };

      var values = value.Split (separators);
      values = values.Select (v => v.Trim ()).ToArray ();
      return values;
    }

    static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars ();

    public static string Prune (this string s, char[] invalid) {
      if (invalid is null)
        invalid = InvalidFileNameChars;
      StringBuilder sb = new StringBuilder ();
      foreach (char c in s) {
        if (invalid.Contains (c))
          continue;
        //sb.Append (',');
        else
          sb.Append (c);
      }
      return sb.ToString ();
    }

    public static string Prune (this string s) {
      string pruned = s.Prune (null);
      pruned = pruned.Trim ('.');
      return pruned;
    }

  }
}
