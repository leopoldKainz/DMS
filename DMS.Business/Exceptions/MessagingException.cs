namespace DMS.Business.Exceptions;

public class MessagingException : Exception
{
    public MessagingException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
