using audiamus.aux;

namespace audiamus.aaxconv.lib {
  public class ChainPunctuationDash : ChainPunctuation {
    public override string Prefix => "<";
    public override string Suffix => ">";
    public override string Infix => " - ";
  }

  public class ChainPunctuationDot : ChainPunctuation {
    public override string Prefix => "<";
    public override string Suffix => ">";
    public override string Infix => ".";
  }
}
