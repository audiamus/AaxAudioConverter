using System.IO;

namespace audiamus.aux {
  /// <summary>
  /// Automatic indentation, managed as a resource
  /// </summary>
  public class Indent : IResource {

    uint _inc = 2;
    uint _offset = 0;
    int _indent;
    string _output = string.Empty;

    public int Level { get; private set; }

    public Indent () { }
    public Indent (uint inc) => this._inc = inc;
    public Indent (uint? inc, uint offset) {
      this._inc = inc ?? this._inc;
      this._offset = offset;
      buildString ();
    }

    public void Acquire () {
      Level++;
      _indent += (int)_inc;
      buildString ();
    }

    public void Release () {
      Level--;
      _indent -= (int)_inc;
      buildString ();
    }

    public bool InRange (int level) {
      if (level < 0)
        return true;
      else
        return Level <= level;
    }

    public override string ToString () => _output;

    public void Write (TextWriter osm) => osm.Write (this);

    private void buildString () => _output = new string (' ', (int)_offset + _indent);

  }
}
