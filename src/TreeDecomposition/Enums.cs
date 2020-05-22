using System;
using System.ComponentModel;

namespace audiamus.aux.diagn {
  
  /// <summary>
  /// Flags to control dump output
  /// </summary>
  [Flags]
  public enum EDumpFlags {
    none = 0,

    /// <summary>
    /// Add a counter to each item in an enumeration
    /// </summary>
    withItmCnt = 1,

    /// <summary>
    /// Include properties with <c>null</c> values 
    /// </summary>
    inclNullVals = 2,

    /// <summary>
    /// Include property description, <see cref="DescriptionAttribute"/> 
    /// </summary>
    inclDesc = 4,

    /// <summary>
    /// Description above property, if included. Behind property by default. 
    /// </summary>
    descOnTop = 8,

    /// <summary>
    /// Include type description, <see cref="DescriptionAttribute"/>
    /// </summary>
    inclTypeDesc = 16,

    /// <summary>
    /// Include description in enumerations 
    /// </summary>
    inclDescInEnum = 32,

    /// <summary>
    /// Inherit attributes defined for properities in base interfaces, <see cref="TreeDecomposition{T}"/> for recognized attributes 
    /// </summary>
    inherInterfaceAttribs = 64,

    /// <summary>
    /// Group properties by implemented interfaces and their hierarchy
    /// </summary>
    byInterface = 128,

    /// <summary>
    /// Include grouping by interface for types further down the hierarchy 
    /// </summary>
    byInterfaceNestedTypes = 256,
  }
}
