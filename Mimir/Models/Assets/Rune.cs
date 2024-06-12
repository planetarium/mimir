using MongoDB.Bson;
using Nekoyume.Model.State;

namespace Mimir.Models.Assets;

public class Rune
{
    public int RuneSheetId { get; set; }
    public int Level { get; set; }

    public Rune(RuneState runeState)
    {
        RuneSheetId = runeState.RuneId;
        Level = runeState.Level;
    }

    public Rune(BsonDocument rune)
    {
        RuneSheetId = rune["RuneId"].AsInt32;
        Level = rune["Level"].AsInt32;
    }
}
