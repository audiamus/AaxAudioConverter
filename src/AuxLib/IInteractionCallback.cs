namespace audiamus.aux {
  public interface IInteractionCallback<T, out TResult> {
    TResult Interact (T value);
  }
}
