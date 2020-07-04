using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using audiamus.aux.ex;

namespace audiamus.aux {
  public static class EnumUtil {
    // Note: constraint System.Enum available in C# 7.3 
    public static IEnumerable<T> GetValues<T> () where T : struct, Enum {
      var values = Enum.GetValues (typeof (T));
      return values.Cast<T> ().ToList ();
    }

    // Note: constraint System.Enum available in C# 7.3 
    public static string[] GetStringValues<T> () where T : struct, Enum {
      var values = GetValues<T> ();
      return values.Select (v => $"<{v}>").ToArray ();
    }

    const char USCORE = '_';
    static readonly byte __a = Convert.ToByte ('a');

    public static string ToDisplayString<TEnum, TPunct> (this TEnum value, ResourceManager rm) 
      where TEnum: struct, Enum
      where TPunct : class, IChainPunctuation, new() 
    {


      var punct = Singleton<TPunct>.Instance;

      string sval = value.ToString ();

      //verbatim ?
      if (sval.StartsWith ("_")) {
        return rm.GetStringEx (sval.Substring (1));
      }

      string[] parts = sval.Split (USCORE);

      bool noSubstitutes = parts.Select (s => s.Length).Min () > 1;
      StringBuilder sb = new StringBuilder ();
      if (noSubstitutes) {
        for (int i = 0; i < parts.Length; i++)
          parts[i] = punct.Prefix + rm.GetStringEx (parts[i]) + punct.Suffix;
        foreach (string s in parts) {
          if (sb.Length > 0)
            sb.Append (punct.Infix?[0]);
          sb.Append (s);
        }
      } else {
        for (int i = 0; i < parts.Length; i++) {
          if (parts[i].Length > 1)
            parts[i] = punct.Prefix + rm.GetStringEx (parts[i]) + punct.Suffix;
          else {
            byte x = Convert.ToByte (parts[i][0]);
            try {
              parts[i] = punct.Infix?[x - __a];
            } catch (IndexOutOfRangeException) {
              parts[i] = string.Empty;
            }
          }
        }
        foreach (string s in parts)
          sb.Append (s);
      }
      return sb.ToString ();
    }

  }
}
