namespace Lib9c.Models.Exceptions;

public class UnsupportedArgumentTypeException<T> : Exception
{
    public UnsupportedArgumentTypeException(
        string argumentName,
        IEnumerable<T> expected,
        T actual,
        Exception? innerException = null)
        : base(
            $"Argument '{argumentName}' is of unsupported type. Expected one of: {string.Join(", ", expected)}, Actual: {actual}.",
            innerException)
    {
    }

    public UnsupportedArgumentTypeException(
        string argumentName,
        T actual,
        Exception? innerException = null)
        : base(
            $"Argument '{argumentName}' is of unsupported type. Actual: {actual}.",
            innerException)
    {
    }
}
