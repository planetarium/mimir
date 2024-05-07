using Mimir.Worker.Models;

namespace Mimir.Worker.Events;

public class ArenaDataCollectedEventArgs : EventArgs
{
    public ArenaData ArenaData { get; set; }
    public AvatarData AvatarData { get; set; }

    public ArenaDataCollectedEventArgs(ArenaData arenaData, AvatarData avatarData)
    {
        ArenaData = arenaData;
        AvatarData = avatarData;
    }
}
