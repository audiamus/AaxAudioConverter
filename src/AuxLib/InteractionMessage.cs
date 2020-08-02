namespace audiamus.aux {
  public enum ECallbackType { info, infoCancel, warning, error, errorQuestion, errorQuestion3, question, question3, custom}

  public class InteractionMessage {
    public ECallbackType Type;
    public string Message;
  }

  public class InteractionMessage<T> : InteractionMessage where T: struct, System.Enum {
    public T Custom;
  }

}
