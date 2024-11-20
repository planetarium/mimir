namespace Lib9c.Models.Exceptions;

public class UnsupportedModelVersionException : Exception
{
    public UnsupportedModelVersionException(
        int expected,
        int actual,
        Exception? innerException = null)
        : base(
            $"Expected model version: {expected}. Actual: {actual}.",
            innerException)
    {
    }
}
