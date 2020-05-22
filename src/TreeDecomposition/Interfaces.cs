using System;

namespace audiamus.aux.diagn {
  /// <summary>
  /// Interface for custom primitive types, to be used with <see cref="TreeDecomposition{T}"/>
  /// </summary>
  public interface IPrimitiveTypes {

    /// <summary>
    /// Determines whether the specified generic type is regarded as a custom primitive type.
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    bool IsPrimitiveType<T> ();


    /// <summary>
    /// Determines whether the specified type is regarded as a custom primitive type.
    /// </summary>
    /// <param name="type">The type to be checked.</param>
    bool IsPrimitiveType (Type type);

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance, 
    /// if registered as a custom primitive type. Type-safe variant.
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    /// <param name="val">The value.</param>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance or <c>null</c>.
    /// </returns>
    string ToString<T> (T val);

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance,
    /// if registered as a custom primitive type. Type deduction variant.
    /// </summary>
    /// <param name="val">The value.</param>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance or <c>null</c>.
    /// </returns>
    string ToString (object val);

    /// <summary>
    /// Returns a <see cref="System.String" /> that represents this instance, 
    /// if registered as a custom primitive type. Non-type-safe variant.
    /// </summary>
    /// <typeparam name="T">generic type</typeparam>
    /// <param name="val">The value.</param>
    /// <returns>
    /// A <see cref="System.String" /> that represents this instance.
    /// </returns>
    string ToString<T> (object val);
  }
}
