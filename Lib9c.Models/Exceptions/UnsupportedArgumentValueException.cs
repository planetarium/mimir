namespace Lib9c.Models.Exceptions;

public class UnsupportedArgumentValueException<T> : Exception
{
    public UnsupportedArgumentValueException(
        string argumentName,
        IEnumerable<T> expected,
        T actual,
        Exception? innerException = null)
        : base(
            $"Argument '{argumentName}' has an unsupported value. Expected one of: {string.Join(", ", expected)}. Actual: {actual}.",
            innerException)
    {
    }

    public UnsupportedArgumentValueException(
        string argumentName,
        T actual,
        Exception? innerException = null)
        : base($"Argument '{argumentName}' has an unsupported value. Actual: {actual}.", innerException)
    {
    }
}
