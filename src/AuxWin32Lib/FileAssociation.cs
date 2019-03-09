using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;

namespace audiamus.aux.w32 {
  class FileAssociation {
    public string Extension { get; set; }
    public string ProgId { get; set; }
    public string FileTypeDescription { get; set; }
    public string ExecutableFilePath { get; set; }
  }

  public class FileAssociations {
    // needed so that Explorer windows get refreshed after the registry is updated
    [System.Runtime.InteropServices.DllImport ("Shell32.dll")]
    private static extern int SHChangeNotify (int eventId, int flags, IntPtr item1, IntPtr item2);

    private const int SHCNE_ASSOCCHANGED = 0x8000000;
    private const int SHCNF_FLUSH = 0x1000;
    
    private const string SOFTWARE_CLASSES = @"Software\Classes";
    private const string OPENWITHPROGIDS = @"OpenWithProgIDs";
    private const string OPENWITHLIST = @"OpenWithList";
    private const string DEFAULTICON = @"DefaultIcon";
    private const string SHELL_OPEN_COMMAND = @"shell\open\command";


    public static void EnsureAssociationsSet (string extension, string fileTypeDescription, Assembly assembly = null) => 
      EnsureAssociationsSet (new[] { extension }, fileTypeDescription, assembly);

    public static void EnsureAssociationsSet (IEnumerable<string> extensions, string fileTypeDescription, Assembly assembly = null) =>
      manipulateAssociationsSet (extensions, fileTypeDescription, assembly, SetAssociation);

    public static void RemoveAssociationsSet (string extension, Assembly assembly = null) =>
      RemoveAssociationsSet (new[] { extension }, assembly);

    public static void RemoveAssociationsSet (IEnumerable<string> extensions, Assembly assembly = null) => 
      manipulateAssociationsSet (extensions, null, assembly, removeAssociation);

    public static bool SetAssociation (string extension, string progId, string fileTypeDescription, string applicationFilePath) {
      string progIdExt = $"{progId}{extension}";
      bool madeChanges = false;
      madeChanges |= setKeyValue ($@"{SOFTWARE_CLASSES}\{extension}", progIdExt);
      madeChanges |= setKeyValue ($@"{SOFTWARE_CLASSES}\{extension}\{OPENWITHPROGIDS}", null);
      madeChanges |= setKeyValue ($@"{SOFTWARE_CLASSES}\{extension}\{OPENWITHPROGIDS}", string.Empty, progIdExt);
      madeChanges |= setKeyValue ($@"{SOFTWARE_CLASSES}\{extension}\{OPENWITHLIST}", null);
      madeChanges |= setKeyValue ($@"{SOFTWARE_CLASSES}\{extension}\{OPENWITHLIST}\{progIdExt}", null);
      madeChanges |= setKeyValue ($@"{SOFTWARE_CLASSES}\{progIdExt}", fileTypeDescription);
      madeChanges |= setKeyValue ($@"{SOFTWARE_CLASSES}\{progIdExt}\{DEFAULTICON}", $"\"{applicationFilePath}\", 0");
      madeChanges |= setKeyValue ($@"{SOFTWARE_CLASSES}\{progIdExt}\{SHELL_OPEN_COMMAND}", $"\"{ applicationFilePath}\" \"%1\"");
      return madeChanges;
    }

    public static bool RemoveAssociation (string extension, string progId, string applicationFilePath) {
      string progIdExt = $"{progId}{extension}";
      bool madeChanges = false;

      madeChanges |= deleteValue ($@"{SOFTWARE_CLASSES}\{extension}");
      madeChanges |= deleteValue ($@"{SOFTWARE_CLASSES}\{extension}\{OPENWITHPROGIDS}", progIdExt);
      madeChanges |= deleteSubKey ($@"{SOFTWARE_CLASSES}\{extension}\{OPENWITHLIST}\{progIdExt}");
      madeChanges |= deleteSubKey ($@"{SOFTWARE_CLASSES}\{progIdExt}", true);

      return madeChanges;
    }

    private static void manipulateAssociationsSet (
      IEnumerable<string> extensions, 
      string fileTypeDescription, 
      Assembly assembly, 
      Func<string, string, string, string, bool> func) 
    {
      if (assembly is null)
        assembly = Assembly.GetEntryAssembly ();
      var associations = new List<FileAssociation> ();
      foreach (string ext in extensions)
        associations.Add (new FileAssociation {
          Extension = ext,
          ProgId = assembly.GetName ().Name,
          FileTypeDescription = fileTypeDescription,
          ExecutableFilePath = assembly.Location
        });
      manipulateAssociationsSet (associations, func);
    }

    private static void manipulateAssociationsSet (IEnumerable<FileAssociation> associations, Func<string, string, string, string, bool> func) {
      try {
        bool madeChanges = false;
        foreach (var association in associations) {
          madeChanges |= func (
              association.Extension,
              association.ProgId,
              association.FileTypeDescription,
              association.ExecutableFilePath);
        }

        if (madeChanges) {
          SHChangeNotify (SHCNE_ASSOCCHANGED, SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }
      } catch (Exception) { }
    }

    private static bool removeAssociation (string extension, string progId, string fileTypeDescription, string applicationFilePath) => 
      RemoveAssociation (extension, progId, applicationFilePath);

    private static bool setKeyValue (string keyPath, string value, string name = null) {
      bool dirty = false;
      // does key exist?
      using (var key = Registry.CurrentUser.OpenSubKey (keyPath)) {
        dirty = key is null;
      }

      using (var key = Registry.CurrentUser.CreateSubKey (keyPath)) {
        if (name is null) {
          if (key.GetValue (name) as string != value) {
            key.SetValue (name, value);
            dirty = true;
          }
        } else {
          var names = key.GetValueNames ();
          if (names.Contains (name)) {
            if (key.GetValue (name) as string != value) {
              key.SetValue (name, value);
              dirty = true;
            } 
          } else { 
            key.SetValue (name, value);
            dirty = true;
          }
        }
      }

      return dirty;
    }

    private static bool deleteValue (string keyPath, string name = null) {
      bool dirty = false;
      using (var key = Registry.CurrentUser.OpenSubKey (keyPath, true)) {
        if (key is null)
          return false;
        name = name ?? string.Empty;
        var names = key.GetValueNames ();
        if (names.Contains (name) || name == String.Empty)
          dirty = true;
        key.DeleteValue (name);
      }
      return dirty;
    }

    private static bool deleteSubKey (string keyPath, bool subtree = false) {
      using (var key = Registry.CurrentUser.OpenSubKey (keyPath)) {
        if (key is null)
          return false;
      }

      int pos = keyPath.LastIndexOf ('\\');
      string path = keyPath.Substring (0, pos);
      string subkey = keyPath.Substring (pos + 1);

      using (var key = Registry.CurrentUser.OpenSubKey (path, true)) {
        if (subtree)
          key.DeleteSubKeyTree (subkey);
        else
          key.DeleteSubKey (subkey);
      }

      return true;
    }
  }

}
