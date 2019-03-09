using System.Text;

namespace audiamus.aux.win {
  public class RtfBuilder {
    StringBuilder _builder = new StringBuilder ();

    public void AppendBold (string text) {
      _builder.Append (@"{\b ");
      _builder.Append (text);
      _builder.Append (@"}");
    }

    public void AppendItalic (string text) {
      _builder.Append (@"{\i ");
      _builder.Append (text);
      _builder.Append (@"}");
    }

    public void Append (string text) {
      _builder.Append (text);
    }

    public void AppendLine () {
      _builder.Append (@"\line ");
    }

    public void AppendPara () {
      _builder.Append (@"\par ");
    }

    public string ToRtf () {
      return @"{\rtf1\ansi " + _builder.ToString () + @" }";
    }
  }
}
