using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using audiamus.aux.ex;
using Newtonsoft.Json;
using static audiamus.aux.Logging;
using AA = audiamus.aaxconv.lib.AudibleAppContentMetadata;

namespace audiamus.aaxconv.lib {
  class AudibleAppSimsBySeries {

    delegate IEnumerable<json.Product> GetProductsDelegate (string json); 

    const string JSON = ".json";
    const string SIMSBYSERIES = "SimsBySeries";
    const string ASIN_JSON_PATTERN = "??????????.json";
    const string SERIES_TITLES = "series_titles_";

    private readonly HashSet<string> _discardedFiles = new HashSet<string> ();


    public void GetSimsBySeries (Book.Part part, bool fileOnly = false) {
      if (part.Book.ExternalMeta != null)
        return;

      var filename = part.AaxFileItem.FileName;
      Log (3, this, () => $"\"{filename.SubstitUser ()}\", file only={fileOnly}");

      // either ASIN.json or series_titles_ASIN.json
      bool succ = findSeriesFile (part, fileOnly, string.Empty, getProductsSingle);
      if (!succ)
        findSeriesFile (part, fileOnly, SERIES_TITLES, getProductsMulti);
    }

    private bool findSeriesFile (Book.Part part, bool fileOnly, string filePrefix, GetProductsDelegate getProducts) {
      try {
        var simsBySeriesFile = findSeriesFileDirectOrSku (part, fileOnly, filePrefix, getProducts);
        if (simsBySeriesFile is null || simsBySeriesFile.Filename is null) {
          part.AaxFileItem.SimsBySeriesFile = new AA.AsinJsonFile (null, null);
          return false;
        }

        part.AaxFileItem.SimsBySeriesFile = simsBySeriesFile;
        string metafile = simsBySeriesFile.Filename;
        Log (3, this, () => $"\"{metafile.SubstitUser ()}\"");
        return true;
      } catch (Exception exc) {
        Log (1, this, () => exc.ToShortString ());
        return false;
      }
    }

    private AA.AsinJsonFile findSeriesFileDirectOrSku (Book.Part part, bool fileOnly, string filePrefix, GetProductsDelegate getProducts) {
      // try to find sims by series file, either locally, relative or absolute
      // precondition content meta file is available
      // either by ASIN (single-part book) or by SKU (multipart book)


      // get asin
      var afi = part.AaxFileItem;
      string asin = afi.ContentMetadataFile?.ASIN;
      if (asin is null) {
        Log (3, this, () => $"ASIN not found.");
        return null;
      }
      Log (3, this, () => $"ASIN={asin}{(filePrefix.IsNullOrWhiteSpace() ? string.Empty : $", prefix={filePrefix}")}");

      string contentDir = Path.GetDirectoryName (afi.FileName).StripUnc ();
      string simsBySeriesFile = filePrefix + asin + JSON;

      // find by asin (should succeed for single-part books)
      var asinJsonFile = AA.AsinJsonFile.FindFile (simsBySeriesFile, contentDir, SIMSBYSERIES, asin);
      if (asinJsonFile != null) {
        if (!fileOnly)
          extractMetaData (asinJsonFile, part);
        return asinJsonFile;
      }

      return findSeriesFileBySku (part, contentDir, fileOnly, filePrefix, getProducts);

    }

    private AA.AsinJsonFile findSeriesFileBySku (Book.Part part, string contentDirPath, bool fileOnly, string filePrefix, GetProductsDelegate getProducts) {
      // if asin direct doesn't succeed, try sku:
      // use a hash set of string for the discarded filenames.
      // for local, relative and absolute directories:
      // get all json file names
      // if not in hash set:
      // deserialize json
      // get and compare sku
      // if match get meta data en route
      // if no match add to hash set

      if (part.SKU.IsNullOrWhiteSpace ())
        return null;

      string filePattern = filePrefix + ASIN_JSON_PATTERN;

      {
        var asinJsonFile = findSeriesFileBySkuSubDir (part, contentDirPath, fileOnly, filePattern, getProducts);
        if (asinJsonFile != null)
          return asinJsonFile;
      }

      {
        string relativePath = AA.AsinJsonFile.GetRelativePath (contentDirPath, SIMSBYSERIES);
        if (relativePath != null) {
          var asinJsonFile = findSeriesFileBySkuSubDir (part, relativePath, fileOnly, filePattern, getProducts);
          if (asinJsonFile != null)
            return asinJsonFile;
        }
      }

      {
        var absolutePaths = AA.AsinJsonFile.GetAbsolutePaths (SIMSBYSERIES);
        if (absolutePaths != null) {
          foreach (var absolutePath in absolutePaths) {
            var asinJsonFile = findSeriesFileBySkuSubDir (part, absolutePath, fileOnly, filePattern, getProducts);
            if (asinJsonFile != null)
              return asinJsonFile;
          }
        }
      }

      return null;
    }

    private AA.AsinJsonFile findSeriesFileBySkuSubDir (
      Book.Part part, string subDirPath, bool fileOnly, string filePattern, GetProductsDelegate getProducts
    ) {
      var filepaths = Directory.GetFiles (subDirPath, filePattern);
      var filenames = filepaths.Select (p => Path.GetFileName (p));
      filenames = filenames.Except (_discardedFiles);

      foreach (var filename in filenames) {
        string asinfile = Path.Combine (subDirPath, filename).AsUncIfLong();
        string json = File.ReadAllText (asinfile);

        var products = getProducts (json);

        string asin = findBySku (products, part, fileOnly);
        if (asin != null)
          return new AA.AsinJsonFile (asinfile, asin);

        _discardedFiles.Add (filename);
      }

      return null;
    }

    IEnumerable<json.Product> getProductsSingle (string json) {
      var metadata = JsonConvert.DeserializeObject<json.AppSimsBySeries> (json);
      return new[] { metadata.product };
    }

    IEnumerable<json.Product> getProductsMulti(string json) {
      var metadata = JsonConvert.DeserializeObject<json.AppSeriesTitles> (json);
      return metadata.similar_products;
    }

    private string findBySku (IEnumerable<json.Product> products, Book.Part part, bool fileOnly) {
      foreach (var product in products) {
        string sku = product?.sku;
        string skuLite = product?.sku_lite;

        if (part.SKU.StartsWith (sku) || part.SKU.StartsWith (skuLite)) {
          Log (3, this, () => $"ASIN={product.asin}, SKU(lite)={product.sku_lite}");
          if (!fileOnly)
            extractMetaData (product, part);
          return product.asin;
        }
      }
      return null;
    }

    private void extractMetaData (AA.AsinJsonFile asinJsonFile, Book.Part part) {
      string json = File.ReadAllText (asinJsonFile.Filename);
      var metadata = JsonConvert.DeserializeObject<json.AppSimsBySeries> (json);
      extractMetaData (metadata.product, part);
    }

    private void extractMetaData (json.Product product, Book.Part part) {
      var book = part.Book;
      if (book.ExternalMeta is null) {
        var extra = new Book.ExternalBookMeta {
          Authors = product.authors.Select (s => s.name).ToList (),
          Book = product.title,
          Series = product.series?.Select (s => (s.title, s.sequence)).ToList (),
        };
        Log (3, this, () => $"\"{extra}\"");
        part.Book.ExternalMeta = extra;
      }
    }

  }
}
