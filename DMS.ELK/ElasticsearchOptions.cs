namespace DMS.ELK;

public class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Url { get; set; } = string.Empty;
    public string IndexName { get; set; } = "documents";
}
