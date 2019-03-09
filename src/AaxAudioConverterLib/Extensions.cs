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
}
