using System.Numerics;
using Bencodex;
using Lib9c;
using Libplanet.Common;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Agent;
using Mimir.Models.Avatar;
using Mimir.Repositories;
using Mimir.Services;
using Mimir.Util;
using Nekoyume.Model.State;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}/avatars/{address}")]
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

    [HttpGet]
    public async Task<Avatar?> GetState(
        string network,
        string address,
        IStateService stateService)
    {
        var avatar = avatarRepository.GetAvatar(network, address);
        if (avatar is not null)
        {
            return avatar;
        }

        Address avatarAddress;
        try
        {
            avatarAddress = new Address(address);
        }
        catch (ArgumentException)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return null;
        }

        var stateGetter = new StateGetter(stateService);
        var avatarState = await stateGetter.GetAvatarStateAsync(avatarAddress);
        if (avatarState is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return null;
        }

        return new Avatar(avatarState);
    }

    [HttpGet("balances/{currency}")]
    public async Task<string> GetBalances(
        string network,
        string address,
        string currency,
        IStateService stateService)
    {
        Address agentAddress;
        try
        {
            agentAddress = new Address(address);
        }
        catch (FormatException)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return string.Empty;
        }

        Currency c;
        try
        {
            c = Currencies.GetMinterlessCurrency(currency);
        }
        catch (ArgumentException)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return string.Empty;
        }

        var stateGetter = new StateGetter(stateService);
        var balance = await stateGetter.GetBalanceAsync(agentAddress, c);
        return $"{balance} {c.Ticker}";
    }

    [HttpGet("inventory")]
    public async Task<Inventory?> GetInventory(
        string network,
        string address,
        IStateService stateService)
    {
        var inventory = avatarRepository.GetInventory(network, address);
        if (inventory is not null)
        {
            return inventory;
        }

        Address inventoryAddress;
        try
        {
            inventoryAddress = new Address(address);
        }
        catch (ArgumentException)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return null;
        }

        var stateGetter = new StateGetter(stateService);
        var inventoryState = await stateGetter.GetInventoryStateAsync(inventoryAddress);
        if (inventoryState is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return null;
        }

        return new Inventory(inventoryState);
    }
}
