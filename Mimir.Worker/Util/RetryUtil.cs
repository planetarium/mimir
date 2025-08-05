using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
namespace Mimir.Worker.Util;

public static class RetryUtil
{
    public static async Task<T> RequestWithRetryAsync<T>(
        Func<Task<T>> operation,
        int retryCount,
        int delayMilliseconds,
        CancellationToken cancellationToken,
        Action<Exception, int>? onRetry
    )
    {
        Exception? lastException = null;

        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                onRetry?.Invoke(ex, i + 1);
                lastException = ex;
                await Task.Delay(TimeSpan.FromMilliseconds(delayMilliseconds), cancellationToken);
            }
        }

        throw new HttpRequestException(
            $"Operation failed after {retryCount} retries.",
            lastException
        );
    }
}
