using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using audiamus.aux;
using audiamus.aux.ex;
using Microsoft.Win32;
using Newtonsoft.Json;
using static audiamus.aux.Logging;
using InteractMessage = audiamus.aux.InteractionMessage2<audiamus.aaxconv.lib.UpdateInteractionMessage>;

namespace audiamus.aaxconv.lib {
  public class OnlineUpdate {
    class DeclinedPackage {
      public string AppName { get; }
      public Version Version { get; }

      public DeclinedPackage (string appname, Version version) {
        AppName = appname;
        Version = version;
      }

      public DeclinedPackage (string serialized) {
        string[] package = serialized?.Split (':');
        if (package is null || package.Length != 2)
          return;

        AppName = package[0].Trim();
        bool succ = Version.TryParse (package[1], out var vers);
        if (succ)
          Version = vers;
      }

      public string Serialize () {
        return $"{AppName}:{Version}";
      }

      public override string ToString () => $"{AppName} {Version}";
    } 

    const string SETUP_REF_URL = "https://raw.githubusercontent.com/audiamus/{0}/master/res/Setup.json";
    //const string RGX_VERSION = @"-(\d+(?:\.\d+){1,3})-Setup";

    private static readonly Regex __rgxMd5 = new Regex (@"^(?:MD5:\s)?([A-Fa-f\d]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    //private readonly Regex _rgxVersion;
    private readonly string _setupRefUrl;
    private readonly string _defaultAppName;
    private readonly bool _dbg;

    //private string _downloadUri;
    //private string _setupFile;
    //private string _md5;
    //private Version _version = new Version ();

    private static HttpClient HttpClient { get; } = new HttpClient ();
    private static string DownloadDir { get; }

    private IUpdateSetting Settings { get; }

    IEnumerable<PackageInfoLocal> _packageInfos;
    PackageInfoLocal _defaultPackageInfo;

    static OnlineUpdate () {
      string downloads = Environment.ExpandEnvironmentVariables (@"%USERPROFILE%\Downloads");
      if (string.IsNullOrWhiteSpace (downloads))
        downloads = ApplEnv.TempDirectory;
      DownloadDir = downloads;
    }

    public OnlineUpdate(IUpdateSetting settings, string defaultAppName, string setupRefUrl, bool dbg) {
      Settings = settings;
      _dbg = dbg;
      _defaultAppName = defaultAppName;
      if (setupRefUrl.IsNullOrWhiteSpace())
        _setupRefUrl = string.Format (SETUP_REF_URL, defaultAppName);
      else
        _setupRefUrl = setupRefUrl;

      //string sRgxVers = "/" + defaultAppName + RGX_VERSION;
      //_rgxVersion = new Regex (sRgxVers, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    public async Task UpdateAsync (
      IInteractionCallback<InteractMessage, bool?> interactCallback,
      Action finalCallback,
      Func<bool> busyCallback
  ) {
      if (Settings.OnlineUpdate == EOnlineUpdate.no)
        return;

      await getSetupRefAsync ();

      await updateDefaultPackageAsync (interactCallback, finalCallback, busyCallback);

      await updateOtherPackagesAsync (interactCallback);

    }

    private async Task updateOtherPackagesAsync (IInteractionCallback<InteractMessage, bool?> interactCallback) {
      var otherPackages = _packageInfos.Where (pi => pi != _defaultPackageInfo).ToList ();
      if (otherPackages.IsNullOrEmpty ())
        return;

      Version zeroVersion = new Version (0, 0);
      
      var declinedPackagesIn = readDeclinedPackages ();
      var declinedPackagesOut = new List<DeclinedPackage> ();

      foreach (var pi in otherPackages) {
        bool decline = declinedPackagesIn.FirstOrDefault (dp =>
            pi.AppName.Equals (dp.AppName) && (pi.Version <= dp.Version || dp.Version == zeroVersion))
          != null;
        if (decline) {
          Log (3, this, () => $"{pi.AppName}: declined by setting.");
          declinedPackagesOut.Add (new DeclinedPackage (pi.AppName, pi.Version));
        } else {
          bool? result = await updateOtherPackageAsync (pi, interactCallback);
          if (!result.HasValue)
            declinedPackagesOut.Add (new DeclinedPackage (pi.AppName, zeroVersion));
          else if (!result.Value)
            declinedPackagesOut.Add (new DeclinedPackage (pi.AppName, pi.Version));
        }
      }

      saveDeclinedPackages ();



      IEnumerable<DeclinedPackage> readDeclinedPackages () {
        var sDeclPckIn = Settings.OnlineUpdateOthersDeclined?.Split (';').ToList ();
        var declPckIn = new List<DeclinedPackage> ();
        if (!(sDeclPckIn.IsNullOrEmpty () || sDeclPckIn.First ().IsNullOrWhiteSpace ()))
          foreach (var decl in sDeclPckIn) {
            var declPackage = new DeclinedPackage (decl);
            declPckIn.Add (declPackage);
          }
        return declPckIn;
      }

      void saveDeclinedPackages () {
        if (declinedPackagesOut.Count > 0) {
          string[] sDeclOut = declinedPackagesOut.Select (dp => dp.Serialize ()).ToArray ();
          Settings.OnlineUpdateOthersDeclined = sDeclOut.Combine ();
        } else
          Settings.OnlineUpdateOthersDeclined = string.Empty;
      }
    }

    private async Task<bool?> updateOtherPackageAsync (
      PackageInfoLocal pi,
      IInteractionCallback<InteractMessage, bool?> interactCallback) 
    {
      Version installedVersion = getInstalledVersion (pi.AppName);
      Log (3, this, () => $"{pi.AppName}: server={pi.Version}, installed={installedVersion}");

      // do we have a new version?
      bool newVersion = installedVersion is null || pi.Version > installedVersion;
      if (_dbg)
        newVersion = true;
      if (!newVersion)
        return true;

      // do we have it downloaded already?
      bool exists = await checkDownloadAsync (pi);
      Log (3, this, () => $"{pi.AppName}: download exists={exists}");

      if (!exists) {

        bool? result = interact (EUpdateInteract.newVersAvail);
        if (!(result ?? false))
          return result;

        // yes: download,  verify md5
        exists = await downloadSetupAsync (pi);

      } else {
        bool? result = interact (EUpdateInteract.installNow);
        if (!(result ?? false))
          return result;

      }

      if (exists)
        install (pi);

      return true;

      bool? interact (EUpdateInteract kind) {
        var interactmsg =
          new InteractMessage (ECallbackType.question, null, 
            new UpdateInteractionMessage (kind, pi, _defaultAppName));

        bool? result = interactCallback.Interact (interactmsg);
        Log (3, this, () => $"{pi.AppName}: interact result for {kind}={result}"); 
        if (!(result.Value)) {
          interactmsg = new InteractMessage (ECallbackType.question, null, 
            new UpdateInteractionMessage (EUpdateInteract.mayAskAgain, pi));
          result = interactCallback.Interact (interactmsg);
          bool? rtnVal = result.Value ? false : (bool?)null;
          Log (3, this, () => $"{pi.AppName}: interact 2nd result for {kind} converted={rtnVal}"); 
          return rtnVal;
        }
        return true;
      }
    }

    private Version getInstalledVersion (string appName) {
      RegistryKey hklm = Registry.LocalMachine;
      string keyPath = $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{appName.WithSpaces()}_is1";
      var key = hklm.OpenSubKey (keyPath);
      if (key is null) { 
        RegistryKey hkcu = Registry.CurrentUser;
        key = hkcu.OpenSubKey (keyPath);
        if (key is null)
          return null;
      }
      object val = key.GetValue ("DisplayVersion");
      if (val is string s) {
        Version.TryParse (s, out Version vers);
        return vers;
      } else
        return null;        
    }

    private async Task updateDefaultPackageAsync (
      IInteractionCallback<InteractMessage, bool?> interactCallback,
      Action finalCallback,
      Func<bool> busyCallback
    ) {
      var pi = _defaultPackageInfo;
      if (pi is null) {
        Log (1, this, () => "no package info");
        return;
      }

      Log (3, this, () => $"{pi.AppName}: server={pi.Version}, installed={ApplEnv.AssemblyVersion}");

      // do we have a new version?
      bool newVersion = pi.Version > ApplEnv.AssemblyVersion;
      if (_dbg)
        newVersion = true;
      if (!newVersion)
        return;

      // do we have it downloaded already?
      bool exists = await checkDownloadAsync (pi);
      Log (3, this, () => $"{pi.AppName}: download exists={exists}");

      if (!exists) {
        if (Settings.OnlineUpdate == EOnlineUpdate.promptForDownload) {
          var interactmsg =
            new InteractMessage (ECallbackType.question, null, new UpdateInteractionMessage (EUpdateInteract.newVersAvail, pi));

          bool? result1 = interactCallback.Interact (interactmsg);
          if (!(result1.Value))
            return;
        }

        // yes: download,  verify md5
        exists = await downloadSetupAsync (pi);
      }

      if (!exists)
        return;

      bool isBusy = busyCallback ();
      if (isBusy) {
        Log (3, this, () => "is already busy, cancel.");
        return;
      }

      bool cont = install (pi, interactCallback, EUpdateInteract.installNow);
      if (!cont) {
        Log (3, this, () => "do not install now, cancel.");
        return;
      }

      if (pi.DefaultApp)
        finalCallback?.Invoke ();
    }

    public async Task<bool> InstallAsync (
      IInteractionCallback<InteractMessage, bool?> interactCallback, 
      Action finalCallback
    ) {
      if (Settings.OnlineUpdate == EOnlineUpdate.no)
        return false;

      await getSetupRefAsync ();

      var pi = _defaultPackageInfo;
      if (pi is null) {
        Log (1, this, () => "no package info");
        return false;
      }

      // do we have a new version?
      bool newVersion = pi.Version > ApplEnv.AssemblyVersion;
      if (_dbg)
        newVersion = true;
      if (!newVersion)
        return false;

      // do we have it downloaded already?
      bool exists = await checkDownloadAsync (pi);
      if (!exists)
        return false;

      install (pi, interactCallback, EUpdateInteract.installLater);

      if (pi.DefaultApp)
        finalCallback?.Invoke ();

      return true;
    }

    private bool install (
      PackageInfoLocal pi, 
      IInteractionCallback<InteractMessage, bool?> interactCallback, 
      EUpdateInteract prompt
    ) {
      var interactmsg =
        new InteractMessage (ECallbackType.question, null, new UpdateInteractionMessage (prompt, pi));
      bool? result2 = interactCallback.Interact (interactmsg);
      if (!(result2 ?? true))
        return false;

      install (pi);
      return true;
    }

    private void install (PackageInfoLocal pi) {
      // launch installer
      try {
        Log (3, this, () => $"{pi.AppName}: launch.");
        Process.Start (pi.SetupFile);
      } catch (Exception exc) {
        Log (1, this, () => $"{pi.AppName}: {exc.ToShortString ()}");
      }
    }

    private async Task getSetupRefAsync () {
      try {

        var packageInfos = await getSetupRefAsync (_setupRefUrl);

        var packageInfosResolved = await resolvePackageInfosAsync (packageInfos);

        _packageInfos = packageInfosResolved.Select (pi => {
          var pil = new PackageInfoLocal (pi);

          string file = Path.GetFileName (pi.Url);
          string setupFile = Path.Combine (DownloadDir, file);
          pil.SetupFile = setupFile;

          return pil;
        }).
        ToList ();

        var defaultPackageInfo = _packageInfos
          .FirstOrDefault (pi => string.Equals (pi.AppName, _defaultAppName, StringComparison.InvariantCultureIgnoreCase));
        if (defaultPackageInfo is null)
          return;
        defaultPackageInfo.DefaultApp = true;
        _defaultPackageInfo = defaultPackageInfo;

      } catch (Exception exc) {
        Log (1, this, () => exc.ToShortString ());
      }
    }

    private async Task<IEnumerable<PackageInfo>> resolvePackageInfosAsync (IEnumerable<PackageInfo> packageInfos) {
      var resolved = new List<PackageInfo> ();
      foreach (var pi in packageInfos) {
        if (pi.InfoLinkUrl.IsNullOrWhiteSpace () && !pi.Url.IsNullOrWhiteSpace ())
          resolved.Add (pi);
        else {
          var packageInfosLinked = await getSetupRefAsync (pi.InfoLinkUrl);
          var packageInfo = packageInfosLinked.FirstOrDefault ();
          if (!(packageInfo is null) && packageInfo.InfoLinkUrl is null && !packageInfo.Url.IsNullOrWhiteSpace ())
            resolved.Add (packageInfo);
        }

      }
      return resolved;
    }

    private async Task<IEnumerable<PackageInfo>> getSetupRefAsync (string url) {
      string result = null;
      try {
        using (HttpResponseMessage response = await HttpClient.GetAsync (url)) {
          response.EnsureSuccessStatusCode ();
          using (HttpContent content = response.Content) {
            result = await content.ReadAsStringAsync ();
          }
        }
        if (string.IsNullOrWhiteSpace (result))
          return null;

        var packageInfos = JsonConvert.DeserializeObject<PackageInfo[]> (result);
        return packageInfos;
      } catch (Exception exc) {
        Log (1, this, () => $"{exc.ToShortString ()}{Environment.NewLine}  \"{result}\"");
      }
      return null;
    } 

    private async Task<bool> checkDownloadAsync (PackageInfoLocal pi) {
      if (!File.Exists (pi.SetupFile))
        return false;

      string md5 = await computeMd5ForFileAsync (pi.SetupFile);
      bool succ = string.Equals (pi.Md5, md5, StringComparison.InvariantCultureIgnoreCase);
      Log (3, this, () => $"succ={succ}, file={md5}, server={pi.Md5}");
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


    private async Task<bool> downloadSetupAsync (PackageInfoLocal pi) {
      Log0 (3, this);
      await downloadAsync (pi.Url, pi.SetupFile);
      return await checkDownloadAsync (pi);
    }


    private async Task downloadAsync (string requestUri, string filename) {
      Log (3, this, $"\"{requestUri}\"");
      try {
        //for .Net framework
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        using (var fileStream = File.OpenWrite (filename)) {
          using (var networkStream = await HttpClient.GetStreamAsync (requestUri)) {
            Log (3, this, $"copy to \"{filename.SubstitUser()}\"");
            await networkStream.CopyToAsync (fileStream);
            Log (3, this, "flush");
            await fileStream.FlushAsync ();
          }
        }
        Log (1, this, () => "complete");
      } catch (Exception exc) {
        Log (1, this, () => $"{exc.ToShortString ()}");
      }
    }

  }
}
