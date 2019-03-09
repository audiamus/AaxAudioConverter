using System;
using System.IO;
using System.Windows.Forms;
using audiamus.aaxconv.lib;
using audiamus.aux;
using audiamus.aux.win;

namespace audiamus.aaxconv {
  using R = Properties.Resources;

  partial class FFmpegLocationForm : Form {
    readonly ISettings _settings = Properties.Settings.Default;

    readonly AaxAudioConverter _converter;
    readonly Func<InteractionMessage, bool?> _callback;

    private ISettings Settings => _settings;


    public FFmpegLocationForm (AaxAudioConverter converter, Func<InteractionMessage, bool?> callback) {
      InitializeComponent ();
      _converter = converter;
      _callback = callback;

      textBoxLocation.Text = Settings.FFMpegDirectory;
    }


    private void btnLocate_Click (object sender, EventArgs e) {


      OpenFileDialog ofd = new OpenFileDialog {
        InitialDirectory = Settings.FFMpegDirectory,
        CheckFileExists = true,
        CheckPathExists = true,
        FileName = FFmpeg.FFMPEG_EXE,
        Filter = R.FilterExeFiles // "Executable files (*.exe)|*.exe" 
      };
      DialogResult result = ofd.ShowDialog ();
      if (result == DialogResult.Cancel)
        return;

      string ffmpegdir  = Path.GetDirectoryName (ofd.FileName);
      textBoxLocation.Text = ffmpegdir;
      string path = Path.Combine (ffmpegdir, FFmpeg.FFMPEG_EXE);
      if (File.Exists (path))
        FFmpeg.FFmpegDir = ffmpegdir;

      bool succ = _converter.VerifyFFmpegPathVersion(_callback);

      if (succ) {
        //"Location and verification successful"
        MsgBox.Show (this, R.MsgLocationVerifSucc, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        btnOK.Enabled = true;
        this.AcceptButton = btnOK;
      }
    }

    private void btnOK_Click (object sender, EventArgs e) {
      Settings.FFMpegDirectory = textBoxLocation.Text;
      DialogResult = DialogResult.OK;
    }
  }
}
