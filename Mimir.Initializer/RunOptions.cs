using System;
using System.Text.Json.Serialization;

namespace Mimir.Initializer;

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter<RunOptions>))]
public enum RunOptions
{
    SnapShotInitializer = 1,
    ProductMigrator = 2,
    AgentRefreshInitializer = 3,
    AvatarRefreshInitializer = 4,
    AdventureCpRefreshInitializer = 5,
}