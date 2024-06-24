using MongoDB.Bson;

namespace Mimir.Exceptions;

public class UnexpectedTypeOfBsonValueException : Exception, IDefaultMessageException
{
    private const string DefaultMessage = "Unexpected type of BsonValue";

    public string GetDefaultMessage() => DefaultMessage;

    public UnexpectedTypeOfBsonValueException(
        string? message = null,
        Exception? innerException = null) : base(message, innerException)
    {
    }

    public UnexpectedTypeOfBsonValueException(
        BsonType[] expected,
        BsonType actual,
        Exception? innerException = null) : base(
        $"Unexpected type of BsonValue. Expected: {string.Join(", ", expected)}, Actual: {actual}",
        innerException)
    {
    }
}
