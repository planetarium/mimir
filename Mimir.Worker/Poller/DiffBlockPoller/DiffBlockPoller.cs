using System.Collections.Concurrent;
using HeadlessGQL;
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
    private readonly ConcurrentQueue<DiffContext> _queue = new ConcurrentQueue<DiffContext>();

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
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;
        _logger.Information("Start {PollerType} background service", GetType().Name);

        var producer = new DiffProducer(_queue, _stateService, _headlessGqlClient, _dbService);
        var consumer = new DiffConsumer(_queue, _dbService);

        var producerTask = producer.ProduceAsync(stoppingToken);
        var consumerTask = consumer.ConsumeAsync(stoppingToken);

        await Task.WhenAll(producerTask, consumerTask);

        _logger.Information(
            "Stopped {PollerType} background service. Elapsed {TotalElapsedMinutes} minutes",
            GetType().Name,
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }
}
