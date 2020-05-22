using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace audiamus.aux.diagn {
  static class TypeExtension {

    public static IEnumerable<IEnumerable<Type>> GetInterfaceHierarchy (this Type root) {
      var leavesDict = new Dictionary<Type, List<Type>> ();
      findLeaves (root, new List<Type> (), leavesDict);
      List<List<Type>> list = sort (leavesDict);
      return list;
    }

    public static string ToHierarchyString (this IEnumerable<Type> path) {
      var sb = new StringBuilder ();
      foreach (var t in path) {
        if (sb.Length > 0)
          sb.Append (':');
        sb.Append (t.Name);
      }
      return sb.ToString ();
    } 

    private static void findLeaves (Type type, IList<Type> path, IDictionary<Type, List<Type>> leaves) {
      if (type.IsInterface) {
        path.Add (type);
        bool succ = leaves.TryGetValue (type, out var p);
        if (!succ || path.Count > p.Count)
          leaves[type] = path.ToList ();
      }

      var ifcTypes = type.GetInterfaces ();
      foreach (var ifcType in ifcTypes)
        findLeaves (ifcType, path.ToList(), leaves);
      
    }

    private static List<List<Type>> sort (Dictionary<Type, List<Type>> leavesDict) {
      var rawlist = leavesDict.Select (kvp => kvp.Value).OrderBy (k => k.Count).ToList ();
      var list = new List<List<Type>> ();

      while (rawlist.Count () > 0) {
        var path = rawlist.Last ();
        rawlist.RemoveAt (rawlist.Count - 1);
        list.Add (path);
        if (path.Count < 2)
          continue;
        var p = path.ToList ();
        while (p.Count > 1) {
          p.RemoveAt (p.Count - 1);
          Type deriv = p[p.Count - 1];
          var dpath = rawlist.Where (k => k.Last () == deriv).FirstOrDefault ();
          if (dpath is null)
            break;
          rawlist.Remove (dpath);
          list.Add (dpath);
        }
      }

      return list;
    }
  }
}
