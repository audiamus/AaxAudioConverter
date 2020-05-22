using System;
using System.Threading;

namespace audiamus.aux {
  public static class Temp {
    public static string GetPseudoUniqueString () {
      long ticks = DateTime.UtcNow.Ticks;
      int thrdid = Thread.CurrentThread.ManagedThreadId;
      return $"{thrdid}_{ticks}";
    }
  }
}
