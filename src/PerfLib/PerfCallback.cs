namespace audiamus.perf {
  public interface IValueMax {
    int Value { get; }
    int Max { get; }
  }

  public interface IPerfCallback {
    IValueMax Processes { get; }
    IValueMax LoadPercent { get; }
  }

  public class PerfCallback : IPerfCallback {
    public class ValueMax : IValueMax {
      public int Value { get; }
      public int Max { get; }

      public ValueMax (int val, int max = 100) {
        Value = val;
        Max = max;
      }
    }

    public IValueMax Processes { get; }
    public IValueMax LoadPercent { get; }

    public PerfCallback (IValueMax processes, IValueMax loadPercent) {
      Processes = processes;
      LoadPercent = loadPercent;
    }
  }
}
