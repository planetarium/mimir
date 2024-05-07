using Microsoft.AspNetCore.Mvc;
using NineChroniclesUtilBackend.Models.Avatar;
using NineChroniclesUtilBackend.Repositories;

namespace NineChroniclesUtilBackend.Controllers;

[ApiController]
[Route("avatars")]
public class AvatarController(AvatarRepository avatarRepository) : ControllerBase
{
    [HttpGet("{avatarAddress}/inventory")]
    public Inventory? GetInventory(string avatarAddress)
    {
        var inventory = avatarRepository.GetInventory(avatarAddress);
        if (inventory is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return null;
        }

        return inventory;
    }
}
