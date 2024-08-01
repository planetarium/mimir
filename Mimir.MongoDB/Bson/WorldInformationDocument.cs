// using Bencodex.Types;
// using Nekoyume.Model;
// using Nekoyume.Model.State;

// namespace Mimir.MongoDB.Bson;

// public record WorldInformationDocument : IMimirBsonDocument(Address)
// {
//     public IDictionary<int, WorldInformation.World> Object { get; init; }
//     private WorldInformation WorldInformationStateObject { get; init; }

//     public WorldInformationDocument(WorldInformation worldInformation)
//     {
//         Object = ((Dictionary)worldInformation.Serialize()).ToDictionary(
//             kv => kv.Key.ToInteger(),
//             kv => new WorldInformation.World((Dictionary)kv.Value));
//         WorldInformationStateObject = worldInformation;
//     }

//     public IValue Bencoded => WorldInformationStateObject.Serialize();
// }
