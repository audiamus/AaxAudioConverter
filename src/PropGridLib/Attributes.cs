using System;

namespace audiamus.aux.propgrid {
  [AttributeUsage (AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class GlobalizedPropertyAttribute : Attribute {
    public string NameTag { get; set; }
    public string DescriptionTag { get; set; }
    public string CategoryTag { get; set; }
    public string ResourceTable { get; set; }
  }
}
