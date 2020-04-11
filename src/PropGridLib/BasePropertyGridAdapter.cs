using System;
using System.ComponentModel;
using System.Reflection;

namespace audiamus.aux.propgrid {
  public enum EReadonly { none, minimum, search, modify, modifyExt, all };
  public enum EVisibility { none, minmum, search, all };

  /// <summary>
  /// <para>Abstract base class tailored as a PropertyGrid subject.
  /// Supports property ordering and dynamic readonly and visibility flags 
  /// for individual properties. </para>
  /// <para>Adds simple DataSource property for external representation of the data to
  /// become the PropertyGrid subject ("SelectedObject").</para>
  /// </summary>
  /// <typeparam name="T">Type of external data representation.</typeparam>
  [TypeConverter (typeof (PropertySorter))]
  public abstract class BasePropertyGridAdapter<T> : DynamicTypeDescriptor {

    #region private fields
    bool _isReadOnly;
    Action _refreshDelegate;

    readonly T _datasource;

    #endregion fields
    #region properties

    [Browsable (false)]
    public virtual T DataSource {
      get
      {
        return _datasource;
      }
    }

    [Browsable (false)]
    public Action RefreshDelegate
    {
      protected get
      {
        return _refreshDelegate;
      }
      set
      {
        if (RefreshDelegate == value)
          return;
        _refreshDelegate = value;
        RefreshDelegate?.Invoke();
      }
    }
    #endregion properties
    #region ctor

    protected BasePropertyGridAdapter (T datasource) {
      this._datasource = datasource;

      Type t = GetType ();

      MemberInfo [] memberinfos = t.FindMembers 
        (MemberTypes.Property,
         BindingFlags.Public | BindingFlags.Instance, // | BindingFlags.DeclaredOnly,
         new MemberFilter(delegateToSearchCriteria),
         null);

      foreach (MemberInfo mi in memberinfos) {
        PropertyInfo pi = mi as PropertyInfo;
        if (pi == null)
          continue;

        string name = pi.Name;
        if (!PropertyCommands.Contains (name)) {
          PropertyCommands.Add (new
            PropertyCommand (name, true, false));
        }
      }
    }

    #endregion ctor
    #region private static methods
    static bool delegateToSearchCriteria (MemberInfo objMemberInfo, Object objSearch) {
      PropertyInfo pi = objMemberInfo as PropertyInfo;
      if (pi == null)
        return false;

      object[] aAttributes = pi.GetCustomAttributes (typeof (BrowsableAttribute), false);

      bool c = true;
      if (aAttributes != null && aAttributes.Length > 0) {
        BrowsableAttribute ba = aAttributes[0] as BrowsableAttribute;
        if (ba != null)
          c = ba.Browsable;
      }

      return c;
    }


    #endregion static methods
    #region public virtual methods

    public virtual void SetReadonly (EReadonly modifier) {
      if (!readOnlyChange (modifier))
        return;
      bool ro = modifier == EReadonly.all;
      _isReadOnly = ro;
      foreach (PropertyCommand pc in PropertyCommands) {
        pc.ReadOnly = ro;
      }
    }


    public virtual void SetVisible (EVisibility modifier) {
      bool vis = modifier == EVisibility.all;
      foreach (PropertyCommand pc in PropertyCommands) {
        pc.Visible = vis;
      }
    }

    #endregion
    #region protected methods
    protected bool readOnlyChange (EReadonly modifier) {
      return modifier == EReadonly.all && !_isReadOnly || modifier == EReadonly.none && _isReadOnly;
    }
    #endregion
  }


}
