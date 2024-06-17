using MongoDB.Bson;

namespace Mimir.Exceptions;

public class UnexpectedTypeOfBsonValueException : Exception
{
    public UnexpectedTypeOfBsonValueException(
        string? message = null,
        Exception? innerException = null) : base(message, innerException)
    {
    }

    public UnexpectedTypeOfBsonValueException(
        BsonType[] expected,
        BsonType actual) : base(
        $"Unexpected type of BsonValue. Expected: {string.Join(", ", expected)}, Actual: {actual}")
    {
    }
}
