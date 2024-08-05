using System.Threading.Channels;
using HeadlessGQL;
using Mimir.Worker.Handler;
using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public class DiffBlockPoller : IBlockPoller
{
    protected readonly MongoDbService _dbService;
    protected readonly IStateService _stateService;
    protected readonly string _pollerType;
    protected readonly ILogger _logger;
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly Dictionary<string, Channel<DiffContext>> _channels = new();

    public DiffBlockPoller(
        IStateService stateService,
        HeadlessGQLClient headlessGqlClient,
        MongoDbService dbService
    )
    {
        _headlessGqlClient = headlessGqlClient;

        _stateService = stateService;
        _dbService = dbService;
        _pollerType = "DiffBlockPoller";
        _logger = Log.ForContext<DiffBlockPoller>();

        foreach (var address in AddressHandlerMappings.HandlerMappings.Keys)
        {
            _channels[address.ToHex()] = Channel.CreateBounded<DiffContext>(3);
        }
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        _logger.Information("Start {PollerType} background service", GetType().Name);

        var producerTasks = AddressHandlerMappings.HandlerMappings.Keys.Select(address =>
            Task.Run(
                () =>
                    RunWithRestart(
                        () =>
                            new DiffProducer(
                                _channels[address.ToHex()],
                                _stateService,
                                _headlessGqlClient,
                                _dbService
                            ).ProduceByAccount(stoppingToken, address),
                        address.ToHex(),
                        stoppingToken
                    )
            )
        );
        var consumerTasks = AddressHandlerMappings.HandlerMappings.Keys.Select(address =>
            Task.Run(
                () =>
                    new DiffConsumer(_channels[address.ToHex()], _dbService).ConsumeAsync(
                        stoppingToken
                    )
            )
        );

        await Task.WhenAll(producerTasks.Concat(consumerTasks));

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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await taskFunc();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Task for address {AddressHex} failed. Restarting...",
                    addressHex
                );
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }
    }
}
