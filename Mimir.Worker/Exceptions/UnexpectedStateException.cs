namespace Mimir.Worker.Exceptions;

public class UnexpectedStateException(
    Type[] expected,
    Exception? innerException = null)
    : Exception(
        $"Unexpected state. Expected: {string.Join(", ", expected.Select(e => e.FullName))}",
        innerException);
