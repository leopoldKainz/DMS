namespace DMS.Storage.Exceptions;

public class StorageException : Exception
{
    public StorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
