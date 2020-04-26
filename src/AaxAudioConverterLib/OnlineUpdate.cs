using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using audiamus.aux;
using audiamus.aux.ex;
using static audiamus.aux.Logging;

namespace audiamus.aaxconv.lib {
  public class OnlineUpdate {

    const string SETUP_REF_URI = "https://raw.githubusercontent.com/audiamus/AaxAudioConverter/master/res/Setup.dat";

    private static readonly Regex _rgxMd5 = new Regex (@"^(?:MD5:\s)?([A-Fa-f\d]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex _rgxVersion = new Regex (@"/AaxAudioConverter-(\d+(?:\.\d+){1,3})-Setup", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private string _downloadUri;
    private string _setupFile;
    private string _md5;
    private Version _version = new Version ();

    private static HttpClient HttpClient { get; } = new HttpClient ();
    private static string DownloadDir { get; }

    private IUpdateSetting Settings { get; }
    private IResources R { get; }

    static OnlineUpdate () {
      string downloads = Environment.ExpandEnvironmentVariables (@"%USERPROFILE%\Downloads");
      if (string.IsNullOrWhiteSpace (downloads))
        downloads = ApplEnv.TempDirectory;
      DownloadDir = downloads;
    }

    public OnlineUpdate(IUpdateSetting settings, IResources resources) {
      Settings = settings;
      R = resources;
    }

    public async Task UpdateAsync (
      IInteractionCallback<InteractionMessage, bool?> interactCallback,
      Action finalCallback,
      Func<bool> busyCallback
    ) {
      if (!Settings.OnlineUpdate.HasValue)
        return;

      await getSetupRefAsync ();
 
      // do we have a new version?
      bool newVersion = _version > ApplEnv.AssemblyVersion;
      if (!newVersion)
        return;

      // do we have it downloaded already?
      bool exists = await checkDownloadAsync ();

      if (!exists) {
        if (!Settings.OnlineUpdate.Value) {
          string msg1 = $"{R.MsgOnlineUpdateNewVersion} {_version} {R.MsgOnlineUpdateDownload}";
          bool? result1 = interactCallback.Interact (new InteractionMessage { Message = msg1, Type = ECallbackType.question });
          if (!(result1.Value))
            return;
        }

        // no: download,  verify md5
        await downloadSetupAsync ();
      }

      bool isBusy = busyCallback ();
      if (isBusy)
        return;

      bool cont = install (interactCallback, R.MsgOnlineUpdateInstallNow);
      if (!cont)
        return;

      finalCallback?.Invoke ();
    }

    public async Task InstallAsync (IInteractionCallback<InteractionMessage, bool?> interactCallback, Action finalCallback) {
      if (!Settings.OnlineUpdate.HasValue)
        return;

      await getSetupRefAsync ();

      // do we have a new version?
      bool newVersion = _version > ApplEnv.AssemblyVersion;
      if (!newVersion)
        return;

      // do we have it downloaded already?
      bool exists = await checkDownloadAsync ();
      if (!exists)
        return;

      install (interactCallback, R.MsgOnlineUpdateInstallLater);

      finalCallback?.Invoke ();
    }

    private bool install (IInteractionCallback<InteractionMessage, bool?> interactCallback, string prompt) {
      string msg2 =
        $"{R.MsgOnlineUpdateNewVersion} {_version} {R.MsgOnlineUpdateInstall}" +
        $"{Environment.NewLine}{prompt}";
      bool? result2 = interactCallback.Interact (new InteractionMessage { Message = msg2, Type = ECallbackType.question });
      if (!(result2 ?? true))
        return false;

      // launch installer
      try {
        Process.Start (_setupFile);
      } catch (Exception) {
      }

      return true;
    }

    private async Task getSetupRefAsync () {
      string result = null;

      try {
        using (HttpResponseMessage response = await HttpClient.GetAsync (SETUP_REF_URI))
          using (HttpContent content = response.Content)
            result = await content.ReadAsStringAsync ();

        if (string.IsNullOrWhiteSpace (result))
          return;

        using (var textReader = new StringReader (result)) {
          _downloadUri = textReader.ReadLine ();

          Match match = _rgxVersion.Match (_downloadUri);
          if (match.Success)
            _version = new Version (match.Groups[1].Value);

          string file = Path.GetFileName (_downloadUri);
          _setupFile = Path.Combine (DownloadDir, file);

          string md5 = textReader.ReadLine ();

          match = _rgxMd5.Match (md5);
          if (match.Success)
            _md5 = match.Groups[1].Value;

        }
      } catch (Exception exc) {
        Log (1, this, () => $"{exc.ToShortString ()}{Environment.NewLine}  \"{result}\"");
      }
    }

    private async Task<bool> checkDownloadAsync () {
      if (!File.Exists (_setupFile))
        return false;

      string md5 = await computeMd5ForFileAsync (_setupFile);
      bool succ = string.Equals (_md5, md5, StringComparison.InvariantCultureIgnoreCase);
      return succ;
    }

    private async Task<string> computeMd5ForFileAsync (string filePath) {
      return await Task.Run (() => computeMd5HashForFile (filePath));
    }

    private string computeMd5HashForFile (string filePath) {
      using (var md5 = MD5.Create ()) {
        using (var stream = File.OpenRead (filePath)) {
          var hash = md5.ComputeHash (stream);
          return BitConverter.ToString (hash).Replace ("-", "").ToLowerInvariant ();
        }
      }
    }


    private async Task<bool> downloadSetupAsync () {
      await downloadAsync (_downloadUri, _setupFile);
      return await checkDownloadAsync ();
    }


    private async Task downloadAsync (string requestUri, string filename) {
      try {
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        using (var fileStream = File.OpenWrite (filename)) {
          using (var networkStream = await HttpClient.GetStreamAsync (requestUri)) {
            await networkStream.CopyToAsync (fileStream);
            await fileStream.FlushAsync ();
          }
        }
      } catch (Exception) { }
    }

  }
}
