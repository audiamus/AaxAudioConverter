using System;
using System.Linq;
using System.Resources;
using System.Windows.Forms;

namespace audiamus.aux.win {
  public class ComboBoxEnumAdapter<TEnum> : EnumConverter<TEnum> where TEnum : struct, Enum {
    private readonly ComboBox _comboBox;

    public TEnum Value => getValue ();

    public ComboBoxEnumAdapter (ComboBox comboBox, ResourceManager resmgr, TEnum value) {
      _comboBox = comboBox;
      base.ResourceManager = resmgr;
      var stringValues = Values.Select (v => ConvertTo (v, typeof (string))).ToArray ();
      comboBox.Items.Clear ();
      comboBox.Items.AddRange (stringValues);

      int idx = Values.IndexOf (value);
      comboBox.SelectedIndex = idx;
    }


    private TEnum getValue () {
      int idx = _comboBox.SelectedIndex;
      if (idx < 0)
        return default;
      return Values.ElementAt (idx);
    }
  }
}
