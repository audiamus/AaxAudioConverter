using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using audiamus.aux.ex;
using static audiamus.aux.ApplEnv;

namespace audiamus.aux.win {

  public interface ILanguageSetting {
    string Language { get; set; }
  }

  public static class Culture {
    class CIItem {
      public CultureInfo CultureInfo { get; private set; }
      private string PlaceholderName { get; set; }

      public CIItem (CultureInfo ci) {
        this.CultureInfo = ci;
      }

      public CIItem (string placeholderName) {
        PlaceholderName = $"<{placeholderName}>";
      }

      public override string ToString () {
        return CultureInfo?.NativeName ?? PlaceholderName;
      }
    }

    private static CultureInfo __defaultUICulture = CultureInfo.CurrentUICulture;

    public static void Init (ILanguageSetting setting) {
      try {
        if (!string.IsNullOrWhiteSpace (setting.Language))
          CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo (setting.Language);
      } catch (Exception) {

      }
    }

    public static void SetCultures (this ComboBox combobox, Type type, ILanguageSetting setting) {
      ResourceManager rm = type.GetDefaultResourceManager();

      var ass = EntryAssembly;
      var exeDir = ApplDirectory;

      IEnumerable<CultureInfo> cultures =
          CultureInfo.GetCultures (CultureTypes.NeutralCultures).Where (c =>
            {
              var satDir = Path.Combine (exeDir, c.TwoLetterISOLanguageName);
              return Directory.Exists (satDir) &&
                new DirectoryInfo (satDir).GetFiles ("*.resources.dll").Count () > 0;
            });

      if (!string.IsNullOrWhiteSpace (NeutralCultureName)) {
        var neutralCulture = new[] { new CultureInfo (NeutralCultureName) };
        cultures = cultures.Union (neutralCulture);
      }
      
      combobox.Items.Clear ();
      combobox.Items.Add (new CIItem (rm.GetStringEx("automatic")));

      var sortedCultures = cultures.ToList ();
      sortedCultures.Sort ((x, y) => x.ToString ().CompareTo (y.ToString ()));

      foreach (CultureInfo culture in sortedCultures)
        combobox.Items.Add (new CIItem (culture));


      if (string.IsNullOrWhiteSpace(setting.Language)) {
        combobox.SelectedIndex = 0;
        return;
      }

      var idx = sortedCultures.FindIndex (c => c.Name == setting.Language);

      if (idx < 0)
        combobox.SelectedIndex = 0;
      else
        combobox.SelectedIndex = idx + 1;
    }

    public static bool ChangeLanguage (this ComboBox combobox, ILanguageSetting setting) {
      if (combobox.SelectedItem is CIItem cii) {
        if (cii.CultureInfo?.Name == setting.Language || (cii.CultureInfo is null && string.IsNullOrWhiteSpace(setting.Language)))
          return false;
        ChangeLanguage (combobox.SelectedItem, setting);
        return true;
      }
      return false;
    }

    public static void ChangeLanguage (object item, ILanguageSetting setting) {
      if (item is CIItem cii) {
        changeLanguage (cii.CultureInfo);
        setting.Language = cii.CultureInfo?.Name;
      }
    }

    private static void changeLanguage (CultureInfo ci) {
      if (ci is null)
        ci = __defaultUICulture;
      
      Thread.CurrentThread.CurrentUICulture = ci;
      CultureInfo.DefaultThreadCurrentUICulture = ci;
      
      //foreach (Form frm in Application.OpenForms)
      //  localizeForm (frm);
    }

    private static void localizeForm (Form frm) {
      var manager = new ComponentResourceManager (frm.GetType ());
      manager.ApplyResources (frm, "$this");
      //FormLanguageSwitch.Instance.ChangeLanguage (frm);
      applyResources (manager, frm.Controls);
    }

    private static void applyResources (ComponentResourceManager manager, Control.ControlCollection ctls) {
      foreach (Control ctl in ctls) {
        manager.ApplyResources (ctl, ctl.Name);
        if (ctl is PropertyGrid pg)
          pg.Refresh ();
        if (ctl is ListView lv)
          foreach (var clm in lv.Columns) {
            var clmhdr = clm as ColumnHeader;
            if (clmhdr is null)
              continue;
            manager.ApplyResources (clmhdr, clmhdr.Text);

          }
        applyResources (manager, ctl.Controls);
      }
    }


  }
}
