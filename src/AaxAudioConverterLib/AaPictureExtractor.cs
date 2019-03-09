using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using audiamus.aux.ex;

namespace audiamus.aaxconv.lib {
  class AaPictureExtractor {
    public readonly static IReadOnlyList<byte> JPG_HDR = new byte[] { 0xff, 0xd8, 0xff};
    public readonly static IReadOnlyList<byte> PNG_HDR = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

    const uint AA_MAGIC = 1469084982;

    public static byte [] Extract (string path) {
      using (var reader = new BinaryReader (File.OpenRead (path))) {
        uint filelen = reader.ReadUInt32BE ();
        uint magnum = reader.ReadUInt32BE ();
        if (AA_MAGIC != magnum)
          return null;

        uint tocsize = reader.ReadUInt32BE ();
        if (tocsize > 16)
          return null;
        reader.ReadUInt32 (); // skip
        var toc = new List<(uint start, uint len)> ();
        for (int i = 0; i < tocsize; i++) {
          reader.ReadUInt32 (); // skip
          uint offs = reader.ReadUInt32BE ();
          uint len = reader.ReadUInt32BE ();
          toc.Add ((offs, len));
        }

        var lastBlock = toc.LastOrDefault ();
        if (lastBlock.len < 4)
          return null;

        reader.BaseStream.Seek (lastBlock.start, SeekOrigin.Begin);

        uint plen = reader.ReadUInt32BE ();
        reader.ReadUInt32 ();

        if (plen < JPG_HDR.Count)
          return null;

        byte[] pic = reader.ReadBytes ((int)plen);
        var hdr = new ArraySegment<byte> (pic, 0, JPG_HDR.Count);
        if (!JPG_HDR.SequenceEqual (hdr))
          return null;

        return pic;

      }
    } 
  }
}
