using System.Threading.Channels;
using Mimir.Worker.Client;
using Mimir.Worker.Handler;
using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public class DiffPoller : IBlockPoller
{
    protected readonly MongoDbService _dbService;
    protected readonly IStateService _stateService;
    protected readonly ILogger _logger;
    private readonly IHeadlessGQLClient _headlessGqlClient;
    private readonly Dictionary<string, Channel<DiffContext>> _channels = new();

    public DiffPoller(
        IStateService stateService,
        IHeadlessGQLClient headlessGqlClient,
        MongoDbService dbService
    )
    {
        _headlessGqlClient = headlessGqlClient;

        _stateService = stateService;
        _dbService = dbService;
        _logger = Log.ForContext<DiffPoller>();

        foreach (var address in AddressHandlerMappings.HandlerMappings.Keys)
        {
            _channels[address.ToHex()] = Channel.CreateBounded<DiffContext>(3);
        }
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        _logger.Information("Start {PollerType} background service", GetType().Name);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        var producerTasks = AddressHandlerMappings
            .HandlerMappings.Keys.Select(address =>
                Task.Run(
                    () =>
                        RunWithRestart(
                            () =>
                                new DiffProducer(
                                    _stateService,
                                    _headlessGqlClient,
                                    _dbService
                                ).ProduceByAccount(
                                    _channels[address.ToHex()].Writer,
                                    address,
                                    cts.Token
                                ),
                            address.ToHex(),
                            cts.Token
                        )
                )
            )
            .ToArray();
        var consumerTasks = AddressHandlerMappings
            .HandlerMappings.Keys.Select(address =>
                Task.Run(
                    () =>
                        new DiffConsumer(_channels[address.ToHex()], _dbService).ConsumeAsync(
                            cts.Token
                        )
                )
            )
            .ToArray();

        var allTasks = producerTasks.Concat(consumerTasks).ToArray();

        try
        {
            var completedTask = await Task.WhenAny(allTasks);

            if (completedTask.IsFaulted)
            {
                _logger.Error(completedTask.Exception, "A task has failed. Cancelling all tasks.");
                cts.Cancel();
            }

            await Task.WhenAll(allTasks);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred during polling.");
        }

        _logger.Information(
            "Stopped {PollerType} background service. Elapsed {TotalElapsedMinutes} minutes",
            GetType().Name,
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }

    private async Task RunWithRestart(
        Func<Task> taskFunc,
        string addressHex,
        CancellationToken stoppingToken
    )
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                await taskFunc();
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Task for address {AddressHex} failed. Restarting...",
                    addressHex
                );
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                if (i == 2)
                    throw;
            }
        }
    }
}
