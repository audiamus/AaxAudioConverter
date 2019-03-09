using System;
using System.ComponentModel;
using System.Linq;
using System.Resources;

namespace audiamus.aux.propgrid {

  public class DynamicTypeDescriptor : ICustomTypeDescriptor, ISupportInitialize {
    const string PROPERTY_CONTROL = "Property Control";

    #region Private Fields

    private PropertyCommands _propertyCommands;
    private CategoryCommands _categoryCommands;
    private PropertyDescriptorCollection _dynamicProps;
    private readonly bool _isInDesignMode;

    #endregion Private Fields
    #region Public Properties

    [Category (PROPERTY_CONTROL)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
    [Browsable (false)]
    public CategoryCommands CategoryCommands {
      get
      {
        if (_categoryCommands == null) {
          _categoryCommands = new CategoryCommands ();
        }
        return _categoryCommands;
      }
    }

    [Category (PROPERTY_CONTROL)]
    [DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
    [Browsable (false)]
    public PropertyCommands PropertyCommands {
      get
      {
        if (_propertyCommands == null) {
          _propertyCommands = new PropertyCommands ();
        }
        return _propertyCommands;
      }
    }

    #endregion Public Properties
    #region Public Constructors

    public DynamicTypeDescriptor () {
      _isInDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
    }

    #endregion Public Constructors

    #region Public Methods

    public virtual void BeginInit () {

    }

    public virtual void EndInit () {
      if (PropertyCommands.Count == 0) {
        fillPropCommColl ();
      }
      if (CategoryCommands.Count == 0) {
        fillCatCommColl ();
      }
    }

    public PropertyCommand[] GetPropertyCommands () {
      PropertyDescriptorCollection baseProps = TypeDescriptor.GetProperties (this, true);
      PropertyCommand[] pcs = new PropertyCommand[baseProps.Count];

      for (int i = 0; i < baseProps.Count; i++) {
        pcs[i] = new PropertyCommand (baseProps[i].Name, baseProps[i].IsBrowsable, baseProps[i].IsReadOnly);
      }
      return pcs;
    }

    public bool ShouldSerializeCategoryCommands () {
      return true;
    }

    public bool ShouldSerializePropertyCommands () {
      return true;
    }

    #endregion Public Methods
    #region Protected Methods

    protected void fillCatCommColl () {
      PropertyDescriptorCollection baseProps = TypeDescriptor.GetProperties (this, true);

      foreach (PropertyDescriptor oProp in baseProps) {
        if (!oProp.DesignTimeOnly && oProp.IsBrowsable && oProp.Category != PROPERTY_CONTROL) {
          CategoryCommand cc = new CategoryCommand (oProp.Category, true);
          if (CategoryCommands.BinarySearch (cc) < 0) {
            CategoryCommands.Add (cc);
          }
        }
      }
      CategoryCommands.Sort ();

    }

    protected void fillPropCommColl () {
      PropertyDescriptorCollection baseProps = TypeDescriptor.GetProperties (this, true);

      foreach (PropertyDescriptor oProp in baseProps) {
        if (!oProp.DesignTimeOnly && oProp.IsBrowsable && oProp.Category != PROPERTY_CONTROL) {
          PropertyCommand pc = new PropertyCommand (oProp.Name, oProp.IsBrowsable, oProp.IsReadOnly);
          PropertyCommands.Add (pc);
        }
      }
      PropertyCommands.Sort ();

    }

    #endregion Protected Methods

    #region "TypeDescriptor Implementation"

    public AttributeCollection GetAttributes () => TypeDescriptor.GetAttributes (this, true);

    public String GetClassName () => TypeDescriptor.GetClassName (this, true);

    public String GetComponentName () => TypeDescriptor.GetComponentName (this, true);

    public TypeConverter GetConverter () => TypeDescriptor.GetConverter (this, true);

    public EventDescriptor GetDefaultEvent () => TypeDescriptor.GetDefaultEvent (this, true);

    public PropertyDescriptor GetDefaultProperty () => TypeDescriptor.GetDefaultProperty (this, true);

    public object GetEditor (Type editorBaseType) => TypeDescriptor.GetEditor (this, editorBaseType, true);

    public EventDescriptorCollection GetEvents (Attribute[] attributes) => TypeDescriptor.GetEvents (this, attributes, true);

    public EventDescriptorCollection GetEvents () => TypeDescriptor.GetEvents (this, true);

    public PropertyDescriptorCollection GetProperties (Attribute[] attributes) {

      if (!_isInDesignMode) {
        PropertyDescriptorCollection baseProps = TypeDescriptor.GetProperties (this, attributes, true);
        _dynamicProps = new PropertyDescriptorCollection (null);

        foreach (PropertyDescriptor oProp in baseProps) {

          if (oProp.Category != PROPERTY_CONTROL) {
            if ((PropertyCommands[oProp.Name] == null || PropertyCommands[oProp.Name].Visible) && (CategoryCommands[oProp.Category] == null || CategoryCommands[oProp.Category].Visible)) {
              _dynamicProps.Add (new DynamicPropertyDescriptor (this, oProp));
            }
          }
        }
        return _dynamicProps;
      }

      return TypeDescriptor.GetProperties (this, attributes, true);

    }

    public PropertyDescriptorCollection GetProperties () {

      if (_dynamicProps == null) {
        PropertyDescriptorCollection baseProps = TypeDescriptor.GetProperties (this, true);
        _dynamicProps = new PropertyDescriptorCollection (null);

        foreach (PropertyDescriptor oProp in baseProps) {

          _dynamicProps.Add (new DynamicPropertyDescriptor (this, oProp));

        }
      }
      return _dynamicProps;
    }

    public object GetPropertyOwner (PropertyDescriptor pd) {
      return this;
    }
    #endregion
  }

  public class DynamicPropertyDescriptor : PropertyDescriptor {
    #region Private Fields

    private readonly DynamicTypeDescriptor _instance;
    private readonly PropertyDescriptor _basePropertyDescriptor;
    private string _localizedCategory;
    private string _localizedDescription;
    private string _localizedName;

    #endregion Private Fields
    #region Public Properties

    public override Type ComponentType => _basePropertyDescriptor.ComponentType;

    public override Type PropertyType => this._basePropertyDescriptor.PropertyType;

    public override bool IsReadOnly {
      get
      {
        if (_instance.PropertyCommands[this.Name] is PropertyCommand pc) {
          return pc.ReadOnly;
        } else {
          return this._basePropertyDescriptor.IsReadOnly;
        }
      }
    }

    public override string DisplayName {
      get
      {
        if (_localizedName is null)
          this._localizedName = getLocalized (pd => pd.Name, pd => pd.DisplayName, a => a.NameTag);
        return this._localizedName;
      }
    }

    public override string Category {
      get
      {
        if (_localizedCategory is null)
          this._localizedCategory = getLocalized (pd => pd.Category, a => a.CategoryTag, "Category");
        return this._localizedCategory;
      }
    }

    public override string Description {
      get
      {
        if (_localizedDescription is null)
          this._localizedDescription = getLocalized (pd => pd.Name, pd => pd.Description, a => a.DescriptionTag, "Description");
        return this._localizedDescription;
      }
    }
    #endregion Public Properties

    #region Public Constructors

    public DynamicPropertyDescriptor (DynamicTypeDescriptor instance, PropertyDescriptor basePropertyDescriptor)
                              : base (basePropertyDescriptor) {
      this._instance = instance;
      this._basePropertyDescriptor = basePropertyDescriptor;
    }

    #endregion Public Constructors

    #region Public Methods
    public override object GetValue (object component) => this._basePropertyDescriptor.GetValue (component);

    public override void SetValue (object component, object value) => this._basePropertyDescriptor.SetValue (component, value);

    public override bool CanResetValue (object component) => _basePropertyDescriptor.CanResetValue (component);

    public override void ResetValue (object component) => this._basePropertyDescriptor.ResetValue (component);

    public override bool ShouldSerializeValue (object component) => this._basePropertyDescriptor.ShouldSerializeValue (component);

    #endregion Public Methods
    #region Private Methods

    private string getLocalized (
      Func<PropertyDescriptor, string> propdescTagDelgat,
      Func<PropertyDescriptor, string> propdescNameDelgat,
      Func<GlobalizedPropertyAttribute, string> attrDelgat = null,
      string tagSuffix = null) 
    {
      return getLocalized (this._basePropertyDescriptor, propdescTagDelgat, propdescNameDelgat, attrDelgat, tagSuffix);
    }

    private string getLocalized (
      Func<PropertyDescriptor, string> propdescTagDelgat,
      Func<GlobalizedPropertyAttribute, string> attrDelgat = null,
      string tagSuffix = null) 
    {
      return getLocalized (this._basePropertyDescriptor, propdescTagDelgat, propdescTagDelgat, attrDelgat, tagSuffix);
    }

    private static string getLocalized (
      PropertyDescriptor propdesc,
      Func<PropertyDescriptor, string> propdescTagDelgat,
      Func<PropertyDescriptor, string> propdescNameDelgat,
      Func<GlobalizedPropertyAttribute, string> attrDelgat = null,
      string tagSuffix = null) 
    {
      // First lookup the property if GlobalizedPropertyAttribute instances are available. 
      // If yes, then try to get resource table name and display name id from that attribute.
      string tableName = null;
      string localizeNameTag = null;
      if (!(attrDelgat is null)) {
        var attr = getAttribute<GlobalizedPropertyAttribute> (propdesc);
        if (!(attr is null)) {
          tableName = attr.ResourceTable;
          localizeNameTag = attrDelgat (attr);
        }
      }

      // If no resource table specified by attribute, then build it itself by using namespace and class name.
      if (tableName is null)
        tableName = propdesc.ComponentType.FullName;

      // If no display name id is specified by attribute, then construct it by using default display name (usually the property name) 
      if (localizeNameTag is null)
        localizeNameTag = propdescTagDelgat (propdesc);

      localizeNameTag += tagSuffix;

      // Now use table name and display name id to access the resources.  
      ResourceManager rm = new ResourceManager (tableName, propdesc.ComponentType.Assembly);

      // Get the string from the resources. 
      // If this fails, then use default display name (usually the property name) 
      string s = null;
      try {
        s = rm.GetString (localizeNameTag);
      } catch (MissingManifestResourceException) {
      }

      string localizedName = s ?? propdescNameDelgat (propdesc);

      return localizedName;
    }

    private static T getAttribute<T> (PropertyDescriptor propdesc) where T : Attribute {
      var attr = propdesc.Attributes
        .Cast<Attribute> ()
        .Where (a => a.GetType ().Equals (typeof (T)))
        .SingleOrDefault () as T;
      return attr;
    }
    #endregion Private Methods
  }
}