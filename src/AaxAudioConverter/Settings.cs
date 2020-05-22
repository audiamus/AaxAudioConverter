namespace audiamus.aaxconv {

  interface IAppSettings : lib.IConvSettings, aux.win.ILanguageSetting {
    void Save ();
    bool? FileAssoc { get; set; }
  }


  namespace Properties {
    [System.Configuration.SettingsProvider (typeof (aux.LocalFileSettingsProvider))]
    partial class Settings : IAppSettings {  }

  }
}
