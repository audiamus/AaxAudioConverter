using audiamus.aux;

namespace audiamus.aaxconv.lib {
  public class ChainPunctuationBracket : ChainPunctuation {
    public override string Prefix => "<";
    public override string Suffix => ">";
    public override string[] Infix => null;
  }

  public class ChainPunctuationDash : ChainPunctuationBracket {
    public override string[] Infix => new[] { " - " };
  }

  public class ChainPunctuationDot : ChainPunctuationBracket {
    public override string[] Infix => new[] { "." , "(", ")"};
  }
}
