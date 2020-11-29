using System;
using System.IO;
using System.Windows.Forms;
using audiamus.aaxconv.lib;
using audiamus.aux;
using audiamus.aux.win;

namespace audiamus.aaxconv {
  using System.ComponentModel;
  using R = Properties.Resources;

  partial class FFmpegLocationForm : Form {
    readonly IAppSettings _settings = Properties.Settings.Default;
    readonly string _origFFMpegDirectory;
    readonly bool? _relaxed;

    readonly AaxAudioConverter _converter;
    readonly Func<InteractionMessage, bool?> _callback;

    private IAppSettings Settings => _settings;


    public FFmpegLocationForm (AaxAudioConverter converter, Func<InteractionMessage, bool?> callback, bool? relaxed = null) {
      InitializeComponent ();
      _converter = converter;
      _callback = callback;
      _relaxed = relaxed;
      _origFFMpegDirectory = Settings.FFMpegDirectory;
      textBoxLocation.Text = _origFFMpegDirectory;
    }

    protected override void OnClosing (CancelEventArgs e) {
      if (DialogResult == DialogResult.Cancel)
        Settings.FFMpegDirectory = _origFFMpegDirectory;
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
        Settings.FFMpegDirectory = ffmpegdir;

      bool succ = _converter.VerifyFFmpegPathVersion(_callback, _relaxed);

      if (succ)
        enableOK ();
    }

    private void btnReset_Click (object sender, EventArgs e) {
      string ffmpegdir = ApplEnv.ApplDirectory;
      string path = Path.Combine (ffmpegdir, FFmpeg.FFMPEG_EXE);
      if (File.Exists (path)) {
        Settings.FFMpegDirectory = ffmpegdir;
        using (new ResourceGuard (() => Settings.FFMpegDirectory = _origFFMpegDirectory)) {
          bool succ = _converter.VerifyFFmpegPathVersion (_callback);
          if (succ) {
            textBoxLocation.Text = null;
            enableOK ();
          }
        }
      }
    }

    private void btnOK_Click (object sender, EventArgs e) {
      Settings.FFMpegDirectory = string.IsNullOrWhiteSpace(textBoxLocation.Text) ? null : textBoxLocation.Text;

      if (string.Equals (Settings.FFMpegDirectory, ApplEnv.ApplDirectory, StringComparison.InvariantCultureIgnoreCase))
        Settings.FFMpegDirectory = null;

      DialogResult = DialogResult.OK;
    }

    private void enableOK () {
      //"Location and verification successful"
      MsgBox.Show (this, R.MsgLocationVerifSucc, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
      btnOK.Enabled = true;
      this.AcceptButton = btnOK;
      this.ActiveControl = btnOK;
    }

  }
}
