using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.Scripts;
using Mimir.Scripts.Migrations;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
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
                    config.PlanetType.ToString().ToLowerInvariant(),
                    config.MongoDbCAFile
                );
            });

            services.AddTransient<UpdateLastStageClearedIdMigration>();
            services.AddTransient<UpdateTransactionDocumentMigration>();
            services.AddTransient<UpdateActionTypeMigration>();
            services.AddTransient<BlockRecoveryMigration>();
            services.AddTransient<AgentStateRecoveryMigration>();
            services.AddTransient<AvatarStateRecoveryMigration>();

            services.AddSingleton<IHeadlessGQLClient, HeadlessGQLClient>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
                return new HeadlessGQLClient(
                    config.HeadlessEndpoints,
                    config.JwtIssuer,
                    config.JwtSecretKey
                );
            });
            services.AddSingleton<IStateService, HeadlessStateService>();
        }
    )
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("마이그레이션 스크립트 시작");

    string? migrationType = args.Length > 0 ? args[0] : null;
    long? startBlockIndex = null;
    long? endBlockIndex = null;

    if (args.Length > 1 && long.TryParse(args[1], out var blockIndex))
    {
        startBlockIndex = blockIndex;
    }

    if (args.Length > 2 && long.TryParse(args[2], out var endIndex))
    {
        endBlockIndex = endIndex;
    }

    if (string.IsNullOrEmpty(migrationType))
    {
        Console.WriteLine("실행할 마이그레이션을 선택하세요:");
        Console.WriteLine("1. LastStageClearedId");
        Console.WriteLine("2. TransactionDocument");
        Console.WriteLine("3. ActionType");
        Console.WriteLine("4. BlockRecovery");
        Console.WriteLine("5. AgentStateRecovery");
        Console.WriteLine("6. AvatarStateRecovery");
        Console.Write("번호 입력: ");
        var input = Console.ReadLine();
        migrationType = input switch
        {
            "1" => "laststage",
            "2" => "transaction",
            "3" => "actiontype",
            "4" => "blockrecovery",
            "5" => "agentstaterecovery",
            "6" => "avatarstaterecovery",
            _ => null,
        };

        if (migrationType == "blockrecovery" && startBlockIndex == null)
        {
            Console.Write("시작 블록 인덱스를 입력하세요: ");
            var blockIndexInput = Console.ReadLine();
            if (long.TryParse(blockIndexInput, out var parsedBlockIndex))
            {
                startBlockIndex = parsedBlockIndex;
            }

            Console.Write("끝 블록 인덱스를 입력하세요 (엔터시 최신 블록까지): ");
            var endBlockIndexInput = Console.ReadLine();
            if (
                !string.IsNullOrEmpty(endBlockIndexInput)
                && long.TryParse(endBlockIndexInput, out var parsedEndBlockIndex)
            )
            {
                endBlockIndex = parsedEndBlockIndex;
            }
        }
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
        var actionTypeMigration = host.Services.GetRequiredService<UpdateActionTypeMigration>();
        var actionTypeResult = await actionTypeMigration.ExecuteAsync();
        logger.LogInformation(
            "ActionType 마이그레이션 완료. 총 {Count}개 문서 추가됨",
            actionTypeResult
        );
    }
    else if (migrationType == "blockrecovery")
    {
        if (startBlockIndex == null)
        {
            logger.LogError("블록 복구를 위해서는 시작 블록 인덱스가 필요합니다.");
            return;
        }

        logger.LogInformation(
            "블록 복구 마이그레이션 시작. 시작 블록 인덱스: {StartBlockIndex}, 끝 블록 인덱스: {EndBlockIndex}",
            startBlockIndex.Value,
            endBlockIndex
        );
        var blockRecoveryMigration = host.Services.GetRequiredService<BlockRecoveryMigration>();
        var blockRecoveryResult = await blockRecoveryMigration.ExecuteAsync(
            startBlockIndex.Value,
            endBlockIndex
        );
        logger.LogInformation(
            "블록 복구 마이그레이션 완료. 총 {Count}개 블록 처리됨",
            blockRecoveryResult
        );
    }
    else if (migrationType == "agentstaterecovery")
    {
        logger.LogInformation("Agent State 복구 마이그레이션 시작");
        var agentStateRecoveryMigration =
            host.Services.GetRequiredService<AgentStateRecoveryMigration>();
        var agentStateRecoveryResult = await agentStateRecoveryMigration.ExecuteAsync();
        logger.LogInformation(
            "Agent State 복구 마이그레이션 완료. 총 {Count}개 Agent 처리됨",
            agentStateRecoveryResult
        );
    }
    else if (migrationType == "avatarstaterecovery")
    {
        logger.LogInformation("Avatar State 복구 마이그레이션 시작");
        var avatarStateRecoveryMigration =
            host.Services.GetRequiredService<AvatarStateRecoveryMigration>();
        var avatarStateRecoveryResult = await avatarStateRecoveryMigration.ExecuteAsync();
        logger.LogInformation(
            "Avatar State 복구 마이그레이션 완료. 총 {Count}개 Avatar 처리됨",
            avatarStateRecoveryResult
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
