using System.Text;
using audiamus.aux.ex;

namespace audiamus.aaxconv {

  interface ISettings : lib.ISettings, aux.win.ILanguageSetting {
    void Save ();
    bool? FileAssoc { get; set; }
  }


  namespace Properties {
    [System.Configuration.SettingsProvider (typeof (aux.LocalFileSettingsProvider))]
    partial class Settings : ISettings {

      public override string ToString () {
        var sb = new StringBuilder ();
        sb.AppendLine ("Settings:");
        sb.AppendLine ($"  {nameof (lib.ISettings)}");
        sb.AppendLine ($"    {nameof (NonParallel)}={NonParallel}");
        sb.AppendLine ($"    {nameof (InputDirectory)}={InputDirectory.SubstitUser()}");
        sb.AppendLine ($"    {nameof (OutputDirectory)}={OutputDirectory.SubstitUser()}");
        sb.AppendLine ($"    {nameof (FFMpegDirectory)}={FFMpegDirectory.SubstitUser()}");
        sb.AppendLine ($"    {nameof (ConvFormat)}={ConvFormat}");
        sb.AppendLine ($"    {nameof (ConvMode)}={ConvMode}");
        sb.AppendLine ($"    {nameof (TrkDurMins)}={TrkDurMins}");
        sb.AppendLine ($"    {nameof (PartNames)}={PartNames}");
        sb.AppendLine ($"    {nameof (Genres)}={Genres}");
        sb.AppendLine ($"    {nameof (FlatFolders)}={FlatFolders}");
        sb.AppendLine ($"    {nameof (FlatFolderNaming)}={FlatFolderNaming}");
        sb.AppendLine ($"    {nameof (ShortChapterSec)}={ShortChapterSec}");
        sb.AppendLine ($"    {nameof (VeryShortChapterSec)}={VeryShortChapterSec}");
        sb.AppendLine ($"    {nameof (Latin1EncodingForPlaylist)}={Latin1EncodingForPlaylist}");
        sb.AppendLine ($"    {nameof (AutoLaunchPlayer)}={AutoLaunchPlayer}");

        sb.AppendLine ($"  {nameof (lib.ISettings)}:{nameof (lib.INamingSettingsEx)}");
        sb.AppendLine ($"    {nameof (PartNaming)}={PartNaming}");
        sb.AppendLine ($"    {nameof (PartName)}={PartName}");
        sb.AppendLine ($"    {nameof (ExtraMetaFiles)}={ExtraMetaFiles}");
        sb.AppendLine ($"    {nameof (NamedChaptersAndAlwaysWithNumbers)}={NamedChaptersAndAlwaysWithNumbers}");
        sb.AppendLine ($"    {nameof (M4B)}={M4B}");

        sb.AppendLine ($"  {nameof (lib.ISettings)}:{nameof (lib.INamingSettingsEx)}:{nameof (lib.INamingSettings)}");
        sb.AppendLine ($"    {nameof (FileNaming)}={FileNaming}");
        sb.AppendLine ($"    {nameof (TitleNaming)}={TitleNaming}");
        sb.AppendLine ($"    {nameof (TrackNumbering)}={TrackNumbering}");
        sb.AppendLine ($"    {nameof (Narrator)}={Narrator}");
        sb.AppendLine ($"    {nameof (GenreNaming)}={GenreNaming}");
        sb.AppendLine ($"    {nameof (GenreName)}={GenreName}");
        sb.AppendLine ($"    {nameof (ChapterNaming)}={ChapterNaming}");
        sb.AppendLine ($"    {nameof (ChapterName)}={ChapterName}");

        sb.AppendLine ($"  {nameof (lib.ISettings)}:{nameof (lib.INamingSettingsEx)}:{nameof (lib.INamingSettings)}:{nameof (lib.ITitleNamingSettings)}");
        sb.AppendLine ($"    {nameof (SeriesTitleLeft)}={SeriesTitleLeft}");
        sb.AppendLine ($"    {nameof (LongBookTitle)}={LongBookTitle}");

        sb.AppendLine ($"  {nameof (lib.ISettings)}:{nameof (lib.IActivationSettings)}");
        sb.AppendLine ($"    {nameof (ActivationCode)}={(ActivationCode.HasValue ? "XXXXXXXX" : null)}");

        sb.AppendLine ($"  {nameof (lib.ISettings)}:{nameof (lib.IAaxCopySettings)}");
        sb.AppendLine ($"    {nameof (AaxCopyMode)}={AaxCopyMode}");
        sb.AppendLine ($"    {nameof (AaxCopyDirectory)}={AaxCopyDirectory}");

        sb.AppendLine ($"  {nameof (lib.ISettings)}:{nameof (lib.IUpdateSetting)}");
        sb.AppendLine ($"    {nameof (OnlineUpdate)}={OnlineUpdate}");

        sb.AppendLine ($"  {nameof (aux.win.ILanguageSetting)}");
        sb.Append     ($"    {nameof (Language)}={Language}");

        return sb.ToString ();
      }
    }

  }
}
