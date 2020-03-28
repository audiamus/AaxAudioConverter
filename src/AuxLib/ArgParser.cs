using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace audiamus.aux {

  public class ArgParser {
    readonly string[] _args;
    readonly bool _ignoreCase;

    public ArgParser (string[] args) {
      _args = args;
    }

    public ArgParser (string[] args, bool ignoreCase) {
      _args = args;
      this._ignoreCase = ignoreCase;
    }

    public void Log () {
      foreach (string arg in _args)
        Logging.Log (1, arg);
    }

    public virtual bool Exists (string tag) {
      if (_args == null)
        return false;
      string key = "-" + tag;
      foreach (string arg in _args) {
        if (arg.StartsWith (key, _ignoreCase, CultureInfo.InvariantCulture))
          return true;
      }
      return false;
    }

    public virtual string FindArg (string tag) {
      string erg = null;
      if (_args == null)
        return erg;

      string key = "-" + tag + "=";
      foreach (string arg in _args) {
        if (arg.StartsWith (key, _ignoreCase, CultureInfo.InvariantCulture)) {
          if (arg.Length > key.Length) {
            erg = arg.Substring (key.Length, arg.Length - key.Length);
            break;
          }
        }
      }
      return erg;
    }

    public virtual bool HasArg (string tag) {
      if (_args == null)
        return false;

      string key = "-" + tag;
      return _args.Where (x => x.StartsWith (key, StringComparison.InvariantCultureIgnoreCase)).Any ();
    }

    public string FindArg (string tag, string defaultArgVal) {
      string arg = FindArg (tag);
      if (arg == null || arg.Length == 0)
        return defaultArgVal;
      else
        return arg;
    }



    public bool? FindBooleanArg (string tag) {
      string arg = FindArg (tag);
      if (arg == null || arg.Length == 0)
        return null;

      bool value = string.Equals (arg, "true", StringComparison.InvariantCultureIgnoreCase) || arg == "1";
      return value;
    }

    public bool FindBooleanArg (string tag, bool defaultArgVal) {
      bool? arg = FindBooleanArg (tag);
      if (arg == null)
        return defaultArgVal;
      else
        return arg.Value;
    }

    public int? FindIntArg (string tag) {
      string arg = FindArg (tag);
      if (arg == null || arg.Length == 0)
        return null;

      if (!int.TryParse (arg, out int result))
        return null;

      return result;
    }

    public int FindIntArg (string tag, int defaultArgVal) {
      int? arg = FindIntArg (tag);
      if (arg == null)
        return defaultArgVal;
      else
        return arg.Value;
    }

    public uint? FindUIntArg (string tag) {
      string arg = FindArg (tag);
      if (arg == null || arg.Length == 0)
        return null;

      if (!uint.TryParse (arg, out uint result))
        return null;

      return result;
    }

    public uint FindUIntArg (string tag, uint defaultArgVal) {
      uint? arg = FindUIntArg (tag);
      if (arg == null)
        return defaultArgVal;
      else
        return arg.Value;
    }

    public double? FindFloatArg (string tag) {
      string arg = FindArg (tag);
      if (arg == null || arg.Length == 0)
        return null;

      if (!double.TryParse (arg, out double result))
        return null;

      return result;
    }

    public double FindFloatArg (string tag, double defaultArgVal) {
      double? arg = FindFloatArg (tag);
      if (arg == null)
        return defaultArgVal;
      else
        return arg.Value;
    }

    public E? FindEnumArg<E> (string tag) where E : struct, Enum {
      string arg = FindArg (tag);
      if (arg == null || arg.Length == 0)
        return null;

      if (!Enum.TryParse<E> (arg, out E result))
        return null;

      return result;

    }

    public E FindEnumArg<E> (string tag, E defaultArgVal) where E : struct, Enum {
      E? arg = FindEnumArg<E> (tag);
      if (arg == null)
        return defaultArgVal;
      else
        return arg.Value;
    }
  }
}
