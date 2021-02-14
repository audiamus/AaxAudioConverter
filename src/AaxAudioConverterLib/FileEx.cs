using System;
using audiamus.aux.w32;

namespace audiamus.aaxconv.lib {
  interface IFileCopyCallout {
    void ProcessBuffer (byte[] buffer, int size, long globalOffset);
  }

  static class FileEx {

    private const int BUFSIZ = 10 * 1000 * 1000; // 10 MB
    private const int IVL_MS = 200;


    public static bool Copy (string sourceFileName, string destFileName, bool overwrite, 
        Action<ProgressMessage> report = null, Func<bool> cancel = null) =>
     Copy (sourceFileName, destFileName, overwrite, null, report, cancel);

    public static bool Copy (string sourceFileName, string destFileName, bool overwrite, IFileCopyCallout callout, 
      Action<ProgressMessage> report = null, Func<bool> cancel = null
    ) {
      byte[] buf = new byte[BUFSIZ];

      DateTime dt0 = DateTime.Now;
      long ivlcnt = 0; 

      long total = new System.IO.FileInfo (sourceFileName).Length;
      long count = 0;

      using (var threadProgress = new ThreadProgress (report)) { 
        using (WinFileIO wfioRd = new WinFileIO (buf), wfioWr = new WinFileIO (buf)) {
          wfioRd.OpenForReading (sourceFileName);
          wfioWr.OpenForWriting (destFileName, overwrite);

          int read = 0;
          while (true) {

            if (cancel?.Invoke () ?? false)
              return false;

            read = wfioRd.ReadBlocks (BUFSIZ);
            if (read <= 0)
              break;

            callout?.ProcessBuffer (buf, read, count);

            wfioWr.Write (read);
            
            count += read;
            DateTime dt = DateTime.Now;
            long tot_ms = (int)(dt - dt0).TotalMilliseconds;
            long q = tot_ms / IVL_MS;
            if (q <= ivlcnt)
              continue;

            ivlcnt = q;
            threadProgress.Report ((double)count / total);
          };
        }
      }
      return true;
    }
  }
}
