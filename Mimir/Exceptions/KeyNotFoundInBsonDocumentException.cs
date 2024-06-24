namespace Mimir.Exceptions;

public class KeyNotFoundInBsonDocumentException : Exception, IDefaultMessageException
{
    private const string DefaultMessage = "Key not found in BsonDocument";

    public string GetDefaultMessage() => DefaultMessage;

    public KeyNotFoundInBsonDocumentException(
        string? message = null,
        Exception? innerException = null) : base(message, innerException)
    {
    }
}
