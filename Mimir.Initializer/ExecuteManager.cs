namespace Mimir.Initializer;

public class ExecuteManager
{
    private readonly IEnumerable<IExecutor> _executors;

    public ExecuteManager(IEnumerable<IExecutor> executors)
    {
        _executors = executors;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (var executor in _executors)
        {
            if (!executor.ShouldRun())
            {
                continue;
            }
            
            await executor.RunAsync(cancellationToken);
        }
    }
}