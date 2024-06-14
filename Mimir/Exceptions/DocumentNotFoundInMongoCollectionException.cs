namespace Mimir.Exceptions;

public class DocumentNotFoundInMongoCollectionException : Exception
{
    public DocumentNotFoundInMongoCollectionException(
        string? message = null,
        Exception? innerException = null) : base(message, innerException)
    {
    }
}
