using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mimir.MongoDB.Services;
using Mimir.Options;
using Mimir.Scripts.Migrations;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<DatabaseOption>(
            context.Configuration.GetSection("Database"));
        
        services.AddSingleton<IMongoDbService, MongoDbService>();
        services.AddTransient<UpdateLastStageClearedIdMigration>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("마이그레이션 스크립트 시작");
    
    var migration = host.Services.GetRequiredService<UpdateLastStageClearedIdMigration>();
    var result = await migration.ExecuteAsync();
    
    logger.LogInformation("마이그레이션 완료. 총 {Count}개 문서 수정됨", result);
}
catch (Exception ex)
{
    logger.LogError(ex, "마이그레이션 실행 중 오류 발생");
    Environment.Exit(1);
}
