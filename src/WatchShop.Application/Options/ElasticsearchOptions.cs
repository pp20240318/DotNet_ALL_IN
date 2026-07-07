namespace WatchShop.Application.Options;

public class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public bool Enabled { get; set; }
    public string Uri { get; set; } = "http://localhost:9200";
    public string IndexName { get; set; } = "watchshop-catalog";
}
