namespace audiamus.aaxconv {

  interface IAppSettings : lib.IConvSettings, aux.win.ILanguageSetting {
    void Save ();
    bool? FileAssoc { get; set; }
    bool? ShowStartupTip { get; set; }
    int SettingsTab { get; set; }
  }


  namespace Properties {
    [System.Configuration.SettingsProvider (typeof (aux.LocalFileSettingsProvider))]
    partial class Settings : IAppSettings {  }

  }
}
