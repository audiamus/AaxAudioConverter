using System;

namespace audiamus.aux {


  /// <summary>
  /// var resource = new NonIDisposableResource();
  /// using {new ResourceGuard(() => resource.Close()) {
  ///   // ...
  /// }
  /// 
  /// bool flag = false;
  /// using {new ResourceGuard(x => flag = x)) {
  ///   // ...
  /// }
  /// 
  /// </summary>
  public class ResourceGuard : IDisposable {
    readonly Action _onDispose;
    readonly Action<bool> _onNewAndDispose;
    
    public ResourceGuard (Action onDispose) {
      _onDispose = onDispose;
    }

    public ResourceGuard (Action<bool> onNewAndDispose) {
      _onNewAndDispose = onNewAndDispose;
      _onNewAndDispose?.Invoke (true);
    }

    public void Dispose () {
      _onDispose?.Invoke ();
      _onNewAndDispose?.Invoke (false);
    }
  }
}
