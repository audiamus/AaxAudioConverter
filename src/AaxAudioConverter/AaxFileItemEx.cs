using System;
using System.Windows.Forms;
using audiamus.aaxconv.lib;

namespace audiamus.aaxconv {

  class AaxFileItemEx : IEquatable<AaxFileItemEx> {
    public AaxFileItem FileItem {get; private set;}
    public ListViewItem ListViewItem {get; set;}

    public AaxFileItemEx (AaxFileItem fi) {
      if (fi is null)
        throw new ArgumentNullException ();
      FileItem = fi;
    }

    public bool Equals (AaxFileItemEx other) {
      //Check whether the compared objects reference the same data.
      if (Object.ReferenceEquals (this, other))
        return true;

      //Check whether any of the compared objects is null.
      if (other is null || other.FileItem is null)
        return false;

      //Check whether the items' properties are equal.
      return this.FileItem.Equals (other.FileItem);
    }

    public override int GetHashCode () {
      //Check whether the object is null
      if (FileItem is null)
        return 0;

      return FileItem.GetHashCode ();
    }
  }

}
