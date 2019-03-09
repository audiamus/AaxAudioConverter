using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;

namespace audiamus.aux.propgrid {

  [TypeConverter (typeof (PropertyCommandConverter))]
  public class PropertyCommand {

    const string PROPERTY_COMMAND = "Property Command";
    const string PROPERTY_NAME = "Property Name";
    const string VISIBILITY = "Visibility";

    [Category (PROPERTY_NAME)]
    public string Name { get; } = PROPERTY_COMMAND;

    [Category (VISIBILITY)]
    public bool ReadOnly { get; set; } = false;

    [Category (VISIBILITY)]
    public bool Visible { get; set; } = true;

    public PropertyCommand () {
    }

    public PropertyCommand (string Name) {
      this.Name = Name;
    }

    public PropertyCommand (string Name, bool Visible, bool ReadOnly) {
      this.Name = Name;
      this.Visible = Visible;
      this.ReadOnly = ReadOnly;
    }

    public override string ToString () {
      return Name;
    }
  }

  [TypeConverter (typeof (CategoryCommandConverter))]
  public class CategoryCommand {
    const string VISIBILITY = "Visibility";
    const string CATEGORY_NAME = "Category Name";

    [Category (VISIBILITY)]
    public bool Visible { get; set; } = true;

    [Category (CATEGORY_NAME)]
    public string Name { get; } = "Category Command";

    public CategoryCommand () {
    }

    public CategoryCommand (string Name) {
      this.Name = Name;
    }

    public CategoryCommand (string Name, bool Visible) {
      this.Name = Name;
      this.Visible = Visible;
    }

    public override string ToString () {
      return Name;
    }

  }


  internal class PropertyCommandConverter : ExpandableObjectConverter {

    public override bool CanConvertTo (ITypeDescriptorContext context, Type destType) {
      if (destType == typeof (InstanceDescriptor)) {
        return true;
      }
      return base.CanConvertTo (context, destType);
    }


    public override object ConvertTo (ITypeDescriptorContext context, CultureInfo info, object value, Type destType) {
      if (destType == typeof (InstanceDescriptor)) {
        return new InstanceDescriptor (typeof (PropertyCommand).GetConstructor (new Type[] { typeof (string), typeof (bool), typeof (bool) }), new object[] { ((PropertyCommand)value).Name, ((PropertyCommand)value).Visible, ((PropertyCommand)value).ReadOnly }, true);
      }
      return base.ConvertTo (context, info, value, destType);
    }


  }


  internal class CategoryCommandConverter : ExpandableObjectConverter {

    public override bool CanConvertTo (ITypeDescriptorContext context, Type destType) {
      if (destType == typeof (InstanceDescriptor)) {
        return true;
      }
      return base.CanConvertTo (context, destType);
    }


    public override object ConvertTo (ITypeDescriptorContext context, CultureInfo info, object value, Type destType) {
      if (destType == typeof (InstanceDescriptor)) {
        return new InstanceDescriptor (typeof (CategoryCommand).GetConstructor (new Type[] { typeof (string), typeof (bool) }), new object[] { ((CategoryCommand)value).Name, ((CategoryCommand)value).Visible }, true);
      }
      return base.ConvertTo (context, info, value, destType);
    }


  }

  public class PropertyCommands : AbstractCommands<PropertyCommand> { }

  public class CategoryCommands : AbstractCommands<CategoryCommand> { }

  public abstract class AbstractCommands<C> : CollectionBase where C : class {

    public int Add (C c) {
      return this.InnerList.Add (c);
    }

    public void AddRange (C[] cs) {
      InnerList.AddRange (cs);
    }

    public void Remove (C c) {
      InnerList.Remove (c);
    }

    public new void RemoveAt (int index) {
      InnerList.RemoveAt (index);
    }

    public bool Contains (C c) {
      return InnerList.Contains (c);
    }

    public bool Contains (string Name) {
      return this[Name] != null;
    }

    public void Sort () {
      InnerList.Sort (CommandComparer.Default);
    }

    public int BinarySearch (C c) {
      InnerList.Sort (CommandComparer.Default);
      return InnerList.BinarySearch (c, CommandComparer.Default);
    }

    public C this[string Name] {
      get
      {
        InnerList.Sort (CommandComparer.Default);
        int index = InnerList.BinarySearch (Name, CommandComparer.Default);
        if (index > -1) {
          return (C)this.InnerList[index];
        } else {
          return null;
        }
      }
    }

    public C this[int index] {
      get => (C)this.InnerList[index];
      set => this.InnerList[index] = value;
    }

  }


  public class CommandComparer : System.Collections.IComparer {
    static CommandComparer _default;

    public static CommandComparer Default {
      get
      {
        if (_default is null)
          _default = new CommandComparer ();
        return _default;
      }
    }

    int System.Collections.IComparer.Compare (object o1, object o2) {
      return string.Compare (o1.ToString (), o2.ToString (), true);
    }
  }


}