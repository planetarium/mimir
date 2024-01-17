namespace NineChroniclesUtilBackend.Models.Arena;

public record ArenaSimulateRequest(
    int? Seed,
    string MyAvatarAddress,
    string EnemyAvatarAddress
);
