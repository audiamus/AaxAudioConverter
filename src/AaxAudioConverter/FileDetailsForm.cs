using System.Drawing;
using System.Windows.Forms;
using audiamus.aaxconv.ex;
using audiamus.aaxconv.lib;
using audiamus.aux.ex;
using audiamus.aux.win;

namespace audiamus.aaxconv {
  using R = Properties.Resources;

  public partial class FileDetailsForm : FileItemForm {

    public FileDetailsForm () {
      InitializeComponent ();
    }


    public override void Set (AaxFileItem fileItem) {
      textBoxTitle.Text = fileItem.BookTitle;

      var rtfbld = new RtfBuilder ();

      rtfbld.AppendItem (R.HdrAuthor, fileItem.Author);
      rtfbld.AppendItem (R.HdrNarrator, fileItem.Narrator);
      rtfbld.AppendItem (R.HdrSize, $"{fileItem.FileSize / (1024 * 1024)} MB");
      rtfbld.AppendItem (R.HdrDuration, fileItem.Duration.ToStringHMS ());
      rtfbld.AppendItem (R.HdrYear, fileItem.PublishingDate?.Year.ToString ());
      rtfbld.AppendItem (R.HdrPublisher, fileItem.Publisher);
      rtfbld.AppendItem (R.HdrCopyright, fileItem.Copyright);


      string rtf = rtfbld.ToRtf ();
      richTextBoxMeta.Clear ();
      richTextBoxMeta.Rtf = rtf;

      var converter = new ImageConverter ();
      pictureBox.Image = converter.ConvertFrom (fileItem.Cover) as Image;

      richTextBoxAbstract.Clear ();
      richTextBoxAbstract.Text = fileItem.Abstract;
    }

    protected override void OnKeyDown (KeyEventArgs e) {
      if (e.KeyCode == Keys.Escape)
        Close ();
      else
        base.OnKeyDown (e);
    }

  }

  namespace ex {
    static class RtfBuilderEx {
      public static void AppendItem (this RtfBuilder rtfbld, string header, string body) {
        rtfbld.AppendBold (header);
        rtfbld.AppendLine ();
        rtfbld.Append (body);
        //rtfbld.AppendLine ();
        rtfbld.AppendPara ();
      }
    }
  }
}
