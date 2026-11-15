namespace DMS.ELK.Exceptions;

public class SearchIndexException : Exception
{
    public SearchIndexException(string message) : base(message)
    {
    }

    public SearchIndexException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
