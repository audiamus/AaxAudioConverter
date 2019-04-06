namespace audiamus.aux {
  public interface IChainPunctuation {
    string Prefix { get; }
    string Suffix { get; }
    string[] Infix { get; }
    //string[] Punctuation { get; }
  }

  public abstract class ChainPunctuation : IChainPunctuation {
    public abstract string Prefix { get; }
    public abstract string Suffix { get; }
    public abstract string[] Infix { get; }
    //public string[] Punctuation => new string[] { Prefix, Suffix, Infix };
  }
}
