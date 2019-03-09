using System;
using System.Collections.Generic;
using System.Linq;

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
  }
}
