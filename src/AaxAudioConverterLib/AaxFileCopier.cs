using System;
using System.IO;
using System.Text;
using audiamus.aux;
using audiamus.aux.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  class AaxFileCopier {
    private readonly Book _book;
    private readonly IAaxCopySettings _settings;
    private readonly IResources _resources;

    private IResources R => _resources;
    private CallbackWrapper Callbacks { get; }

    public AaxFileCopier (Book book, IAaxCopySettings settings, IResources resources, CallbackWrapper callbacks) {
      _book = book;
      _settings = settings;
      _resources = resources;
      Callbacks = callbacks;
    }

    public void Copy (Action<ProgressMessage> report) {

      bool bookFirst = _settings.AaxCopyMode.HasFlag ((EAaxCopyMode)1);
      bool bookWithAuthor = _settings.AaxCopyMode.HasFlag ((EAaxCopyMode)2);
      bool foldersByAuthor = _settings.AaxCopyMode.HasFlag ((EAaxCopyMode)4);

      bool withParts = _book.PartsType != Book.EParts.none;

      string outdir = _settings.AaxCopyDirectory.AsUnc ();
      if (foldersByAuthor) {
        outdir = Path.Combine (outdir, _book.AuthorFile);
        Directory.CreateDirectory (outdir);
      }

      string stub;
      if (bookWithAuthor) {
        if (bookFirst)
          stub = $"{_book.TitleFile} - {_book.AuthorFile}";
        else
          stub = $"{_book.AuthorFile} - {_book.TitleFile}";
      } else
        stub = _book.TitleFile;

      foreach (var part in _book.Parts) {
        string indir = Path.GetDirectoryName (part.AaxFileItem.FileName);

        //using (new ResourceGuard (() => Callbacks.Progress (new ProgressMessage { IncSteps = 1 }))) {

          if (string.Equals (indir.StripUnc (), outdir.StripUnc (), StringComparison.InvariantCultureIgnoreCase)) {
            Log (3, this, $"same dir, skip: indir=\"{indir.SubstitUser()}\", outdir=\"{outdir.SubstitUser()}\"");
            return;
          }

          var sb = new StringBuilder (stub);
          if (withParts)
            sb.Append ($" ({R.PartNamePrefixStandard} {part.PartNumber})");

          if (part.AaxFileItem.ContentMetadataFile is null) {
            var appMetadadata = new AudibleAppContentMetadata ();
            appMetadadata.GetContentMetadata (part, true);
          }

          var cmf = part.AaxFileItem.ContentMetadataFile;
          if (!(cmf?.ASIN is null))
            sb.Append ($"_{cmf.ASIN}");

          string ext = Path.GetExtension (part.AaxFileItem.FileName);
          sb.Append (ext);
          string filename = sb.ToString ();
          string audioOutfile = Path.Combine (outdir, filename);

          try {
            Log (3, this, $"copy \"{part.AaxFileItem.FileName.SubstitUser ()}\" to \"{audioOutfile.SubstitUser ()}\"");
            FileEx.Copy (part.AaxFileItem.FileName, audioOutfile, true, report);
          } catch (Exception exc) {
            Log (1, this, exc.ToShortString ());
          }

          if (!(cmf?.Filename is null)) {
            string metafile = Path.GetFileName (cmf.Filename);
            string metaOutfile = Path.Combine (outdir, metafile);
            try {
              Log (3, this, $"copy \"{cmf.Filename.SubstitUser ()}\" to \"{metaOutfile.SubstitUser ()}\"");
              File.Copy (cmf.Filename, metaOutfile, true);
            } catch (Exception exc) {
              Log (1, this, exc.ToShortString ());
            }
          //}
        }
      }
    }
  }
}
