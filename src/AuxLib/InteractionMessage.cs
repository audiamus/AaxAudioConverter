namespace audiamus.aux {
  public enum ECallbackType { info, infoCancel, warning, error, errorQuestion, errorQuestion3, question, question3, custom}

  public class InteractionMessage {
    public ECallbackType Type;
    public string Message;

    public InteractionMessage () { }
    public InteractionMessage (ECallbackType type, string message) {
      Type = type;
      Message = message;
    }
  }

  public class InteractionMessage<T> : InteractionMessage where T: struct, System.Enum {
    public T Custom;
  }

  public class InteractionMessage2<T> : InteractionMessage {
    public T Custom;
  
    public InteractionMessage2 (ECallbackType type, string message, T custom) {
      Type = type;
      Message = message;
      Custom = custom;
    }
  }

}
