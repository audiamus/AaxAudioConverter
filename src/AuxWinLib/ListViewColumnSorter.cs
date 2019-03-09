using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace audiamus.aux.win {
  /// <summary>
  /// This class is an implementation of the 'IComparer' interface.
  /// Based on http://www.codeproject.com/Articles/5332/ListView-Column-Sorter 
  /// </summary>
  public class ListViewColumnSorter : IComparer {
    public enum ESortModifiers {
      SortByImage,
      SortByCheckbox,
      SortByText,
      SortByTagOrText
    }

    /// <summary>
    /// Specifies the column to be sorted
    /// </summary>
    private int _columnToSort;
    /// <summary>
    /// Specifies the order in which to sort (i.e. 'Ascending').
    /// </summary>
    private SortOrder _orderOfSort;
    
    /// <summary>
    /// Case insensitive comparer object
    /// </summary>
    private NumberCaseInsensitiveComparer _objectCompare;
    private ImageTextComparer _firstObjectCompare;
    private CheckboxTextComparer _firstObjectCompare2;

    private ESortModifiers _sortModifier = ESortModifiers.SortByText;
    public ESortModifiers SortModifier {
      set {
        _sortModifier = value;
      }
      get {
        return _sortModifier;
      }
    }

    /// <summary>
    /// Class constructor.  Initializes various elements
    /// </summary>
    public ListViewColumnSorter () {
      // Initialize the column to '0'
      _columnToSort = 0;

      // Initialize the CaseInsensitiveComparer object
      _objectCompare = new NumberCaseInsensitiveComparer ();
      _firstObjectCompare = new ImageTextComparer ();
      _firstObjectCompare2 = new CheckboxTextComparer ();
    }

    /// <summary>
    /// This method is inherited from the IComparer interface.  It compares the two objects passed using a case insensitive comparison.
    /// </summary>
    /// <param name="x">First object to be compared</param>
    /// <param name="y">Second object to be compared</param>
    /// <returns>The result of the comparison. "0" if equal, negative if 'x' is less than 'y' and positive if 'x' is greater than 'y'</returns>
    public int Compare (object x, object y) {
      int compareResult = 0;
      ListViewItem listviewX, listviewY;

      // Cast the objects to be compared to ListViewItem objects
      listviewX = (ListViewItem)x;
      listviewY = (ListViewItem)y;

      ListView listViewMain = listviewX.ListView;

      // Calculate correct return value based on object comparison
      if (listViewMain.Sorting != SortOrder.Ascending &&
          listViewMain.Sorting != SortOrder.Descending) {
        // Return '0' to indicate they are equal
        return compareResult;
      }

      if (_sortModifier.Equals (ESortModifiers.SortByText) || 
          _sortModifier.Equals (ESortModifiers.SortByTagOrText) || _columnToSort > 0) {
        // Compare the two items

        if (listviewX.SubItems.Count <= _columnToSort &&
            listviewY.SubItems.Count <= _columnToSort) {
          compareResult = _objectCompare.Compare (null, null);
        } else if (listviewX.SubItems.Count <= _columnToSort &&
                   listviewY.SubItems.Count > _columnToSort) {
          compareResult = _objectCompare.Compare (null, listviewY.SubItems[_columnToSort].Text.Trim ());
        } else if (listviewX.SubItems.Count > _columnToSort && listviewY.SubItems.Count <= _columnToSort) {
          compareResult = _objectCompare.Compare (listviewX.SubItems[_columnToSort].Text.Trim (), null);
        } else {
          if (listviewX.SubItems[_columnToSort].Tag != null && listviewX.SubItems[_columnToSort].Tag is IComparable &&
              listviewY.SubItems[_columnToSort].Tag != null && listviewY.SubItems[_columnToSort].Tag is IComparable)
            compareResult = _objectCompare.Compare (
              listviewX.SubItems[_columnToSort].Tag, 
              listviewY.SubItems[_columnToSort].Tag); 
          else
           compareResult = _objectCompare.Compare (
             listviewX.SubItems[_columnToSort].Text.Trim (), 
             listviewY.SubItems[_columnToSort].Text.Trim ());
        }
      } else {
        switch (_sortModifier) {
          case ESortModifiers.SortByCheckbox:
            compareResult = _firstObjectCompare2.Compare (x, y);
            break;
          case ESortModifiers.SortByImage:
            compareResult = _firstObjectCompare.Compare (x, y);
            break;
          default:
            //if (mySortModifier.Equals (ESortModifiers.SortByTagOrText))
            compareResult = _firstObjectCompare.Compare (x, y);
            break;
        }
      }

      // Calculate correct return value based on object comparison
      if (_orderOfSort == SortOrder.Ascending) {
        // Ascending sort is selected, return normal result of compare operation
        return compareResult;
      } else if (_orderOfSort == SortOrder.Descending) {
        // Descending sort is selected, return negative result of compare operation
        return (-compareResult);
      } else {
        // Return '0' to indicate they are equal
        return 0;
      }
    }

    /// <summary>
    /// Gets or sets the number of the column to which to apply the sorting operation (Defaults to '0').
    /// </summary>
    public int SortColumn {
      set {
        _columnToSort = value;
      }
      get {
        return _columnToSort;
      }
    }

    /// <summary>
    /// Gets or sets the order of sorting to apply (for example, 'Ascending' or 'Descending').
    /// </summary>
    public SortOrder Order {
      set {
        _orderOfSort = value;
      }
      get {
        return _orderOfSort;
      }
    }

  }

  public class ImageTextComparer : IComparer {
    //private CaseInsensitiveComparer ObjectCompare;
    private NumberCaseInsensitiveComparer _objectCompare;

    public ImageTextComparer () {
      // Initialize the CaseInsensitiveComparer object
      _objectCompare = new NumberCaseInsensitiveComparer ();
    }

    public int Compare (object x, object y) {
      //int compareResult;
      int image1, image2;
      ListViewItem listviewX, listviewY;

      // Cast the objects to be compared to ListViewItem objects
      listviewX = (ListViewItem)x;
      image1 = listviewX.ImageIndex;
      listviewY = (ListViewItem)y;
      image2 = listviewY.ImageIndex;

      if (image1 < image2) {
        return -1;
      } else if (image1 == image2) {
        return _objectCompare.Compare (listviewX.Text.Trim (), listviewY.Text.Trim ());
      } else {
        return 1;
      }
    }
  }

  public class CheckboxTextComparer : IComparer {
    private NumberCaseInsensitiveComparer ObjectCompare;

    public CheckboxTextComparer () {
      // Initialize the CaseInsensitiveComparer object
      ObjectCompare = new NumberCaseInsensitiveComparer ();
    }

    public int Compare (object x, object y) {
      // Cast the objects to be compared to ListViewItem objects
      ListViewItem listviewX = (ListViewItem)x;
      ListViewItem listviewY = (ListViewItem)y;

      if (listviewX.Checked && !listviewY.Checked) {
        return -1;
      } else if (listviewX.Checked.Equals (listviewY.Checked)) {
        if (listviewX.ImageIndex < listviewY.ImageIndex) {
          return -1;
        } else if (listviewX.ImageIndex == listviewY.ImageIndex) {
          return ObjectCompare.Compare (listviewX.Text.Trim (), listviewY.Text.Trim ());
        } else {
          return 1;
        }
      } else {
        return 1;
      }
    }
  }


  public class NumberCaseInsensitiveComparer : CaseInsensitiveComparer {
    public NumberCaseInsensitiveComparer () {

    }

    public new int Compare (object x, object y) {
      if (x is null && y is null) {
        return 0;
      } else if (x is null && y != null) {
        return -1;
      } else if (x != null && y is null) {
        return 1;
      }
      if ((x is System.String) && isWholeNumber ((string)x) && (y is System.String) && isWholeNumber ((string)y)) {
        try {
          return base.Compare (Convert.ToUInt64 (((string)x).Trim ()), Convert.ToUInt64 (((string)y).Trim ()));
        } catch {
          return -1;
        }
      } else {
        return base.Compare (x, y);
      }
    }

    private bool isWholeNumber (string strNumber) {
      Regex wholePattern = new Regex (@"^\d+$");
      return wholePattern.IsMatch (strNumber);
    }
  }

}
