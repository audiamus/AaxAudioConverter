using System;

namespace audiamus.aaxconv.lib {
  class FixAtomESDS : IFileCopyCallout {

    static readonly byte[] ESDS_ATOM = {0x05,0x80,0x80,0x80,0x02};
    static readonly byte[] PARA = {0x12,0x12};
    static readonly byte[] PARA_FIX = {0x12,0x10};

    void IFileCopyCallout.ProcessBuffer (byte[] buffer, int size, long globalOffset) {
      if (globalOffset > 0)
        return;

      int idxAtom = findFirstPattern (buffer, 0, size, ESDS_ATOM);
      if (idxAtom < 0)
        return;

      int idxPara = idxAtom + ESDS_ATOM.Length;

      int idx2 = findFirstPattern (buffer, idxPara, size, PARA, true);
      if (idx2 != idxPara)
        return;

      for (int i = 0; i < PARA_FIX.Length; i++)
        buffer[i + idxPara] = PARA_FIX[i];
      
    }

    private static int findFirstPattern (byte[] source, int offset, int size, byte[] pattern, bool noIteration = false) {
      int len = Math.Min (source.Length, size);
      int max = len - pattern.Length;
      if (noIteration)
        max = offset + 1;
      for (int i = offset; i < max; i++) {
        for (int j = 0; j < pattern.Length; j++) {
          if (source[i + j] != pattern[j])
            break;
          if (j == pattern.Length - 1)
            return i;
        }
      }
      return -1;
    }
  }
}
