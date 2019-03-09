using System;

namespace audiamus.aux {
  /// <summary>
  /// Implementation of the "Singleton" pattern.
  /// </summary>
  /// <typeparam name="T">Type of class to be instantiated as a singleton.</typeparam>
  public class Singleton<T> where T : class, new() {
    private static T __t;
    private static readonly object __lockable;

    /// <summary>
    /// Static ctor. Initializes the <see cref="Singleton{T}"/> class, but does not yet create the instance.
    /// </summary>
    static Singleton () {
      __lockable = new object ();
    }

    /// <summary>
    /// Get the instance of the singleton. 
    /// C#-style implementation as a property. 
    /// </summary>
    /// <value>
    /// Instance of singleton.
    /// </value>
    public static T Instance {
      get {
        lock (__lockable) {
          if (__t is null)
            __t = new T ();

          return __t;
        }
      }
    }

    public static void Dispose () {
      lock (__lockable) {
        if (__t is IDisposable obj)
          obj.Dispose ();
        __t = null;
      }
    }

  }
}
