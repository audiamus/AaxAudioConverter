using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using audiamus.aux;
using audiamus.aux.w32;
using audiamus.aux.win;
using static audiamus.aux.ApplEnv;

namespace audiamus.aaxconv {

  using R = Properties.Resources;

  class FileAssoc {
    readonly IAppSettings _settings;
    readonly IEnumerable<string> _extensions = new[] { ".aax", ".aa" };
    readonly string _fileDesc;
    readonly Assembly _assembly;
    readonly Control _owner;

    private IAppSettings Settings => _settings;

    public FileAssoc (IAppSettings settings, Control owner) {
      _settings = settings;
      _assembly = EntryAssembly;
      _owner = owner;

      var control = _owner;
      while (!(control.Parent is null))
        control = control.Parent;
      _fileDesc = control.Text;
    }

    public void AssociateInit () {
      if (Settings.FileAssoc.HasValue && !Settings.FileAssoc.Value)
        return;

      if (!Settings.FileAssoc.HasValue) {

        string msg = $"{R.MsgFileAssoc} {ApplName}?";
        var result = MsgBox.Show (_owner, msg, _fileDesc, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        Settings.FileAssoc = result == DialogResult.Yes;
        if (result != DialogResult.Yes)
          return;
      }

      Associate ();
    }

    public void Update () {
      if (Settings.FileAssoc ?? false)
        Associate ();
      else
        Remove ();
    }

    public void Associate () => FileAssociations.EnsureAssociationsSet (_extensions, _fileDesc, _assembly);

    public void Remove () => FileAssociations.RemoveAssociationsSet (_extensions, _assembly);

  }
}

