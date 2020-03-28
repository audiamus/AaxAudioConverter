using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static audiamus.aux.ApplEnv;

namespace audiamus.aux {

  public class Logging {

    #region Nested Classes

    class LogMessage {
      public DateTime DateTime { get; private set; }
      public int ThreadId { get; private set; }
      public string Context { get; private set; }
      public string Message { get; private set; }

      public LogMessage (string message) :
        this (null, message) { }

      public LogMessage (string context, string message) :
        this (DateTime.Now, Environment.CurrentManagedThreadId, context, message) { }

      public LogMessage (DateTime timestamp, int threadId, string context, string message) {
        DateTime = timestamp;
        ThreadId = threadId;
        Context = context;
        Message = message;
      }
    }


    #endregion Nested Classes
    #region singleton
    //private static Logger __instance;
    //private static Logger Instance {
    //  get
    //  {
    //    if (__instance is null)
    //      __instance = new Logger ();
    //    return __instance;
    //  }
    //}

    private static Logging Instance { get; } = new Logging ();

    #endregion singleton
    #region Private Fields
    const string EXT = ".log";
    public const long DefaultFileSize = 20 * 1024 * 1024;


    private readonly object _lockable = new object ();
    private bool _instantFlush;
    private bool _fullClassNames;
    //private bool _initDone;
    private int _level = -1;
    private string _currentfilename;
    private uint _filecount;
    private DateTime _filedate;
    //private long _filesize = DefaultFileSize;
    private string _filestub;
    private bool _ignoreExisting;
    //private bool _useDefaultTraceListener = true;
    private StreamWriter _logStreamWriter;
    private System.Threading.Timer _flushTimer;
    private uint _linecount;
    private bool _logfileLocationOutputDone;


    #endregion Private Fields
    #region Public Properties

    public static int Level {
      get => Instance._level;
      set => Instance.setLevel (value);
    }

    public static bool InstantFlush {
      get => Instance._instantFlush; 
      set => Instance._instantFlush = value;
    }

    public static bool FullClassNames {
      get => Instance._fullClassNames;
      set => Instance._fullClassNames = value;
    }


    #endregion Public Properties
    #region Private Properties

    private static long FileSize => DefaultFileSize;

    private TextWriter Writer => _logStreamWriter;


    #endregion Private Properties
    #region ctor

    // cannot instatiate from outside class
    private Logging () => setFileNameStub ();

    #endregion ctor
    #region Public Methods


    public static void Log0 (uint level, object caller, [CallerMemberName] string method = null) => Instance.log0 (level, caller, method);

    public static void Log0 (uint level, Type caller, [CallerMemberName] string method = null) => Instance.log0 (level, caller, method);

    public static void Log (uint level, object caller, string what, [CallerMemberName] string method = null) => Instance.log (level, caller, what, method);

    public static void Log (uint level, Type caller, string what, [CallerMemberName] string method = null) => Instance.log(level, caller, what, method);

    public static void Log (uint level, object caller, Func<string> getWhat, [CallerMemberName] string method = null) => Instance.log (level, caller, getWhat, method);

    public static void Log (uint level, Type caller, Func<string> getWhat, [CallerMemberName] string method = null) => Instance.log(level, caller, getWhat, method);

    public static void Log (uint level, string msg) => Log (level, null, msg);

    public static void Log (uint level, string context, string msg) => Instance.log (level, context, msg);

    #endregion Public Methods

    #region Private Methods

    private void setLevel (int value) {
      {
        if (value >= 0) {
          _level = value;
          log ($"{nameof (Level)}={_level}");
        }
      }
    }

    private void log0 (uint level, object caller, [CallerMemberName] string method = null) {
      if (level <= _level)
        log (level, context (caller, method), null);
    }

    private void log0 (uint level, Type caller, [CallerMemberName] string method = null) {
      if (level <= _level)
        log (level, context (caller, method), null);
    }

    private void log (uint level, object caller, string what, [CallerMemberName] string method = null) {
      if (level <= _level)
        log (level, context (caller, method), what);
    }

    private void log (uint level, Type caller, string what, [CallerMemberName] string method = null) {
      if (level <= _level)
        log (level, context (caller, method), what);
    }

    private void log (uint level, object caller, Func<string> getWhat, [CallerMemberName] string method = null) {
      if (level <= _level && !(getWhat is null))
        log (level, context (caller, method), getWhat ());
    }

    private void log (uint level, Type caller, Func<string> getWhat, [CallerMemberName] string method = null) {
      if (level <= _level && !(getWhat is null))
        log (level, context (caller, method), getWhat());
    }

    private void log (uint level, string context, string msg) {
      if (level <= _level)
        log (context, msg);
    }

    private void log (string msg) => log (null, msg);

    private void log (string context, string msg) => handleWrite (new LogMessage (context, msg));


    private static string context (object caller, string method) => context (caller.GetType (), method);

    private static string context (string method) => $"???.{method}";

    private static string context (Type caller, string method) {
      if (FullClassNames)
        return $"{caller.FullName}.{method}";
      else
        return $"{caller.Name}.{method}";
    }

    private void handleWrite (LogMessage logMessage) {
      ensureWriter ();
      write (logMessage);
    }

    private void ensureWriter () {
      // Do we have a stream writer?
      lock (_lockable) {
        if (_logStreamWriter is null) {
          openWriter (true);
        } else {
          if (DateTime.Now.Date != _filedate.Date) {
            nextWriter (true);
          } else if (_logStreamWriter.BaseStream.Position >= FileSize) {
            nextWriter (false);
          }
        }
      }
    }

    private void nextWriter (bool newDay) {
      close ();
      openWriter (newDay);
    }

    private void close () {
      closeFlushTimer ();
      closeWriter ();
    }

    private void closeFlushTimer () {
      if (_flushTimer != null)
        _flushTimer.Dispose ();
      _flushTimer = null;
    }

    private void closeWriter () {
      string prevfilename = _currentfilename;
      if (!(_logStreamWriter is null)) {
        _logStreamWriter.Dispose ();
      }
      _logStreamWriter = null;
    }

    private void openWriter (bool newDay) {
      if (newDay) {
        _filedate = DateTime.Today.Date;
        _filecount = 0;
        _ignoreExisting = false;
      }

      string stub = $"{_filestub}_{_filedate:yyyy-MM-dd}_";
      string ext = EXT;

      var filenames = getExisting (stub);

      string filename = null;
      while (true) {
        // next file, theoretically
        _filecount++;

        // build a filename
        filename = $"{stub}{_filecount:000}{ext}";

        bool exists = filenames?.Where (n => filename.ToLower ().IndexOf (n) >= 0).Any () ?? false;

        if (exists && !_ignoreExisting) {
          if (_filecount < 1000)
            continue;
          _ignoreExisting = true;
          _filecount = 1;
        }


        bool succ = openWriter (filename);
        if (succ)
          break;
      }

      if (!_logfileLocationOutputDone) {
        _logfileLocationOutputDone = true;
        Console.WriteLine ($"{typeof(Logging).Name} written to \"{filename}\".");
      }
    }

    private IEnumerable<string> getExisting (string stub) {
      string folder = Path.GetDirectoryName (stub);

      if (!Directory.Exists (folder))
        return null;

      string filestub = Path.GetFileNameWithoutExtension (stub);

      string search = $"{filestub}*{EXT}";
      string[] files = Directory.GetFiles (folder, search);
      var names = files.Select (f => Path.GetFileName (f.ToLower ()));
      return names;
    }

    private bool openWriter (string filename) {
      FileMode createOption = _ignoreExisting ? FileMode.Create : FileMode.CreateNew;

      string folder = Path.GetDirectoryName (filename);
      filename = Path.GetFileName (filename);
      if (string.IsNullOrEmpty (folder))
        folder = LogDirectory;
      filename = Path.Combine (folder, filename);

      Directory.CreateDirectory (folder);

      Stream stream = new FileStream (filename, createOption, FileAccess.ReadWrite);
      _logStreamWriter = new StreamWriter (stream);
      _currentfilename = filename;

      if (!InstantFlush)
        openFlushTimer ();
      return true;
    }

    private void openFlushTimer () {
      _flushTimer = new System.Threading.Timer (flushTimerCallback, null, 5000, 5000);
    }

    private void write (LogMessage msg) {
      string s = format (msg);
      lock (_lockable) {
        Writer.WriteLine (s);
        if (InstantFlush)
          Writer.Flush ();
        else
          _linecount++;
      }
    }

    private void flushTimerCallback (object state) {
      lock (_lockable) {
        if (_linecount > 0)
          Writer.Flush ();
        _linecount = 0;
      }
    }

    private static string format (LogMessage msg) {
      string ctx = string.IsNullOrWhiteSpace (msg.Context) ? string.Empty : $"[{msg.Context}] ";
      string s = $"{msg.DateTime:HH:mm:ss.fff} {msg.ThreadId:0000} {ctx}{msg.Message}";
      return s;
    }

    private void setFileNameStub () {
      _filecount = 0;
      _filedate = DateTime.Today;
      _filestub = Path.Combine (LogDirectory, ApplName);
    }

    #endregion Private Methods

  }
}
