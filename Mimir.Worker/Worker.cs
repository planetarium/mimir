using Microsoft.Extensions.Options;
using Mimir.Worker.Client;
using Mimir.Worker.Constants;
using Mimir.Worker.Handler;
using Mimir.Worker.Initializer;
using Mimir.Worker.Poller;
using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IHeadlessGQLClient _headlessGqlClient;
    private readonly IStateService _stateService;
    private readonly MongoDbService _dbService;
    private readonly InitializerManager _initializerManager;
    private readonly PollerType _pollerType;
    private readonly bool _enableInitializing;

    public Worker(
        IHeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        MongoDbService dbService,
        InitializerManager initializerManager,
        IOptions<Configuration> config)
    {
        var conf = config.Value;
        _headlessGqlClient = headlessGqlClient;
        _stateService = stateService;
        _dbService = dbService;
        _initializerManager = initializerManager;
        _pollerType = conf.PollerType;
        _enableInitializing = conf.EnableInitializing;
        
        AddressHandlerMappings.RegisterCurrencyHandler(PlanetType.FromString(conf.PlanetType));

        _logger = Log.ForContext<Worker>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        _logger.Information(
            "Start Worker background service, init: {EnableInitializing}",
            _enableInitializing
        );

        await DoWork(stoppingToken);

        _logger.Information(
            "Stopped Worker background service. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
        // Temp force exits
        throw new Exception();
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        if (_enableInitializing)
        {
            _logger.Information("Initializing enabled, start initializing");

            await _initializerManager.WaitInitializers(stoppingToken);
        }

        try
        {
            _logger.Information("Start Polling");
            var poller = PollerFactory.CreatePoller(
                _pollerType,
                _stateService,
                _headlessGqlClient,
                _dbService
            );

            await poller.RunAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.Fatal(ex, "An error occurred during polling.");
            throw;
        }
    }
}
