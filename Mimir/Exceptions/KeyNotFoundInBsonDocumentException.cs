namespace Mimir.Exceptions;

public class KeyNotFoundInBsonDocumentException : Exception
{
    public const string DefaultMessage = "Key not found in BsonDocument";

    public KeyNotFoundInBsonDocumentException(
        string? message = null,
        Exception? innerException = null) : base(message, innerException)
    {
    }
}
