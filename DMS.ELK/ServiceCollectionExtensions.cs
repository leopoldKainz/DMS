using DMS.ELK.Interfaces;
using DMS.ELK.Services;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DMS.ELK;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSearchIndex(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(ElasticsearchOptions.SectionName);
        var options = section.Get<ElasticsearchOptions>()
            ?? throw new InvalidOperationException($"Missing '{ElasticsearchOptions.SectionName}' configuration section.");

        if (string.IsNullOrWhiteSpace(options.Url))
        {
            throw new InvalidOperationException($"'{ElasticsearchOptions.SectionName}:Url' is not configured.");
        }

        services.Configure<ElasticsearchOptions>(section);
        services.AddSingleton(sp =>
        {
            var elasticOptions = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            var settings = new ElasticsearchClientSettings(new Uri(elasticOptions.Url))
                .DefaultIndex(elasticOptions.IndexName);
            return new ElasticsearchClient(settings);
        });
        services.AddSingleton<ISearchIndex, ElasticSearchIndex>();

        return services;
    }
}
