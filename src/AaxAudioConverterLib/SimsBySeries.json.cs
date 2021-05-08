namespace audiamus.aaxconv.lib.json {


  public class AppSimsBySeries {
    public Product product { get; set; }
    public string[] response_groups { get; set; }
  }

  public class Product {
    public string asin { get; set; }
    public Author[] authors { get; set; }
    public AvailableCodecs[] available_codecs { get; set; }
    public string content_delivery_type { get; set; }
    public string content_type { get; set; }
    public string format_type { get; set; }
    public bool has_children { get; set; }
    public bool is_adult_product { get; set; }
    public bool is_listenable { get; set; }
    public string issue_date { get; set; }
    public string language { get; set; }
    public string merchandising_summary { get; set; }
    public Narrator[] narrators { get; set; }
    public ProductImages product_images { get; set; }
    public string publisher_name { get; set; }
    public Rating rating { get; set; }
    public string release_date { get; set; }
    public int runtime_length_min { get; set; }
    public string sample_url { get; set; }
    public Series[] series { get; set; }
    public string sku { get; set; }
    public string sku_lite { get; set; }
    public string subtitle { get; set; }
    public string[] thesaurus_subject_keywords { get; set; }
    public string title { get; set; }
    public string publication_name { get; set; }
  }

  public class ProductImages {
    public string _300 { get; set; }
  }

  public class Rating {
    public int num_reviews { get; set; }
    public OverallDistribution overall_distribution { get; set; }
    public PerformanceDistribution performance_distribution { get; set; }
    public StoryDistribution story_distribution { get; set; }
  }

  public class OverallDistribution {
    public float average_rating { get; set; }
    public string display_average_rating { get; set; }
    public float display_stars { get; set; }
    public int num_five_star_ratings { get; set; }
    public int num_four_star_ratings { get; set; }
    public int num_one_star_ratings { get; set; }
    public int num_ratings { get; set; }
    public int num_three_star_ratings { get; set; }
    public int num_two_star_ratings { get; set; }
  }

  public class PerformanceDistribution {
    public float average_rating { get; set; }
    public string display_average_rating { get; set; }
    public float display_stars { get; set; }
    public int num_five_star_ratings { get; set; }
    public int num_four_star_ratings { get; set; }
    public int num_one_star_ratings { get; set; }
    public int num_ratings { get; set; }
    public int num_three_star_ratings { get; set; }
    public int num_two_star_ratings { get; set; }
  }

  public class StoryDistribution {
    public float average_rating { get; set; }
    public string display_average_rating { get; set; }
    public float display_stars { get; set; }
    public int num_five_star_ratings { get; set; }
    public int num_four_star_ratings { get; set; }
    public int num_one_star_ratings { get; set; }
    public int num_ratings { get; set; }
    public int num_three_star_ratings { get; set; }
    public int num_two_star_ratings { get; set; }
  }

  public class Author {
    public string asin { get; set; }
    public string name { get; set; }
  }

  public class AvailableCodecs {
    public string enhanced_codec { get; set; }
    public string format { get; set; }
    public bool is_kindle_enhanced { get; set; }
    public string name { get; set; }
  }

  public class Narrator {
    public string name { get; set; }
  }

  public class Series {
    public string asin { get; set; }
    public string sequence { get; set; }
    public string title { get; set; }
    public string url { get; set; }
  }

}
