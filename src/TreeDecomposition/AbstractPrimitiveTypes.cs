using System;
using System.Collections.Generic;

namespace audiamus.aux.diagn {
  /// <summary>
  /// Base class to implement <see cref="IPrimitiveTypes"/> with a dictionary. 
  /// Derived classes simply need to call the provided add method with custom function delegates.
  /// </summary>
  /// <seealso cref="audiamus.aux.diagn.IPrimitiveTypes" />
  public abstract class AbstractPrimitiveTypes : IPrimitiveTypes {

    Dictionary<Type, Delegate> _dict = new Dictionary<Type, Delegate> ();

    /// <summary>
    /// Determines whether the specified generic type is regarded as a custom primitive type.
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    public bool IsPrimitiveType<T> () {
      Type type = typeof (T);
      return IsPrimitiveType (type);
    }

    /// <summary>
    /// Determines whether the specified type is regarded as a custom primitive type.
    /// </summary>
    /// <param name="type">The type to be checked.</param>
    public bool IsPrimitiveType (Type type) => _dict.ContainsKey (type);


    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance, 
    /// if registered as a custom primitive type. Type-safe variant.
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    /// <param name="val">The value.</param>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance or <c>null</c>.
    /// </returns>
    public string ToString<T> (T val) => toStringFunc<T> ()?.Invoke (val);

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance,
    /// if registered as a custom primitive type. Type deduction variant.
    /// </summary>
    /// <param name="val">The value.</param>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance or <c>null</c>.
    /// </returns>
    public string ToString (object val) {
      if (val is null)
        return string.Empty;
      Type type = val.GetType();
      Delegate d = toStringFunc (type);
      return d?.Method.Invoke (d.Target, new object[] { val }) as string;
    }

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance, 
    /// if registered as a custom primitive type. Non-type-safe variant.
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    /// <param name="val">The value.</param>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    public string ToString<T> (object val) {
      Type type = typeof(T);
      Delegate d = toStringFunc (type);
      return d?.Method.Invoke (d.Target, new object[] { val }) as string;
    }

    /// <summary>
    /// Function delegate for generic type a custom primitive type to obtain string representation. Type-safe variant.
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    /// <returns>Function delegate for generic type {T} or <c>null</c>.</returns>
    private Func<T, string> toStringFunc<T> () {
      Type type = typeof (T);
      bool succ = _dict.TryGetValue (type, out var func);
      if (succ)
        return func as Func<T, string>;
      else
        return null;
    }

    /// <summary>
    /// Function delegate for type  to obtain string representation. Non-type-safe variant.
    /// </summary>
    /// <returns>Generic delegate for type or <c>null</c>.</returns>
    private Delegate toStringFunc (Type type) {
      bool succ = _dict.TryGetValue (type, out var func);
      if (succ)
        return func;
      else
        return null;
    }


    /// <summary>
    /// Sets the specified function for the given type.
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    /// <param name="func">The function delegate.</param>
    protected void add<T> (Func<T, string> func) {
      Type type = typeof (T);
      _dict[type] = func;
    }
  }

  /// <summary>
  /// Convenience class as default implementation of <see cref="IPrimitiveTypes"/> with no additional custom types.  
  /// </summary>
  /// <seealso cref="audiamus.aux.diagn.AbstractPrimitiveTypes" />
  internal class NoPrimitiveTypes : AbstractPrimitiveTypes {  }

}


