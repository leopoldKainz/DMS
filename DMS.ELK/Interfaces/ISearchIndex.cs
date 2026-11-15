namespace DMS.ELK.Interfaces;

public interface ISearchIndex
{
    Task IndexAsync(SearchDocument document, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> SearchAsync(string term, CancellationToken ct = default);
    Task DeleteAsync(Guid documentId, CancellationToken ct = default);
}
