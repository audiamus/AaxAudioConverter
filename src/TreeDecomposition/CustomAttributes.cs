using System;
using System.Collections.Generic;

namespace audiamus.aux.diagn {

  /// <summary>
  /// An attribute intended to be used as a custom format string in text serialization.   
  /// </summary>
  /// <seealso cref="System.Attribute" />
  [AttributeUsage (AttributeTargets.Property)]
  public class TextFormatAttribute : Attribute {
    public readonly string Format;

    public TextFormatAttribute (string format) => Format = format;
  }

  /// <summary>
  /// An attribute intended to convey a custom ToString() method. 
  /// Given type must be derived from <see cref="ToStringConverter"/>. 
  /// Optional second parameter to be interpreted as a format specification.
  /// </summary>
  /// <seealso cref="System.Attribute" />
  [AttributeUsage (AttributeTargets.Property)]
  public class ToStringAttribute : Attribute {
    static readonly Dictionary<Type, ToStringConverter> __converters = new Dictionary<Type, ToStringConverter> ();    
    
    public readonly ToStringConverter Converter;
    public readonly string Format;

    public ToStringAttribute (Type type, string format = null) {
      if (typeof (ToStringConverter).IsAssignableFrom (type)) {
        lock (__converters) {
          bool succ = __converters.TryGetValue (type, out var converter);
          if (succ)
            Converter = converter;
          else
            try {
              Converter = (ToStringConverter)Activator.CreateInstance (type);
              __converters.Add (type, Converter);
            } catch (Exception) { }
        }
      }
      Format = format;
    }
  }

  /// <summary>
  /// An attribute similar to <see cref="System.ComponentModel.DisplayNameAttribute"/>, 
  /// but intended to be used with collection items.
  /// </summary>
  /// <seealso cref="System.Attribute" />
  [AttributeUsage (AttributeTargets.Property)]
  public class DisplayItemNameAttribute : Attribute {
    public readonly string Name;

    public DisplayItemNameAttribute (string name) => Name = name;
  }


}
