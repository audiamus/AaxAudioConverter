using System.IO;


namespace audiamus.aux.diagn {
  /// <summary>
  /// <see cref="TreeDecomposition{T}"/>
  /// </summary>
  public static class TreeDecompositionExtension {

    /// <summary>
    /// Dumps the specified object as a text tree to a string,
    /// without additional primitive types.
    /// </summary>
    /// <param name="o">The object to dump.</param>
    /// <param name="flags">Output modifier flags.</param>
    /// <param name="caption">The optional caption for this indentation level.</param>
    /// <returns>
    /// Text tree
    /// </returns>
    public static string Dump (this object o, EDumpFlags flags = default, string caption = null) {
      using (var sw = new StringWriter ()) {
        o.Dump (sw, new Indent (), flags, caption);
        return sw.ToString ();
      }
    }

    /// <summary>
    /// Dumps the specified object as a text tree to the specified <see cref="TextWriter" />,
    /// without additional primitive types.
    /// </summary>
    /// <param name="o">The object to dump.</param>
    /// <param name="tw">The TextWriter output.</param>
    /// <param name="ind">The indentation.</param>
    /// <param name="flags">Output modifier flags.</param>
    /// <param name="caption">The optional caption for this indentation level.</param>
    public static void Dump (this object o, TextWriter tw, Indent ind = null, EDumpFlags flags = default, string caption = null) => 
      TreeDecomposition<NoPrimitiveTypes>.Default.Dump (o, tw, ind ?? new Indent (), flags, caption);

    /// <summary>
    /// Dumps the specified object as a text tree to a string, 
    /// allowing additional user provided primitive types.
    /// </summary>
    /// <typeparam name="T">Additional primitive types, implementing <see cref="IPrimitiveTypes"/></typeparam>
    /// <param name="o">The object to dump.</param>
    /// <param name="flags">Output modifier flags.</param>
    /// <param name="caption">The optional caption for this indentation level.</param>
    /// <returns>Text tree</returns>
    public static string Dump<T> (this object o, EDumpFlags flags, string caption = null) 
      where T : IPrimitiveTypes, new() 
    {
      using (var sw = new StringWriter ()) {
        o.Dump<T> (sw, new Indent (), flags, caption);
        return sw.ToString ();
      }
    }

    /// <summary>
    /// Dumps the specified object as a text tree to the specified <see cref="TextWriter"/>, 
    /// allowing additional user provided primitive types.
    /// </summary>
    /// <typeparam name="T">Additional primitive types, implementing <see cref="IPrimitiveTypes"/></typeparam>
    /// <param name="o">The object to dump.</param>
    /// <param name="tw">The TextWriter output.</param>
    /// <param name="ind">The indentation.</param>
    /// <param name="flags">Output modifier flags.</param>
    /// <param name="caption">The optional caption for this indentation level.</param>
    public static void Dump<T> (this object o, TextWriter tw, Indent ind = null, EDumpFlags flags = default, string caption = null)
      where T : IPrimitiveTypes, new() => 
        TreeDecomposition<T>.Default.Dump (o, tw, ind ?? new Indent (), flags, caption);
  }
}
