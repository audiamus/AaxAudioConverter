namespace audiamus.aaxconv {

  interface ISettings : lib.ISettings, aux.win.ILanguageSetting {
    void Save ();
    bool? FileAssoc { get; set; }
  }


  namespace Properties {
    [System.Configuration.SettingsProvider (typeof (aux.LocalFileSettingsProvider))]
    partial class Settings : ISettings {
    }

  }
}
