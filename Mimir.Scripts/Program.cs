using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mimir.Scripts;
using Mimir.Scripts.Migrations;
using Mimir.Worker.Services;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(
        (hostingContext, config) =>
        {
            string configPath =
                Environment.GetEnvironmentVariable("MIMIR_CONFIG_FILE") ?? "appsettings.Local.json";

            config
                .AddJsonFile("Mimir.Scripts/appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"Mimir.Scripts/{configPath}", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables("MIMIR_");
        }
    )
    .UseSerilog(
        (hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext();
        }
    )
    .ConfigureServices(
        (context, services) =>
        {
            services.Configure<Configuration>(context.Configuration.GetSection("Configuration"));

            services.AddSingleton(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
                return new MongoDbService(
                    config.MongoDbConnectionString,
                    config.PlanetType,
                    config.MongoDbCAFile
                );
            });

            services.AddTransient<UpdateLastStageClearedIdMigration>();
            services.AddTransient<UpdateTransactionDocumentMigration>();
            services.AddTransient<UpdateActionTypeMigration>();
        }
    )
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("마이그레이션 스크립트 시작");

    string? migrationType = args.Length > 0 ? args[0] : null;
    if (string.IsNullOrEmpty(migrationType))
    {
        Console.WriteLine("실행할 마이그레이션을 선택하세요:");
        Console.WriteLine("1. LastStageClearedId");
        Console.WriteLine("2. TransactionDocument");
        Console.WriteLine("3. ActionType");
        Console.Write("번호 입력: ");
        var input = Console.ReadLine();
        migrationType = input switch
        {
            "1" => "laststage",
            "2" => "transaction",
            "3" => "actiontype",
            _ => null,
        };
    }

    if (migrationType == "laststage")
    {
        var lastStageMigration =
            host.Services.GetRequiredService<UpdateLastStageClearedIdMigration>();
        var lastStageResult = await lastStageMigration.ExecuteAsync();
        logger.LogInformation(
            "LastStageClearedId 마이그레이션 완료. 총 {Count}개 문서 수정됨",
            lastStageResult
        );
    }
    else if (migrationType == "transaction")
    {
        logger.LogInformation("TransactionDocument 마이그레이션 시작");
        var transactionMigration =
            host.Services.GetRequiredService<UpdateTransactionDocumentMigration>();
        var transactionResult = await transactionMigration.ExecuteAsync();
        logger.LogInformation(
            "TransactionDocument 마이그레이션 완료. 총 {Count}개 문서 수정됨",
            transactionResult
        );
    }
    else if (migrationType == "actiontype")
    {
        logger.LogInformation("ActionType 마이그레이션 시작");
        var actionTypeMigration =
            host.Services.GetRequiredService<UpdateActionTypeMigration>();
        var actionTypeResult = await actionTypeMigration.ExecuteAsync();
        logger.LogInformation(
            "ActionType 마이그레이션 완료. 총 {Count}개 문서 추가됨",
            actionTypeResult
        );
    }
    else
    {
        logger.LogWarning("알 수 없는 마이그레이션 타입입니다.");
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "마이그레이션 실행 중 오류 발생");
}
