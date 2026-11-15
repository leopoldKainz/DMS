using DMS.ELK.Exceptions;
using DMS.ELK.Interfaces;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DMS.ELK.Services;

public class ElasticSearchIndex : ISearchIndex
{
    private const int MaxResults = 100;

    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticSearchIndex> _logger;
    private readonly string _indexName;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexReady;

    public ElasticSearchIndex(ElasticsearchClient client, IOptions<ElasticsearchOptions> options, ILogger<ElasticSearchIndex> logger)
    {
        _client = client;
        _logger = logger;
        _indexName = options.Value.IndexName;
    }

    public async Task IndexAsync(SearchDocument document, CancellationToken ct = default)
    {
        await EnsureIndexAsync(ct);

        var response = await ExecuteAsync(
            () => _client.IndexAsync(document, i => i.Index(_indexName).Id(document.DocumentId.ToString()), ct),
            $"Could not index document {document.DocumentId}.");

        if (!response.IsValidResponse)
        {
            throw new SearchIndexException($"Could not index document {document.DocumentId}: {response.DebugInformation}");
        }

        _logger.LogInformation("Indexed document {DocumentId} in {Index}", document.DocumentId, _indexName);
    }

    public async Task<IReadOnlyList<Guid>> SearchAsync(string term, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return [];
        }

        await EnsureIndexAsync(ct);

        var response = await ExecuteAsync(
            () => _client.SearchAsync<SearchDocument>(s => s
                .Indices(_indexName)
                .Size(MaxResults)
                .Query(q => q.MultiMatch(m => m
                    .Query(term)
                    .Fields(Infer.Fields<SearchDocument>(d => d.Name, d => d.Content))
                    .Fuzziness(new Fuzziness("AUTO")))), ct),
            $"Could not search for '{term}'.");

        if (!response.IsValidResponse)
        {
            throw new SearchIndexException($"Could not search for '{term}': {response.DebugInformation}");
        }

        return response.Documents.Select(d => d.DocumentId).ToList();
    }

    public async Task DeleteAsync(Guid documentId, CancellationToken ct = default)
    {
        await EnsureIndexAsync(ct);

        var response = await ExecuteAsync(
            () => _client.DeleteAsync(_indexName, documentId.ToString(), ct),
            $"Could not delete document {documentId} from the index.");

        if (!response.IsValidResponse && response.Result != Result.NotFound)
        {
            throw new SearchIndexException($"Could not delete document {documentId} from the index: {response.DebugInformation}");
        }

        _logger.LogInformation("Removed document {DocumentId} from {Index}", documentId, _indexName);
    }

    private async Task EnsureIndexAsync(CancellationToken ct)
    {
        if (_indexReady)
        {
            return;
        }

        await _indexLock.WaitAsync(ct);
        try
        {
            if (_indexReady)
            {
                return;
            }

            var exists = await ExecuteAsync(
                () => _client.Indices.ExistsAsync(_indexName, ct),
                $"Could not check index {_indexName}.");

            if (!exists.Exists)
            {
                var created = await ExecuteAsync(
                    () => _client.Indices.CreateAsync<SearchDocument>(_indexName, c => c
                        .Mappings(m => m.Properties(p => p
                            .Keyword(d => d.DocumentId)
                            .Text(d => d.Name)
                            .Text(d => d.Content)
                            .Date(d => d.CreatedAt))), ct),
                    $"Could not create index {_indexName}.");

                if (!created.IsValidResponse)
                {
                    throw new SearchIndexException($"Could not create index {_indexName}: {created.DebugInformation}");
                }

                _logger.LogInformation("Created index {Index}", _indexName);
            }

            _indexReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string errorMessage)
    {
        try
        {
            return await action();
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not SearchIndexException)
        {
            throw new SearchIndexException(errorMessage, ex);
        }
    }
}
