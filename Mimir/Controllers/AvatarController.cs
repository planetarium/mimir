using System.Numerics;
using Bencodex;
using Libplanet.Common;
using Libplanet.Crypto;
using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Avatar;
using Mimir.Repositories;
using Mimir.Services;
using Mimir.Util;
using MongoDB.Bson;
using Nekoyume.Model.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}/avatars")]
public class AvatarController(AvatarRepository avatarRepository) : ControllerBase
{
    #region temporary snippets

    // This is a snippet from Mimir/Util/BigIntegerToStringConverter.cs
    private class BigIntegerToStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BigInteger);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }

    // This is a snippet from Mimir/Util/StateJsonConverter.cs
    private class StateJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IState).IsAssignableFrom(objectType);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object? existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                return;
            }

            var jo = JObject.FromObject(value, JsonSerializer.CreateDefault(new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new List<JsonConverter> { new BigIntegerToStringConverter() },
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            }));

            var iValue = value switch
            {
                AvatarState avatarState => avatarState.SerializeList(),
                IState state => state.Serialize(),
                _ => null
            };

            if (iValue != null)
            {
                var rawValue = ByteUtil.Hex(new Codec().Encode(iValue));
                jo.Add("Raw", rawValue);
            }

            jo.WriteTo(writer);
        }
    }

    // This is a snippet from Mimir.Worker/Models/State/BaseData.cs
    private static JsonSerializerSettings JsonSerializerSettings => new()
    {
        Converters = { new StateJsonConverter() },
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    #endregion snippets

    [HttpGet("{avatarAddress}/inventory")]
    public async Task<Inventory?> GetInventory(
        string network,
        string avatarAddress,
        IStateService stateService)
    {
        var inventory = avatarRepository.GetInventory(network, avatarAddress);
        if (inventory is not null)
        {
            return inventory;
        }

        var stateGetter = new StateGetter(stateService);
        var inventoryState = await stateGetter.GetInventoryStateAsync(new Address(avatarAddress));
        if (inventoryState is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return null;
        }

        try
        {
            var jsonString = JsonConvert.SerializeObject(inventoryState, JsonSerializerSettings);
            var bsonDocument = BsonDocument.Parse(jsonString);
            inventory = new Inventory(bsonDocument);
        }
        catch
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return null;
        }

        return inventory;
    }
}
