using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using audiamus.aux.ex;

namespace audiamus.aux.diagn {

  public static class TreeDecomposition {
    /// <summary>
    /// Custom marker/separator for description text. Will use default if <c>null</c>.
    /// </summary>
    public static string DescriptionMarker { get; set; }
  }


  /// <summary>
  /// <para>Decomposition of specified object into a text tree.</para>
  /// <para>Iterates public properties in specified object. 
  /// One property name/value pair per text line. 
  /// Hierarchy levels are determined by property type.  
  /// Primitive types are written to the same hierarchy level.  
  /// Non-primitive types are written to the next lower hierarchy level, working recursively.  
  /// As an option, properties can additionally be grouped by implemented interfaces and their hierarchy. 
  /// Hierarchy levels are indicated through indentation. 
  /// Collections are decomposed by item, iterating <see cref="IEnumerable"/>. 
  /// Custom primitive types can be set by providing a generic argument, implementing <see cref="IPrimitiveTypes"/>.</para>
  /// <para>Built-in primitive types as per definition here are not only the scalar types (int, double etc) 
  /// but also string, enum, and - important - all other .Net framework types in the System namespaces. 
  /// (The purpose of this facility is to decompose user types, not system types.) 
  /// Built-in primitive type handling can be overridden 
  /// by custom primimitive types.</para>
  /// <para>Note: <see cref="AbstractPrimitiveTypes"/> can be used as a base class to implement <see cref="IPrimitiveTypes"/>.</para>
  /// <para>Properties can be annotated with attributes. Recognized attributes are:</para>
  /// <list type="bullet">
  /// <item><see cref="DescriptionAttribute"/>to describe a property, must be activated with <see cref="EDumpFlags.inclDesc"/></item>
  /// <item><see cref="BrowsableAttribute"/>to hide a property, if set to <c>false</c>.</item>
  /// <item><see cref="DisplayNameAttribute"/>to override the property name.</item>
  /// <item><see cref="DisplayItemNameAttribute"/>to override the item name in a collection.</item>
  /// <item><see cref="TextFormatAttribute"/>to specify a format string.</item></list>
  /// <item><see cref="ToStringAttribute"/>to specify a custom ToString() method with a type converter derived from <see cref="ToStringConverter"/>.</item></list>
  /// <para>To be used as a singleton from extension methods, see <see cref="TreeDecompositionExtension"/> for details.</para> 
  /// </summary>
  /// <typeparam name="T">Additional primitive types, implementing <see cref="IPrimitiveTypes"/></typeparam>
  internal class TreeDecomposition<T>
    where T : IPrimitiveTypes, new() {

    abstract class CustomFormat { }

    class CustomFormatString : CustomFormat {
      public readonly string Format;
      public CustomFormatString (string format) => Format = format;
    }

    class CustomToString : CustomFormatString {
      public readonly ToStringConverter Converter;
      public CustomToString (ToStringConverter converter, string format) : base (format) => Converter = converter;
    }
 

    static IPrimitiveTypes __primitveTypes = new T ();

    static TreeDecomposition<T> __default;

    public static TreeDecomposition<T> Default {
      get {
        if (__default is null)
          __default = new TreeDecomposition<T> ();
        return __default;
      }
    }

    private TreeDecomposition () { }

    /// <summary>
    /// Dumps the specified object as a text tree. Will be used recursively.
    /// </summary>
    /// <param name="o">The object to dump.</param>
    /// <param name="tw">The TextWriter output.</param>
    /// <param name="ind">The indentation.</param>
    /// <param name="falgs">The output modifier flags.</param>
    /// <param name="caption">The optional caption for this indentation level.</param>
    public void Dump (object o, TextWriter tw, Indent ind, EDumpFlags flags = default, string caption = null) =>
      dump (o, new Stack<Type> (), tw, ind, flags, caption, null, null, null, false);


    private void dump (
      object o, Stack<Type> stack, TextWriter tw, Indent ind, EDumpFlags flags,
      string caption, string itemCaption, CustomFormat itemFormat, string oDesc, bool inEnum
    ) {
      if (o is null)
        return;

      // via reflection
      Type objectType = o.GetType ();
      stack.Push (objectType);

      if (caption is null && ind.Level == 0)
        caption = objectType.Name;

      // caption
      write (tw, ind, caption, flags, oDesc);

      // next level
      using (new ResourceGuard (ind)) {


        // is it a collection?
        bool isEnumerable = typeof (IEnumerable).IsAssignableFrom (objectType);

        if (isEnumerable) {

          dumpCollection (o, stack, objectType, tw, ind, flags, itemCaption, itemFormat);

        } else {

          // all public properties, including inherited ones
          IEnumerable<PropertyInfo> propInfos = objectType.GetProperties (BindingFlags.Public | BindingFlags.Instance);

          if (flags.HasFlag(EDumpFlags.byInterface) && (stack.Count < 2 || flags.HasFlag(EDumpFlags.byInterfaceNestedTypes))) {
            var interfaceHierarchy = objectType.GetInterfaceHierarchy ();
            if (interfaceHierarchy.Count () > 0) {
              foreach (var path in interfaceHierarchy)
                dump (ref propInfos, o, path, stack, tw, ind, flags, inEnum);
              dump (o, propInfos, stack, tw, ind, flags, inEnum);
            } else 
              dump (o, propInfos, stack, tw, ind, flags, inEnum);
          } else 
            dump (o, propInfos, stack, tw, ind, flags, inEnum);
        }
      }

      stack.Pop ();
    }

    private void dump (
      ref IEnumerable<PropertyInfo> propInfos, object o, IEnumerable<Type> path,
      Stack<Type> stack, TextWriter tw, Indent ind, EDumpFlags flags, bool inEnum
    ) {

      Type ifcType = path.Last ();
      if (!ifcType.IsInterface)
        return;
      IEnumerable<PropertyInfo> ifcPropInfos = ifcType.GetProperties ();
      if (ifcPropInfos.Count () == 0)
        return;

      var propNames = ifcPropInfos.Select (pi => pi.Name);
      var filteredPropInfos = propInfos.Where (pi => propNames.Contains (pi.Name));
      if (filteredPropInfos.Count () == 0)
        return;

      propInfos = propInfos.Except (filteredPropInfos);

      string sPath = path.ToHierarchyString ();
      tw.WriteLine ($"{ind}:{sPath}");
      using (new ResourceGuard (ind))
        dump (o, filteredPropInfos, stack, tw, ind, flags, inEnum);

    }

    private void dump (object o, IEnumerable<PropertyInfo> propInfos, Stack<Type> stack, TextWriter tw, Indent ind, EDumpFlags flags, bool inEnum) {
      foreach (var propInfo in propInfos) {

        //skip indexed
        bool isIndexed = propInfo.GetIndexParameters ().Length > 0;
        if (isIndexed)
          continue;

        // value
        object propValue = propInfo.GetValue (o);

        //skip null
        if (!flags.HasFlag (EDumpFlags.inclNullVals)) {
          if (propValue is null)
            continue;
          else
          //skip whitespace
          if (propValue is string sPropValue)
            if (sPropValue.IsNullOrWhiteSpace ())
              continue;
        }

        // recursive types only allowed to maximum depth, exceeding instance will be handled as primitive type
        bool isRecursive = stack.Where (t => t == propInfo.PropertyType).Count () > 20;

        // check for modification attributes
        IEnumerable<object> attrs = null;
        if (flags.HasFlag(EDumpFlags.inherInterfaceAttribs))
          attrs = propInfo.GetCustomAttributesIncludingBaseInterfaces ();
        else
          attrs = propInfo.GetCustomAttributes (true);

        // shall be ignored?
        var browsable = attrs.FirstOfType<BrowsableAttribute> ();
        if (!(browsable?.Browsable ?? true))
          continue;

        // name
        string propName = propInfo.Name;

        // alternative name
        var displName = attrs.FirstOfType<DisplayNameAttribute> ();
        if (displName != null)
          propName = displName.DisplayName;

        // alternative item name for collection
        string itemName = attrs.FirstOfType<DisplayItemNameAttribute> ()?.Name;

        // optional custom formats
        CustomFormat customFormat = getCustomFormat (attrs);

        // optional description
        string desc = getDesc (propInfo, attrs, flags, inEnum);

        // actual type
        Type propType = propValue?.GetType ();

        // how to dump
        if (isPrimitiveType (propType) || isRecursive)

          // this level, as primitive
          write (tw, ind, propName, propValue, customFormat, desc, flags.HasFlag (EDumpFlags.descOnTop));

        else

          //deeper level, recursive call
          dump (propValue, stack, tw, ind, flags, propName, itemName, customFormat, desc, inEnum);

      }

    }

    private static CustomFormat getCustomFormat (IEnumerable<object> attrs) {
      CustomFormat customFormat = null;
      var toStringAttr = attrs.FirstOfType<ToStringAttribute> ();
      if (toStringAttr.IsNull()) {
        string format = attrs.FirstOfType<TextFormatAttribute> ()?.Format;
        if (!format.IsNull())
          customFormat = new CustomFormatString (format);
      } else
        if (!toStringAttr.Converter.IsNull())
        customFormat = new CustomToString (toStringAttr.Converter, toStringAttr.Format);
      return customFormat;
    }

    private string getDesc (PropertyInfo propInfo, IEnumerable<object> attrs, EDumpFlags flags, bool inEnum) {
      if (!flags.HasFlag (EDumpFlags.inclDesc) || (inEnum && !flags.HasFlag (EDumpFlags.inclDescInEnum)))
        return null;

      string desc = attrs.FirstOfType<DescriptionAttribute> ()?.Description;
      if (desc.IsNull ())
        desc = getTypeDesc (propInfo.PropertyType, flags, inEnum);

      return desc;
    }

    private string getTypeDesc (Type type, EDumpFlags flags, bool inEnum) {
      if (!flags.HasFlag (EDumpFlags.inclDesc) || !flags.HasFlag (EDumpFlags.inclTypeDesc) || (inEnum && !flags.HasFlag (EDumpFlags.inclDescInEnum)))
        return null;      
      object[] attrs = type.GetCustomAttributes (true);
      return attrs.FirstOfType<DescriptionAttribute> ()?.Description;
    }

    private void dumpCollection (object o, Stack<Type> stack, Type objectType, TextWriter tw, Indent ind, 
      EDumpFlags flags, string itemCaption, CustomFormat itemFormat
    ) {

      // item type
      Type itemType = objectType.GetInterfaces ()
        .Where (t => t.IsGenericType && t.GetGenericTypeDefinition ().Equals (typeof (IEnumerable<>)))
          .Select (t => t.GetGenericArguments ()[0])
            .FirstOrDefault () ?? typeof (object);
      string desc = getTypeDesc (itemType, flags, true);
      //if (!desc.IsNull())
      //  ; // for debug

      bool isPrimitive = isPrimitiveType (itemType);

      if (flags.HasFlag (EDumpFlags.withItmCnt)) {
        if (itemCaption.IsNullOrWhiteSpace ()) {
          if (isPrimitive)
            itemCaption = "#";
          else
            itemCaption = itemType.Name + " ";
        } else
          itemCaption += " ";
      } else if (itemCaption.IsNullOrWhiteSpace () && !isPrimitive)
        itemCaption = itemType.Name;

      int i = 0;
      // hard cast
      foreach (var item in (IEnumerable)o) {
        i++;
        string caption;
        if (flags.HasFlag (EDumpFlags.withItmCnt))
          caption = $"{itemCaption}{i}";
        else
          caption = itemCaption;

        if (isPrimitive)

          //  this level, as primitive
          write (tw, ind, caption, item, itemFormat, desc, flags.HasFlag(EDumpFlags.descOnTop));

        else

          //  deeper level, recursive call
          dump (item, stack, tw, ind, flags, caption, itemCaption, itemFormat, desc, true);

      }
    }

    private static bool isPrimitiveType (Type type) {
      // determine what defines a primitive type

      if (type is null)
        return true;

      // simple cases
      bool isBuiltInPrimitive =
          type.IsPrimitive ||
          type == typeof (decimal) ||
          type == typeof (string) ||
          type.IsEnum;

      // "catch all"
      bool isSystemType =
          type.Namespace.StartsWith (nameof (System));

      // but not if it is enumerable
      bool isEnumerableSystemType = typeof (IEnumerable).IsAssignableFrom (type);
      if (isEnumerableSystemType)
        isSystemType = false;

      //type.IsPrimitive ||
      //type == typeof (decimal) ||
      //type == typeof (string) ||
      //type == typeof (DateTime) ||
      //type == typeof (DateTimeOffset) ||
      //type == typeof (TimeSpan);
      //||  Nullable.GetUnderlyingType (type) != null;

      bool isAddedPrimitive = __primitveTypes.IsPrimitiveType (type);

      bool isPrimitive = isBuiltInPrimitive || isSystemType || isAddedPrimitive;
      return isPrimitive;

    }

    private static void write (TextWriter tw, Indent ind, string value, EDumpFlags flags, string desc = null) {
      if (value.IsNullOrWhiteSpace ())
        return;

      var (m1, m2) = mkr ();
      string c = flags.HasFlag (EDumpFlags.byInterface) ? string.Empty : ":"; 

      if (desc.IsNullOrWhiteSpace ())
        tw.WriteLine ($"{ind}{value}{c}");
      else if (flags.HasFlag (EDumpFlags.descOnTop)) {
        tw.WriteLine ();
        tw.WriteLine (ind + m1 + desc);
        tw.WriteLine ($"{ind}{value}{c}");
      } else
        tw.WriteLine ($"{ind}{value}{m2}{desc}");
    }

    private static void write (TextWriter tw, Indent ind, string name, object value, CustomFormat format, string desc = null, bool descOnTop = false) {

      var (m1, m2) = mkr ();

      string sValue;
      if (!format.IsNull()) {
        try {
          switch (format) {
            case CustomToString c:
              sValue = c.Converter.ToString(value, c.Format);
              break;
            case CustomFormatString s:
              sValue = string.Format ($"{{0:{s.Format}}}", value);
              break;
            default:
              sValue = value.ToString ();
              break;
          }
        } catch {
          sValue = value.ToString ();
        }
      } else {
        sValue = __primitveTypes.ToString (value);
        if (sValue is null && value.GetType ().IsEnum)
          sValue = __primitveTypes.ToString<Enum> (value);
        if (sValue is null)
          sValue = value.ToString ();
      }

      if (name.IsNullOrWhiteSpace ())
        tw.WriteLine (ind + sValue);
      else if (desc.IsNullOrWhiteSpace ())
        tw.WriteLine (snamval (ind, name, sValue));
      else if (descOnTop) {
        tw.WriteLine ();
        tw.WriteLine (ind + m1 + desc);
        tw.WriteLine (snamval (ind, name, sValue));
      } else
        tw.WriteLine (snamval (ind, name, sValue) + m2 + desc);

    }

    private static (string m1, string m2) mkr () {
      string m = TreeDecomposition.DescriptionMarker ?? "!";
      string m1 = m + " ";
      string m2 = "  " + m1;
      return (m1, m2);
    }

    private static string snamval (Indent ind, string name, string sValue) => $"{ind}{name} = {sValue}";

  }


  static class WhereSelectEx {
    public static T FirstOfType<T> (this IEnumerable<object> enumerable) where T : class {
      return enumerable.OfType<T>().FirstOrDefault ();
    }

    public static IEnumerable<object> GetCustomAttributesIncludingBaseInterfaces (this PropertyInfo pi) {
      return pi.GetCustomAttributes (true).
        Union (pi.DeclaringType.GetInterfaces ().
          Select (it => it.GetProperty (pi.Name, pi.PropertyType)).
          Where (p => !p.IsNull()).
          SelectMany (p => p.GetCustomAttributes (true))).
        Distinct ().
        ToList ();
    }
  }
}
