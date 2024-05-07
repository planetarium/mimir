using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Avatar;
using Mimir.Repositories;

namespace Mimir.Controllers;

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
