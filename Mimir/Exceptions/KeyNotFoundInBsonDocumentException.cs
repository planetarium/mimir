namespace Mimir.Exceptions;

public class KeyNotFoundInBsonDocumentException : Exception
{
    public KeyNotFoundInBsonDocumentException(
        string? message = null,
        Exception? innerException = null) : base(message, innerException)
    {
    }
}
