using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

namespace audiamus.aux {
  public static class ApplEnv {

    static readonly char[] INVALID_CHARS = Path.GetInvalidFileNameChars ();


    public static Assembly EntryAssembly { get; } = Assembly.GetEntryAssembly ();
    public static Assembly ExecutingAssembly { get; } = Assembly.GetExecutingAssembly ();

    public static Version AssemblyVersion { get; } = EntryAssembly.GetName ().Version;
    public static string AssemblyTitle { get; } = 
      getAttribute<AssemblyTitleAttribute> ()?.Title ?? Path.GetFileNameWithoutExtension (ExecutingAssembly.CodeBase);
    public static string AssemblyProduct { get; } = getAttribute<AssemblyProductAttribute> ()?.Product;
    public static string AssemblyCopyright { get; } = getAttribute<AssemblyCopyrightAttribute> ()?.Copyright;
    public static string AssemblyCompany { get; } = getAttribute<AssemblyCompanyAttribute> ()?.Company;
    public static string NeutralCultureName { get; } = getAttribute<NeutralResourcesLanguageAttribute> ()?.CultureName;

    public static string AssemblyGuid { get; } = getAttribute<GuidAttribute> ()?.Value;  

    public static string ApplName { get; } = EntryAssembly.GetName ().Name;
    public static string ApplDirectory { get; } = Path.GetDirectoryName(EntryAssembly.Location);
    public static string LocalDirectoryRoot { get; } = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
    public static string LocalCompanyDirectory { get; } = Path.Combine (LocalDirectoryRoot, getCompanyFileName());
    public static string LocalApplDirectory { get; } = Path.Combine (LocalCompanyDirectory, ApplName);
    public static string TempDirectory { get; } = Path.Combine (LocalApplDirectory, "tmp");
    public static string LogDirectory { get; } = Path.Combine (LocalApplDirectory, "log");
    public static string UserName { get; } = Environment.UserName;
    public static string UserDirectoryRoot { get; } = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);

    private static T getAttribute<T> () where T : Attribute {
      object[] attributes = EntryAssembly.GetCustomAttributes (typeof (T), false);
      if (attributes.Length == 0)
        return null;
      return attributes[0] as T;
    }


    private static string getCompanyFileName () {
      string company = AssemblyCompany;
      if (string.IsNullOrEmpty (company))
        company = "misc";
      if (company.IndexOfAny (INVALID_CHARS) >= 0)
        foreach (char c in INVALID_CHARS)
          company.Replace (c, ' ');
      company = company.Replace (' ', '_');
      return company;
    }
  }
}
