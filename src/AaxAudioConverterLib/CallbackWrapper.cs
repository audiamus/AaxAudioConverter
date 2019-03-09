using System;
using System.Threading;
using audiamus.aux;

namespace audiamus.aaxconv.lib {

  class CallbackWrapper {
    readonly CancellationToken _token;
    readonly IProgress<ProgressMessage> _progress;
    readonly IInteractionCallback<InteractionMessage, bool?> _interaction;

    public CancellationToken CancellationToken => _token;
    public bool Cancelled => _token != null ? _token.IsCancellationRequested : false;  

    public CallbackWrapper (CancellationToken token, IProgress<ProgressMessage> progress, IInteractionCallback<InteractionMessage, bool?> interaction = null) {
      _token = token;
      _progress = progress;
      _interaction = interaction;
    }

    public bool Cancel () {
      return _token != null ? _token.IsCancellationRequested : false;
    }

    public void Progress (ProgressMessage msg) {
      _progress?.Report (msg);
    }

    public bool? Interact (string msg, ECallbackType type) {
      return Interact (new InteractionMessage { Message = msg, Type = type });
    }

    public bool? Interact (InteractionMessage msg) {
      return _interaction?.Interact (msg);
    }

    public bool? Interact (EInteractionCustomCallback type) {
      return _interaction?.Interact (new InteractionMessage<EInteractionCustomCallback> { Type = ECallbackType.custom, Custom = type });
    }

  }
}
