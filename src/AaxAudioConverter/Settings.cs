using audiamus.aaxconv.lib;

namespace audiamus.aaxconv {

  interface IAppSettings : lib.IConvSettings, aux.win.ILanguageSetting {
    bool? FileAssoc { get; set; }
    bool? ShowStartupTip { get; set; }
    bool FileDateColumn { get; set; }
    int SettingsTab { get; set; }
    
    void Save ();
    void FixNarrator ();
  }


  namespace Properties {
    [System.Configuration.SettingsProvider (typeof (aux.LocalFileSettingsProvider))]
    partial class Settings : IAppSettings {

      public bool ConvertByFileDate { get; set; }

      public void FixNarrator () {
        TagArtist = fixNarrator (TagArtist);
        TagAlbumArtist = fixNarrator (TagAlbumArtist);
        TagComposer = fixNarrator (TagComposer);
        TagConductor = fixNarrator (TagConductor);
        Narrator = null;
      }

      private ERoleTagAssignment fixNarrator (ERoleTagAssignment role) {
        bool narrator = Narrator ?? false;
        switch (role) {
          default:
            return role;
          case ERoleTagAssignment.author__narrator__:
            if (narrator)
              return ERoleTagAssignment.author_narrator;
            else
              return ERoleTagAssignment.author;
          case ERoleTagAssignment.__narrator__:
            if (narrator)
              return ERoleTagAssignment.narrator;
            else
              return ERoleTagAssignment.none;
        }

      }
    }
  }
}
