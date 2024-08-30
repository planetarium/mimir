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
                Task.Run(async () =>
                {
                    try
                    {
                        _logger.Information("Starting Producer Task for {Address}", address);
                        await new DiffProducer(
                            _stateService,
                            _headlessGqlClient,
                            _dbService,
                            address
                        ).ProduceByAccount(_channels[address.ToHex()].Writer, cts.Token);
                        _logger.Information("Completed Producer Task for {Address}", address);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Producer Task failed for {Address}", address);
                        throw;
                    }
                })
            )
            .ToArray();

        var consumerTasks = AddressHandlerMappings
            .HandlerMappings.Keys.Select(address =>
                Task.Run(async () =>
                {
                    try
                    {
                        _logger.Information("Starting Consumer Task for {Address}", address);
                        await new DiffConsumer(_dbService, address).ConsumeAsync(
                            _channels[address.ToHex()].Reader,
                            cts.Token
                        );
                        _logger.Information("Completed Consumer Task for {Address}", address);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Consumer Task failed for {Address}", address);
                        throw;
                    }
                })
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
}
