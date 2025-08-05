using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Services;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Services;
using Mimir.Worker.Util;
using Mimir.Worker.Extensions;
using Mimir.Scripts;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Text.Json;
using Libplanet.Crypto;
using Lib9c.Models.Extensions;
using Mimir.MongoDB;

namespace Mimir.Scripts.Migrations;

public class AvatarStateRecoveryMigration
{
    private readonly IMongoDbService _mongoDbService;
    private readonly IHeadlessGQLClient _headlessGqlClient;
    private readonly IStateService _stateService;
    private readonly ILogger<AvatarStateRecoveryMigration> _logger;
    private readonly Worker.Configuration _configuration;
    private const string ProgressFileName = "avatar_state_recovery_progress.json";
    private const int LIMIT = 50;

    public AvatarStateRecoveryMigration(
        IMongoDbService mongoDbService,
        IHeadlessGQLClient headlessGqlClient,
        IStateService stateService,
        ILogger<AvatarStateRecoveryMigration> logger,
        IOptions<Worker.Configuration> configuration
    )
    {
        _mongoDbService = mongoDbService;
        _headlessGqlClient = headlessGqlClient;
        _stateService = stateService;
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task<int> ExecuteAsync()
    {
        _logger.LogInformation("Avatar State 복구 마이그레이션 시작");

        var progress = await LoadProgressAsync();
        var totalAgents = await GetTotalAgentCountAsync();

        _logger.LogInformation(
            "복구 대상: 총 {TotalAgents}개 Agent",
            totalAgents
        );

        if (totalAgents == 0)
        {
            _logger.LogInformation("복구할 Agent가 없습니다.");
            return 0;
        }

        var totalProcessedAgents = 0;
        var totalProcessedAvatars = 0;
        var processedCount = 0;

        var collection = _mongoDbService.GetCollection<AgentDocument>();
        var filter = Builders<BsonDocument>.Filter.Empty;
        var sort = Builders<BsonDocument>.Sort.Ascending("_id");
        
        using var cursor = await collection.Find(filter).Sort(sort).ToCursorAsync();

        while (await cursor.MoveNextAsync())
        {
            foreach (var bsonDoc in cursor.Current)
            {
                try
                {
                    var agentDoc = BsonSerializer.Deserialize<AgentDocument>(bsonDoc);
                    if (agentDoc == null)
                    {
                        _logger.LogWarning("AgentDocument 역직렬화 실패");
                        continue;
                    }

                    var agentAddress = new Address(agentDoc.Id);
                    var avatarAddresses = agentDoc.Object.AvatarAddresses;

                    if (avatarAddresses == null || avatarAddresses.Count == 0)
                    {
                        _logger.LogDebug("Agent {AgentAddress}에 아바타가 없습니다.", agentAddress.ToHex());
                        totalProcessedAgents++;
                        continue;
                    }

                    var processedAvatarsInAgent = 0;
                    foreach (var avatarAddressEntry in avatarAddresses)
                    {
                        var avatarAddress = avatarAddressEntry.Value;
                        var isAvatarExists = await _mongoDbService.IsExistAvatarAsync(avatarAddress);
                        if (isAvatarExists)
                        {
                            _logger.LogDebug("Avatar {AvatarAddress}는 이미 존재합니다. 건너뜁니다.", avatarAddress.ToHex());
                            continue;
                        }

                        await InsertAvatar(avatarAddress, agentDoc.StoredBlockIndex);
                        processedAvatarsInAgent++;
                        totalProcessedAvatars++;
                    }

                    totalProcessedAgents++;
                    processedCount++;

                    if (processedCount % LIMIT == 0)
                    {
                        progress.ProcessedAgents = totalProcessedAgents;
                        progress.ProcessedAvatars = totalProcessedAvatars;
                        await SaveProgressAsync(progress);

                        _logger.LogInformation(
                            "진행상황: {ProcessedCount}/{TotalAgents} Agent 처리됨, 총 {TotalAvatars}개 Avatar 처리됨",
                            processedCount,
                            totalAgents,
                            totalProcessedAvatars
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Agent 처리 중 오류 발생");
                }
            }
        }

        progress.ProcessedAgents = totalProcessedAgents;
        progress.ProcessedAvatars = totalProcessedAvatars;
        await SaveProgressAsync(progress);

        _logger.LogInformation(
            "Avatar State 복구 마이그레이션 완료. 총 {Agents}개 Agent, {Avatars}개 Avatar 처리됨",
            totalProcessedAgents,
            totalProcessedAvatars
        );

        return totalProcessedAvatars;
    }

    private async Task<long> GetTotalAgentCountAsync()
    {
        var collection = _mongoDbService.GetCollection<AgentDocument>();
        return await collection.CountDocumentsAsync(Builders<BsonDocument>.Filter.Empty);
    }

    private async Task InsertAvatar(Address avatarAddress, long blockIndex)
    {
        var stateGetter = _stateService.At(new OptionsWrapper<Worker.Configuration>(_configuration));
        
        try
        {
            var avatarState = await stateGetter.GetAvatarStateAsync(avatarAddress);
            var inventoryState = await stateGetter.GetInventoryState(avatarAddress, CancellationToken.None);
            var armorId = inventoryState.GetArmorId();
            var portraitId = inventoryState.GetPortraitId();

            var document = new AvatarDocument(
                blockIndex,
                avatarState.Address,
                avatarState,
                armorId,
                portraitId
            );

            await _mongoDbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AvatarDocument>(),
                [document]
            );
            _logger.LogDebug("Avatar State 삽입 완료: {AvatarAddress}", avatarAddress.ToHex());
        }
        catch (Mimir.Worker.Exceptions.StateNotFoundException)
        {
            _logger.LogDebug("Avatar State를 찾을 수 없습니다: {AvatarAddress}", avatarAddress.ToHex());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Avatar State 삽입 중 오류 발생: {AvatarAddress}", avatarAddress.ToHex());
        }
    }

    private async Task<AvatarStateRecoveryProgress> LoadProgressAsync()
    {
        if (File.Exists(ProgressFileName))
        {
            try
            {
                var json = await File.ReadAllTextAsync(ProgressFileName);
                var progress = JsonSerializer.Deserialize<AvatarStateRecoveryProgress>(json);
                _logger.LogInformation(
                    "진행상황 파일 로드 완료: ProcessedAgents={ProcessedAgents}, ProcessedAvatars={ProcessedAvatars}",
                    progress?.ProcessedAgents,
                    progress?.ProcessedAvatars
                );
                return progress ?? new AvatarStateRecoveryProgress();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "진행상황 파일 로드 실패: {FileName}", ProgressFileName);
            }
        }

        var newProgress = new AvatarStateRecoveryProgress();
        await SaveProgressAsync(newProgress);
        return newProgress;
    }

    private async Task SaveProgressAsync(AvatarStateRecoveryProgress progress)
    {
        try
        {
            var json = JsonSerializer.Serialize(progress, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ProgressFileName, json);
            _logger.LogDebug(
                "진행상황 저장 완료: ProcessedAgents={ProcessedAgents}, ProcessedAvatars={ProcessedAvatars}",
                progress.ProcessedAgents,
                progress.ProcessedAvatars
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "진행상황 저장 실패: {FileName}", ProgressFileName);
        }
    }
}

public class AvatarStateRecoveryProgress
{
    public int ProcessedAgents { get; set; } = 0;
    public int ProcessedAvatars { get; set; } = 0;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
} 