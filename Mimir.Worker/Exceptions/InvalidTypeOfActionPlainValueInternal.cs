using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
namespace Mimir.Worker.Exceptions;

public class InvalidTypeOfActionPlainValueInternalException : Exception
{
    public InvalidTypeOfActionPlainValueInternalException(
        string? message,
        Exception? innerException = null) :
        base(message, innerException)
    {
    }

    public InvalidTypeOfActionPlainValueInternalException(
        Bencodex.Types.ValueKind[] expected,
        Bencodex.Types.ValueKind? actual,
        Exception? innerException = null) :
        base(
            $"Invalid type of action plain value internal." +
            $" Expected: {string.Join(", ", expected)}, Actual: {actual?.ToString() ?? "null"}",
            innerException)
    {
    }
}
